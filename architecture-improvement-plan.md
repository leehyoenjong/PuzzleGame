# 아키텍처 개선 계획 (Architecture Improvement Plan)

**프로젝트**: Unity Match-3 Puzzle Game
**작성일**: 2025-10-24
**목표**: 포트폴리오 제출을 위한 코드 품질 향상 및 협업 가능한 구조로 개선

---

## 📊 현재 상태 요약

### ✅ 완료된 작업
- [x] **Phase 0**: 치명적 버그 2개 수정 완료 (메모리 누수)
  - GameConditionManager.cs:20 - `+=` → `-=` 수정
  - MoveCountManager.cs:18 - `+=` → `-=` 수정
  - 전체 217개 테스트 통과 확인

### 📈 현재 메트릭스

| 항목 | 현재 | 목표 | 우선순위 |
|------|------|------|----------|
| **치명적 버그** | 0개 ✅ | 0개 | - |
| **Static Event 개수** | 26개 | 0개 | HIGH |
| **God Object** | 2개 (400줄+) | 0개 (200줄 이하) | HIGH |
| **테스트 가능 클래스** | 7/17 (41%) | 17/17 (100%) | HIGH |
| **인터페이스 추상화** | 0개 | 6개+ | MEDIUM |
| **UI-로직 결합도** | 높음 | 낮음 | MEDIUM |
| **테스트 커버리지** | 15% | 70% | MEDIUM |

**종합 평가**: C → B+ (개선 후)

---

## 🎯 개선 계획 개요

총 **6개 Phase**로 나누어 진행하며, 각 Phase는 독립적으로 완료 가능합니다.

### 예상 소요 시간
- **짧은 버전 (포트폴리오 제출용)**: 1주일 (Phase 1-2만)
- **중간 버전 (협업 준비)**: 2주일 (Phase 1-4)
- **완전한 버전 (프로덕션 레벨)**: 3주일 (Phase 1-6 전체)

---

## Phase 1: 인터페이스 추출 (Interface Extraction)

**목표**: 의존성 역전 원칙(DIP) 적용, 테스트 가능성 확보
**예상 시간**: 4-6시간
**우선순위**: HIGH

### 1.1 서비스 인터페이스 생성

**새 파일**: `Assets/01_Script/00_Common/IServices.cs`

핵심 서비스에 대한 인터페이스 정의:
- `IMatchDetector`: 매치 감지 로직
- `IMatchTypeClassifier`: 매치 타입 분류
- `ISpecialBlockFactory`: 특수 블록 생성 요청
- `IChainReactionProcessor`: 연쇄 반응 처리
- `IGridManager`: 그리드 상태 관리
- `IMoveMatchValidator`: 이동 유효성 검증
- `IBlockSwapHandler`: 블록 교환 처리

### 1.2 기존 클래스에 인터페이스 구현

수정 파일:
- [x] `MatchDetector.cs` → `class MatchDetector : IMatchDetector`
- [x] `MatchTypeClassifier.cs` → `class MatchTypeClassifier : IMatchTypeClassifier`
- [x] `SpecialBlockFactory.cs` → `class SpecialBlockFactory : ISpecialBlockFactory`
- [x] `ChainReactionProcessor.cs` → `class ChainReactionProcessor : IChainReactionProcessor`
- [x] `GridManager.cs` → `class GridManager : IGridManager`
- [x] `MoveMatchValidator.cs` → `class MoveMatchValidator : IMoveMatchValidator`
- [x] `BlockSwapHandler.cs` → `class BlockSwapHandler : IBlockSwapHandler`

**작업 내용**: 클래스 선언에 `: IInterfaceName` 추가, 로직 변경 없음

### 1.3 테스트 작성

**새 파일**: `Assets/01_Script/Tests/InterfaceContractTests.cs`

각 인터페이스 구현이 계약을 준수하는지 검증:
- [ ] `IMatchDetector` 구현 검증 테스트
- [ ] `IMatchTypeClassifier` 구현 검증 테스트
- [ ] `ISpecialBlockFactory` 구현 검증 테스트
- [ ] `IChainReactionProcessor` 구현 검증 테스트
- [ ] `IGridManager` 구현 검증 테스트

