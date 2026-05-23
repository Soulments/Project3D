using UnityEngine;

/// <summary>
/// 플레이어 HP 관리 컴포넌트
/// </summary>
/// <description>
/// HP 변경 시 EventBus.PlayerHPChanged() 호출로 UI에 알림.
/// 직접 HPBarUI를 참조하지 않아 느슨한 결합 유지.
/// 스탯은 CharacterStatData ScriptableObject에서 참조.
/// </description>
public class PlayerHealth : MonoBehaviour
{
    // Inspector에서 Player_Stat SO 연결
    [SerializeField] private CharacterStatData _statData;

    private float _currentHP;

    /// <summary>
    /// 초기화 - SO에서 maxHP 읽어 설정 후 UI에 알림
    /// </summary>
    void Start()
    {
        _currentHP = _statData.maxHP;
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);
    }

    /// <summary>
    /// 데미지 처리
    /// </summary>
    /// <param name="amount">받을 데미지량</param>
    public void TakeDamage(float amount)
    {
        _currentHP = Mathf.Max(0f, _currentHP - amount);
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);

        if (_currentHP <= 0f)
            Die();
    }

    /// <summary>
    /// 회복 처리
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        _currentHP = Mathf.Min(_statData.maxHP, _currentHP + amount);
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);
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