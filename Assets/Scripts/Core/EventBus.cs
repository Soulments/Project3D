using System;
using UnityEngine;

public static class EventBus
{
    // ── 플레이어 관련 이벤트 ──────────────────────────────

    /// <summary>플레이어 HP 변경 (현재 HP, 최대 HP)</summary>
    public static event Action<float, float> OnPlayerHPChanged;

    /// <summary>플레이어 사망</summary>
    public static event Action OnPlayerDied;

    // ── 적 관련 이벤트 ───────────────────────────────────

    /// <summary>적 사망 — 사망한 적의 GameObject 전달</summary>
    public static event Action<GameObject> OnEnemyDied;

    // ── 전투 이벤트 ──────────────────────────────────────

    /// <summary>공격 히트 — 히트스탑 / 카메라 쉐이크 트리거용 (Phase 2)</summary>
    public static event Action<Vector3> OnHitLanded;
    public static void HitLanded(Vector3 hitPosition) => OnHitLanded?.Invoke(hitPosition);

    // ── 발행 메서드 ──────────────────────────────────────

    /// <summary>
    /// 플레이어 HP 변경 이벤트 발행
    /// </summary>
    /// <param name="current">현재 HP</param>
    /// <param name="max">최대 HP</param>
    public static void PlayerHPChanged(float current, float max)
        => OnPlayerHPChanged?.Invoke(current, max);

    /// <summary>플레이어 사망 이벤트 발행</summary>
    public static void PlayerDied()
        => OnPlayerDied?.Invoke();

    /// <summary>
    /// 적 사망 이벤트 발행
    /// </summary>
    /// <param name="enemy">사망한 적의 GameObject</param>
    public static void EnemyDied(GameObject enemy)
        => OnEnemyDied?.Invoke(enemy);

    // ── 스킬 관련 이벤트 ─────────────────────────────────

    /// <summary>스킬 쿨타임 변경 (스킬 인덱스, 총 쿨타임, 경과 시간)</summary>
    public static event Action<int, float, float> OnSkillCooldownChanged;

    /// <summary>스킬 쿨타임 변경 이벤트 발행</summary>
    public static void SkillCooldownChanged(int skillIndex, float total, float elapsed)
        => OnSkillCooldownChanged?.Invoke(skillIndex, total, elapsed);
}