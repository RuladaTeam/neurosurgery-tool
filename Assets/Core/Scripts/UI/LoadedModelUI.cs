using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Scripts.MeshCreation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Core.Scripts.UI
{
    public class LoadedModelUI : LoadedObjectUI
    {
        [Header("Color Scheme Toggles")]
        [SerializeField] private Toggle _xDepthToggle;
        [SerializeField] private Toggle _yDepthToggle;
        [SerializeField] private Toggle _zDepthToggle;
        [SerializeField] private Toggle _sphereToggle;
        [Header("Cut Plane Sliders")]
        [SerializeField] private Slider _depthSlider;
        [SerializeField] private Slider _rotationSlider;
        
        private static string _currentDensityMode = "sphere";
        private GameObject _loadedObject;
        private Material _cutPlaneMaterial;
        private Mesh _loadedMesh;

        private void Awake()
        {
            _loadedMesh = GetComponentInParent<MeshFilter>().mesh;
            _cutPlaneMaterial = GetComponentInParent<MeshRenderer>().material;
            SetCutPlaneDepth(0f);
        }
        
        protected override void Start()
        {
            base.Start();
            _loadedObject = gameObject.transform.parent.gameObject;
            _xDepthToggle.onValueChanged.AddListener(SetXDepthColorScheme);
            _yDepthToggle.onValueChanged.AddListener(SetYDepthColorScheme);
            _zDepthToggle.onValueChanged.AddListener(SetZDepthColorScheme);
            _sphereToggle.onValueChanged.AddListener(SetSphereDepthColorScheme);
            
            _depthSlider.onValueChanged.AddListener(SetCutPlaneDepth);
            _rotationSlider.onValueChanged.AddListener(SetCutPlaneRotation);

            
            ResetAllToggles();
        }
        
        private void SetZDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            _currentDensityMode = "z";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetYDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            _currentDensityMode = "y";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetXDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            _currentDensityMode = "x";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetSphereDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            _currentDensityMode = "sphere";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private IEnumerator ChangeColorScheme()
        {
            var colorsRequest = UnityWebRequest.Get(Config.URL_BASE_COLORS + _loadedObject.name + "&density=" + _currentDensityMode);
            yield return colorsRequest.SendWebRequest();

            if (colorsRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(colorsRequest.result);
                yield break;
            }

            MeshFilter loadedObjectMeshFilter = _loadedObject.GetComponent<MeshFilter>();
            
            byte[] colorsData = colorsRequest.downloadHandler.data;
            var colorsReader = new BinaryReader(new MemoryStream(colorsData));
            List<Color> colors = MeshCreator.ReadColors(colorsReader);


            loadedObjectMeshFilter.mesh.colors = colors.ToArray();
        }
        
        private void ResetAllToggles()
        {
            switch (_currentDensityMode)
            {
                case "z":
                    _xDepthToggle.SetIsOnWithoutNotify(false);
                    _yDepthToggle.SetIsOnWithoutNotify(false);
                    _zDepthToggle.SetIsOnWithoutNotify(true);
                    _sphereToggle.SetIsOnWithoutNotify(false);
                    break;
                case "y":
                    _xDepthToggle.SetIsOnWithoutNotify(false);
                    _yDepthToggle.SetIsOnWithoutNotify(true);
                    _zDepthToggle.SetIsOnWithoutNotify(false);
                    _sphereToggle.SetIsOnWithoutNotify(false);
                    break;
                case "x":
                    _xDepthToggle.SetIsOnWithoutNotify(true);
                    _yDepthToggle.SetIsOnWithoutNotify(false);
                    _zDepthToggle.SetIsOnWithoutNotify(false);
                    _sphereToggle.SetIsOnWithoutNotify(false);
                    break;
                case "sphere":
                    _xDepthToggle.SetIsOnWithoutNotify(false);
                    _yDepthToggle.SetIsOnWithoutNotify(false);
                    _zDepthToggle.SetIsOnWithoutNotify(false);
                    _sphereToggle.SetIsOnWithoutNotify(true);
                    break;
            }
        }

        private void SetCutPlaneDepth(float value)
        {
            Vector4 cutPlanePoint = _cutPlaneMaterial.GetVector("_CutPlanePoint");
            _cutPlaneMaterial.SetVector("_CutPlanePoint", new Vector4(cutPlanePoint.x, cutPlanePoint.y, 
                _loadedMesh.bounds.min.z + value * (_loadedMesh.bounds.max.z - _loadedMesh.bounds.min.z), 1));
        }

        private void SetCutPlaneRotation(float value)
        {
            
        }
    }
}
