import os
import numpy as np
import pydicom
import vtk
from vtk.util import numpy_support
from PyQt5.QtWidgets import QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, QSlider, QLabel
from PyQt5.QtCore import Qt
from vtk.qt.QVTKRenderWindowInteractor import QVTKRenderWindowInteractor




def load_dicom_series(directory, target_description="+C Sag T1 FSPGR 3D"):
    """Load DICOM slices from the specified series only."""
    print(f"Searching for series: '{target_description}'")
    slices = []

    for filename in os.listdir(directory):
        filepath = os.path.join(directory, filename)

        # Skip directories
        if os.path.isdir(filepath):
            continue

        try:
            ds = pydicom.dcmread(filepath, stop_before_pixels=False)
        except Exception:
            continue

        if not hasattr(ds, "SeriesDescription") or ds.SeriesDescription != target_description:
            continue

        slices.append(ds)

    if not slices:
        raise ValueError(f"No DICOM slices found matching series description: '{target_description}'")

    # Sort slices by Z position
    try:
        slices.sort(key=lambda x: float(x.ImagePositionPatient[2]))
    except Exception as e:
        raise ValueError("Slices missing or invalid ImagePositionPatient tag.") from e

    first_shape = slices[0].pixel_array.shape
    valid_slices = [s for s in slices if s.pixel_array.shape == first_shape]

    print(f"Loaded {len(valid_slices)} consistent slices.")
    image_data = np.stack([s.pixel_array for s in valid_slices]).astype(np.int16)

    
    if hasattr(valid_slices[0], 'RescaleIntercept') and hasattr(valid_slices[0], 'RescaleSlope'):
        intercept = valid_slices[0].RescaleIntercept
        slope = valid_slices[0].RescaleSlope
        image_data = (slope * image_data + intercept).astype(np.int16)
        
    z_positions = np.array([float(s.ImagePositionPatient[2]) for s in valid_slices])
    diff = np.diff(z_positions)

    # Find where direction changes (from increasing to decreasing or vice versa)
    direction_changes = np.where(np.diff(np.sign(diff)) != 0)[0] + 1

    if len(direction_changes) > 0:
        print(f"Direction change detected at slice index: {direction_changes[0]}")
        image_data = image_data[:direction_changes[0]]  # Keep only the first continuous segment
        print(f"Trimmed volume to {image_data.shape[0]} slices")
    else:
        print("No direction change detected – volume appears continuous.")

    print(f"Final volume shape: {image_data.shape}")
    return image_data



