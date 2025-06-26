using System;
using System.IO;
using UnityEngine;

namespace Core.Scripts.Volume
{
    public class VolumeLoader : MonoBehaviour
    {
        public string FilePath = "Assets/volume_data.raw";
        public int Width = 128;
        public int Height = 64;
        public int Depth = 128;

        [Range(0.001f, 1f)]
        public float Intensity = 1.0f;

        [Range(1f, 10000f)]
        public float Iteration = 1f;

        [Range(-1000, 8920f)]
        public float DataMin = 0f;

        [Range(-1000, 8920f)]
        public float DataMax = 8920f;
        
        [Range(-1000, 8920f)]
        public float HuValue = 8920f;
        
        [Range(-1, 1f)]
        public float YOffset = .58f;

        [Header("Volume Clipping Ranges (Shader)")]
        [Range(0, 1)] public float MinX = 0.0f;
        [Range(0, 1)] public float MaxX = 1.0f;
        [Range(0, 1)] public float MinY = 0.0f;
        [Range(0, 1)] public float MaxY = 1.0f;
        [Range(0, 1)] public float MinZ = 0.0f;
        [Range(0, 1)] public float MaxZ = 1.0f;
        
        [SerializeField] private Texture2D _transferTexture;
        
        private Texture3D _volumeTexture;
        private Material _material;
        private Renderer _renderer;
        
        private void Update() { 
            if (_material != null)
            {
                _material.SetFloat("_MinX", MinX);
                _material.SetFloat("_MaxX", MaxX);
                _material.SetFloat("_MinY", MinY);
                _material.SetFloat("_MaxY", MaxY);
                _material.SetFloat("_MinZ", MinZ);
                _material.SetFloat("_MaxZ", MaxZ);

                _material.SetFloat("_DataMin", DataMin);
                _material.SetFloat("_DataMax", DataMax);
                _material.SetFloat("_Intensity", Intensity);
                _material.SetFloat("_Iteration", Iteration);
                _material.SetFloat("_yOffset", YOffset);

                UpdateTransferFunction();
            }
        }

        private void UpdateTransferFunction()
        {
            for (int x = 0; x < _transferTexture.width; x++)
            {
                float huValue = HuValue;

                Color color;

                // Air
                if (huValue < -500)
                {
                    color = new Color(0, 0, 0, 0);
                }
                // Soft Tissue
                else if (huValue >= 400 && huValue <= 1500)
                {
                    float alpha = Mathf.InverseLerp(400, 1500, huValue);
                    color = new Color(0.8f, 0.5f, 0.3f, alpha * 0.3f);
                }
                // Bone
                else if (huValue > 1500)
                {
                    float alpha = Mathf.InverseLerp(1500, DataMax, huValue);
                    color = new Color(1, 1, 1, Mathf.Min(alpha * 2, 1));
                }
                // Default tissue
                else
                {
                    float gray = Mathf.InverseLerp(DataMin, DataMax, huValue);
                    color = new Color(gray, gray, gray, gray * 0.2f);
                }

                // Inside the loop in UpdateTransferFunction()
                float dist = Mathf.Abs(huValue - HuValue);
                float weight = 1.0f - Mathf.InverseLerp(0, 200, dist); // blend over 200 HU range

                color.a = weight * 0.8f; // boost opacity near target

                _transferTexture.SetPixel(x, 0, color);
            }

            _transferTexture.Apply();

            // Optional: update material immediately
            if (_renderer && _renderer.material)
            {
                _renderer.material.SetTexture("_Transfer", _transferTexture);
            }

        }
        
        private void Start()
        {
            _transferTexture = new Texture2D(Width, 1, TextureFormat.RGBAFloat, false);

            UpdateTransferFunction();

            _renderer = GetComponent<Renderer>();
            if (_renderer && _renderer.material)
            {
                _renderer.material.SetTexture("_Transfer", _transferTexture);
            }


            transform.localScale = new Vector3(1, .5f, 1f);
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
            _volumeTexture = new Texture3D(Width, Height, Depth, TextureFormat.RFloat, false);
            Color[] pixels = new Color[Width * Height * Depth];

            for (int i = 0; i < pixels.Length; i++)
            {
                float val = floatData[i];
                // Normalize later in shader � optional to do here
                pixels[i] = new Color(val, val, val, 1);
            }

            _volumeTexture.filterMode = FilterMode.Bilinear;
            _volumeTexture.SetPixels(pixels);
            _volumeTexture.Apply();

            if (_renderer != null && _renderer.material != null)
            {
                _renderer.material.SetTexture("_Volume", _volumeTexture);
                _renderer.material.SetFloat("_DataMin", 0);
                _renderer.material.SetFloat("_DataMax", 8920);
                _renderer.material.SetFloat("_Intensity", 1.0f);
                _material = _renderer.material;
            }
            else
            {
                Debug.LogError("Renderer or material missing!");
            }   
        }
    }
}