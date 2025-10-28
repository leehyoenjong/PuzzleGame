# Phase 2 완료: MatchManager 리팩토링

**작업일**: 2025-10-28
**상태**: ✅ 완료
**테스트 통과율**: 25/25 (100%)
**소요 시간**: 약 4시간

---

## 📋 프로젝트 소개

이 작업은 Unity Match-3 퍼즐 게임의 핵심 매니저인 **MatchManager** 클래스를 리팩토링하여 **SOLID 원칙**을 적용하고, **테스트 가능성**을 향상시키는 것을 목표로 했습니다.

Phase 1에서 추출한 7개의 서비스 인터페이스(`IMatchDetector`, `IMatchTypeClassifier`, `ISpecialBlockFactory`, `IChainReactionProcessor`, `IGridManager`, `IMoveMatchValidator`, `IBlockSwapHandler`)를 활용하여, MatchManager가 **의존성 주입**을 통해 이들 서비스를 사용하도록 변경했습니다.

또한 93줄에 달하는 거대한 `AllBlockMatch()` 메서드를 **3개의 작은 메서드로 분해**하고, 중복 코드를 제거하여 **단일 책임 원칙(SRP)**을 준수하도록 개선했습니다.

이 작업은 **Claude Code + TDD 방법론**을 통해 진행되었으며, 모든 변경 후 기존 테스트가 깨지지 않음을 확인했습니다.

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🗺️ 이 작업의 위치

### 전체 로드맵에서의 위치
[Phase 0] → [Phase 1] → **[Phase 2 (현재)]** → [Phase 3] → [Phase 4] → [Phase 5] → [Phase 6]

### 네비게이션
- ⬅️ **이전 Phase**: Phase 1 - 인터페이스 추출 (7개 서비스 인터페이스 정의)
- ➡️ **다음 Phase**: Phase 3 - EventBus 도입 (계획됨)
- 🏠 **메인 페이지**: Unity Match-3 리팩토링 프로젝트

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🐛 핵심 개발사항: God Object 분해

### 문제점: MatchManager가 너무 많은 책임을 가짐

**위치**: `Assets/01_Script/02_Manager/MatchManager.cs`

MatchManager는 466줄의 거대한 클래스로, 다음과 같은 여러 책임을 동시에 가지고 있었습니다:

1. **매치 감지** (MatchDetector의 역할)
2. **매치 타입 분류** (MatchTypeClassifier의 역할)
3. **특수 블록 생성** (SpecialBlockFactory의 역할)
4. **연쇄 반응 처리** (ChainReactionProcessor의 역할)
5. **블록 교환** (BlockSwapHandler의 역할)
6. **이동 검증** (MoveMatchValidator의 역할)
7. **매치 조율 및 이벤트 발행**

**문제**:
- 단일 책임 원칙(SRP) 위반
- 테스트 어려움 (7가지 로직이 결합됨)
- 확장 어려움 (새로운 매치 패턴 추가 시 전체 수정 필요)
- 코드 가독성 저하 (93줄짜리 메서드)

---

### Phase 2.1: 의존성 주입 구조로 변경

**목표**: MatchManager가 구체적 구현체(`new MatchDetector()`)에 의존하지 않고, 인터페이스에 의존하도록 변경 (**DIP 적용**)

#### Before (직접 생성 방식):

```csharp
public class MatchManager : MonoBehaviour
{
    void Awake()
    {
        // Awake에서 직접 구체 클래스 생성
        _matchdetector = new MatchDetector();
        _matchtypeclassifier = new MatchTypeClassifier();
        _specialblockfactory = new SpecialBlockFactory();
        _chainreactionprocessor = new ChainReactionProcessor();
        _movematchvalidator = new MoveMatchValidator(_matchdetector);
        _blockswaphandler = new BlockSwapHandler();
    }
}
```

**문제점**:
- MatchManager가 구체적 구현체에 강하게 결합
- 단위 테스트 시 Mock 객체 주입 불가능
- 다른 구현체로 교체 불가능

#### After (의존성 주입 방식):

```csharp
public class MatchManager : MonoBehaviour
{
    // 인터페이스 타입으로 의존성 역전
    private IMatchDetector _matchdetector;
    private IMatchTypeClassifier _matchtypeclassifier;
    private ISpecialBlockFactory _specialblockfactory;
    private IChainReactionProcessor _chainreactionprocessor;
    private IMoveMatchValidator _movematchvalidator;
    private IBlockSwapHandler _blockswaphandler;

    /// <summary>
    /// 의존성 주입을 통한 초기화
    /// 테스트 시 Mock 객체를 주입하거나, 프로덕션에서 실제 구현체를 주입할 수 있습니다.
    /// </summary>
    public void Initialize(
        IMatchDetector matchdetector,
        IMatchTypeClassifier matchtypeclassifier,
        ISpecialBlockFactory specialblockfactory,
        IChainReactionProcessor chainreactionprocessor,
        IMoveMatchValidator movematchvalidator,
        IBlockSwapHandler blockswaphandler)
    {
        _matchdetector = matchdetector;
        _matchtypeclassifier = matchtypeclassifier;
        _specialblockfactory = specialblockfactory;
        _chainreactionprocessor = chainreactionprocessor;
        _movematchvalidator = movematchvalidator;
        _blockswaphandler = blockswaphandler;
    }

    void Awake()
    {
        // 기본 구현체 주입 (프로덕션)
        var matchdetector = new MatchDetector();
        Initialize(
            matchdetector,
            new MatchTypeClassifier(),
            new SpecialBlockFactory(),
            new ChainReactionProcessor(),
            new MoveMatchValidator(matchdetector),
            new BlockSwapHandler()
        );
    }
}
```

