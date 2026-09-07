using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    // Inspector에서 Player_Stat SO 연결
    [SerializeField] private CharacterStatData _statData;

    private float _currentHP;

    /// <summary>현재 생존 여부 — IDamageable 구현.</summary>
    public bool IsAlive => _currentHP > 0f;

    void Start()
    {
        _currentHP = _statData.maxHP;
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);
    }

    /// <summary>데미지 처리 — HP가 0 이하가 되면 Die() 호출.</summary>
    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        _currentHP = Mathf.Max(0f, _currentHP - amount);
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);

        if (_currentHP <= 0f)
            Die();
    }

    /// <summary>회복 처리.</summary>
    public void Heal(float amount)
    {
        _currentHP = Mathf.Min(_statData.maxHP, _currentHP + amount);
        EventBus.PlayerHPChanged(_currentHP, _statData.maxHP);
    }

    /// <summary>사망 처리 — EventBus로 사망 이벤트 발행.</summary>
    private void Die()
    {
        EventBus.PlayerDied();
        Debug.Log("Player died");
    }
}