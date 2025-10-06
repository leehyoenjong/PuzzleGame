# Match Detection Bug Fix Plan

## 🔴 발견된 버그

### Bug #1: MatchDetector 단방향 스캔 문제
**파일**: `Assets/01_Script/01_Core/MatchDetector.cs:18-78`

**문제**:
- `DetectHorizontalMatch`가 오른쪽으로만 스캔 (27-43줄)
- `DetectVerticalMatch`가 위쪽으로만 스캔 (59-74줄)
- `AllBlockMatch`는 모든 위치를 스캔하므로 매치의 중간/끝에서 감지 실패

**영향**:
```
그리드: [R][R][R][ ]

AllBlockMatch 스캔 순서:
1. (0,0) 검사 → 오른쪽 스캔 → [R][R][R] ✅ 감지
2. (1,0) 검사 → 오른쪽 스캔 → [R][R] ❌ 2개만 (감지 실패)
3. (2,0) 검사 → 오른쪽 스캔 → [R] ❌ 1개만 (감지 실패)
```

### Bug #2: Cross Match는 양방향 스캔하지만 일반 매치는 안함
**파일**: `Assets/01_Script/01_Core/MatchDetector.cs:108-186`

- `DetectFullHorizontalMatch`는 좌우 양방향 확인 (113-143줄) ✅
- `DetectFullVerticalMatch`는 상하 양방향 확인 (154-183줄) ✅
- **하지만 이 로직은 십자 매치에만 사용됨**
- **일반 가로/세로 매치는 단방향 로직 사용** ❌

### Bug #3: 테스트 커버리지 부족
**파일**: `Assets/01_Script/Tests/MatchDetectorTests.cs`

**현재 테스트**:
```csharp
var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
// 항상 매치의 시작 위치(0,0)에서만 테스트
```

**누락된 테스트**:
- 매치 중간 위치(1,0)에서 감지 테스트
- 매치 끝 위치(2,0)에서 감지 테스트
- AllBlockMatch의 전체 그리드 스캔 동작 테스트
- 중력 후 생성된 매치 감지 테스트
- L자/T자 겹치는 매치 테스트

---

## 🔧 수정 계획

### Phase 1: MatchDetector 양방향 스캔 구현 (최우선) ✅ COMPLETED

#### Test 1.1: 매치 중간 위치에서 감지 ✅
- [x] 테스트: 3-매치의 중간 위치(1,0)에서 전체 매치를 찾아야 함
- [x] 테스트: 3-매치의 끝 위치(2,0)에서 전체 매치를 찾아야 함
- [x] 테스트: 4-매치의 모든 위치에서 전체 매치를 찾아야 함
- [x] 테스트: 5-매치의 모든 위치에서 전체 매치를 찾아야 함
- [x] 구현: `DetectHorizontalMatch`에 좌우 양방향 스캔 추가
- [x] 구현: `DetectVerticalMatch`에 상하 양방향 스캔 추가

**커밋**: `d905c22` - [BUGFIX] Implement bidirectional scanning in MatchDetector

**구현 방법**:
```csharp
public List<(int, int)> DetectHorizontalMatch(grid, startposition)
{
    var matches = new List<(int, int)>();

    // 1. 왼쪽으로 스캔 (startposition 포함)
    int leftx = startposition.x;
    while (IsMatchingBlock(grid, (leftx, startposition.y), startcolor))
    {
        matches.Insert(0, (leftx, startposition.y));
        leftx--;
    }

    // 2. 오른쪽으로 스캔 (startposition 제외, 이미 추가됨)
    int rightx = startposition.x + 1;
    while (IsMatchingBlock(grid, (rightx, startposition.y), startcolor))
    {
        matches.Add((rightx, startposition.y));
        rightx++;
    }

    return matches.Count >= 3 ? matches : null;
}
```

#### Test 1.2: 중복 매치 방지 ✅
- [x] 테스트: (0,0), (1,0), (2,0)을 모두 스캔해도 하나의 매치만 반환해야 함
- [x] 테스트: 동일한 블록이 여러 매치 결과에 포함되지 않아야 함
- [x] 구현: AllBlockMatch에서 이미 처리된 블록 추적
- [x] 구현: HashSet을 사용하여 중복 제거

