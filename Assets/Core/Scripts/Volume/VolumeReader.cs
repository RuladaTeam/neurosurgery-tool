
using System;
using System.IO;
using UnityEngine;

public class VolumeLoader : MonoBehaviour
{
    public string filePath = "Assets/volume_data_hu.raw";
    public int width = 512;
    public int height = 256;
    public int depth = 512;

    [Range(0.001f, 1f)]
    public float intensity = 1.0f;

    [Range(1f, 10000f)]
    public float iteration = 1f;

    [Range(-1000, 8920f)]
    public float data_min =0;

    [Range(-1000, 8920f)]
    public float data_max = 8920f;


    [Range(-1000, 8920f)]
    public float _huValue = 8920f;



    public Vector3 VolumeScale = new Vector4(1, 1, 0.5f, 1);

    [Header("Volume Clipping Ranges (Shader)")]
    [Range(0, 1)] public float minX = 0.0f;
    [Range(0, 1)] public float maxX = 1.0f;
    [Range(0, 1)] public float minY = 0.0f;
    [Range(0, 1)] public float maxY = 1.0f;
    [Range(0, 1)] public float minZ = 0.0f;
    [Range(0, 1)] public float maxZ = 1.0f;


    private Texture3D volumeTexture;
    private Material material;


    public Texture2D transferTex;

    void Update() { 
        if (material != null)
            {

                material.SetFloat("_MinX", minX);
                material.SetFloat("_MaxX", maxX);
                material.SetFloat("_MinY", minY);
                material.SetFloat("_MaxY", maxY);
                material.SetFloat("_MinZ", minZ);
                material.SetFloat("_MaxZ", maxZ);

                material.SetFloat("_DataMin", data_min);
                material.SetFloat("_DataMax", data_max);
                material.SetFloat("_Intensity", intensity);
                material.SetFloat("_Iteration", iteration);

                UpdateTransferFunction();
        }
    }

    void UpdateTransferFunction()
    {
        for (int x = 0; x < transferTex.width; x++)
        {
            float huValue = _huValue;

            Color color = Color.clear;

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
                float alpha = Mathf.InverseLerp(1500, data_max, huValue);
                color = new Color(1, 1, 1, Mathf.Min(alpha * 2, 1));
            }
            // Default tissue
            else
            {
                float gray = Mathf.InverseLerp(data_min, data_max, huValue);
                color = new Color(gray, gray, gray, gray * 0.2f);
            }

            // Inside the loop in UpdateTransferFunction()
            float dist = Mathf.Abs(huValue - _huValue);
            float weight = 1.0f - Mathf.InverseLerp(0, 200, dist); // blend over 200 HU range

            color.a = weight * 0.8f; // boost opacity near target

            transferTex.SetPixel(x, 0, color);
        }

        transferTex.Apply();

        // Optional: update material immediately
        Renderer renderer = GetComponent<Renderer>();
        if (renderer && renderer.material)
        {
            renderer.material.SetTexture("_Transfer", transferTex);
        }

        }


    void Start()
    {

        transferTex = new Texture2D(width, 1, TextureFormat.RGBAFloat, false);

        UpdateTransferFunction();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer && renderer.material)
        {
            renderer.material.SetTexture("_Transfer", transferTex);
        }


        transform.localScale = new Vector3(1, .5f, 1f);
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        int totalValues = width * height * depth;
        float[] floatData = new float[totalValues];

        for (int i = 0; i < totalValues; i++)
        {
            // Read two bytes for int16
            short intValue = BitConverter.ToInt16(bytes, i * 2);

            // Convert to float and normalize later in shader or here
            floatData[i] = intValue;
        }

        // Create Texture3D
        volumeTexture = new Texture3D(width, height, depth, TextureFormat.RFloat, false);
        Color[] pixels = new Color[width * height * depth];

        for (int i = 0; i < pixels.Length; i++)
        {
            float val = floatData[i];
            // Normalize later in shader — optional to do here
            pixels[i] = new Color(val, val, val, 1);
        }

        volumeTexture.SetPixels(pixels);
        volumeTexture.Apply();

        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetTexture("_Volume", volumeTexture);
            renderer.material.SetFloat("_DataMin", 0);
            renderer.material.SetFloat("_DataMax", 8920);
            renderer.material.SetFloat("_Intensity", 1.0f);
            material = renderer.material;
        }
        else
        {
            Debug.LogError("Renderer or material missing!");
        }   
    }
}