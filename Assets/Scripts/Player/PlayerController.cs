using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _jumpForce = 5f;

    private CharacterController _characterController;
    private PlayerInputActions _input;
    private Vector2 _moveDir;
    private float _verticalVelocity;
    private PlayerState _currentState = PlayerState.Idle;
    private const float Gravity = -9.81f;

    // Ctrl 토글로 달리기/걷기 모드 전환 — 기본값 달리기
    private bool _isRunMode = true;

    // Animator Parameter 해시 — string 비교보다 성능 효율적
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunModeHash = Animator.StringToHash("IsRunMode");

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerInputActions();
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
        _input.Player.Jump.performed += OnJump;
        _input.Player.Sprint.performed += OnSprintToggle;
        _animator?.SetBool(IsRunModeHash, _isRunMode);
    }

    void OnDisable()
    {
        _input.Player.Jump.performed -= OnJump;
        _input.Player.Sprint.performed -= OnSprintToggle;
        _input.Player.Disable();
    }

    void Update()
    {
        HandleGravity();
        _animator?.SetBool(IsGroundedHash, _characterController.isGrounded);

        switch (_currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
            case PlayerState.Jump:
                HandleMove();
                break;
            case PlayerState.Dead:
                break;
        }

        // 착지 감지 — Jump 상태에서 isGrounded 되면 Idle로 복귀
        if (_currentState == PlayerState.Jump && _characterController.isGrounded)
            _currentState = PlayerState.Idle;
    }

    /// <summary>
    /// 중력 처리 — isGrounded 시 소량 음수로 고정해 바닥 감지 안정화
    /// </summary>
    private void HandleGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -1f;
        else
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// 이동 방향 계산 + Animator 파라미터 갱신
    /// 실제 이동은 OnAnimatorMove()의 Root Motion이 담당
    /// </summary>
    private void HandleMove()
    {
        _moveDir = _input.Player.Move.ReadValue<Vector2>();

        float inputMag = new Vector3(_moveDir.x, 0f, _moveDir.y).magnitude;
        float animSpeed = inputMag > 0.01f ? (_isRunMode ? 1f : 0.5f) : 0f;
        _animator?.SetFloat(SpeedHash, animSpeed);

        if (_currentState != PlayerState.Jump)
            _currentState = inputMag > 0.01f ? PlayerState.Move : PlayerState.Idle;

        // 카메라 방향 기준 이동 방향으로 캐릭터 회전
        if (inputMag > 0.01f)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
            Vector3 camRight = new Vector3(cam.right.x, 0f, cam.right.z).normalized;
            Vector3 moveDir = camForward * _moveDir.y + camRight * _moveDir.x;

            Quaternion targetRot = Quaternion.LookRotation(
                new Vector3(moveDir.x, 0f, moveDir.z));
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// Root Motion 이동에 중력/점프 수직 속도를 합산해 CharacterController에 전달
    /// Apply Root Motion이 켜져 있을 때 Unity가 자동 호출
    /// </summary>
    private void OnAnimatorMove()
    {
        // Root Motion의 수평 이동 + 수직 속도(중력/점프) 합산
        Vector3 deltaPos = _animator.deltaPosition;
        deltaPos.y = _verticalVelocity * Time.deltaTime;
        _characterController.Move(deltaPos);
    }

    /// <summary>
    /// 달리기/걷기 모드 토글 — Ctrl 누를 때마다 전환, Animator에도 동기화
    /// </summary>
    private void OnSprintToggle(InputAction.CallbackContext ctx)
    {
        _isRunMode = !_isRunMode;
        _animator?.SetBool(IsRunModeHash, _isRunMode);
    }

    /// <summary>
    /// 점프 입력 콜백 — isGrounded 상태에서만 점프 가능
    /// </summary>
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_currentState == PlayerState.Dead) return;
        if (!_characterController.isGrounded) return;
        _verticalVelocity = _jumpForce;
        _currentState = PlayerState.Jump;
        _animator?.SetTrigger(JumpHash);
    }

    /// <summary>
    /// 피격 처리 — Phase 2 HitBox에서 호출 예정
    /// </summary>
    public void OnHit()
    {
        if (_currentState == PlayerState.Dead) return;
        _currentState = PlayerState.Hit;
        _animator?.SetTrigger(HitHash);
    }

    /// <summary>
    /// 사망 상태로 전환 — EventBus OnPlayerDied 구독으로 호출
    /// </summary>
    public void SetDead()
    {
        _currentState = PlayerState.Dead;
        _animator?.SetBool(IsDeadHash, true);
    }
}