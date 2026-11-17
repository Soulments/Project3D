# 설계 결정 기록

날짜별로 "무엇을 / 왜" 를 짧게 남긴다. README의 "문제 → 선택 → 결과"로 옮길 원재료.

## 2026-11-17 — Phase 3 Step 3: Strategy 패턴 + 공격 선딜

- **행동 전략을 ScriptableObject로 만들었다 (`EnemyBehaviorData : ScriptableObject, IEnemyBehavior`).**
  프리팹이 SO 에셋 하나를 참조하면 되므로 적 종류를 코드 수정 없이 바꿔 끼울 수 있다.
  `EnemyController.SetBehavior(IEnemyBehavior)`로 런타임 주입도 가능 (생성자 주입 대용).
  Play 모드에서 근접 적에게 원거리 SO를 주입해 동작이 바뀌는 것을 확인했다.
- **선딜 → 타격 → 후딜 골격은 베이스 클래스(Template Method)에 고정하고, 구현체는 `Strike()`만 정의한다.**
  모든 적이 같은 타이밍 구조를 갖게 되어 v2.0 패링 판정은 Windup 구간 한 곳에만 붙이면 된다.
  `EnemyController.AttackPhase` / `AttackPhaseChanged`로 외부에 구간을 노출했다.
- **선딜 시간은 감이 아니라 애니메이션 데이터로 정했다.**
  클립을 샘플링해 오른손 속도가 최대인 시점을 구했다 — Slash 2: 0.83s, Slash 3: 1.30s, Jump Attack: 1.17s.
  Slash 1은 클립 이벤트(OnAttackStart@0.51)와 샘플링 결과(0.50)가 일치해 방법을 검증했다.
  근접 적은 Slash 2 기준 선딜 0.8s / 후딜 0.84s.
- **판정은 타격 순간의 상태로 다시 한다.** 선딜 중에 물러나면 빗나간다 (Play 모드에서 HP 불변 확인).
  선딜이 "반응할 수 있는 윈도우"가 되려면 반응의 결과가 판정에 반영되어야 한다.
- **피격 시 공격을 중단한다 (`EnemyAIData.staggerable`).** 보스처럼 경직하지 않는 적은 끈다.
  중단은 `CancellationToken`으로 UniTask를 취소해 구현했다.
- **`ServiceLocator.Unregister<T>(instance)` 추가.** 같은 인스턴스일 때만 제거한다.
  씬 소속 서비스(PlayerHealth, HitEffectPool, ProjectilePool)가 OnDestroy에서 해제해 씬 재진입 시 죽은 참조를 막는다.
- **GameScene의 플레이어/카메라/UI/매니저를 Dungeon_01로 합쳤다.**
  씬이 둘로 나뉘어 있으면 "입장 → 처치 → 보스" 검증과 Phase 4 씬 흐름을 만들 수 없다.

## 2026-11-17 — Phase 3 Step 4: 적 탐지 비용 측정

- **기준을 측정 전에 먼저 커밋했다** (30마리 0.5 ms / 100마리 2.0 ms 이상이면 병목). 수치를 본 뒤 기준을 맞추지 않으려는 장치.
- **결과: 병목이 아니라 QuadTree를 채택하지 않았다.** 100마리에서 현재 방식 0.062 ms, OverlapSphere 0.187 ms (기준의 3% / 9%).
  상세 수치·한계는 `docs/measurements.md`.
- 계획서는 `Physics.OverlapSphere`를 베이스라인으로 가정했지만 현재 구현은 제곱거리 비교다.
  그래서 **두 방식을 같은 조건에서 모두 측정**해 "더 싼 쪽이 이미 쓰이고 있다"는 것까지 확인했다.
- 측정 중 **플레이어가 `Player` 레이어가 아니라 `Default` 레이어**에 있는 것을 발견해 옮겼다 (Phase 2 "Physics Layer 분리"의 누락).
  재귀로 바꾸다 `PlayerAttack` 레이어의 `WeaponHitBox`까지 바뀐 것은 다시 되돌렸다.
- 이 측정으로 **다음 병목 후보**가 나왔다: 적 100마리에서 FPS 260 → 125인데 탐지는 0.06 ms뿐 → Phase 5 Profiler 사례 후보.
