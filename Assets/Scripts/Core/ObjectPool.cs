using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제네릭 Object Pool — Instantiate/Destroy 제거로 GC 최소화
/// 히트 이펙트 등 빈번하게 생성/삭제되는 오브젝트에 적용
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool = new();

    /// <summary>
    /// Pool 초기화
    /// </summary>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 생성 개수</param>
    /// <param name="parent">비활성 오브젝트를 담을 부모 Transform</param>
    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateNew());
    }

    /// <summary>Pool에서 오브젝트 꺼내기 — 없으면 새로 생성</summary>
    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
        obj.gameObject.SetActive(true);
        return obj;
    }

    /// <summary>오브젝트 반환 — 비활성화 후 Pool에 다시 넣기</summary>
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        if (_parent != null)
            obj.transform.SetParent(_parent);
        _pool.Enqueue(obj);
    }

    private T CreateNew()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        return obj;
    }
}