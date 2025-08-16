
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct St_GameClearCondition
{
    public EGAMECLEARCONDITION _condtion;
    public int _score;
    public int _blockbreak;


    public string GetConditionExplain()
    {
        switch (_condtion)
        {
            case EGAMECLEARCONDITION.SCORE:
                return $"ClearCondition - Score: {_score}";
            case EGAMECLEARCONDITION.BLOCKBRAKE:
                return $"ClearCondition - BlockBreak: {_blockbreak}";
        }

        return null;
    }
}
[Serializable]
public struct St_GameOverCondtion
{
    public EGAMEOVERCONDITION _condtion;
    public int _movecount;

    public string GetConditionExplain()
    {
        switch (_condtion)
        {
            case EGAMEOVERCONDITION.MOVECOUNT:
                return $"MoveCount: {_movecount}";
        }

        return null;
    }
}

[Serializable]
public struct St_GameData
{
    public int _score;
    public int _blockbreak;
    public int _movecount;
}

[Serializable]
public struct St_ChapterData
{
    public int _chapternumber;
    public TextAsset _chapterdata;
    public St_GameClearCondition _clear_condition;
    public St_GameOverCondtion _over_condition;


    public string GetChapterNumber()
    {
        var number = _chapternumber + 1;
        return number.ToString();
    }

    public (int width, int height, List<(int x, int y)> mapdata, Dictionary<int, int> topslotlistxy) GetMapData()
    {
        if (_chapterdata == null)
        {
            Debug.LogError("Map Data (TextAsset) is not assigned to FiledManager!");
            return default;
        }

        List<(int x, int y)> mapdata = new List<(int, int)>();
        Dictionary<int, int> topslotlistxy = new Dictionary<int, int>();

        var lines = _chapterdata.text.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        var height = lines.Count;
        var width = 0;

        for (int y = 0; y < height; y++)
        {
            var cells = lines[y].Trim().Split(' ');
            if (cells.Length > width) width = cells.Length;

            for (int x = 0; x < cells.Length; x++)
            {
                if (cells[x] == "1")
                {
                    var key = (x, y);
                    mapdata.Add(key);

                    // 각 열의 가장 높은 슬롯 y좌표 저장 (y값이 작을수록 위)
                    if (!topslotlistxy.ContainsKey(x) || y < topslotlistxy[x])
                    {
                        topslotlistxy[x] = y;
                    }
                }
            }
        }

        return (width, height, mapdata, topslotlistxy);
    }
}