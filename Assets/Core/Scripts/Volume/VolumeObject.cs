using Core.Scripts.LoadedObjects;
using UnityEngine;

namespace Core.Scripts.Volume
{
    public class VolumeObject : LoadedObject
    {
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


        [Header("Volume Frame Clipping Ranges (Shader)")]
        [SerializeField] public float clipValue = 0.1f;

        [Range(0, .9f)] public float ClipX = 0.0f;
        [Range(0, .9f)] public float ClipY = 0.0f;
        [Range(0, .9f)] public float ClipZ = 0.0f;

        [SerializeField] public Texture2D TransferTexture;


        public Material Material;
        public Renderer Renderer;
        public Texture3D Texture;

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            Material = Renderer.material;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Material != null)
            {
                if (ClipX == 0.0f)
                {
                    Material.SetFloat("_MinX", MinX);
                    Material.SetFloat("_MaxX", MaxX);
                } else
                {
                    Material.SetFloat("_MinX", ClipX);
                    Material.SetFloat("_MaxX", ClipX+clipValue);
                }
                if (ClipY == 0.0f)
                {
                    Material.SetFloat("_MinY", MinY);
                    Material.SetFloat("_MaxY", MaxY);
                }
                else
                {
                    Material.SetFloat("_MinY", ClipY);
                    Material.SetFloat("_MaxY", ClipY +clipValue);
                }
                if (ClipZ == 0.0f)
                {
                    Material.SetFloat("_MinZ", MinZ);
                    Material.SetFloat("_MaxZ", MaxZ);
                }
                else
                {
                    Material.SetFloat("_MinZ", ClipZ);
                    Material.SetFloat("_MaxZ", ClipZ + clipValue);
                }

                Material.SetFloat("_DataMin", DataMin);
                Material.SetFloat("_DataMax", DataMax);
                Material.SetFloat("_Intensity", Intensity);
                Material.SetFloat("_Iteration", Iteration);
                Material.SetFloat("_yOffset", YOffset);

                UpdateTransferFunction();
            }
        }
        public void UpdateTransferFunction()
        {
            if (TransferTexture == null)
                return;

            float minValue = DataMin;
            float maxValue = DataMax;

            for (int x = 0; x < TransferTexture.width; x++)
            {
                float t = x / (float)(TransferTexture.width - 1);
                TransferTexture.SetPixel(x, 0, new Color(0,0,0));
            }

            TransferTexture.Apply();

            if (Renderer != null && Renderer.material != null)
            {
                Renderer.material.SetTexture("_Transfer", TransferTexture);
            }
        }

    }
}
