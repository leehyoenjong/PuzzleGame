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

    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void OnEnable()
    {
        InitializeGrid();
    }

    private void OnGUI()
    {
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
        if (GUILayout.Button("Save to File"))
        {
            SaveGridToFile();
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
}
