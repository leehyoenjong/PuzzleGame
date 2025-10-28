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

#### 1.3.1 IMatchDetector 테스트 (5개)
- [x] `ShouldDetectHorizontalThreeMatch` - 가로 3매치 감지
- [x] `ShouldDetectVerticalThreeMatch` - 세로 3매치 감지
- [x] `ShouldDetectHorizontalFourMatch` - 가로 4매치 감지
- [x] `ShouldDetectCrossMatch` - 십자형 매치 감지
- [x] `ShouldReturnEmptyListWhenNoMatch` - 매치 없을 때 빈 리스트 반환

#### 1.3.2 IMatchTypeClassifier 테스트 (5개)
- [x] `ShouldClassifyThreeMatchAsThree` - 3매치 분류
- [x] `ShouldClassifyFourMatchAsForeUpDown` - 4매치 상하 분류
- [x] `ShouldClassifyFourMatchAsForeLeftRight` - 4매치 좌우 분류
- [x] `ShouldClassifyFiveMatchAsFive` - 5매치 분류
- [x] `ShouldClassifyCrossMatchAsCrossThree` - 십자형 3매치 분류

#### 1.3.3 ISpecialBlockFactory 테스트 (5개)
- [x] `ShouldCreateRequestForForeUpDown` - 상하 특수블록 요청 생성
- [x] `ShouldCreateRequestForForeLeftRight` - 좌우 특수블록 요청 생성
- [x] `ShouldCreateRequestForFive` - 5매치 특수블록 요청 생성
- [x] `ShouldCreateRequestForCrossThree` - 십자형 특수블록 요청 생성
- [x] `ShouldNotCreateRequestForThreeMatch` - 일반 3매치는 요청 없음

#### 1.3.4 IChainReactionProcessor 테스트 (5개)
- [x] `ShouldProcessForeUpDownChainReaction` - 상하 블록 연쇄반응 처리
- [x] `ShouldProcessForeLeftRightChainReaction` - 좌우 블록 연쇄반응 처리
- [x] `ShouldProcessFiveBlockWithColor` - 5매치 블록 색상 연쇄반응
- [x] `ShouldCombineTwoSpecialBlocks` - 특수블록 2개 조합
- [x] `ShouldReturnEmptyListForNormalBlocks` - 일반 블록은 빈 리스트 반환

#### 1.3.5 IGridManager 테스트 (5개)
- [x] `ShouldGetBlockAtPosition` - 위치로 블록 가져오기
- [x] `ShouldSetBlockAtPosition` - 위치에 블록 설정
- [x] `ShouldRemoveBlockAtPosition` - 위치의 블록 제거
- [x] `ShouldReturnNullForInvalidPosition` - 유효하지 않은 위치 null 반환
- [x] `ShouldCheckIfPositionIsValid` - 위치 유효성 확인

**총 예상 테스트 수**: 25개

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

### 1.5 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 1 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 1: 인터페이스 추출" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase1_Interface_Extraction.md`

**포함 내용**:
- 작업 개요 및 목표
- 생성된 7개 인터페이스 목록 및 코드
- 수정된 파일 목록
- 작성된 테스트 (25개)
- Before/After 코드 비교
- 기술적 구현 세부사항
- 성과 및 배운 점

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
- [x] `Initialize()` 메서드 추가
- [x] Awake()에서 기본 의존성 주입
- [x] 필드를 인터페이스 타입으로 변경

### 2.2 메서드 분해 (Extract Method)

**현재 문제**: `AllBlockMatch()` 메서드가 93줄, 3가지 책임

**분해 대상**:
1. [x] `CollectMatchesFromGrid()` - 그리드 순회 및 매치 수집
2. [x] `ProcessMatchRequests()` - 매치 타입 분류 및 특수 블록 요청
3. [x] `ExecuteMatchDestruction()` - 블록 파괴 및 연쇄 반응

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

#### 2.4.1 의존성 주입 테스트 (3개)
- [ ] `ShouldInitializeWithMockDependencies` - Mock 의존성으로 초기화
- [ ] `ShouldCallMatchDetectorWhenMatching` - 매치 시 MatchDetector 호출 확인
- [ ] `ShouldCallSpecialBlockFactoryWhenCreating` - 특수블록 생성 시 Factory 호출 확인

#### 2.4.2 CollectMatchesFromGrid 테스트 (3개)
- [ ] `ShouldCollectAllMatchesFromGrid` - 그리드의 모든 매치 수집
- [ ] `ShouldReturnEmptyListWhenNoMatches` - 매치 없을 때 빈 리스트
- [ ] `ShouldHandleLargeGridEfficiently` - 대규모 그리드 효율적 처리

