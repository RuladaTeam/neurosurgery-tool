using Core.Scripts.UI;
using UnityEngine;

namespace Core.Scripts.LoadedObjects
{
    public class LoadedObject : MonoBehaviour
    {
        public bool IsHovered { get; private set; }

        [SerializeField] private GameObject _objectMenu;
        [field: SerializeField] public bool ShowUIOnHover { get; private set; }

        public Vector3 SnapPosition { get; private set; }
        
        private void Start()
        {
            SettingsUI.OnShowUIParameterChanged += SettingsUI_OnShowUIParameterChanged;
            if (_objectMenu)
            {
                _objectMenu.SetActive(false);
            }   
        }

        private void SettingsUI_OnShowUIParameterChanged(object sender, SettingsUI.SettingsButtonEventArgs e)
        {
            ShowUIOnHover = e.IsEnabled;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.transform.parent.TryGetComponent(out Needle needle))
            {
                SnapPosition = other.gameObject.transform.position - transform.position ;
                needle.Snap(this);
            }
        }
        
        public void OnHover()
        {
            IsHovered = true;
            _objectMenu.GetComponent<LoadedObjectUI>().OnSetActive();
        }

        public void OnUnhover()
        {
            IsHovered = false;
            _objectMenu.SetActive(false);
        }

        public void SetObjectMenu(GameObject objectMenu)
        {
            _objectMenu = objectMenu;
            _objectMenu.SetActive(false);
        }

        private void OnDisable()
        {
            SettingsUI.OnShowUIParameterChanged -= SettingsUI_OnShowUIParameterChanged;
        }
    }
}
