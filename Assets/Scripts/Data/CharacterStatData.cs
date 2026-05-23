using UnityEngine;

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