#### 2.4.3 ProcessMatchRequests 테스트 (3개)
- [ ] `ShouldClassifyMatchTypes` - 매치 타입 분류
- [ ] `ShouldCreateSpecialBlockRequests` - 특수블록 요청 생성
- [ ] `ShouldHandleMultipleMatchTypes` - 여러 매치 타입 동시 처리

#### 2.4.4 ExecuteMatchDestruction 테스트 (3개)
- [ ] `ShouldDestroyMatchedBlocks` - 매치된 블록 파괴
- [ ] `ShouldProcessChainReactions` - 연쇄반응 처리
- [ ] `ShouldHandleEmptyMatchList` - 빈 매치 리스트 처리

#### 2.4.5 통합 테스트 (3개)
- [ ] `ShouldCompleteFullMatchFlow` - 전체 매치 플로우 완료
- [ ] `ShouldHandleUserMoveMatch` - 유저 이동 매치 처리
- [ ] `ShouldHandleAutoMatch` - 자동 매치 처리

**총 예상 테스트 수**: 15개

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

### 2.6 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 2 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 2: MatchManager 리팩토링" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase2_MatchManager_Refactoring.md`

**포함 내용**:
- God Object 문제점 분석
- 의존성 주입 구조 변경 (Before/After)
- 93줄 메서드 → 3개 메서드 분해 과정
- 중복 코드 제거 사례
- 작성된 테스트 (15개)
- 코드 라인 수 변화 (402 → 280줄)
- 성과 및 배운 점

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

#### 3.4.1 기본 발행/구독 테스트 (3개)
- [ ] `ShouldPublishAndReceiveEvent` - 이벤트 발행 및 수신
- [ ] `ShouldReceiveCorrectEventData` - 올바른 이벤트 데이터 수신
- [ ] `ShouldNotReceiveAfterUnsubscribe` - 구독 해제 후 수신 안됨

#### 3.4.2 다중 구독자 테스트 (3개)
- [ ] `ShouldNotifyAllSubscribers` - 모든 구독자에게 알림
- [ ] `ShouldMaintainSubscriberOrder` - 구독자 순서 유지
- [ ] `ShouldHandleSubscriberException` - 구독자 예외 처리

#### 3.4.3 메모리 관리 테스트 (3개)
- [ ] `ShouldNotLeakMemoryAfterUnsubscribe` - 구독 해제 후 메모리 누수 없음
- [ ] `ShouldClearAllSubscriptions` - Clear() 시 모든 구독 제거
- [ ] `ShouldHandleNullReferences` - null 참조 처리

#### 3.4.4 고급 시나리오 테스트 (3개)
- [ ] `ShouldHandleMultipleEventTypes` - 여러 이벤트 타입 동시 처리
- [ ] `ShouldPreventInfiniteLoop` - 무한 루프 방지
- [ ] `ShouldHandleConcurrentSubscriptions` - 동시 구독 처리

**총 예상 테스트 수**: 12개

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

### 3.6 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 3 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 3: EventBus 도입" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase3_EventBus_Implementation.md`

**포함 내용**:
- Static Event의 문제점 (메모리 누수 위험)
- EventBus 설계 및 구현
- 26개 이벤트 타입 정의
- 6개 Manager 변환 과정
- Before/After 코드 비교
- 작성된 테스트 (12개)
- 메모리 안전성 개선
- 성과 및 배운 점

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

#### 4.5.1 BlockModel 기본 테스트 (5개)
- [ ] `ShouldCreateBlockModelWithPosition` - 위치와 함께 생성
- [ ] `ShouldUpdatePosition` - 위치 업데이트
- [ ] `ShouldMarkForDestruction` - 파괴 마킹
- [ ] `ShouldHaveUniqueId` - 고유 ID 생성
- [ ] `ShouldStoreMatchTypeAndColor` - 매치 타입과 색상 저장

#### 4.5.2 GridManager Pure Logic 테스트 (5개)
- [ ] `ShouldManageBlockModelsWithoutUI` - UI 없이 블록 관리
- [ ] `ShouldAddBlockModelToGrid` - 그리드에 블록 추가
- [ ] `ShouldRemoveBlockModelFromGrid` - 그리드에서 블록 제거
- [ ] `ShouldSwapBlockModels` - 블록 모델 교환
- [ ] `ShouldDetectMatchesWithBlockModels` - 블록 모델로 매치 감지

#### 4.5.3 IBlockView 인터페이스 테스트 (5개)
- [ ] `ShouldImplementIBlockView` - IBlockView 구현 확인
- [ ] `ShouldUpdateVisualFromModel` - 모델로부터 비주얼 업데이트
- [ ] `ShouldPlaySwapAnimation` - 교환 애니메이션 재생
- [ ] `ShouldPlayDestructionAnimation` - 파괴 애니메이션 재생
- [ ] `ShouldSetInteractable` - 상호작용 가능 여부 설정

