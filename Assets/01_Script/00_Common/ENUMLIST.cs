public enum EMATCHTYPE
{
    THREE,
    FORE_UPDOWN,
    FORE_LEFTRIGHT,
    FIVE,
    CROSS_THREE,
    CROSS_FOUR,
    CROSS_FIVE,
    MAX
}

public enum EBLOCKCOLORTYPE
{
    RED,
    BLUE,
    YELLOW,
    PINK,
    GREEN,
    MAX,

    //여기서 부턴 특수 타입
    FIVE
}

public enum EGAMECLEARCONDITION
{
    SCORE,//점수
    BLOCKBRAKE, //블록 제거 갯수
}

public enum EGAMEOVERCONDITION
{
    MOVECOUNT, //이동 횟수
}

public enum EMATCHING
{
    NONE,
    ING,
    STOP,
    MAX_MATCHEND,
    MAX_NOMATCHEND
}