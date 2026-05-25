/// <summary>
/// 피격 가능한 오브젝트 인터페이스
/// 플레이어/적 모두 이 인터페이스를 구현해 HitBox와 디커플링
/// </summary>
public interface IDamageable
{
    /// <summary>데미지를 받아 HP를 감소시킨다.</summary>
    /// <param name="amount">받을 데미지 수치</param>
    void TakeDamage(float amount);

    /// <summary>현재 생존 여부</summary>
    bool IsAlive { get; }
}