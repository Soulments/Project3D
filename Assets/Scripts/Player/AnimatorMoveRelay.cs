using UnityEngine;

/// <summary>
/// Model의 Animator Root Motion을 부모 PlayerController로 전달하는 릴레이
/// Animator와 같은 오브젝트(Model)에 부착해야 함
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimatorMoveRelay : MonoBehaviour
{
    private Animator _animator;
    private PlayerController _playerController;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerController = GetComponentInParent<PlayerController>();
    }

    /// <summary>
    /// Apply Root Motion이 켜져 있을 때 Unity가 이 오브젝트에서 자동 호출
    /// </summary>
    private void OnAnimatorMove()
        => _playerController.OnAnimatorMove(_animator);
}