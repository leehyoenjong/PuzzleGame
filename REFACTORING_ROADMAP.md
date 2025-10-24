# Refactoring Roadmap - Phase by Phase

This document provides a structured plan to refactor the architecture from highly-coupled to loosely-coupled, improving testability and team collaboration.

---

## Current State vs. Target State

### Current Architecture Problems
```
UI_Match_Block (182 lines)
  ├─ Handles input (Event_Point_Down, Event_Point_Enter)
  ├─ Manages grid position (_x, _y)
  ├─ Stores block type and color
  ├─ Manages animation state
  └─ Triggers match detection events

Result: Cannot test grid logic without UI components
```

### Target Architecture
```
BlockData (Data Model)
  └─ Position, Type, Color (immutable data)

IBlockView (Interface)
  └─ UI_Match_Block (implements)
     └─ Only handles rendering and input delegation

MatchService (Pure Logic)
  ├─ Detects matches
  ├─ Classifies types
  ├─ Creates special blocks
  └─ Can be tested without any UI

Game Coordinator
  └─ Orchestrates services via dependency injection
```

---

## PHASE 1: Extract Testable Interfaces (Week 1)

### Goal
Create interfaces that decouple managers from concrete implementations.

### Step 1.1: Create Service Interfaces

**File:** `/Assets/01_Script/00_Common/IServices.cs` (NEW FILE)

```csharp
/// <summary>
/// Services that can be mocked for testing
/// </summary>

public interface IMatchDetector
{
    List<(int, int)> DetectHorizontalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition);
    List<(int, int)> DetectVerticalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition);
    CrossMatchResult? DetectCrossMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) centerposition);
}

public interface IMatchTypeClassifier
{
    EMATCHTYPE ClassifyMatchType(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list);
}

public interface ISpecialBlockFactory
{
    SpecialBlockCreationRequest? CreateRequest(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list, EMATCHTYPE matchtype, UI_Match_Block usermoveblock);
}

public interface IChainReactionProcessor
{
    List<UI_Match_Block> ProcessChainReaction(List<UI_Match_Block> blocks, Dictionary<(int, int), UI_Match_Block> grid);
}

public interface IGridManager
{
    void Initialize(int width, int height);
    void SetBlock((int, int) position, UI_Match_Block block);
    UI_Match_Block GetBlock((int, int) position);
    bool HasBlock((int, int) position);
    Dictionary<(int, int), UI_Match_Block> GetGridDictionary();
    int Width { get; }
    int Height { get; }
}
```

### Step 1.2: Make Existing Classes Implement Interfaces

**File:** `/Assets/01_Script/01_Core/MatchDetector.cs`

```csharp
// Add interface implementation
public class MatchDetector : IMatchDetector
{
    // Existing code unchanged
}
```

Repeat for:
- `MatchTypeClassifier : IMatchTypeClassifier`
- `SpecialBlockFactory : ISpecialBlockFactory`
- `ChainReactionProcessor : IChainReactionProcessor`
- `GridManager : IGridManager`

**Changes:** Add `: IInterfaceName` to class declaration. No logic changes needed.

---

## PHASE 2: Refactor MatchManager for Testability (Week 2)

### Goal
Extract dependencies, make MatchManager injectable and testable.

### Step 2.1: Update MatchManager Constructor

**Current Code** (Awake method):
```csharp
void Awake()
{
    _matchdetector = new MatchDetector();
    _matchtypeclassifier = new MatchTypeClassifier();
    _specialblockfactory = new SpecialBlockFactory();
    _chainreactionprocessor = new ChainReactionProcessor();
    _movematchvalidator = new MoveMatchValidator(_matchdetector);
    _blockswaphandler = new BlockSwapHandler();
}
```

