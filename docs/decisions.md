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

## 2026-11-17 — Phase 3 Step 5: 보스 몬스터

- **보스는 `EnemyController`의 서브클래스(`BossController`)다.** FSM(Idle/Chase/Attack/Hit/Dead)은 그대로 쓰고,
  변하는 부분만 `virtual` 확장 지점(`RunBehaviorAsync`, `AttackRange`, `MoveSpeed`, `HasBehavior`)으로 열었다.
  보스 전용 상태 머신을 따로 만들면 순찰/추격/사망 처리를 중복해서 짜게 된다.
- **페이즈 구성은 `BossData` SO로 분리했다.** 임계값, 패턴 목록, 간격, 이동 속도 배율을 코드 수정 없이 조정한다.
  패턴 하나하나는 근접/원거리와 같은 `EnemyBehaviorData`라서 일반 적 패턴을 그대로 재사용한다.
- **패턴 체이닝은 UniTask `await` 순차 실행이다.** 취소 토큰이 이어져 있어 사망 시 체인 전체가 즉시 중단된다.
  대상이 닿지 않는 패턴은 건너뛴다 → 멀리 있으면 범위 공격, 가까이 있으면 근접 패턴이 나간다.
- **보스는 경직하지 않는다 (`staggerable = false`).** 경직하면 패턴 체인이 끊겨 보스가 플레이어 연타에 무력해진다.
  피격 이벤트는 그대로 발행되어 페이즈 판정에만 쓰인다.
- **페이즈 전환 연출은 기존 시스템을 재사용했다.** `EventBus.OnBossPhaseChanged`를 `HitStopManager`(0.25s)와
  `CameraShakeManager`(힘 0.15)가 구독한다 — 매니저는 보스를 모르고 이벤트만 안다.
  측정: 전환 순간 `timeScale = 0`, 카메라 변위 0.20 m (일반 타격 0.067 m의 약 3배).
- **충격파는 `AreaBehaviorData`로 만들었다.** 선딜 동안 바닥 범위 원이 차오르고, 타격 순간 `HitLanded`를 발행해 쉐이크/히트스탑을 재사용한다.
- **구현한 것:** 2페이즈 (HP 50% 이하에서 충격파 추가 + 이동 속도 ×1.3). 데이터 기반이라 비용이 거의 들지 않아 같이 했다.
- **씬 배치:** 잡몹 5(근접 3 + 원거리 2)를 방 중간에, 보스를 북쪽 끝에 두어 "전진 → 잡몹 → 보스" 흐름을 만들었다.
  `SpawnType.Boss`와 `BossSpawn` 마커를 추가했다.

## 2026-11-17 — Phase 3 Step 6: 전투 통합 검증

- **검증을 "직접 호출"이 아니라 "실제 경로"로 했다.** 적에게 `TakeDamage`를 직접 부르는 테스트는 통과했지만,
  플레이어의 실제 공격(`HitBox` 트리거)은 한 번도 적을 맞히지 못하고 있었다. 단위 호출만으로는 못 잡는 통합 버그였다.
- **`HitBox`가 `Rigidbody`를 요구하도록 했다 (`RequireComponent` + `Reset()`에서 kinematic).**
  적 콜라이더에 붙이면 100마리에 붙어야 하지만, 공격 판정은 플레이어 쪽 한 곳이라 그쪽에 둔다.
- **원인 의심을 하나씩 배제했다.** 직전에 바꾼 플레이어 레이어 변경이 범인인지 되돌려 확인했고, 아니라는 것을 확인한 뒤 기록했다.
- 측정 도구의 시동 비용(첫 프레임 2.9초)은 게임 성능과 분리해서 제외하고, 제외 사유를 문서에 적었다.

## 2026-11-17 — Phase 4: 아웃게임 최소 루프 (타이틀 → 던전 → 결과)

- **`SceneLoader`·`GameSession`은 Singleton이 아니라 일반 클래스 + ServiceLocator다.** Singleton은 `GameManager` 하나로 유지된다.
  앱 시작 직전(`RuntimeInitializeOnLoadMethod`, BeforeSceneLoad)에 한 번 등록하므로 어느 씬에서 Play를 눌러도 동작하고,
  씬을 넘어 유지되는 서비스라 씬 소속 서비스와 달리 해제하지 않는다. 모든 전환은 `SceneLoader.Go()`를 거친다.
- **결과 데이터는 `RunResult`(일반 클래스)를 `GameSession`에 담아 씬 사이로 넘긴다.** 결과 씬은 값을 읽기만 한다.
- **결과 화면 항목: 클리어 시간 / 처치한 적 수 / 세션 누적 사망 횟수.**
  사망 시 바로 결과 화면으로 가므로 한 판의 사망 횟수는 0 또는 1이라 정보량이 없다. 그래서 판 단위 값은 "처치 수"로,
  사망 횟수는 세션 누적으로 보여 준다. 시간은 `Time.unscaledTime` 기준이라 히트스탑에 영향받지 않는다.
- **씬 이름은 `SceneNames` 상수로 관리한다.** 던전 씬은 파일명을 바꾸지 않고(`Dungeon_01`) 상수만 연결해 불필요한 변경을 피했다.
- **UI 문구는 영문이다.** TMP 기본 폰트에 한글 글리프가 없어 □로 깨진다. 한글 폰트 에셋은 필요해지면 추가한다.
- **플레이어 사망만 연결하고 피격 리액션(`OnHit`)은 연결하지 않았다.** `OnPlayerDied` → `SetDead`는 결과 화면 흐름에 필수다.
  피격 경직은 잡몹 여럿이 계속 때리는 환경에서 모든 피격마다 걸면 플레이어가 공격 도중 계속 끊긴다. 데미지 임계값 또는
  슈퍼아머 설계가 먼저 필요해 후순위로 두었다.
- **루프 통합 검증이 두 가지 버그를 잡았다.**
  1. `HitStopManager`/`CameraShakeManager`가 `GameManager`(DontDestroyOnLoad)와 같은 오브젝트에 붙어 씬을 넘어 살아남았다.
     재진입 후 이미 파괴된 `ImpulseSource`를 참조해 NRE가 났고, 이 예외가 같은 이벤트의 뒤쪽 구독자까지 끊는다.
     → 씬 소속 매니저를 `CombatFeedback` 오브젝트로 분리했다. (영속 오브젝트에 씬 종속 컴포넌트를 두지 않는다.)
  2. 보스를 잡았는데 처치 수가 0으로 표시됐다. `OnBossDefeated`가 보스의 `OnEnemyDied` 처리 도중 발행되어
     집계 전에 결과가 만들어졌다. → 결과 생성을 한 프레임 뒤로 미뤘다 (종료 시각은 이벤트 시점에 고정).
