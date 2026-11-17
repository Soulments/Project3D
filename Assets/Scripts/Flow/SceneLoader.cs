using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 서비스 — ServiceLocator로 조회하며, 모든 씬 전환은 여기를 거친다
/// (직접 SceneManager.LoadScene 호출 금지)
/// 앱 시작 시 한 번 등록되고 씬을 넘어 유지되므로 씬 소속 서비스처럼 해제하지 않는다
/// </summary>
public class SceneLoader
{
    private bool _isLoading;

    /// <summary>어느 씬에서 Play를 시작해도 첫 씬이 로드되기 전에 영속 서비스를 등록한다</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterPersistentServices()
    {
        ServiceLocator.Register(new SceneLoader());
        ServiceLocator.Register(new GameSession());
    }

    /// <summary>ServiceLocator에서 SceneLoader를 꺼내 전환을 시작 — 호출하는 쪽을 단순하게 유지</summary>
    public static void Go(string sceneName)
    {
        if (ServiceLocator.TryGet(out SceneLoader loader))
            loader.LoadAsync(sceneName).Forget();
    }

    /// <summary>
    /// 씬 비동기 로드 — 로딩 중 중복 호출은 무시한다
    /// 히트스탑(timeScale 0) 도중에 전환돼도 다음 씬이 멈춘 채 시작하지 않도록 timeScale을 복구한다
    /// </summary>
    public async UniTask LoadAsync(string sceneName)
    {
        if (_isLoading) return;
        _isLoading = true;
        Time.timeScale = 1f;

        try
        {
            await SceneManager.LoadSceneAsync(sceneName);
        }
        finally
        {
            _isLoading = false;
        }

        Time.timeScale = 1f;
    }
}