**New Code:**
```csharp
public class MatchManager : MonoBehaviour
{
    // Declare dependencies as interfaces
    private IMatchDetector _matchdetector;
    private IMatchTypeClassifier _matchtypeclassifier;
    private ISpecialBlockFactory _specialblockfactory;
    private IChainReactionProcessor _chainreactionprocessor;
    private MoveMatchValidator _movematchvalidator;
    private BlockSwapHandler _blockswaphandler;

    // Constructor for dependency injection
    public MatchManager(
        IMatchDetector matchDetector,
        IMatchTypeClassifier matchTypeClassifier,
        ISpecialBlockFactory specialBlockFactory,
        IChainReactionProcessor chainReactionProcessor,
        MoveMatchValidator moveMatchValidator,
        BlockSwapHandler blockSwapHandler)
    {
        _matchdetector = matchDetector;
        _matchtypeclassifier = matchTypeClassifier;
        _specialblockfactory = specialBlockFactory;
        _chainreactionprocessor = chainReactionProcessor;
        _movematchvalidator = moveMatchValidator;
        _blockswaphandler = blockSwapHandler;
    }

    // Factory method for Unity initialization
    public static MatchManager CreateDefault()
    {
        return new MatchManager(
            new MatchDetector(),
            new MatchTypeClassifier(),
            new SpecialBlockFactory(),
            new ChainReactionProcessor(),
            new MoveMatchValidator(new MatchDetector()),
            new BlockSwapHandler());
    }

    void Awake()
    {
        // For now, call factory if no dependencies injected
        if (_matchdetector == null)
        {
            var defaultInstance = CreateDefault();
            _matchdetector = defaultInstance._matchdetector;
            _matchtypeclassifier = defaultInstance._matchtypeclassifier;
            // ... copy all fields
        }
    }
}
```

### Step 2.2: Extract Duplicate Match Detection Logic

**Current Problem:** Lines 138-148 and 237-248 are duplicated

**New Private Method:**
```csharp
private List<SpecialBlockCreationRequest> CreateSpecialBlockRequests(
    (List<UI_Match_Block> x, List<UI_Match_Block> y) matchresult,
    UI_Match_Block triggerBlock = null)
{
    var creationRequests = new List<SpecialBlockCreationRequest>();

    var matchtype = _matchtypeclassifier.ClassifyMatchType(
        matchresult.x,
        matchresult.y);

    var request = _specialblockfactory.CreateRequest(
        matchresult.x,
        matchresult.y,
        matchtype,
        triggerBlock);

    if (request.HasValue)
        creationRequests.Add(request.Value);

    return creationRequests;
}
```

**Usage in AllBlockMatch (Line 138-148):**
```csharp
// Old: 11 lines of code
var matchtype = _matchtypeclassifier.ClassifyMatchType(matchresult.matchblocklist_x, matchresult.matchblocklist_y);
var creationrequest = _specialblockfactory.CreateRequest(
    matchresult.matchblocklist_x,
    matchresult.matchblocklist_y,
    matchtype,
    usermoveblock: null);
if (creationrequest.HasValue)
{
    creationRequests.Add(creationrequest.Value);
}

// New: 2 lines of code
var reqs = CreateSpecialBlockRequests(
    (matchresult.matchblocklist_x, matchresult.matchblocklist_y));
creationRequests.AddRange(reqs);
```

### Step 2.3: Test MatchManager in Isolation

**New Test File:** `/Assets/01_Script/Tests/MatchManagerIsolationTests.cs`

