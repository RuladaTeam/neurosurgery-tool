using Core.Scripts.UI;
using UnityEngine;

namespace Core.Scripts.LoadedObjects
{
    public class LoadedObject : MonoBehaviour
    {
        public bool IsHovered { get; private set; }

        [SerializeField] private GameObject _objectMenu;
        [SerializeField] private bool _showUIOnHover = true;

        private void Start()
        {
            if (_objectMenu)
            {
                _objectMenu.SetActive(false);
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
    }
}
