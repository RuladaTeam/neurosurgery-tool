using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutPlaneVisualController : MonoBehaviour
{
    [SerializeField] Material _cutPlaneMaterial;


    //DEBUG ONLY
    private Vector3 position = new Vector3(.05f, 1.5f, .5f);
    private void Update()
    {
        Vector4 cp = _cutPlaneMaterial.GetVector("_CutPlanePoint");
        transform.position = position + new Vector3(cp.x, cp.y,cp.z);

        Vector4 cr = _cutPlaneMaterial.GetVector("_CutPlaneNormal");
        transform.rotation = new Quaternion(cr.x, cr.y, cr.z, cr.w);
    }
}
