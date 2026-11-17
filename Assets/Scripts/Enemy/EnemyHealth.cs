using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 적 HP 관리 컴포넌트 — IDamageable 구현
/// 사망 시 EventBus.EnemyDied 발행
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private Animator _animator;

    // 사망 애니메이션을 보여준 뒤 비활성화 — Animator가 없으면 즉시 비활성화
    [SerializeField] private float _despawnDelay = 2.5f;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private float _currentHP;

    /// <summary>피격 후에도 생존했을 때 발행 — 적 FSM의 Hit 상태 전환용</summary>
    public event System.Action Damaged;

    public bool IsAlive => _currentHP > 0f;

    /// <summary>현재 HP 비율 (0~1) — 보스 페이즈 전환 기준</summary>
    public float HPRatio => _statData.maxHP > 0f ? _currentHP / _statData.maxHP : 0f;

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

        // Unity 오브젝트는 ?. 가 fake-null을 통과하므로 명시적으로 검사 (Animator 없는 적 대응)
        if (_animator != null) _animator.SetTrigger(HitHash);
        Damaged?.Invoke();
    }

    /// <summary>
    /// 사망 처리 — 애니메이션 재생 및 EnemyDied 이벤트 발행
    /// </summary>
    private void Die()
    {
        _currentHP = 0f;
        if (_animator != null) _animator.SetTrigger(DeadHash);
        EventBus.EnemyDied(gameObject);

        // Pool 반환은 적 스폰 구조가 정해지는 시점에 연결 (현재는 비활성화)
        if (_animator != null && _despawnDelay > 0f)
            DespawnAfterDelayAsync().Forget();
        else
            gameObject.SetActive(false);
    }

    private async UniTaskVoid DespawnAfterDelayAsync()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_despawnDelay),
            cancellationToken: this.GetCancellationTokenOnDestroy());
        gameObject.SetActive(false);
    }
}