#### 4.5.4 View-Model 통합 테스트 (5개)
- [ ] `ShouldSyncViewWithModel` - View와 Model 동기화
- [ ] `ShouldUpdateViewWhenModelChanges` - Model 변경 시 View 업데이트
- [ ] `ShouldHandleModelDestruction` - Model 파괴 처리
- [ ] `ShouldMapModelIdToView` - Model ID로 View 매핑
- [ ] `ShouldCleanupViewReferences` - View 참조 정리

**총 예상 테스트 수**: 20개

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

### 4.7 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 4 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 4: UI-로직 분리" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase4_UI_Logic_Separation.md`

**포함 내용**:
- UI-로직 결합의 문제점
- MVC 패턴 적용 설계
- BlockModel 순수 C# 클래스 설계
- IBlockView 인터페이스 정의
- UI_Match_Block 리팩토링 (182 → 120줄)
- GridManager 변경 사항
- 작성된 테스트 (20개)
- 병렬 작업 가능성 향상
- 성과 및 배운 점

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

#### 5.3.1 GridLayoutService 테스트 (10개)
- [ ] `ShouldCalculateSlotPosition` - 슬롯 위치 계산
- [ ] `ShouldCalculateCenterPosition` - 중앙 위치 계산
- [ ] `ShouldHandleOddWidthGrid` - 홀수 너비 그리드 처리
- [ ] `ShouldHandleEvenWidthGrid` - 짝수 너비 그리드 처리
- [ ] `ShouldCalculateTopSlots` - 최상단 슬롯 계산
- [ ] `ShouldHandleIrregularMap` - 불규칙 맵 처리
- [ ] `ShouldCalculateSlotSize` - 슬롯 크기 계산
- [ ] `ShouldPositionAtGridBoundary` - 그리드 경계 위치
- [ ] `ShouldHandleLargeGrid` - 대규모 그리드 처리
- [ ] `ShouldCachePositionCalculations` - 위치 계산 캐싱

#### 5.3.2 BlockSpawner 테스트 (10개)
- [ ] `ShouldSpawnBlockForEmptySlot` - 빈 슬롯에 블록 생성
- [ ] `ShouldSpawnMultipleBlocks` - 여러 블록 동시 생성
- [ ] `ShouldSpawnRandomColorBlock` - 랜덤 색상 블록 생성
- [ ] `ShouldSpawnSpecialBlock` - 특수 블록 생성
- [ ] `ShouldNotSpawnOnOccupiedSlot` - 점유된 슬롯에 생성 안함
- [ ] `ShouldSpawnAtTopPosition` - 최상단 위치에 생성
- [ ] `ShouldTrackSpawnedBlocks` - 생성된 블록 추적
- [ ] `ShouldRespectColorDistribution` - 색상 분포 준수
- [ ] `ShouldHandleSpawnFailure` - 생성 실패 처리
- [ ] `ShouldPoolBlockInstances` - 블록 인스턴스 풀링

#### 5.3.3 GravityService 테스트 (10개)
- [ ] `ShouldApplyGravityToFloatingBlock` - 떠있는 블록에 중력 적용
- [ ] `ShouldMoveBlockDown` - 블록 아래로 이동
- [ ] `ShouldStopAtOccupiedSlot` - 점유된 슬롯에서 정지
- [ ] `ShouldHandleMultipleColumns` - 여러 열 동시 처리
- [ ] `ShouldProcessBottomToTop` - 아래에서 위로 처리
- [ ] `ShouldDetectNeedRefill` - 리필 필요 감지
- [ ] `ShouldCalculateFallDistance` - 낙하 거리 계산
- [ ] `ShouldHandleIrregularShape` - 불규칙 형태 처리
- [ ] `ShouldOptimizeGravityPass` - 중력 적용 최적화
- [ ] `ShouldTriggerSettleEvent` - 안착 이벤트 트리거

**총 예상 테스트 수**: 30개

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

### 5.5 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 5 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 5: MatchFiledManager 리팩토링" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase5_MatchFiledManager_Refactoring.md`

**포함 내용**:
- God Object 문제점 (401줄, 5가지 책임)
- 3개 서비스 클래스 추출 과정
  - GridLayoutService (슬롯 배치)
  - BlockSpawner (블록 생성)
  - GravityService (중력/리필)
- MatchFiledManager 슬림화 (401 → 200줄)
- 작성된 테스트 (30개)
- 서비스 재사용 가능성
- 성과 및 배운 점

---

## Phase 6: 종합 테스트 및 문서화 (Integration Testing & Documentation)

