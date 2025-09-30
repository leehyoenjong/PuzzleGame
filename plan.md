# Match-3 Refactoring Plan (TDD Approach)

## Overview
Refactor MatchManager.cs and MatchFiledManager.cs following TDD principles and SOLID design patterns.

## Phase 1: Extract Match Detection Logic

### Test 1.1: MatchDetector - Horizontal Match Detection
- [x] Test: Should detect 3 horizontal matching blocks
- [x] Test: Should detect 4 horizontal matching blocks
- [x] Test: Should detect 5+ horizontal matching blocks
- [x] Test: Should not detect match with 2 blocks
- [x] Implementation: Create `MatchDetector` class with `DetectHorizontalMatch` method

### Test 1.2: MatchDetector - Vertical Match Detection
- [x] Test: Should detect 3 vertical matching blocks
- [x] Test: Should detect 4 vertical matching blocks
- [x] Test: Should detect 5+ vertical matching blocks
- [x] Test: Should not detect match with 2 blocks
- [x] Implementation: Create `DetectVerticalMatch` method

### Test 1.3: MatchDetector - Cross Match Detection
- [x] Test: Should detect 3x3 cross pattern
- [x] Test: Should detect 4x4 cross pattern
- [x] Test: Should detect 5x5 cross pattern
- [x] Test: Should return null when no cross pattern exists
- [x] Implementation: Create `DetectCrossMatch` method

### Test 1.4: Integration - Replace MatchManager.GetMatchBlock with MatchDetector
- [x] Test: MatchManager.AllBlockMatch uses MatchDetector correctly
- [x] Test: MatchManager.UserMoveBlockMatch uses MatchDetector correctly
- [x] Refactor: Replace inline match detection with MatchDetector
- [ ] Commit: [STRUCTURAL] Extract match detection into MatchDetector class

## Phase 2: Extract Match Type Classification

### Test 2.1: MatchTypeClassifier - Basic Pattern Classification
- [ ] Test: Should classify 3-match as EMATCHTYPE.THREE
- [ ] Test: Should classify 4-horizontal as EMATCHTYPE.FORE_LEFTRIGHT
- [ ] Test: Should classify 4-vertical as EMATCHTYPE.FORE_UPDOWN
- [ ] Test: Should classify 5-match as EMATCHTYPE.FIVE
- [ ] Implementation: Create `MatchTypeClassifier` class with `ClassifyMatchType` method

### Test 2.2: MatchTypeClassifier - Cross Pattern Classification
- [ ] Test: Should classify 3x3 same color cross as EMATCHTYPE.CROSS_THREE
- [ ] Test: Should classify 4x4 same color cross as EMATCHTYPE.CROSS_FOUR
- [ ] Test: Should classify 5x5 same color cross as EMATCHTYPE.CROSS_FIVE
- [ ] Test: Should not classify cross when colors differ
- [ ] Implementation: Extend `ClassifyMatchType` with cross pattern logic

### Test 2.3: Integration - Replace MatchManager.GetMatchTypes with MatchTypeClassifier
- [ ] Test: MatchComplte uses MatchTypeClassifier correctly
- [ ] Test: GetSpecialBlockCreationRequest uses MatchTypeClassifier correctly
- [ ] Refactor: Replace GetMatchTypes with MatchTypeClassifier
- [ ] Commit: [STRUCTURAL] Extract match type classification logic

## Phase 3: Extract Special Block Creation Logic

### Test 3.1: SpecialBlockFactory - Creation Request Generation
- [ ] Test: Should generate creation request for 4-match at user move position
- [ ] Test: Should generate creation request for 5-match with correct color
- [ ] Test: Should generate creation request for cross patterns
- [ ] Test: Should return null for 3-match
- [ ] Implementation: Create `SpecialBlockFactory` class with `CreateSpecialBlockRequest` method

### Test 3.2: SpecialBlockFactory - Middle Point Calculation
- [ ] Test: Should calculate middle point for line matches
- [ ] Test: Should calculate intersection point for cross matches
- [ ] Test: Should use user move position when available
- [ ] Implementation: Create `CalculateSpawnPoint` method

### Test 3.3: Integration - Replace inline special block creation logic
- [ ] Test: UserMoveBlockMatch uses SpecialBlockFactory correctly
- [ ] Test: MatchComplte uses SpecialBlockFactory correctly
- [ ] Refactor: Replace GetSpecialBlockCreationRequest with SpecialBlockFactory
- [ ] Commit: [STRUCTURAL] Extract special block creation into factory

