using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 보스 FSM — EnemyController의 상태 머신을 그대로 쓰고, 공격만 페이즈별 패턴 체이닝으로 바꾼다
/// HP 임계값을 넘으면 페이즈가 전환되고(EventBus.OnBossPhaseChanged → 히트스탑/카메라 쉐이크),
/// 사망하면 EventBus.OnBossDefeated를 발행해 던전 클리어로 이어진다
/// </summary>
public class BossController : EnemyController
{
    [SerializeField] private BossData _bossData;

    private int _phaseIndex;

    /// <summary>현재 페이즈 인덱스 (0부터)</summary>
    public int PhaseIndex => _phaseIndex;

    /// <summary>페이즈 전환 시 발행 — 새 인덱스</summary>
    public event Action<int> PhaseChanged;

    private BossPhase CurrentPhase => _bossData.phases[_phaseIndex];

    // 패턴은 BossData가 관리하므로 단일 행동 주입이 필요 없다
    protected override bool HasBehavior => _bossData != null;

    protected override float MoveSpeed => Stats.moveSpeed * CurrentPhase.moveSpeedMultiplier;

    /// <summary>현재 페이즈 패턴 중 가장 긴 사거리 — 이 안에 들어오면 공격을 시작한다</summary>
    protected override float AttackRange
    {
        get
        {
            float range = 0f;
            foreach (EnemyBehaviorData behavior in CurrentPhase.sequence)
                range = Mathf.Max(range, behavior.AttackRange);
            return range;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (_bossData == null || _bossData.phases == null || _bossData.phases.Length == 0)
            Debug.LogError($"{name}: BossData에 페이즈가 없음", this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Health.Damaged += CheckPhaseTransition;
        EventBus.OnEnemyDied += OnBossDied;
    }

    protected override void OnDisable()
    {
        Health.Damaged -= CheckPhaseTransition;
        EventBus.OnEnemyDied -= OnBossDied;
        base.OnDisable();
    }

    /// <summary>
    /// 현재 페이즈의 패턴을 순서대로 잇는다 — 대상이 닿지 않는 패턴은 건너뛰어
    /// 멀리 있을 때는 범위 공격, 가까이 있을 때는 근접 패턴이 나가게 한다
    /// </summary>
    protected override async UniTask RunBehaviorAsync(EnemyAttackContext context, CancellationToken token)
    {
        BossPhase phase = CurrentPhase;

        foreach (EnemyBehaviorData behavior in phase.sequence)
        {
            if (!HasTarget) return;
            if (context.DistanceToTarget() > behavior.AttackRange + _bossData.chainRangeMargin)
                continue;

            await behavior.ExecuteAsync(context, token);
            await UniTask.Delay(
                Mathf.RoundToInt(phase.sequenceGap * 1000f), cancellationToken: token);
        }
    }

    /// <summary>HP 비율이 다음 페이즈의 임계값 이하가 되면 페이즈를 올린다 (한 번에 여러 단계도 가능)</summary>
    private void CheckPhaseTransition()
    {
        float ratio = Health.HPRatio;
        int next = _phaseIndex;
        while (next + 1 < _bossData.phases.Length && ratio <= _bossData.phases[next + 1].enterHpRatio)
            next++;

        if (next == _phaseIndex) return;

        _phaseIndex = next;
        PhaseChanged?.Invoke(_phaseIndex);
        EventBus.BossPhaseChanged(_phaseIndex);
    }

    private void OnBossDied(GameObject enemy)
    {
        if (enemy == gameObject)
            EventBus.BossDefeated();
    }
}
