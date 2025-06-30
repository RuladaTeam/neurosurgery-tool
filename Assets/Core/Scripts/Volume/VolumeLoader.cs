using System.IO;
using Core.Scripts.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.Scripts.Volume
{
    public class VolumeLoader : MonoBehaviour
    {
        [SerializeField] private string _filePath = "Assets/volume.raw";
        [SerializeField] private Material _material;
        [SerializeField] private GameObject _loadedVolumeObjectPrefab;
        [SerializeField] private GameObject _loadedVolumeUIPrefab;
        [Header("Initial values")]
        [SerializeField] private int _iterations = 2048;
        [SerializeField] private float _dataMin = 0;
        [SerializeField] private float _dataMax = 1;
        [SerializeField] private float _sliceAxis1Min = 0;
        [SerializeField] private float _sliceAxis1Max = 1;
        [SerializeField] private float _sliceAxis2Min = 0;
        [SerializeField] private float _sliceAxis2Max = 1;
        [SerializeField] private float _sliceAxis3Min = 0;
        [SerializeField] private float _sliceAxis3Max = 1;
        [SerializeField] private float _normalisation = 1;

        private readonly int[] _size = { 208, 320, 316 }; // XYZ

        private void Start()
        {
            LoadObject();
        }

        private void LoadObject()
        {
            int x = _size[0]; // 208
            int y = _size[1]; // 316
            int z = _size[2]; // 320

            // Normalize to max dimension to preserve aspect ratio
            float maxDim = Mathf.Max(x, y, z);

            Vector3 scale = new Vector3(
                x / maxDim,
                y / maxDim,
                z / maxDim
            );

            byte[] buffer = ReadFile();
            if (buffer == null) return;
            
            VolumeObject loadedVolumeObject = Instantiate(_loadedVolumeObjectPrefab).GetComponent<VolumeObject>();
            
            Color[] volumeColors = new Color[buffer.Length];
            for (int i = 0; i < buffer.Length; ++i)
            {
                float intensity = buffer[i] / 255f;
                volumeColors[i] = new Color(intensity, intensity, intensity, intensity);
            }
            
            Texture3D texture = new Texture3D(_size[0], _size[1], _size[2], TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            
            texture.SetPixels(volumeColors);
            texture.Apply();
            
            loadedVolumeObject.SetMaterialValues(scale, texture, _iterations, _dataMin, _dataMax, _sliceAxis1Min, 
                _sliceAxis1Max, _sliceAxis2Min, _sliceAxis2Max, _sliceAxis3Min, _sliceAxis3Max, _normalisation);
            GameObject objectMenu = Instantiate(_loadedVolumeUIPrefab, loadedVolumeObject.gameObject.transform);
            objectMenu.transform.localScale *= (1 / loadedVolumeObject.gameObject.transform.localScale.x)/5;
            objectMenu.transform.rotation = new Quaternion(0, 0, 0, 0);
            loadedVolumeObject.SetObjectMenu(objectMenu);
            objectMenu.GetComponent<LoadedVolumeUI>().SetSliderValues(_sliceAxis1Min, _sliceAxis1Max, _sliceAxis2Min, 
                _sliceAxis2Max, _sliceAxis3Min, _sliceAxis3Max);
        }
        
        private byte[] ReadFile()
        {
            FileStream file = new FileStream(_filePath, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
        
            int totalBytes = _size[0] * _size[1] * _size[2];
            byte[] buffer = new byte[totalBytes];
        
            int bytesRead = reader.Read(buffer, 0, buffer.Length);
            reader.Close();
            file.Close();
        
            if (bytesRead != buffer.Length)
            {
                Debug.LogError("Failed to read full volume data.");
                return null;
            }
            
            return buffer;
        }
    }
}