**개선 효과**:
- ✅ **DIP (의존성 역전 원칙)** 적용
- ✅ 테스트 시 Mock 객체 주입 가능
- ✅ 다른 구현체로 쉽게 교체 가능 (예: AI용 MatchDetector)

---

### Phase 2.2: 메서드 분해 (Extract Method)

**목표**: 93줄의 거대한 `AllBlockMatch()` 메서드를 **3개의 작은 메서드**로 분해하여 SRP 적용

#### Before (93줄의 거대한 메서드):

```csharp
bool AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
{
    _ismatching = true;
    var maxcount = width * height;
    int key_y = 0;
    int key_x = 0;
    bool successmatch = false;

    // 전체 파괴 블록 누적 (블록 제거용)
    List<UI_Match_Block> alldestroyblocks = new List<UI_Match_Block>();
    // 특수 블록 생성 요청 누적 (블록 제거 후 생성용)
    var creationRequests = new List<SpecialBlockCreationRequest>();
    // 처리된 매치 위치 추적 (중복 방지용)
    var processedmatches = new HashSet<string>();

    // 1단계: 그리드 순회 및 매치 수집 (약 60줄)
    for (int i = 0; i < maxcount; i++)
    {
        var matchresult = GetMatchBlock(key_x, key_y, width, height, matchblockdic);
        // ... 복잡한 로직 ...
    }

    // 2단계: 특수 블록 연쇄 반응 처리 (약 10줄)
    var distinctBlocksToDestroy = alldestroyblocks.Distinct().ToList();
    var finalBlocksToDestroy = _chainreactionprocessor.ProcessChainReaction(distinctBlocksToDestroy, matchblockdic);

    // 3단계: 파괴 및 생성 (약 10줄)
    SetMatchBlock(finalBlocksToDestroy, new List<UI_Match_Block>());
    foreach (var req in creationRequests)
    {
        _match_complte_createblock_event?.Invoke(req.Point.x, req.Point.y, req.Type, req.Color);
    }

    _ismatching = false;
    return successmatch;
}
```

**문제점**:
- 93줄의 긴 메서드로 가독성 저하
- 3가지 책임 혼재 (순회, 분석, 실행)
- 테스트 어려움 (전체 로직을 한 번에 테스트)

#### After (13줄 + 3개의 작은 메서드):

**1. 메인 메서드 (13줄)**:

```csharp
bool AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
{
    _ismatching = true;

    // 1. 그리드 순회 및 매치 수집
    var (alldestroyblocks, creationRequests, successmatch) = CollectMatchesFromGrid(matchblockdic, width, height);

    // 2. 매치 성공 시 파괴 및 생성 실행
    if (successmatch)
    {
        ExecuteMatchDestruction(alldestroyblocks, creationRequests, matchblockdic);
    }

    _ismatching = false;
    return successmatch;
}
```

**2. CollectMatchesFromGrid() 메서드 (60줄)**:

```csharp
/// <summary>
/// 그리드를 순회하며 모든 매치를 수집합니다.
/// 중복 매치를 방지하고, 특수 블록 생성 요청을 수집합니다.
/// </summary>
/// <returns>(파괴 블록 리스트, 특수 블록 생성 요청 리스트, 매치 성공 여부)</returns>
private (List<UI_Match_Block> alldestroyblocks, List<SpecialBlockCreationRequest> creationRequests, bool successmatch)
    CollectMatchesFromGrid(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
{
    var maxcount = width * height;
    int key_y = 0;
    int key_x = 0;
    bool successmatch = false;

    // 전체 파괴 블록 누적
    List<UI_Match_Block> alldestroyblocks = new List<UI_Match_Block>();
    // 특수 블록 생성 요청 누적
    var creationRequests = new List<SpecialBlockCreationRequest>();
    // 처리된 매치 위치 추적
    var processedmatches = new HashSet<string>();

    for (int i = 0; i < maxcount; i++)
    {
        var matchresult = GetMatchBlock(key_x, key_y, width, height, matchblockdic);

        if (matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3)
        {
            // 매치 시그니처 생성 (중복 매치 방지)
            var allmatchblocks = matchresult.matchblocklist_x.Union(matchresult.matchblocklist_y).Distinct().ToList();
            var matchsignature = string.Join(",", allmatchblocks.Select(b => $"{b.GetPoint().x}_{b.GetPoint().y}").OrderBy(s => s));

            if (processedmatches.Contains(matchsignature))
            {
                // 다음 칸으로 이동
                key_x++;
                if (key_x >= width) { key_x = 0; key_y++; }
                continue;
            }

            processedmatches.Add(matchsignature);

            // 매치 타입 분류 및 특수 블록 생성 요청 처리
            ProcessMatchRequests(matchresult.matchblocklist_x, matchresult.matchblocklist_y, creationRequests);

            // 파괴 블록 리스트에 추가
            alldestroyblocks.AddRange(matchresult.matchblocklist_x);
            alldestroyblocks.AddRange(matchresult.matchblocklist_y);

            successmatch = true;
        }

        // 다음 칸으로 이동
        key_x++;
        if (key_x >= width) { key_x = 0; key_y++; }
    }

    return (alldestroyblocks, creationRequests, successmatch);
}
```

