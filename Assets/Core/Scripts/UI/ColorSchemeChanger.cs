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

        public static string CurrentDensityMode { get; private set; } = "y";
        
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
            if (loadedObjectMeshFilter == null || loadedObjectMeshFilter.sharedMesh == null)
            {
                Debug.LogError("MeshFilter or sharedMesh is missing!");
                yield break;
            }

            // Load colors
            byte[] colorsData = colorsRequest.downloadHandler.data;
            var colorsReader = new BinaryReader(new MemoryStream(colorsData));
            List<Color> colors = MeshCreator.ReadColors(colorsReader);

            // Create a new mesh copy
            Mesh originalMesh = loadedObjectMeshFilter.sharedMesh;
            Mesh mesh = new Mesh();
            mesh.indexFormat = 
                IndexFormat.UInt32;

            mesh.vertices = originalMesh.vertices;
            mesh.triangles = originalMesh.triangles;
            mesh.uv = originalMesh.uv;
            mesh.normals = originalMesh.normals;
            mesh.tangents = originalMesh.tangents;
            mesh.bounds = originalMesh.bounds;

// If your mesh uses sub-meshes or multiple materials:
            mesh.subMeshCount = originalMesh.subMeshCount;
            for (int i = 0; i < originalMesh.subMeshCount; i++)
            {
                mesh.SetTriangles(originalMesh.GetTriangles(i), i);
            }

            // Validate color count
            if (colors.Count != mesh.vertexCount)
            {
                Debug.LogError($"Color count ({colors.Count}) does not match vertex count ({mesh.vertexCount})");
                yield break;
            }

            // Apply colors
            mesh.colors = colors.ToArray();

            // Finalize
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();

            // Replace mesh
            loadedObjectMeshFilter.mesh = mesh;

            Debug.Log("Colors applied successfully.");
            
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
                    _zDepthToggle.SetIsOnWithoutNotify(false);
                    _yDepthToggle.SetIsOnWithoutNotify(false);
                    _xDepthToggle.SetIsOnWithoutNotify(true);
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
