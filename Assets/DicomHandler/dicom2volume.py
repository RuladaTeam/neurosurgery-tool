import os
from pathlib import Path
import pydicom
from pydicom.pixels import pixel_array
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.widgets import Slider
from scipy.ndimage import map_coordinates

dicom_dir = r"C:\Users\kb4mi\Desktop\RULADA\Coloring\DCM_DATASETS\BRAIN_MRI\dotaev"

#================================DICOM READING======================

#Go through each file in the folder, load each dataset
dicom_datasets = []
for root, _, filenames in os.walk(dicom_dir):
    for filename in filenames:
        file_path = Path(root, filename)
        try:
            dataset = pydicom.dcmread(file_path, force=True)
            dicom_datasets.append(dataset)
        except Exception as e:
            print(f"[ERROR] Error reading {file_path}: {e}")

if not dicom_datasets:
    raise ValueError("No valid DICOM files found")

#Go through all the datasets and extract pixel_data
slices = []
for ds in dicom_datasets:
    if 'PixelData' in ds:
        pixel_array = ds.pixel_array.astype(np.float32)
        slices.append(pixel_array)

if not slices:
    raise ValueError('No pixel data found in the dicom series')

#Stack all slices in a volume
volume = np.stack(slices, axis=0)

#Normilize to grayscale

if volume.shape[-1] == 3:
    volume = np.mean(volume, axis=-1)


#================== Slice Viewer===================
class VolumeViewer:
    def __init__(self, volume):
        self.volume = volume
        self.slice_idx = volume.shape[0] // 2  # Start at middle slice

        fig, ax = plt.subplots()
        plt.subplots_adjust(bottom=0.25)

        self.ax = ax
        self.fig = fig

        self.shown_volume = self.volume

        # Show initial slice
        self.im = self.ax.imshow(self.shown_volume[self.slice_idx], cmap='gray')

        # Slider axis
        ax_slider = plt.axes([0.25, 0.1, 0.65, 0.03])
        self.slider = Slider(ax_slider, 'Slice', 0, self.volume.shape[0]-1,
                             valinit=self.slice_idx, valstep=1)

        # Connect event
        self.slider.on_changed(self.update_slice)
        self.fig.canvas.mpl_connect('key_press_event', self.on_key)

    def update_slice(self, val):
        self.slice_idx = int(self.slider.val)
        self.im.set_data(self.shown_volume[self.slice_idx])
        self.fig.canvas.draw_idle()

    def on_key(self, event):
        if event.key == 'right':
            self.slider.set_val(min(self.slice_idx + 1, self.volume.shape[0] - 1))
        elif event.key == 'left':
            self.slider.set_val(max(self.slice_idx - 1, 0))

# Normalize and run viewer
viewer = VolumeViewer(volume.transpose((0,1,2))) # Change numbers to transpose to axial, sagittal or frontal view


plt.show(block=False)

# ======================== 3D Rendering ===================
import pyvista as pv

data_array = volume[:104]

grid = pv.ImageData(dimensions=np.array(data_array.shape))
grid.point_data["Intensity"] = data_array.flatten(order="F")

grid.spacing = (1.0, 1.0, 1.0)
grid.origin = (0.0, 0.0, 0.0)

grid.plot(volume=True, cmap="gray")

print(f"Saving file with dimensions {volume.shape}")
volume = volume.astype(np.uint8)
unique, counts = np.unique(volume, return_counts=True)
print(dict(zip(unique, counts)))

volume = np.transpose(volume, (2, 1, 0))

volume.flatten(order='F')

volume.tofile("volume.raw")

