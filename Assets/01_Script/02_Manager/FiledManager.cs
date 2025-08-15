using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FiledManager : MonoBehaviour
{
    [SerializeField] private TextAsset _mapData;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private List<(int x, int y)> _mapdata = new List<(int, int)>();
    public IReadOnlyList<(int x, int y)> GetMapData() => _mapdata;

    private Dictionary<int, int> _topslotlist = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> GetTopSlot() => _topslotlist;

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
        Height = lines.Count;
        Width = 0;

        for (int y = 0; y < Height; y++)
        {
            var cells = lines[y].Trim().Split(' ');
            if (cells.Length > Width) Width = cells.Length;

            for (int x = 0; x < cells.Length; x++)
            {
                if (cells[x] == "1")
                {
                    var key = (x, y);
                    _mapdata.Add(key);

                    // 각 열의 가장 높은 슬롯 y좌표 저장 (y값이 작을수록 위)
                    if (!_topslotlist.ContainsKey(x) || y < _topslotlist[x])
                    {
                        _topslotlist[x] = y;
                    }
                }
            }
        }
    }
}