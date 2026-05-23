using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Inspector에서 Player_Stat SO 연결
    [SerializeField] private CharacterStatData _statData;

    private float _currentHP;

    void Start()
    {
        _currentHP = _statData.maxHP;
        // 초기 HP를 UI에 전달
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);
    }

    /// <summary>
    /// 데미지 처리 — HP가 0 이하가 되면 Die() 호출
    /// </summary>
    /// <param name="amount">받을 데미지량</param>
    public void TakeDamage(float amount)
    {
        _currentHP = Mathf.Max(0f, _currentHP - amount);
        // HP 변경을 EventBus로 발행 — HPBarUI가 직접 구독해서 처리
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