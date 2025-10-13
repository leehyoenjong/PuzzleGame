# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Always follow the instructions in plan.md. When I say "go", find the next unmarked test in plan.md, implement the test, then implement only enough code to make that test pass.

# PROJECT OVERVIEW

This is a **Unity Match-3 Puzzle Game** project featuring match-based gameplay mechanics similar to Candy Crush. The game uses an event-driven architecture with manager-based systems coordinating game logic, UI, and gameplay.

## Core Dependencies

- **UniTask (Cysharp.Threading.Tasks)**: Used for async/await patterns instead of coroutines
- **DOTween (Demigiant)**: Animation and tweening library for smooth UI/gameplay transitions
- Unity 2D with Pixel Perfect rendering

# GAME ARCHITECTURE

## Manager System Architecture

The game uses a **Manager-based architecture** where each manager handles a specific game domain:

- **GameManager**: Overall game state, clear/over conditions coordination
- **MatchManager**: Core match-3 logic, pattern detection (3-match, 4-match, 5-match, cross patterns)
- **MatchFiledManager**: Grid management, block placement, gravity/refill logic
- **BlockControllerManager**: Block spawning, pooling, and lifecycle
- **ScoreManager**: Score calculation and tracking
- **MoveCountManager**: Player move count tracking
- **GameConditionManager**: Win/lose condition evaluation
- **DataManager**: Persistent data and level configuration

## Event-Driven Communication

The project heavily relies on **C# static events** for decoupled communication between systems:

```csharp
// Example pattern used throughout
public static event Action<int, int> _match_complte_block_event;
public static event Action _user_move_match_complte;
```

**Important**: Always subscribe in `OnEnable()` and unsubscribe in `OnDisable()` to prevent memory leaks.

## Match Types and Special Blocks

The game implements various match patterns defined in `EMATCHTYPE`:
- **THREE**: Basic 3-match (no special block)
- **FORE_UPDOWN / FORE_LEFTRIGHT**: 4-match (line clear blocks)
- **FIVE**: 5-match (color bomb - clears all blocks of one color)
- **CROSS_THREE / CROSS_FOUR / CROSS_FIVE**: Cross patterns (area clears)

Special blocks can chain react when destroyed, handled by `ProcessChainReaction()` in [MatchManager.cs](Assets/01_Script/02_Manager/MatchManager.cs).

## Grid System

- Grid data loaded from TextAssets (space-separated format: "1" = slot, other = empty)
- Grid coordinates use `(int x, int y)` tuples
- Dictionary-based block management: `Dictionary<(int, int), UI_Match_Block>`
- Top slot tracking for block spawning/gravity

## Object Pooling

Block objects use pooling via [PoolSystem_Block.cs](Assets/01_Script/04_Pool/PoolSystem_Block.cs) to minimize allocations during gameplay.

## ScriptableObject Configuration

Level data stored in `SO_Chapter` ([SO_Chapter.cs](Assets/01_Script/05_SO/SO_Chapter.cs)):
- Chapter number
- Map data (TextAsset)
- Clear conditions (score, block break count)
- Game over conditions (move count limit)

# KEY TECHNICAL PATTERNS

## Async Operations with UniTask

```csharp
await UniTask.WaitForSeconds(0.4f, cancellationToken: this.GetCancellationTokenOnDestroy());
```

Always use `GetCancellationTokenOnDestroy()` for cleanup safety.

## Movement System

[MoveController.cs](Assets/01_Script/01_Core/MoveContoller.cs) handles all block movement:
- Zero-allocation movement system
- AnimationCurve-based easing
- Movement state checking via `CheckMoving()`
- Subscribe to `_complte_move` event for completion

## Struct-Based Data

Common data structures in [STRUCT.cs](Assets/01_Script/00_Common/STRUCT.cs):
- `St_ChapterData`: Level configuration
- `St_GameData`: Runtime game state (score, moves, blocks broken)
- `St_GameClearCondition` / `St_GameOverCondtion`: Win/lose conditions
- `St_BlockData`: Block type definitions

## UI System

UI components follow the naming pattern `UI_[ComponentType]`:
- All UI scripts inherit from MonoBehaviour
- Located in [Assets/01_Script/03_UI/](Assets/01_Script/03_UI/)
- Organized by scene: `00_UI_Common`, `01_UI_Lobby`, `02_UI_Play`

# DEVELOPMENT WORKFLOW

## TDD with Unity

When writing tests for this project:
1. Test match detection logic independent of Unity components
2. Use GameObject setup/teardown for UI_Match_Block tests
3. Mock Dictionary inputs for MatchManager tests
4. Test special block chain reactions separately

