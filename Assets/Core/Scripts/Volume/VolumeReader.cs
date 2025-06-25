
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

    [Range(0.001f, 1f)]
    public float stepSize = .01f;


    [Range(0.001f, 1f)]
    public float opacity = 1.0f;

    public Vector3 VolumeScale = new Vector4(1, 1, 0.5f, 1);


    private Texture3D volumeTexture;
    private Material material;
    void Update() { 
        if (material != null)
            {
                material.SetFloat("_DataMin", dataMin);
                material.SetFloat("_DataMax", dataMax);
                material.SetFloat("_Intensity", intensity);
                material.SetFloat("_StepSize", stepSize);
        }
    } 

    void Start()
    {
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

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetTexture("_VolumeTex", volumeTexture);
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