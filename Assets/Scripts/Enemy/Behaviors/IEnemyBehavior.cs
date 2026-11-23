using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 적 공격 행동 전략 — FSM의 Attack 상태가 이 인터페이스만 호출한다
/// 근접/원거리/보스 패턴은 구현체만 바꿔 끼우고, 호출하는 쪽은 구체 타입을 알 필요가 없다
/// </summary>
public interface IEnemyBehavior
{
    /// <summary>공격을 시작하는 거리 — FSM의 Chase → Attack 전환 기준</summary>
    float AttackRange { get; }

    /// <summary>
    /// 공격 한 번의 전체 시퀀스 (선딜 → 타격 → 후딜)
    /// 피격 경직 등으로 token이 취소되면 즉시 중단된다
    /// </summary>
    UniTask ExecuteAsync(EnemyAttackContext context, CancellationToken token);
}