```csharp
[TestFixture]
public class MatchManagerIsolationTests
{
    private MatchManager _matchManager;
    private Mock<IMatchDetector> _mockMatchDetector;
    private Mock<IMatchTypeClassifier> _mockTypeClassifier;
    private Mock<ISpecialBlockFactory> _mockFactory;
    private Mock<IChainReactionProcessor> _mockChainProcessor;

    [SetUp]
    public void SetUp()
    {
        // Create mocks
        _mockMatchDetector = new Mock<IMatchDetector>();
        _mockTypeClassifier = new Mock<IMatchTypeClassifier>();
        _mockFactory = new Mock<ISpecialBlockFactory>();
        _mockChainProcessor = new Mock<IChainReactionProcessor>();

        // Inject into manager
        _matchManager = new MatchManager(
            _mockMatchDetector.Object,
            _mockTypeClassifier.Object,
            _mockFactory.Object,
            _mockChainProcessor.Object,
            new MoveMatchValidator(_mockMatchDetector.Object),
            new BlockSwapHandler());
    }

    [Test]
    public void AllBlockMatch_WithFourHorizontalMatch_ShouldCreateLineBreaker()
    {
        // Arrange
        var grid = new Dictionary<(int, int), UI_Match_Block>();
        // ... set up 4 horizontal blocks

        var matchResult = (
            x: new List<UI_Match_Block> { block1, block2, block3, block4 },
            y: new List<UI_Match_Block>());

        _mockMatchDetector
            .Setup(x => x.DetectHorizontalMatch(It.IsAny<Dictionary<(int, int), UI_Match_Block>>(), It.IsAny<(int, int)>()))
            .Returns(new List<(int, int)> { (0,0), (1,0), (2,0), (3,0) });

        _mockTypeClassifier
            .Setup(x => x.ClassifyMatchType(It.IsAny<List<UI_Match_Block>>(), It.IsAny<List<UI_Match_Block>>()))
            .Returns(EMATCHTYPE.FORE_LEFTRIGHT);

        var request = new SpecialBlockCreationRequest
        {
            Point = (1, 0),
            Type = EMATCHTYPE.FORE_LEFTRIGHT,
            Color = EBLOCKCOLORTYPE.RED
        };

        _mockFactory
            .Setup(x => x.CreateRequest(It.IsAny<List<UI_Match_Block>>(), It.IsAny<List<UI_Match_Block>>(), It.IsAny<EMATCHTYPE>(), It.IsAny<UI_Match_Block>()))
            .Returns(request);

        // Act
        var result = _matchManager.AllBlockMatch(grid, 5, 5);

        // Assert
        Assert.IsTrue(result);
        _mockFactory.Verify(
            x => x.CreateRequest(It.IsAny<List<UI_Match_Block>>(), It.IsAny<List<UI_Match_Block>>(), EMATCHTYPE.FORE_LEFTRIGHT, null),
            Times.Once);
    }
}
```

---

## PHASE 3: Replace Static Events with Event Bus (Week 2-3)

### Goal
Centralize event management, eliminate 26+ static events scattered across codebase.

### Step 3.1: Create Event Bus Interface

**File:** `/Assets/01_Script/00_Common/IEventBus.cs` (NEW FILE)

```csharp
/// <summary>
/// Centralized event management to replace 26+ static events
/// </summary>
public interface IEventBus
{
    void Subscribe<T>(string eventName, Action<T> handler);
    void Unsubscribe<T>(string eventName, Action<T> handler);
    void Publish<T>(string eventName, T data);

    void Subscribe(string eventName, Action handler);
    void Unsubscribe(string eventName, Action handler);
    void Publish(string eventName);
}

/// <summary>
/// Default implementation using C# events internally
/// </summary>
public class EventBus : IEventBus
{
    private Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();

    public void Subscribe<T>(string eventName, Action<T> handler)
    {
        if (!_events.ContainsKey(eventName))
            _events[eventName] = handler;
        else
            _events[eventName] = Delegate.Combine(_events[eventName], (Delegate)handler);
    }

    public void Unsubscribe<T>(string eventName, Action<T> handler)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName] = Delegate.Remove(_events[eventName], (Delegate)handler);
            if (_events[eventName] == null)
                _events.Remove(eventName);
        }
    }

    public void Publish<T>(string eventName, T data)
    {
        if (_events.TryGetValue(eventName, out var handlers))
        {
            (handlers as Action<T>)?.Invoke(data);
        }
    }

    public void Subscribe(string eventName, Action handler)
    {
        if (!_events.ContainsKey(eventName))
            _events[eventName] = handler;
        else
            _events[eventName] = Delegate.Combine(_events[eventName], (Delegate)handler);
    }

    public void Unsubscribe(string eventName, Action handler)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName] = Delegate.Remove(_events[eventName], (Delegate)handler);
            if (_events[eventName] == null)
                _events.Remove(eventName);
        }
    }

    public void Publish(string eventName)
    {
        if (_events.TryGetValue(eventName, out var handlers))
        {
            (handlers as Action)?.Invoke();
        }
    }
}

/// <summary>
/// Event names - centralized instead of scattered across managers
/// </summary>
public static class GameEvents
{
    // User Input
    public const string BLOCK_POINT_DOWN = "Block.PointDown";
    public const string BLOCK_POINT_UP = "Block.PointUp";
    public const string BLOCK_POINT_ENTER = "Block.PointEnter";

    // Matching
    public const string MATCH_COMPLETE_BLOCK = "Match.CompleteBlock";
    public const string MATCH_COMPLETE_CREATE_BLOCK = "Match.CompleteCreateBlock";
    public const string USER_MOVE_MATCH_COMPLETE = "UserMove.MatchComplete";

    // Game State
    public const string CHECK_CLEAR_CONDITION = "Game.CheckClearCondition";
    public const string CHECK_OVER_CONDITION = "Game.CheckOverCondition";
    public const string GAME_CLEAR = "Game.Clear";
    public const string GAME_OVER = "Game.Over";

    // Field
    public const string BLOCK_MOVE = "Block.Move";
    public const string MATCH_COMPLETE = "Match.Complete";
    public const string MATCH_SIMULATION_CHECK = "Match.SimulationCheck";
    public const string NO_MATCH_BLOCK = "Field.NoMatchBlock";
    public const string REPLAY_COMPLETE = "Field.ReplayComplete";

    // Scoring
    public const string ADD_SCORE = "Score.Add";
    public const string MOVE_COUNT_CHANGE = "MoveCount.Change";
}
```