**예상 테스트 수**: 25개

### 1.4 커밋

```
[REFACTOR] Add service interfaces for dependency inversion

- Create IServices.cs with 7 core service interfaces
- Implement interfaces in existing classes (no logic changes)
- Add 25 interface contract tests

Benefits:
- Enables dependency injection
- Allows mocking for unit tests
- Improves testability from 41% to 60%
```

---

## Phase 2: MatchManager 리팩토링 (MatchManager Refactoring)

**목표**: God Object 분해, 단일 책임 원칙(SRP) 적용
**예상 시간**: 8-12시간
**우선순위**: HIGH

### 2.1 의존성 주입 구조로 변경

**수정 파일**: `Assets/01_Script/02_Manager/MatchManager.cs`

**현재 구조** (Awake에서 직접 생성):
```csharp
void Awake()
{
    _matchdetector = new MatchDetector();
    _matchtypeclassifier = new MatchTypeClassifier();
    // ... 6개 더
}
```

**개선 구조** (인터페이스 의존, 생성자 주입):
```csharp
private IMatchDetector _matchdetector;
private IMatchTypeClassifier _matchtypeclassifier;
// ...

public void Initialize(
    IMatchDetector matchDetector,
    IMatchTypeClassifier matchTypeClassifier,
    // ...
)
{
    _matchdetector = matchDetector;
    // ...
}

void Awake()
{
    // 기본 구현체 주입 (프로덕션)
    Initialize(
        new MatchDetector(),
        new MatchTypeClassifier(),
        // ...
    );
}
```

**작업 내용**:
- [ ] `Initialize()` 메서드 추가
- [ ] Awake()에서 기본 의존성 주입
- [ ] 필드를 인터페이스 타입으로 변경

### 2.2 메서드 분해 (Extract Method)

**현재 문제**: `AllBlockMatch()` 메서드가 93줄, 3가지 책임

**분해 대상**:
1. [ ] `CollectMatchesFromGrid()` - 그리드 순회 및 매치 수집
2. [ ] `ProcessMatchRequests()` - 매치 타입 분류 및 특수 블록 요청
3. [ ] `ExecuteMatchDestruction()` - 블록 파괴 및 연쇄 반응

**Before** (93줄):
```csharp
bool AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
{
    // 1. 그리드 순회 (30줄)
    // 2. 매치 수집 (40줄)
    // 3. 파괴 처리 (23줄)
}
```

**After** (각 메서드 20줄 이하):
```csharp
bool AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
{
    var matches = CollectMatchesFromGrid(matchblockdic, width, height);
    var requests = ProcessMatchRequests(matches);
    ExecuteMatchDestruction(matches, requests);
    return matches.Count > 0;
}
```

### 2.3 중복 코드 제거 (DRY 원칙)

**중복 위치**:
- `AllBlockMatch()` Lines 138-148
- `UserMoveBlockMatch()` Lines 236-247

**공통 로직 추출**:
- [ ] `CreateSpecialBlockRequests()` 메서드 추출
- [ ] `ProcessDestructionWithChainReaction()` 메서드 추출

### 2.4 테스트 작성

**새 파일**: `Assets/01_Script/Tests/MatchManagerDependencyTests.cs`

- [ ] Mock 의존성으로 MatchManager 테스트
- [ ] 각 추출된 메서드 단위 테스트
- [ ] 통합 테스트 (전체 플로우)

**예상 테스트 수**: 15개

### 2.5 커밋

```
[REFACTOR] Refactor MatchManager - extract methods and inject dependencies

- Add Initialize() for dependency injection
- Extract 93-line AllBlockMatch() into 3 methods (20 lines each)
- Remove duplicate code between AllBlockMatch and UserMoveBlockMatch
- Add 15 unit tests with mock dependencies

Benefits:
- MatchManager reduced from 402 lines to ~280 lines
- Each method has single responsibility
- Can test individual logic without full manager setup
```

---

