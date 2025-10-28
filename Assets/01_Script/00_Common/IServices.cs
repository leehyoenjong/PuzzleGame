using System.Collections.Generic;

/// <summary>
/// 매치 감지 서비스 인터페이스
/// 그리드에서 수평, 수직, 십자 매치 패턴을 감지합니다.
/// </summary>
public interface IMatchDetector
{
    /// <summary>
    /// 수평 매치를 감지합니다.
    /// </summary>
    /// <param name="grid">검색할 그리드</param>
    /// <param name="startposition">검색 시작 위치</param>
    /// <returns>매치된 블록 위치 리스트 (3개 이상) 또는 null</returns>
    List<(int, int)> DetectHorizontalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition);

    /// <summary>
    /// 수직 매치를 감지합니다.
    /// </summary>
    /// <param name="grid">검색할 그리드</param>
    /// <param name="startposition">검색 시작 위치</param>
    /// <returns>매치된 블록 위치 리스트 (3개 이상) 또는 null</returns>
    List<(int, int)> DetectVerticalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition);

    /// <summary>
    /// 십자 매치를 감지합니다.
    /// </summary>
    /// <param name="grid">검색할 그리드</param>
    /// <param name="centerposition">십자 중심 위치</param>
    /// <returns>십자 매치 결과 또는 null</returns>
    CrossMatchResult? DetectCrossMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) centerposition);
}

/// <summary>
/// 매치 타입 분류 서비스 인터페이스
/// 매치된 블록들을 분석하여 3-매치, 4-매치, 5-매치, 십자 패턴으로 분류합니다.
/// </summary>
public interface IMatchTypeClassifier
{
    /// <summary>
    /// 매치된 블록 리스트를 분석하여 매치 타입을 분류합니다.
    /// </summary>
    /// <param name="xlist">가로 방향 매치 블록 리스트</param>
    /// <param name="ylist">세로 방향 매치 블록 리스트</param>
    /// <returns>분류된 매치 타입</returns>
    EMATCHTYPE ClassifyMatchType(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist);
}

/// <summary>
/// 특수 블록 생성 팩토리 서비스 인터페이스
/// 매치 타입에 따라 특수 블록 생성 요청을 생성합니다.
/// </summary>
public interface ISpecialBlockFactory
{
    /// <summary>
    /// 매치 타입과 블록 정보를 기반으로 특수 블록 생성 요청을 생성합니다.
    /// </summary>
    /// <param name="xlist">가로 방향 매치 블록 리스트</param>
    /// <param name="ylist">세로 방향 매치 블록 리스트</param>
    /// <param name="matchtype">매치 타입</param>
    /// <param name="usermoveblock">사용자가 이동한 블록 (선택)</param>
    /// <returns>특수 블록 생성 요청 또는 null</returns>
    SpecialBlockCreationRequest? CreateRequest(
        List<UI_Match_Block> xlist,
        List<UI_Match_Block> ylist,
        EMATCHTYPE matchtype,
        UI_Match_Block usermoveblock = null);
}

/// <summary>
/// 연쇄 반응 처리 서비스 인터페이스
/// 특수 블록의 효과와 연쇄 반응을 처리합니다.
/// </summary>
public interface IChainReactionProcessor
{
    /// <summary>
    /// 특수 블록의 효과를 처리하여 영향받는 블록 리스트를 반환합니다.
    /// </summary>
    /// <param name="specialblock">특수 블록</param>
    /// <param name="matchblockdic">그리드 딕셔너리</param>
    /// <param name="targetcolor">목표 색상 (FIVE 블록용, 선택)</param>
    /// <returns>영향받은 블록 리스트</returns>
    List<UI_Match_Block> ProcessEffect(UI_Match_Block specialblock, Dictionary<(int, int), UI_Match_Block> matchblockdic, EBLOCKCOLORTYPE? targetcolor = null);

    /// <summary>
    /// 초기 블록 리스트로부터 연쇄 반응을 처리합니다.
    /// </summary>
    /// <param name="initialblocks">초기 파괴 블록 리스트</param>
    /// <param name="matchblockdic">그리드 딕셔너리</param>
    /// <returns>최종 파괴 블록 리스트</returns>
    List<UI_Match_Block> ProcessChainReaction(List<UI_Match_Block> initialblocks, Dictionary<(int, int), UI_Match_Block> matchblockdic);
}

