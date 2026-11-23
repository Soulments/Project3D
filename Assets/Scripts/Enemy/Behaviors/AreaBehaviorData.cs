using UnityEngine;

/// <summary>
/// 범위 공격 — 자기 주변 반경 안의 대상을 한 번에 타격
/// 선딜 동안 바닥에 범위 원이 차오르고, 타격 순간 카메라 쉐이크/히트스탑을 발생시킨다
/// </summary>
[CreateAssetMenu(fileName = "AreaBehavior", menuName = "Data/Enemy Behavior/Area")]
public class AreaBehaviorData : EnemyBehaviorData
{
    /// <summary>범위 공격 반경 (m)</summary>
    public float areaRadius = 5f;

    /// <summary>타격 순간 EventBus.HitLanded를 발행해 쉐이크/히트스탑/이펙트를 재사용할지</summary>
    public bool impactFeedback = true;

    protected override float TelegraphRadius => areaRadius;

    protected override void Strike(EnemyAttackContext context)
    {
        if (impactFeedback)
            EventBus.HitLanded(context.Owner.position);

        PlayerHealth target = context.Target;
        if (target == null || !target.IsAlive) return;
        if (context.DistanceToTarget() > areaRadius) return;

        target.TakeDamage(context.Stats.attackPower * damageMultiplier);
    }
}
