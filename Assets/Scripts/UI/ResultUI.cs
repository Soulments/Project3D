using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결과 화면 — 성공/실패를 같은 씬에서 분기 표시하고 타이틀/재도전으로 이동
/// 값은 GameSession.LastResult에서 읽는다 (던전 씬의 DungeonRunController가 기록)
/// </summary>
public class ResultUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _headlineText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _killText;
    [SerializeField] private TMP_Text _deathText;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _titleButton;

    [SerializeField] private Color _clearColor = new Color(0.45f, 0.9f, 0.5f);
    [SerializeField] private Color _failColor = new Color(0.95f, 0.35f, 0.3f);

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnEnable()
    {
        _retryButton.onClick.AddListener(OnRetryClicked);
        _titleButton.onClick.AddListener(OnTitleClicked);
    }

    void OnDisable()
    {
        _retryButton.onClick.RemoveListener(OnRetryClicked);
        _titleButton.onClick.RemoveListener(OnTitleClicked);
    }

    void Start()
    {
        // 결과 씬을 단독으로 열었을 때(결과 없음)도 깨지지 않게 한다
        RunResult result = ServiceLocator.TryGet(out GameSession session) ? session.LastResult : null;
        Show(result);
    }

    private void Show(RunResult result)
    {
        if (result == null)
        {
            _headlineText.text = "NO RESULT";
            _timeText.text = "TIME  --:--.-";
            _killText.text = "ENEMIES DEFEATED  -";
            _deathText.text = "DEATHS  -";
            return;
        }

        _headlineText.text = result.IsCleared ? "VICTORY" : "DEFEAT";
        _headlineText.color = result.IsCleared ? _clearColor : _failColor;
        _timeText.text = $"TIME  {FormatTime(result.ElapsedSeconds)}";
        _killText.text = $"ENEMIES DEFEATED  {result.KillCount}";
        _deathText.text = $"DEATHS  {result.TotalDeaths}";
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        return $"{minutes:00}:{seconds - minutes * 60f:00.0}";
    }

    private void OnRetryClicked()
        => SceneLoader.Go(SceneNames.Dungeon);

    private void OnTitleClicked()
        => SceneLoader.Go(SceneNames.Title);
}
