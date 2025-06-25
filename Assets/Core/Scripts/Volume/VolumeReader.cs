
using System;
using System.IO;
using UnityEngine;

public class VolumeLoader : MonoBehaviour
{
    public string filePath = "Assets/volume_data_hu.raw";
    public int width = 512;
    public int height = 256;
    public int depth = 512;

    [Range(0, 4000)]
    public float dataMin = -1000;

    [Range(-1000, 8000)]
    public float dataMax = 4000;

    [Range(0.001f, 1f)]
    public float intensity = 1.0f;

    [Range(1f, 10000f)]
    public float stepSize = 1f;


    [Range(0.001f, 1f)]
    public float opacity = 1.0f;

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

                material.SetFloat("_DataMin", dataMin);
                material.SetFloat("_DataMax", dataMax);
                material.SetFloat("_Intensity", intensity);
                material.SetFloat("_Iteration", stepSize);
        }
    } 

    void Start()
    {

        transferTex = new Texture2D(width, 1, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);

            // Simple grayscale mapping
            float value = t;
            float alpha = t;

            transferTex.SetPixel(x, 0, new Color(value, value, value, alpha));
        }

        transferTex.Apply();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer && renderer.material)
        {
            renderer.material.SetTexture("_Transfer", transferTex);
        }


        transform.localScale = new Vector3(1, 1, .5f);
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
            renderer.material.SetFloat("_DataMin",0);
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