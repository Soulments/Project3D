using UnityEngine;

/// <summary>
/// 기본 공격 커맨드
/// 공격 실행 시 Animator 트리거 + EventBus로 히트 이펙트 발생
/// </summary>
public class BasicAttackCommand : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private readonly Transform _ownerTransform;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public BasicAttackCommand(Animator animator, CharacterStatData statData, Transform ownerTransform)
    {
        _animator = animator;
        _statData = statData;
        _ownerTransform = ownerTransform;
    }

    public bool CanExecute() => true;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"BasicAttack — 공격력: {_statData.attackPower}");

        // 임시 — Step 4 히트박스 구현 후 HitBox에서 호출로 교체 예정
        EventBus.HitLanded(_ownerTransform.position + _ownerTransform.forward);
    }
}