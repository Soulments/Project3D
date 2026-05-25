using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 스킬 콤보 시퀀스 관리 — 원신형 시너지
/// 콤보 윈도우 내에서 스킬 A → B 순서 입력 시 콤보 이벤트 발행
/// </summary>
public class SkillSequencer : MonoBehaviour
{
    [SerializeField] private float _comboWindowSeconds = 1.5f;

    private int _lastSkillIndex = -1;
    private bool _inComboWindow;
    private CancellationTokenSource _comboCts;

    /// <summary>
    /// 스킬 실행 시 호출 — 콤보 판정 및 윈도우 리셋
    /// </summary>
    /// <param name="index">0=기본공격, 1=Skill1, 2=Skill2, 3=Skill3</param>
    public void OnSkillExecuted(int index)
    {
        if (_inComboWindow && _lastSkillIndex != -1)
            TryTriggerCombo(_lastSkillIndex, index);

        _lastSkillIndex = index;
        ResetComboWindowAsync().Forget();
    }

    /// <summary>
    /// UniTask 기반 콤보 윈도우 리셋 — 이전 윈도우 취소 후 재시작
    /// </summary>
    private async UniTaskVoid ResetComboWindowAsync()
    {
        _comboCts?.Cancel();
        _comboCts?.Dispose();
        _comboCts = new CancellationTokenSource();

        _inComboWindow = true;
        await UniTask.Delay(
            (int)(_comboWindowSeconds * 1000),
            cancellationToken: _comboCts.Token
        );

        _inComboWindow = false;
        _lastSkillIndex = -1;
    }

    /// <summary>
    /// 이전 스킬과 현재 스킬 조합으로 콤보 이벤트 발행
    /// </summary>
    private void TryTriggerCombo(int prev, int curr)
    {
        if (prev == 1 && curr == 2)
            EventBus.ComboTriggered(ComboType.Skill1To2);
        else if (prev == 2 && curr == 3)
            EventBus.ComboTriggered(ComboType.Skill2To3);
    }

    private void OnDestroy()
    {
        _comboCts?.Cancel();
        _comboCts?.Dispose();
    }
}