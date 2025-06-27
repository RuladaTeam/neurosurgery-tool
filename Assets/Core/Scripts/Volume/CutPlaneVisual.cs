using Core.Scripts.UI;
using Palmmedia.ReportGenerator.Core;
using UnityEngine;

namespace Core.Scripts.Volume
{
    public class CutPlaneVisual : MonoBehaviour
    {
        [SerializeField] private VolumeObject _volumeContainer;
        [SerializeField] private GameObject _limits;
        [Header("Cut Planes")]
        [SerializeField] private GameObject _yMinPlane;
        [SerializeField] private GameObject _yMaxPlane;
        [SerializeField] private GameObject _xMinPlane;
        [SerializeField] private GameObject _xMaxPlane;
        [SerializeField] private GameObject _zMinPlane;
        [SerializeField] private GameObject _zMaxPlane;

        private bool _showCutPlaneCube;
        
        private void Start()
        {
            SettingsUI.OnShowCutPlaneCubeParameterChanged += SettingsUI_OnShowCutPlaneCubeParameterChanged;
            _limits.SetActive(_showCutPlaneCube);
        }

        private void SettingsUI_OnShowCutPlaneCubeParameterChanged(object sender, SettingsUI.SettingsButtonEventArgs e)
        {
            _showCutPlaneCube = e.IsEnabled;
            _limits.SetActive(_showCutPlaneCube);
        }
        
        private void Update()
        {
            if (!_showCutPlaneCube) return;
            _yMinPlane.transform.localPosition = new Vector3(0, 0.5f - _volumeContainer.MinY, 0);
            _yMaxPlane.transform.localPosition = new Vector3(0, 0.5f - _volumeContainer.MaxY, 0);

            _xMinPlane.transform.localPosition = new Vector3(0.5f - _volumeContainer.MinX, 0, 0);
            _xMaxPlane.transform.localPosition = new Vector3(0.5f - _volumeContainer.MaxX, 0, 0);

            _zMinPlane.transform.localPosition = new Vector3(0, 0, 0.5f - _volumeContainer.MaxZ);
            _zMaxPlane.transform.localPosition = new Vector3(0, 0, 0.5f - _volumeContainer.MinZ);
        }

        private void OnDisable()
        {
            SettingsUI.OnShowCutPlaneCubeParameterChanged -= SettingsUI_OnShowCutPlaneCubeParameterChanged;
        }
    }
}