### Step 3.2: Migrate BlockControllerManager to Event Bus

**Current Code:**
```csharp
public class BlockControllerManager : MonoBehaviour
{
    void OnEnable()
    {
        UI_Match_Block._point_down_event += PointDown;
        UI_Match_Block._point_enter_event += PointEnter;
        UI_Match_Block._point_up_event += PointUp;
    }

    void OnDisable()
    {
        UI_Match_Block._point_down_event -= PointDown;
        UI_Match_Block._point_enter_event -= PointEnter;
        UI_Match_Block._point_up_event -= PointUp;
        _block_controller_check_list.Clear();
    }
}
```

**New Code with Event Bus:**
```csharp
public class BlockControllerManager : MonoBehaviour
{
    private IEventBus _eventBus;

    public BlockControllerManager(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    void OnEnable()
    {
        _eventBus.Subscribe<UI_Match_Block>(GameEvents.BLOCK_POINT_DOWN, PointDown);
        _eventBus.Subscribe<UI_Match_Block>(GameEvents.BLOCK_POINT_ENTER, PointEnter);
        _eventBus.Subscribe(GameEvents.BLOCK_POINT_UP, PointUp);
    }

    void OnDisable()
    {
        _eventBus.Unsubscribe<UI_Match_Block>(GameEvents.BLOCK_POINT_DOWN, PointDown);
        _eventBus.Unsubscribe<UI_Match_Block>(GameEvents.BLOCK_POINT_ENTER, PointEnter);
        _eventBus.Unsubscribe(GameEvents.BLOCK_POINT_UP, PointUp);
    }
}
```

**Benefits:**
- No more static events ✓
- Automatic cleanup with Unsubscribe ✓
- Easier to test: can pass mock EventBus ✓
- Centralized event names: no typos ✓
- No memory leaks from forgotten -= ✓

### Step 3.3: Migration Order

Migrate in this order (lowest dependencies first):
1. BlockControllerManager (only UI_Match_Block -> MatchFiledManager)
2. ScoreManager (only UI_Match_Block)
3. MoveCountManager (only MatchManager)
4. UI_Match_Block (publish events)
5. MatchManager (complex, many events)
6. MatchFiledManager (complex, many dependencies)
7. GameManager (depends on most)

---

## PHASE 4: Extract UI from Business Logic (Week 3-4)

### Goal
Separate UI rendering from game logic.

### Step 4.1: Create BlockData Model

**File:** `/Assets/01_Script/00_Common/BlockData.cs` (NEW FILE)

