using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Scripts.MeshCreation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Core.Scripts.UI
{
    public class ColorSchemeChanger : MonoBehaviour
    {
        [SerializeField] private Toggle _xDepthToggle;
        [SerializeField] private Toggle _yDepthToggle;
        [SerializeField] private Toggle _zDepthToggle;
        [SerializeField] private Toggle _sphereToggle;

        public static string CurrentDensityMode { get; private set; } = "sphere";
        
        private void Start()
        {
            _xDepthToggle.onValueChanged.AddListener(SetXDepthColorScheme);
            _yDepthToggle.onValueChanged.AddListener(SetYDepthColorScheme);
            _zDepthToggle.onValueChanged.AddListener(SetZDepthColorScheme);
            _sphereToggle.onValueChanged.AddListener(SetSphereDepthColorScheme);
            
            ResetAllToggles();
        }
        
        private void SetZDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            CurrentDensityMode = "z";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetYDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            CurrentDensityMode = "y";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetXDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            CurrentDensityMode = "x";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }
        
        private void SetSphereDepthColorScheme(bool isOn)
        {
            if (!isOn) return;
            CurrentDensityMode = "sphere";
            ResetAllToggles();
            StartCoroutine(ChangeColorScheme());
        }

        private static IEnumerator ChangeColorScheme()
        {
            GameObject loadedObject = GameObject.FindGameObjectWithTag(Config.LOADED_OBJECT_TAG);
            if (!loadedObject) yield break;

            var colorsRequest = UnityWebRequest.Get(Config.URL_BASE_COLORS + loadedObject.name + "&density=" + CurrentDensityMode);
            yield return colorsRequest.SendWebRequest();

            if (colorsRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(colorsRequest.result);
                yield break;
            }

            MeshFilter loadedObjectMeshFilter = loadedObject.GetComponent<MeshFilter>();
            // Load colors
            byte[] colorsData = colorsRequest.downloadHandler.data;
            var colorsReader = new BinaryReader(new MemoryStream(colorsData));
            List<Color> colors = MeshCreator.ReadColors(colorsReader);


            // Apply colors
            loadedObjectMeshFilter.mesh.colors = colors.ToArray();

            
        }

        private void ResetAllToggles()
        {
            switch (CurrentDensityMode)
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
    }
}