**목표**: 전체 시스템 통합 테스트, 문서 정리
**예상 시간**: 6-8시간
**우선순위**: LOW (포트폴리오 가점)

### 6.1 통합 테스트 작성

**새 파일**: `Assets/01_Script/Tests/SystemIntegrationTests.cs`

실제 게임 시나리오 테스트:

#### 6.1.1 기본 게임 플로우 테스트 (5개)
- [ ] `ShouldCompleteUserMoveToMatchFlow` - 사용자 이동 → 매치 플로우
- [ ] `ShouldApplyGravityAfterMatch` - 매치 후 중력 적용
- [ ] `ShouldDetectCascadeMatches` - 연쇄 매치 감지
- [ ] `ShouldRefillEmptySlots` - 빈 슬롯 리필
- [ ] `ShouldStabilizeGrid` - 그리드 안정화

#### 6.1.2 특수 블록 시나리오 테스트 (5개)
- [ ] `ShouldCreateSpecialBlockOnFourMatch` - 4매치 시 특수블록 생성
- [ ] `ShouldCreateSpecialBlockOnFiveMatch` - 5매치 시 특수블록 생성
- [ ] `ShouldTriggerChainReaction` - 연쇄반응 트리거
- [ ] `ShouldCombineSpecialBlocks` - 특수블록 조합
- [ ] `ShouldClearLargeArea` - 넓은 영역 제거

#### 6.1.3 게임 조건 테스트 (5개)
- [ ] `ShouldDetectClearCondition` - 클리어 조건 감지
- [ ] `ShouldDetectGameOverCondition` - 게임오버 조건 감지
- [ ] `ShouldTrackScore` - 점수 추적
- [ ] `ShouldTrackMoveCount` - 이동 횟수 추적
- [ ] `ShouldCompleteFullGameSession` - 전체 게임 세션 완료

**총 예상 테스트 수**: 15개

### 6.2 성능 테스트

**새 파일**: `Assets/01_Script/Tests/PerformanceTests.cs`

#### 6.2.1 스캔 성능 테스트 (2개)
- [ ] `ShouldScanLargeGridUnder1Second` - 100x100 그리드 스캔 (< 1초)
- [ ] `ShouldScanIrregularGridEfficiently` - 불규칙 그리드 효율적 스캔

#### 6.2.2 매치 시뮬레이션 테스트 (2개)
- [ ] `ShouldSimulate1000MatchesUnder1Second` - 1000번 매치 시뮬레이션 (< 1초)
- [ ] `ShouldHandleCascadeMatchesEfficiently` - 연쇄 매치 효율적 처리

#### 6.2.3 메모리 관리 테스트 (2개)
- [ ] `ShouldNotLeakMemoryAfter1000Iterations` - 1000번 반복 후 메모리 누수 없음 (< 10%)
- [ ] `ShouldReleaseResourcesProperly` - 리소스 적절히 해제

#### 6.2.4 GC 할당 테스트 (2개)
- [ ] `ShouldMinimizeGCInMatchLoop` - 매치 루프에서 GC 최소화
- [ ] `ShouldReuseCollections` - 컬렉션 재사용

**총 예상 테스트 수**: 8개

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

### 6.6 Notion 포트폴리오 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] Phase 6 작업 내용을 Notion 문서로 생성
- [ ] 메인 페이지 "Phase 6: 통합 테스트 및 문서화" 섹션에 하위 페이지로 추가

**생성 파일**: `Documentation/Phase6_Integration_Testing_Documentation.md`

**포함 내용**:
- 통합 테스트 전략
- 작성된 통합 테스트 (15개)
  - 전체 게임 플로우 테스트
  - 연쇄 반응 통합 테스트
  - 씬 전환 테스트
- 성능 테스트 (8개)
  - 대규모 그리드 성능
  - 메모리 사용량
  - GC 할당 최소화
- Architecture.md 작성 내용
- README 업데이트 사항
- 프로젝트 최종 메트릭스
- 전체 리팩토링 회고

### 6.7 프로젝트 완료 최종 문서화

**작업 내용**:
- [ ] unity-notion-documenter agent 실행
- [ ] 전체 프로젝트 회고 문서 생성
- [ ] 메인 페이지 마지막에 "프로젝트 완료 회고" 섹션 추가

**생성 파일**: `Documentation/Project_Retrospective.md`

**포함 내용**:
- Phase 0-6 전체 요약
- Before/After 메트릭스 비교표
- AI 협업 워크플로우 효과 분석
- 배운 점 및 성장 포인트
- 향후 개선 방향
- 포트폴리오 하이라이트

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

#### 🔄 Phase 1: 인터페이스 추출
- [x] 1.1: IServices.cs 생성
- [x] 1.2: 기존 클래스 인터페이스 구현 (7개)
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