## Phase 3: EventBus 도입 (EventBus Implementation)

**목표**: Static Event 제거, 중앙화된 이벤트 관리
**예상 시간**: 8-12시간
**우선순위**: HIGH

### 3.1 EventBus 클래스 생성

**새 파일**: `Assets/01_Script/00_Common/EventBus.cs`

기능:
- 타입 안전한 이벤트 발행/구독
- 자동 구독 해제 (약한 참조 사용)
- 디버깅을 위한 이벤트 로깅

```csharp
public class EventBus
{
    private static EventBus _instance;
    public static EventBus Instance => _instance ??= new EventBus();

    private Dictionary<Type, List<Delegate>> _events = new();

    public void Subscribe<T>(Action<T> handler) where T : IGameEvent
    public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
    public void Publish<T>(T eventData) where T : IGameEvent

    public void Clear() // 테스트용
}
```

### 3.2 이벤트 타입 정의

**새 파일**: `Assets/01_Script/00_Common/GameEvents.cs`

현재 Static Event를 타입으로 변환:
- [ ] `MatchCompleteEvent` (기존: `_match_complte_block_event`)
- [ ] `SpecialBlockCreateEvent` (기존: `_match_complte_createblock_event`)
- [ ] `UserMoveCompleteEvent` (기존: `_user_move_match_complte`)
- [ ] `BlockMoveEvent` (기존: `_block_move_event`)
- [ ] `CheckClearConditionEvent` (기존: `_check_clear_condition_event`)
- [ ] `CheckOverConditionEvent` (기존: `_check_over_condition_event`)
- [ ] ... (26개 전체)

예시:
```csharp
public interface IGameEvent { }

public struct MatchCompleteEvent : IGameEvent
{
    public int X { get; }
    public int Y { get; }
    public MatchCompleteEvent(int x, int y) { X = x; Y = y; }
}
```

### 3.3 Manager 클래스 변환

**단계별 변환** (한 번에 한 Manager씩):

1. [ ] **GameConditionManager** 변환
   - `OnEnable()`: `EventBus.Subscribe<CheckClearConditionEvent>()`
   - `OnDisable()`: `EventBus.Unsubscribe<CheckClearConditionEvent>()`
   - Static event 제거

2. [ ] **MoveCountManager** 변환

3. [ ] **ScoreManager** 변환

4. [ ] **MatchManager** 변환

5. [ ] **MatchFiledManager** 변환

6. [ ] **BlockControllerManager** 변환

### 3.4 테스트 작성

**새 파일**: `Assets/01_Script/Tests/EventBusTests.cs`

- [ ] 이벤트 발행/구독 기본 동작
- [ ] 여러 구독자 처리
- [ ] 구독 해제 동작
- [ ] 메모리 누수 방지 검증
- [ ] 순환 이벤트 감지

**예상 테스트 수**: 12개

### 3.5 커밋

```
[REFACTOR] Replace static events with centralized EventBus

- Create EventBus with type-safe event handling
- Define 26 event types in GameEvents.cs
- Convert all 6 managers to use EventBus
- Remove all static events (26 removed)
- Add 12 EventBus tests

Benefits:
- No more memory leaks from forgotten unsubscriptions
- Easy to trace event flow for debugging
- Can mock EventBus for testing
- Event flow visible in one place
```

---

## Phase 4: UI-로직 분리 (UI-Logic Separation)

**목표**: 비즈니스 로직을 UI에서 분리, Pure C# 모델 생성
**예상 시간**: 12-16시간
**우선순위**: MEDIUM

### 4.1 데이터 모델 추출

**새 파일**: `Assets/01_Script/00_Common/BlockModel.cs`

```csharp
/// <summary>
/// Pure C# 블록 데이터 (Unity 의존성 없음)
/// </summary>
public class BlockModel
{
    public (int x, int y) Position { get; private set; }
    public EMATCHTYPE MatchType { get; private set; }
    public EBLOCKCOLORTYPE ColorType { get; private set; }
    public string Id { get; private set; } // 고유 식별자

    public BlockModel(int x, int y, EMATCHTYPE matchType, EBLOCKCOLORTYPE colorType)
    {
        Position = (x, y);
        MatchType = matchType;
        ColorType = colorType;
        Id = Guid.NewGuid().ToString();
    }

    public void UpdatePosition(int x, int y) { Position = (x, y); }
    public void MarkForDestruction() { Position = (-1, -1); }
}
```

