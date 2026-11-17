/// <summary>
/// 적 공격 한 번의 진행 단계
/// Windup 구간이 "반응 가능한 윈도우" — v2.0 패링 판정은 이 구간에 붙인다
/// </summary>
public enum EnemyAttackPhase { None, Windup, Strike, Recovery }
