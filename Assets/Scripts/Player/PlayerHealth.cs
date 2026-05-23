using UnityEngine;

/// <summary>
/// 플레이어 HP 관리 컴포넌트
/// </summary>
/// <description>
/// HP 변경 시 EventBus.PlayerHPChanged() 호출로 UI에 알림.
/// 직접 HPBarUI를 참조하지 않아 느슨한 결합 유지.
/// 스탯 데이터는 Step 5에서 ScriptableObject로 교체 예정.
/// </description>
public class PlayerHealth : MonoBehaviour
{
    // 임시 값 — Step 5에서 CharacterStatData SO로 교체 예정
    [SerializeField] private float _maxHP = 100f;
    private float _currentHP;

    /// <summary>
    /// 초기화 - 최대 HP로 설정 후 UI에 알림
    /// </summary>
    void Start()
    {
        _currentHP = _maxHP;
        // 초기 HP를 UI에 전달
        EventBus.PlayerHPChanged(_currentHP, _maxHP);
    }

    /// <summary>
    /// 데미지 처리
    /// </summary>
    /// <param name="amount">받을 데미지량</param>
    public void TakeDamage(float amount)
    {
        _currentHP = Mathf.Max(0f, _currentHP - amount);
        // HP 변경을 EventBus로 발행 — HPBarUI가 직접 구독해서 처리
        EventBus.PlayerHPChanged(_currentHP, _maxHP);

        if (_currentHP <= 0f)
            Die();
    }

    /// <summary>
    /// 회복 처리
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        _currentHP = Mathf.Min(_maxHP, _currentHP + amount);
        EventBus.PlayerHPChanged(_currentHP, _maxHP);
    }

    /// <summary>
    /// 사망 처리 — EventBus로 사망 이벤트 발행
    /// </summary>
    private void Die()
    {
        EventBus.PlayerDied();
        Debug.Log("Player died");
    }
}