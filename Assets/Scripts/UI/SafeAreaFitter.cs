using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        // SafeArea 변경 시에만 재적용 (화면 회전 대응)
        if (_lastSafeArea != Screen.safeArea)
            ApplySafeArea();
    }

    /// <summary>
    /// Screen.safeArea 기준으로 앵커 min/max 계산 후 적용
    /// </summary>
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // SafeArea를 0~1 비율로 변환해 앵커에 적용
        Vector2 anchorMin = safeArea.position / screenSize;
        Vector2 anchorMax = (safeArea.position + safeArea.size) / screenSize;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}