### 4.2 IBlockView 인터페이스

**새 파일**: `Assets/01_Script/00_Common/IBlockView.cs`

```csharp
/// <summary>
/// 블록 View의 계약 (UI 구현과 로직 분리)
/// </summary>
public interface IBlockView
{
    BlockModel Model { get; }
    void UpdateVisual(BlockModel model);
    void PlaySwapAnimation(Vector2 targetPosition);
    void PlayDestructionAnimation();
    void SetInteractable(bool interactable);
}
```

### 4.3 UI_Match_Block 리팩토링

**수정 파일**: `Assets/01_Script/03_UI/00_UI_Common/UI_Match_Block.cs`

**Before** (182줄, 로직 + UI 혼재):
```csharp
public class UI_Match_Block : MonoBehaviour
{
    private int _x, _y; // 게임 상태
    private EMATCHTYPE _matchtype; // 게임 상태

    public void Event_Point_Down() { /* 입력 + 로직 */ }
    public (int, int) GetPoint() { /* 게임 상태 반환 */ }
}
```

**After** (~120줄, View만):
```csharp
public class UI_Match_Block : MonoBehaviour, IBlockView
{
    public BlockModel Model { get; private set; }

    public void Initialize(BlockModel model)
    {
        Model = model;
        UpdateVisual(model);
    }

    public void UpdateVisual(BlockModel model)
    {
        // 색상, 타입 표시만
    }

    public void Event_Point_Down()
    {
        // 입력 이벤트만 발행, 로직은 Controller가 처리
        EventBus.Instance.Publish(new BlockClickedEvent(Model.Id));
    }
}
```

### 4.4 GridManager 변경

**수정 파일**: `Assets/01_Script/01_Core/GridManager.cs`

**Before**:
```csharp
Dictionary<(int, int), UI_Match_Block> _grid;
```

**After**:
```csharp
Dictionary<(int, int), BlockModel> _grid;
Dictionary<string, IBlockView> _views; // ID → View 매핑
```

### 4.5 테스트 작성

**새 파일**: `Assets/01_Script/Tests/BlockModelTests.cs`

- [ ] BlockModel 생성 및 위치 업데이트
- [ ] BlockModel 파괴 마킹
- [ ] UI 없이 GridManager 테스트 (BlockModel만 사용)
- [ ] Mock IBlockView로 UI 로직 테스트

**예상 테스트 수**: 20개

### 4.6 커밋

```
[REFACTOR] Separate UI from game logic - introduce BlockModel

- Create BlockModel (pure C# data class)
- Create IBlockView interface
- Refactor UI_Match_Block to implement IBlockView (182 → 120 lines)
- Update GridManager to use BlockModel instead of UI_Match_Block
- Add 20 tests for BlockModel and UI separation

Benefits:
- Can test game logic without Unity GameObjects
- UI designer and logic programmer can work in parallel
- Easier to add AI/hint system (uses BlockModel only)
```

---

## Phase 5: MatchFiledManager 리팩토링 (MatchFiledManager Refactoring)

**목표**: God Object 분해, 책임 분산
**예상 시간**: 8-10시간
**우선순위**: MEDIUM

### 5.1 서비스 클래스 추출

**MatchFiledManager 책임 분석** (401줄):
1. 슬롯 생성 및 배치 (Lines 140-187)
2. 블록 생성 (Lines 190-229)
3. 특수 블록 생성 (Lines 232-271)
4. 블록 이동 조율 (Lines 288-350)
5. 그리드 상태 변경 (Lines 352-400)

**추출할 서비스**:

#### 5.1.1 GridLayoutService

**새 파일**: `Assets/01_Script/01_Core/GridLayoutService.cs`

