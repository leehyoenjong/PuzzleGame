using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FiledManager : MonoBehaviour
{
    [SerializeField] private TextAsset _mapData;

    int _width;
    int _height;

    public int GetWidth() => _width;
    public int GetHeight() => _height;

    private List<(int x, int y)> _mapdata = new List<(int, int)>();
    public IReadOnlyList<(int x, int y)> GetMapData() => _mapdata;

    private Dictionary<int, int> _topslotlistxy = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> GetTopSlotXY() => _topslotlistxy;

    void Awake()
    {
        ParseMapData();
    }

    private void ParseMapData()
    {
        if (_mapData == null)
        {
            Debug.LogError("Map Data (TextAsset) is not assigned to FiledManager!");
            return;
        }

        var lines = _mapData.text.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        _height = lines.Count;
        _width = 0;

        for (int y = 0; y < _height; y++)
        {
            var cells = lines[y].Trim().Split(' ');
            if (cells.Length > _width) _width = cells.Length;

            for (int x = 0; x < cells.Length; x++)
            {
                if (cells[x] == "1")
                {
                    var key = (x, y);
                    _mapdata.Add(key);

                    // 각 열의 가장 높은 슬롯 y좌표 저장 (y값이 작을수록 위)
                    if (!_topslotlistxy.ContainsKey(x) || y < _topslotlistxy[x])
                    {
                        _topslotlistxy[x] = y;
                    }
                }
            }
        }
    }

    public bool CheckMap((int, int) key)
    {
        return _mapdata.Contains(key);
    }
}