using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScenesBrowser.Data
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

        public void SetActiveScene(SBScene sbScene)
        {
            // Deactivate all scene
            for (var i = 0; i < _SceneList.Count; i++)
                _SceneList[i].Active = false;

            //  Get active scene
            sbScene.Active = true;
        }


    }
}
