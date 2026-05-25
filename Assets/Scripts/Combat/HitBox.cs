using UnityEngine;

/// <summary>
/// 공격 히트박스 컴포넌트 — OnTriggerEnter 기반 피격 판정
/// SetActive(true/false)로 공격 타이밍에만 활성화
/// </summary>
[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    /// <summary>한 번의 공격에서 중복 피격을 방지하는 플래그</summary>
    private bool _hasHit;

    public float Damage { get; set; }

    private void OnEnable()
    {
        // 활성화될 때마다 중복 피격 플래그 초기화
        _hasHit = false;
    }

    /// <summary>
    /// 트리거 진입 시 IDamageable 컴포넌트를 찾아 TakeDamage 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;
        if (!damageable.IsAlive) return;

        damageable.TakeDamage(Damage);
        _hasHit = true;

        EventBus.HitLanded(transform.position);
        // 히트스탑/카메라 쉐이크는 Step 6에서 OnHitLanded 구독
    }
}