using UnityEngine;

/// <summary>
/// 히트 이펙트 Pool 관리 컴포넌트
/// EventBus.OnHitLanded 구독으로 공격 히트 시 자동 재생
/// </summary>
public class HitEffectPool : MonoBehaviour
{
    [SerializeField] private HitEffect _prefab;
    [SerializeField] private int _initialSize = 5;

    private ObjectPool<HitEffect> _pool;
    private bool _isReturning = false; // 무한루프 방지 플래그

    void Awake()
    {
        _pool = new ObjectPool<HitEffect>(_prefab, _initialSize, transform);
        ServiceLocator.Register<HitEffectPool>(this);
    }

    void OnEnable() => EventBus.OnHitLanded += OnHitLanded;
    void OnDisable() => EventBus.OnHitLanded -= OnHitLanded;

    /// <summary>
    /// 공격 히트 이벤트 수신 — 히트 위치에 이펙트 재생
    /// </summary>
    private void OnHitLanded(Vector3 hitPosition)
    {
        HitEffect effect = _pool.Get();
        effect.Play(hitPosition, ReturnEffect);
    }

    /// <summary>
    /// 무한루프 방지 플래그로 안전하게 반환
    /// </summary>
    private void ReturnEffect(HitEffect effect)
    {
        if (_isReturning) return;
        _isReturning = true;
        _pool.Return(effect);
        _isReturning = false;
    }
}