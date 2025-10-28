using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 그리드의 상태를 관리하는 클래스
/// 블록 추가, 제거, 조회 및 맵 검증을 담당합니다.
/// </summary>
public class GridManager : IGridManager
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Dictionary<(int, int), UI_Match_Block> _grid;
    private HashSet<(int, int)> _validslots;

    public void Initialize(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new Dictionary<(int, int), UI_Match_Block>();
        _validslots = new HashSet<(int, int)>();
    }

    public void LoadMapData(string[] mapdata)
    {
        if (mapdata == null || mapdata.Length == 0)
            return;

        Height = mapdata.Length;
        var firstrow = mapdata[0].Split(' ');
        Width = firstrow.Length;

        _grid = new Dictionary<(int, int), UI_Match_Block>();
        _validslots = new HashSet<(int, int)>();

        for (int y = 0; y < Height; y++)
        {
            var row = mapdata[y].Split(' ');
            for (int x = 0; x < row.Length; x++)
            {
                if (row[x] == "1")
                {
                    _validslots.Add((x, y));
                }
            }
        }
    }

    public void AddBlock((int, int) position, UI_Match_Block block)
    {
        _grid[position] = block;
    }

    public bool HasBlock((int, int) position)
    {
        return _grid.ContainsKey(position) && _grid[position] != null;
    }

    public UI_Match_Block GetBlock((int, int) position)
    {
        return _grid.TryGetValue(position, out var block) ? block : null;
    }

    public void RemoveBlock((int, int) position)
    {
        _grid.Remove(position);
    }

    public void SetBlock((int, int) position, UI_Match_Block block)
    {
        _grid[position] = block;
    }

    public IEnumerable<UI_Match_Block> GetAllBlocks()
    {
        return _grid.Values;
    }

    public Dictionary<(int, int), UI_Match_Block> GetGridDictionary()
    {
        return _grid;
    }

    public bool IsValidPosition((int x, int y) position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

    public bool IsValidSlot((int, int) position)
    {
        return _validslots != null && _validslots.Contains(position);
    }

    public List<(int, int)> GetTopSlots()
    {
        if (_validslots == null)
            return new List<(int, int)>();

        var topslots = new List<(int, int)>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_validslots.Contains((x, y)))
                {
                    topslots.Add((x, y));
                    break; // 각 열의 첫 번째 유효 슬롯만 추가
                }
            }
        }

        return topslots;
    }
}
