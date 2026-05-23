using UnityEngine;

/// <summary>
/// 캐릭터 스탯 데이터 ScriptableObject
/// </summary>
/// <description>
/// 플레이어/적 스탯을 코드와 분리해 외부 데이터로 관리.
/// 인스턴스별로 다른 스탯을 Inspector에서 직접 조정 가능.
/// Phase 5에서 Addressables 연동 예정.
/// </description>
[CreateAssetMenu(fileName = "CharacterStat", menuName = "Data/CharacterStat")]
public class CharacterStatData : ScriptableObject
{
    // ── 기본 스탯 ─────────────────────────────────────────

    /// <summary>최대 HP</summary>
    public float maxHP = 100f;

    /// <summary>이동 속도</summary>
    public float moveSpeed = 5f;

    // ── 전투 스탯 ─────────────────────────────────────────

    /// <summary>공격력</summary>
    public float attackPower = 10f;

    /// <summary>공격 범위</summary>
    public float attackRange = 2f;

    /// <summary>공격 쿨다운 (초)</summary>
    public float attackCooldown = 1f;
}