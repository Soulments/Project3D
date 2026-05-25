using UnityEngine;

/// <summary>
/// 적 HP 관리 컴포넌트 — IDamageable 구현
/// 사망 시 EventBus.EnemyDied 발행
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private Animator _animator;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private float _currentHP;

    public bool IsAlive => _currentHP > 0f;

    private void Awake()
        => _currentHP = _statData.maxHP;

    /// <summary>
    /// 데미지를 받아 HP 감소. 0 이하 시 사망 처리
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        _currentHP = Mathf.Max(_currentHP - amount, 0f);
        Debug.Log($"{gameObject.name} 피격 — 남은 HP: {_currentHP}");

        if (_currentHP <= 0f)
        {
            Die();
            return;
        }

        _animator?.SetTrigger(HitHash);
    }

    /// <summary>
    /// 사망 처리 — 애니메이션 재생 및 EnemyDied 이벤트 발행
    /// </summary>
    private void Die()
    {
        _currentHP = 0f;
        _animator?.SetTrigger(DeadHash);
        EventBus.EnemyDied(gameObject);
        // 오브젝트 비활성화는 Phase 3 적 AI 완성 후 Pool로 반환
        gameObject.SetActive(false);
    }
}