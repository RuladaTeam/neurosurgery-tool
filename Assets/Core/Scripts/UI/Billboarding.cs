using UnityEngine;

namespace Core.Scripts.UI
{
    public class Billboarding : MonoBehaviour
    {
        private Transform _mainCamera;

        private bool _isBeingHolded;

        private void Start()
        {
            _mainCamera = Camera.main.transform;
        }

        private void Update()
        {
            if (_mainCamera != null && _isBeingHolded)
            {
                transform.forward = (transform.position - _mainCamera.transform.position);
            }
        }

        public void SwitchHolded()
        {
            _isBeingHolded = !_isBeingHolded;
        }
    }
}
