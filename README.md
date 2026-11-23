# ⚔️ Unity Action RPG

> 3인칭 싱글 액션 RPG — 던전을 돌파해 보스를 처치하면 클리어  
> Unity 6 · URP · C#

---

## 📌 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 3인칭 액션 RPG (TPS 시점) |
| 엔진 | Unity 6, URP |
| 개발 기간 | - |
| 개발 규모 | 1인 개발 |

---

## 🎮 게임 컨셉

```
타이틀 → 던전 (인게임) → 결과 화면 (성공/실패) → 타이틀 복귀
```

- 스킬 1~3의 **순서/조합**이 전투의 핵심
- 히트스탑 + 카메라 쉐이크로 타격감 보조
- 확장 예정: 패링 시스템

---

## 🏗️ 아키텍처

```
Assets/
├── Core/         # GameManager, EventBus, ServiceLocator
├── Player/       # FSM, InputHandler, SkillHandler
├── Enemy/        # AI FSM, NavMesh, Strategy 행동, 보스 페이즈
├── Combat/       # Command Pattern 스킬, HitBox, ObjectPool
├── Flow/         # SceneLoader, GameSession, 던전 진행 관리
├── UI/           # 타이틀/결과 화면, HP/쿨타임 UI, SafeAreaFitter
└── Data/         # ScriptableObject 스탯 데이터
```

### 적용 디자인 패턴

| 패턴 | 적용 위치 | 이유 |
|---|---|---|
| Singleton | GameManager (1개) | 전역 접근이 꼭 필요한 최소 시스템만 제한 적용 |
| Service Locator | SceneLoader, GameSession, PlayerHealth, 이펙트/투사체 풀 | Singleton 남용 방지, 씬 소속 서비스는 OnDestroy에서 해제 |
| Observer (EventBus) | HP 변경, 사망, 스킬 쿨타임 등 | 컴포넌트 간 직접 참조 제거 |
| Command | 기본공격, 스킬 1~3 | 스킬 확장/조합 시 코드 수정 최소화 |
| State (FSM) | 플레이어, 적 AI | 상태별 행동 명확히 분리 |
| Strategy | 적 공격 행동 (근접/원거리/범위/보스 패턴) | SO로 주입해 코드 수정 없이 적 종류 교체, 선딜→타격→후딜 골격 고정 |
| Object Pool | 투사체, 이펙트 | Instantiate/Destroy 제거 |

---

## ⚙️ 주요 기술 구현

### New Input System
- `PlayerInputActions.inputactions` 기반 키보드 + 게임패드 동시 바인딩
- 모바일 가상 조이스틱 추가 시 **코드 수정 없이** 동작하도록 설계

### Command Pattern 스킬 시스템
```csharp
public interface ISkillCommand
{
    void Execute();
    bool CanExecute();
}
```
- 스킬 A → B 순서에 따라 효과 차이 발생 (스킬 시너지)
- 스킬 추가/수정 시 기존 코드 변경 없이 새 Command 클래스만 추가

### EventBus (Observer Pattern)
```csharp
// 직접 참조 없이 이벤트 기반 통신
EventBus.OnPlayerHpChanged += HPBarUI.UpdateHP;
```
- 게임 로직에서 `FindObjectOfType`/`GameObject.Find` 제거 (Singleton의 지연 조회만 예외)
- OnEnable/OnDisable에서 구독/해제로 메모리 누수 방지

### Object Pool
- 제네릭 `ObjectPool<T>` 구현
- 투사체, 이펙트 Instantiate/Destroy 완전 제거

### UniTask 비동기 처리
- 스킬 쿨타임 — `UniTask.Delay` 기반 (Coroutine 대비 GC 최소화)
- 스킬 시퀀스 — async/await 체이닝

### 히트스탑 + 카메라 쉐이크
- 히트스탑: 공격 히트 시 `Time.timeScale` 순간 정지 → UniTask 복귀
- 카메라 쉐이크: Cinemachine Impulse Source 활용

### ScriptableObject 스탯 설계
- `CharacterStatData` SO — 플레이어/적 종류별 인스턴스 분리
- Addressables 연동 고려한 구조

### 모바일 대응
- Canvas Scale With Screen Size (1920×1080 기준)
- SafeAreaFitter 컴포넌트 (노치/펀치홀 대응)
- IL2CPP + .NET Standard 2.1 + ASTC 텍스처 압축

---

## 🗓️ 개발 진행 상황

| Phase | 내용 | 상태 |
|---|---|---|
| Phase 1 | 코어 아키텍처 (Singleton, EventBus, FSM, SO) | ✅ 완료 |
| Phase 2 | 전투/스킬 시스템 (Command, ObjectPool, UniTask) | ✅ 완료 |
| Phase 3 | 적 AI + 던전 (NavMesh, Strategy 행동, 보스 2페이즈, 탐지 비용 측정) | ✅ 완료 |
| Phase 4 | 아웃게임 최소 루프 (타이틀 → 던전 → 결과, SceneLoader) | ✅ 완료 |
| Phase 5 | 최적화·마무리 (Profiler 병목 1건, Pool 수치, 문서) | 🔄 예정 |

설계 결정은 [`docs/decisions.md`](docs/decisions.md), 측정 수치는 [`docs/measurements.md`](docs/measurements.md)에 기록한다.

---

## 🔧 개발 환경

- **Unity 6**, URP
- **Cinemachine 3.x**
- **New Input System**
- **UniTask**
- **NavMesh (AI Navigation)**

---

## 📈 향후 추가 예정

- [x] 적 탐지 비용 측정 — 병목이 아니라 QuadTree 미채택 (100마리에서 0.06 ms, [근거](docs/measurements.md))
- [ ] Profiler 병목 1건 측정 후 개선, Object Pool Before/After 수치 정리
- [ ] 패링 시스템 (v2.0 — 적 공격 선딜 구간에 판정을 붙이도록 설계해 둠)
