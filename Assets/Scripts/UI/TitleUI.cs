using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면 — 게임 시작 / 종료
/// 씬 전환은 SceneLoader(ServiceLocator)를 거친다
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;

    void Awake()
    {
        // 던전에서 잠근 마우스 커서를 풀어 버튼을 누를 수 있게 한다
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
        _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnStartClicked()
        => SceneLoader.Go(SceneNames.Dungeon);

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        // 에디터에서는 Application.Quit이 동작하지 않으므로 Play 모드를 종료한다
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
