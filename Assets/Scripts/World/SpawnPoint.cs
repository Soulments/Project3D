using UnityEngine;

/// <summary>스폰 지점의 종류.</summary>
public enum SpawnType { Player, Enemy }

/// <summary>던전 내 스폰/배치 지점 마커. 에디터 기즈모로 위치·방향을 표시한다.</summary>
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private SpawnType _spawnType = SpawnType.Enemy;

    public SpawnType Type => _spawnType;
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = _spawnType == SpawnType.Player
            ? new Color(0.4f, 0.8f, 1f)
            : new Color(1f, 0.4f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
#endif
}