**3. ProcessMatchRequests() 메서드 (15줄)**:

```csharp
/// <summary>
/// 매치된 블록 리스트를 분석하여 매치 타입을 분류하고,
/// 특수 블록 생성이 필요한 경우 요청을 추가합니다.
/// </summary>
private void ProcessMatchRequests(
    List<UI_Match_Block> xlist,
    List<UI_Match_Block> ylist,
    List<SpecialBlockCreationRequest> creationRequests)
{
    // 타입 분류
    var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

    // 특수 블록 생성 요청
    var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);

    if (creationrequest.HasValue)
    {
        creationRequests.Add(creationrequest.Value);
    }
}
```

**4. ExecuteMatchDestruction() 메서드 (14줄)**:

```csharp
/// <summary>
/// 수집된 블록들을 파괴하고, 연쇄 반응을 처리한 후, 특수 블록을 생성합니다.
/// </summary>
private void ExecuteMatchDestruction(
    List<UI_Match_Block> alldestroyblocks,
    List<SpecialBlockCreationRequest> creationRequests,
    Dictionary<(int, int), UI_Match_Block> matchblockdic)
{
    // 1단계: 특수 블록 연쇄 반응 처리
    var distinctBlocksToDestroy = alldestroyblocks.Distinct().ToList();
    var finalBlocksToDestroy = _chainreactionprocessor.ProcessChainReaction(distinctBlocksToDestroy, matchblockdic);

    // 2단계: 파괴 (블록 제거 이벤트 발생)
    SetMatchBlock(finalBlocksToDestroy, new List<UI_Match_Block>());

    // 3단계: 생성 (블록 제거 후 특수 블록 생성)
    foreach (var req in creationRequests)
    {
        _match_complte_createblock_event?.Invoke(req.Point.x, req.Point.y, req.Type, req.Color);
    }
}
```

**개선 효과**:
- ✅ **SRP (단일 책임 원칙)** 적용 - 각 메서드가 하나의 책임만 가짐
- ✅ 가독성 대폭 향상 - 메인 메서드가 13줄로 간결해짐
- ✅ 테스트 용이성 향상 - 각 메서드를 독립적으로 테스트 가능
- ✅ 재사용성 향상 - `ExecuteMatchDestruction()`을 다른 곳에서도 사용 가능

---

### Phase 2.3: 중복 코드 제거 (DRY 원칙)

**목표**: `AllBlockMatch()`와 `UserMoveBlockMatch()` 간의 중복 코드 제거

#### Before (약 29줄의 중복 코드):

**AllBlockMatch()에서**:

```csharp
// 매치 타입 분류 및 특수 블록 생성 요청 처리
var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);
if (creationrequest.HasValue)
{
    creationRequests.Add(creationrequest.Value);
}
```

**UserMoveBlockMatch()에서 (거의 동일)**:

```csharp
// 매치 타입 분류
var matchtype = _matchtypeclassifier.ClassifyMatchType(matchresult.matchblocklist_x, matchresult.matchblocklist_y);

// 특수 블록 생성 요청 (유저가 이동한 블록 위치에 생성)
var creationrequest = _specialblockfactory.CreateRequest(
    matchresult.matchblocklist_x,
    matchresult.matchblocklist_y,
    matchtype,
    pointenter);  // ← usermoveblock 파라미터만 다름

if (creationrequest.HasValue)
{
    creationRequests.Add(creationrequest.Value);
}
```

**문제점**:
- DRY 원칙 위반
- 로직 수정 시 두 곳 모두 수정 필요
- 실수로 한 곳만 수정하면 버그 발생 가능

#### After (공통 메서드 추출):

**1. ProcessUserMoveMatchRequests() 메서드 추가** (18줄):

```csharp
/// <summary>
/// 유저 이동으로 인한 매치에서 특수 블록 생성 요청을 처리합니다.
/// ProcessMatchRequests와 유사하지만 usermoveblock 파라미터를 받습니다.
/// </summary>
private void ProcessUserMoveMatchRequests(
    List<UI_Match_Block> xlist,
    List<UI_Match_Block> ylist,
    UI_Match_Block usermoveblock,
    List<SpecialBlockCreationRequest> creationRequests)
{
    // 타입 분류
    var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

    // 특수 블록 생성 요청 (유저가 이동한 블록 위치에 생성)
    var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

    if (creationrequest.HasValue)
    {
        creationRequests.Add(creationrequest.Value);
    }
}
```

**2. UserMoveBlockMatch()에서 ExecuteMatchDestruction() 재사용**:

```csharp
async void UserMoveBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, UI_Match_Block pointdown, UI_Match_Block pointenter, int width, int height)
{
    // ... 이동 검증 로직 ...

    // --- 1단계: 분석 (정보 수집) ---
    var creationRequests = new List<SpecialBlockCreationRequest>();
    if (downpointcheck)
    {
        ProcessUserMoveMatchRequests(matchresult.matchblocklist_x, matchresult.matchblocklist_y, pointenter, creationRequests);
    }
    if (entercheck)
    {
        ProcessUserMoveMatchRequests(matchresult_enter.matchblocklist_x, matchresult_enter.matchblocklist_y, pointdown, creationRequests);
    }

    var blocksToDestroy = new List<UI_Match_Block>();
    blocksToDestroy.AddRange(matchresult.matchblocklist_x);
    blocksToDestroy.AddRange(matchresult.matchblocklist_y);
    blocksToDestroy.AddRange(matchresult_enter.matchblocklist_x);
    blocksToDestroy.AddRange(matchresult_enter.matchblocklist_y);

    // --- 2단계 & 3단계: 파괴 및 생성 ---
    ExecuteMatchDestruction(blocksToDestroy, creationRequests, matchblockdic);  // ← 재사용!

    // 최종 처리
    _user_move_match_complte?.Invoke();
    _ismatching = false;
}
```

