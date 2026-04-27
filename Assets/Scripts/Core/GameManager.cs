using UnityEngine;

/// <summary>
/// 게임 전반적인 상태를 관리하는 매니저
/// </summary>
/// <description>
/// Singleton 패턴 적용. 씬 전환 시에도 유지됨 (DontDestroyOnLoad).
/// 게임 상태(Playing/Paused/GameOver) 관리 및 모바일 생명주기 처리 담당.
/// AudioManager는 ServiceLocator로 참조.
/// </description>
public class GameManager : Singleton<GameManager>
{
    // 게임 상태 열거형
    public enum GameState { Playing, Paused, GameOver }

    public GameState CurrentState { get; private set; }

    /// <summary>
    /// 초기화 - 게임 상태를 Playing으로 설정
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
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"GameState changed: {newState}");
    }

    /// <summary>
    /// 앱이 백그라운드로 진입하거나 복귀할 때 호출
    /// </summary>
    /// <description>
    /// pause == true  : 백그라운드 진입 → 자동 세이브 연결 예정 (Phase 4 SaveSystem 구현 후)
    /// pause == false : 포그라운드 복귀 → 필요 시 상태 복원
    /// </description>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 백그라운드 진입 시 자동 세이브 (Phase 4 SaveSystem 구현 후 연결)
            Debug.Log("App paused - 자동 세이브 예정");
        }
    }

    /// <summary>
    /// 앱 포커스 변경 시 호출 (다른 앱 전환 등)
    /// </summary>
    /// <description>
    /// focus == true  : 포커스 복귀 → BGM 재개 연결 예정 (Phase 1 AudioManager 구현 후)
    /// focus == false : 포커스 소실 → BGM 일시정지
    /// </description>
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