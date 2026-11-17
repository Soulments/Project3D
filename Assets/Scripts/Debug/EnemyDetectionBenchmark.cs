#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 탐지 비용 측정 도구 (docs/measurements.md)
/// 같은 조건에서 현재 방식(거리 비교, 마커 Enemy.Detection)과
/// Physics.OverlapSphere 방식(마커 Bench.OverlapSphere)의 프레임당 비용을 잰다
/// 개발 빌드/에디터 전용 — 릴리스 빌드에는 포함되지 않는다
/// </summary>
public class EnemyDetectionBenchmark : MonoBehaviour
{
    [SerializeField] private EnemyController _enemyPrefab;
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private LayerMask _targetMask;

    // 플레이어 스폰(z=-20)에서 멀리 떨어진 영역에 흩뿌려 측정 중 추격/공격이 발생하지 않게 한다
    [SerializeField] private Vector2 _spawnMin = new Vector2(-23f, 4f);
    [SerializeField] private Vector2 _spawnMax = new Vector2(23f, 23f);

    private static readonly ProfilerMarker OverlapMarker = new ProfilerMarker("Bench.OverlapSphere");

    private readonly List<EnemyController> _spawned = new();
    private readonly Collider[] _buffer = new Collider[8];
    private bool _useOverlapSphere;

    /// <summary>적을 count마리가 되도록 생성/제거</summary>
    public void SetCount(int count)
    {
        while (_spawned.Count > count)
        {
            Destroy(_spawned[^1].gameObject);
            _spawned.RemoveAt(_spawned.Count - 1);
        }

        while (_spawned.Count < count)
        {
            Vector3 candidate = new Vector3(
                Random.Range(_spawnMin.x, _spawnMax.x), 0f, Random.Range(_spawnMin.y, _spawnMax.y));
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)) continue;

            EnemyController enemy = Instantiate(_enemyPrefab, hit.position + Vector3.up, Quaternion.identity, transform);
            _spawned.Add(enemy);
        }
    }

    void Update()
    {
        if (!_useOverlapSphere) return;

        foreach (EnemyController enemy in _spawned)
        {
            using (OverlapMarker.Auto())
                Physics.OverlapSphereNonAlloc(enemy.transform.position, _detectionRange, _buffer, _targetMask);
        }
    }

    /// <summary>
    /// 방식 × 적 수 조합별로 프레임당 탐지 비용(µs)과 FPS를 측정해 CSV로 저장
    /// </summary>
    public async UniTask RunSuiteAsync(int[] counts, int warmupFrames, int sampleFrames, string outputPath)
    {
        var csv = new StringBuilder("method,enemies,mean_us,median_us,p95_us,max_us,fps\n");

        foreach (bool overlap in new[] { false, true })
        {
            foreach (int count in counts)
            {
                _useOverlapSphere = overlap;
                SetCount(count);
                for (int i = 0; i < warmupFrames; i++) await UniTask.NextFrame();

                string marker = overlap ? "Bench.OverlapSphere" : "Enemy.Detection";
                using ProfilerRecorder recorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, marker);
                var samples = new List<double>(sampleFrames);
                float startTime = Time.realtimeSinceStartup;

                for (int i = 0; i < sampleFrames; i++)
                {
                    await UniTask.NextFrame();
                    samples.Add(recorder.LastValue / 1000.0); // ns → µs
                }

                float fps = sampleFrames / (Time.realtimeSinceStartup - startTime);
                samples.Sort();
                double mean = 0;
                foreach (double s in samples) mean += s;
                mean /= samples.Count;

                csv.AppendLine(string.Format("{0},{1},{2:F1},{3:F1},{4:F1},{5:F1},{6:F0}",
                    overlap ? "OverlapSphere" : "DistanceCheck", count, mean,
                    samples[samples.Count / 2], samples[(int)(samples.Count * 0.95)], samples[^1], fps));
            }
        }

        _useOverlapSphere = false;
        SetCount(0);
        File.WriteAllText(outputPath, csv.ToString());
    }
}
#endif
