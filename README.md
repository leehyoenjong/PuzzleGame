# Unity Match-3 Puzzle Game

Unity 기반 Match-3 퍼즐 게임 프로젝트입니다. TDD 방법론과 SOLID 원칙을 적용한 포트폴리오 프로젝트입니다.

---

## 📋 프로젝트 개요

- **장르**: Match-3 Puzzle Game (Candy Crush 스타일)
- **엔진**: Unity 6000.0.58f2
- **언어**: C#
- **아키텍처**: Event-driven, Manager-based
- **개발 방법론**: TDD (Test-Driven Development)

---

## 🎯 주요 기능

### ✅ 구현 완료
- Match-3 기본 매치 감지 (3/4/5/Cross 패턴)
- 특수 블록 시스템 (FORE, FIVE, CROSS)
- 연쇄 반응 (Chain Reaction)
- 중력 및 블록 재생성
- 게임 클리어/오버 조건 관리
- 점수 및 이동 횟수 관리

### 🔧 개선 진행 중
- 아키텍처 리팩토링 (의존성 역전, UI-로직 분리)
- EventBus 도입 (Static Event 제거)
- 테스트 커버리지 향상 (15% → 70%)

---

## 📊 코드 품질 메트릭스

### 현재 상태
| 항목 | 현재 | 목표 | 상태 |
|------|------|------|------|
| **치명적 버그** | 0개 ✅ | 0개 | ✅ 완료 |
| **테스트 커버리지** | 15% | 70% | 🔄 진행 중 |
| **테스트 개수** | 217개 | 300개+ | 🔄 진행 중 |
| **테스트 통과율** | 100% | 100% | ✅ 유지 |
| **Static Events** | 26개 | 0개 | 📋 계획됨 |
| **God Objects** | 2개 | 0개 | 📋 계획됨 |

### 아키텍처 등급
- **현재**: C등급
- **목표**: A등급 (Phase 1-6 완료 후)

---

## 📁 프로젝트 구조

```
Assets/
└── 01_Script/
    ├── 00_Common/         # 공통 구조체, Enum, 유틸리티
    ├── 01_Core/           # 핵심 로직 (MatchDetector, GridManager 등)
    ├── 02_Manager/        # 게임 매니저들
    ├── 03_UI/             # UI 컴포넌트
    ├── 04_Pool/           # 오브젝트 풀링
    ├── 05_SO/             # ScriptableObjects
    ├── Editor/            # 에디터 툴
    └── Tests/             # 단위 테스트 (217개)

Documentation/
├── ANALYSIS_SUMMARY.md           # Gemini 아키텍처 분석 요약
├── CRITICAL_BUGS_TO_FIX.md       # 치명적 버그 상세 (✅ 수정 완료)
├── ARCHITECTURE_ANALYSIS.md      # 심층 아키텍처 분석 (40페이지)
├── REFACTORING_ROADMAP.md        # 리팩토링 로드맵 (50페이지)
└── DEPENDENCY_MAP.txt            # 의존성 맵 시각화

Plans/
├── tdd-test-plan.md              # TDD 단위 테스트 계획 (123개 완료)
└── plan.md                       # 아키텍처 개선 계획 (현재 문서)
```

---

## 🚀 시작하기

### 필수 요구사항
- Unity 6000.0.58f2 이상
- .NET Standard 2.1
- UniTask (Cysharp.Threading.Tasks)
- DOTween (Demigiant)

### 프로젝트 열기
```bash
# Unity Hub에서 프로젝트 추가
/Users/ihyeonjong/Desktop/Git/PuzzleGame
```

### 테스트 실행
```bash
# Unity CLI로 EditMode 테스트 실행
/Applications/Unity/Hub/Editor/6000.0.58f2/Unity.app/Contents/MacOS/Unity \
  -runTests -batchmode \
  -projectPath /Users/ihyeonjong/Desktop/Git/PuzzleGame \
  -testResults TestResults.xml \
  -testPlatform EditMode \
  -logFile -
```

---

## 📝 개발 계획

### ✅ Phase 0: 치명적 버그 수정 (완료)
- [x] GameConditionManager 메모리 누수 수정
- [x] MoveCountManager 메모리 누수 수정
- [x] 전체 테스트 통과 확인 (217/217)

### 📋 Phase 1: 인터페이스 추출 (예정)
- [ ] IServices.cs 생성 (7개 인터페이스)
- [ ] 기존 클래스 인터페이스 구현
- [ ] 인터페이스 계약 테스트 (25개)

### 📋 Phase 2: MatchManager 리팩토링 (예정)
- [ ] 의존성 주입 구조 변경
- [ ] 메서드 분해 (93줄 → 3개 메서드)
- [ ] 중복 코드 제거

### 📋 Phase 3-6: 전체 리팩토링 (예정)
- EventBus 도입
- UI-로직 분리
- MatchFiledManager 리팩토링
- 종합 테스트 및 문서화

**상세 계획**: [plan.md](plan.md) 참조

---

## 🧪 테스트 전략

### TDD 사이클
1. **Red**: 실패하는 테스트 작성
2. **Green**: 최소 구현으로 테스트 통과
3. **Refactor**: 코드 개선 및 중복 제거

