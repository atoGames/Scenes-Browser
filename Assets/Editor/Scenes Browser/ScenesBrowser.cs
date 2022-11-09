using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityToolbarExtender;

// Unity toolbar extender : https://github.com/marijnz/unity-toolbar-extender
// Unity editor icons Link : https://github.com/halak/unity-editor-icons

namespace ScenesBrowser
{

    public class ScenesBrowser : EditorWindow
    {
        protected static readonly GUIContent _WindowName = EditorGUIUtility.TrTextContent("Scenes Browser - Settings");
        protected static EditorWindow _EditorWindow;
        protected static Vector2 _WindowSettingsMaxSize = new Vector2(512, 256);
        // protected static List<SceneAsset> m_SceneAssets = new List<SceneAsset>();
        //
        protected static float _Width = 128, _Heigth = 20;
        // For filtering scenes
        protected const string _FilterBy = "*.unity";
        private const string _DataSavePath = "Assets/Editor/Scenes Browser";
        protected static string[] _GetAllScenesInProject;


        // To save stuff
        protected static SBD _DataSettings;

        #region Top bar 
        protected static int _ScenesToolbarGridSize = 0;
        protected string _ShowToolbarAt = "Show Toolbar At: ";
        // For scroll content 
        protected static Vector2 _ScrollPositionOnToolbar;
        // Width & Heigth Settings and refresh BG
        protected static int _WidthSettingsAndRefreshBG = 26, _HeigthSettingsAndRefreshBG = 20;
        protected static Action<int> onOpenNewScene;
        protected static bool _IsSaveWindwoOpen = false;
        protected static void ResetIsSaveWindwoOpen() => _IsSaveWindwoOpen = false;


        #endregion
        protected static Vector2 _ScrollPositionOnSettingsWindow;

        // Use this to save / remove / order
        // Key is the path of the scene , value is the scene it's self
        protected static Dictionary<string, SceneAsset> _SceneDictionary = new Dictionary<string, SceneAsset>();
        // Grid thing
        protected int _ScenesWindowGridSize = 0;
        protected int _GridCulm = 5;

        [MenuItem("Scenes Browser/Settings %E")]
        public static void ShowScenesBrowserSettings()
        {
            _EditorWindow = GetWindowSettings;
            _EditorWindow.titleContent = _WindowName;
            _EditorWindow.minSize = _WindowSettingsMaxSize;
            _EditorWindow.maxSize = _WindowSettingsMaxSize;
            _EditorWindow.maximized = false;
        }
        // Window settings
        protected static EditorWindow GetWindowSettings => EditorWindow.GetWindow(typeof(ScenesBrowser));

        [InitializeOnLoadMethod]
        private static void LoadScenesBrowser()
        {

            SceneStyles.LoadTextures();

            // We don't have settings-data ? 
            if (!_DataSettings)
                GetSettingsData();
            // We have the data
            if (_DataSettings)
                ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);

            // 
            AddSceneToDictionary();

            onOpenNewScene += OpenNewScene;

