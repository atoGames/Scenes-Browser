using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

        protected int _CurrentIndex = 0;
        public int GetActiveScene(string sceneName)
        {
            /*  // Deactivate all scene
             for (var i = 0; i < _SceneList.Count; i++)
                 _SceneList[i].Active = false; */

            //  Get active scene
            var _ActiveScane = _SceneList.Find(ac => ac.Scene.name == sceneName);
            _CurrentIndex = _SceneList.IndexOf(_ActiveScane);

            /*  if (_ActiveScane.Hide && (_CurrentIndex >= _SceneList.Count - 1))
             {
                 _CurrentIndex--;
                 Debug.Log(_CurrentIndex);
             } */
            // _ActiveScane.Active = true;


            return _CurrentIndex;
        }


    }
}