class VolumeViewer(QMainWindow):
    def __init__(self, image_data):
        super().__init__()
        self.image_data = image_data
        self.volume = None
        self.renderer = None

        # Initialize transfer functions and property
        self.color_tf = vtk.vtkColorTransferFunction()
        self.opacity_tf = vtk.vtkPiecewiseFunction()
        self.volume_property = vtk.vtkVolumeProperty()

        self.init_ui()
        self.create_volume()

    def init_ui(self):
        central_widget = QWidget()
        layout = QVBoxLayout()

        # VTK Render Window
        self.vtk_widget = QVTKRenderWindowInteractor()
        layout.addWidget(self.vtk_widget)

        # Sliders
        slider_layout = QHBoxLayout()

        self.lower_slider = QSlider(Qt.Horizontal)
        self.upper_slider = QSlider(Qt.Horizontal)
        self.lower_slider.setRange(-1000, 4000)
        self.upper_slider.setRange(-1000, 4000)
        self.lower_slider.setValue(-1000)
        self.upper_slider.setValue(4000)

        self.lower_label = QLabel("-1000")
        self.upper_label = QLabel("4000")

        slider_layout.addWidget(QLabel("Lower HU:"))
        slider_layout.addWidget(self.lower_label)
        slider_layout.addWidget(self.lower_slider)
        slider_layout.addWidget(QLabel("Upper HU:"))
        slider_layout.addWidget(self.upper_label)
        slider_layout.addWidget(self.upper_slider)

        layout.addLayout(slider_layout)

        # Connect sliders
        self.lower_slider.valueChanged.connect(self.update_threshold)
        self.upper_slider.valueChanged.connect(self.update_threshold)

        central_widget.setLayout(layout)
        self.setCentralWidget(central_widget)
        self.setWindowTitle("DICOM Volume Viewer with HU Control")
        self.resize(800, 600)

    def create_volume(self):
        flat_data = self.image_data.ravel()
        vtk_data = numpy_support.numpy_to_vtk(
            num_array=flat_data,
            deep=True,
            array_type=vtk.VTK_SHORT
        )

        imageData = vtk.vtkImageData()
        dimensions = self.image_data.shape[1], self.image_data.shape[0], self.image_data.shape[2]
        imageData.SetDimensions(dimensions)
        imageData.GetPointData().SetScalars(vtk_data)

        self.renderer = vtk.vtkRenderer()
        self.vtk_widget.GetRenderWindow().AddRenderer(self.renderer)
        interactor = self.vtk_widget.GetRenderWindow().GetInteractor()

        # Transfer functions
        self.color_tf = vtk.vtkColorTransferFunction()
        self.opacity_tf = vtk.vtkPiecewiseFunction()

        self._update_transfer_functions(-1000, 4000)

        # Volume property
        self.volume_property = vtk.vtkVolumeProperty()
        self.volume_property.SetColor(self.color_tf)
        self.volume_property.SetScalarOpacity(self.opacity_tf)
        self.volume_property.ShadeOn()
        self.volume_property.SetInterpolationTypeToLinear()

        # Mapper
        mapper = vtk.vtkSmartVolumeMapper()
        mapper.SetInputData(imageData)

        # Volume
        self.volume = vtk.vtkVolume()
        self.volume.SetMapper(mapper)
        self.volume.SetProperty(self.volume_property)

        # Add volume
        self.renderer.AddVolume(self.volume)
        self.renderer.SetBackground(0.1, 0.1, 0.2)
        self.renderer.ResetCamera()

        # Cut plane
        # plane_widget = vtk.vtkImagePlaneWidget()
        # plane_widget.SetInteractor(interactor)
        # plane_widget.SetInputData(imageData)
        # plane_widget.DisplayTextOn()
        # plane_widget.PlaceWidget()
        # plane_widget.On()

        interactor.Initialize()
        self.vtk_widget.GetRenderWindow().Render()

    def _update_transfer_functions(self, lower, upper):
        if self.color_tf is None or self.opacity_tf is None:
            print("Transfer functions not initialized!")
            return

        self.color_tf.RemoveAllPoints()
        self.opacity_tf.RemoveAllPoints()

        self.color_tf.AddRGBPoint(lower, 0, 0, 0)
        self.color_tf.AddRGBPoint((lower + upper) / 2, 0.5, 0.5, 0.5)
        self.color_tf.AddRGBPoint(upper, 1.0, 1.0, 1.0)

        self.opacity_tf.AddPoint(lower, 0.0)
        self.opacity_tf.AddPoint(upper, 1.0)

        self.volume_property.SetColor(self.color_tf)
        self.volume_property.SetScalarOpacity(self.opacity_tf)
        self.volume_property.Modified()

    def update_threshold(self):
        lower = self.lower_slider.value()
        upper = self.upper_slider.value()
        if lower > upper:
            self.lower_slider.setValue(upper)
            lower = upper
        elif upper < lower:
            self.upper_slider.setValue(lower)
            upper = lower

        self.lower_label.setText(str(lower))
        self.upper_label.setText(str(upper))

        self._update_transfer_functions(lower, upper)
        self.vtk_widget.GetRenderWindow().Render()



if __name__ == "__main__":
    dicom_directory = r"A/A"

    if not os.path.isdir(dicom_directory):
        raise FileNotFoundError(f"DICOM directory not found: {dicom_directory}")

    try:
        volume_data = load_dicom_series(dicom_directory, target_description="+C Cor T1 FSPGR 3D")

        app = QApplication([])
        print("Creating VolumeViewer...")
        viewer = VolumeViewer(volume_data)
        print("Showing viewer...")

        volume = viewer.volume
        mapper = volume.GetMapper()
        imageData = mapper.GetInput()  # This is vtkImageData

        dimensions = imageData.GetDimensions()
        scalar_type = imageData.GetScalarType()

        point_data = imageData.GetPointData().GetScalars()
        array = numpy_support.vtk_to_numpy(point_data)

        # Reshape according to dimensions (x, y, z) -> (z, y, x)
        array = array.reshape(dimensions[2], dimensions[1], dimensions[0])

        data_min = -1000
        data_max = 4000
        array = np.clip(array, data_min, data_max)
        array = (array - data_min) / (data_max - data_min + 1e-6)  # avoid div by zero

        array = array.astype(np.float32)
        array.tofile("volume_data.raw")
        
        raw_array = numpy_support.vtk_to_numpy(point_data)
        raw_array = raw_array.reshape(dimensions[2], dimensions[1], dimensions[0])
        raw_array.astype(np.int16).tofile("volume_data_hu.raw")

        with open("volume_metadata_hu.txt", "w") as f:
            f.write(f"Raw Min: {raw_array.min()}\n")
            f.write(f"Raw Max: {raw_array.max()}\n")
            f.write(f"Shape: {raw_array.shape}\n")
            f.write(f"Dtype: {raw_array.dtype}\n")
                

        
        viewer.show()
        print("App exec_ starting...")
        app.exec_()
    except Exception as e:
        print(f"[ERROR] {e}")

