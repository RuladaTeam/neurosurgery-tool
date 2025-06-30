using Core.Scripts.Volume;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.UI
{
    public class LoadedVolumeUI : LoadedObjectUI
    {
        [Header("Volume Sliders")] 
        [SerializeField] private Slider _intensitySlider;
        [SerializeField] private Slider _iterationSlider;
        [SerializeField] private Slider _yOffsetSlider;
        [Header("HU Limit Sliders")] 
        [SerializeField] private Slider _dataMinSlider;
        [SerializeField] private Slider _dataMaxSlider;
        [SerializeField] private Slider _huValueSlider;
        [Header("Cut Plane Sliders")] 
        [SerializeField] private Slider _minXSlider;
        [SerializeField] private Slider _maxXSlider;
        [SerializeField] private Slider _minYSlider;
        [SerializeField] private Slider _maxYSlider;
        [SerializeField] private Slider _minZSlider;
        [SerializeField] private Slider _maxZSlider;
        [Space(15)]
        [Header("Volume Value Text")]
        [SerializeField] private TextMeshProUGUI _intensityValueText;
        [SerializeField] private TextMeshProUGUI _iterationValueText;
        [SerializeField] private TextMeshProUGUI _yOffsetValueText;
        [Header("HU Limit Value Text")]
        [SerializeField] private TextMeshProUGUI _dataMinValueText;
        [SerializeField] private TextMeshProUGUI _dataMaxValueText;
        [SerializeField] private TextMeshProUGUI _huValueValueText;
        [Header("Cut Plane Value Text")]
        [SerializeField] private TextMeshProUGUI _minXValueText;
        [SerializeField] private TextMeshProUGUI _maxXValueText;
        [SerializeField] private TextMeshProUGUI _minYValueText;
        [SerializeField] private TextMeshProUGUI _maxYValueText;
        [SerializeField] private TextMeshProUGUI _minZValueText;
        [SerializeField] private TextMeshProUGUI _maxZValueText;

        private VolumeObject _volumeObject;
        
        protected override void Start()
        {
            base.Start();
            _volumeObject = GetComponentInParent<VolumeObject>();
            
            _minXSlider.onValueChanged.AddListener(OnMinXSliderValueChanged);
            _maxXSlider.onValueChanged.AddListener(OnMaxXSliderValueChanged);
            _minYSlider.onValueChanged.AddListener(OnMinYSliderValueChanged);
            _maxYSlider.onValueChanged.AddListener(OnMaxYSliderValueChanged);
            _minZSlider.onValueChanged.AddListener(OnMinZSliderValueChanged);
            _maxZSlider.onValueChanged.AddListener(OnMaxZSliderValueChanged);
        }

        private void OnMinXSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis1Min:value);
            UpdateUI(_minXValueText, ((int)(value * 100)).ToString());
        }

        private void OnMaxXSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis1Max:value);
            UpdateUI(_maxXValueText, ((int)(value * 100)).ToString());
        }

        private void OnMinYSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis2Min:value);
            UpdateUI(_minYValueText, ((int)(value * 100)).ToString());
        }

        private void OnMaxYSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis2Max:value);
            UpdateUI(_maxYValueText, ((int)(value * 100)).ToString());
        }

        private void OnMinZSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis3Min:value);
            UpdateUI(_minZValueText, ((int)(value * 100)).ToString());
        }

        private void OnMaxZSliderValueChanged(float value)
        {
            _volumeObject.SetSliceAxisValues(sliceAxis3Max:value);
            UpdateUI(_maxZValueText, ((int)(value * 100)).ToString());
        }

        private void UpdateUI(TextMeshProUGUI textMeshProUGUI, string text)
        {
            textMeshProUGUI.text = text + "%";
        }

        public void SetSliderValues(float sliceAxis1Min, float sliceAxis1Max, float sliceAxis2Min,
            float sliceAxis2Max, float sliceAxis3Min, float sliceAxis3Max)
        {
            _minXSlider.SetValueWithoutNotify(sliceAxis1Min);
            UpdateUI(_minXValueText, ((int)(sliceAxis1Min * 100)).ToString());
            _maxXSlider.SetValueWithoutNotify(sliceAxis1Max);
            UpdateUI(_maxXValueText, ((int)(sliceAxis1Max * 100)).ToString());
            _minYSlider.SetValueWithoutNotify(sliceAxis2Min);
            UpdateUI(_minYValueText, ((int)(sliceAxis2Min * 100)).ToString());
            _maxYSlider.SetValueWithoutNotify(sliceAxis2Max);
            UpdateUI(_maxYValueText, ((int)(sliceAxis2Max * 100)).ToString());
            _minZSlider.SetValueWithoutNotify(sliceAxis3Min);
            UpdateUI(_minZValueText, ((int)(sliceAxis3Min * 100)).ToString());
            _maxZSlider.SetValueWithoutNotify(sliceAxis3Max);
            UpdateUI(_maxZValueText, ((int)(sliceAxis3Max * 100)).ToString());
        }
    }
}
