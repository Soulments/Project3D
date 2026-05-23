using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // 게임 상태 열거형
    public enum GameState { Playing, Paused, GameOver }

    public GameState CurrentState { get; private set; }

    /// <summary>
    /// 초기화 — 게임 상태를 Playing으로 설정
    /// </summary>
    protected override void Awake()
    {
        // 부모 Awake 먼저 호출 (DontDestroyOnLoad 처리)
        base.Awake();
        CurrentState = GameState.Playing;
    }

    /// <summary>
    /// 게임 상태 변경
    /// </summary>
    /// <param name="newState">변경할 게임 상태</param>
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"GameState changed: {newState}");
    }

    /// <summary>
    /// 앱 백그라운드 진입/복귀 시 호출
    /// </summary>
    /// <param name="pause">true: 백그라운드 진입 / false: 포그라운드 복귀</param>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 백그라운드 진입 시 자동 세이브 (Phase 4 SaveSystem 구현 후 연결)
            Debug.Log("App paused - 자동 세이브 예정");
        }
    }

    /// <summary>
    /// 앱 포커스 변경 시 호출
    /// </summary>
    /// <param name="focus">true: 포커스 복귀 / false: 포커스 소실</param>
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            // 포커스 복귀 시 BGM 재개 (AudioManager 구현 후 연결)
            Debug.Log("App focused - BGM 재개 예정");
        }
        else
        {
            // 포커스 소실 시 BGM 일시정지
            Debug.Log("App unfocused - BGM 일시정지 예정");
        }
    }
}