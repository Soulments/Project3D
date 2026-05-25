using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 히트스탑 관리자 — 공격 히트 시 Time.timeScale 순간 정지 후 복귀
/// EventBus.OnHitLanded 구독으로 자동 트리거
/// </summary>
public class HitStopManager : MonoBehaviour
{
    [SerializeField] private float _stopDuration = 0.08f;
    [SerializeField] private float _stopTimeScale = 0f;

    private CancellationTokenSource _cts;

    private void OnEnable()
        => EventBus.OnHitLanded += OnHitLanded;

    private void OnDisable()
        => EventBus.OnHitLanded -= OnHitLanded;

    private void OnHitLanded(Vector3 hitPosition)
        => StartHitStopAsync().Forget();

    /// <summary>
    /// Time.timeScale을 순간 정지 후 UniTask로 복귀
    /// 이전 히트스탑 진행 중이면 취소 후 재시작
    /// </summary>
    private async UniTaskVoid StartHitStopAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        Time.timeScale = _stopTimeScale;

        await UniTask.Delay(
            (int)(_stopDuration * 1000),
            ignoreTimeScale: true,
            cancellationToken: _cts.Token
        );

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}