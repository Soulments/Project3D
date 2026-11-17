using System;
using UnityEngine;

/// <summary>
/// 적 투사체 — ProjectilePool로 관리, 플레이어에 닿거나 벽에 닿거나 수명이 다하면 Pool에 반환
/// </summary>
[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float _lifetime = 4f;

    private Vector3 _direction;
    private float _speed;
    private float _damage;
    private float _age;
    private bool _active;
    private Action<EnemyProjectile> _returnToPool;

    /// <summary>Pool에서 꺼낸 직후 호출 — 위치/방향/수치 설정</summary>
    public void Launch(Vector3 origin, Vector3 direction, float speed, float damage,
        Action<EnemyProjectile> returnCallback)
    {
        transform.position = origin;
        transform.forward = direction;
        _direction = direction;
        _speed = speed;
        _damage = damage;
        _age = 0f;
        _returnToPool = returnCallback;
        _active = true;
    }

    void Update()
    {
        if (!_active) return;

        transform.position += _direction * (_speed * Time.deltaTime);
        _age += Time.deltaTime;
        if (_age >= _lifetime)
            Release();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_active) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null)
        {
            if (player.IsAlive) player.TakeDamage(_damage);
            Release();
            return;
        }

        // 다른 트리거와 같은 편(적)은 통과, 벽 같은 일반 콜라이더에만 막힌다
        if (other.isTrigger) return;
        if (other.GetComponentInParent<EnemyHealth>() != null) return;
        Release();
    }

    private void Release()
    {
        if (!_active) return;
        _active = false;
        _returnToPool?.Invoke(this);
    }
}