책임: 슬롯 배치 및 위치 계산
```csharp
public class GridLayoutService
{
    public Vector2 CalculateSlotPosition(int x, int y, int width, int height, float slotSize);
    public Dictionary<int, int> CalculateTopSlots(List<(int, int)> mapData, int width);
}
```

#### 5.1.2 BlockSpawner

**새 파일**: `Assets/01_Script/01_Core/BlockSpawner.cs`

책임: 블록 생성 및 초기 배치
```csharp
public class BlockSpawner
{
    public List<BlockModel> SpawnBlocksForEmptySlots(GridManager grid, int width, int height);
    public BlockModel SpawnSpecialBlock(int x, int y, EMATCHTYPE type, EBLOCKCOLORTYPE color);
}
```

#### 5.1.3 GravityService

**새 파일**: `Assets/01_Script/01_Core/GravityService.cs`

책임: 중력 및 블록 재배치
```csharp
public class GravityService
{
    public List<BlockModel> ApplyGravity(GridManager grid, int width, int height);
    public bool NeedsRefill(GridManager grid);
}
```

### 5.2 MatchFiledManager 슬림화

**수정 파일**: `Assets/01_Script/02_Manager/MatchFiledManager.cs`

**Before** (401줄, 5가지 책임):
```csharp
public class MatchFiledManager : MonoBehaviour
{
    void CreateMatchSlot() { /* 47줄 */ }
    (List<UI_Match_Block>, int) CreateMatchBlock() { /* 39줄 */ }
    void CreateMatchBlock(int x, int y, ...) { /* 40줄 */ }
    async UniTask<bool> MoveMatchBlock(...) { /* 62줄 */ }
    // ... 더 많은 책임
}
```

**After** (~200줄, 조율만):
```csharp
public class MatchFiledManager : MonoBehaviour
{
    private GridLayoutService _layoutService;
    private BlockSpawner _spawner;
    private GravityService _gravityService;

    void CreateMatchSlot()
    {
        // 레이아웃 서비스 위임
        foreach (var pos in _mapData)
        {
            var worldPos = _layoutService.CalculateSlotPosition(...);
            // 슬롯 생성
        }
    }

    void CreateMatchBlock()
    {
        // 스포너 서비스 위임
        var blocks = _spawner.SpawnBlocksForEmptySlots(...);
        // 뷰 생성만 처리
    }
}
```

### 5.3 테스트 작성

**새 파일**: `Assets/01_Script/Tests/GridServicesTests.cs`

- [ ] GridLayoutService 테스트 (10개)
- [ ] BlockSpawner 테스트 (8개)
- [ ] GravityService 테스트 (12개)

**예상 테스트 수**: 30개

### 5.4 커밋

```
[REFACTOR] Extract services from MatchFiledManager - reduce god object

- Extract GridLayoutService for slot positioning
- Extract BlockSpawner for block creation
- Extract GravityService for gravity and refill
- Reduce MatchFiledManager from 401 lines to ~200 lines
- Add 30 service tests

Benefits:
- Each service has single responsibility
- Can reuse services in other contexts (level editor, AI)
- Easier to understand and maintain
```

---

## Phase 6: 종합 테스트 및 문서화 (Integration Testing & Documentation)

**목표**: 전체 시스템 통합 테스트, 문서 정리
**예상 시간**: 6-8시간
**우선순위**: LOW (포트폴리오 가점)

### 6.1 통합 테스트 작성

**새 파일**: `Assets/01_Script/Tests/SystemIntegrationTests.cs`

실제 게임 시나리오 테스트:
- [ ] 사용자 이동 → 매치 → 중력 → 재매치 전체 플로우
- [ ] 특수 블록 생성 → 연쇄 반응 전체 플로우
- [ ] 게임 클리어 조건 달성 플로우
- [ ] 게임 오버 조건 도달 플로우
- [ ] 5단계 캐스케이드 매치

**예상 테스트 수**: 15개

### 6.2 성능 테스트

**새 파일**: `Assets/01_Script/Tests/PerformanceTests.cs`