## Phase 4: Extract Chain Reaction Logic

### Test 4.1: ChainReactionProcessor - Special Block Effect Processing
- [ ] Test: Should process FORE_LEFTRIGHT line clear effect
- [ ] Test: Should process FORE_UPDOWN line clear effect
- [ ] Test: Should process FIVE color match effect
- [ ] Test: Should process CROSS patterns area clear
- [ ] Implementation: Create `ChainReactionProcessor` class with `ProcessEffect` method

### Test 4.2: ChainReactionProcessor - Recursive Chain Handling
- [ ] Test: Should detect and process chained special blocks
- [ ] Test: Should inherit color for FIVE block chains
- [ ] Test: Should not process same block twice
- [ ] Test: Should handle circular chain references
- [ ] Implementation: Create `ProcessChainReaction` method

### Test 4.3: Integration - Replace MatchManager chain reaction logic
- [ ] Test: GetMatchTypeFuction uses ChainReactionProcessor correctly
- [ ] Test: ProcessChainReaction in UserMoveBlockMatch uses new processor
- [ ] Refactor: Replace inline chain reaction with ChainReactionProcessor
- [ ] Commit: [STRUCTURAL] Extract chain reaction processing

## Phase 5: Simplify UserMoveBlockMatch Method

### Test 5.1: MoveMatchValidator - Match Validation
- [ ] Test: Should validate if move creates any match
- [ ] Test: Should handle FIVE block special case
- [ ] Test: Should return match results for both swapped blocks
- [ ] Implementation: Create `MoveMatchValidator` class with `ValidateMove` method

### Test 5.2: BlockSwapHandler - Swap and Rollback
- [ ] Test: Should swap two blocks and update positions
- [ ] Test: Should rollback swap when no match found
- [ ] Test: Should maintain dictionary consistency during swap
- [ ] Implementation: Create `BlockSwapHandler` class

### Test 5.3: Refactor UserMoveBlockMatch - Use Extracted Components
- [ ] Test: UserMoveBlockMatch works correctly with MoveMatchValidator
- [ ] Test: UserMoveBlockMatch works correctly with BlockSwapHandler
- [ ] Test: UserMoveBlockMatch works correctly with SpecialBlockFactory
- [ ] Test: UserMoveBlockMatch works correctly with ChainReactionProcessor
- [ ] Refactor: Simplify UserMoveBlockMatch using extracted components
- [ ] Commit: [STRUCTURAL] Simplify UserMoveBlockMatch method

## Phase 6: Extract Grid Management from MatchFiledManager

### Test 6.1: GridManager - Grid State Management
- [ ] Test: Should initialize grid with correct dimensions
- [ ] Test: Should add block to grid at position
- [ ] Test: Should remove block from grid at position
- [ ] Test: Should check if position contains block
- [ ] Test: Should get block at position
- [ ] Implementation: Create `GridManager` class with grid dictionary management

### Test 6.2: GridManager - Map Validation
- [ ] Test: Should validate if position is within map bounds
- [ ] Test: Should load map data correctly
- [ ] Test: Should track top slots for spawning
- [ ] Implementation: Add map validation methods

### Test 6.3: Integration - Replace MatchFiledManager grid management
- [ ] Test: MatchFiledManager uses GridManager for all grid operations
- [ ] Test: ChangeIDX and RemoveIDX delegate to GridManager
- [ ] Refactor: Extract grid management into GridManager
- [ ] Commit: [STRUCTURAL] Extract grid management logic

## Phase 7: Extract Block Movement Logic

### Test 7.1: BlockMover - Gravity Movement
- [ ] Test: Should move block down to nearest empty slot
- [ ] Test: Should skip non-empty slots
- [ ] Test: Should handle multiple blocks in same column
- [ ] Test: Should respect map boundaries
- [ ] Implementation: Create `BlockMover` class with `MoveBlocksDown` method

### Test 7.2: BlockMover - Step-by-Step Movement
- [ ] Test: Should move blocks row by row for cascade effect
- [ ] Test: Should sort blocks by y-position before moving
- [ ] Test: Should track movement count
- [ ] Implementation: Add cascade movement logic