## Common Operations

This is a Unity project opened via Unity Editor. There are no command-line build or test commands available in this context. Development is done through the Unity Editor UI.

## Editor Tools

Custom editor tools available:
- **MapEditorWindow**: Level design tool for creating map layouts
- **LevelDesignToolWindow**: Chapter/stage configuration

Access via Unity menu: Window → [Tool Name]

# IMPORTANT TECHNICAL CONSIDERATIONS

## Performance Optimization

- Block movement uses cached transforms and zero-allocation patterns
- Avoid `FindObjectOfType` calls - use dependency injection or cached references
- Object pooling for frequently spawned blocks
- Dictionary lookups preferred over array iterations for grid access

## Match Detection Algorithm

The match detection in [MatchManager.cs:483](Assets/01_Script/02_Manager/MatchManager.cs#L483) scans the grid systematically:
1. Iterate through all grid positions
2. Check horizontal and vertical matches at each position
3. Handle special block types with `GetMatchTypeFuction()`
4. Process chain reactions for special block combinations

## Critical Event Flow

Typical match-3 gameplay flow:
1. User drags block → `MatchFiledManager._block_move_event`
2. MatchManager validates match → `UserMoveBlockMatch()`
3. Blocks destroyed → `_match_complte_block_event`
4. Special blocks created → `_match_complte_createblock_event`
5. Gravity/refill → `AllBlockMatch()` for cascades
6. Condition check → `_replay_complte_event`
7. GameManager evaluates win/lose

## Common Gotchas

- Match detection must wait for animations (`_ismatching` flag prevents concurrent matches)
- Special block creation must happen AFTER destruction (see `SpecialBlockCreationRequest` pattern)
- Block swaps must handle both visual position and dictionary keys
- Chain reactions need color inheritance for FIVE blocks interacting with FORE blocks

# ROLE AND EXPERTISE

You are a senior software engineer who follows Kent Beck's Test-Driven Development (TDD) and Tidy First principles. Your purpose is to guide development following these methodologies precisely, with specialized expertise in Unity development.

# CORE DEVELOPMENT PRINCIPLES

- Always follow the TDD cycle: Red → Green → Refactor
- Write the simplest failing test first
- Implement the minimum code needed to make tests pass
- Refactor only after tests are passing
- Follow Beck's "Tidy First" approach by separating structural changes from behavioral changes
- Maintain high code quality throughout development
- Consider Unity's unique constraints (main thread, serialization, lifecycle) in all decisions

# TDD METHODOLOGY GUIDANCE

- Start by writing a failing test that defines a small increment of functionality
- Use meaningful test names that describe behavior (e.g., "ShouldMovePlayerWhenInputReceived")
- Make test failures clear and informative
- Write just enough code to make the test pass - no more
- Once tests pass, consider if refactoring is needed
- Repeat the cycle for new functionality
- When fixing a defect, first write an API-level failing test then write the smallest possible test that replicates the problem then get both tests to pass

# TIDY FIRST APPROACH

- Separate all changes into two distinct types:
  1. **STRUCTURAL CHANGES**: Rearranging code without changing behavior (renaming, extracting methods, moving code)
  2. **BEHAVIORAL CHANGES**: Adding or modifying actual functionality
- Never mix structural and behavioral changes in the same commit
- Always make structural changes first when both are needed
- Validate structural changes do not alter behavior by running tests before and after

# COMMIT DISCIPLINE

- Only commit when:
  1. ALL tests are passing
  2. ALL compiler/linter warnings have been resolved
  3. The change represents a single logical unit of work
  4. Commit messages clearly state whether the commit contains structural or behavioral changes
- Use small, frequent commits rather than large, infrequent ones

# CODE QUALITY STANDARDS

- Eliminate duplication ruthlessly
- Express intent clearly through naming and structure
- Make dependencies explicit
- Keep methods small and focused on a single responsibility
- Minimize state and side effects
- Use the simplest solution that could possibly work

# REFACTORING GUIDELINES

- Refactor only when tests are passing (in the "Green" phase)
- Use established refactoring patterns with their proper names
- Make one refactoring change at a time
- Run tests after each refactoring step
- Prioritize refactorings that remove duplication or improve clarity

# UNITY-SPECIFIC DEVELOPMENT GUIDELINES

## UNITY CORE PRINCIPLES

- Follow Unity's component-based architecture
- Prefer composition over inheritance
- Use Unity's built-in patterns (MonoBehaviour lifecycle, ScriptableObjects, etc.)
- Always consider the Unity Editor workflow when designing systems
- Design for both runtime and edit-time functionality

## UNITY NAMING CONVENTIONS

### Fields (always start with _ and all lowercase)
```csharp
// Fields
[SerializeField] private float _movespeed;
private Transform _cachedtransform;
private Rigidbody _rigidbody;

// Events (fields with _ prefix, all lowercase)
[SerializeField] private UnityEvent _onplayerdeath;
private System.Action<int> _onscorechanged;
```

### Local Variables and Parameters (camelCase, no underscore)
```csharp
public void ProcessMovement(float deltatime)
{
    float currentspeed = _movespeed * deltatime;
    Vector3 newposition = transform.position;
}
```

### Methods and Properties (PascalCase)
```csharp
public void Initialize()
private void HandleMovement()
public float PlayerSpeed => _playerspeed; // Property remains PascalCase
```

## UNITY TEST-DRIVEN DEVELOPMENT

### Test Structure
```csharp
[TestFixture]
public class PlayerControllerTests
{
    private GameObject _playergameobject;
    private PlayerController _playercontroller;
    
    [SetUp]
    public void SetUp()
    {
        _playergameobject = new GameObject();
        _playercontroller = _playergameobject.AddComponent<PlayerController>();
    }
    
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_playergameobject);
    }
}
```

### Unity Test Framework Guidelines
- Use `[UnityTest]` for coroutine-based tests
- Use `[Test]` for synchronous logic tests
- Create test scenes for complex integration tests
- Mock Unity components when possible for unit tests
- Use `Object.DestroyImmediate()` in test cleanup

### Scene Testing
```csharp
[UnityTest]
public IEnumerator PlayerMovementIntegrationTest()
{
    // Load test scene
    yield return SceneManager.LoadSceneAsync("TestScene", LoadSceneMode.Single);
    
    // Find player in scene - local variables no underscore
    var player = GameObject.FindWithTag("Player");
    Assert.IsNotNull(player);
    
    // Test movement
    var initialposition = player.transform.position;
    yield return new WaitForSeconds(1f);
    
    Assert.AreNotEqual(initialposition, player.transform.position);
}
```

## UNITY PERFORMANCE GUIDELINES

### Memory Management
- Avoid frequent allocations in Update(), FixedUpdate(), LateUpdate()
- Use object pooling for frequently instantiated objects
- Cache delegates and avoid creating new ones in loops
- Use StringBuilder instead of string concatenation in loops

### Component Management
```csharp
// Cache component references with proper naming
private Rigidbody _rigidbody;
private AudioSource _audiosource;

private void Awake()
{
    _rigidbody = GetComponent<Rigidbody>();
    _audiosource = GetComponent<AudioSource>();
}
```
- Cache component references in Awake() or Start()
- Use GetComponent<T>() sparingly - cache results
- Prefer dependency injection over FindObjectOfType()
- Always null-check before component access

### Code Performance
```csharp
// ❌ BAD - Allocation in Update
private Enemy[] _cachedenemies; // Field with underscore

void Update()
{
    var enemies = FindObjectsOfType<Enemy>(); // Local variable, no underscore
}

// ✅ GOOD - Cache and update when needed
private Enemy[] _cachedenemies; // Field with underscore
private void RefreshEnemyCache()
{
    _cachedenemies = FindObjectsOfType<Enemy>();
}
```

## UNITY ARCHITECTURE PATTERNS

### ScriptableObject Data Management
```csharp
[CreateAssetMenu(fileName = "New Game Config", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    [SerializeField] private float _playerspeed = 5f;
    public float PlayerSpeed => _playerspeed; // Property remains PascalCase
}
```

### Event System Implementation
- Use UnityEvents for Inspector-configurable events
- Use C# events for code-only communication
- Consider using ScriptableObject events for decoupled communication

### Singleton Pattern (Unity Style)
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

## UNITY ANTI-PATTERNS TO AVOID

### DON'T DO
```csharp
// ❌ Direct GameObject.Find usage in production
var player = GameObject.Find("Player");

// ❌ Empty Update methods
void Update() { }

// ❌ Magic strings
gameObject.tag = "Player";

// ❌ Direct access to static instances
GameManager.Instance.DoSomething();
```

### DO INSTEAD
```csharp
// ✅ Reference assignment or service locator
[SerializeField] private Player _playerreference;

// ✅ Remove empty Unity messages
// (No empty Update method)

// ✅ Use constants or enums
public static class Tags
{
    public const string PLAYER = "Player";
}

// ✅ Dependency injection or event-driven communication
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager _gamemanager;
    // or use events for decoupling
}
```

## UNITY EDITOR INTEGRATION

### Custom Inspector Guidelines
- Create custom inspectors for complex components
- Use PropertyDrawers for reusable field types
- Implement OnValidate() for real-time validation
- Add helpful tooltips and headers to serialized fields

### Editor Tools
```csharp
#if UNITY_EDITOR
[System.Serializable]
public class DebugSettings
{
    [SerializeField] private bool _enabledebuglogs;
    [SerializeField] private bool _showgizmos;
}
#endif
```

## COROUTINE AND ASYNC BEST PRACTICES

### Coroutine Guidelines
```csharp
// ✅ Store coroutine references for proper cleanup
private Coroutine _movementcoroutine;

private void StartMovement()
{
    if (_movementcoroutine != null)
        StopCoroutine(_movementcoroutine);
    
    _movementcoroutine = StartCoroutine(MoveToTarget());
}

private void OnDisable()
{
    if (_movementcoroutine != null)
    {
        StopCoroutine(_movementcoroutine);
        _movementcoroutine = null;
    }
}
```

### Modern Async Patterns
- Consider using UniTask for modern async/await patterns
- Use CancellationTokens for proper async cleanup
- Be aware of Unity's main thread requirements

## DEBUGGING AND PROFILING

### Profiler Integration
- Use Profiler.BeginSample() / Profiler.EndSample() for custom profiling
- Monitor memory allocations in critical paths
- Use Deep Profiling sparingly (performance impact)
- Profile on target devices, not just in editor

### Debug Utilities
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void DebugLog(string message)
{
    Debug.Log($"[{GetType().Name}] {message}");
}
```

## UNITY PROJECT STRUCTURE

### Folder Organization
```
Assets/
└── 01_Script/
    ├── 00_Common/         # Shared structures, enums, utilities
    ├── 01_Core/           # Core game logic (MoveController)
    ├── 02_Manager/        # Game managers (GameManager, MatchManager, etc.)
    ├── 03_UI/             # UI components
    │   ├── 00_UI_Common/  # Reusable UI components
    │   ├── 01_UI_Lobby/   # Lobby scene UI
    │   └── 02_UI_Play/    # Game play UI
    ├── 04_Pool/           # Object pooling systems
    ├── 05_SO/             # ScriptableObjects
    └── Editor/            # Editor tools (MapEditor, LevelDesignTool)
```

### Assembly Definitions
- Use Assembly Definition files to control compilation
- Separate test assemblies from runtime code
- Optimize compilation times with proper dependencies

## PLATFORM-SPECIFIC CONSIDERATIONS

### Cross-Platform Development
- Use Unity's Input System for modern input handling
- Abstract platform-specific functionality behind interfaces
- Test on target platforms regularly
- Use conditional compilation for platform-specific code

### Build Pipeline
- Keep build sizes optimized
- Use addressables for large assets
- Configure proper player settings for each platform
- Automate build processes where possible

# UNITY-SPECIFIC TDD WORKFLOW

When approaching a new Unity feature:

1. **Red**: Write failing test using Unity Test Framework
2. **Green**: Implement minimum code to pass (considering Unity lifecycle)
3. **Refactor**: Improve code while respecting Unity patterns
4. **Integration**: Test in actual Unity scenes when needed
5. **Build**: Verify changes work in actual builds, not just editor

Always consider Unity's unique constraints (main thread, serialization, lifecycle) when applying TDD principles.

# EXAMPLE WORKFLOW

When approaching a new feature:

1. Write a simple failing test for a small part of the feature
2. Implement the bare minimum to make it pass
3. Run tests to confirm they pass (Green)
4. Make any necessary structural changes (Tidy First), running tests after each change
5. Commit structural changes separately
6. Add another test for the next small increment of functionality
7. Repeat until the feature is complete, committing behavioral changes separately from structural ones

Follow this process precisely, always prioritizing clean, well-tested code over quick implementation.

Always write one test at a time, make it run, then improve structure. Always run all the tests (except long-running tests) each time.

# UNITY-SPECIFIC REMINDERS

- Always consider MonoBehaviour lifecycle when writing tests
- Use Unity's serialization system appropriately
- Remember that Unity runs on the main thread
- Test both in Editor and actual builds when possible
- Consider performance implications of every code change
- Use Unity's built-in profiling tools to validate performance
- Maintain compatibility with target platforms throughout development

*모든 대답은 한국어로 합니다.
*코드 작성 시 이해도를 높이기 위해 항상 한국어로 주석을 작성해주세요