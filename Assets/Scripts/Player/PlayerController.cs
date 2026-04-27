using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력 처리 및 이동 컨트롤러
/// New Input System 기반으로 키보드/게임패드 동시 지원
/// Step 6 FSM 구현 시 CharacterController 기반으로 교체 예정
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Inspector에서 이동 속도 조절 가능하도록 SerializeField 사용
    [SerializeField] private float _moveSpeed = 5f;

    // Input System 액션 인스턴스 (Generate C# Class로 자동 생성된 클래스)
    private PlayerInputActions _input;

    // 매 프레임 입력값을 저장할 변수
    private Vector2 _moveDir;

    /// <summary>
    /// 컴포넌트 초기화 - Input 인스턴스 생성
    /// Start보다 먼저 실행되므로 의존성 초기화에 사용
    /// </summary>
    void Awake()
    {
        _input = new PlayerInputActions();
    }

    /// <summary>
    /// 오브젝트 활성화 시 Input 활성화 및 이벤트 구독
    /// OnDisable과 쌍으로 관리해 메모리 누수 방지
    /// </summary>
    void OnEnable()
    {
        _input.Player.Enable();
        // Attack은 performed 이벤트로 구독 (버튼을 눌렀을 때 1회 발생)
        _input.Player.Attack.performed += OnAttack;
    }

    /// <summary>
    /// 오브젝트 비활성화 시 Input 비활성화 및 이벤트 해제
    /// 이벤트 해제를 먼저 하고 Disable 호출
    /// </summary>
    void OnDisable()
    {
        _input.Player.Attack.performed -= OnAttack;
        _input.Player.Disable();
    }

    /// <summary>
    /// 매 프레임 이동 입력 처리
    /// WASD / 게임패드 왼쪽 스틱 모두 Vector2로 읽힘 (2D Vector Composite)
    /// Step 6에서 카메라 방향 기준 이동으로 교체 예정 (TPS)
    /// </summary>
    void Update()
    {
        _moveDir = _input.Player.Move.ReadValue<Vector2>();

        // 입력의 Y값을 3D 공간의 Z축으로 변환 (3D 기준 앞뒤 이동)
        Vector3 move = new Vector3(_moveDir.x, 0f, _moveDir.y);
        transform.Translate(move * _moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 공격 입력 콜백 - performed: 버튼이 눌린 순간 1회 호출
    /// Phase 2에서 실제 공격 로직으로 교체 예정
    /// </summary>
    private void OnAttack(InputAction.CallbackContext ctx)
    {
        Debug.Log("Attack!");
    }
}