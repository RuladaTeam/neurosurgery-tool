using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button _showUIButton;
        [SerializeField] private Button _snapNeedleButton;
        [SerializeField] private Button _showCutPlaneCubeButton;
        [Header("Colors")]
        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;
        [Header("Values")] 
        [SerializeField] private bool _showUIOnHover;
        [SerializeField] private bool _snapNeedleOnCollision;
        [SerializeField] private bool _showCutPlaneCube;

        public static event EventHandler<SettingsButtonEventArgs> OnShowUIParameterChanged;
        public static event EventHandler<SettingsButtonEventArgs> OnShowCutPlaneCubeParameterChanged;
        public static event EventHandler<SettingsButtonEventArgs> OnSnapNeedleOnCollisionParameterChanged;

        public class SettingsButtonEventArgs : EventArgs
        {
            public bool IsEnabled;
        }
        
        private void Start()
        {
            Dictionary<Button, bool> enabledButtons = new Dictionary<Button, bool>
            {
                { _showUIButton, _showUIOnHover }, { _snapNeedleButton, _snapNeedleOnCollision },
                { _showCutPlaneCubeButton, _showCutPlaneCube }
            };
            
            foreach (var button in enabledButtons.Keys)
            {
                button.gameObject.GetComponent<Image>().color = enabledButtons[button] ? _enabledColor : _disabledColor;
            }
           
            _showUIButton.onClick.AddListener(ShowUIOnHover);
            _snapNeedleButton.onClick.AddListener(SnapNeedleOnCollision);
            _showCutPlaneCubeButton.onClick.AddListener(ShowCutPlaneCube);
        }

        private void ShowUIOnHover()
        {
            _showUIOnHover = !_showUIOnHover;
            OnShowUIParameterChanged?.Invoke(this, new SettingsButtonEventArgs 
            {
                IsEnabled = _showUIOnHover 
            });
            _showUIButton.gameObject.GetComponent<Image>().color = _showUIOnHover ? _enabledColor : _disabledColor;
        }

        private void SnapNeedleOnCollision()
        {
            _snapNeedleOnCollision = !_snapNeedleOnCollision;
            OnSnapNeedleOnCollisionParameterChanged?.Invoke(this, new SettingsButtonEventArgs
            {
                IsEnabled = _snapNeedleOnCollision
            });
            _snapNeedleButton.gameObject.GetComponent<Image>().color = _snapNeedleOnCollision ? _enabledColor : _disabledColor;
        }

        private void ShowCutPlaneCube()
        {
            _showCutPlaneCube = !_showCutPlaneCube;
            OnShowCutPlaneCubeParameterChanged?.Invoke(this, new SettingsButtonEventArgs()
            {
                IsEnabled = _showCutPlaneCube
            });
            _showCutPlaneCubeButton.gameObject.GetComponent<Image>().color = _showCutPlaneCube ? _enabledColor : _disabledColor;
        }
    }
}
