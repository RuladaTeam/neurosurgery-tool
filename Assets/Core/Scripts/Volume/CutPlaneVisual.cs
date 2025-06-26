using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutPlaneVisual : MonoBehaviour
{
    [SerializeField] private VolumeObject _volumeContainer;

    [Header("Cut Planes")]
    [SerializeField] private GameObject _yMinPlane;
    [SerializeField] private GameObject _yMaxPlane;
    [SerializeField] private GameObject _xMinPlane;
    [SerializeField] private GameObject _xMaxPlane;
    [SerializeField] private GameObject _zMinPlane;
    [SerializeField] private GameObject _zMaxPlane;


    // Update is called once per frame
    void Update()
    {
        _yMinPlane.transform.localPosition = new Vector3(0, 0.5f - _volumeContainer.MinY, 0);
        _yMaxPlane.transform.localPosition = new Vector3(0, 0.5f - _volumeContainer.MaxY, 0);

        _xMinPlane.transform.localPosition = new Vector3(0.5f - _volumeContainer.MinX, 0, 0);
        _xMaxPlane.transform.localPosition = new Vector3(0.5f - _volumeContainer.MaxX, 0, 0);

        _zMinPlane.transform.localPosition = new Vector3(0, 0, 0.5f - _volumeContainer.MinZ);
        _zMaxPlane.transform.localPosition = new Vector3(0, 0, 0.5f - _volumeContainer.MaxZ);
    }
}
