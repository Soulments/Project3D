/// <summary>
/// 스킬 커맨드 인터페이스 — Command Pattern
/// 모든 스킬은 이 인터페이스를 구현
/// </summary>
public interface ISkillCommand
{
    /// <summary>스킬을 실행한다.</summary>
    void Execute();

    /// <summary>스킬 실행 가능 여부 — 쿨타임, 상태 조건 체크</summary>
    bool CanExecute();

    /// <summary>CancellationTokenSource를 정리한다.</summary>
    void Dispose();
}