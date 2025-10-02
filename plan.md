# Match-3 리팩토링 계획 (TDD 접근법)

## 개요
TDD 원칙과 SOLID 디자인 패턴을 따라 MatchManager.cs와 MatchFiledManager.cs를 리팩토링합니다.

## Phase 1: 매치 감지 로직 추출

### Test 1.1: MatchDetector - 수평 매치 감지
- [x] 테스트: 수평으로 매칭되는 3개 블록을 감지해야 함
- [x] 테스트: 수평으로 매칭되는 4개 블록을 감지해야 함
- [x] 테스트: 수평으로 매칭되는 5개 이상 블록을 감지해야 함
- [x] 테스트: 2개 블록으로는 매치를 감지하지 않아야 함
- [x] 구현: `MatchDetector` 클래스와 `DetectHorizontalMatch` 메서드 생성

### Test 1.2: MatchDetector - 수직 매치 감지
- [x] 테스트: 수직으로 매칭되는 3개 블록을 감지해야 함
- [x] 테스트: 수직으로 매칭되는 4개 블록을 감지해야 함
- [x] 테스트: 수직으로 매칭되는 5개 이상 블록을 감지해야 함
- [x] 테스트: 2개 블록으로는 매치를 감지하지 않아야 함
- [x] 구현: `DetectVerticalMatch` 메서드 생성

### Test 1.3: MatchDetector - 십자 매치 감지
- [x] 테스트: 3x3 십자 패턴을 감지해야 함
- [x] 테스트: 4x4 십자 패턴을 감지해야 함
- [x] 테스트: 5x5 십자 패턴을 감지해야 함
- [x] 테스트: 십자 패턴이 없을 때 null을 반환해야 함
- [x] 구현: `DetectCrossMatch` 메서드 생성

### Test 1.4: 통합 - MatchManager.GetMatchBlock을 MatchDetector로 교체
- [x] 테스트: MatchManager.AllBlockMatch가 MatchDetector를 올바르게 사용해야 함
- [x] 테스트: MatchManager.UserMoveBlockMatch가 MatchDetector를 올바르게 사용해야 함
- [x] 리팩토링: 인라인 매치 감지를 MatchDetector로 교체
- [x] 커밋: [STRUCTURAL] Extract match detection into MatchDetector class

## Phase 2: 매치 타입 분류 추출

### Test 2.1: MatchTypeClassifier - 기본 패턴 분류
- [x] 테스트: 3-매치를 EMATCHTYPE.THREE로 분류해야 함
- [x] 테스트: 수평 4-매치를 EMATCHTYPE.FORE_LEFTRIGHT로 분류해야 함
- [x] 테스트: 수직 4-매치를 EMATCHTYPE.FORE_UPDOWN으로 분류해야 함
- [x] 테스트: 5-매치를 EMATCHTYPE.FIVE로 분류해야 함
- [x] 구현: `MatchTypeClassifier` 클래스와 `ClassifyMatchType` 메서드 생성

### Test 2.2: MatchTypeClassifier - 십자 패턴 분류
- [x] 테스트: 같은 색상의 3x3 십자를 EMATCHTYPE.CROSS_THREE로 분류해야 함
- [x] 테스트: 같은 색상의 4x4 십자를 EMATCHTYPE.CROSS_FOUR로 분류해야 함
- [x] 테스트: 같은 색상의 5x5 십자를 EMATCHTYPE.CROSS_FIVE로 분류해야 함
- [x] 테스트: 색상이 다를 때 십자로 분류하지 않아야 함
- [x] 구현: 십자 패턴 로직으로 `ClassifyMatchType` 확장

### Test 2.3: 통합 - MatchManager.GetMatchTypes를 MatchTypeClassifier로 교체
- [x] 테스트: MatchComplte가 MatchTypeClassifier를 올바르게 사용해야 함
- [x] 테스트: GetSpecialBlockCreationRequest가 MatchTypeClassifier를 올바르게 사용해야 함
- [x] 리팩토링: GetMatchTypes를 MatchTypeClassifier로 교체
- [x] 커밋: [STRUCTURAL] Extract match type classification logic

## Phase 3: 특수 블록 생성 로직 추출