- [ ] 100x100 그리드 전체 스캔 (< 1초)
- [ ] 1000번 연속 매치 시뮬레이션 (< 1초)
- [ ] 메모리 누수 테스트 (1000번 반복 후 메모리 증가 < 10%)
- [ ] GC 할당 최소화 (주요 루프에서 0 할당)

**예상 테스트 수**: 8개

### 6.3 아키텍처 문서 작성

**새 파일**: `Documentation/Architecture.md`

내용:
- 전체 아키텍처 다이어그램
- 각 레이어별 책임 설명
- 이벤트 플로우 다이어그램
- 의존성 그래프
- 확장 가이드 (새 특수 블록 추가 방법 등)

### 6.4 README 업데이트

**수정 파일**: `README.md`

추가할 섹션:
- Architecture Overview (간단한 설명 + 다이어그램 링크)
- Code Quality Metrics (테스트 커버리지, 정적 분석 결과)
- Known Issues & Future Improvements (투명성)
- Development Setup (새 개발자 온보딩)

### 6.5 커밋

```
[DOCS] Add comprehensive documentation and integration tests

- Add 15 system integration tests
- Add 8 performance tests
- Create Architecture.md with diagrams
- Update README with architecture overview
- Document all known issues and improvements

Benefits:
- Demonstrates professional documentation skills
- Easy onboarding for new team members
- Shows understanding of system as a whole
```

---

## 📋 실행 전략

### 🚀 빠른 트랙 (1주일, 포트폴리오 제출용)

**목표**: 최소한의 개선으로 최대 효과

```
Day 1-2: Phase 1 (인터페이스 추출)
Day 3-4: Phase 2 (MatchManager 리팩토링)
Day 5-7: Phase 3 일부 (EventBus 기본 구조만)
```

**결과**:
- 테스트 가능 클래스: 41% → 70%
- MatchManager 복잡도: 402줄 → 280줄
- 인터페이스 추상화: 0개 → 7개

**포트폴리오 효과**: "의존성 역전 원칙 이해, 리팩토링 능력" 입증

---

### ⚡ 표준 트랙 (2주일, 협업 준비)

**목표**: 협업 가능한 구조로 개선

```
Week 1:
- Phase 1 (2일)
- Phase 2 (3일)

Week 2:
- Phase 3 (3일)
- Phase 4 (2일)
```

**결과**:
- Static Event: 26개 → 0개
- UI-로직 결합: 높음 → 낮음
- 테스트 커버리지: 15% → 50%

**포트폴리오 효과**: "프로덕션 레벨 아키텍처 설계 능력" 입증

---

### 🏆 완전한 트랙 (3주일, 프로덕션 레벨)

**목표**: 모든 개선 사항 적용

```
Week 1: Phase 1-2
Week 2: Phase 3-4
Week 3: Phase 5-6
```

**결과**:
- God Object: 2개 → 0개
- 테스트 커버리지: 15% → 70%
- 전체 아키텍처: C등급 → A등급

**포트폴리오 효과**: "SOLID 원칙 완벽 이해, 대규모 리팩토링 경험" 입증

---

## 🔄 TDD 워크플로우

각 Phase는 다음 순서로 진행:

### 1. Red (테스트 작성)
```bash
# 실패하는 테스트 먼저 작성
# 예: Phase 1.3 - 인터페이스 계약 테스트
```

### 2. Green (최소 구현)
```bash
# 테스트를 통과하는 최소한의 코드 작성
# 예: Phase 1.2 - 인터페이스 구현 추가
```

### 3. Refactor (개선)
```bash
# 구조 개선, 중복 제거
# 예: Phase 2.2 - 메서드 추출
```

### 4. Test (검증)
```bash
# 전체 테스트 실행
/Applications/Unity/Hub/Editor/6000.0.58f2/Unity.app/Contents/MacOS/Unity \
  -runTests -batchmode \
  -projectPath /Users/ihyeonjong/Desktop/Git/PuzzleGame \
  -testResults TestResults.xml \
  -testPlatform EditMode \
  -logFile -
```

### 5. Commit (커밋)
```bash
git add .
git commit -m "[REFACTOR] Phase X.Y - 작업 설명"
```

