using UnityEngine;

/// <summary>
/// 모바일 터치 컨트롤(가상 조이스틱/버튼) 표시 제어
/// OnScreenStick/OnScreenButton이 게임패드 경로(<Gamepad>/leftStick 등)에 값을 넣으므로
/// PlayerInputActions의 기존 게임패드 바인딩이 그대로 동작한다 — 입력 처리 코드는 수정하지 않는다
/// 터치 기기에서만 켜서, PC에서는 가상 게임패드 장치가 만들어지지 않게 한다
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
    [SerializeField] private GameObject _controlsRoot;

    // 에디터에서 모바일 UI를 확인할 때 켠다
    [SerializeField] private bool _forceShow;

    void Awake()
        => _controlsRoot.SetActive(_forceShow || Application.isMobilePlatform);
}
