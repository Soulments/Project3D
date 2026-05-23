/// <summary>
/// 플레이어 FSM 상태 열거형
/// Phase 3에서 각 상태를 클래스로 분리하는 리팩토링 예정
/// </summary>
public enum PlayerState
{
    Idle,    // 정지 상태
    Move,    // 이동 상태
    Attack,  // 공격 상태 — Phase 2에서 구현
    Hit,     // 피격 상태 — Phase 2에서 구현
    Dead     // 사망 상태
}