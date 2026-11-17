using UnityEngine;

/// <summary>
/// 공격 선딜 경고 표시 — 선딜 동안 모델이 붉게 물들고, 범위 공격이면 바닥에 범위 원이 차오른다
/// 플레이어가 반응할 타이밍을 시각적으로 알려주는 용도 (v2.0 패링 확장에도 그대로 사용)
/// </summary>
public class EnemyTelegraph : MonoBehaviour
{
    [SerializeField] private Color _warnColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] private Transform _areaIndicator;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private SkinnedMeshRenderer[] _renderers;
    private Color[] _baseColors;
    private MaterialPropertyBlock _block;
    private float _duration;
    private float _elapsed;
    private float _areaRadius;
    private bool _active;

    void Awake()
    {
        _block = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // 재질의 원래 색(변종 틴트 포함)에서 경고색으로 섞는다
        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            Material material = _renderers[i].sharedMaterial;
            _baseColors[i] = material != null && material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : Color.white;
        }

        if (_areaIndicator != null)
            _areaIndicator.gameObject.SetActive(false);
    }

    /// <summary>선딜 시작 — duration 동안 경고색을 점점 진하게</summary>
    /// <param name="areaRadius">0보다 크면 바닥 범위 표시</param>
    public void Begin(float duration, float areaRadius = 0f)
    {
        _duration = Mathf.Max(0.01f, duration);
        _elapsed = 0f;
        _areaRadius = areaRadius;
        _active = true;

        if (_areaIndicator != null)
            _areaIndicator.gameObject.SetActive(areaRadius > 0f);
        ApplyProgress(0f);
    }

    /// <summary>선딜 종료 또는 중단 — 원래 모습으로 복구</summary>
    public void End()
    {
        if (!_active) return;
        _active = false;

        foreach (SkinnedMeshRenderer r in _renderers)
            r.SetPropertyBlock(null);
        if (_areaIndicator != null)
            _areaIndicator.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_active) return;

        _elapsed += Time.deltaTime;
        ApplyProgress(Mathf.Clamp01(_elapsed / _duration));
    }

    private void ApplyProgress(float t)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _block.SetColor(BaseColorId, Color.Lerp(_baseColors[i], _warnColor, t));
            _renderers[i].SetPropertyBlock(_block);
        }

        if (_areaIndicator != null && _areaRadius > 0f)
        {
            // 월드 기준 반경에 맞추기 위해 부모 스케일을 보정
            float diameter = _areaRadius * 2f * t;
            Vector3 parentScale = _areaIndicator.parent != null ? _areaIndicator.parent.lossyScale : Vector3.one;
            _areaIndicator.localScale = new Vector3(
                diameter / Mathf.Max(0.0001f, parentScale.x), 0.02f, diameter / Mathf.Max(0.0001f, parentScale.z));
        }
    }

    void OnDisable()
        => End();
}
