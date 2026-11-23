# Project3D — Unity 액션 RPG 포트폴리오

3인칭 싱글 액션 RPG. 던전을 돌파해 보스를 처치하면 클리어. Unity 6000.3.13f1 · URP · C#.
계획과 체크리스트는 Notion "Unity 액션 RPG 포트폴리오 개발 계획 (개선판)"이 기준이다.

## 현재 진행 상태 (2026-11-23)

- Phase 1·2·3·4 구현 완료. 남은 것은 Phase 5(최적화·문서) — Profiler 병목 1건 개선, Pool Before/After 수치, README 다이어그램, PDF.
- 플레이 루프: `TitleScene` → `Dungeon_01`(잡몹 5 + 보스) → `ResultScene` → 타이틀. Build Settings 순서 그대로.
- 아직 없는 것: 스킬 1~3의 서로 다른 효과/콤보 보너스(지금은 같은 공격 트리거 + 로그), 플레이어 피격 리액션,
  AudioManager/SaveSystem, 한글 폰트(UI는 영문), 플레이 영상. `GameScene.unity`는 `Dungeon_01`에 합쳐져 더 이상 쓰지 않는다.
- 2026-11-23에 Phase 3~4 작업(`feature/enemy-behavior-strategy` ~ `feature/outgame-loop`)을 `develop`에 병합했다. 원격 푸시는 아직 하지 않았다.

## 구조

```
Assets/Scripts/
  Core/    GameManager(Singleton 1개), EventBus, ServiceLocator, ObjectPool, HitStop/CameraShake
  Player/  PlayerController(FSM), PlayerHealth, AnimatorMoveRelay(Root Motion)
  Combat/  Command 패턴 스킬, SkillHandler, HitBox(+kinematic Rigidbody 필수), HitEffectPool
  Enemy/   EnemyController(FSM) · Behaviors/(Strategy SO) · Boss/ · ProjectilePool · EnemyTelegraph
  Flow/    SceneLoader, GameSession, RunResult, DungeonRunController, SceneNames
  UI/      HPBarUI, SkillCooldownUI, TitleUI, ResultUI, SafeAreaFitter
  Data/    CharacterStatData, EnemyAIData (ScriptableObject)
  Debug/   EnemyDetectionBenchmark (에디터/개발 빌드 전용)
docs/      decisions.md (설계 결정), measurements.md (측정 기록)
```

## 코드 규칙

- 컴포넌트 간 통신은 `EventBus`. `OnEnable`에서 구독, `OnDisable`에서 반드시 해제.
- 전역 서비스는 `ServiceLocator`. **씬 소속 서비스는 `Awake`에서 `Register`, `OnDestroy`에서 `Unregister`.**
  씬을 넘어 유지되는 서비스(`SceneLoader`, `GameSession`)는 `RuntimeInitializeOnLoadMethod`로 한 번 등록하고 해제하지 않는다.
- **씬 종속 컴포넌트를 `DontDestroyOnLoad` 오브젝트(GameManager)에 붙이지 않는다** (씬 재진입 시 죽은 참조 NRE).
- 게임 로직에서 `FindObjectOfType`/`GameObject.Find` 금지. 씬 전환은 `SceneLoader.Go()`만 사용 (`SceneManager.LoadScene` 직접 호출 금지).
- 수치(거리, 시간, 배율)는 SO로 주입. 하드코딩 금지.
- Unity 오브젝트에는 `?.`를 쓰지 않는다 (fake-null 통과). `if (x != null)`.
- 비동기는 UniTask. 취소 토큰을 이어서 사망/파괴 시 중단되게 한다.
- 주석·문서는 한국어, 커밋 메시지는 영어 (`<대상> - <요약>` 형식 + 본문에 이유).
- 기존 파일은 UTF-8 BOM + CRLF다. 스크립트로 고칠 때 줄바꿈/BOM을 유지한다.

## Unity CLI로 에디터 다루기

Unity Hub의 CLI(`unity.exe`, Hub의 `resources/cli`)와 Pipeline 패키지(`com.unity.pipeline`)로 열려 있는 에디터를 제어한다.
에디터가 켜져 있어야 하고 `unity status`에 포트가 보여야 한다.

- 명령: `unity command "scene list_open"`, `"console get"`, `"editor.playmode start"`, `"script eval_file" --file x.cs` 등.
  명령 이름은 공백이 있으면 따옴표로 묶고, 인자는 `--key value`.
- `script eval_file`: `using` 문을 쓸 수 없다 (전체 경로로 쓸 것). UniTask `.Forget()`은 확장 메서드가 안 먹어서
  `Cysharp.Threading.Tasks.UniTaskExtensions.Forget(...)`로 호출. 파일은 UTF-8로 저장.
- Play 모드 중에는 `scene open` 불가. 스크립트 수정 후에는 `script recompile` → `recompile_status`로 완료 확인.
- 마우스 입력 시뮬레이션은 `Attack` 액션을 발동시키지 못했다. 공격은 `SkillHandler.TryExecute`를 리플렉션으로 호출해 검증했다.
- 에디터를 포커스하지 않은 상태에서 테스트하려면 `editor.playmode set_run_in_background --enable true` (끝나면 false로 복구).
- 사용자가 에디터를 직접 조작 중일 수 있다. Play 모드를 종료하기 전에 상태를 확인하고, 저장되지 않은 씬을 덮어쓰지 않는다.
