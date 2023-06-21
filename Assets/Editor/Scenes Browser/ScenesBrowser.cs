using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenesBrowser.Data;
using ScenesBrowser.Utils;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityToolbarExtender;

// Unity toolbar extender : https://github.com/marijnz/unity-toolbar-extender
// Unity editor icons Link : https://github.com/halak/unity-editor-icons

namespace ScenesBrowser
{
    public class ScenesBrowser : EditorWindow
    {
        protected static Texture _ScenesBrowserIcon;
        protected static GUIContent _WindowName;

        protected static EditorWindow _EditorWindow;
        protected static readonly Vector2 _WindowSettingsMaxSize = new Vector2(512, 350);
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
        #endregion

        #region  Settings window
        // Scroll position on settings window
        protected static Vector2 _ScrollPositionOnSettingsWindow;
        protected float _ButtonSize = 122f;
        protected static List<GUIContent> _SceneNameAndIcon = new List<GUIContent>();
        protected static GUIStyle _ActiveSceneStyle, _SettingWindowSceneStyle;
        private int _RowCount = 4, _SelectedRowIndex = 0;
        protected string[] _RowCountOptions = new string[] { "4", "8", "12" };
        private string _NewSceneName = "";
        public static bool IsPlayModeOn = false;

        #endregion

        [MenuItem("Scenes Browser/Settings %E")]
        public static void ShowScenesBrowserSettings()
        {
            // Load icon
            _ScenesBrowserIcon = EditorGUIUtility.IconContent("Favorite@2x").image;// AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scenes Browser/Icon/.png");
            _WindowName = EditorGUIUtility.TrTextContent("Scenes Browser - Settings", _ScenesBrowserIcon);

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
            // We don't have settings-data ? 
            if (!_DataSettings)
                SetupSettingsData();
            // We have the data
            if (_DataSettings)
                ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);
            // 
            UpdateSceneInDictionary();
            // 
            onOpenNewScene += OpenNewScene;
            // select last saved value 
            _SelectedSceneIndex = _DataSettings.m_PreviousScenesToolbarGridSize;
            // On scene change
            OnSceneChange();

            EditorApplication.playModeStateChanged += PlayModeON;
        }

        protected static void PlayModeON(PlayModeStateChange state) => IsPlayModeOn = state == PlayModeStateChange.EnteredPlayMode;


