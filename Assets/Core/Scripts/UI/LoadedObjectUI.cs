using UnityEngine;

namespace Core.Scripts.UI
{
    public abstract class LoadedObjectUI : MonoBehaviour
    {
        private Vector3 _defaultWorldPosition;
        private Quaternion _defaultWorldRotation;
        
        protected virtual void Start()
        {
            _defaultWorldPosition = transform.position;
            _defaultWorldRotation = transform.rotation;
        }
        
        private void Update()
        {
            if (transform.position != _defaultWorldPosition) transform.position = _defaultWorldPosition;
            if (transform.rotation != _defaultWorldRotation) transform.rotation = _defaultWorldRotation;
        }
    }
}
