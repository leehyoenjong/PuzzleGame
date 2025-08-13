
using System;

[Serializable]
public struct St_GameClearCondition
{
    public EGAMECLEARCONDITION _condtion;
    public int _score;
    public int _blockbreak;
}
[Serializable]
public struct St_GameOverCondtion
{
    public EGAMEOVERCONDITION _condtion;
    public int _movecount;
}

[Serializable]
public struct St_GameData
{
    public int _score;
    public int _blockbreak;
    public int _movecount;
}