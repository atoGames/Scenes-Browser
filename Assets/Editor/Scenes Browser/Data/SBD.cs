using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScenesBrowser.Utils;
using UnityEditor;
using UnityEngine;

namespace ScenesBrowser.Data
{
    public class SBD : ScriptableObject
    {
        // Save current index
        // public int m_CurrentSceneIndex = 0;
        // Enable this to auto find scene in the project , else enter a folder path
        public bool m_AutoFindScene = true;
        // Toolbar at : True = Left , false = Rigth
        public bool m_IsLeft = true;
        // Show or hide scens on toolbar
        public bool m_ShowQuickAccess = true;
        // Scene folder path : to load scene from it
        public string m_ScenePath = string.Empty;
        // List of all scenes
        [SerializeField] protected List<SBScene> _SceneList = new();
        // List of scenes
        public List<SBScene> SceneList { get => _SceneList; }

        /// <summary>
        /// Return true if the list has this scene
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public bool IsContainScene(SceneAsset scene) => _SceneList.Find(c => c.Scene == scene) != null;

        /// <summary>
        /// Add a scene
        /// </summary>
        public void AddScene(SBScene sbScene) => _SceneList.Add(sbScene);
        /// <summary>
        ///  Activate a scene
        /// </summary>
        /// <param name="sbScene"></param>
        public void SetActiveScene(SBScene sbScene)
        {
            foreach (var scene in _SceneList) scene.Active = scene == sbScene;
        }
        /// <summary>
        /// Check for any change in scene
        /// </summary>
        public void OnSceneChange(bool clearList = false)
        {
            // If this true , Clear
            if (clearList)
            {
                SceneList.Clear();
                return;
            }
            // Else , Remove the empty one
            for (int i = SceneList.Count - 1; i >= 0; i--)
            {
                if (SceneList[i].Scene == null)
                    SceneList.RemoveAt(i);
            }
        }

    }
}
