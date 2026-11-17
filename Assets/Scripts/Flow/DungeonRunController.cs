using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 던전 한 판의 진행 관리 — 시간 측정, 처치 수 집계, 클리어/실패 판정, 결과 씬 전환
/// 보스 처치(OnBossDefeated) → 클리어, 플레이어 사망(OnPlayerDied) → 실패
/// 어떤 이벤트가 먼저 오든 한 번만 종료 처리한다
/// </summary>
public class DungeonRunController : MonoBehaviour
{
    // 사망/보스 처치 연출(애니메이션)을 보여준 뒤 결과 씬으로 넘어간다
    [SerializeField] private float _endDelaySeconds = 2.5f;

    private float _startTime;
    private float _endTime;
    private int _killCount;
    private bool _finished;
    private bool _cleared;

    void OnEnable()
    {
        EventBus.OnEnemyDied += OnEnemyDied;
        EventBus.OnBossDefeated += OnBossDefeated;
        EventBus.OnPlayerDied += OnPlayerDied;
    }

    void OnDisable()
    {
        EventBus.OnEnemyDied -= OnEnemyDied;
        EventBus.OnBossDefeated -= OnBossDefeated;
        EventBus.OnPlayerDied -= OnPlayerDied;
    }

    void Start()
        => _startTime = Time.unscaledTime;

    private void OnEnemyDied(GameObject enemy)
        => _killCount++;

    private void OnBossDefeated()
        => Finish(cleared: true);

    private void OnPlayerDied()
        => Finish(cleared: false);

    private void Finish(bool cleared)
    {
        if (_finished) return;
        _finished = true;
        _cleared = cleared;

        // 종료 시각은 이벤트가 온 순간으로 고정 (히트스탑이 timeScale을 0으로 만들어도 흐르는 시간 기준)
        _endTime = Time.unscaledTime;

        FinishAndTransitionAsync().Forget();
    }

    /// <summary>
    /// OnBossDefeated는 보스의 OnEnemyDied 처리 도중 발행되므로, 같은 프레임의 처치 집계가
    /// 끝난 뒤(한 프레임 뒤)에 결과를 만들어야 보스 처치가 처치 수에 포함된다
    /// </summary>
    private async UniTaskVoid FinishAndTransitionAsync()
    {
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        await UniTask.Yield(token);

        if (!ServiceLocator.TryGet(out GameSession session)) return;
        if (!_cleared) session.TotalDeaths++;

        session.LastResult = new RunResult
        {
            IsCleared = _cleared,
            ElapsedSeconds = _endTime - _startTime,
            KillCount = _killCount,
            TotalDeaths = session.TotalDeaths,
        };

        // 사망/보스 처치 연출(애니메이션)을 보여준 뒤 결과 씬으로 넘어간다
        await UniTask.Delay(TimeSpan.FromSeconds(_endDelaySeconds), ignoreTimeScale: true, cancellationToken: token);
        SceneLoader.Go(SceneNames.Result);
    }
}
