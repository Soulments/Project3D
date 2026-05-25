using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스킬 버튼 쿨타임 오버레이 UI
/// EventBus를 구독해 fillAmount와 남은 시간 텍스트를 갱신
/// </summary>
public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private int _skillIndex;
    [SerializeField] private Image _cooldownOverlay;
    [SerializeField] private TextMeshProUGUI _cooldownText;

    private void OnEnable()
        => EventBus.OnSkillCooldownChanged += OnCooldownChanged;

    private void OnDisable()
        => EventBus.OnSkillCooldownChanged -= OnCooldownChanged;

    /// <summary>
    /// 쿨타임 상태 변경 시 오버레이와 텍스트 갱신
    /// </summary>
    private void OnCooldownChanged(int index, float duration, float elapsed)
    {
        if (index != _skillIndex) return;

        float remaining = duration - elapsed;

        if (remaining <= 0f)
        {
            _cooldownOverlay.fillAmount = 0f;
            _cooldownText.gameObject.SetActive(false);
            return;
        }

        _cooldownOverlay.fillAmount = remaining / duration;
        _cooldownText.gameObject.SetActive(true);
        _cooldownText.text = remaining.ToString("F1");
    }
}