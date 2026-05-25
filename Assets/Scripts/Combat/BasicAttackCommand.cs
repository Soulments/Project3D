using UnityEngine;

/// <summary>
/// 기본공격 커맨드 — 마우스 좌클릭
/// HitBox 활성화로 피격 판정 시작
/// </summary>
public class BasicAttackCommand : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private readonly Transform _ownerTransform;
    private readonly HitBox _hitBox;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    /// <summary>
    /// <param name="hitBox">플레이어 무기에 부착된 HitBox 컴포넌트</param>
    /// </summary>
    public BasicAttackCommand(Animator animator, CharacterStatData statData, Transform ownerTransform, HitBox hitBox)
    {
        _animator = animator;
        _statData = statData;
        _ownerTransform = ownerTransform;
        _hitBox = hitBox;
    }

    public bool CanExecute() => true;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        _hitBox.Damage = _statData.attackPower;
        _hitBox.gameObject.SetActive(true);
        // HitBox 비활성화는 Animation Event로 처리 (Step 5-6)
    }

    public void Dispose() { }
}