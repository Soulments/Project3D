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

    // 보스 페이즈 전환은 일반 타격보다 길게 멈춰 "전환 순간"을 강조한다
    [SerializeField] private float _phaseChangeStopDuration = 0.25f;

    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        EventBus.OnHitLanded += OnHitLanded;
        EventBus.OnBossPhaseChanged += OnBossPhaseChanged;
    }

    private void OnDisable()
    {
        EventBus.OnHitLanded -= OnHitLanded;
        EventBus.OnBossPhaseChanged -= OnBossPhaseChanged;
    }

    private void OnHitLanded(Vector3 hitPosition)
        => StartHitStopAsync(_stopDuration).Forget();

    private void OnBossPhaseChanged(int phase)
        => StartHitStopAsync(_phaseChangeStopDuration).Forget();

    /// <summary>
    /// Time.timeScale을 순간 정지 후 UniTask로 복귀
    /// 이전 히트스탑 진행 중이면 취소 후 재시작
    /// </summary>
    private async UniTaskVoid StartHitStopAsync(float duration)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        Time.timeScale = _stopTimeScale;

        await UniTask.Delay(
            (int)(duration * 1000),
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