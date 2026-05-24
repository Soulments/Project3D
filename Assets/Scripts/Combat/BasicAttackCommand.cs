using UnityEngine;

/// <summary>
/// 기본 공격 커맨드
/// 쿨타임 없이 즉시 실행, Animator Attack 트리거 발동
/// </summary>
public class BasicAttackCommand : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public BasicAttackCommand(Animator animator, CharacterStatData statData)
    {
        _animator = animator;
        _statData = statData;
    }

    public bool CanExecute() => true; // 기본 공격은 항상 실행 가능

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"BasicAttack — 공격력: {_statData.attackPower}");
    }
}