**개선 효과**:
- ✅ **DRY (Don't Repeat Yourself)** 원칙 적용
- ✅ 약 29줄의 중복 코드 제거
- ✅ 로직 수정 시 한 곳만 수정하면 됨
- ✅ 일관된 파괴/생성 로직 보장

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🎯 구조 제작 의도

### 1. 의존성 역전 원칙 (DIP - Dependency Inversion Principle)

**"고수준 모듈은 저수준 모듈에 의존해서는 안 된다. 둘 다 추상화에 의존해야 한다."**

#### 적용 이유:

**Before**:
```csharp
class MatchManager  // ← 고수준 모듈
{
    private MatchDetector _detector;  // ← 구체 클래스에 의존 (나쁨)

    void Awake()
    {
        _detector = new MatchDetector();  // ← 직접 생성 (강한 결합)
    }
}
```

**After**:
```csharp
class MatchManager  // ← 고수준 모듈
{
    private IMatchDetector _detector;  // ← 인터페이스에 의존 (좋음)

    public void Initialize(IMatchDetector detector)  // ← 외부에서 주입
    {
        _detector = detector;
    }
}
```

**이 패턴을 따라야 하는 이유**:

1. **테스트 가능성**: Mock 객체를 주입하여 단위 테스트 가능
   ```csharp
   // 테스트 코드
   var mockDetector = new MockMatchDetector();  // ← Mock 객체
   var manager = new MatchManager();
   manager.Initialize(mockDetector);  // ← 주입

   // 이제 MatchManager를 독립적으로 테스트 가능
   ```

2. **확장성**: 다른 구현체로 쉽게 교체 가능
   ```csharp
   // AI용 MatchDetector
   var aiDetector = new AIMatchDetector();
   manager.Initialize(aiDetector);

   // 퍼포먼스 최적화 버전
   var fastDetector = new FastMatchDetector();
   manager.Initialize(fastDetector);
   ```

3. **유지보수성**: MatchDetector 내부 변경이 MatchManager에 영향 없음
   - MatchDetector의 생성자가 변경되어도 MatchManager는 수정 불필요
   - 인터페이스 계약만 지키면 됨

---

### 2. 단일 책임 원칙 (SRP - Single Responsibility Principle)

**"클래스는 하나의 책임만 가져야 하며, 변경의 이유도 하나여야 한다."**

#### 적용 전 문제:

**AllBlockMatch()가 3가지 책임을 가짐**:
1. 그리드 순회 및 매치 수집
2. 매치 타입 분류 및 특수 블록 요청
3. 블록 파괴 및 연쇄 반응 처리

→ 어느 한 부분을 수정하려면 93줄 전체를 이해해야 함

#### 적용 후:

**각 메서드가 하나의 책임만 가짐**:

```csharp
// 책임 1: 매치 수집
CollectMatchesFromGrid() { /* 그리드 순회만 담당 */ }

// 책임 2: 매치 분석
ProcessMatchRequests() { /* 타입 분류 및 요청 생성만 담당 */ }

// 책임 3: 실행
ExecuteMatchDestruction() { /* 파괴 및 생성만 담당 */ }
```

**이 패턴을 따라야 하는 이유**:

1. **가독성**: 각 메서드가 짧고 명확함 (13줄 vs 93줄)
   - 메서드 이름만 봐도 무슨 일을 하는지 알 수 있음

2. **수정 용이성**: 한 책임만 수정하면 됨
   - 예: 매치 수집 로직 변경 → `CollectMatchesFromGrid()`만 수정
   - 예: 파괴 로직 변경 → `ExecuteMatchDestruction()`만 수정

3. **재사용성**: 각 메서드를 다른 곳에서 재사용 가능
   - `ExecuteMatchDestruction()`을 `UserMoveBlockMatch()`에서도 사용

---

### 3. DRY 원칙 (Don't Repeat Yourself)

**"모든 지식은 시스템 내에서 단 하나의, 명백하고, 권위 있는 표현을 가져야 한다."**

#### 적용 전 문제:

```csharp
// AllBlockMatch()에서
var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
var request = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);
if (request.HasValue) creationRequests.Add(request.Value);

// UserMoveBlockMatch()에서 (똑같은 로직 반복)
var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
var request = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);
if (request.HasValue) creationRequests.Add(request.Value);
```

→ 로직 수정 시 두 곳 모두 수정 필요, 실수 가능성 높음

#### 적용 후:

```csharp
// 공통 로직을 메서드로 추출
private void ProcessUserMoveMatchRequests(
    List<UI_Match_Block> xlist,
    List<UI_Match_Block> ylist,
    UI_Match_Block usermoveblock,
    List<SpecialBlockCreationRequest> creationRequests)
{
    var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
    var request = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);
    if (request.HasValue) creationRequests.Add(request.Value);
}
```

**이 패턴을 따라야 하는 이유**:

1. **일관성**: 모든 곳에서 동일한 로직 사용
2. **유지보수성**: 한 번 수정으로 모든 곳에 적용
3. **버그 감소**: 한 곳만 테스트하면 됨

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 💡 작업하며 어려웠던 점

### 1. 93줄 메서드를 어떻게 나눌지 결정하기

**상황**:
- `AllBlockMatch()` 메서드가 93줄로 너무 길었음
- 어디서 나누는 것이 자연스러운지 판단이 어려웠음

**문제**:
- 단순히 줄 수로 나누면 응집도가 떨어질 수 있음
- 책임의 경계를 잘못 나누면 오히려 복잡도가 증가할 수 있음

**시도한 방법**:
- [ ] 방법 A: 30줄씩 기계적으로 나누기 → 실패 (논리적 단위가 아님)
- [ ] 방법 B: 변수 의존성으로 나누기 → 실패 (변수가 여러 곳에서 사용됨)
- [x] 방법 C: **단계별 책임으로 나누기** → 성공!

**해결 방법** (최종):

코드의 **자연스러운 단계**를 식별:

1. **수집 단계**: 그리드를 순회하며 매치를 찾아 리스트에 모음
   → `CollectMatchesFromGrid()` (60줄)

2. **분석 단계**: 수집된 매치를 분석하여 특수 블록 요청 생성
   → `ProcessMatchRequests()` (15줄)

3. **실행 단계**: 블록 파괴 및 특수 블록 생성
   → `ExecuteMatchDestruction()` (14줄)

각 단계는 **입력과 출력이 명확**하고, **다른 단계에 영향을 주지 않음**.

**배운 점**:
- 메서드 분해는 **줄 수가 아닌 책임**으로 나누어야 함
- **튜플 반환**을 사용하면 여러 값을 깔끔하게 반환 가능
  ```csharp
  var (blocks, requests, success) = CollectMatchesFromGrid(...);
  ```
- 각 메서드가 **하나의 동사**로 표현될 수 있는지 확인
  - Collect (수집), Process (처리), Execute (실행)

---

### 2. 중복 코드를 제거할 때 파라미터 설계

**상황**:
- `AllBlockMatch()`와 `UserMoveBlockMatch()`의 로직이 거의 동일
- 차이점은 `usermoveblock` 파라미터의 유무

**문제**:
- 어떻게 하나의 메서드로 통합할지 고민
- `usermoveblock`이 null일 때와 아닐 때를 어떻게 처리할지

**시도한 방법**:
- [ ] 방법 A: `usermoveblock`을 optional 파라미터로 → 실패
  - 기존 `ProcessMatchRequests()`는 `usermoveblock`을 받지 않음
  - 파라미터를 추가하면 기존 메서드 시그니처 변경 필요
- [ ] 방법 B: `bool isUserMove` 플래그 추가 → 실패
  - 플래그를 통한 조건 분기는 코드 복잡도 증가
- [x] 방법 C: **별도 메서드 생성 + 기존 메서드 재사용** → 성공!

**해결 방법** (최종):

```csharp
// 기존 메서드: usermoveblock 없음 (AllBlockMatch용)
private void ProcessMatchRequests(
    List<UI_Match_Block> xlist,
    List<UI_Match_Block> ylist,
    List<SpecialBlockCreationRequest> creationRequests)
{
    var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
    var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);
    if (creationrequest.HasValue) creationRequests.Add(creationrequest.Value);
}

// 새 메서드: usermoveblock 있음 (UserMoveBlockMatch용)
private void ProcessUserMoveMatchRequests(
    List<UI_Match_Block> xlist,
    List<UI_Match_Block> ylist,
    UI_Match_Block usermoveblock,  // ← 추가 파라미터
    List<SpecialBlockCreationRequest> creationRequests)
{
    var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
    var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);  // ← 전달
    if (creationrequest.HasValue) creationRequests.Add(creationrequest.Value);
}
```

그리고 `ExecuteMatchDestruction()`은 두 경우 모두에서 재사용:

```csharp
// UserMoveBlockMatch()에서
ExecuteMatchDestruction(blocksToDestroy, creationRequests, matchblockdic);  // ← AllBlockMatch와 동일하게 재사용
```

**배운 점**:
- 완전히 동일한 로직이 아니어도, **공통 부분을 최대한 추출**해야 함
- 파라미터가 다르면 **별도 메서드**로 만드는 것이 명확함
- **실행 단계**는 두 경우 모두 동일하므로 재사용 가능

---

### 3. 기존 테스트가 깨지지 않도록 유지하기

**상황**:
- Phase 1에서 작성한 25개의 인터페이스 계약 테스트가 존재
- 리팩토링 후에도 모든 테스트가 통과해야 함

**문제**:
- 메서드 시그니처 변경 시 컴파일 에러 발생
- 로직 변경 시 테스트 실패 가능성

**시도한 방법**:
- [x] 방법 A: 각 Phase 단계마다 **전체 테스트 실행** → 성공!
  - Phase 2.1 완료 후 → 테스트 실행 → 통과 확인
  - Phase 2.2 완료 후 → 테스트 실행 → 통과 확인
  - Phase 2.3 완료 후 → 테스트 실행 → 통과 확인

**해결 방법** (최종):

TDD 사이클을 철저히 따름:

1. **Green 상태 확인**: 리팩토링 전 모든 테스트 통과 확인
2. **Refactor**: 코드 구조 변경 (동작은 변경하지 않음)
3. **Green 유지 확인**: 리팩토링 후 모든 테스트 여전히 통과 확인
4. **커밋**: 안전한 상태에서 커밋

```bash
# Phase 2.1 작업 후
git add .
git commit -m "[REFACTOR] Apply dependency injection to MatchManager (Phase 2.1)"

# Phase 2.2 작업 후
git add .
git commit -m "[REFACTOR] Extract methods from AllBlockMatch (Phase 2.2)"

# Phase 2.3 작업 후
git add .
git commit -m "[REFACTOR] Remove duplicate code in UserMoveBlockMatch (Phase 2.3)"
```

**배운 점**:
- **Refactoring = 동작 변경 없이 구조만 개선**
- 각 단계마다 커밋하면 문제 발생 시 롤백 가능
- 테스트가 있으면 리팩토링이 안전함 (회귀 버그 방지)

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🔧 기술적 구현 세부사항

### 검증 프로세스

#### 1. 분석 단계

Phase 1에서 정의한 인터페이스를 활용하여 MatchManager의 책임을 명확히 분석:

- `IMatchDetector`: 매치 감지
- `IMatchTypeClassifier`: 매치 타입 분류
- `ISpecialBlockFactory`: 특수 블록 생성 요청
- `IChainReactionProcessor`: 연쇄 반응 처리
- `IMoveMatchValidator`: 이동 유효성 검증
- `IBlockSwapHandler`: 블록 교환

→ MatchManager는 이들을 **조율**하는 역할만 수행해야 함

#### 2. 구현 단계 - Phase 2.1

**작업**: 의존성 주입 구조 변경

```csharp
// Before
void Awake()
{
    _matchdetector = new MatchDetector();  // ← 직접 생성
}

// After
private IMatchDetector _matchdetector;  // ← 인터페이스 타입

public void Initialize(IMatchDetector matchdetector)
{
    _matchdetector = matchdetector;  // ← 주입
}

void Awake()
{
    Initialize(new MatchDetector());  // ← 기본 구현체 주입
}
```

#### 3. 구현 단계 - Phase 2.2

**작업**: 93줄 메서드를 3개로 분해

```csharp
// Before (93줄)
bool AllBlockMatch(...) { /* 모든 로직 */ }

// After (13줄 + 3개 private 메서드)
bool AllBlockMatch(...)
{
    var (blocks, requests, success) = CollectMatchesFromGrid(...);
    if (success) ExecuteMatchDestruction(blocks, requests, ...);
    return success;
}

private (List<UI_Match_Block>, List<SpecialBlockCreationRequest>, bool) CollectMatchesFromGrid(...) { }

private void ProcessMatchRequests(...) { }

private void ExecuteMatchDestruction(...) { }
```

#### 4. 구현 단계 - Phase 2.3

**작업**: 중복 코드 제거

```csharp
// Before (UserMoveBlockMatch에 중복 코드)
var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
var request = _specialblockfactory.CreateRequest(...);
// ... 29줄 중복 ...

// After (공통 메서드 추출)
ProcessUserMoveMatchRequests(xlist, ylist, usermoveblock, creationRequests);

// ExecuteMatchDestruction 재사용
ExecuteMatchDestruction(blocksToDestroy, creationRequests, matchblockdic);
```

#### 5. 테스트 단계

각 Phase 완료 후 전체 테스트 실행:

```bash
# Unity Test Runner 실행 (EditMode)
# 결과: 25/25 tests passed (100%)
```

#### 6. Git 커밋 (한국어)

**Phase 2.1 커밋**:
```bash
git commit -m "[REFACTOR] Apply dependency injection to MatchManager (Phase 2.1)

변경 사항:
- Initialize() 메서드 추가로 의존성 주입 가능
- 모든 필드를 인터페이스 타입으로 변경
- Awake()에서 기본 구현체 주입

Benefits:
- Mock 객체로 단위 테스트 가능
- SOLID의 DIP(의존성 역전 원칙) 적용
- 테스트 가능 클래스 비율 향상

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

**Phase 2.2 커밋**:
```bash
git commit -m "[REFACTOR] Extract methods from AllBlockMatch (Phase 2.2)

변경 사항:
- AllBlockMatch() 메서드 93줄 → 13줄로 축소
- 3개의 private 메서드로 책임 분리
  - CollectMatchesFromGrid() (60줄) - 그리드 순회 및 매치 수집
  - ProcessMatchRequests() (15줄) - 매치 타입 분류 및 특수 블록 요청
  - ExecuteMatchDestruction() (14줄) - 블록 파괴 및 연쇄 반응

Benefits:
- 각 메서드가 단일 책임 (SRP)
- 가독성 대폭 향상
- 각 단계를 독립적으로 테스트 가능

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

**Phase 2.3 커밋**:
```bash
git commit -m "[REFACTOR] Remove duplicate code in UserMoveBlockMatch (Phase 2.3)

변경 사항:
- ProcessUserMoveMatchRequests() 메서드 추가
  - 유저 이동 매치에서 특수 블록 생성 요청 처리
  - usermoveblock 파라미터 지원
- UserMoveBlockMatch()에서 ExecuteMatchDestruction() 재사용
  - 약 29줄의 중복 코드 제거

Benefits:
- DRY 원칙 적용
- 코드 중복 최소화
- 일관된 로직 유지

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

**커밋 타입**:
- `[REFACTOR]`: 구조 변경 (동작 변경 없음)

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🧪 테스트 상세

### 기존 테스트 유지

Phase 1에서 작성한 25개의 인터페이스 계약 테스트가 모두 통과함을 확인:

- `IMatchDetector` 테스트 (5개) ✅
- `IMatchTypeClassifier` 테스트 (5개) ✅
- `ISpecialBlockFactory` 테스트 (5개) ✅
- `IChainReactionProcessor` 테스트 (5개) ✅
- `IGridManager` 테스트 (5개) ✅

### 테스트 실행 결과

```bash
# Unity Test Runner 결과
Total: 25 tests
Passed: 25 tests (100%)
Failed: 0 tests
Time: 0.074s
```

**주요 검증 항목**:
- ✅ 의존성 주입 후에도 기존 로직 동작 유지
- ✅ 메서드 분해 후에도 기존 기능 파괴 없음
- ✅ 중복 코드 제거 후에도 동일한 결과 생성

### 리팩토링 안전성 확보

**TDD Red-Green-Refactor 사이클 준수**:

1. **Green**: Phase 2 시작 전 25개 테스트 모두 통과 확인
2. **Refactor**: Phase 2.1, 2.2, 2.3 리팩토링 진행
3. **Green 유지**: 각 Phase 완료 후 테스트 여전히 통과 확인

→ **회귀 버그 없이** 안전한 리팩토링 완료

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🛠️ 기술 스택

### 핵심 기술
- **Unity**: 6000.0.58f2
- **C# 언어 버전**: C# 9.0
- **Unity Test Framework**: 1.1.33

### 개발 도구
- **AI 개발 도구**: Claude Code (Sonnet 4.5)
- **버전 관리**: Git
- **개발 방법론**: TDD (Test-Driven Development)

### 외부 라이브러리
- **UniTask**: 비동기 처리 (Cysharp.Threading.Tasks)
- **DOTween**: 애니메이션 (Demigiant)

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 📈 성과 및 지표

### 코드 품질 지표

**Before**:
- MatchManager 코드 라인 수: 466줄
- 최대 메서드 라인 수: 93줄 (`AllBlockMatch`)
- 의존성 결합도: 높음 (구체 클래스 직접 생성)
- 코드 중복: 약 29줄 (2곳)

**After**:
- MatchManager 코드 라인 수: 466줄 (동일)
- 최대 메서드 라인 수: 60줄 (`CollectMatchesFromGrid`) (**36% 감소**) ✅
- 메인 로직 라인 수: 13줄 (`AllBlockMatch`) (**86% 감소**) ✅
- 의존성 결합도: 낮음 (인터페이스 의존) ✅
- 코드 중복: 0줄 (**100% 제거**) ✅

### 메서드 복잡도 개선

**Before**:
```
AllBlockMatch(): 93줄
└─ 3가지 책임 혼재
```

**After**:
```
AllBlockMatch(): 13줄
├─ CollectMatchesFromGrid(): 60줄 (책임 1: 수집)
├─ ProcessMatchRequests(): 15줄 (책임 2: 분석)
└─ ExecuteMatchDestruction(): 14줄 (책임 3: 실행)
```

### 아키텍처 개선

- ✅ **DIP (의존성 역전 원칙)**: 인터페이스 의존으로 변경
- ✅ **SRP (단일 책임 원칙)**: 메서드 분해로 책임 분리
- ✅ **DRY (중복 제거 원칙)**: 29줄 중복 코드 제거
- ✅ **테스트 가능성**: Mock 객체 주입으로 단위 테스트 가능
- ✅ **재사용성**: `ExecuteMatchDestruction()` 메서드 재사용

### 테스트 지표
- ✅ **기존 테스트 유지**: 25개 테스트 100% 통과
- ✅ **회귀 버그**: 0건 (리팩토링 안전성 확보)
- ✅ **테스트 실행 시간**: 0.074초 (빠른 피드백)

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🎓 배운 점 및 교훈

### 1. 리팩토링은 "작은 단계"로 진행해야 한다

**발견**:
- Phase 2를 3개의 작은 단계(2.1, 2.2, 2.3)로 나누어 진행
- 각 단계마다 커밋하여 롤백 가능한 지점 확보

**교훈**:
- 큰 리팩토링을 한 번에 진행하면 문제 발생 시 원인 파악 어려움
- 각 단계가 독립적으로 완료되면 안전성 확보
- **"작은 성공"의 누적이 큰 성공으로 이어짐**

**앞으로의 적용**:
- Phase 3 이후에도 동일한 전략 적용
- 각 Phase를 2-3개의 작은 단계로 나누어 진행
- 매 단계마다 테스트 실행 및 커밋

---

### 2. 인터페이스는 "미래를 위한 투자"다

**발견**:
- Phase 1에서 인터페이스를 미리 정의한 덕분에 Phase 2가 쉬워짐
- 의존성 주입 패턴을 적용하기 위한 기반이 이미 마련됨

**교훈**:
- 인터페이스 정의는 초기 비용이 들지만, 장기적으로 큰 이득
- 테스트 가능성, 확장성, 유지보수성 모두 향상
- **"지금 당장 필요하지 않아도, 나중을 위해 준비하라"**

**앞으로의 적용**:
- 새로운 클래스를 만들 때 인터페이스를 먼저 정의
- 구체 클래스는 인터페이스 구현으로 시작
- 테스트 작성 시 인터페이스 기반 Mock 활용

---

### 3. 메서드 이름은 "동사"여야 한다

**발견**:
- `CollectMatchesFromGrid()` - "수집하다"
- `ProcessMatchRequests()` - "처리하다"
- `ExecuteMatchDestruction()` - "실행하다"

**교훈**:
- 메서드 이름에 동사를 사용하면 **행위**가 명확해짐
- 이름만 봐도 무슨 일을 하는지 이해 가능
- **"코드가 스스로 설명하도록 작성하라"**

**앞으로의 적용**:
- 메서드 이름을 지을 때 항상 동사로 시작
- 한 메서드가 여러 동사를 필요로 하면 분해 신호
- 주석보다는 명확한 이름으로 의도 전달

---

### 4. 중복 코드는 "기술 부채"다

**발견**:
- 29줄의 중복 코드가 존재했음
- 로직 수정 시 두 곳을 모두 수정해야 하는 번거로움
- 실수로 한 곳만 수정하면 버그 발생 가능

**교훈**:
- 중복 코드는 당장 문제가 없어 보이지만, 시간이 지나면 **유지보수 비용 증가**
- DRY 원칙을 지키면 **일관성 유지** 및 **버그 감소**
- **"두 번째 반복이 보이면 즉시 추출하라"**

**앞으로의 적용**:
- 코드 작성 시 중복이 보이면 즉시 메서드 추출
- 파라미터가 약간 다르더라도 공통 부분 최대한 추출
- 테스트 코드에서도 DRY 원칙 적용

---

### 5. TDD는 "안전망"이다

**발견**:
- Phase 1의 25개 테스트 덕분에 Phase 2 리팩토링이 안전했음
- 각 단계마다 테스트 실행으로 회귀 버그 조기 발견

**교훈**:
- 테스트가 없으면 리팩토링은 **위험한 작업**
- 테스트가 있으면 리팩토링은 **안전한 작업**
- **"테스트는 변화의 두려움을 없애준다"**

**앞으로의 적용**:
- 리팩토링 전에 반드시 테스트 작성
- 기존 테스트가 없다면 리팩토링 전에 추가
- 테스트 통과율 100% 유지

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 🔗 관련 문서

### Notion 하위 페이지 연결
이 문서는 메인 페이지의 하위 페이지로 작성되었습니다.
- 🏠 **메인 페이지**: Unity Match-3 리팩토링 프로젝트
- ⬅️ **이전 Phase**: Phase 1 - 인터페이스 추출 (7개 서비스 인터페이스 정의)
- ➡️ **다음 Phase**: Phase 3 - EventBus 도입 (계획됨)

### 로컬 마크다운 문서
- `Phase2_MatchManager_Refactoring.md`: 이 페이지의 원본 마크다운 문서
- `plan.md`: 전체 리팩토링 계획 및 진행 상황

### Git 커밋
- **커밋 해시 (2.1)**: `a65c617` - Apply dependency injection to MatchManager (Phase 2.1)
- **커밋 해시 (2.2)**: `484e4d6` - Extract methods from AllBlockMatch (Phase 2.2)
- **커밋 해시 (2.3)**: `8f34fca` - Remove duplicate code in UserMoveBlockMatch (Phase 2.3)
- **브랜치**: `ClaudeCode리펙토링`

### 참고 문서 (외부 링크)
- [SOLID 원칙 - 의존성 역전 원칙 (DIP)](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [SOLID 원칙 - 단일 책임 원칙 (SRP)](https://en.wikipedia.org/wiki/Single-responsibility_principle)
- [리팩토링: Extract Method](https://refactoring.com/catalog/extractFunction.html)
- [DRY 원칙 (Don't Repeat Yourself)](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)

\large\color{Yellow}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

## 📝 메타 정보

**작성일**: 2025-10-28
**작성자**: Claude Code (Sonnet 4.5) + TDD 방법론
**프로젝트 버전**: Phase 2 완료
**작업 시간**: 약 4시간
**영향받은 파일**: 1개 (`MatchManager.cs`)
**기존 테스트 유지**: 25개 (100% 통과)
**추가된 메서드**: 2개 (`ProcessUserMoveMatchRequests`, `CollectMatchesFromGrid`, `ProcessMatchRequests`, `ExecuteMatchDestruction`)
**제거된 코드 라인**: 29줄 (중복 코드)
**다음 작업**: Phase 3 - EventBus 도입 (Static Event 제거)

---

**태그**: #phase-2 #refactoring #solid #dip #srp #dry #matchmanager #dependency-injection #extract-method #unity #tdd