        private void OnGUI()
        {
            var _RectTest = new Rect(85, -10, Screen.width, 128);

            // GUI.BeginGroup(_RectTest);
            // GUI.Button(_RectTest, EditorGUIUtility.IconContent("_Help"));



            // GUI.EndGroup();
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

                // End of top
                EditorGUILayout.EndHorizontal();

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
                    EditorGUILayout.LabelField(_DataSettings.m_ShowQuickAccess ? " Hide Quick Access " : " Show Quick Access ", GUILayout.MaxWidth(118));
                    // Draw vertical line
                    EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.MaxWidth(10));
                    EditorGUILayout.LabelField("Row count", GUILayout.MaxWidth(64));
                    _SelectedRowIndex = EditorGUILayout.Popup(_SelectedRowIndex, _RowCountOptions, GUILayout.Width(32));
                    _RowCount = int.Parse(_RowCountOptions[_SelectedRowIndex]);
                    // Draw vertical line
                    EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.MaxWidth(10));
                    // Refresh
                    if (GUILayout.Button("Refresh", GUILayout.Width(64), GUILayout.Height(18)))
                        UpdateSceneInDictionary();
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
            var _ScrollView = new Rect(0, 0, !maximized ? (_RowCount / 4) * (Screen.width - 20) : (Screen.width - 20), (_ButtonSize + 40) * _DataSettings.SceneList.Count / 4);
            // Begin scroll view
            _ScrollPositionOnSettingsWindow = GUI.BeginScrollView(_ScrollViewPosition, _ScrollPositionOnSettingsWindow, _ScrollView, !maximized ? _RowCount > 4 : false, false);
            // Setting window scene style  
            _SettingWindowSceneStyle = null ?? new GUIStyle("Button");
            _SettingWindowSceneStyle.alignment = TextAnchor.LowerCenter;
            _SettingWindowSceneStyle.imagePosition = ImagePosition.ImageAbove;
            _SettingWindowSceneStyle.padding = new RectOffset(10, 10, 10, 10);

            var yPos = 0f;
            var _ChoiceWidth = 36f;
            var _Count = 0;

            // foreach (var scene in _SceneDictionary)
            foreach (var sc in _DataSettings.SceneList.ToList())
            {
                var _Scene = sc;

                // Draw scene 4/4
                if (_Scene.Scene != null)
                {
                    if (_Count >= _RowCount)
                    {
                        yPos += _ButtonSize + 5;
                        _Count = 0;
                    }

                    var xPos = (_ButtonSize + 2) * _Count;
                    var _ButtonRect = new Rect(xPos, yPos, _ButtonSize, _ButtonSize);

                    GUILayout.BeginArea(_ButtonRect, GUI.skin.box);
                    // Get scene name
                    var _SceneName = _Scene.Scene.name;
                    // Draw button for the scene > What we want to do with it ?
                    if (GUILayout.Button(new GUIContent(_SceneName, EditorGUIUtility.IconContent("SceneAsset On Icon").image), _SettingWindowSceneStyle, GUILayout.Height(_ButtonSize - 26)))
                        Debug.Log(_SceneName);

                    // Draw more choice under scene..
                    using (new GUILayout.HorizontalScope())
                    {

                        if (!_Scene.IsRenameSceneActive)
                        {
                            // Dsiable if this scene open
                            GUI.enabled = !_Scene.Active;
                            // Un/Hide scene
                            if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent(_Scene.Hide ? "animationvisibilitytoggleon@2x" : "animationvisibilitytoggleoff@2x").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                                _Scene.Hide = !_Scene.Hide;
                            // Enable gui again
                            GUI.enabled = true;

                            GUI.enabled = !IsPlayModeOn;

                            // Rename a scene
                            if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_CustomTool@2x").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                            {
                                // Set scene name
                                _NewSceneName = _Scene.Scene.name;
                                // Show rename text field
                                _Scene.IsRenameSceneActive = true;
                            }

                            // Delete a scene
                            if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("TreeEditor.Trash").image), GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)))
                            {
                                // Ask for confirmation
                                if (EditorUtility.DisplayDialog("Delete Confirmation", "Are you sure you want to delete this scene?", "Delete", "Cancel"))
                                {
                                    // Delete meta file
                                    File.Delete(_Scene.ScenePath + ".meta");
                                    // Delete scene file
                                    File.Delete(_Scene.ScenePath);
                                    // Remove scene from the list
                                    _DataSettings.SceneList.Remove(_Scene);
                                    // Refresh unity
                                    AssetDatabase.Refresh();
                                }
                            }
                            GUI.enabled = true;

                        }
                        else
                        {
                            GUI.enabled = !IsPlayModeOn;
                            //Press enter
                            var _PressEnter = !IsPlayModeOn && (_Scene.IsRenameSceneActive && Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return);
                            // Press escape
                            var _PressEscape = _Scene.IsRenameSceneActive && Event.current.keyCode == KeyCode.Escape;

                            // Rename text field
                            using (new GUILayout.HorizontalScope())
                            {
                                // Get the new scene name
                                _NewSceneName = GUILayout.TextField(_NewSceneName);

                                // Cancel
                                if (_PressEscape)
                                    _Scene.DisableRename();

                                // Ok
                                if (GUILayout.Button("Ok", GUILayout.MaxWidth(_ChoiceWidth), GUILayout.MaxHeight((_ChoiceWidth + 2) / 2)) || _PressEnter)
                                {
                                    if (_NewSceneName != string.Empty)
                                        // Apply new name
                                        _Scene.SetNewSceneName(_NewSceneName);
                                    else
                                        _Scene.DisableRename();

                                    // Refresh unity
                                    AssetDatabase.Refresh();
                                }
                            }
                            GUI.enabled = true;

                        }
                    }
                    //
                    GUILayout.EndArea();
                    _Count++;
                }
            }
            // End scroll view
            GUI.EndScrollView();

            GUILayout.Space(Screen.height - 160);
            using (new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button(new GUIContent("  Save", EditorGUIUtility.IconContent("SaveActive").image), GUILayout.Width(Screen.width - 170), GUILayout.Height(25)))
                {
                    ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);
                    EditorUtility.SetDirty(_DataSettings);
                    AssetDatabase.SaveAssets();
                }
                if (GUILayout.Button(new GUIContent(" Reload scenes", EditorGUIUtility.IconContent("RotateTool On").image), GUILayout.Width(128), GUILayout.Height(25)))
                {
                    UpdateSceneInDictionary(true);

                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Linked@2x"), GUILayout.Width(25), GUILayout.Height(25)))
                {
                    // create the menu and add items to it
                    GenericMenu menu = new GenericMenu();
                    var _Ss = new GUIStyle("Box");

                    menu.AddItem(new GUIContent(" Toolbar Extender : GitHub"), false, OpenLink, "https://github.com/marijnz/unity-toolbar-extender");
                    menu.AddItem(new GUIContent(" Scenes Browser : GitHub"), false, OpenLink, "https://github.com/atoGames/Scenes-Browser");
                    menu.AddItem(new GUIContent(" Devloper on : Twitter"), false, OpenLink, "https://twitter.com/_atoGames");
                    menu.ShowAsContext();
                }
            }

            /* 
               var _ButtonReloadScenesSize = 110;
            var _BottomPosition = Screen.height - 110;
             // Save button
             if (GUI.Button(new Rect(0, _BottomPosition, Screen.width - 150, 26), new GUIContent("  Save", EditorGUIUtility.IconContent("SaveActive").image)))
             {
                 ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);
                 EditorUtility.SetDirty(_DataSettings);
                 AssetDatabase.SaveAssets();
             }
             // Clear all 
             if (GUI.Button(new Rect(Screen.width - 150, _BottomPosition, 100, 26), "Reload scenes"))
                 UpdateSceneInDictionary(true);

             if (GUI.Button(new Rect(Screen.width - 50, _BottomPosition, 32, 26), "US"))
             {
                 // create the menu and add items to it
                 GenericMenu menu = new GenericMenu();

                 menu.AddItem(new GUIContent("MenuItem1"), false, Callback, "item 1");
                 menu.ShowAsContext();
             } */


            GUI.EndGroup();
        }

        /// <summary>
        /// On toolbar gui
        /// </summary>
        private static void OnToolbarGUI()
        {
            // Is show quick access true ?
            if (_DataSettings.m_ShowQuickAccess && !IsPlayModeOn)
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
            // Scene on Tool bar > If the user has hidden a scene, this will select the next scene .. but not activated
            _SelectedSceneIndex = GUILayout.Toolbar(_SelectedSceneIndex, _SceneAndIconArray, GUILayout.MaxWidth(_Width * _SceneAndIconArray.Length), GUILayout.MaxHeight(_Heigth));

            // Not the same scene ? load the new scene
            if (GUI.changed)
            {
                // If there unsave change > ask if i want to save , If user click yes
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    // Open scene
                    onOpenNewScene?.Invoke(_SceneAndIconArray[_SelectedSceneIndex].text);
                // To avoid : "EndLayoutGroup: BeginLayoutGroup must be called first."
                GUIUtility.ExitGUI();
            }
        }
        /// <summary>
        /// Open new scene by index
        /// </summary>
        protected static void OpenNewScene(string sceneName)
        {
            //  This scene a already open
            if (EditorSceneManager.GetActiveScene().name == sceneName) return;

            // Get current scene
            var _CurrentScene = _DataSettings.SceneList.Find(c => c.Scene.name == sceneName);

            // We have a scene ?
            if (_CurrentScene != null)
            {
                var _Index = _DataSettings.SceneList.IndexOf(_CurrentScene);
                // Save prev scene index
                _DataSettings.m_PreviousScenesToolbarGridSize = _Index;
                // Set active scene
                _DataSettings.SetActiveScene(_CurrentScene);
                // Open scene
                EditorSceneManager.OpenScene(_CurrentScene.ScenePath);
            }
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
        private static void UpdateSceneInDictionary(bool clearList = false)
        {
            // On scene change
            OnSceneChange(clearList);

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
                // Load scene 
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
        private static void OnSceneChange(bool clearList = false)
        {
            // If this true , Clear
            if (clearList)
            {
                _DataSettings.SceneList.Clear();
                return;
            }
            // Else , Remove the empty one
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

        public void OpenLink(object obj)
        {
            Application.OpenURL(obj.ToString());
        }


    }
}