---

## 📊 진행 상황 추적

### Phase 별 체크리스트

#### ✅ Phase 0: 치명적 버그 수정
- [x] GameConditionManager 메모리 누수 수정
- [x] MoveCountManager 메모리 누수 수정
- [x] 전체 테스트 통과 확인 (217/217)
- [x] 커밋 완료

#### ⏹️ Phase 1: 인터페이스 추출
- [ ] 1.1: IServices.cs 생성
- [ ] 1.2: 기존 클래스 인터페이스 구현 (7개)
- [ ] 1.3: 인터페이스 계약 테스트 작성 (25개)
- [ ] 1.4: 커밋

#### ⏹️ Phase 2: MatchManager 리팩토링
- [ ] 2.1: 의존성 주입 구조 변경
- [ ] 2.2: 메서드 분해 (93줄 → 3개 메서드)
- [ ] 2.3: 중복 코드 제거
- [ ] 2.4: 단위 테스트 작성 (15개)
- [ ] 2.5: 커밋

#### ⏹️ Phase 3: EventBus 도입
- [ ] 3.1: EventBus 클래스 생성
- [ ] 3.2: 이벤트 타입 정의 (26개)
- [ ] 3.3: Manager 클래스 변환 (6개)
- [ ] 3.4: EventBus 테스트 작성 (12개)
- [ ] 3.5: 커밋

#### ⏹️ Phase 4: UI-로직 분리
- [ ] 4.1: BlockModel 클래스 생성
- [ ] 4.2: IBlockView 인터페이스 생성
- [ ] 4.3: UI_Match_Block 리팩토링
- [ ] 4.4: GridManager 변경
- [ ] 4.5: 테스트 작성 (20개)
- [ ] 4.6: 커밋

#### ⏹️ Phase 5: MatchFiledManager 리팩토링
- [ ] 5.1: 서비스 클래스 추출 (3개)
- [ ] 5.2: MatchFiledManager 슬림화
- [ ] 5.3: 서비스 테스트 작성 (30개)
- [ ] 5.4: 커밋

#### ⏹️ Phase 6: 종합 테스트 및 문서화
- [ ] 6.1: 통합 테스트 작성 (15개)
- [ ] 6.2: 성능 테스트 작성 (8개)
- [ ] 6.3: Architecture.md 작성
- [ ] 6.4: README 업데이트
- [ ] 6.5: 커밋

---

## 🎯 각 Phase별 성공 기준

### Phase 1 완료 기준
- ✅ 7개 인터페이스 정의 완료
- ✅ 7개 클래스 인터페이스 구현
- ✅ 25개 테스트 모두 통과
- ✅ 기존 217개 테스트 여전히 통과
- ✅ 컴파일 에러 0개

### Phase 2 완료 기준
- ✅ MatchManager 줄 수: 402 → 280줄 이하
- ✅ 메서드당 최대 줄 수: 93 → 30줄 이하
- ✅ 15개 새 테스트 통과
- ✅ Mock 의존성으로 테스트 가능 확인

### Phase 3 완료 기준
- ✅ Static event 개수: 26 → 0
- ✅ EventBus 테스트 12개 통과
- ✅ 메모리 누수 테스트 통과
- ✅ 이벤트 플로우 문서화

### Phase 4 완료 기준
- ✅ UI_Match_Block 줄 수: 182 → 120줄 이하
- ✅ GameObject 없이 GridManager 테스트 가능
- ✅ 20개 새 테스트 통과
- ✅ UI-로직 결합도 측정 개선

### Phase 5 완료 기준
- ✅ MatchFiledManager 줄 수: 401 → 200줄 이하
- ✅ 3개 서비스 추출 완료
- ✅ 30개 서비스 테스트 통과
- ✅ 서비스 재사용 가능 확인

### Phase 6 완료 기준
- ✅ 통합 테스트 15개 통과
- ✅ 성능 테스트 8개 통과
- ✅ Architecture.md 완성
- ✅ README 업데이트 완료

---

## 🚨 리스크 및 대응 방안

