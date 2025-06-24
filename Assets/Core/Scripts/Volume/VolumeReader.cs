using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumeReader : MonoBehaviour
{

    private string filePath = "Assets/volume_data.raw";
    private int width = 512;
    private int height = 512;
    private int depth = 256;

    private Texture3D volumeTexture;

    void Start()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        Debug.Log($"Loaded {bytes.Length} bytes from {filePath}");

        volumeTexture = new Texture3D(width, height, depth, TextureFormat.RFloat, false);
        Debug.Log($"Created Texture3D: {width}x{height}x{depth}, format: {volumeTexture.format}");

        Color[] pixels = new Color[width * height * depth];
        for (int i = 0; i < pixels.Length; i++)
        {
            float value = System.BitConverter.ToSingle(bytes, i * 4); // assuming float32
            pixels[i] = new Color(value, value, value, 1);
        }

        volumeTexture.SetPixels(pixels);
        volumeTexture.Apply();

        Material material = GetComponent<Renderer>().material;
        if (material == null)
        {
            Debug.LogError("Renderer.material is null!");
            return;
        }

        material.SetTexture("_VolumeTex", volumeTexture);
        Debug.Log("Assigned Texture3D to material.");
    }

}
