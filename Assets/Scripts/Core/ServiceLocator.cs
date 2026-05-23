using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    // 타입을 키로 사용하는 서비스 딕셔너리
    private static readonly Dictionary<Type, object> _services = new();

    /// <summary>
    /// 서비스 등록
    /// </summary>
    /// <param name="service">등록할 서비스 인스턴스</param>
    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
            Debug.LogWarning($"ServiceLocator: {type.Name} 이미 등록됨. 덮어씀.");
        _services[type] = service;
    }

    /// <summary>
    /// 등록된 서비스 조회 — 미등록 시 에러 로그 출력
    /// </summary>
    /// <param name="service">조회된 서비스 인스턴스</param>
    /// <returns>등록 여부</returns>
    public static bool TryGet<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }
        Debug.LogError($"ServiceLocator: {typeof(T).Name} 등록되지 않음.");
        service = default;
        return false;
    }

    /// <summary>
    /// 씬 전환 시 서비스 초기화 — 씬 종속 서비스 정리용
    /// </summary>
    public static void Clear() => _services.Clear();
}