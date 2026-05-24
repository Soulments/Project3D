using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 스킬 실행 관리 컴포넌트 — Command Pattern
/// PlayerController의 입력을 받아 각 스킬 커맨드 실행
/// </summary>
public class SkillHandler : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterStatData _statData;

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
        _basicAttack = new BasicAttackCommand(_animator, _statData);
        _skill1 = new Skill1Command(_animator, _statData);
        _skill2 = new Skill2Command(_animator, _statData);
        _skill3 = new Skill3Command(_animator, _statData);

        _input = new PlayerInputActions();
        _input.Player.Disable();
        yield return null;
        _input.Player.Enable();

        _input.Player.Attack.performed += ctx => TryExecute(_basicAttack);
        _input.Player.Skill1.performed += ctx => TryExecute(_skill1);
        _input.Player.Skill2.performed += ctx => TryExecute(_skill2);
        _input.Player.Skill3.performed += ctx => TryExecute(_skill3);
    }

    void OnDisable()
    {
        _input?.Player.Disable();
        (_skill1 as Skill1Command)?.Dispose();
        (_skill2 as Skill2Command)?.Dispose();
        (_skill3 as Skill3Command)?.Dispose();
    }

    /// <summary>
    /// CanExecute 확인 후 스킬 실행
    /// </summary>
    private void TryExecute(ISkillCommand command)
    {
        if (command.CanExecute())
            command.Execute();
    }
}