```csharp
/// <summary>
/// Pure data model for a block (no UI, no MonoBehaviour)
/// </summary>
public class BlockData
{
    public int X { get; set; }
    public int Y { get; set; }
    public EMATCHTYPE Type { get; }
    public EBLOCKCOLORTYPE Color { get; set; }
    public int Score { get; }

    public BlockData(int x, int y, EMATCHTYPE type, EBLOCKCOLORTYPE color, int score)
    {
        X = x;
        Y = y;
        Type = type;
        Color = color;
        Score = score;
    }

    public BlockData WithPosition(int x, int y) =>
        new BlockData(x, y, Type, Color, Score);

    public BlockData WithColor(EBLOCKCOLORTYPE newColor) =>
        new BlockData(X, Y, Type, newColor, Score);
}
```

### Step 4.2: Create IBlockView Interface

**File:** `/Assets/01_Script/00_Common/IBlockView.cs` (NEW FILE)

```csharp
/// <summary>
/// Interface that any block view must implement
/// Decouples game logic from UI representation
/// </summary>
public interface IBlockView
{
    Vector2 Position { get; set; }
    BlockData Data { get; set; }

    void AnimateToPosition(Vector2 targetPosition, float duration);
    void AnimateDestroy();
    void AnimateCreate();
}
```

### Step 4.3: Refactor UI_Match_Block to Implement IBlockView

**File:** `/Assets/01_Script/03_UI/02_UI_Play/UI_Match_Block.cs`

```csharp
public class UI_Match_Block : MonoBehaviour, IBlockView
{
    private BlockData _blockData;

    public BlockData Data
    {
        get => _blockData;
        set => _blockData = value;
    }

    public Vector2 Position
    {
        get => _rt.anchoredPosition;
        set => _rt.anchoredPosition = value;
    }

    public void AnimateToPosition(Vector2 targetPosition, float duration)
    {
        _movecontroller.MoveTo(targetPosition);
    }

    public void AnimateDestroy()
    {
        DisableAni();
    }

    public void AnimateCreate()
    {
        ActiveAni();
    }

    // Keep existing methods, just add interface implementation
}
```

### Step 4.4: Refactor PoolSystem to Use IBlockView

**Current:**
```csharp
public class PoolSystem_Block : MonoBehaviour
{
    UI_Match_Block CreateBlock(EMATCHTYPE blocktypes, Transform parent)
    {
        // Returns UI_Match_Block directly
    }
}
```

**New:**
```csharp
public interface IBlockViewFactory
{
    IBlockView CreateBlock(EMATCHTYPE blockType, Transform parent);
}

public class PoolSystem_Block : MonoBehaviour, IBlockViewFactory
{
    public IBlockView CreateBlock(EMATCHTYPE blocktypes, Transform parent)
    {
        // Returns IBlockView (still UI_Match_Block internally)
        return CreateBlockInternal(blocktypes, parent) as IBlockView;
    }

    private UI_Match_Block CreateBlockInternal(EMATCHTYPE blocktypes, Transform parent)
    {
        // Existing logic
    }
}
```

---

## PHASE 5: Dependency Injection Container (Week 4-5)

### Goal
Initialize all dependencies properly, making system composable.

### Step 5.1: Create Service Container

**File:** `/Assets/01_Script/00_Common/ServiceContainer.cs` (NEW FILE)

```csharp
/// <summary>
/// Simple dependency injection container
/// Initializes all game services with proper dependencies
/// </summary>
public class ServiceContainer
{
    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public void Register<T>(T instance) where T : class
    {
        _services[typeof(T)] = instance;
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var instance))
            return instance as T;

        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    public static ServiceContainer CreateDefault()
    {
        var container = new ServiceContainer();

        // Register core services
        var eventBus = new EventBus();
        container.Register<IEventBus>(eventBus);

        var matchDetector = new MatchDetector();
        container.Register<IMatchDetector>(matchDetector);

        var typeClassifier = new MatchTypeClassifier();
        container.Register<IMatchTypeClassifier>(typeClassifier);

        var specialBlockFactory = new SpecialBlockFactory();
        container.Register<ISpecialBlockFactory>(specialBlockFactory);

        var chainReactionProcessor = new ChainReactionProcessor();
        container.Register<IChainReactionProcessor>(chainReactionProcessor);

        var gridManager = new GridManager();
        container.Register<IGridManager>(gridManager);

        // Register managers
        var matchManager = new MatchManager(
            matchDetector,
            typeClassifier,
            specialBlockFactory,
            chainReactionProcessor,
            new MoveMatchValidator(matchDetector),
            new BlockSwapHandler());
        container.Register(matchManager);

        var blockControllerManager = new BlockControllerManager(eventBus);
        container.Register(blockControllerManager);

        return container;
    }
}
```

