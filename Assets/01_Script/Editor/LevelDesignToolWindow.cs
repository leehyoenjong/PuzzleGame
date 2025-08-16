using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelDesignToolWindow : EditorWindow
{
    private SO_Chapter _chapterAsset;
    private Vector2 _scrollPosition;
    private int _selectedChapterIndex = -1;
    private St_ChapterData _selectedChapterData;
    private const string CHAPTER_ASSET_PATH = "SO_Chapter";

    [MenuItem("Tools/Level Design Tool")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesignToolWindow>("Level Design Tool");
    }

    private void OnEnable()
    {
        LoadChapterAsset();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Design Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // SO_Chapter 에셋 상태 표시
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        _chapterAsset = (SO_Chapter)EditorGUILayout.ObjectField("Chapter Asset", _chapterAsset, typeof(SO_Chapter), false);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Reload", GUILayout.Width(80)))
        {
            LoadChapterAsset();
            _selectedChapterIndex = -1;
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        // SO_Chapter 에셋이 없을 때 생성 버튼 표시
        if (_chapterAsset == null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("SO_Chapter 에셋이 'Assets/Resources/SO_Chapter.asset' 경로에 없습니다.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create SO_Chapter Asset", GUILayout.Height(30), GUILayout.Width(200)))
            {
                CreateChapterAsset();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            return;
        }

        EditorGUILayout.Space();

        if (_chapterAsset._chapterdata == null || _chapterAsset._chapterdata.Count == 0)
        {
            EditorGUILayout.HelpBox("챕터 데이터가 없습니다. 새 챕터를 추가해보세요.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add New Chapter", GUILayout.Height(25), GUILayout.Width(150)))
            {
                AddNewChapter();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            return;
        }

        // 챕터 리스트 헤더
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Chapter List", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(25), GUILayout.Height(20)))
        {
            AddNewChapter();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));

        for (int i = 0; i < _chapterAsset._chapterdata.Count; i++)
        {
            var chapterData = _chapterAsset._chapterdata[i];

            EditorGUILayout.BeginHorizontal();

            // 챕터 번호와 정보 표시
            string buttonText = $"Chapter {chapterData._chapternumber}";
            if (chapterData._chapterdata != null)
            {
                buttonText += $" ({chapterData._chapterdata.name})";
            }

            // 선택된 챕터는 다른 색상으로 표시
            Color originalColor = GUI.backgroundColor;
            if (_selectedChapterIndex == i)
            {
                GUI.backgroundColor = Color.green;
            }

            if (GUILayout.Button(buttonText, GUILayout.Height(30)))
            {
                _selectedChapterIndex = i;
                _selectedChapterData = chapterData;
                
                // 열려있는 창들 업데이트
                UpdateOpenWindows();
            }

            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 선택된 챕터 정보 표시
        if (_selectedChapterIndex >= 0 && _selectedChapterIndex < _chapterAsset._chapterdata.Count)
        {
            ShowSelectedChapterInfo();
        }
        else
        {
            EditorGUILayout.HelpBox("챕터를 선택해주세요.", MessageType.Info);
        }
    }

    private void ShowSelectedChapterInfo()
    {
        EditorGUILayout.LabelField("Selected Chapter Info", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.BeginVertical("box");

        // 챕터 번호
        EditorGUILayout.LabelField("Chapter Number", _selectedChapterData._chapternumber.ToString());

        // 챕터 데이터 파일
        EditorGUILayout.ObjectField("Chapter Data File", _selectedChapterData._chapterdata, typeof(TextAsset), false);

        EditorGUILayout.Space();

        // 클리어 조건
        EditorGUILayout.LabelField("Clear Condition", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Type", _selectedChapterData._clear_condition._condtion.ToString());
        if (_selectedChapterData._clear_condition._condtion == EGAMECLEARCONDITION.SCORE)
        {
            EditorGUILayout.LabelField("Target Score", _selectedChapterData._clear_condition._score.ToString());
        }
        else if (_selectedChapterData._clear_condition._condtion == EGAMECLEARCONDITION.BLOCKBRAKE)
        {
            EditorGUILayout.LabelField("Target Block Break", _selectedChapterData._clear_condition._blockbreak.ToString());
        }

        EditorGUILayout.Space();

        // 게임오버 조건
        EditorGUILayout.LabelField("Game Over Condition", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Type", _selectedChapterData._over_condition._condtion.ToString());
        if (_selectedChapterData._over_condition._condtion == EGAMEOVERCONDITION.MOVECOUNT)
        {
            EditorGUILayout.LabelField("Max Move Count", _selectedChapterData._over_condition._movecount.ToString());
        }

        EditorGUILayout.EndVertical();

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // 액션 버튼들
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Edit Chapter", GUILayout.Height(30)))
        {
            // 챕터 편집 윈도우 열기
            ChapterEditWindow.ShowWindow(_chapterAsset, _selectedChapterIndex);
        }

        if (GUILayout.Button("View Map Data", GUILayout.Height(30)))
        {
            if (_selectedChapterData._chapterdata != null)
            {
                // Map Editor에서 파일을 직접 열기
                MapEditorWindow.ShowWindowWithFile(_selectedChapterData._chapterdata);
            }
            else
            {
                EditorUtility.DisplayDialog("No Map Data", "이 챕터에는 맵 데이터가 없습니다.", "OK");
            }
        }

        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Resources 폴더에서 SO_Chapter 에셋을 로드합니다.
    /// </summary>
    private void LoadChapterAsset()
    {
        _chapterAsset = Resources.Load<SO_Chapter>(CHAPTER_ASSET_PATH);

        if (_chapterAsset != null)
        {
            Debug.Log($"SO_Chapter 에셋을 성공적으로 로드했습니다: {AssetDatabase.GetAssetPath(_chapterAsset)}");
        }
        else
        {
            Debug.LogWarning($"SO_Chapter 에셋을 찾을 수 없습니다. 경로: Assets/Resources/{CHAPTER_ASSET_PATH}.asset");
        }
    }

    /// <summary>
    /// Resources 폴더에 새로운 SO_Chapter 에셋을 생성합니다.
    /// </summary>
    private void CreateChapterAsset()
    {
        // Resources 폴더 확인 및 생성
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.Refresh();
        }

        // SO_Chapter 인스턴스 생성
        SO_Chapter newChapterAsset = ScriptableObject.CreateInstance<SO_Chapter>();

        // 기본 챕터 데이터 추가 (예시)
        newChapterAsset._chapterdata = new List<St_ChapterData>();

        // 에셋 파일로 저장
        string assetPath = $"{resourcesPath}/{CHAPTER_ASSET_PATH}.asset";
        AssetDatabase.CreateAsset(newChapterAsset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 생성된 에셋 로드
        _chapterAsset = newChapterAsset;

        // 에셋 선택 (Project 창에서 하이라이트)
        Selection.activeObject = _chapterAsset;
        EditorGUIUtility.PingObject(_chapterAsset);

        Debug.Log($"새로운 SO_Chapter 에셋이 생성되었습니다: {assetPath}");
        EditorUtility.DisplayDialog("Success", $"SO_Chapter 에셋이 성공적으로 생성되었습니다!\n\n경로: {assetPath}", "OK");

        Repaint();
    }

    /// <summary>
    /// 새로운 챕터 데이터를 추가합니다.
    /// </summary>
    private void AddNewChapter()
    {
        if (_chapterAsset == null) return;

        // 챕터 리스트 초기화
        if (_chapterAsset._chapterdata == null)
        {
            _chapterAsset._chapterdata = new List<St_ChapterData>();
        }

        // 새 챕터 번호 계산 (기존 챕터 중 가장 큰 번호 + 1)
        int newChapterNumber = 1;
        if (_chapterAsset._chapterdata.Count > 0)
        {
            int maxChapterNumber = 0;
            foreach (var chapter in _chapterAsset._chapterdata)
            {
                if (chapter._chapternumber > maxChapterNumber)
                {
                    maxChapterNumber = chapter._chapternumber;
                }
            }
            newChapterNumber = maxChapterNumber + 1;
        }

        // 새 챕터 데이터 생성
        St_ChapterData newChapter = new St_ChapterData
        {
            _chapternumber = newChapterNumber,
            _chapterdata = null, // 추후 맵 데이터 할당
            _clear_condition = new St_GameClearCondition
            {
                _condtion = EGAMECLEARCONDITION.SCORE,
                _score = 1000,
                _blockbreak = 0
            },
            _over_condition = new St_GameOverCondtion
            {
                _condtion = EGAMEOVERCONDITION.MOVECOUNT,
                _movecount = 20
            }
        };

        // 챕터 추가
        _chapterAsset._chapterdata.Add(newChapter);

        // 에셋 저장
        EditorUtility.SetDirty(_chapterAsset);
        AssetDatabase.SaveAssets();

        // 새로 추가된 챕터 선택
        _selectedChapterIndex = _chapterAsset._chapterdata.Count - 1;
        _selectedChapterData = newChapter;
        
        // 열려있는 창들 업데이트
        UpdateOpenWindows();

        Debug.Log($"새 챕터가 추가되었습니다: Chapter {newChapterNumber}");

        Repaint();
    }
    
    /// <summary>
    /// 열려있는 Edit Chapter 및 Map Data Viewer 창들을 업데이트합니다.
    /// </summary>
    private void UpdateOpenWindows()
    {
        if (_selectedChapterIndex < 0 || _selectedChapterIndex >= _chapterAsset._chapterdata.Count)
            return;
            
        // Edit Chapter 창 업데이트
        ChapterEditWindow.UpdateChapterData(_chapterAsset, _selectedChapterIndex);
        
        // Map Data Viewer 창 업데이트
        if (_selectedChapterData._chapterdata != null)
        {
            string mapContent = _selectedChapterData._chapterdata.text;
            MapDataViewerWindow.UpdateChapterData(_selectedChapterData, mapContent);
            
            // Map Editor 창 업데이트
            MapEditorWindow.UpdateChapterData(_selectedChapterData._chapterdata);
        }
    }
}

// 맵 데이터 뷰어 윈도우
public class MapDataViewerWindow : EditorWindow
{
    private St_ChapterData _chapterData;
    private string _mapContent;
    private Vector2 _scrollPosition;

    public static void ShowWindow(St_ChapterData chapterData, string mapContent)
    {
        var window = GetWindow<MapDataViewerWindow>($"Map Data - Chapter {chapterData._chapternumber}");
        window._chapterData = chapterData;
        window._mapContent = mapContent;
        window.Show();
    }
    
    /// <summary>
    /// 외부에서 챕터 데이터를 업데이트합니다.
    /// </summary>
    public static void UpdateChapterData(St_ChapterData chapterData, string mapContent)
    {
        // 현재 열려있는 MapDataViewerWindow 찾기
        var windows = Resources.FindObjectsOfTypeAll<MapDataViewerWindow>();
        foreach (var window in windows)
        {
            // 같은 챕터 번호의 창 찾기
            if (window._chapterData._chapternumber == chapterData._chapternumber)
            {
                window.RefreshData(chapterData, mapContent);
                break;
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Map Data - Chapter {_chapterData._chapternumber}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Raw Map Data:");

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.TextArea(_mapContent, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Open in Map Editor"))
        {
            // Map Editor에서 파일을 직접 열기
            if (_chapterData._chapterdata != null)
            {
                MapEditorWindow.ShowWindowWithFile(_chapterData._chapterdata);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "챕터 데이터 파일이 없습니다.", "OK");
            }
        }
    }
    
    /// <summary>
    /// 외부에서 호출되어 챕터 데이터를 새로고침합니다.
    /// </summary>
    public void RefreshData(St_ChapterData chapterData, string mapContent)
    {
        _chapterData = chapterData;
        _mapContent = mapContent;
        
        // 윈도우 제목 업데이트
        string windowTitle = $"Map Data - Chapter {_chapterData._chapternumber}";
        this.titleContent = new GUIContent(windowTitle);
        
        // 화면 갱신
        Repaint();
    }
}

// 챕터 편집 윈도우
public class ChapterEditWindow : EditorWindow
{
    private SO_Chapter _chapterAsset;
    private int _chapterIndex;
    private St_ChapterData _editingChapter;
    private Vector2 _scrollPosition;

    // 편집 중인 데이터
    private St_GameClearCondition _editingClearCondition;
    private St_GameOverCondtion _editingGameOverCondition;
    private int _editingChapterNumber;
    private TextAsset _editingChapterData;
    
    // 중복 파일 검사 결과
    private bool _hasDuplicateFile = false;
    private List<int> _duplicateChapterNumbers = new List<int>();
    
    // 변경사항 추적
    private bool _hasUnsavedChanges = false;
    private St_GameClearCondition _originalClearCondition;
    private St_GameOverCondtion _originalGameOverCondition;
    private int _originalChapterNumber;
    private TextAsset _originalChapterData;

        public static void ShowWindow(SO_Chapter chapterAsset, int chapterIndex)
    {
        if (chapterAsset == null || chapterIndex < 0 || chapterIndex >= chapterAsset._chapterdata.Count)
        {
            EditorUtility.DisplayDialog("Error", "올바르지 않은 챕터 데이터입니다.", "OK");
            return;
        }
        
        var window = GetWindow<ChapterEditWindow>($"Edit Chapter {chapterAsset._chapterdata[chapterIndex]._chapternumber}");
        window.Initialize(chapterAsset, chapterIndex);
        window.Show();
    }
    
    /// <summary>
    /// 외부에서 챕터 데이터를 업데이트합니다.
    /// </summary>
    public static void UpdateChapterData(SO_Chapter chapterAsset, int chapterIndex)
    {
        // 현재 열려있는 ChapterEditWindow 찾기
        var windows = Resources.FindObjectsOfTypeAll<ChapterEditWindow>();
        foreach (var window in windows)
        {
            if (window._chapterAsset == chapterAsset)
            {
                window.RefreshData(chapterAsset, chapterIndex);
                break;
            }
        }
    }

        private void Initialize(SO_Chapter chapterAsset, int chapterIndex)
    {
        _chapterAsset = chapterAsset;
        _chapterIndex = chapterIndex;
        _editingChapter = _chapterAsset._chapterdata[_chapterIndex];
        
        // 편집용 데이터 복사
        _editingChapterNumber = _editingChapter._chapternumber;
        _editingChapterData = _editingChapter._chapterdata;
        _editingClearCondition = _editingChapter._clear_condition;
        _editingGameOverCondition = _editingChapter._over_condition;
        
        // 원본 데이터 저장 (변경 감지용)
        _originalChapterNumber = _editingChapter._chapternumber;
        _originalChapterData = _editingChapter._chapterdata;
        _originalClearCondition = _editingChapter._clear_condition;
        _originalGameOverCondition = _editingChapter._over_condition;
        
        // 초기화
        _hasUnsavedChanges = false;
        
        // 초기 중복 파일 검사
        CheckForDuplicateFile();
    }
    
    /// <summary>
    /// 외부에서 호출되어 챕터 데이터를 새로고침합니다.
    /// </summary>
    public void RefreshData(SO_Chapter chapterAsset, int chapterIndex)
    {
        // 저장되지 않은 변경사항이 있는지 확인
        if (_hasUnsavedChanges)
        {
            bool switchChapter = EditorUtility.DisplayDialog(
                "저장되지 않은 변경사항", 
                "현재 편집 중인 챕터에 저장되지 않은 변경사항이 있습니다.\n\n변경사항을 무시하고 다른 챕터로 전환하시겠습니까?", 
                "전환", 
                "취소"
            );
            
            if (!switchChapter)
            {
                return; // 전환 취소
            }
        }
        
        // 새로운 챕터 데이터로 초기화
        Initialize(chapterAsset, chapterIndex);
        
        // 윈도우 제목 업데이트
        string windowTitle = $"Edit Chapter {_editingChapterNumber}";
        this.titleContent = new GUIContent(windowTitle);
        
        // 화면 갱신
        Repaint();
    }

        private void OnGUI()
    {
        if (_chapterAsset == null)
        {
            EditorGUILayout.HelpBox("챕터 에셋이 null입니다.", MessageType.Error);
            return;
        }
        
        // 변경사항 감지
        CheckForChanges();
        
        // 윈도우 제목 업데이트
        string windowTitle = $"Edit Chapter {_editingChapterNumber}";
        if (_hasUnsavedChanges)
        {
            windowTitle += " *";
        }
        this.titleContent = new GUIContent(windowTitle);
        
        // 창 닫기 이벤트 처리
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            HandleCancel();
            Event.current.Use();
        }
        
        // 제목에 변경사항 표시
        string titleText = $"Edit Chapter {_editingChapterNumber}";
        if (_hasUnsavedChanges)
        {
            titleText += " *";
        }
        EditorGUILayout.LabelField(titleText, EditorStyles.boldLabel);
        
        // 저장 필요 알림
        if (_hasUnsavedChanges)
        {
            EditorGUILayout.HelpBox("⚠️ 저장되지 않은 변경사항이 있습니다!", MessageType.Warning);
        }
        
        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                // 기본 정보 편집
        EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        
        _editingChapterNumber = EditorGUILayout.IntField("Chapter Number", _editingChapterNumber);
        
        // Chapter Data File 섹션
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _editingChapterData = (TextAsset)EditorGUILayout.ObjectField("Chapter Data File", _editingChapterData, typeof(TextAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            CheckForDuplicateFile();
        }
        
        // 파일이 없을 때 생성 버튼 표시
        if (_editingChapterData == null)
        {
            if (GUILayout.Button("Create", GUILayout.Width(60), GUILayout.Height(18)))
            {
                CreateNewChapterDataFile();
            }
        }
        else
        {
            // 파일이 있을 때는 "View" 버튼 표시 (선택사항)
            if (GUILayout.Button("View", GUILayout.Width(60), GUILayout.Height(18)))
            {
                ViewChapterDataFile();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // 파일 상태 표시
        if (_editingChapterData == null)
        {
            EditorGUILayout.HelpBox("챕터 데이터 파일이 없습니다. 'Create' 버튼을 클릭하여 새 파일을 생성하세요.", MessageType.Warning);
            
            // 빠른 생성 옵션들
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quick Create:", GUILayout.Width(85));
            
            if (GUILayout.Button("10x10 Map", EditorStyles.miniButton))
            {
                CreateQuickMap(10, 10);
            }
            
            if (GUILayout.Button("8x8 Map", EditorStyles.miniButton))
            {
                CreateQuickMap(8, 8);
            }
            
            if (GUILayout.Button("Custom Size", EditorStyles.miniButton))
            {
                ShowCustomSizeDialog();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        else if (_hasDuplicateFile)
        {
            // 중복 파일 경고
            string duplicateInfo = "이 파일은 다음 챕터에서도 사용 중입니다: ";
            duplicateInfo += string.Join(", ", _duplicateChapterNumbers.ConvertAll(x => $"Chapter {x}"));
            EditorGUILayout.HelpBox($"❌ 중복 파일 감지!\n\n{duplicateInfo}\n\n다른 파일을 선택하거나 새로 생성해주세요.", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox($"✅ 파일: {_editingChapterData.name}", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Clear Condition 편집
        EditorGUILayout.LabelField("Clear Condition", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        _editingClearCondition._condtion = (EGAMECLEARCONDITION)EditorGUILayout.EnumPopup("Condition Type", _editingClearCondition._condtion);

        if (_editingClearCondition._condtion == EGAMECLEARCONDITION.SCORE)
        {
            _editingClearCondition._score = EditorGUILayout.IntField("Target Score", _editingClearCondition._score);
            _editingClearCondition._blockbreak = 0; // 다른 조건 초기화
        }
        else if (_editingClearCondition._condtion == EGAMECLEARCONDITION.BLOCKBRAKE)
        {
            _editingClearCondition._blockbreak = EditorGUILayout.IntField("Target Block Break", _editingClearCondition._blockbreak);
            _editingClearCondition._score = 0; // 다른 조건 초기화
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Game Over Condition 편집
        EditorGUILayout.LabelField("Game Over Condition", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        _editingGameOverCondition._condtion = (EGAMEOVERCONDITION)EditorGUILayout.EnumPopup("Condition Type", _editingGameOverCondition._condtion);

        if (_editingGameOverCondition._condtion == EGAMEOVERCONDITION.MOVECOUNT)
        {
            _editingGameOverCondition._movecount = EditorGUILayout.IntField("Max Move Count", _editingGameOverCondition._movecount);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.EndScrollView();

        // 버튼들
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

                // 취소 버튼
        if (GUILayout.Button("Cancel & Close", GUILayout.Height(30)))
        {
            HandleCancel();
        }
        
        // 저장 버튼
        bool canSave = !_hasDuplicateFile;
        EditorGUI.BeginDisabledGroup(!canSave);
        GUI.backgroundColor = canSave ? Color.green : Color.gray;
        if (GUILayout.Button("Save Changes (Keep Open)", GUILayout.Height(30)))
        {
            ApplyChanges();
        }
        GUI.backgroundColor = Color.white;
        EditorGUI.EndDisabledGroup();
        
        // 저장 불가 상태 안내
        if (_hasDuplicateFile)
        {
            EditorGUILayout.HelpBox("중복 파일 문제를 해결한 후 저장할 수 있습니다.", MessageType.Warning);
        }
        else if (!_hasUnsavedChanges)
        {
            EditorGUILayout.HelpBox("✅ 모든 변경사항이 저장되었습니다. 계속 편집할 수 있습니다.", MessageType.Info);
        }

        EditorGUILayout.EndHorizontal();

        // 미리보기 정보
        EditorGUILayout.Space();
        ShowPreview();
    }

    private void ShowPreview()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpBox");

        EditorGUILayout.LabelField("Clear Condition:", _editingClearCondition.GetConditionExplain());
        EditorGUILayout.LabelField("Game Over Condition:", _editingGameOverCondition.GetConditionExplain());

        EditorGUILayout.EndVertical();
    }

    private void ApplyChanges()
    {
        // 유효성 검사
        if (!ValidateInput())
        {
            return;
        }

        // 챕터 번호 중복 확인 (자기 자신 제외)
        for (int i = 0; i < _chapterAsset._chapterdata.Count; i++)
        {
            if (i != _chapterIndex && _chapterAsset._chapterdata[i]._chapternumber == _editingChapterNumber)
            {
                EditorUtility.DisplayDialog("Error", $"챕터 번호 {_editingChapterNumber}는 이미 존재합니다.", "OK");
                return;
            }
        }

        // 변경사항 적용
        _editingChapter._chapternumber = _editingChapterNumber;
        _editingChapter._chapterdata = _editingChapterData;
        _editingChapter._clear_condition = _editingClearCondition;
        _editingChapter._over_condition = _editingGameOverCondition;

        // 리스트에 반영
        _chapterAsset._chapterdata[_chapterIndex] = _editingChapter;

        // 에셋 저장
        EditorUtility.SetDirty(_chapterAsset);
        AssetDatabase.SaveAssets();

                // 원본 데이터 업데이트 (저장 완료)
        _originalChapterNumber = _editingChapterNumber;
        _originalChapterData = _editingChapterData;
        _originalClearCondition = _editingClearCondition;
        _originalGameOverCondition = _editingGameOverCondition;
        _hasUnsavedChanges = false;
        
        Debug.Log($"챕터 {_editingChapterNumber} 편집이 완료되었습니다.");
        EditorUtility.DisplayDialog("Success", "챕터가 성공적으로 수정되었습니다!\n\n창은 계속 열려있어 추가 편집이 가능합니다.", "OK");
        
        // 창은 닫지 않고 계속 열어둠
        // Close(); // 제거됨
    }

    private bool ValidateInput()
    {
        // 챕터 번호 유효성 검사
        if (_editingChapterNumber <= 0)
        {
            EditorUtility.DisplayDialog("Error", "챕터 번호는 1 이상이어야 합니다.", "OK");
            return false;
        }

        // Clear Condition 유효성 검사
        if (_editingClearCondition._condtion == EGAMECLEARCONDITION.SCORE && _editingClearCondition._score <= 0)
        {
            EditorUtility.DisplayDialog("Error", "목표 점수는 0보다 커야 합니다.", "OK");
            return false;
        }

        if (_editingClearCondition._condtion == EGAMECLEARCONDITION.BLOCKBRAKE && _editingClearCondition._blockbreak <= 0)
        {
            EditorUtility.DisplayDialog("Error", "목표 블록 파괴 수는 0보다 커야 합니다.", "OK");
            return false;
        }

        // Game Over Condition 유효성 검사
        if (_editingGameOverCondition._condtion == EGAMEOVERCONDITION.MOVECOUNT && _editingGameOverCondition._movecount <= 0)
        {
            EditorUtility.DisplayDialog("Error", "최대 이동 횟수는 0보다 커야 합니다.", "OK");
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// 새로운 Chapter Data File을 생성합니다.
    /// </summary>
    private void CreateNewChapterDataFile()
    {
        // Resources 폴더 확인
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.Refresh();
        }
        
        // 기본 파일명 생성
        string defaultFileName = $"Stage{_editingChapterNumber}";
        string fileName = EditorUtility.SaveFilePanel(
            "Create Chapter Data File", 
            resourcesPath, 
            defaultFileName, 
            "txt"
        );
        
        if (string.IsNullOrEmpty(fileName))
        {
            return; // 사용자가 취소한 경우
        }
        
        // 기본 맵 데이터 생성 (10x10 격자, 모든 슬롯 활성화)
        string defaultMapData = CreateDefaultMapData();
        
        try
        {
            // 파일 생성
            System.IO.File.WriteAllText(fileName, defaultMapData);
            AssetDatabase.Refresh();
            
            // 생성된 파일을 TextAsset으로 로드
            string assetPath = fileName.Replace(Application.dataPath, "Assets");
            TextAsset createdAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            
            if (createdAsset != null)
            {
                _editingChapterData = createdAsset;
                
                // Project 창에서 파일 선택
                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);
                
                // 중복 파일 검사
                CheckForDuplicateFile();
                
                Debug.Log($"새로운 챕터 데이터 파일이 생성되었습니다: {assetPath}");
                EditorUtility.DisplayDialog("Success", $"챕터 데이터 파일이 성공적으로 생성되었습니다!\n\n파일: {createdAsset.name}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "파일 생성은 성공했지만 로드에 실패했습니다.", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"파일 생성 중 오류가 발생했습니다:\n{e.Message}", "OK");
        }
    }
    
    /// <summary>
    /// 기본 맵 데이터를 생성합니다 (10x10 격자).
    /// </summary>
    private string CreateDefaultMapData()
    {
        var lines = new System.Collections.Generic.List<string>();
        
        // 10x10 격자, 모든 슬롯 활성화 (1)
        for (int y = 0; y < 10; y++)
        {
            var row = new System.Collections.Generic.List<string>();
            for (int x = 0; x < 10; x++)
            {
                row.Add("1");
            }
            lines.Add(string.Join(" ", row));
        }
        
        return string.Join("\n", lines);
    }
    
    /// <summary>
    /// Chapter Data File을 Map Editor에서 엽니다.
    /// </summary>
    private void ViewChapterDataFile()
    {
        if (_editingChapterData == null) return;
        
        // Map Editor에서 파일을 직접 열기
        MapEditorWindow.ShowWindowWithFile(_editingChapterData);
    }
    
    /// <summary>
    /// 빠른 맵 생성 (지정된 크기)
    /// </summary>
    private void CreateQuickMap(int width, int height)
    {
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.Refresh();
        }
        
        string fileName = $"{resourcesPath}/Stage{_editingChapterNumber}.txt";
        string mapData = CreateCustomMapData(width, height);
        
        try
        {
            System.IO.File.WriteAllText(fileName, mapData);
            AssetDatabase.Refresh();
            
            TextAsset createdAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(fileName);
            if (createdAsset != null)
            {
                _editingChapterData = createdAsset;
                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);
                
                // 중복 파일 검사
                CheckForDuplicateFile();
                
                Debug.Log($"빠른 맵 생성 완료: {width}x{height} - {fileName}");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"빠른 맵 생성 중 오류 발생:\n{e.Message}", "OK");
        }
    }
    
    /// <summary>
    /// 커스텀 크기 다이얼로그 표시
    /// </summary>
    private void ShowCustomSizeDialog()
    {
        // 간단한 입력 다이얼로그 (Unity 기본 기능 사용)
        string input = EditorUtility.DisplayDialog("Custom Map Size", 
            "원하는 맵 크기를 선택하세요:", 
            "10x10", "8x8") ? "10x10" : "8x8";
            
        if (input == "10x10")
        {
            CreateQuickMap(10, 10);
        }
        else
        {
            CreateQuickMap(8, 8);
        }
    }
    
    /// <summary>
    /// 커스텀 크기의 맵 데이터를 생성합니다.
    /// </summary>
    private string CreateCustomMapData(int width, int height)
    {
        var lines = new System.Collections.Generic.List<string>();
        
        for (int y = 0; y < height; y++)
        {
            var row = new System.Collections.Generic.List<string>();
            for (int x = 0; x < width; x++)
            {
                row.Add("1");
            }
            lines.Add(string.Join(" ", row));
        }
        
        return string.Join("\n", lines);
    }
    
    /// <summary>
    /// 중복 Chapter Data File을 검사합니다.
    /// </summary>
    private void CheckForDuplicateFile()
    {
        _hasDuplicateFile = false;
        _duplicateChapterNumbers.Clear();
        
        if (_editingChapterData == null || _chapterAsset == null)
            return;
            
        // 현재 편집 중인 챕터를 제외하고 다른 챕터들과 비교
        for (int i = 0; i < _chapterAsset._chapterdata.Count; i++)
        {
            if (i == _chapterIndex) continue; // 자기 자신 제외
            
            var otherChapter = _chapterAsset._chapterdata[i];
            if (otherChapter._chapterdata == _editingChapterData)
            {
                _hasDuplicateFile = true;
                _duplicateChapterNumbers.Add(otherChapter._chapternumber);
            }
        }
        
        // 중복 파일이 발견되면 로그 출력
        if (_hasDuplicateFile)
        {
            string duplicateList = string.Join(", ", _duplicateChapterNumbers.ConvertAll(x => $"Chapter {x}"));
            Debug.LogWarning($"중복 파일 감지: '{_editingChapterData.name}'이 {duplicateList}에서도 사용 중입니다.");
        }
    }
    
    /// <summary>
    /// Cancel 버튼 처리 - 변경사항 확인 후 중복 파일 정보 표시 및 Chapter Data File 비우기
    /// </summary>
    private void HandleCancel()
    {
        // 저장되지 않은 변경사항이 있는지 확인
        if (_hasUnsavedChanges)
        {
            bool discardChanges = EditorUtility.DisplayDialog(
                "저장되지 않은 변경사항", 
                "저장되지 않은 변경사항이 있습니다.\n\n변경사항을 저장하지 않고 취소하시겠습니까?", 
                "변경사항 무시하고 닫기", 
                "계속 편집하기"
            );
            
            if (!discardChanges)
            {
                return; // 계속 편집
            }
        }
        
        if (_hasDuplicateFile && _editingChapterData != null)
        {
            // 중복 파일 정보 다이얼로그 표시
            string duplicateList = string.Join(", ", _duplicateChapterNumbers.ConvertAll(x => $"Chapter {x}"));
            string message = $"현재 선택된 파일 '{_editingChapterData.name}'은 다음 챕터에서도 사용 중입니다:\n\n{duplicateList}\n\n" +
                           "Chapter Data File을 비우고 창을 닫습니다.";
            
            EditorUtility.DisplayDialog("중복 파일 감지", message, "확인");
            
            // Chapter Data File 비우기
            _editingChapterData = null;
        }
        else if (_editingChapterData != null)
        {
            // 일반적인 취소 - 파일 정보만 표시
            EditorUtility.DisplayDialog("편집 취소", 
                $"'{_editingChapterData.name}' 파일의 편집을 취소하고 Chapter Data File을 비웁니다.", 
                "확인");
            
            // Chapter Data File 비우기
            _editingChapterData = null;
        }
        
        Close();
    }
    
    private void OnDestroy()
    {
        // 창이 강제로 닫힐 때도 변경사항 확인
        if (_hasUnsavedChanges)
        {
            Debug.LogWarning("Edit Chapter 창이 저장되지 않은 변경사항과 함께 닫혔습니다.");
        }
    }
    
    /// <summary>
    /// 현재 편집 중인 데이터와 원본 데이터를 비교하여 변경사항을 감지합니다.
    /// </summary>
    private void CheckForChanges()
    {
        bool hasChanges = false;
        
        // 챕터 번호 변경 확인
        if (_editingChapterNumber != _originalChapterNumber)
        {
            hasChanges = true;
        }
        
        // 챕터 데이터 파일 변경 확인
        if (_editingChapterData != _originalChapterData)
        {
            hasChanges = true;
        }
        
        // Clear Condition 변경 확인
        if (!CompareClearConditions(_editingClearCondition, _originalClearCondition))
        {
            hasChanges = true;
        }
        
        // Game Over Condition 변경 확인
        if (!CompareGameOverConditions(_editingGameOverCondition, _originalGameOverCondition))
        {
            hasChanges = true;
        }
        
        _hasUnsavedChanges = hasChanges;
    }
    
    /// <summary>
    /// 두 Clear Condition을 비교합니다.
    /// </summary>
    private bool CompareClearConditions(St_GameClearCondition a, St_GameClearCondition b)
    {
        return a._condtion == b._condtion &&
               a._score == b._score &&
               a._blockbreak == b._blockbreak;
    }
    
    /// <summary>
    /// 두 Game Over Condition을 비교합니다.
    /// </summary>
    private bool CompareGameOverConditions(St_GameOverCondtion a, St_GameOverCondtion b)
    {
        return a._condtion == b._condtion &&
               a._movecount == b._movecount;
    }
}