            // select last saved value 
            _ScenesToolbarGridSize = _DataSettings.m_PreviousScenesToolbarGridSize;
            //


        }
        private void OnEnable()
        {
            // _SettingsAndRefreshBG ??= CreateNewTexture2D(16, 16, CreateNewColor());
        }
        private void OnGUI()
        {
            // I test this line
            // _SettingsData = EditorGUILayout.ObjectField("Select Database Dictionary ", _SettingsData, typeof(SBD), true) as SBD;


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
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("FolderOpened On Icon")), GUILayout.Width(25), GUILayout.Height(_Heigth)))
                        {
                            ScenesBrowserExtender.SelectFolder(Application.dataPath, _DataSettings);
                            Debug.Log("Open scenes folder to select path");
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                }

                var _ResetContent = ScenesBrowserExtender.GetGUIContent("", "Reset path", new GUIContent(EditorGUIUtility.IconContent("d_Preset.Context")).image);

                if (GUILayout.Button(_ResetContent, GUILayout.Width(25), GUILayout.Height(_Heigth)))
                {
                    NewReset();
                }
                EditorGUILayout.EndHorizontal();
                // End of top


                // EditorGUILayout.Space(3);
                // Show Scene At Left Or Right
                using (var _ShowSceneAtLeftOrRight = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    // Left Or Right
                    _DataSettings.m_IsLeft = EditorGUILayout.Toggle(_DataSettings.m_IsLeft, GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(_DataSettings.m_IsLeft ? _ShowToolbarAt + " Left " : _ShowToolbarAt + " Right ", GUILayout.MaxWidth(150));

                    // Quick Access
                    _DataSettings.m_ShowQuickAccess = EditorGUILayout.Toggle(_DataSettings.m_ShowQuickAccess, GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(_DataSettings.m_ShowQuickAccess ? " Hide Quick Access " : " Show Quick Access ", GUILayout.MaxWidth(128));
                }

                ShowSceneOnWindowSettings();


                // Save button
                if (GUILayout.Button(new GUIContent(" Save", EditorGUIUtility.IconContent("SaveActive").image), GUILayout.Height(_Heigth + 10)))
                {
                    ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);
                    EditorUtility.SetDirty(_DataSettings);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                // No Data found ? Create new data
                if (GUILayout.Button(new GUIContent(" Create", EditorGUIUtility.IconContent("CreateAddNew").image), GUILayout.Height(_Heigth + 10)))
                    GetSettingsData();

            }
        }

        private void ShowSceneOnWindowSettings()
        {

            // Grid - /* EditorStyles.helpBox */
            using (var _ShowSceneOnWindow = new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUILayout.Button(_GridCulm.ToString(), GUILayout.Width(22), GUILayout.Height(_Heigth)))
                {
                    _GridCulm += 2;
                    // _GridCulm = Mathf.Clamp(_GridCulm, 1, 10);
                    if (_GridCulm > 6)
                        _GridCulm = 2;

                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_ScrollPositionOnSettingsWindow))
                {
                    _ScrollPositionOnSettingsWindow = scrollView.scrollPosition;
                    // ShowScenes(64, 64);
                    // TODO: I don't like this but is work!
                    var _Names = new List<string>();
                    foreach (var scene in _SceneDictionary)
                    {
                        if (ScenesBrowserExtender.IsNotSceneNull(scene) && !_Names.Contains(scene.Value.name))
                            _Names.Add(scene.Value.name);
                    }

                    // This is a old ? whoe care
                    _ScenesWindowGridSize = GUILayout.SelectionGrid(_ScenesWindowGridSize, _Names.ToArray(), _GridCulm/* , GUILayout.MaxWidth(512), GUILayout.MaxHeight(512) */);
                }

            }
        }

        private static void OnToolbarGUI()
        {
            // Is show quick access true ?
            if (_DataSettings.m_ShowQuickAccess)
                GetScenes();
        }

        public static void GetScenes()
        {
            // Settings and refresh button
            SettingsAndRefreshButton();
            // Scroll - all scenes 
            _ScrollPositionOnToolbar = EditorGUILayout.BeginScrollView(_ScrollPositionOnToolbar, false, true, GUILayout.MinHeight(50));
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
                ShowScenesOnTopBar();
            }
            EditorGUILayout.EndScrollView();
        }

        private static void ShowScenesOnTopBar()
        {
            /*  var _BtnStyle = SceneStyles.ButtonStyle();
             //
               foreach (var scene in _SceneDictionary)
               {
                   if (ScenesBrowserExtender.IsNotSceneNull(scene) && GUILayout.Button(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image), _BtnStyle, GUILayout.Width(_Width), GUILayout.Height(_Heigth)))
                   {
                       // If there unsave change > ask if i want to save , If user click yes
                       if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                           // Open scene
                           EditorSceneManager.OpenScene(scene.Key);

                       // To avoid : "EndLayoutGroup: BeginLayoutGroup must be called first."
                       GUIUtility.ExitGUI();
                   }
               } */

            // TODO: Move this to SettingsAndRefreshButton()
            var _SceneNameAndIcon = new List<GUIContent>();
            foreach (var scene in _SceneDictionary)
            {
                if (ScenesBrowserExtender.IsNotSceneNull(scene) && !_SceneNameAndIcon.Contains(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image)))
                    _SceneNameAndIcon.Add(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image));
            }

            _ScenesToolbarGridSize = GUILayout.Toolbar(_ScenesToolbarGridSize, _SceneNameAndIcon.ToArray(), GUILayout.MaxWidth(_Width * _SceneNameAndIcon.Count), GUILayout.MaxHeight(_Heigth));

            // Is not thie same scene ? load the new scene
            if (_ScenesToolbarGridSize != _DataSettings.m_PreviousScenesToolbarGridSize && !_IsSaveWindwoOpen)
            {
                // So this dumb.. but to make this SaveCurrentModifiedScenesIfUserWantsTo() not show tows >> Fix me or let me alive
                _IsSaveWindwoOpen = true;

                // If there unsave change > ask if i want to save , If user click yes
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {   // Open scene
                    onOpenNewScene?.Invoke(_ScenesToolbarGridSize);
                    ResetIsSaveWindwoOpen();
                }
                else
                {
                    _ScenesToolbarGridSize = _DataSettings.m_PreviousScenesToolbarGridSize;
                    ResetIsSaveWindwoOpen();
                }

                // To avoid : "EndLayoutGroup: BeginLayoutGroup must be called first."
                GUIUtility.ExitGUI();
            }


        }
        protected static void OpenNewScene(int index)
        {
            _DataSettings.m_PreviousScenesToolbarGridSize = index;
            EditorSceneManager.OpenScene(_SceneDictionary.ElementAt(_DataSettings.m_PreviousScenesToolbarGridSize).Key);
        }
        private static void SettingsAndRefreshButton()
        {
            // Settings && Refresh
            using (var scenes = new EditorGUILayout.HorizontalScope(SceneStyles.SettingsAndRefreshStyle(), GUILayout.Width(_WidthSettingsAndRefreshBG)))
            {
                //                                                                                                   22                        22
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("EditorSettings Icon").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                    ShowScenesBrowserSettings();
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Refresh@2x").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                    AddSceneToDictionary();
            }
        }
        private static void AddSceneToDictionary()
        {
            // If Auto find scenes active > Find all scene in this ptoject
            if (_DataSettings.m_AutoFindScene)
                _GetAllScenesInProject = Directory.GetFiles(Application.dataPath, _FilterBy, SearchOption.AllDirectories);
            else
            {
                if (_DataSettings.m_ScenePath != string.Empty)
                    _GetAllScenesInProject = Directory.GetFiles(_DataSettings.m_ScenePath, _FilterBy);
                else
                    Debug.LogError($"Path is null {_DataSettings.m_ScenePath}");
            }

            // Start adding to dictionary
            foreach (var sc in _GetAllScenesInProject)
            {
                // Get path
                var _Path = sc.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                // Get scenes name
                var _Name = Between(_Path, "Scenes/", ".unity");
                // We don't have this ??
                if (!_SceneDictionary.ContainsKey(_Path))
                    // Add it
                    _SceneDictionary.Add(_Path, AssetDatabase.LoadAssetAtPath<SceneAsset>(_Path));
            }

        }
        // I find this on (https://stackoverflow.com/) but i forget to copy the link , now i don't remember what this do );
        private static string Between(string str, string firstStr, string lastStr)
        {
            string FinalString;
            int Pos1 = str.IndexOf(firstStr) + firstStr.Length;
            int Pos2 = str.IndexOf(lastStr);
            FinalString = str.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }
        private void NewReset()
        {
            Debug.Log("Fun Reset $Remove me");
            _DataSettings.m_ScenePath = "";

        }
        public static void GetSettingsData()
        {
            // Data folder
            var _DataPath = _DataSavePath + "/Data";
            // If we don't have data folder ... create new one
            if (!Directory.Exists(_DataPath))
            {
                // Create directory/Folder
                Directory.CreateDirectory(_DataPath);
                // Refresh assets
                AssetDatabase.Refresh();
            }
            // Get data paths  
            var _FindAllSettingsData = Directory.GetFiles(_DataPath, "*.asset"); //[0];
                                                                                 // Get (data.asset)
            _DataSettings = (_FindAllSettingsData.Length == 0) ? ScenesBrowserExtender.CreateNewData(_DataPath) : (SBD)EditorGUIUtility.Load(_FindAllSettingsData[0]);
        }


    }
}
