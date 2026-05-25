using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 스킬 2 커맨드 — Q 키
/// Skill1보다 긴 쿨타임, 높은 배율
/// </summary>
public class Skill2Command : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private readonly int _skillIndex;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private bool _isReady = true;
    private CancellationTokenSource _cts;

    /// <summary>
    /// <param name="skillIndex">EventBus 쿨타임 이벤트에 사용할 스킬 인덱스</param>
    /// </summary>
    public Skill2Command(Animator animator, CharacterStatData statData, int skillIndex)
    {
        _animator = animator;
        _statData = statData;
        _skillIndex = skillIndex;
    }

    public bool CanExecute() => _isReady;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"Skill2 — 공격력: {_statData.attackPower * 2f}");
        StartCooldown().Forget();
    }

    /// <summary>
    /// UniTask 기반 쿨타임 처리 — GC 최소화
    /// </summary>
    private async UniTaskVoid StartCooldown()
    {
        _isReady = false;
        float cooldown = _statData.attackCooldown * 2f;
        EventBus.SkillCooldownChanged(_skillIndex, cooldown, 0f);

        _cts = new CancellationTokenSource();
        float elapsed = 0f;

        while (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
            EventBus.SkillCooldownChanged(_skillIndex, cooldown, elapsed);
            await UniTask.Yield(_cts.Token);
        }

        _isReady = true;
        EventBus.SkillCooldownChanged(_skillIndex, cooldown, cooldown);
    }

    public void Dispose() => _cts?.Cancel();
}