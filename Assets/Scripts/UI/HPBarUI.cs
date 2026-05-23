using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 HP 바 UI 컴포넌트
/// </summary>
/// <description>
/// EventBus.OnPlayerHPChanged 구독으로 HP 변경 시 자동 갱신.
/// PlayerHealth를 직접 참조하지 않아 느슨한 결합 유지.
/// OnEnable/OnDisable에서 구독/해제로 메모리 누수 방지.
/// </description>
public class HPBarUI : MonoBehaviour
{
    // Inspector에서 Slider 컴포넌트 연결
    [SerializeField] private Slider _hpSlider;

    /// <summary>
    /// 활성화 시 EventBus 구독
    /// </summary>
    void OnEnable()
    {
        EventBus.OnPlayerHPChanged += UpdateHPBar;
    }

    /// <summary>
    /// 비활성화 시 EventBus 구독 해제 — 메모리 누수 방지
    /// </summary>
    void OnDisable()
    {
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
        // 0~1 사이 비율로 변환해 슬라이더에 적용
        _hpSlider.value = current / max;
    }
}