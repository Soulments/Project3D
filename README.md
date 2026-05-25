# ⚔️ Unity Action RPG

> 3인칭 싱글 액션 RPG — 던전을 돌파해 보스를 처치하면 클리어  
> Unity 6 · URP · C#

---

## 📌 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 3인칭 액션 RPG (TPS 시점) |
| 엔진 | Unity 6, URP |
| 개발 기간 | 8주 |
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
├── Enemy/        # AI FSM, NavMesh, QuadTree (Phase 3)
├── Combat/       # Command Pattern 스킬, HitBox, ObjectPool
├── UI/           # MVP 패턴 (Phase 4), SafeAreaFitter
└── Data/         # ScriptableObject 스탯 데이터
```

### 적용 디자인 패턴

| 패턴 | 적용 위치 | 이유 |
|---|---|---|
| Singleton | GameManager, AudioManager (최대 2~3개) | 전역 접근 필요한 최소 시스템만 제한 적용 |
| Service Locator | UIManager, SceneLoader, SaveSystem | Singleton 남용 방지, 의존성 역전 |
| Observer (EventBus) | HP 변경, 사망, 스킬 쿨타임 등 | 컴포넌트 간 직접 참조 제거 |
| Command | 기본공격, 스킬 1~3 | 스킬 확장/조합 시 코드 수정 최소화 |
| State (FSM) | 플레이어, 적 AI | 상태별 행동 명확히 분리 |
| Strategy | 적 AI 행동 (Phase 3) | 적 종류별 AI 교체 가능 구조 |
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
- `FindObjectOfType`, `GameObject.Find` 완전 제거
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
| Phase 3 | 적 AI + 던전 (NavMesh, QuadTree, 보스) | 🔄 진행 중 |
| Phase 4 | UI / 씬 흐름 (MVP 리팩토링, 씬 3개) | 🔄 예정 |
| Phase 5 | 최적화 (LOD, GPU Instancing, Before/After 수치화) | 🔄 예정 |

---

## 🔧 개발 환경

- **Unity 6**, URP
- **Cinemachine 3.x**
- **New Input System**
- **UniTask**
- **ProBuilder** (Phase 3~)

---

## 📈 향후 추가 예정

- [ ] QuadTree 기반 적 탐지 최적화 (Before/After 수치화)
- [ ] LOD Group + GPU Instancing (Draw Call Before/After 수치화)
- [ ] 패링 시스템
- [ ] Photon Fusion 2 멀티플레이어
