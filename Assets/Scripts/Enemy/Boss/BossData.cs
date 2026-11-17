using System;
using UnityEngine;

/// <summary>
/// 보스 페이즈 한 단계 — HP가 enterHpRatio 이하가 되면 진입하고, sequence의 패턴을 순서대로 잇는다
/// </summary>
[Serializable]
public class BossPhase
{
    /// <summary>이 페이즈에 진입하는 HP 비율 (0~1). 첫 페이즈는 1</summary>
    [Range(0f, 1f)] public float enterHpRatio = 1f;

    /// <summary>한 번의 공격에서 이어서 실행할 패턴 목록 (UniTask 체이닝)</summary>
    public EnemyBehaviorData[] sequence;

    /// <summary>패턴 사이 간격 (초)</summary>
    public float sequenceGap = 0.4f;

    /// <summary>이동 속도 배율</summary>
    public float moveSpeedMultiplier = 1f;
}

/// <summary>
/// 보스 데이터 — 페이즈 구성을 SO로 분리해 코드 수정 없이 패턴/임계값을 조정한다
/// </summary>
[CreateAssetMenu(fileName = "BossData", menuName = "Data/BossData")]
public class BossData : ScriptableObject
{
    /// <summary>페이즈 목록 — enterHpRatio가 큰 순서(1 → 0)로 정렬</summary>
    public BossPhase[] phases;

    /// <summary>패턴 사이에서 대상이 사거리 + 이 값보다 멀면 해당 패턴은 건너뛴다 (m)</summary>
    public float chainRangeMargin = 1f;
}