/// <summary>
/// 그리드 관리 서비스 인터페이스
/// 게임 그리드의 상태를 관리합니다 (블록 추가, 제거, 조회 등).
/// </summary>
public interface IGridManager
{
    /// <summary>
    /// 그리드 너비
    /// </summary>
    int Width { get; }

    /// <summary>
    /// 그리드 높이
    /// </summary>
    int Height { get; }

    /// <summary>
    /// 그리드를 초기화합니다.
    /// </summary>
    /// <param name="width">그리드 너비</param>
    /// <param name="height">그리드 높이</param>
    void Initialize(int width, int height);

    /// <summary>
    /// 맵 데이터를 로드합니다.
    /// </summary>
    /// <param name="mapdata">맵 데이터 문자열 배열</param>
    void LoadMapData(string[] mapdata);

    /// <summary>
    /// 지정된 위치에 블록을 추가합니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="block">블록</param>
    void AddBlock((int, int) position, UI_Match_Block block);

    /// <summary>
    /// 지정된 위치에 블록이 있는지 확인합니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <returns>블록 존재 여부</returns>
    bool HasBlock((int, int) position);

    /// <summary>
    /// 지정된 위치의 블록을 가져옵니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <returns>블록 또는 null</returns>
    UI_Match_Block GetBlock((int, int) position);

    /// <summary>
    /// 지정된 위치의 블록을 제거합니다.
    /// </summary>
    /// <param name="position">위치</param>
    void RemoveBlock((int, int) position);

    /// <summary>
    /// 지정된 위치에 블록을 설정합니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="block">블록</param>
    void SetBlock((int, int) position, UI_Match_Block block);

    /// <summary>
    /// 모든 블록을 가져옵니다.
    /// </summary>
    /// <returns>블록 컬렉션</returns>
    IEnumerable<UI_Match_Block> GetAllBlocks();

    /// <summary>
    /// 그리드 딕셔너리를 가져옵니다.
    /// </summary>
    /// <returns>그리드 딕셔너리</returns>
    Dictionary<(int, int), UI_Match_Block> GetGridDictionary();

    /// <summary>
    /// 위치가 그리드 범위 내에 유효한지 확인합니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <returns>유효 여부</returns>
    bool IsValidPosition((int x, int y) position);

    /// <summary>
    /// 위치가 유효한 슬롯인지 확인합니다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <returns>유효한 슬롯 여부</returns>
    bool IsValidSlot((int, int) position);

    /// <summary>
    /// 각 열의 최상단 슬롯 목록을 가져옵니다.
    /// </summary>
    /// <returns>최상단 슬롯 위치 리스트</returns>
    List<(int, int)> GetTopSlots();
}

/// <summary>
/// 이동 매치 검증 서비스 인터페이스
/// 사용자 블록 이동이 유효한 매치를 생성하는지 검증합니다.
/// </summary>
public interface IMoveMatchValidator
{
    /// <summary>
    /// 두 블록의 교환이 유효한 매치를 생성하는지 검증합니다.
    /// </summary>
    /// <param name="grid">그리드 딕셔너리</param>
    /// <param name="pos1">첫 번째 블록 위치</param>
    /// <param name="pos2">두 번째 블록 위치</param>
    /// <returns>유효한 이동 여부</returns>
    bool ValidateMove(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2);
}

/// <summary>
/// 블록 교환 처리 서비스 인터페이스
/// 블록 교환 및 롤백 작업을 처리합니다.
/// </summary>
public interface IBlockSwapHandler
{
    /// <summary>
    /// 두 블록의 위치를 교환합니다.
    /// </summary>
    /// <param name="grid">그리드 딕셔너리</param>
    /// <param name="pos1">첫 번째 블록 위치</param>
    /// <param name="pos2">두 번째 블록 위치</param>
    void SwapBlocks(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2);

    /// <summary>
    /// 블록 교환을 롤백합니다.
    /// </summary>
    /// <param name="grid">그리드 딕셔너리</param>
    /// <param name="pos1">첫 번째 블록 위치</param>
    /// <param name="pos2">두 번째 블록 위치</param>
    void RollbackSwap(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2);
}
