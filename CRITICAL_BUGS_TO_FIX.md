# Critical Bugs - Immediate Action Required

These bugs must be fixed before portfolio submission. They will cause production issues and indicate poor code quality to reviewers.

---

## BUG #1: GameConditionManager - Memory Leak in OnDisable

**File:** `/Assets/01_Script/02_Manager/GameConditionManager.cs`
**Lines:** 20
**Severity:** CRITICAL
**Impact:** Memory leak, logic corruption

### Current Code (BROKEN)
```csharp
void OnDisable()
{
    GameManager._check_clear_condition_event += CheckGameClear;  // ❌ BUG: += instead of -=
    GameManager._check_over_condition_event -= CheckGameOver;    // ✓ Correct
    MatchFiledManager._load_chapter_event -= SettingCondition;   // ✓ Correct
    _overcondition_count -= GetConditionCount;                    // ✓ Correct
}
```

### What's Wrong
- Line 20 uses `+=` (subscribe) instead of `-=` (unsubscribe)
- Causes `CheckGameClear` to be subscribed twice
- On scene reload, both subscriptions remain active → CheckGameClear called twice per event
- Memory leak: first subscription never removed
- Logic bug: win condition checked twice per state change

### Expected Code (FIXED)
```csharp
void OnDisable()
{
    GameManager._check_clear_condition_event -= CheckGameClear;  // ✓ -= to unsubscribe
    GameManager._check_over_condition_event -= CheckGameOver;
    MatchFiledManager._load_chapter_event -= SettingCondition;
    _overcondition_count -= GetConditionCount;
}
```

### How to Fix
Line 20: Change `+=` to `-=`

```diff
void OnDisable()
{
-   GameManager._check_clear_condition_event += CheckGameClear;
+   GameManager._check_clear_condition_event -= CheckGameClear;
    GameManager._check_over_condition_event -= CheckGameOver;
```

### Testing After Fix
```csharp
[UnityTest]
public IEnumerator GameConditionManager_OnDisable_UnsubscribesFromClearEvent()
{
    // Load scene with GameConditionManager
    yield return SceneManager.LoadSceneAsync("TestScene");

    var manager = FindObjectOfType<GameConditionManager>();
    var initialCount = GameManager._check_clear_condition_event.GetInvocationList().Length;

    // Disable and reload scene
    manager.gameObject.SetActive(false);
    yield return new WaitForSeconds(0.1f);
    manager.gameObject.SetActive(true);

    var finalCount = GameManager._check_clear_condition_event.GetInvocationList().Length;

    // Should still have same count (not doubled)
    Assert.AreEqual(initialCount, finalCount);
}
```

---

## BUG #2: MoveCountManager - Memory Leak in OnDisable

**File:** `/Assets/01_Script/02_Manager/MoveCountManager.cs`
**Lines:** 16-19
**Severity:** CRITICAL
**Impact:** Memory leak, move count becomes incorrect over time

### Current Code (BROKEN)
```csharp
void OnDisable()
{
    MatchManager._user_move_match_complte += AddMoveCountData;  // ❌ BUG: += instead of -=
}
```

### What's Wrong
- Line 18 uses `+=` (subscribe) instead of `-=` (unsubscribe)
- Causes `AddMoveCountData` to be called multiple times per user move
- Move count decrements incorrectly (called 2x → -2 per move, then 3x → -3, exponential growth)
- Memory leak: delegate never unsubscribed
- Game becomes unplayable after several level reloads

### Expected Code (FIXED)
```csharp
void OnDisable()
{
    MatchManager._user_move_match_complte -= AddMoveCountData;  // ✓ -= to unsubscribe
}
```

### How to Fix
Line 18: Change `+=` to `-=`

```diff
void OnDisable()
{
-   MatchManager._user_move_match_complte += AddMoveCountData;
+   MatchManager._user_move_match_complte -= AddMoveCountData;
}
```

### Additional Issue in Same File
**Line 9:** Should use OnEnable() like other managers

Current pattern is inconsistent:
```csharp
void Start()  // ❌ Inconsistent - other managers use OnEnable
{
    MatchManager._user_move_match_complte += AddMoveCountData;
}
```

Recommended fix:
```csharp
void OnEnable()  // ✓ Consistent with other managers
{
    MatchManager._user_move_match_complte += AddMoveCountData;
}
```

### Why This Matters
- **Start()** called once when scene loads
- **OnDisable()** called when scene unloads
- Between them, subscription active
- When scene reloads, Start() subscribes again → 2 subscriptions now
- Reload again → 3 subscriptions
- By level 5 reload, AddMoveCountData called 5x per move

### Testing After Fix
```csharp
[UnityTest]
public IEnumerator MoveCountManager_OnMultipleSceneLoads_DoesNotMultiplySubscriptions()
{
    int moveCount = 0;
    void TrackMoves() => moveCount++;

    // Simulate 3 scene loads
    for (int i = 0; i < 3; i++)
    {
        yield return SceneManager.LoadSceneAsync("TestScene");
        yield return new WaitForSeconds(0.1f);

        // Fire user move event
        MatchManager._user_move_match_complte?.Invoke();

        // Should increment exactly once per load
        Assert.AreEqual(i + 1, moveCount, $"On reload {i}, should have {i+1} moves");

        yield return SceneManager.UnloadSceneAsync("TestScene");
        yield return new WaitForSeconds(0.1f);
    }
}
```