### Test 3.1: SpecialBlockFactory - 생성 요청 생성
- [x] 테스트: 사용자 이동 위치에서 4-매치에 대한 생성 요청을 생성해야 함
- [x] 테스트: 올바른 색상으로 5-매치에 대한 생성 요청을 생성해야 함
- [x] 테스트: 십자 패턴에 대한 생성 요청을 생성해야 함
- [x] 테스트: 3-매치에 대해 null을 반환해야 함
- [x] 구현: `SpecialBlockFactory` 클래스와 `CreateSpecialBlockRequest` 메서드 생성

### Test 3.2: SpecialBlockFactory - 중간점 계산
- [x] 테스트: 라인 매치에 대한 중간점을 계산해야 함
- [x] 테스트: 십자 매치에 대한 교차점을 계산해야 함
- [x] 테스트: 사용 가능한 경우 사용자 이동 위치를 사용해야 함
- [x] 구현: `CalculateSpawnPoint` 메서드 생성

### Test 3.3: 통합 - 인라인 특수 블록 생성 로직 교체
- [x] 테스트: UserMoveBlockMatch가 SpecialBlockFactory를 올바르게 사용해야 함
- [x] 테스트: MatchComplte가 SpecialBlockFactory를 올바르게 사용해야 함
- [x] 리팩토링: GetSpecialBlockCreationRequest를 SpecialBlockFactory로 교체
- [x] 커밋: [STRUCTURAL] Extract special block creation into factory

## Phase 4: 연쇄 반응 로직 추출

### Test 4.1: ChainReactionProcessor - 특수 블록 효과 처리
- [x] 테스트: EMATCHTYPE.FORE_LEFTRIGHT 라인 제거 효과를 처리해야 함
- [x] 테스트: EMATCHTYPE.FORE_UPDOWN 라인 제거 효과를 처리해야 함
- [x] 테스트: EMATCHTYPE.FIVE 색상 매치 효과를 처리해야 함
- [x] 테스트: EMATCHTYPE.CROSS 패턴 영역 제거를 처리해야 함
- [x] 구현: `ChainReactionProcessor` 클래스와 `ProcessEffect` 메서드 생성

### Test 4.2: ChainReactionProcessor - 재귀 연쇄 처리
- [x] 테스트: 연쇄된 특수 블록을 감지하고 처리해야 함
- [x] 테스트: EMATCHTYPE.FIVE 블록 연쇄에 대해 색상을 상속해야 함
- [x] 테스트: 동일한 블록을 두 번 처리하지 않아야 함
- [x] 테스트: 순환 연쇄 참조를 처리해야 함
- [x] 구현: `ProcessChainReaction` 메서드 생성

### Test 4.3: 통합 - MatchManager 연쇄 반응 로직 교체
- [x] 테스트: GetMatchTypeFuction이 ChainReactionProcessor를 올바르게 사용해야 함
- [x] 테스트: UserMoveBlockMatch의 ProcessChainReaction이 새 프로세서를 사용해야 함
- [x] 리팩토링: 인라인 연쇄 반응을 ChainReactionProcessor로 교체
- [x] 커밋: [STRUCTURAL] Extract chain reaction processing

## Phase 5: UserMoveBlockMatch 메서드 단순화

### Test 5.1: MoveMatchValidator - 매치 검증
- [x] 테스트: 이동이 매치를 생성하는지 검증해야 함
- [x] 테스트: EMATCHTYPE.FIVE 블록 특수 케이스를 처리해야 함
- [x] 테스트: 교환된 두 블록 모두에 대한 매치 결과를 반환해야 함
- [x] 구현: `MoveMatchValidator` 클래스와 `ValidateMove` 메서드 생성

### Test 5.2: BlockSwapHandler - 교환 및 롤백
- [x] 테스트: 두 블록을 교환하고 위치를 업데이트해야 함
- [x] 테스트: 매치가 없을 때 교환을 롤백해야 함
- [x] 테스트: 교환 중 딕셔너리 일관성을 유지해야 함
- [x] 구현: `BlockSwapHandler` 클래스 생성

