using System;
using UnityEngine;

/// <summary>
/// 전역 이벤트 버스 — Observer 패턴 구현
/// </summary>
/// <description>
/// 컴포넌트 간 직접 참조 없이 이벤트로 통신하는 구조.
/// FindObjectOfType, GameObject.Find 없이 느슨한 결합 달성.
/// 구독은 반드시 OnEnable/OnDisable 쌍으로 관리해 메모리 누수 방지.
/// </description>
public static class EventBus
{
    // ── 플레이어 관련 이벤트 ──────────────────────────────

    /// <summary>플레이어 HP 변경 (현재 HP, 최대 HP)</summary>
    public static event Action<float, float> OnPlayerHPChanged;

    /// <summary>플레이어 사망</summary>
    public static event Action OnPlayerDied;

    // ── 적 관련 이벤트 ───────────────────────────────────

    /// <summary>적 사망 (사망한 적의 GameObject 전달)</summary>
    public static event Action<GameObject> OnEnemyDied;

    // ── 호출 메서드 ──────────────────────────────────────

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
}