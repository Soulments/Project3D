using System;
using UnityEngine;

/// <summary>
/// 공격 행동이 필요로 하는 의존성 묶음 — 행동 구현체가 EnemyController를 직접 알지 않도록 분리
/// </summary>
public sealed class EnemyAttackContext
{
    private readonly Animator _animator;
    private readonly Action<EnemyAttackPhase> _onPhaseChanged;

    public Transform Owner { get; }
    public PlayerHealth Target { get; }
    public CharacterStatData Stats { get; }
    public EnemyTelegraph Telegraph { get; }

    public EnemyAttackContext(
        Transform owner, PlayerHealth target, CharacterStatData stats,
        Animator animator, EnemyTelegraph telegraph, Action<EnemyAttackPhase> onPhaseChanged)
    {
        Owner = owner;
        Target = target;
        Stats = stats;
        _animator = animator;
        Telegraph = telegraph;
        _onPhaseChanged = onPhaseChanged;
    }

    /// <summary>타겟 가슴 높이 — 투사체 조준점</summary>
    public Vector3 TargetChestPosition => Target.transform.position + Vector3.up;

    /// <summary>XZ 평면 거리</summary>
    public float DistanceToTarget()
    {
        Vector3 delta = Target.transform.position - Owner.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    public void SetPhase(EnemyAttackPhase phase)
        => _onPhaseChanged?.Invoke(phase);

    public void PlayAnimation(string trigger)
    {
        if (_animator != null && !string.IsNullOrEmpty(trigger))
            _animator.SetTrigger(trigger);
    }
}
