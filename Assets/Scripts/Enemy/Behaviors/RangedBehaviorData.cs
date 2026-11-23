using UnityEngine;

/// <summary>
/// 원거리 공격 — 타격 순간에 ProjectilePool에서 투사체를 꺼내 타겟 방향으로 발사
/// </summary>
[CreateAssetMenu(fileName = "RangedBehavior", menuName = "Data/Enemy Behavior/Ranged")]
public class RangedBehaviorData : EnemyBehaviorData
{
    /// <summary>투사체 속도 (m/s)</summary>
    public float projectileSpeed = 12f;

    /// <summary>발사 위치 — 적 중심 기준 앞쪽 거리</summary>
    public float muzzleForward = 0.6f;

    /// <summary>발사 위치 — 적 중심 기준 위쪽 높이</summary>
    public float muzzleUp = 0.4f;

    protected override void Strike(EnemyAttackContext context)
    {
        if (context.Target == null || !context.Target.IsAlive) return;
        if (!ServiceLocator.TryGet(out ProjectilePool pool)) return;

        Vector3 origin = context.Owner.position
            + context.Owner.forward * muzzleForward
            + Vector3.up * muzzleUp;
        Vector3 direction = (context.TargetChestPosition - origin).normalized;

        pool.Fire(origin, direction, projectileSpeed, context.Stats.attackPower * damageMultiplier);
    }
}
