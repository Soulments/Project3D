using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 AI FSM 베이스 — switch 기반 상태 전환 + NavMeshAgent 순찰/추격
/// Attack 상태의 실제 공격은 IEnemyBehavior(Strategy)에 위임한다
/// 행동은 SO(_behaviorData) 또는 SetBehavior()로 주입해 코드 수정 없이 교체한다
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private EnemyAIData _aiData;
    [SerializeField] private EnemyBehaviorData _behaviorData;
    [SerializeField] private Animator _animator;
    [SerializeField] private EnemyTelegraph _telegraph;

    private const int PatrolSampleAttempts = 5;
    private const float PatrolSampleRadius = 2f;
    private const float PatrolArriveTolerance = 0.3f;
    private const float AnimatorSpeedDamp = 0.1f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly ProfilerMarker DetectionMarker = new ProfilerMarker("Enemy.Detection");

    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private Collider _collider;
    private PlayerHealth _target;
    private IEnemyBehavior _behavior;
    private EnemyAttackContext _attackContext;
    private CancellationTokenSource _attackCts;

    private EnemyState _state = EnemyState.Idle;
    private EnemyAttackPhase _attackPhase = EnemyAttackPhase.None;
    private bool _isAttacking;
    private Vector3 _homePosition;
    private float _stateTimer;
    private float _repathTimer;
    private float _lastAttackTime = float.NegativeInfinity;

    /// <summary>현재 FSM 상태 (디버그/테스트용)</summary>
    public EnemyState State => _state;

    /// <summary>현재 공격 단계 — 선딜(Windup) 구간에 패링 판정을 붙이기 위한 노출</summary>
    public EnemyAttackPhase AttackPhase => _attackPhase;

    /// <summary>공격 단계가 바뀔 때 발행</summary>
    public event Action<EnemyAttackPhase> AttackPhaseChanged;

    protected CharacterStatData Stats => _statData;
    protected EnemyAIData AIData => _aiData;
    protected EnemyHealth Health => _health;
    protected PlayerHealth Target => _target;
    protected bool HasTarget => _target != null && _target.IsAlive;

    /// <summary>공격 시작 거리 — 보스처럼 상황에 따라 달라지는 경우 오버라이드</summary>
    protected virtual float AttackRange => _behavior.AttackRange;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();
        _collider = GetComponent<Collider>();

        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_telegraph == null) _telegraph = GetComponentInChildren<EnemyTelegraph>();
        if (_behavior == null && _behaviorData != null) _behavior = _behaviorData;
    }

    void OnEnable()
    {
        EventBus.OnEnemyDied += OnEnemyDied;
        _health.Damaged += OnDamaged;
    }

    void OnDisable()
    {
        EventBus.OnEnemyDied -= OnEnemyDied;
        _health.Damaged -= OnDamaged;
        CancelAttack();
    }

    void OnDestroy()
        => CancelAttack();

    /// <summary>
    /// 코드로 행동을 교체한다 (생성자 주입 대용) — Awake 이후 언제든 호출 가능
    /// 진행 중인 공격은 중단된다
    /// </summary>
    public void SetBehavior(IEnemyBehavior behavior)
    {
        CancelAttack();
        _behavior = behavior;
    }

    /// <summary>
    /// 스폰 위치 기록 + 플레이어 조회 (FindObjectOfType 대신 ServiceLocator)
    /// </summary>
    void Start()
    {
        _homePosition = transform.position;
        ServiceLocator.TryGet(out _target);

        if (_behavior == null)
        {
            Debug.LogError($"{name}: IEnemyBehavior가 주입되지 않음 — AI 비활성화", this);
            enabled = false;
            return;
        }

        if (!_agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: NavMesh 위에 배치되지 않음 — AI 비활성화", this);
            enabled = false;
            return;
        }

        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        switch (_state)
        {
            case EnemyState.Idle:   UpdateIdle();   break;
            case EnemyState.Patrol: UpdatePatrol(); break;
            case EnemyState.Chase:  UpdateChase();  break;
            case EnemyState.Attack: UpdateAttack(); break;
            case EnemyState.Hit:    UpdateHit();    break;
            case EnemyState.Dead:   break;
        }

        UpdateAnimatorSpeed();
    }

    // ── 상태 전환 ─────────────────────────────────────────

    /// <summary>
    /// 상태 진입 처리 — 에이전트 속도/정지 여부를 상태에 맞게 설정
    /// Attack을 벗어나면 진행 중인 공격 시퀀스를 중단한다. Dead는 종료 상태
    /// </summary>
    private void ChangeState(EnemyState next)
    {
        if (_state == EnemyState.Dead) return;

        if (next != EnemyState.Attack)
            CancelAttack();

        _state = next;
        _stateTimer = 0f;

        switch (next)
        {
            case EnemyState.Idle:
            case EnemyState.Attack:
            case EnemyState.Hit:
                StopAgent();
                break;

            case EnemyState.Dead:
                StopAgent();
                // 사망 연출 동안 길을 막지 않도록 이동/충돌을 끈다
                _agent.enabled = false;
                if (_collider != null) _collider.enabled = false;
                break;

            case EnemyState.Patrol:
                _agent.stoppingDistance = 0f;
                _agent.speed = _statData.moveSpeed * _aiData.patrolSpeedMultiplier;
                if (TryGetPatrolPoint(out Vector3 point))
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(point);
                }
                else
                {
                    _state = EnemyState.Idle;
                    StopAgent();
                }
                break;

            case EnemyState.Chase:
                // 사거리 안쪽에서 멈추도록 여유를 둔다
                _agent.stoppingDistance = AttackRange * 0.8f;
                _agent.speed = _statData.moveSpeed;
                _agent.isStopped = false;
                _repathTimer = 0f;
                break;
        }
    }

    private void StopAgent()
    {
        if (!_agent.isOnNavMesh) return;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    // ── 상태별 Update ─────────────────────────────────────

    private void UpdateIdle()
    {
        if (DetectsTarget())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        _stateTimer += Time.deltaTime;
        if (_stateTimer >= _aiData.patrolWaitTime)
            ChangeState(EnemyState.Patrol);
    }

    private void UpdatePatrol()
    {
        if (DetectsTarget())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        bool arrived = !_agent.pathPending
            && _agent.remainingDistance <= _agent.stoppingDistance + PatrolArriveTolerance;
        if (arrived)
            ChangeState(EnemyState.Idle);
    }

    private void UpdateChase()
    {
        if (!HasTarget || !IsTargetWithin(_aiData.loseTargetRange))
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (IsTargetWithin(AttackRange))
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        _repathTimer -= Time.deltaTime;
        if (_repathTimer <= 0f)
        {
            _agent.SetDestination(_target.transform.position);
            _repathTimer = _aiData.repathInterval;
        }
    }

    /// <summary>
    /// 공격 상태 — 시퀀스(선딜 → 타격 → 후딜)가 끝날 때까지 상태를 유지하고,
    /// 끝난 뒤 쿨다운이 지나면 다시 시작하거나 사거리를 벗어났으면 추격으로 복귀
    /// </summary>
    private void UpdateAttack()
    {
        if (!HasTarget)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (_isAttacking)
        {
            // 선딜 중에는 계속 타겟을 바라본다 — 타격 후에는 방향을 고정
            if (_attackPhase == EnemyAttackPhase.Windup) FaceTarget();
            return;
        }

        if (!IsTargetWithin(AttackRange + _aiData.attackExitBuffer))
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FaceTarget();

        if (Time.time - _lastAttackTime >= _statData.attackCooldown)
            StartAttack();
    }

    private void UpdateHit()
    {
        _stateTimer += Time.deltaTime;
        if (_stateTimer < _aiData.hitStunDuration) return;

        // 맞은 뒤에는 타겟이 탐지 범위 밖이어도 추격 (loseTargetRange 안이면 유지)
        ChangeState(HasTarget ? EnemyState.Chase : EnemyState.Idle);
    }

    // ── 공격 시퀀스 ───────────────────────────────────────

    private void StartAttack()
    {
        if (_attackContext == null || _attackContext.Target != _target)
        {
            _attackContext = new EnemyAttackContext(
                transform, _target, _statData, _animator, _telegraph, SetAttackPhase);
        }

        _isAttacking = true;
        _attackCts = new CancellationTokenSource();
        RunAttackAsync(_attackCts.Token).Forget();
    }

    private async UniTaskVoid RunAttackAsync(CancellationToken token)
    {
        try
        {
            await RunBehaviorAsync(_attackContext, token);
        }
        catch (OperationCanceledException)
        {
            // 피격 경직/사망/행동 교체로 중단됨 — 정상 흐름
        }
        finally
        {
            if (_telegraph != null) _telegraph.End();
            SetAttackPhase(EnemyAttackPhase.None);
            _isAttacking = false;
            _lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// 실제 공격 실행 — 기본은 주입된 단일 행동. 여러 패턴을 잇는 보스는 오버라이드
    /// </summary>
    protected virtual UniTask RunBehaviorAsync(EnemyAttackContext context, CancellationToken token)
        => _behavior.ExecuteAsync(context, token);

    private void CancelAttack()
    {
        if (_attackCts == null) return;
        _attackCts.Cancel();
        _attackCts.Dispose();
        _attackCts = null;
    }

    private void SetAttackPhase(EnemyAttackPhase phase)
    {
        if (_attackPhase == phase) return;
        _attackPhase = phase;
        AttackPhaseChanged?.Invoke(phase);
    }

    // ── 헬퍼 / 이벤트 ────────────────────────────────────

    /// <summary>플레이어 탐지 판정 — 프로파일러 마커로 비용을 측정한다 (docs/measurements.md)</summary>
    private bool DetectsTarget()
    {
        using (DetectionMarker.Auto())
            return IsTargetWithin(_aiData.detectionRange);
    }

    /// <summary>XZ 평면 거리 비교 — sqrMagnitude로 루트 연산 생략</summary>
    private bool IsTargetWithin(float range)
    {
        if (!HasTarget) return false;

        Vector3 delta = _target.transform.position - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= range * range;
    }

    private void FaceTarget()
    {
        Vector3 dir = _target.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _aiData.turnSpeed);
    }

    /// <summary>이동 속도 비율(0~1)을 Animator에 전달 — Idle/Walk/Run 전환용</summary>
    private void UpdateAnimatorSpeed()
    {
        if (_animator == null) return;

        float speed = _agent.enabled && _agent.isOnNavMesh && !_agent.isStopped
            ? _agent.velocity.magnitude / Mathf.Max(0.01f, _statData.moveSpeed)
            : 0f;
        _animator.SetFloat(SpeedHash, speed, AnimatorSpeedDamp, Time.deltaTime);
    }

    /// <summary>스폰 위치 주변의 NavMesh 위 임의 지점 — 실패 시 false</summary>
    private bool TryGetPatrolPoint(out Vector3 point)
    {
        for (int i = 0; i < PatrolSampleAttempts; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * _aiData.patrolRadius;
            Vector3 candidate = _homePosition + new Vector3(offset.x, 0f, offset.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, PatrolSampleRadius, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        point = default;
        return false;
    }

    private void OnDamaged()
    {
        if (!_aiData.staggerable) return;
        ChangeState(EnemyState.Hit);
    }

    private void OnEnemyDied(GameObject enemy)
    {
        if (enemy == gameObject)
            ChangeState(EnemyState.Dead);
    }

#if UNITY_EDITOR
    /// <summary>선택 시 탐지(노랑) / 추격 포기(회색) / 공격(빨강) 범위 표시</summary>
    private void OnDrawGizmosSelected()
    {
        if (_aiData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _aiData.detectionRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, _aiData.loseTargetRange);
        if (_behaviorData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _behaviorData.attackRange);
        }
    }
#endif
}
