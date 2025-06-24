using UnityEngine;

namespace Core.Scripts
{
    public class LoadedObject : MonoBehaviour
    {
        public bool IsHovered { get; private set; }

        private BoxCollider _hoverCollider;
        private GameObject _objectMenu;
        
        public void OnHover()
        {
            Debug.Log("OnHover");
            _hoverCollider.size *= 2;
            IsHovered = true;
            _objectMenu.SetActive(true);
        }

        public void OnUnhover()
        {
            Debug.Log("OnUnhover");
            _hoverCollider.size /= 2;
            IsHovered = false;
            _objectMenu.SetActive(false);
        }

        public void Init(BoxCollider boxCollider, GameObject objectMenu)
        {
            _hoverCollider = boxCollider;
            _objectMenu = objectMenu;
            _objectMenu.SetActive(false);
        }
    }
}
