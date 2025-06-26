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
            Material.SetFloat("_MinX", MinX);
            Material.SetFloat("_MaxX", MaxX);
            Material.SetFloat("_MinY", MinY);
            Material.SetFloat("_MaxY", MaxY);
            Material.SetFloat("_MinZ", MinZ);
            Material.SetFloat("_MaxZ", MaxZ);

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
        for (int x = 0; x < TransferTexture.width; x++)
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

            TransferTexture.SetPixel(x, 0, color);
        }

        TransferTexture.Apply();

        // Optional: update material immediately
        if (Renderer && Renderer.material)
        {
            Renderer.material.SetTexture("_Transfer", TransferTexture);
        }

    }

}