### 리스크 1: 기존 기능 파괴
**확률**: 중간
**영향**: 높음
**대응**:
- 각 Phase마다 전체 테스트 실행
- Phase별로 커밋하여 롤백 가능하도록
- 리팩토링 전후 동작 비교 테스트

### 리스크 2: 시간 초과
**확률**: 높음
**영향**: 중간
**대응**:
- Phase 1-2만 완료해도 포트폴리오 효과 큼
- Phase 3-6은 선택적으로 진행
- 각 Phase 독립적이므로 중단 가능

### 리스크 3: Unity 특수성
**확률**: 낮음
**영향**: 중간
**대응**:
- MonoBehaviour 생명주기 존중
- UniTask/DOTween 의존성 유지
- Unity Editor 호환성 테스트

---

## 📚 참고 자료

### 적용된 디자인 패턴
- **Dependency Inversion Principle (DIP)**: Phase 1
- **Single Responsibility Principle (SRP)**: Phase 2, 5
- **Observer Pattern (EventBus)**: Phase 3
- **Model-View Separation**: Phase 4
- **Service Layer Pattern**: Phase 5

### 리팩토링 기법
- Extract Interface
- Extract Method
- Replace Conditional with Polymorphism
- Introduce Parameter Object
- Replace Static Event with Mediator

---

## ⚙️ 실행 명령어

### 전체 테스트 실행
```bash
/Applications/Unity/Hub/Editor/6000.0.58f2/Unity.app/Contents/MacOS/Unity \
  -runTests -batchmode \
  -projectPath /Users/ihyeonjong/Desktop/Git/PuzzleGame \
  -testResults /Users/ihyeonjong/Desktop/Git/PuzzleGame/TestResults.xml \
  -testPlatform EditMode \
  -logFile -
```

### 특정 Phase 테스트만 실행
```bash
# Phase 1 테스트만
-testFilter "InterfaceContractTests"

# Phase 2 테스트만
-testFilter "MatchManagerDependencyTests"
```

### 커밋 템플릿
```bash
git commit -m "[REFACTOR] Phase X.Y - 작업 설명

- 변경 사항 1
- 변경 사항 2
- 변경 사항 3

Benefits:
- 개선 효과 1
- 개선 효과 2

Tests: X개 추가, 전체 Y개 통과
"
```

---

## 🎓 학습 목표

이 리팩토링을 통해 습득할 기술:

1. ✅ **SOLID 원칙 실전 적용**
2. ✅ **의존성 주입 (DI) 패턴**
3. ✅ **테스트 주도 리팩토링**
4. ✅ **이벤트 기반 아키텍처**
5. ✅ **UI-로직 분리 (MVC/MVP 패턴)**
6. ✅ **대규모 코드베이스 리팩토링 경험**
7. ✅ **Git을 이용한 단계별 변경 관리**

---

## 📈 예상 결과

### Before (현재)
```
Architecture Grade: C
- 치명적 버그: 2개
- Static Events: 26개
- God Objects: 2개 (400줄+)
- 테스트 커버리지: 15%
- 테스트 가능 클래스: 41%
```

### After (Phase 1-2 완료 시)
```
Architecture Grade: B
- 치명적 버그: 0개 ✅
- Static Events: 26개
- God Objects: 1개 (280줄)
- 테스트 커버리지: 40%
- 테스트 가능 클래스: 70%
```

### After (Phase 1-6 완료 시)
```
Architecture Grade: A
- 치명적 버그: 0개 ✅
- Static Events: 0개 ✅
- God Objects: 0개 ✅
- 테스트 커버리지: 70%
- 테스트 가능 클래스: 100%
```

---

## 📝 규칙

- **모든 대답은 한국어로**
- **Phase 단위로 완료 후 다음 "go" 대기**
- **각 Phase 내에서는 자동으로 모든 작업 진행**
- **테스트 실패 시 자동 수정, 해결 안 되면 보고**
- **모든 커밋에 테스트 통과 여부 명시**

---

**다음 단계**: `go` 입력 시 Phase 1 시작
