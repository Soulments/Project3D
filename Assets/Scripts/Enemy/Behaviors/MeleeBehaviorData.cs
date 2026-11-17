using UnityEngine;

/// <summary>
/// 근접 공격 — 타격 순간에 사거리를 다시 검사해, 선딜 중 물러난 대상은 맞지 않는다
/// </summary>
[CreateAssetMenu(fileName = "MeleeBehavior", menuName = "Data/Enemy Behavior/Melee")]
public class MeleeBehaviorData : EnemyBehaviorData
{
    /// <summary>타격 순간 판정에 더하는 사거리 여유 (m)</summary>
    public float hitTolerance = 0.5f;

    protected override void Strike(EnemyAttackContext context)
    {
        PlayerHealth target = context.Target;
        if (target == null || !target.IsAlive) return;
        if (context.DistanceToTarget() > attackRange + hitTolerance) return;

        target.TakeDamage(context.Stats.attackPower * damageMultiplier);
    }
}
