using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityToolbarExtender;

// Unity toolbar extender : https://github.com/marijnz/unity-toolbar-extender
// Unity editor icons Link : https://github.com/halak/unity-editor-icons

namespace ScenesBrowser
{
    public class ScenesBrowser : EditorWindow
    {

        protected static readonly GUIContent _WindowName = EditorGUIUtility.TrTextContent("Scenes Browser - Settings");
        protected static EditorWindow _EditorWindow;
        protected static Vector2 _WindowSettingsMaxSize = new Vector2(512, 350);
        // w/h
        protected static float _Width = 128, _Heigth = 20;
        // For filtering scenes
        protected const string _FilterBy = "*.unity";
        private const string _DataSettingsSavePath = "Assets/Editor/Scenes Browser";
        protected static string[] _AllScenesPathInProject;
        // To save stuff
        protected static SBD _DataSettings;

        #region Toolbar 
        protected static int _SelectedSceneIndex = 0;
        protected string _ShowToolbarAt = "Show Toolbar At: ";
        // For scroll content 
        protected static Vector2 _ScrollPositionOnToolbar;
        // Width & Heigth Settings and refresh BG
        protected static int _WidthSettingsAndRefreshBG = 26, _HeigthSettingsAndRefreshBG = 20;
        protected static Action<string> onOpenNewScene;
        protected static bool _IsWindwoOpen = false;
        protected static void ResetIsSaveWindwoOpen() => _IsWindwoOpen = false;
        #endregion

        #region  Settings window
        // Scroll position on settings window
        protected static Vector2 _ScrollPositionOnSettingsWindow;
        protected float _ButtonSize = 122f;
        private static Texture2D _ActiveSceneTexture;
        protected static List<GUIContent> _SceneNameAndIcon = new List<GUIContent>();
        protected static GUIStyle _ActiveSceneStyle, _SettingWindowSceneStyle;

        #endregion

        [MenuItem("Scenes Browser/Settings %E")]
        public static void ShowScenesBrowserSettings()
        {
            _EditorWindow = GetWindowSettings;
            _EditorWindow.titleContent = _WindowName;
            _EditorWindow.minSize = _WindowSettingsMaxSize;
            _EditorWindow.maxSize = _WindowSettingsMaxSize;
            _EditorWindow.maximized = false;
        }
        // Get window settings
        protected static EditorWindow GetWindowSettings => EditorWindow.GetWindow(typeof(ScenesBrowser));

        [InitializeOnLoadMethod]
        private static void LoadScenesBrowser()
        {

            SceneStyles.LoadTextures();

            // We don't have settings-data ? 
            if (!_DataSettings)
                SetupSettingsData();
            // We have the data
            if (_DataSettings)
                ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);

            // 
            UpdateSceneInDictionary();

            onOpenNewScene += OpenNewScene;

