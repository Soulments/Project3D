using UnityEngine;

/// <summary>
/// 제네릭 Singleton 베이스 클래스
/// </summary>
/// <description>
/// MonoBehaviour 기반 Singleton 패턴 구현.
/// GameManager, AudioManager 등 전역 매니저에서 상속해서 사용.
/// Singleton은 2~3개로 제한하고 나머지는 ServiceLocator 사용 권장.
/// </description>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            // 인스턴스가 없으면 씬에서 자동 탐색
            if (_instance == null)
                _instance = FindAnyObjectByType<T>();
            return _instance;
        }
    }

    /// <summary>
    /// 씬 로드 시 중복 인스턴스 제거 및 DontDestroyOnLoad 설정
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // 이미 인스턴스가 존재하면 중복 오브젝트 제거
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        // 씬 전환 시에도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }
}