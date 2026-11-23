/// <summary>
/// 한 판(던전 입장 ~ 종료)의 결과 데이터 — MonoBehaviour가 아닌 일반 클래스
/// GameSession을 통해 던전 씬에서 결과 씬으로 전달된다 (Singleton을 늘리지 않기 위함)
/// </summary>
public class RunResult
{
    /// <summary>보스 처치로 클리어했는가 (false면 플레이어 사망)</summary>
    public bool IsCleared;

    /// <summary>클리어/종료까지 걸린 시간(초) — 히트스탑(timeScale 0)과 무관한 unscaled 시간</summary>
    public float ElapsedSeconds;

    /// <summary>이번 판에 처치한 적 수 (보스 포함)</summary>
    public int KillCount;

    /// <summary>이번 세션의 누적 사망 횟수 — 한 판은 사망 즉시 끝나므로 판 단위로는 0 또는 1</summary>
    public int TotalDeaths;
}
