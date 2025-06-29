using System;
using System.IO;
using UnityEngine;

public class VolumeLoader : MonoBehaviour
{
    [SerializeField] private string FilePath = "Assets/volume.raw";

    private int[] size = { 208, 320, 316 }; // XYZ

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer component missing.");
            return;
        }

        Material material = renderer.sharedMaterial;
        if (material == null)
        {
            Debug.LogError("Material not assigned.");
            return;
        }

        FileStream file = new FileStream(FilePath, FileMode.Open);
        BinaryReader reader = new BinaryReader(file);

        int totalBytes = size[0] * size[1] * size[2];
        byte[] buffer = new byte[totalBytes];

        int bytesRead = reader.Read(buffer, 0, buffer.Length);
        reader.Close();
        file.Close();

        if (bytesRead != buffer.Length)
        {
            Debug.LogError("Failed to read full volume data.");
            return;
        }

        Color[] volumeColors = new Color[buffer.Length];
        for (int i = 0; i < buffer.Length; ++i)
        {
            float intensity = buffer[i] / 255f;
            volumeColors[i] = new Color(intensity, intensity, intensity, intensity);
        }

        Texture3D texture = new Texture3D(size[0], size[1], size[2], TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        texture.SetPixels(volumeColors);
        texture.Apply();

        int x = size[0]; // 208
        int y = size[1]; // 316
        int z = size[2]; // 320

        // Normalize to max dimension to preserve aspect ratio
        float maxDim = Mathf.Max(x, y, z);

        Vector3 scale = new Vector3(
            (float)x / maxDim,
            (float)y / maxDim,
            (float)z / maxDim
        );

        material.SetVector("_VolumeScale", scale);

        material.SetTexture("_Volume", texture);
        material.SetInt("_Iterations", 2048);
        material.SetFloat("_DataMin", 0);
        material.SetFloat("_DataMax", 1);
        material.SetFloat("_SliceAxis1Min", 0);
        material.SetFloat("_SliceAxis1Max", 1);
        material.SetFloat("_SliceAxis2Min", 0);
        material.SetFloat("_SliceAxis2Max", 1);
        material.SetFloat("_SliceAxis3Min", 0);
        material.SetFloat("_SliceAxis3Max", 1);
        material.SetFloat("_Normalisation", 1);
    }
}