using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 스킬 3 커맨드 — R 키
/// Skill2보다 긴 쿨타임, 가장 높은 배율
/// </summary>
public class Skill3Command : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private bool _isReady = true;
    private CancellationTokenSource _cts;

    public Skill3Command(Animator animator, CharacterStatData statData)
    {
        _animator = animator;
        _statData = statData;
    }

    public bool CanExecute() => _isReady;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"Skill3 — 공격력: {_statData.attackPower * 3f}");
        StartCooldown().Forget();
    }

    /// <summary>
    /// UniTask 기반 쿨타임 처리 — GC 최소화
    /// </summary>
    private async UniTaskVoid StartCooldown()
    {
        _isReady = false;
        float cooldown = _statData.attackCooldown * 3f;
        EventBus.SkillCooldownChanged(2, cooldown, 0f);

        _cts = new CancellationTokenSource();
        float elapsed = 0f;

        while (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
            EventBus.SkillCooldownChanged(2, cooldown, elapsed);
            await UniTask.Yield(_cts.Token);
        }

        _isReady = true;
        EventBus.SkillCooldownChanged(2, cooldown, cooldown);
    }

    public void Dispose() => _cts?.Cancel();
}