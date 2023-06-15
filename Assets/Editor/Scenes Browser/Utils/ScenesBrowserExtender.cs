using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScenesBrowser
{
    public class ScenesBrowserExtender
    {
        // List of all scenes
        protected static List<SBScene> _SceneList = new();
        public static List<SBScene> SceneList { get => _SceneList; }
        public static bool IsContainScene(SceneAsset scene) => _SceneList.Find(c => c.Scene == scene) != null;
        /// <summary>
        /// Add scene
        /// </summary>
        public static void AddScene(SBScene sbScene) => _SceneList.Add(sbScene);


        protected static List<GUIContent> _SceneNameAndIcon = new List<GUIContent>();
        public static GUIContent[] GetSceneNameAndIcon()
        {
            foreach (var scene in ScenesBrowserExtender.SceneList)
            {
                Debug.Log(scene);
                // if (ScenesBrowserExtender.IsSceneNotNull(scene) && !_SceneNameAndIcon.Contains(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image)))
                // _SceneNameAndIcon.Add(new GUIContent(scene.Value.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image));
                if (scene != null)
                    _SceneNameAndIcon.Add(new GUIContent(scene?.Scene.name, EditorGUIUtility.IconContent("SceneAsset On Icon").image));
            }

            return _SceneNameAndIcon.ToArray();
        }


        // Create a new data to save the settings
        public static SBD CreateNewData(string path)
        {
            var _Data = ScriptableObject.CreateInstance<SBD>();
            AssetDatabase.CreateAsset(_Data, path + "/Data.asset");
            AssetDatabase.SaveAssets();
            return _Data;
        }
        // Select folder > Get path by select folder
        public static void SelectFolder(string path, SBD _DataSettings)
        {
            // If path null-Empty return
            if (path == string.Empty)
                return;
            // Curent project path
            // var _CurrentProjectPath = path + "/Assets";
            // Return the full path
            var _ScenePath = EditorUtility.OpenFolderPanel("Select Scenes Folder", path, "Scenes");
            // Path not null ?
            if (!string.IsNullOrEmpty(_ScenePath))
                // Don't show full path .. just (Assets/..)
                _DataSettings.m_ScenePath = (_ScenePath.Contains("Assets")) ? _ScenePath.Substring(_ScenePath.IndexOf("Assets")) : path;
        }
        public static bool IsSceneNotNull(string path) => _SceneList.Find(p => p.ScenePath == path) != null;
        public static Texture2D CreateNewTexture2D(int width, int height, Color col)
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
        public static Color CreateNewColor(string hex = "3C3C3C")
        {
            //    => new Color(60f / 256f, 60f / 256f, 60f / 256f, 1f);
            var _Color = new Color();
            // Get settings and refresh BG color 
            if (ColorUtility.TryParseHtmlString("#" + hex, out _Color))
                return _Color;

            return _Color = Color.black;

        }

        public static GUIContent GetGUIContent(string text, string tooltip, Texture texture = null)
        {
            var _GUIContent = new GUIContent();
            _GUIContent.text = text;
            _GUIContent.tooltip = tooltip;
            _GUIContent.image = texture;


            return _GUIContent;
        }

        public static string Between(string path, string firstStr, string lastStr)
        {
            var _Str = path.IndexOf(firstStr) + firstStr.Length;
            return path.Substring(_Str, path.IndexOf(lastStr) - _Str);
        }
    }
}