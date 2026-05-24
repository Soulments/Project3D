using UnityEngine;
using System;

/// <summary>
/// 히트 이펙트 컴포넌트 — ObjectPool로 관리
/// 파티클 재생 완료 후 자동으로 Pool에 반환
/// </summary>
public class HitEffect : MonoBehaviour
{
    private ParticleSystem _particle;
    private Action<HitEffect> _returnToPool;

    void Awake()
    {
        _particle = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Pool에서 꺼낼 때 호출 — 위치 설정 후 파티클 재생
    /// </summary>
    /// <param name="position">이펙트 발생 위치</param>
    /// <param name="returnCallback">Pool 반환 콜백</param>
    public void Play(Vector3 position, Action<HitEffect> returnCallback)
    {
        _returnToPool = returnCallback;
        transform.position = position;
        _particle.Play();
    }

    // 파티클 Stop Action이 Disable이면 OnDisable이 자동 호출됨
    void OnDisable()
    {
        _returnToPool?.Invoke(this);
    }
}