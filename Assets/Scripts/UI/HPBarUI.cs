using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    // Inspector에서 Slider 컴포넌트 연결
    [SerializeField] private Slider _hpSlider;

    void OnEnable()
    {
        // 활성화 시 EventBus 구독
        EventBus.OnPlayerHPChanged += UpdateHPBar;
    }

    void OnDisable()
    {
        // 비활성화 시 구독 해제 — 메모리 누수 방지
        EventBus.OnPlayerHPChanged -= UpdateHPBar;
    }

    /// <summary>
    /// HP 변경 시 슬라이더 갱신
    /// </summary>
    /// <param name="current">현재 HP</param>
    /// <param name="max">최대 HP</param>
    private void UpdateHPBar(float current, float max)
    {
        if (_hpSlider == null) return;
        // 0~1 비율로 변환해 슬라이더에 적용
        _hpSlider.value = current / max;
    }
}