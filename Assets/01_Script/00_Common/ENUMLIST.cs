public enum EMATCHTYPE
{
    THREE,
    FORE,
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
    MAX
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