### 테스트 분류
- **단위 테스트** (217개): 핵심 로직 검증
  - MatchDetector, SpecialBlockFactory, ChainReactionProcessor 등
- **통합 테스트** (계획 중): 전체 시스템 플로우 검증
- **성능 테스트** (계획 중): 대규모 그리드 처리

### 테스트 커버리지 목표
- 핵심 로직: 90% 이상
- Manager 레이어: 70% 이상
- UI 레이어: 50% 이상

---

## 🔍 알려진 이슈 및 개선 사항

### ✅ 해결 완료
- [x] **[CRITICAL]** GameConditionManager 메모리 누수 (OnDisable에서 += 대신 -= 사용)
- [x] **[CRITICAL]** MoveCountManager 메모리 누수 (OnDisable에서 += 대신 -= 사용)

### 🔄 개선 진행 중
- [ ] **[HIGH]** Static Event 과다 사용 (26개) → EventBus로 전환 예정
- [ ] **[HIGH]** God Object 존재 (MatchManager 402줄, MatchFiledManager 401줄)
- [ ] **[MEDIUM]** UI-로직 결합 (UI_Match_Block에 게임 상태 저장)

### 📋 향후 개선 사항
- [ ] 의존성 주입 (DI) 패턴 적용
- [ ] 서비스 레이어 분리
- [ ] 성능 최적화 (100x100 그리드 지원)

**상세 분석**: [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md) 참조

---

## 🎓 학습 및 적용된 기술

### 디자인 패턴
- ✅ **Observer Pattern**: 이벤트 기반 통신
- ✅ **Factory Pattern**: SpecialBlockFactory
- ✅ **Object Pool Pattern**: 블록 재사용
- 📋 **Dependency Inversion**: 인터페이스 추상화 (예정)
- 📋 **Mediator Pattern**: EventBus (예정)

### SOLID 원칙
- ✅ **Single Responsibility**: 핵심 로직 클래스들
- 📋 **Open/Closed**: 인터페이스 확장 (예정)
- 📋 **Liskov Substitution**: 인터페이스 구현 (예정)
- 📋 **Interface Segregation**: 세분화된 인터페이스 (예정)
- 📋 **Dependency Inversion**: DI 패턴 (예정)

### Unity 특화 기술
- UniTask를 이용한 비동기 처리
- DOTween을 이용한 애니메이션
- ScriptableObject 기반 데이터 관리
- Unity Test Framework (EditMode/PlayMode)

---

## 📚 문서

### 아키텍처 분석 (Gemini AI)
- [ANALYSIS_SUMMARY.md](ANALYSIS_SUMMARY.md) - 전체 요약 (20페이지)
- [CRITICAL_BUGS_TO_FIX.md](CRITICAL_BUGS_TO_FIX.md) - 치명적 버그 상세
- [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md) - 심층 분석 (40페이지)
- [REFACTORING_ROADMAP.md](REFACTORING_ROADMAP.md) - 리팩토링 계획 (50페이지)
- [DEPENDENCY_MAP.txt](DEPENDENCY_MAP.txt) - 의존성 시각화

### 개발 계획
- [plan.md](plan.md) - 아키텍처 개선 계획 (현재 진행 중)
- [tdd-test-plan.md](tdd-test-plan.md) - TDD 테스트 계획 (123개 완료)

---

## 🤝 기여 및 협업

### 브랜치 전략
- `main`: 안정 버전
- `ClaudeCode리팩토링`: 현재 리팩토링 작업 브랜치

### 커밋 컨벤션
```
[TYPE] 간단한 설명

- 변경 사항 1
- 변경 사항 2

Benefits:
- 개선 효과

Tests: X개 추가, 전체 Y개 통과
```

**타입**:
- `FEATURE`: 새 기능
- `BUGFIX`: 버그 수정
- `REFACTOR`: 리팩토링
- `TEST`: 테스트 추가/수정
- `DOCS`: 문서 업데이트

---

## 📊 프로젝트 타임라인

```
2025-10-24: Phase 0 완료 (치명적 버그 2개 수정)
2025-10-24: Gemini 아키텍처 분석 완료 (110페이지 문서)
2025-10-24: Phase 1-6 계획 수립
```

---

## 🎯 포트폴리오 하이라이트

### 기술적 성과
- ✅ **TDD 실천**: 217개 단위 테스트 작성 및 100% 통과
- ✅ **아키텍처 분석**: Gemini AI를 활용한 체계적 코드 분석
- ✅ **버그 발견 및 수정**: 치명적 메모리 누수 2건 발견 및 수정
- 🔄 **대규모 리팩토링**: 400줄+ God Object 분해 (진행 중)

### 학습 역량
- Unity 6000 최신 기능 활용
- Kent Beck의 TDD 및 Tidy First 원칙 적용
- SOLID 원칙 및 디자인 패턴 실전 적용
- Git을 이용한 체계적 버전 관리

---

## 📞 연락처

- **개발자**: ihyeonjong
- **프로젝트**: Unity Match-3 Puzzle Game
- **GitHub**: [Repository Link]
- **문서**: [Documentation](Documentation/)

---

**마지막 업데이트**: 2025-10-24
**버전**: 0.2.0 (Phase 0 완료)
