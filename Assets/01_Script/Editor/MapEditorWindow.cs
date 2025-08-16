using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class MapEditorWindow : EditorWindow
{
    private int _width = 10;
    private int _height = 10;
    private bool[,] _grid;
    private Vector2 _scrollPosition;
    private TextAsset _targetFile;


    /// <summary>
    /// 특정 파일과 함께 Map Editor를 엽니다.
    /// </summary>
    public static void ShowWindowWithFile(TextAsset targetFile)
    {
        var window = GetWindow<MapEditorWindow>("Map Editor");
        window.SetTargetFile(targetFile);
        window.Show();
    }

    /// <summary>
    /// 타겟 파일을 설정하고 자동으로 로드합니다.
    /// </summary>
    public void SetTargetFile(TextAsset targetFile)
    {
        _targetFile = targetFile;
        if (_targetFile != null)
        {
            LoadGridFromFile();
        }
    }

    /// <summary>
    /// 외부에서 챕터 데이터를 업데이트합니다.
    /// </summary>
    public static void UpdateChapterData(TextAsset newTargetFile)
    {
        // 현재 열려있는 MapEditorWindow 찾기
        var windows = Resources.FindObjectsOfTypeAll<MapEditorWindow>();
        foreach (var window in windows)
        {
            if (window._targetFile != null && newTargetFile != null)
            {
                // 파일이 다르면 업데이트
                if (window._targetFile != newTargetFile)
                {
                    window.SetTargetFile(newTargetFile);
                    window.Repaint();
                }
                break;
            }
        }
    }

    private void OnEnable()
    {
        InitializeGrid();
    }

    private void OnGUI()
    {
        // 키보드 단축키 처리
        HandleKeyboardInput();

        EditorGUILayout.LabelField("Map Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- 파일 및 크기 설정 ---
        EditorGUILayout.BeginHorizontal();
        _targetFile = (TextAsset)EditorGUILayout.ObjectField("Map File", _targetFile, typeof(TextAsset), false);
        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            LoadGridFromFile();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _width = EditorGUILayout.IntField("Width", _width);
        _height = EditorGUILayout.IntField("Height", _height);
        if (GUILayout.Button("Apply Size", GUILayout.Width(100)))
        {
            InitializeGrid();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // --- 그리드 편집 ---
        EditorGUILayout.LabelField("Grid Data (Check = Slot exists)");

        // --- 전체 선택/해제 ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check All"))
        {
            SetAllGridValues(true);
        }
        if (GUILayout.Button("Uncheck All"))
        {
            SetAllGridValues(false);
        }
        EditorGUILayout.EndHorizontal();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));

        if (_grid != null)
        {
            // --- Column Headers (열 전체 선택/해제) ---
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(88); // Row headers 너비만큼 공간 확보
            for (int x = 0; x < _width; x++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(20));
                if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SetColumnValues(x, true);
                }
                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SetColumnValues(x, false);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // --- Row Data ---
            for (int y = 0; y < _height; y++)
            {
                EditorGUILayout.BeginHorizontal();

                // --- Row Headers (행 전체 선택/해제) ---
                EditorGUILayout.LabelField($"Row {y}", GUILayout.Width(40));
                if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SetRowValues(y, true);
                }
                if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SetRowValues(y, false);
                }

                for (int x = 0; x < _width; x++)
                {
                    _grid[x, y] = EditorGUILayout.Toggle(_grid[x, y], GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // --- 저장 ---
        EditorGUILayout.BeginHorizontal();

        // 현재 파일에 저장
        GUI.backgroundColor = Color.green;
        bool canSave = _targetFile != null;
        EditorGUI.BeginDisabledGroup(!canSave);
        if (GUILayout.Button("Save (Ctrl+S)", GUILayout.Height(25)))
        {
            SaveToCurrentFile();
        }
        EditorGUI.EndDisabledGroup();
        GUI.backgroundColor = Color.white;

        // 새 파일로 저장
        if (GUILayout.Button("Save As (Ctrl+Shift+S)", GUILayout.Height(25)))
        {
            SaveGridToFile();
        }

        EditorGUILayout.EndHorizontal();

        // 저장 상태 표시
        if (_targetFile != null)
        {
            EditorGUILayout.HelpBox($"Current File: {_targetFile.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No file loaded. Use 'Save As' to create a new file.", MessageType.Warning);
        }
    }

    private void InitializeGrid()
    {
        _grid = new bool[_width, _height];
    }

    private void SetAllGridValues(bool value)
    {
        if (_grid == null) return;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _grid[x, y] = value;
            }
        }
    }

    private void SetRowValues(int y, bool value)
    {
        if (_grid == null) return;
        for (int x = 0; x < _width; x++)
        {
            _grid[x, y] = value;
        }
    }

    private void SetColumnValues(int x, bool value)
    {
        if (_grid == null) return;
        for (int y = 0; y < _height; y++)
        {
            _grid[x, y] = value;
        }
    }

    private void LoadGridFromFile()
    {
        if (_targetFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a map file to load.", "OK");
            return;
        }

        var lines = _targetFile.text.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        if (lines.Count == 0) return;

        _height = lines.Count;
        _width = lines[0].Trim().Split(' ').Length;

        InitializeGrid();

        for (int y = 0; y < _height; y++)
        {
            var cells = lines[y].Trim().Split(' ');
            for (int x = 0; x < _width; x++)
            {
                if (x < cells.Length && cells[x] == "1")
                {
                    _grid[x, y] = true;
                }
            }
        }
        Repaint();
    }

    private void SaveGridToFile()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }

        string path = EditorUtility.SaveFilePanel("Save Map File", resourcesPath, "NewMap.txt", "txt");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int y = 0; y < _height; y++)
            {
                var line = new List<string>();
                for (int x = 0; x < _width; x++)
                {
                    line.Add(_grid[x, y] ? "1" : "0");
                }
                writer.WriteLine(string.Join(" ", line));
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Map file saved successfully!", "OK");

        // 방금 저장한 파일을 Target File로 지정
        string assetPath = path.Replace(Application.dataPath, "Assets");
        _targetFile = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
    }

    /// <summary>
    /// 현재 로드된 파일에 직접 저장합니다.
    /// </summary>
    private void SaveToCurrentFile()
    {
        if (_targetFile == null)
        {
            EditorUtility.DisplayDialog("Error", "저장할 파일이 지정되지 않았습니다.", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(_targetFile);
        string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));

        try
        {
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                for (int y = 0; y < _height; y++)
                {
                    var line = new List<string>();
                    for (int x = 0; x < _width; x++)
                    {
                        line.Add(_grid[x, y] ? "1" : "0");
                    }
                    writer.WriteLine(string.Join(" ", line));
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"파일이 성공적으로 저장되었습니다!\n\n{_targetFile.name}", "OK");

            Debug.Log($"맵 데이터가 현재 파일에 저장되었습니다: {assetPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"파일 저장 중 오류가 발생했습니다:\n{e.Message}", "OK");
        }
    }

    /// <summary>
    /// 키보드 단축키를 처리합니다.
    /// </summary>
    private void HandleKeyboardInput()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            // Ctrl+S: 현재 파일에 저장
            if (e.control && e.keyCode == KeyCode.S && !e.shift)
            {
                if (_targetFile != null)
                {
                    SaveToCurrentFile();
                    e.Use();
                }
            }
            // Ctrl+Shift+S: Save As
            else if (e.control && e.keyCode == KeyCode.S && e.shift)
            {
                SaveGridToFile();
                e.Use();
            }
            // Ctrl+O: 파일 로드
            else if (e.control && e.keyCode == KeyCode.O)
            {
                // 현재는 수동으로 ObjectField에서 파일을 선택해야 함
                // 추후 파일 브라우저 기능 추가 가능
                Debug.Log("Tip: Use the 'Map File' field to load a file, or Ctrl+S to save, Ctrl+Shift+S for Save As");
                e.Use();
            }
        }
    }
}
