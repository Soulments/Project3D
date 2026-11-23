using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 적 행동 전략의 SO 베이스 — 선딜 → 타격 → 후딜 골격(Template Method)은 여기서 고정하고
/// 타격 순간의 동작(Strike)만 구현체가 정의한다
/// 시간 값은 전부 SO로 주입 (하드코딩 금지)
/// </summary>
public abstract class EnemyBehaviorData : ScriptableObject, IEnemyBehavior
{
    // ── 범위 ──────────────────────────────────────────────

    /// <summary>공격 시작 거리</summary>
    public float attackRange = 1.5f;

    // ── 타이밍 ────────────────────────────────────────────

    /// <summary>선딜(예비 동작) 시간 (초) — 플레이어가 반응할 수 있는 윈도우</summary>
    public float windupTime = 0.8f;

    /// <summary>후딜 시간 (초)</summary>
    public float recoveryTime = 0.8f;

    // ── 연출 / 수치 ───────────────────────────────────────

    /// <summary>Animator 트리거 이름 — 선딜 시작 시점에 발동</summary>
    public string animationTrigger = "Attack";

    /// <summary>CharacterStatData.attackPower에 곱하는 배율</summary>
    public float damageMultiplier = 1f;

    public float AttackRange => attackRange;

    /// <summary>바닥 범위 표시 반경 — 0이면 표시하지 않음 (범위 공격 구현체가 지정)</summary>
    protected virtual float TelegraphRadius => 0f;

    public async UniTask ExecuteAsync(EnemyAttackContext context, CancellationToken token)
    {
        // 선딜 — 예비 동작 + 경고 표시
        context.SetPhase(EnemyAttackPhase.Windup);
        context.PlayAnimation(animationTrigger);
        if (context.Telegraph != null)
            context.Telegraph.Begin(windupTime, TelegraphRadius);
        await UniTask.Delay(ToMilliseconds(windupTime), cancellationToken: token);

        // 타격 — 이 시점의 상태로 판정하므로 선딜 중 물러나면 빗나간다
        if (context.Telegraph != null)
            context.Telegraph.End();
        context.SetPhase(EnemyAttackPhase.Strike);
        Strike(context);

        // 후딜
        context.SetPhase(EnemyAttackPhase.Recovery);
        await UniTask.Delay(ToMilliseconds(recoveryTime), cancellationToken: token);
    }

    /// <summary>타격 순간의 동작 — 데미지 판정 또는 투사체 발사</summary>
    protected abstract void Strike(EnemyAttackContext context);

    private static int ToMilliseconds(float seconds)
        => Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f));
}