### Step 5.2: Initialize in Bootstrap MonoBehaviour

**File:** `/Assets/01_Script/00_Common/GameBootstrapper.cs` (NEW FILE)

```csharp
/// <summary>
/// Single entry point for game initialization
/// Responsible for setting up all services
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    public static ServiceContainer Container { get; private set; }

    void Awake()
    {
        // Create service container with all dependencies
        Container = ServiceContainer.CreateDefault();

        // Initialize DataManager
        var dataManager = FindObjectOfType<DataManager>();
        if (dataManager == null)
        {
            var go = new GameObject("DataManager");
            dataManager = go.AddComponent<DataManager>();
        }

        // Get managers from container and initialize
        var matchManager = Container.Get<MatchManager>();
        var blockControllerManager = Container.Get<BlockControllerManager>();

        // Everything is now initialized with proper dependencies!
    }
}
```

---

## PHASE 6: Testing Infrastructure (Week 5-6)

### Goal
Make codebase testable with fast, reliable unit tests.

### Step 6.1: Create Test Mocks

**File:** `/Assets/01_Script/Tests/Mocks/MockEventBus.cs` (NEW FILE)

```csharp
public class MockEventBus : IEventBus
{
    private Dictionary<string, List<Delegate>> _subscribers = new();

    public List<string> PublishedEvents { get; } = new();

    public void Subscribe<T>(string eventName, Action<T> handler)
    {
        if (!_subscribers.ContainsKey(eventName))
            _subscribers[eventName] = new List<Delegate>();
        _subscribers[eventName].Add(handler);
    }

    public void Unsubscribe<T>(string eventName, Action<T> handler)
    {
        if (_subscribers.TryGetValue(eventName, out var handlers))
            handlers.Remove(handler);
    }

    public void Publish<T>(string eventName, T data)
    {
        PublishedEvents.Add(eventName);
        if (_subscribers.TryGetValue(eventName, out var handlers))
        {
            foreach (var handler in handlers)
                (handler as Action<T>)?.Invoke(data);
        }
    }

    // ... Implement non-generic versions similarly
}
```

### Step 6.2: Example Testable Test

**File:** `/Assets/01_Script/Tests/BlockControllerManagerTests.cs`

```csharp
[TestFixture]
public class BlockControllerManagerTests
{
    private BlockControllerManager _manager;
    private MockEventBus _eventBus;

    [SetUp]
    public void SetUp()
    {
        _eventBus = new MockEventBus();
        _manager = new BlockControllerManager(_eventBus);
    }

    [Test]
    public void PointEnter_WithAdjacent_ShouldPublishMoveBlockEvent()
    {
        // Arrange
        var block1 = CreateMockBlock(x: 0, y: 0);
        var block2 = CreateMockBlock(x: 1, y: 0);

        // Act
        _manager.PointEnter(block2);

        // Assert
        Assert.That(_eventBus.PublishedEvents, Contains.Item(GameEvents.BLOCK_MOVE));
    }

    private IBlockView CreateMockBlock(int x, int y)
    {
        var mock = new Mock<IBlockView>();
        mock.Setup(b => b.Data).Returns(new BlockData(x, y, EMATCHTYPE.THREE, EBLOCKCOLORTYPE.RED, 10));
        return mock.Object;
    }
}
```

---

## Migration Checklist

Use this checklist to track progress:

