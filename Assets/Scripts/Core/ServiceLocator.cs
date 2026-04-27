using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 서비스 등록 및 조회를 담당하는 정적 로케이터
/// </summary>
/// <description>
/// Singleton 남용 방지를 위한 의존성 관리 패턴.
/// UIManager, SceneLoader, SaveSystem 등 전역 접근이 필요하지만
/// Singleton까지는 불필요한 서비스를 등록해서 사용.
/// FindObjectOfType, GameObject.Find 대체 목적.
/// </description>
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
    /// 등록된 서비스 조회
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
    /// 씬 전환 시 서비스 초기화 (씬 종속 서비스 정리용)
    /// </summary>
    public static void Clear() => _services.Clear();
}