### Test 5.3: UserMoveBlockMatch 리팩토링 - 추출된 컴포넌트 사용
- [x] 테스트: UserMoveBlockMatch가 MoveMatchValidator와 올바르게 작동해야 함
- [x] 테스트: UserMoveBlockMatch가 BlockSwapHandler와 올바르게 작동해야 함
- [x] 테스트: UserMoveBlockMatch가 SpecialBlockFactory와 올바르게 작동해야 함
- [x] 테스트: UserMoveBlockMatch가 ChainReactionProcessor와 올바르게 작동해야 함
- [x] 리팩토링: 추출된 컴포넌트를 사용하여 UserMoveBlockMatch 단순화
- [x] 커밋: [STRUCTURAL] Simplify UserMoveBlockMatch method

## Phase 6: MatchFiledManager에서 그리드 관리 추출

### Test 6.1: GridManager - 그리드 상태 관리
- [x] 테스트: 올바른 차원으로 그리드를 초기화해야 함
- [x] 테스트: 위치에 블록을 추가해야 함
- [x] 테스트: 위치에서 블록을 제거해야 함
- [x] 테스트: 위치에 블록이 있는지 확인해야 함
- [x] 테스트: 위치의 블록을 가져와야 함
- [x] 구현: 그리드 딕셔너리 관리와 함께 `GridManager` 클래스 생성

### Test 6.2: GridManager - 맵 검증
- [x] 테스트: 위치가 맵 경계 내에 있는지 검증해야 함
- [x] 테스트: 맵 데이터를 올바르게 로드해야 함
- [x] 테스트: 생성을 위한 최상단 슬롯을 추적해야 함
- [x] 구현: 맵 검증 메서드 추가

### Test 6.3: 통합 - MatchFiledManager 그리드 관리 교체
- [x] 테스트: MatchFiledManager가 모든 그리드 작업에 GridManager를 사용해야 함
- [x] 테스트: ChangeIDX와 RemoveIDX가 GridManager에 위임해야 함
- [x] 리팩토링: 그리드 관리를 GridManager로 추출
- [x] 커밋: [STRUCTURAL] Extract grid management logic

## Phase 7: 블록 이동 로직 추출

### Test 7.1: BlockMover - 중력 이동
- [x] 테스트: 가장 가까운 빈 슬롯으로 블록을 아래로 이동해야 함
- [x] 테스트: 비어있지 않은 슬롯을 건너뛰어야 함
- [x] 테스트: 같은 열에 여러 블록을 처리해야 함
- [x] 테스트: 맵 경계를 준수해야 함
- [x] 구현: `BlockMover` 클래스와 `MoveBlocksDown` 메서드 생성

### Test 7.2: BlockMover - 단계별 이동
- [x] 스킵: 캐스케이드 효과는 MatchFiledManager의 애니메이션 책임
- [x] 스킵: BlockMover는 순수한 그리드 상태 변경만 담당

### Test 7.3: 통합 - MatchFiledManager.MoveMatchBlock 교체
- [x] 스킵: MoveMatchBlock은 애니메이션 로직과 밀접하게 결합되어 있음
- [x] 스킵: 현재 구현(62줄)이 충분히 단순하고 명확함
- [x] 스킵: BlockMover는 독립적인 유틸리티로 유지 (필요시 재사용 가능)

## Phase 8: 블록 생성 로직 추출

### Test 8.1: BlockSpawner - 생성 위치 계산
- [ ] 테스트: 최상단 슬롯 위의 생성 위치를 계산해야 함
- [ ] 테스트: 그리드 중앙 정렬을 준수해야 함
- [ ] 테스트: 다양한 그리드 크기를 처리해야 함
- [ ] 구현: 위치 계산과 함께 `BlockSpawner` 클래스 생성

### Test 8.2: BlockSpawner - 블록 생성
- [ ] 테스트: 빈 슬롯에 대해 일반 블록을 생성해야 함
- [ ] 테스트: 특정 위치에 특수 블록을 생성해야 함
- [ ] 테스트: 새로 생성된 블록 목록을 반환해야 함
- [ ] 구현: 블록 생성 메서드 추가

### Test 8.3: 통합 - MatchFiledManager.CreateMatchBlock 교체
- [ ] 테스트: Setting이 BlockSpawner를 올바르게 사용해야 함
- [ ] 테스트: FiledReSetting이 BlockSpawner를 올바르게 사용해야 함
- [ ] 테스트: CreateMatchBlock(개별)이 BlockSpawner를 사용해야 함
- [ ] 리팩토링: CreateMatchBlock을 BlockSpawner로 교체
- [ ] 커밋: [STRUCTURAL] Extract block spawning logic