### Phase 1: Interfaces (Week 1)
- [ ] Create IServices.cs with all interfaces
- [ ] Update MatchDetector to implement IMatchDetector
- [ ] Update MatchTypeClassifier to implement IMatchTypeClassifier
- [ ] Update SpecialBlockFactory to implement ISpecialBlockFactory
- [ ] Update ChainReactionProcessor to implement IChainReactionProcessor
- [ ] Update GridManager to implement IGridManager
- [ ] Run all tests - should pass (no logic changes)

### Phase 2: MatchManager Refactoring (Week 2)
- [ ] Add constructor to MatchManager with interface dependencies
- [ ] Create CreateDefault() factory method
- [ ] Update Awake() to use CreateDefault() as fallback
- [ ] Extract CreateSpecialBlockRequests() private method
- [ ] Remove duplicate match detection logic
- [ ] Write MatchManagerIsolationTests
- [ ] Run all tests - should pass

### Phase 3: Event Bus Migration (Week 2-3)
- [ ] Create IEventBus and EventBus implementation
- [ ] Create GameEvents constant class
- [ ] Migrate BlockControllerManager to use EventBus
- [ ] Migrate ScoreManager to use EventBus
- [ ] Migrate MoveCountManager to use EventBus
- [ ] Migrate UI_Match_Block to use EventBus
- [ ] Migrate MatchManager to use EventBus
- [ ] Migrate MatchFiledManager to use EventBus
- [ ] Migrate GameManager to use EventBus
- [ ] Run all tests - should pass
- [ ] Remove all static events from codebase

### Phase 4: UI Separation (Week 3-4)
- [ ] Create BlockData model class
- [ ] Create IBlockView interface
- [ ] Implement IBlockView in UI_Match_Block
- [ ] Create IBlockViewFactory interface
- [ ] Implement IBlockViewFactory in PoolSystem_Block
- [ ] Refactor MatchFiledManager to use IBlockView instead of UI_Match_Block
- [ ] Update GridManager to use IBlockView
- [ ] Run all tests - should pass

### Phase 5: Dependency Injection (Week 4-5)
- [ ] Create ServiceContainer class
- [ ] Create GameBootstrapper MonoBehaviour
- [ ] Update scene to use GameBootstrapper
- [ ] Remove all Manager instantiation from scene setup
- [ ] Update all Manager constructors to accept dependencies
- [ ] Run all tests - should pass
- [ ] Test game startup with new DI system

### Phase 6: Testing (Week 5-6)
- [ ] Create MockEventBus
- [ ] Create test doubles for all interfaces
- [ ] Write BlockControllerManagerTests
- [ ] Write MatchManagerTests
- [ ] Write MatchFiledManagerTests (integration)
- [ ] Write end-to-end game flow tests
- [ ] Achieve 70%+ code coverage
- [ ] All tests green

---

## Expected Improvements Post-Refactoring

| Metric | Before | After |
|--------|--------|-------|
| Static Events | 26+ | 0 |
| Manager Testability | 0% | 95%+ |
| Test Execution Time | 2+ minutes | <5 seconds |
| Code Duplication | High | Minimal |
| Circular Dependencies | 2+ | 0 |
| Test Coverage | 15% | 70%+ |
| Lines per Method (Max) | 93 | <20 |
| Memory Leaks | 2 confirmed | 0 |

---

## Commit Strategy

Make small, focused commits:

1. **feat: Add service interfaces (IMatchDetector, IGridManager, etc)**
2. **refactor: Make MatchDetector implement IMatchDetector (no logic change)**
3. **refactor: Extract MatchDetector dependency injection in MatchManager**
4. **feat: Add EventBus for centralized event management**
5. **refactor: Migrate BlockControllerManager to use EventBus**
6. **refactor: Migrate ScoreManager to use EventBus**
7. **refactor: Migrate remaining managers to use EventBus**
8. **feat: Create BlockData model for pure game logic**
9. **refactor: Implement IBlockView in UI_Match_Block**
10. **feat: Add ServiceContainer and GameBootstrapper**
11. **test: Add comprehensive test suite for migrated code**
12. **refactor: Remove all static events from codebase**

Each commit should:
- Have all tests passing
- Be reviewable (< 300 lines changed ideally)
- Have clear commit message
- Not mix refactoring with feature changes

---

