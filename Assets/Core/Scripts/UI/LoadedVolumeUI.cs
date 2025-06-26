using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.UI
{
    public class LoadedVolumeUI : MonoBehaviour
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
        
        
    }
}
