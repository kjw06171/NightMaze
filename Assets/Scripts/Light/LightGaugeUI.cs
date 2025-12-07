using UnityEngine;
using UnityEngine.UI; 

public class LightGaugeUI : MonoBehaviour
{
    private Slider lightSlider;

    [Header("Source Reference")]
    // 💡 참조할 스크립트를 TorchLightToggle로 변경
    public LightControl lightSource; 

    void Start()
    {
        lightSlider = GetComponent<Slider>(); 

        if (lightSlider == null || lightSource == null)
        {
            Debug.LogError("Slider 또는 TorchLightToggle이 연결되지 않았습니다. Inspector를 확인하세요.");
            enabled = false; 
            return;
        }

        lightSlider.minValue = 0f;
        lightSlider.maxValue = 1f;
    }

    void Update()
    {
        // TorchLightToggle에서 계산된 LightRatio 값을 Slider에 반영합니다.
        if (lightSource != null)
        {
            lightSlider.value = lightSource.LightRatio;
        }
    }
}