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
        protected static float _Width = 64, _Heigth = 20;
        // For filtering scenes
        protected const string _FilterBy = "*.unity";
        private const string _DataSavePath = "Assets/Editor/Scenes Browser";
        protected static string[] _GetAllScenesInProject;


        // To save stuff
        protected static SBD _DataSettings;

        #region Top bar 
        protected string _ShowTopBarAt = "Show Toolbar At: ";
        // For scroll content 
        protected static Vector2 _ScrollPosAtTop;
        // Width & Heigth Settings and refresh BG
        protected static int _WidthSettingsAndRefreshBG = 26, _HeigthSettingsAndRefreshBG = 20;
        // Settings and refresh BG
        protected static Texture2D _SettingsAndRefreshBG; //= CreateNewTexture2D(16, 16, CreateNewColor());
        #endregion
        private static Vector2 _ScrollPosAtSettingsWindow;

        // Use this to save / remove / order
        // Key is the path of the scene , value is the scene it's self
        private static Dictionary<string, SceneAsset> _Scenes = new Dictionary<string, SceneAsset>();
        // Grid thing
        protected int _GridSize = 0;
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

            // We don't have settings-data ? 
            if (!_DataSettings)
                GetSettingsData();
            // We have the data
            if (_DataSettings)
                ToolbarExtender.AddToolBarGUI(_DataSettings.m_IsLeft, OnToolbarGUI);

            // 
            AddSceneToDictionary();



        }
        private void OnEnable()
        {
            _SettingsAndRefreshBG ??= CreateNewTexture2D(16, 16, CreateNewColor());
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
                    var _AutoContent = new GUIContent();
                    _AutoContent.text = "Auto";
                    _AutoContent.tooltip = "Auto find scenes";

                    _DataSettings.m_AutoFindScene = EditorGUILayout.Toggle(_DataSettings.m_AutoFindScene, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField(_AutoContent, GUILayout.MaxWidth(50));
                    if (!_DataSettings.m_AutoFindScene)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Select Path : ", GUILayout.Width(75), GUILayout.Height(_Heigth));
                        _DataSettings.m_ScenePath = EditorGUILayout.TextField(_DataSettings.m_ScenePath, GUILayout.Height(_Heigth));
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("FolderOpened On Icon")), GUILayout.Width(25), GUILayout.Height(_Heigth)))
                        {
                            SelectFolder(Application.dataPath);
                            Debug.Log("Open scenes folder to select path");
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                }
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Preset.Context")), GUILayout.Width(25), GUILayout.Height(_Heigth)))
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
                    EditorGUILayout.LabelField(_DataSettings.m_IsLeft ? _ShowTopBarAt + " Left " : _ShowTopBarAt + " Right ", GUILayout.MaxWidth(150));

                    // Quick Access
                    _DataSettings.m_ShowQuickAccess = EditorGUILayout.Toggle(_DataSettings.m_ShowQuickAccess, GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(_DataSettings.m_ShowQuickAccess ? " Hide Quick Access " : " Show Quick Access ", GUILayout.MaxWidth(128));



                    // EditorGUILayout.LabelField("Width : ", GUILayout.MaxWidth(45));
                    // _WindowSettingsMaxSize.x = EditorGUILayout.FloatField(_WindowSettingsMaxSize.x, GUI.skin.textField/* , GUILayout.Width(155) */);
                    // EditorGUILayout.LabelField("Heigth : ", GUILayout.MaxWidth(45));
                    // _WindowSettingsMaxSize.y = EditorGUILayout.FloatField(_WindowSettingsMaxSize.y, GUI.skin.textField/* , GUILayout.Width(155) */);

                }

                ShowSceneOnWindowSettingsGrid();


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

        private void ShowSceneOnWindowSettingsGrid()
        {

            // Grid - /* EditorStyles.helpBox */
            using (var _ShowSceneOnWindow = new EditorGUILayout.VerticalScope("Box"))
            {
                if (GUILayout.Button(_GridCulm.ToString(), GUILayout.Width(22), GUILayout.Height(_Heigth)))
                {
                    _GridCulm += 2;
                    // _GridCulm = Mathf.Clamp(_GridCulm, 1, 10);
                    if (_GridCulm > 6)
                        _GridCulm = 2;

                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_ScrollPosAtSettingsWindow))
                {
                    _ScrollPosAtSettingsWindow = scrollView.scrollPosition;
                    // ShowScenes(64, 64);
                    // TODO: I don't like this but is work!
                    var _Names = new List<string>();
                    foreach (var scene in _Scenes)
                    {
                        if (IsNotSceneNull(scene) && !_Names.Contains(scene.Value.name))
                            _Names.Add(scene.Value.name);
                    }

                    // var ss = new GUIStyle();
                    // ss.onActive.background = (Texture2D)EditorGUIUtility.IconContent("SceneAsset On Icon").image;

                    // This is a old ? whoe care
                    _GridSize = GUILayout.SelectionGrid(_GridSize, _Names.ToArray(), _GridCulm, GUILayout.MaxWidth(512), GUILayout.MaxHeight(512));
                }

            }
        }

        private static void OnToolbarGUI()
        {
            GetScenes();
        }

        public static void GetScenes()
        {
            // Settings and refresh button
            SettingsAndRefreshButton();
            // Scroll - all scenes 
            _ScrollPosAtTop = EditorGUILayout.BeginScrollView(_ScrollPosAtTop, false, true, GUILayout.MinHeight(50));
            using (var scenes = new EditorGUILayout.HorizontalScope())
            {
                // If mouse over rect/content
                if (scenes.rect.Contains(Event.current.mousePosition))
                {
                    // Scroll by scroll wheel
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        // Scroll x value > Horizontal
                        _ScrollPosAtTop.x += Event.current.delta.y * 10f;
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


            //
            foreach (var scene in _Scenes)
            {
                if (IsNotSceneNull(scene) && GUILayout.Button(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image), GUILayout.Width(_Width), GUILayout.Height(_Heigth)))
                {
                    // If there unsave change > ask if i want to save , If user click yes
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        // Open scene
                        EditorSceneManager.OpenScene(scene.Key);

                    // To avoid : "EndLayoutGroup: BeginLayoutGroup must be called first."
                    GUIUtility.ExitGUI();
                }
            }

        }
        private static void SettingsAndRefreshButton()
        {
            if (_DataSettings.m_ShowQuickAccess)
            {
                var sty = new GUIStyle();
                sty.normal.background = _SettingsAndRefreshBG;

                // Settings && Refresh
                using (var scenes = new EditorGUILayout.HorizontalScope(sty, GUILayout.Width(_WidthSettingsAndRefreshBG)))
                {
                    //                                                                                                   22                        22
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("EditorSettings Icon").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                        ShowScenesBrowserSettings();
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Refresh@2x").image), GUILayout.Width(_WidthSettingsAndRefreshBG), GUILayout.Height(_HeigthSettingsAndRefreshBG)))
                        AddSceneToDictionary();
                }
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
                if (!_Scenes.ContainsKey(_Path))
                    // Add it
                    _Scenes.Add(_Path, AssetDatabase.LoadAssetAtPath<SceneAsset>(_Path));
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
        private static void GetSettingsData()
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
            _DataSettings = (_FindAllSettingsData.Length == 0) ? CreateNewData(_DataPath) : (SBD)EditorGUIUtility.Load(_FindAllSettingsData[0]);
        }
        // Create a new data to save the settings
        private static SBD CreateNewData(string path)
        {
            var _Data = ScriptableObject.CreateInstance<SBD>();
            AssetDatabase.CreateAsset(_Data, path + "/Data.asset");
            AssetDatabase.SaveAssets();
            return _Data;
        }
        // Select folder > Get path by select folder
        protected void SelectFolder(string path)
        {
            // If path null-Empty return
            if (path == string.Empty)
                return;
            // Curent project path
            var _CurrentProjectPath = path + "/Assets";
            // Return the full path
            var _ScenePath = EditorUtility.OpenFolderPanel("Select Scenes Folder", _CurrentProjectPath, "Scenes");
            // Don't show full path .. just (Assets/..)
            _DataSettings.m_ScenePath = (_ScenePath.Contains("Assets")) ? _ScenePath.Substring(_ScenePath.IndexOf("Assets")) : _CurrentProjectPath;
        }
        public static bool IsNotSceneNull(KeyValuePair<string, SceneAsset> scene) => scene.Value != null;
        private static Texture2D CreateNewTexture2D(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        private static Color CreateNewColor(string hex = "3C3C3C")
        {
            //    => new Color(60f / 256f, 60f / 256f, 60f / 256f, 1f);
            var _Color = new Color();
            // Get settings and refresh BG color 
            if (ColorUtility.TryParseHtmlString("#" + hex, out _Color))
                return _Color;

            return _Color = Color.black;
        }
    }
}
