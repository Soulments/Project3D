using UnityEngine;

/// <summary>
/// 적 투사체 Pool 관리 컴포넌트 — 씬에 하나 두고 ServiceLocator로 조회
/// Instantiate/Destroy 없이 ObjectPool로 재사용
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private EnemyProjectile _prefab;
    [SerializeField] private int _initialSize = 8;

    private ObjectPool<EnemyProjectile> _pool;
    private System.Action<EnemyProjectile> _release;

    void Awake()
    {
        _pool = new ObjectPool<EnemyProjectile>(_prefab, _initialSize, transform);
        _release = _pool.Return; // 발사마다 델리게이트를 새로 만들지 않도록 캐시
        ServiceLocator.Register(this);
    }

    void OnDestroy()
        => ServiceLocator.Unregister(this);

    /// <summary>투사체 발사</summary>
    public void Fire(Vector3 origin, Vector3 direction, float speed, float damage)
    {
        EnemyProjectile projectile = _pool.Get();
        projectile.Launch(origin, direction, speed, damage, _release);
    }
}