**구현 완료**:
- `AllBlockMatchTests.cs` 추가: 중복 처리 방지 테스트
- `MatchManager.AllBlockMatch()`: `processedblocks` HashSet 추가하여 이미 처리된 블록 스킵
- 같은 매치를 여러 번 처리하는 버그 수정

**다음 커밋 예정**: Phase 1 완료 커밋 필요

---

### Phase 2: AllBlockMatch 통합 수정

#### Test 2.1: 전체 그리드 스캔 정확성 ✅
- [x] 테스트: 3x3 그리드 하단에 가로 매치가 있을 때 AllBlockMatch가 감지해야 함
- [x] 테스트: 여러 매치가 동시에 존재할 때 모두 감지해야 함
- [x] 테스트: L자/T자 겹치는 매치를 올바르게 처리해야 함
- [x] 구현: GetMatchBlock이 양방향 감지를 올바르게 처리 (Phase 1에서 이미 완료)
- [x] 구현: 매치 결과 병합 시 중복 제거 (processedblocks HashSet으로 해결)

**커밋**: `b180e1a` - [TEST] Add comprehensive AllBlockMatch grid scanning tests

#### Test 2.2: 매치 결과 정규화 ✅
- [x] 테스트: 같은 매치를 다른 위치에서 스캔해도 동일한 결과여야 함
- [x] 테스트: 매치 결과는 항상 정렬된 순서여야 함 (좌→우, 하→상)
- [x] 구현: 매치 결과를 정규화하는 메서드 추가 (이미 구현됨 - Insert(0) + Add 패턴)
- [x] 구현: 중복 매치를 HashSet으로 필터링 (Phase 1.2에서 이미 완료)

**구현 완료**:
- `AllBlockMatchTests.cs`: `ShouldReturnIdenticalResultsFromDifferentScanPositions` 테스트 추가
- `AllBlockMatchTests.cs`: `ShouldReturnMatchResultsInSortedOrder` 테스트 추가
- `MatchDetector.DetectHorizontalMatch()`: Insert(0) + Add 패턴으로 좌→우 정렬 보장
- `MatchDetector.DetectVerticalMatch()`: Insert(0) + Add 패턴으로 하→상 정렬 보장
- 모든 테스트 통과 확인

**다음 커밋 예정**: Phase 2 Test 2.2 완료 커밋

---

### Phase 3: 타이밍 및 동기화 수정

#### Test 3.1: 그리드 딕셔너리 동기화
- [ ] 테스트: 블록 스왑 후 딕셔너리가 매치 감지 전에 업데이트되어야 함
- [ ] 테스트: 블록 위치와 그리드 키가 일치해야 함
- [ ] 테스트: 애니메이션 중에도 그리드 상태는 정확해야 함
- [ ] 구현: ChangeIDX 호출 타이밍 검증
- [ ] 구현: 필요 시 동기화 포인트 추가

#### Test 3.2: 애니메이션 타이밍
- [ ] 테스트: 스왑 애니메이션 완료 후 매치 감지가 실행되어야 함
- [ ] 테스트: UniTask await가 올바른 순서로 실행되어야 함
- [ ] 검증: UserMoveBlockMatch의 await 순서 확인
- [ ] 검증: AllBlockMatch 호출 타이밍 확인

---

### Phase 4: 케스케이드 및 중력 통합

#### Test 4.1: 중력 후 매치 감지
- [ ] 테스트: 블록이 떨어져서 생성된 매치를 감지해야 함
- [ ] 테스트: 여러 단계의 케스케이드 매치가 작동해야 함
- [ ] 테스트: 중력 후 십자 매치가 생성되면 감지해야 함
- [ ] 구현: BlockMover 사용 후 AllBlockMatch 호출 확인

#### Test 4.2: 실제 게임플레이 시나리오
- [ ] 테스트: 엔드투엔드 - 사용자 이동 → 매치 → 중력 → 케스케이드 → 매치
- [ ] 테스트: 동시 다발적 매치 (L자, T자 패턴)
- [ ] 테스트: 케스케이드 중 특수 블록 상호작용
- [ ] 통합 테스트: 실제 게임 씬에서 플레이 시뮬레이션

