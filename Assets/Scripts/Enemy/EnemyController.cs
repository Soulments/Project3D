using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 AI FSM 베이스 — switch 기반 상태 전환 + NavMeshAgent 순찰/추격
/// 상태별 동작을 virtual 메서드로 분리해 Step 3에서 IEnemyBehavior로 교체할 여지를 둔다
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private EnemyAIData _aiData;

    private const int PatrolSampleAttempts = 5;
    private const float PatrolSampleRadius = 2f;
    private const float PatrolArriveTolerance = 0.3f;

    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private PlayerHealth _target;

    private EnemyState _state = EnemyState.Idle;
    private Vector3 _homePosition;
    private float _stateTimer;
    private float _repathTimer;
    private float _lastAttackTime = float.NegativeInfinity;

    /// <summary>현재 FSM 상태 (디버그/테스트용)</summary>
    public EnemyState State => _state;

    private bool HasTarget => _target != null && _target.IsAlive;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();
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
    }

    /// <summary>
    /// 스폰 위치 기록 + 플레이어 조회 (FindObjectOfType 대신 ServiceLocator)
    /// </summary>
    void Start()
    {
        _homePosition = transform.position;
        ServiceLocator.TryGet(out _target);

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
    }

    // ── 상태 전환 ─────────────────────────────────────────

    /// <summary>
    /// 상태 진입 처리 — 에이전트 속도/정지 여부를 상태에 맞게 설정
    /// Dead는 종료 상태라 이후 전환하지 않는다
    /// </summary>
    private void ChangeState(EnemyState next)
    {
        if (_state == EnemyState.Dead) return;

        _state = next;
        _stateTimer = 0f;

        switch (next)
        {
            case EnemyState.Idle:
            case EnemyState.Attack:
            case EnemyState.Hit:
            case EnemyState.Dead:
                StopAgent();
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
                _agent.stoppingDistance = _statData.attackRange * 0.8f;
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
        if (IsTargetWithin(_aiData.detectionRange))
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
        if (IsTargetWithin(_aiData.detectionRange))
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

        if (IsTargetWithin(_statData.attackRange))
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

    private void UpdateAttack()
    {
        if (!HasTarget)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (!IsTargetWithin(_statData.attackRange + _aiData.attackExitBuffer))
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FaceTarget();

        if (Time.time - _lastAttackTime >= _statData.attackCooldown)
        {
            _lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    private void UpdateHit()
    {
        _stateTimer += Time.deltaTime;
        if (_stateTimer < _aiData.hitStunDuration) return;

        // 맞은 뒤에는 타겟이 탐지 범위 밖이어도 추격 (loseTargetRange 안이면 유지)
        ChangeState(HasTarget ? EnemyState.Chase : EnemyState.Idle);
    }

    // ── 확장 지점 ─────────────────────────────────────────

    /// <summary>
    /// 공격 실행 — 현재는 즉시 데미지
    /// Step 3에서 공격 선딜(예비 동작) → 공격 → 후딜을 가진 IEnemyBehavior로 교체
    /// </summary>
    protected virtual void PerformAttack()
        => _target.TakeDamage(_statData.attackPower);

    // ── 헬퍼 / 이벤트 ────────────────────────────────────

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

    /// <summary>스폰 위치 주변의 NavMesh 위 임의 지점 — 실패 시 false</summary>
    private bool TryGetPatrolPoint(out Vector3 point)
    {
        for (int i = 0; i < PatrolSampleAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * _aiData.patrolRadius;
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
        => ChangeState(EnemyState.Hit);

    private void OnEnemyDied(GameObject enemy)
    {
        if (enemy == gameObject)
            ChangeState(EnemyState.Dead);
    }

#if UNITY_EDITOR
    /// <summary>선택 시 탐지(노랑) / 추격 포기(회색) / 공격(빨강) 범위 표시</summary>
    private void OnDrawGizmosSelected()
    {
        if (_aiData == null || _statData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _aiData.detectionRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, _aiData.loseTargetRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _statData.attackRange);
    }
#endif
}
