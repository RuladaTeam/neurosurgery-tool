using System;
using System.IO;
using UnityEngine;

namespace Core.Scripts.Volume
{
    public class VolumeLoader : MonoBehaviour
    {
        public string FilePath = "Assets/volume_data_hu .raw";
        public int Width = 128;
        public int Height = 64;
        public int Depth = 128;
        

        [SerializeField] private VolumeObject _container;
        
       
        private void Start()
        {
            _container.TransferTexture = new Texture2D(Width, 1, TextureFormat.RGBAFloat, false);

            _container.UpdateTransferFunction();

            if (_container.Renderer && _container.Renderer.material)
            {
                _container.Renderer.material.SetTexture("_Transfer", _container.TransferTexture);
            }


            _container.gameObject.transform.localScale = new Vector3(1, .5f, 1f);
            if (!File.Exists(FilePath))
            {
                Debug.LogError("File not found: " + FilePath);
                return;
            }

            byte[] bytes = File.ReadAllBytes(FilePath);
            int totalValues = Width * Height * Depth;
            float[] floatData = new float[totalValues];

            for (int i = 0; i < totalValues; i++)
            {
                // Read two bytes for int16
                short intValue = BitConverter.ToInt16(bytes, i * 2);

                // Convert to float and normalize later in shader or here
                floatData[i] = intValue;
            }

            // Create Texture3D
            _container.Texture = new Texture3D(Width, Height, Depth, TextureFormat.RFloat, false);
            Color[] pixels = new Color[Width * Height * Depth];

            for (int i = 0; i < pixels.Length; i++)
            {
                float val = floatData[i];
                // Normalize later in shader � optional to do here
                pixels[i] = new Color(val, val, val, 1);
            }

            _container.Texture.filterMode = FilterMode.Bilinear;
            _container.Texture.SetPixels(pixels);
            _container.Texture.Apply();

            if (_container.Renderer != null && _container.Renderer.material != null)
            {
                _container.Renderer.material.SetTexture("_Volume", _container.Texture);
                _container.Renderer.material.SetFloat("_DataMin", 0);
                _container.Renderer.material.SetFloat("_DataMax", 8920);
                _container.Renderer.material.SetFloat("_Intensity", 1.0f);
                _container.Material = _container.Renderer.material;
            }
            else
            {
                Debug.Log(_container.Renderer);
                Debug.LogError("Renderer or material missing!");
            }   
        }
    }
}