### Test 7.3: Integration - Replace MatchFiledManager.MoveMatchBlock
- [ ] Test: FiledReSetting uses BlockMover correctly
- [ ] Test: WaitAndMove triggers BlockMover correctly
- [ ] Refactor: Replace MoveMatchBlock with BlockMover
- [ ] Commit: [STRUCTURAL] Extract block movement logic

## Phase 8: Extract Block Spawning Logic

### Test 8.1: BlockSpawner - Spawn Position Calculation
- [ ] Test: Should calculate spawn position above top slot
- [ ] Test: Should respect grid center alignment
- [ ] Test: Should handle different grid sizes
- [ ] Implementation: Create `BlockSpawner` class with position calculation

### Test 8.2: BlockSpawner - Block Creation
- [ ] Test: Should create normal blocks for empty slots
- [ ] Test: Should create special blocks at specific positions
- [ ] Test: Should return list of newly created blocks
- [ ] Implementation: Add block creation methods

### Test 8.3: Integration - Replace MatchFiledManager.CreateMatchBlock
- [ ] Test: Setting uses BlockSpawner correctly
- [ ] Test: FiledReSetting uses BlockSpawner correctly
- [ ] Test: CreateMatchBlock (individual) uses BlockSpawner
- [ ] Refactor: Replace CreateMatchBlock with BlockSpawner
- [ ] Commit: [STRUCTURAL] Extract block spawning logic

## Phase 9: Introduce Match Coordinator

### Test 9.1: MatchCoordinator - Orchestrate Match Flow
- [ ] Test: Should coordinate full match sequence
- [ ] Test: Should handle special block creation after destruction
- [ ] Test: Should trigger events in correct order
- [ ] Implementation: Create `MatchCoordinator` to orchestrate MatchManager components

### Test 9.2: Integration - Simplify MatchManager
- [ ] Test: AllBlockMatch delegates to MatchCoordinator
- [ ] Test: UserMoveBlockMatch delegates to MatchCoordinator
- [ ] Refactor: Reduce MatchManager to event handling and coordination
- [ ] Commit: [STRUCTURAL] Introduce MatchCoordinator for orchestration

## Phase 10: Introduce Field Coordinator

### Test 10.1: FieldCoordinator - Orchestrate Field Operations
- [ ] Test: Should coordinate spawn → move → match cycle
- [ ] Test: Should handle simulation checks correctly
- [ ] Test: Should manage setting state correctly
- [ ] Implementation: Create `FieldCoordinator` to orchestrate MatchFiledManager components

### Test 10.2: Integration - Simplify MatchFiledManager
- [ ] Test: Setting delegates to FieldCoordinator
- [ ] Test: FiledReSetting delegates to FieldCoordinator
- [ ] Refactor: Reduce MatchFiledManager to event handling and coordination
- [ ] Commit: [STRUCTURAL] Introduce FieldCoordinator for orchestration

## Phase 11: Final Integration and Cleanup

### Test 11.1: End-to-End Integration Tests
- [ ] Test: Complete user move → match → spawn → gravity cycle works
- [ ] Test: Cascade matches work correctly
- [ ] Test: Chain reactions work correctly
- [ ] Test: Special block interactions work correctly

### Test 11.2: Performance Validation
- [ ] Test: No allocation increases in Update/FixedUpdate
- [ ] Test: Dictionary operations remain O(1)
- [ ] Test: No FindObjectOfType calls added

### Test 11.3: Final Cleanup
- [ ] Remove unused methods from MatchManager
- [ ] Remove unused methods from MatchFiledManager
- [ ] Update documentation comments
- [ ] Commit: [STRUCTURAL] Final cleanup and documentation

## Refactoring Goals Summary

### Before:
- MatchManager: 674 lines, 10+ responsibilities
- MatchFiledManager: 350 lines, 8+ responsibilities

### After:
- MatchManager: ~100 lines (event coordination only)
- MatchFiledManager: ~100 lines (event coordination only)
- MatchDetector: Match detection logic
- MatchTypeClassifier: Pattern classification
- SpecialBlockFactory: Special block creation
- ChainReactionProcessor: Chain reaction handling
- MoveMatchValidator: Move validation
- BlockSwapHandler: Swap operations
- GridManager: Grid state management
- BlockMover: Movement logic
- BlockSpawner: Spawning logic
- MatchCoordinator: Match flow orchestration
- FieldCoordinator: Field flow orchestration

### Benefits:
- Single Responsibility Principle compliance
- Testable components
- Reusable logic
- Clear separation of concerns
- Easier maintenance and debugging