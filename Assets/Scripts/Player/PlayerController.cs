using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Inspector에서 Player_Stat SO 연결
    [SerializeField] private CharacterStatData _statData;

    private CharacterController _characterController;
    private PlayerInputActions _input;
    private Vector2 _moveDir;
    private float _verticalVelocity;
    private PlayerState _currentState = PlayerState.Idle;
    private const float Gravity = -9.81f;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerInputActions();
        // 커서 숨기기 + 화면 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 한 프레임 대기 후 Input 활성화
    /// Play 시작 시 로딩 중 마우스 이동이 카메라에 반영되는 문제 방지
    /// </summary>
    IEnumerator Start()
    {
        _input.Player.Disable();
        yield return null;
        _input.Player.Enable();
        _input.Player.Attack.performed += OnAttack;
    }

    void OnDisable()
    {
        _input.Player.Attack.performed -= OnAttack;
        _input.Player.Disable();
    }

    void Update()
    {
        HandleGravity();
        switch (_currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
                HandleMove();
                break;
            case PlayerState.Dead:
                break;
        }
    }

    /// <summary>
    /// 중력 처리 — isGrounded 시 소량 음수로 고정해 바닥 감지 안정화
    /// </summary>
    private void HandleGravity()
    {
        if (_characterController.isGrounded)
            _verticalVelocity = -1f;
        else
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// TPS 이동 — Main Camera forward/right 기준으로 이동 방향 계산
    /// 플레이어는 이동 방향으로만 회전, 카메라와 독립적으로 동작
    /// </summary>
    private void HandleMove()
    {
        _moveDir = _input.Player.Move.ReadValue<Vector2>();

        // Main Camera Y축 방향 기준으로 이동 방향 계산
        Transform cam = Camera.main.transform;
        Vector3 camForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 camRight = new Vector3(cam.right.x, 0f, cam.right.z).normalized;

        Vector3 moveVec = camForward * _moveDir.y + camRight * _moveDir.x;
        moveVec.y = _verticalVelocity;

        _characterController.Move(moveVec * _statData.moveSpeed * Time.deltaTime);

        // 이동 상태 갱신
        _currentState = new Vector3(_moveDir.x, 0f, _moveDir.y).sqrMagnitude > 0.01f
            ? PlayerState.Move : PlayerState.Idle;

        // 이동 방향으로 캐릭터 회전
        if (new Vector3(_moveDir.x, 0f, _moveDir.y).sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(
                new Vector3(moveVec.x, 0f, moveVec.z));
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// 공격 입력 콜백 — Phase 2에서 SkillHandler 호출로 교체 예정
    /// </summary>
    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_currentState == PlayerState.Dead) return;
        Debug.Log("Attack!");
    }

    /// <summary>
    /// 사망 상태로 전환 — EventBus OnPlayerDied 구독으로 호출
    /// </summary>
    public void SetDead()
    {
        _currentState = PlayerState.Dead;
    }
}