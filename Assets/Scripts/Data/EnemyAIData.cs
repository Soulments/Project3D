using UnityEngine;

/// <summary>
/// 적 AI 튜닝 데이터 — 탐지/순찰/피격 경직 수치를 코드에서 분리
/// 공격 거리·이동 속도·공격력은 CharacterStatData에서 가져온다
/// </summary>
[CreateAssetMenu(fileName = "EnemyAI", menuName = "Data/EnemyAI")]
public class EnemyAIData : ScriptableObject
{
    // ── 탐지 ──────────────────────────────────────────────

    /// <summary>플레이어를 발견하는 거리 (Idle/Patrol → Chase)</summary>
    public float detectionRange = 8f;

    /// <summary>추격을 포기하는 거리 — detectionRange보다 커야 경계에서 상태가 떨리지 않음</summary>
    public float loseTargetRange = 12f;

    /// <summary>공격 사거리를 이만큼 벗어나야 Attack → Chase 로 복귀 (히스테리시스)</summary>
    public float attackExitBuffer = 0.5f;

    // ── 순찰 ──────────────────────────────────────────────

    /// <summary>스폰 위치 기준 순찰 반경</summary>
    public float patrolRadius = 6f;

    /// <summary>순찰 지점 도착 후 대기 시간 (초)</summary>
    public float patrolWaitTime = 2f;

    /// <summary>순찰 시 이동 속도 배율 (추격은 moveSpeed 그대로)</summary>
    public float patrolSpeedMultiplier = 0.5f;

    // ── 기타 ──────────────────────────────────────────────

    /// <summary>피격 경직 시간 (초)</summary>
    public float hitStunDuration = 0.4f;

    /// <summary>추격 중 목적지 갱신 주기 (초) — 매 프레임 SetDestination 방지</summary>
    public float repathInterval = 0.2f;

    /// <summary>공격 중 타겟을 바라보는 회전 속도</summary>
    public float turnSpeed = 10f;
}
