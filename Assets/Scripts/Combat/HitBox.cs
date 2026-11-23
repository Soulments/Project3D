using UnityEngine;

/// <summary>
/// 공격 히트박스 컴포넌트 — OnTriggerEnter 기반 피격 판정
/// SetActive(true/false)로 공격 타이밍에만 활성화
/// 트리거 이벤트는 양쪽 중 하나에 Rigidbody가 있어야 발생하므로 kinematic Rigidbody를 요구한다
/// (적 콜라이더에는 Rigidbody가 없다 — 없으면 겹쳐도 OnTriggerEnter가 호출되지 않는다)
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class HitBox : MonoBehaviour
{
    /// <summary>한 번의 공격에서 중복 피격을 방지하는 플래그</summary>
    private bool _hasHit;

    public float Damage { get; set; }

    private void Reset()
    {
        // 컴포넌트를 처음 붙일 때 물리 영향을 받지 않는 kinematic으로 설정
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

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