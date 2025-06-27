using System;
using Core.Scripts.LoadedObjects;
using Core.Scripts.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.Scripts
{
    public class Needle : MonoBehaviour
    {
        [SerializeField] private Transform _needlePoint;

        private GameObject _snappedObject;
        private bool _snapToNeedleOnCollision;

        private void Start()
        {
            SettingsUI.OnSnapNeedleOnCollisionParameterChanged += SettingsUI_OnSnapNeedleOnCollisionParameterChanged;
        }

        private void SettingsUI_OnSnapNeedleOnCollisionParameterChanged(object sender, SettingsUI.SettingsButtonEventArgs e)
        {
            _snapToNeedleOnCollision = e.IsEnabled;
            if (!_snapToNeedleOnCollision && _snappedObject)
            {
                UnSnap();
            }
        }

        public void Snap(LoadedObject snappable)
        {
            if (!_snapToNeedleOnCollision) return;
            if (_snappedObject) return;
            snappable.gameObject.transform.parent = _needlePoint;
            snappable.gameObject.transform.localPosition = new Vector3(0,0,0);

            Debug.Log(snappable.SnapPosition);
            
            snappable.gameObject.transform.localPosition -= snappable.SnapPosition / 10.6f;
            snappable.gameObject.GetComponent<XRGrabInteractable>().enabled = false;
            _snappedObject = snappable.gameObject;
        }

        private void UnSnap()
        {
            _snappedObject.transform.parent = null;
            _snappedObject.GetComponent<XRGrabInteractable>().enabled = true;
        }

        private void OnDisable()
        {
            SettingsUI.OnSnapNeedleOnCollisionParameterChanged -= SettingsUI_OnSnapNeedleOnCollisionParameterChanged;
        }
    }
}
