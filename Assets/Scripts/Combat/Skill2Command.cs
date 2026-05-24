using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 스킬 2 커맨드 — Q 키
/// Skill1보다 긴 쿨타임, 높은 배율
/// </summary>
public class Skill2Command : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private bool _isReady = true;
    private CancellationTokenSource _cts;

    public Skill2Command(Animator animator, CharacterStatData statData)
    {
        _animator = animator;
        _statData = statData;
    }

    public bool CanExecute() => _isReady;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"Skill2 — 공격력: {_statData.attackPower * 2f}");
        StartCooldown().Forget();
    }

    private async UniTaskVoid StartCooldown()
    {
        _isReady = false;
        EventBus.SkillCooldownChanged(1, _statData.attackCooldown * 2f, 0f);

        _cts = new CancellationTokenSource();
        float elapsed = 0f;
        float cooldown = _statData.attackCooldown * 2f;

        while (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
            EventBus.SkillCooldownChanged(1, cooldown, elapsed);
            await UniTask.Yield(_cts.Token);
        }

        _isReady = true;
        EventBus.SkillCooldownChanged(1, cooldown, cooldown);
    }

    public void Dispose() => _cts?.Cancel();
}