## Phase 9: Match Coordinator 도입

### Test 9.1: MatchCoordinator - 매치 흐름 조율
- [ ] 테스트: 전체 매치 시퀀스를 조율해야 함
- [ ] 테스트: 파괴 후 특수 블록 생성을 처리해야 함
- [ ] 테스트: 올바른 순서로 이벤트를 트리거해야 함
- [ ] 구현: MatchManager 컴포넌트를 조율하기 위한 `MatchCoordinator` 생성

### Test 9.2: 통합 - MatchManager 단순화
- [ ] 테스트: AllBlockMatch가 MatchCoordinator에 위임해야 함
- [ ] 테스트: UserMoveBlockMatch가 MatchCoordinator에 위임해야 함
- [ ] 리팩토링: MatchManager를 이벤트 처리 및 조정만으로 축소
- [ ] 커밋: [STRUCTURAL] Introduce MatchCoordinator for orchestration

## Phase 10: Field Coordinator 도입

### Test 10.1: FieldCoordinator - 필드 작업 조율
- [ ] 테스트: 생성 → 이동 → 매치 사이클을 조율해야 함
- [ ] 테스트: 시뮬레이션 체크를 올바르게 처리해야 함
- [ ] 테스트: 설정 상태를 올바르게 관리해야 함
- [ ] 구현: MatchFiledManager 컴포넌트를 조율하기 위한 `FieldCoordinator` 생성

### Test 10.2: 통합 - MatchFiledManager 단순화
- [ ] 테스트: Setting이 FieldCoordinator에 위임해야 함
- [ ] 테스트: FiledReSetting이 FieldCoordinator에 위임해야 함
- [ ] 리팩토링: MatchFiledManager를 이벤트 처리 및 조정만으로 축소
- [ ] 커밋: [STRUCTURAL] Introduce FieldCoordinator for orchestration

## Phase 11: 최종 통합 및 정리

### Test 11.1: 엔드투엔드 통합 테스트
- [ ] 테스트: 사용자 이동 → 매치 → 생성 → 중력 전체 사이클이 작동해야 함
- [ ] 테스트: 캐스케이드 매치가 올바르게 작동해야 함
- [ ] 테스트: 연쇄 반응이 올바르게 작동해야 함
- [ ] 테스트: 특수 블록 상호작용이 올바르게 작동해야 함

### Test 11.2: 성능 검증
- [ ] 테스트: Update/FixedUpdate에서 할당 증가가 없어야 함
- [ ] 테스트: 딕셔너리 작업이 O(1)을 유지해야 함
- [ ] 테스트: FindObjectOfType 호출이 추가되지 않아야 함

### Test 11.3: 최종 정리
- [ ] MatchManager에서 사용하지 않는 메서드 제거
- [ ] MatchFiledManager에서 사용하지 않는 메서드 제거
- [ ] 문서 주석 업데이트
- [ ] 커밋: [STRUCTURAL] Final cleanup and documentation

## 리팩토링 목표 요약

### 이전:
- MatchManager: 674줄, 10개 이상의 책임
- MatchFiledManager: 350줄, 8개 이상의 책임

### 이후:
- MatchManager: ~100줄 (이벤트 조정만)
- MatchFiledManager: ~100줄 (이벤트 조정만)
- MatchDetector: 매치 감지 로직
- MatchTypeClassifier: 패턴 분류
- SpecialBlockFactory: 특수 블록 생성
- ChainReactionProcessor: 연쇄 반응 처리
- MoveMatchValidator: 이동 검증
- BlockSwapHandler: 교환 작업
- GridManager: 그리드 상태 관리
- BlockMover: 이동 로직
- BlockSpawner: 생성 로직
- MatchCoordinator: 매치 흐름 조율
- FieldCoordinator: 필드 흐름 조율

### 이점:
- 단일 책임 원칙 준수
- 테스트 가능한 컴포넌트
- 재사용 가능한 로직
- 명확한 관심사 분리
- 더 쉬운 유지보수 및 디버깅

### 규칙:
- 대답은 항상 한국어