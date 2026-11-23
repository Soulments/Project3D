#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Object Pool Before/After 측정 도구 (docs/measurements.md)
/// 같은 부하(매 프레임 N개 생성, 일정 프레임 뒤 제거)를 Instantiate/Destroy 방식과 ObjectPool 방식으로
/// 번갈아 실행해 GC 할당량, GC 횟수, 스폰/반환 CPU 시간, 프레임 시간을 잰다
/// 개발 빌드/에디터 전용 — 릴리스 빌드에는 포함되지 않는다
/// </summary>
public class PoolBenchmark : MonoBehaviour
{
    private struct Live
    {
        public Transform Instance;
        public int DespawnFrame;
    }

    /// <summary>
    /// 프리팹별로 [Instantiate/Destroy → Pool]을 repeat번 교대로 측정하고 CSV로 저장
    /// </summary>
    public async UniTask RunAsync(
        IReadOnlyList<(string label, Transform prefab)> prefabs,
        int spawnsPerFrame, int lifetimeFrames, int frames, int repeat, string outputPath)
    {
        var csv = new StringBuilder(
            "target,method,round,spawns,gc_alloc_kb,gc_gen0,spawn_cpu_us_per_frame,frame_ms_mean,frame_ms_p95,frame_ms_max,fps\n");

        foreach ((string label, Transform prefab) in prefabs)
        {
            for (int round = 1; round <= repeat; round++)
            {
                foreach (bool pooled in new[] { false, true })
                {
                    string row = await MeasureAsync(label, prefab, pooled, round, spawnsPerFrame, lifetimeFrames, frames);
                    csv.AppendLine(row);
                    await UniTask.Delay(300, ignoreTimeScale: true);
                }
            }
        }

        File.WriteAllText(outputPath, csv.ToString());
    }

    private async UniTask<string> MeasureAsync(
        string label, Transform prefab, bool pooled, int round, int spawnsPerFrame, int lifetimeFrames, int frames)
    {
        // 풀은 최대 동시 생존 수만큼 미리 만들어 둔다 (로딩 시 1회 비용 — 측정 구간에 포함하지 않는다)
        int peak = spawnsPerFrame * lifetimeFrames + spawnsPerFrame;
        ObjectPool<Transform> pool = pooled ? new ObjectPool<Transform>(prefab, peak, transform) : null;

        var live = new Queue<Live>();
        var frameTimes = new List<float>(frames);
        var stopwatch = new Stopwatch();
        long spawnTicks = 0;
        int spawns = 0;

        await UniTask.NextFrame();
        GC.Collect();
        await UniTask.NextFrame();

        int gen0Before = GC.CollectionCount(0);
        using ProfilerRecorder gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        long gcAllocBytes = 0;
        float startTime = Time.realtimeSinceStartup;

        for (int frame = 0; frame < frames; frame++)
        {
            await UniTask.NextFrame();
            frameTimes.Add(Time.unscaledDeltaTime);
            gcAllocBytes += gcRecorder.LastValue;

            stopwatch.Restart();

            while (live.Count > 0 && live.Peek().DespawnFrame <= frame)
            {
                Transform instance = live.Dequeue().Instance;
                if (pooled) pool.Return(instance);
                else Destroy(instance.gameObject);
            }

            for (int i = 0; i < spawnsPerFrame; i++)
            {
                Vector3 position = new Vector3(UnityEngine.Random.Range(-5f, 5f), 1f, UnityEngine.Random.Range(-5f, 5f));
                Transform instance;
                if (pooled)
                {
                    instance = pool.Get();
                    instance.position = position;
                }
                else
                {
                    instance = Instantiate(prefab, position, Quaternion.identity, transform);
                }

                live.Enqueue(new Live { Instance = instance, DespawnFrame = frame + lifetimeFrames });
                spawns++;
            }

            stopwatch.Stop();
            spawnTicks += stopwatch.ElapsedTicks;
        }

        float elapsed = Time.realtimeSinceStartup - startTime;
        int gen0 = GC.CollectionCount(0) - gen0Before;

        // 정리
        while (live.Count > 0)
        {
            Transform instance = live.Dequeue().Instance;
            if (pooled) pool.Return(instance);
            else Destroy(instance.gameObject);
        }

        await UniTask.NextFrame();
        if (pooled)
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
        }

        frameTimes.Sort();
        double mean = 0;
        foreach (float t in frameTimes) mean += t;
        mean /= frameTimes.Count;
        double spawnUs = spawnTicks * 1_000_000.0 / Stopwatch.Frequency / frames;

        return string.Format("{0},{1},{2},{3},{4:F0},{5},{6:F1},{7:F3},{8:F3},{9:F3},{10:F0}",
            label, pooled ? "ObjectPool" : "Instantiate/Destroy", round, spawns, gcAllocBytes / 1024.0, gen0, spawnUs,
            mean * 1000.0, frameTimes[(int)(frameTimes.Count * 0.95)] * 1000.0, frameTimes[^1] * 1000.0, frames / elapsed);
    }
}
#endif
