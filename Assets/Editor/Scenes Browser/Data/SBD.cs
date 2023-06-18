using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// [CreateAssetMenu(fileName = "SBD", menuName = "SBD/New data", order = 1)]
namespace ScenesBrowser
{
    public class SBD : ScriptableObject
    {
        public int m_PreviousScenesToolbarGridSize = 0;

        public bool m_AutoFindScene = true;
        // True = Left , false = Rigth
        public bool m_IsLeft = true;
        public bool m_ShowQuickAccess = true;
        public string m_ScenePath = string.Empty;

        // List of all scenes
        [SerializeField] protected List<SBScene> _SceneList = new();
        public List<SBScene> SceneList { get => _SceneList; }
        public bool IsContainScene(SceneAsset scene) => _SceneList.Find(c => c.Scene == scene) != null;
        /// <summary>
        /// Add scene
        /// </summary>
        public void AddScene(SBScene sbScene) => _SceneList.Add(sbScene);
        public bool IsSceneNotNull(string path) => _SceneList.Find(p => p.ScenePath == path) != null;


    }
}