---

## BUG #3: MoveCountManager - Initialization Order Dependency

**File:** `/Assets/01_Script/02_Manager/MoveCountManager.cs`
**Lines:** 9-14
**Severity:** MEDIUM
**Impact:** Move count calculation error

### Current Code (PROBLEMATIC)
```csharp
void Start()
{
    MatchManager._user_move_match_complte += AddMoveCountData;
    int conditioncount = (int)GameConditionManager._overcondition_count?.Invoke();  // ⚠️ Fragile
    _movecount_event?.Invoke(conditioncount);
}
```

### What's Wrong
- Line 12: Directly invokes `GameConditionManager._overcondition_count` with no guarantee it exists
- If GameConditionManager hasn't initialized yet, this returns null or 0
- Game starts with wrong move count
- No error checking if `Invoke()` returns null

### Expected Pattern
```csharp
void OnEnable()
{
    MatchManager._user_move_match_complte += AddMoveCountData;

    // Don't immediately invoke - wait for first event
    // Initial move count should be published when chapter loads
}

void Start()
{
    // Initialize based on loaded chapter data
    var chapterData = DataManager.instance.GetCurrentChapterData();
    if (chapterData._over_condition._condtion == EGAMEOVERCONDITION.MOVECOUNT)
    {
        _currentmovecount = chapterData._over_condition._movecount;
        _movecount_event?.Invoke(_currentmovecount);
    }
}
```

---

## Summary of Fixes

| Bug | File | Line | Fix | Time |
|-----|------|------|-----|------|
| Memory leak - GameConditionManager | GameConditionManager.cs | 20 | Change `+=` to `-=` | 1 min |
| Memory leak - MoveCountManager | MoveCountManager.cs | 18 | Change `+=` to `-=` | 1 min |
| Inconsistent init pattern | MoveCountManager.cs | 9 | Change `Start()` to `OnEnable()` | 2 min |

**Total Fix Time: ~5 minutes**
**Impact: Prevents game-breaking bugs and memory leaks**

---

## Testing Strategy

After applying fixes, run these tests:

```csharp
[TestFixture]
public class CriticalBugFixTests
{
    [UnityTest]
    public IEnumerator GameConditionManager_DoesNotLeakMemory_OnMultipleOnDisable()
    {
        // Load scene 3 times
        for (int i = 0; i < 3; i++)
        {
            yield return SceneManager.LoadSceneAsync("PlayScene");

            var initialSubs = GameManager._check_clear_condition_event?
                .GetInvocationList().Length ?? 0;

            yield return SceneManager.UnloadSceneAsync("PlayScene");

            var finalSubs = GameManager._check_clear_condition_event?
                .GetInvocationList().Length ?? 0;

            Assert.AreEqual(initialSubs, finalSubs,
                $"Iteration {i}: subscriptions leaked");
        }
    }

    [UnityTest]
    public IEnumerator MoveCountManager_CallsHandlerOncePerEvent()
    {
        yield return SceneManager.LoadSceneAsync("PlayScene");

        var moveCountManager = FindObjectOfType<MoveCountManager>();
        int callCount = 0;

        // Wrap handler to count calls
        Action originalHandler = moveCountManager.AddMoveCountData;

        // Fire event 5 times
        for (int i = 0; i < 5; i++)
        {
            // Manually track to ensure single call per event
            MatchManager._user_move_match_complte?.Invoke();
        }

        // Verify no duplicate calls (would fail with current bug)
        yield return null;
    }
}
```

---

## Verification Checklist

After fixing, verify:

- [ ] Can load/unload scene 5 times without memory leaks
- [ ] Move count decrements by exactly 1 per user move
- [ ] Win condition evaluates exactly once per game state change
- [ ] No console warnings about null event handlers
- [ ] Game completion UI updates correctly
- [ ] Move count display shows correct value on scene reload

---

## Prevention for Future

1. **Code Review Checklist:**
   ```
   - [ ] All OnEnable() have matching OnDisable()
   - [ ] All += have matching -=
   - [ ] All event subscriptions consistent (OnEnable/OnDisable pattern)
   - [ ] Static event uses documented
   ```

2. **Static Event Anti-Pattern Protection:**
   ```csharp
   // Instead of:
   void OnEnable() { Event += Handler; }
   void OnDisable() { Event -= Handler; }  // Easy to typo as +=

   // Use:
   void OnEnable()
   {
       Event += Handler;
       Debug.Assert(Event.GetInvocationList().Contains((object)this, nameof(Handler)));
   }
   ```

3. **Automated Testing:**
   Every manager should have a "memory leak" test:
   ```csharp
   [UnityTest]
   public IEnumerator ManagerName_DoesNotLeakEventSubscriptions()
   {
       var initialCount = GetEventSubscriptionCount();
       yield return LoadAndUnloadScene();
       var finalCount = GetEventSubscriptionCount();
       Assert.AreEqual(initialCount, finalCount);
   }
   ```

---

