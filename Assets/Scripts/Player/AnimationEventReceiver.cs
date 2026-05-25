using UnityEngine;

/// <summary>
/// 애니메이션 이벤트 수신 컴포넌트
/// Animator가 붙은 오브젝트에 함께 부착
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private HitBox _hitBox;

    /// <summary>
    /// 애니메이션 클립에서 공격 판정 시작 시점에 호출
    /// </summary>
    public void OnAttackStart()
        => _hitBox.gameObject.SetActive(true);

    /// <summary>
    /// 애니메이션 클립에서 공격 판정 종료 시점에 호출
    /// </summary>
    public void OnAttackEnd()
        => _hitBox.gameObject.SetActive(false);
}