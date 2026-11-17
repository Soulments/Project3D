/// <summary>
/// 씬을 넘어 유지되는 세션 데이터 — ServiceLocator에 등록되어 씬 간 값 전달에 쓴다
/// (DontDestroyOnLoad Singleton을 추가하는 대신 일반 클래스 + ServiceLocator)
/// </summary>
public class GameSession
{
    /// <summary>가장 최근에 끝난 판의 결과 — 결과 화면이 읽는다</summary>
    public RunResult LastResult { get; set; }

    /// <summary>세션 누적 사망 횟수</summary>
    public int TotalDeaths { get; set; }
}
