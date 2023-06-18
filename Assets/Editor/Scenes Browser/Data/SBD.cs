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

        public int GetActiveScene(int index)
        {
            var _CurrentIndex = index;
            // Deactivate all scene
            for (var i = 0; i < _SceneList.Count; i++)
                _SceneList[i].Active = false;

            // The scene we call is hiding ? Get another scene
            if (_SceneList[_CurrentIndex].Hide)
            {
                _CurrentIndex = _SceneList.IndexOf(_SceneList.Find(active => !active.Hide));
                Debug.Log("This is hideing " + index + " New one : " + _CurrentIndex);
            }
            else
            {
                // Get scene index if hide false
                _CurrentIndex = _SceneList.IndexOf(_SceneList[_CurrentIndex]);
                Debug.Log("This not hideing " + _CurrentIndex);
            }
            // Set active scene
            _SceneList[_CurrentIndex].Active = true;

            return _CurrentIndex;
        }


    }
}
