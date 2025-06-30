using System.Collections.Generic;
using Core.Scripts.LoadedObjects;
using UnityEngine;

namespace Core.Scripts.Volume
{
    public class VolumeObject : LoadedObject
    {
        [SerializeField] private Material _material;

        public void SetMaterialValues(Vector3 scale, Texture3D texture, int iterations, float dataMin, float dataMax, 
            float sliceAxis1Min, float sliceAxis1Max, float sliceAxis2Min, float sliceAxis2Max,float sliceAxis3Min, 
            float sliceAxis3Max, float normalisation)
        {
            _material.SetVector("_VolumeScale", scale);
            _material.SetTexture("_Volume", texture);
            _material.SetInt("_Iterations", iterations);
            _material.SetFloat("_DataMin", dataMin);
            _material.SetFloat("_DataMax", dataMax);
            _material.SetFloat("_SliceAxis1Min", sliceAxis1Min);
            _material.SetFloat("_SliceAxis1Max", sliceAxis1Max);
            _material.SetFloat("_SliceAxis2Min", sliceAxis2Min);
            _material.SetFloat("_SliceAxis2Max", sliceAxis2Max);
            _material.SetFloat("_SliceAxis3Min", sliceAxis3Min);
            _material.SetFloat("_SliceAxis3Max", sliceAxis3Max);
            _material.SetFloat("_Normalisation", normalisation);
        }

        public void SetSliceAxisValues(float sliceAxis1Min = -1, float sliceAxis1Max = -1, float sliceAxis2Min =-1,
            float sliceAxis2Max = -1, float sliceAxis3Min = -1, float sliceAxis3Max = -1)
        {
            Dictionary<string, float> slices = new()
            {
                { "_SliceAxis1Min", sliceAxis1Min },
                { "_SliceAxis1Max", sliceAxis1Max },
                { "_SliceAxis2Min", sliceAxis2Min },
                { "_SliceAxis2Max", sliceAxis2Max },
                { "_SliceAxis3Min", sliceAxis3Min },
                { "_SliceAxis3Max", sliceAxis3Max },
            };
            foreach (var key in slices.Keys)
            {
                if (slices[key] < 0) continue;
                _material.SetFloat(key, slices[key]);
            }
        }
    }
}
