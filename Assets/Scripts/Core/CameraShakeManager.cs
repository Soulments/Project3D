using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 카메라 쉐이크 관리자 — Cinemachine Impulse 기반
/// EventBus.OnHitLanded 구독으로 자동 트리거
/// </summary>
public class CameraShakeManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private float _impulseForce = 0.5f;

    private void OnEnable()
        => EventBus.OnHitLanded += OnHitLanded;

    private void OnDisable()
        => EventBus.OnHitLanded -= OnHitLanded;

    /// <summary>
    /// 히트 위치에서 Impulse 발생 — 카메라 쉐이크 트리거
    /// </summary>
    private void OnHitLanded(Vector3 hitPosition)
        => _impulseSource.GenerateImpulse(_impulseForce);
}