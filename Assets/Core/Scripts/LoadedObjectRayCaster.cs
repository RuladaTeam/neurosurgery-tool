using Core.Scripts.LoadedObjects;
using UnityEngine;

namespace Core.Scripts
{
    public class LoadedObjectRayCaster : MonoBehaviour
    {
        [SerializeField] private float _maxRayCastDistance;
        
        private LoadedObject _selectedObject;
        
        private void Update()
        {
            if (Physics.Raycast(transform.position, transform.forward,
                    out RaycastHit hit, 100))
            {
                if (hit.transform.gameObject.layer != LayerMask.NameToLayer(Config.LOADED_OBJECT_LAYER))
                {
                    if (!_selectedObject) return;

                    if (hit.transform.gameObject.layer != LayerMask.NameToLayer("LoadedObjectUI"))
                    {
                        _selectedObject.OnUnhover();
                        _selectedObject = null;
                    }
                }
                
                LoadedObject loadedObject = hit.transform.gameObject.GetComponent<LoadedObject>();
                if (loadedObject.IsHovered) return;
                
                _selectedObject = loadedObject;
                loadedObject.OnHover();
            }
            else if (_selectedObject != null)
            {
                _selectedObject.OnUnhover();
                _selectedObject = null;
            }
        }
    }
}
