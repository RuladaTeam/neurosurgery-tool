using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class VolumeObject : MonoBehaviour
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
            float huValue = Mathf.Lerp(minValue, maxValue, t);

            Color color;

            // Air (-1000 to -500)
            if (huValue < -500)
            {
                color = new Color(0, 0, 0, 0); // Transparent
            }
            // Soft Tissue (-500 to 400)
            else if (huValue <= 400)
            {
                float gray = Mathf.InverseLerp(-500, 400, huValue);
                gray = Mathf.Pow(gray, 1.2f); // Slightly boost mid-range contrast

                // Warm tone for soft tissue
                color = new Color(gray * 0.7f, gray * 0.6f, gray * 0.8f, gray * 0.3f);
            }
            // Muscle/Fat Highlight (400 to 1500)
            else if (huValue <= 1500)
            {
                float alpha = Mathf.InverseLerp(400, 1500, huValue);
                alpha = Mathf.Clamp01(alpha * 0.8f); // Increase max opacity

                // Orange-red for muscle/fat
                color = new Color(1.0f, 0.7f, 0.5f, alpha);
            }
            // Bone (1500 to 4000)
            else
            {
                float alpha = Mathf.InverseLerp(1500, 4000, huValue);
                alpha = Mathf.Min(alpha * 0.6f, 0.6f); // Reduce max opacity to avoid burnout

                // Desaturated white-gray for bone
                color = new Color(0.9f, 0.9f, 0.9f, alpha);
            }

            TransferTexture.SetPixel(x, 0, color);
        }

        TransferTexture.Apply();

        if (Renderer != null && Renderer.material != null)
        {
            Renderer.material.SetTexture("_Transfer", TransferTexture);
        }
    }

}