            // select last saved value 
            _SelectedSceneIndex = _DataSettings.m_PreviousScenesToolbarGridSize;
            //
            OnSceneChange();
            // ShowActiveScene();

        }
        private void OnGUI()
        {
            // No data?
            if (_DataSettings)
            {
                // Auto-Select scene
                EditorGUILayout.BeginHorizontal();
                using (var _SelectPath = new EditorGUILayout.HorizontalScope())
                {
                    var _AutoContent = ScenesBrowserExtender.GetGUIContent("Auto", "Auto find scenes");

                    _DataSettings.m_AutoFindScene = EditorGUILayout.Toggle(_DataSettings.m_AutoFindScene, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField(_AutoContent, GUILayout.MaxWidth(50));
                    if (!_DataSettings.m_AutoFindScene)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Select Path : ", GUILayout.Width(75), GUILayout.Height(_Heigth));
                        _DataSettings.m_ScenePath = EditorGUILayout.TextField(_DataSettings.m_ScenePath, GUILayout.Height(_Heigth));
                        // Open folder to select a scenes path
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("FolderOpened On Icon")), GUILayout.Width(25), GUILayout.Height(_Heigth)))
                            ScenesBrowserExtender.SelectFolder(Application.dataPath, _DataSettings);

                        EditorGUILayout.EndHorizontal();
                    }
                }

                var _ResetContent = ScenesBrowserExtender.GetGUIContent("", "Reset path", new GUIContent(EditorGUIUtility.IconContent("d_Preset.Context")).image);

                if (GUILayout.Button(_ResetContent, GUILayout.Width(25), GUILayout.Height(_Heigth)))
                    NewReset();

                EditorGUILayout.EndHorizontal();
                // End of top

                // EditorGUILayout.Space(3);
                // Show Scene At Left Or Right
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    // Left Or Right
                    _DataSettings.m_IsLeft = EditorGUILayout.Toggle(_DataSettings.m_IsLeft, GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(_DataSettings.m_IsLeft ? _ShowToolbarAt + " Left " : _ShowToolbarAt + " Right ", GUILayout.MaxWidth(130));
                    // Draw vertical line
                    EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.MaxWidth(10));
                    // Quick Access
                    _DataSettings.m_ShowQuickAccess = EditorGUILayout.Toggle(_DataSettings.m_ShowQuickAccess, GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(_DataSettings.m_ShowQuickAccess ? " Hide Quick Access " : " Show Quick Access ", GUILayout.MaxWidth(128));
                }
                // Draw scenes on window setting
                DrawScenesOnWindowSetting();
            }
            else
            {
                // No Data found ? Create new data
                if (GUILayout.Button(new GUIContent(" Create", EditorGUIUtility.IconContent("CreateAddNew").image), GUILayout.Height(_Heigth + 10)))
                    SetupSettingsData();

            }
        }
        /// <summary>
        /// Draw scenes on window setting
        /// </summary>
        private void DrawScenesOnWindowSetting()
        {
            // using (var _ShowSceneOnWindow = new EditorGUILayout.HorizontalScope(GUI.skin.box))
            GUI.BeginGroup(new Rect(2.5f, 55, Screen.width, Screen.height));
            // Set scroll view : position
            var _ScrollViewPosition = new Rect(0, 0, Screen.width - 5, Screen.height - 120);
            // Set scroll view : contetn view
            var _ScrollView = new Rect(0, 0, Screen.width - 25, (_ButtonSize + 40) * _DataSettings.SceneList.Count / 5);
            // Begin scroll view
            _ScrollPositionOnSettingsWindow = GUI.BeginScrollView(_ScrollViewPosition, _ScrollPositionOnSettingsWindow, _ScrollView, false, false);
            // Setting window scene style  
            _SettingWindowSceneStyle = null ?? new GUIStyle("Button");
            _SettingWindowSceneStyle.alignment = TextAnchor.LowerCenter;
            _SettingWindowSceneStyle.imagePosition = ImagePosition.ImageAbove;
            _SettingWindowSceneStyle.padding = new RectOffset(10, 10, 10, 10);

            var yPos = 0f;
            var _ChoiceWidth = 36f;
            var _Count = 0;

            // foreach (var scene in _SceneDictionary)
            foreach (var scene in _DataSettings.SceneList)
            {
                // Draw scene 4/4
                if (scene.Scene != null)
                {
                    if (_Count >= 4)
                    {
                        yPos += _ButtonSize + 5;
                        _Count = 0;
                    }

                    var xPos = (_ButtonSize + 2) * _Count;
                    var _ButtonRect = new Rect(xPos, yPos, _ButtonSize, _ButtonSize);

                    GUILayout.BeginArea(_ButtonRect, GUI.skin.box);
                    // Get scene name
                    var _SceneName = scene.Scene.name;
                    // Draw button for the scene > What we want to do with it ?
                    if (GUILayout.Button(new GUIContent(_SceneName, EditorGUIUtility.IconContent("SceneAsset On Icon").image), _SettingWindowSceneStyle, GUILayout.Height(_ButtonSize - 26)))
                        Debug.Log(_SceneName);

                    // Draw more choice under scene..
                    using (new EditorGUILayout.HorizontalScope())
                    {

                        GUI.enabled = !scene.Active;
                        // Dsiable if this scene open
                        if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent(scene.Hide ? "animationvisibilitytoggleon@2x" : "animationvisibilitytoggleoff@2x").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                        {
                            scene.Hide = !scene.Hide;
                            // Invoke("ShowScenesOnToolbar", .1f);
                            // OnToolbarGUI();
                            // ShowActiveScene(EditorSceneManager.GetActiveScene().name);
                        }
                        GUI.enabled = true;

                        if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_CustomTool@2x").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                            Debug.Log("1");
                        if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("TreeEditor.Trash").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                            Debug.Log("1");
                    }
                    //
                    GUILayout.EndArea();
                    _Count++;
                }
            }
            // End scroll view
            GUI.EndScrollView();
            // Save button
            if (GUI.Button(new Rect(0, Screen.height - 110, Screen.width - 25, 26), new GUIContent("  Save", EditorGUIUtility.IconContent("SaveActive").image)))
            {
                ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);
                EditorUtility.SetDirty(_DataSettings);
                AssetDatabase.SaveAssets();
            }
            GUI.EndGroup();
        }
        /// <summary>
        /// On toolbar gui
        /// </summary>
        private static void OnToolbarGUI()
        {
            // Is show quick access true ?
            if (_DataSettings.m_ShowQuickAccess)
                ShowScenesOnToolbar();
        }
        /// <summary>
        /// Show scenes on toolbar
        /// </summary>
        public static void ShowScenesOnToolbar()
        {
            // Debug.Log("Show scenes on toolbar");

            // Settings and refresh button
            SettingsAndRefreshButton();
            // Scroll - all scenes 
            _ScrollPositionOnToolbar = EditorGUILayout.BeginScrollView(_ScrollPositionOnToolbar, false, false, GUILayout.MinHeight(50));
            using (var scenes = new EditorGUILayout.HorizontalScope())
            {
                // If mouse over rect/content
                if (scenes.rect.Contains(Event.current.mousePosition))
                {
                    // Scroll by scroll wheel
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        // Scroll x value > Horizontal
                        _ScrollPositionOnToolbar.x += Event.current.delta.y * 10f;
                        // Apply
                        Event.current.Use();
                    }
                }
                DrawScenesOnToolbar();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw scenes on toolbar
        /// </summary>
        private static void DrawScenesOnToolbar()
        {
            var _SceneAndIconArray = GetSceneNameAndIcon();
            // Scene on Tool bar
            _SelectedSceneIndex = GUILayout.Toolbar(_SelectedSceneIndex, _SceneAndIconArray, GUILayout.MaxWidth(_Width * _SceneAndIconArray.Length), GUILayout.MaxHeight(_Heigth));

            ShowActiveScene(EditorSceneManager.GetActiveScene().name);

            // Is not thie same scene ? load the new scene
            // if (_SelectedSceneIndex != _DataSettings.m_PreviousScenesToolbarGridSize && !_IsWindwoOpen)
            if (GUI.changed)
            {
                // So this dumb.. but to make this SaveCurrentModifiedScenesIfUserWantsTo() not show tows >> Fix me or let me alive
                _IsWindwoOpen = true;

                // If there unsave change > ask if i want to save , If user click yes
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Open scene
                    onOpenNewScene?.Invoke(_SceneAndIconArray[_SelectedSceneIndex].text);
                    ResetIsSaveWindwoOpen();
                }
                else
                {
                    _SelectedSceneIndex = _DataSettings.m_PreviousScenesToolbarGridSize;
                    ResetIsSaveWindwoOpen();
                }
                // To avoid : "EndLayoutGroup: BeginLayoutGroup must be called first."
                GUIUtility.ExitGUI();
            }
        }
        // Show active scene
        private static void ShowActiveScene(string sceneName)
        {
            // Style
            _ActiveSceneStyle = new GUIStyle(/* GUI.skin.horizontalScrollbar */);
            // Create a new texture  
            _ActiveSceneTexture = null ?? ScenesBrowserExtender.CreateNewTexture2D(1, 1, ScenesBrowserExtender.CreateNewColor("D9D9D9"));
            // Set texture  
            _ActiveSceneStyle.normal.background = _ActiveSceneTexture;
            // Set height
            _ActiveSceneStyle.fixedHeight = 4;

            // Get active scene index
            var _ActiveSceneIndex = _DataSettings.GetActiveScene(sceneName);
            // Get start at value
            var _StartAt = _ActiveSceneIndex == 0 ? 6f : 4f;
            // X position
            var _xPos = _StartAt + _Width * _ActiveSceneIndex;
            // Y position
            var _yPos = _Heigth;

            // Check we have scene + and the scene not hiding 
            if (_DataSettings.SceneList.Count != 0 && !_DataSettings.SceneList.All(a => a.Hide))
                // Show line under a active scene 
                GUI.Label(new Rect(_xPos, _yPos, _Width - _StartAt, _Heigth), "", _ActiveSceneStyle);
        }

        /// <summary>
        /// Open new scene by index
        /// </summary>
        protected static void OpenNewScene(string sceneName)
        {
            if (EditorSceneManager.GetActiveScene().name == sceneName) return;
            Debug.Log("OpenNewScene " + sceneName);
            var _Index = _DataSettings.SceneList.IndexOf(_DataSettings.SceneList.Find(c => c.Scene.name == sceneName));
            // Save prev scene index
            _DataSettings.m_PreviousScenesToolbarGridSize = _Index;
            // Open scene
            EditorSceneManager.OpenScene(_DataSettings.SceneList.Find(c => c.Scene.name == sceneName).ScenePath);
        }
        /// <summary>
        /// Settings and refresh buttons
        /// </summary>
        private static void SettingsAndRefreshButton()
        {
            // Settings && Refresh
            using (var scenes = new EditorGUILayout.HorizontalScope(GUILayout.Width(_WidthSettingsAndRefreshBG)))
            {
                // Open settings
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("EditorSettings Icon").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                    ShowScenesBrowserSettings();
                // Refresh
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Refresh@2x").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                    UpdateSceneInDictionary();
            }
        }
        /// <summary>
        /// Update scene in dictionary
        /// </summary>
        private static void UpdateSceneInDictionary()
        {
            // On scene change
            OnSceneChange();

            Debug.Log("Update Scene In Dictionary");

            // If Auto find scenes active > Find all scene in this ptoject
            if (_DataSettings.m_AutoFindScene)
                _AllScenesPathInProject = Directory.GetFiles(Application.dataPath, _FilterBy, SearchOption.AllDirectories);
            else
            {
                // We have a path ? Look for scenes ..
                if (_DataSettings.m_ScenePath != string.Empty)
                    _AllScenesPathInProject = Directory.GetFiles(_DataSettings.m_ScenePath, _FilterBy);
                else
                    Debug.LogError($"Path is null {_DataSettings.m_ScenePath}");
            }
            // Start adding to dictionary
            foreach (var sc in _AllScenesPathInProject)
            {
                // Get path
                var _Path = sc.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                // Get scenes name
                // var _Name = ScenesBrowserExtender.Between(_Path, "Scenes/", ".unity");
                var _Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(_Path);
                // We don't have this ??
                if (!_DataSettings.IsContainScene(_Scene))
                {
                    // Create a new 
                    var _NewScene = new SBScene(_Path, AssetDatabase.LoadAssetAtPath<SceneAsset>(_Path));
                    // Add it
                    _DataSettings.AddScene(_NewScene);
                }
            }
        }
        /// <summary>
        /// Check for any change in scene
        /// </summary>
        private static void OnSceneChange()
        {
            for (int i = _DataSettings.SceneList.Count - 1; i >= 0; i--)
            {
                if (_DataSettings.SceneList[i].Scene == null)
                    _DataSettings.SceneList.RemoveAt(i);
            }

        }

        /// <summary>
        /// Setup settings data
        /// </summary>
        private static void SetupSettingsData()
        {
            // Data folder
            var _DataPath = _DataSettingsSavePath + "/Data";
            // If we don't have data folder ... create new one
            if (!Directory.Exists(_DataPath))
            {
                // Create directory/Folder
                Directory.CreateDirectory(_DataPath);
                // Refresh assets
                AssetDatabase.Refresh();
            }
            // Get data paths  
            var _FindAllSettingsData = Directory.GetFiles(_DataPath, "*.asset");
            // Get (data.asset)
            _DataSettings = (_FindAllSettingsData.Length == 0) ? ScenesBrowserExtender.CreateNewData(_DataPath) : (SBD)EditorGUIUtility.Load(_FindAllSettingsData[0]);
        }
        private void NewReset()
        {
            Debug.Log("Fun Reset $Remove me");
            _DataSettings.m_ScenePath = "";
        }
        public static GUIContent[] GetSceneNameAndIcon()
        {
            if (!_DataSettings) return null;

            _SceneNameAndIcon.Clear();

            // If user delete a scene manually , this scene we be in the list until he click refresh
            foreach (var scene in _DataSettings.SceneList)
            {
                if (scene.Scene)
                {
                    // Scene hide not true  
                    if (!scene.Hide)
                        _SceneNameAndIcon.Add(new GUIContent(scene.Scene.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image));
                }
            }
            // Return an array
            return _SceneNameAndIcon.ToArray();
        }


    }
}