---

### Phase 5: Cross Match 로직 검증

#### Test 5.1: 십자 매치 경계 케이스
- [ ] 테스트: 3x3 십자가 그리드 경계에서 올바르게 감지되어야 함
- [ ] 테스트: 가로/세로 매치가 겹칠 때 십자로 올바르게 분류되어야 함
- [ ] 테스트: 색상이 다를 때 십자로 분류되지 않아야 함
- [ ] 검증: DetectCrossMatch가 DetectFullHorizontalMatch/DetectFullVerticalMatch와 일관성 있게 작동

---

## 🎯 우선순위

### 🔥 즉시 수정 (Phase 1)
1. **MatchDetector 양방향 스캔 구현** - 핵심 버그
2. **중간 위치 감지 테스트 추가** - 버그 재현 방지

### ⚡ 단기 수정 (Phase 2)
3. **AllBlockMatch 중복 제거** - 안정성 향상
4. **매치 결과 정규화** - 일관성 보장

### 📌 중기 수정 (Phase 3-4)
5. **타이밍 동기화 검증** - 엣지 케이스 방지
6. **케스케이드 통합 테스트** - 실제 게임플레이 검증

### 📋 장기 개선 (Phase 5)
7. **Cross Match 검증** - 특수 패턴 안정성

---

## 🧪 테스트 전략

### 단위 테스트 (Unit Tests)
- `MatchDetectorTests.cs`: 모든 스캔 위치에서 감지 테스트
- `MatchManagerTests.cs`: AllBlockMatch 통합 테스트
- 각 Phase별로 테스트 먼저 작성 (TDD)

### 통합 테스트 (Integration Tests)
- `MatchManagerIntegrationTests.cs`: 컴포넌트 간 상호작용
- `EndToEndIntegrationTests.cs`: 전체 게임 사이클

### 수동 테스트 (Manual Tests)
- Unity Editor에서 실제 게임플레이 검증
- 각 수정 후 3x3, 4-match, 5-match, cross 패턴 확인
- 케스케이드 시나리오 테스트

---

## 📝 커밋 전략

각 Phase별로 별도 커밋:

```
[BUGFIX] Fix MatchDetector bidirectional scanning
[BUGFIX] Add duplicate match prevention in AllBlockMatch
[BUGFIX] Synchronize grid dictionary before match detection
[BUGFIX] Fix cascade match detection after gravity
[TEST] Add comprehensive match detection test coverage
```

---

## ✅ 완료 조건

- [ ] 모든 테스트 통과 (단위 + 통합)
- [ ] Unity Editor에서 3x3 매치 정상 작동 확인
- [ ] 케스케이드 매치 정상 작동 확인
- [ ] 특수 블록 매치 정상 작동 확인
- [ ] 성능 저하 없음 (프로파일링)
- [ ] 기존 기능 회귀 없음

---

## 📚 참고 자료

### 현재 구현 파일
- `Assets/01_Script/01_Core/MatchDetector.cs` - 매치 감지 로직
- `Assets/01_Script/02_Manager/MatchManager.cs` - 매치 관리자
- `Assets/01_Script/02_Manager/MatchFiledManager.cs` - 그리드 관리자
- `Assets/01_Script/Tests/MatchDetectorTests.cs` - 기존 테스트

### 추가할 테스트 파일
- `Assets/01_Script/Tests/MatchDetectorBidirectionalTests.cs` - 양방향 스캔 테스트
- `Assets/01_Script/Tests/AllBlockMatchIntegrationTests.cs` - AllBlockMatch 통합 테스트
- `Assets/01_Script/Tests/CascadeMatchTests.cs` - 케스케이드 매치 테스트

---

## 🚀 시작 방법

1. `plan.md` 파일 업데이트 - 이 버그픽스 계획 링크 추가
2. Phase 1 Test 1.1 시작: "매치 중간 위치에서 감지" 테스트 작성
3. Red → Green → Refactor 사이클 진행
4. 각 테스트 통과 후 커밋
5. Phase별로 순차 진행

**다음 명령**: "go" 입력 시 Phase 1 Test 1.1의 첫 번째 테스트 구현 시작
