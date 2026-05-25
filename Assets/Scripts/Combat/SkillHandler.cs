using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 스킬 실행 관리 컴포넌트 — Command Pattern
/// PlayerController의 입력을 받아 각 스킬 커맨드 실행
/// </summary>
public class SkillHandler : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterStatData _statData;
    [SerializeField] private SkillSequencer _sequencer;
    [SerializeField] private HitBox _hitBox;

    private PlayerInputActions _input;
    private ISkillCommand _basicAttack;
    private ISkillCommand _skill1;
    private ISkillCommand _skill2;
    private ISkillCommand _skill3;

    /// <summary>
    /// 스킬 커맨드 인스턴스 생성 및 입력 구독
    /// </summary>
    IEnumerator Start()
    {
        // transform을 ownerTransform으로 전달
        _basicAttack = new BasicAttackCommand(_animator, _statData, transform, _hitBox);
        _skill1 = new Skill1Command(_animator, _statData, 1);
        _skill2 = new Skill2Command(_animator, _statData, 2);
        _skill3 = new Skill3Command(_animator, _statData, 3);

        _input = new PlayerInputActions();
        _input.Player.Disable();
        yield return null;
        _input.Player.Enable();

        _input.Player.Attack.performed += ctx => TryExecute(_basicAttack, 0);
        _input.Player.Skill1.performed += ctx => TryExecute(_skill1, 1);
        _input.Player.Skill2.performed += ctx => TryExecute(_skill2, 2);
        _input.Player.Skill3.performed += ctx => TryExecute(_skill3, 3);
    }

    void OnDisable()
    {
        _input?.Player.Disable();
        _basicAttack?.Dispose();
        _skill1?.Dispose();
        _skill2?.Dispose();
        _skill3?.Dispose();
    }

    /// <summary>
    /// CanExecute 확인 후 스킬 실행 및 시퀀서에 알림
    /// </summary>
    private void TryExecute(ISkillCommand command, int index)
    {
        if (!command.CanExecute()) return;
        command.Execute();
        _sequencer.OnSkillExecuted(index);
    }
}