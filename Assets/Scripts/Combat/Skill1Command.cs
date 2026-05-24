using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 스킬 1 커맨드 — E 키
/// UniTask 기반 쿨타임 관리
/// Phase 2 Step 2에서 실제 효과 구현 예정
/// </summary>
public class Skill1Command : ISkillCommand
{
    private readonly Animator _animator;
    private readonly CharacterStatData _statData;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private bool _isReady = true;
    private CancellationTokenSource _cts;

    public Skill1Command(Animator animator, CharacterStatData statData)
    {
        _animator = animator;
        _statData = statData;
    }

    public bool CanExecute() => _isReady;

    public void Execute()
    {
        _animator.SetTrigger(AttackHash);
        Debug.Log($"Skill1 — 공격력: {_statData.attackPower * 1.5f}");
        StartCooldown().Forget();
    }

    /// <summary>
    /// UniTask 기반 쿨타임 처리 — GC 최소화
    /// </summary>
    private async UniTaskVoid StartCooldown()
    {
        _isReady = false;
        EventBus.SkillCooldownChanged(0, _statData.attackCooldown, 0f);

        _cts = new CancellationTokenSource();
        float elapsed = 0f;

        while (elapsed < _statData.attackCooldown)
        {
            elapsed += Time.deltaTime;
            EventBus.SkillCooldownChanged(0, _statData.attackCooldown, elapsed);
            await UniTask.Yield(_cts.Token);
        }

        _isReady = true;
        EventBus.SkillCooldownChanged(0, _statData.attackCooldown, _statData.attackCooldown);
    }

    public void Dispose() => _cts?.Cancel();
}