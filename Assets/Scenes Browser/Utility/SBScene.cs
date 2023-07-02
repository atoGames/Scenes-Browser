using UnityEditor;
using System;

namespace ScenesBrowser.Utility
{

    [Serializable]
    public class SBScene
    {
        // Screen folder path .. manually
        public string ScenePath;
        // Scene asset
        public SceneAsset Scene;
        // Used to know if the scene available
        public bool Hide = false;
        // Used to know what scene is active
        public bool Active = false;
        // Used to know is user want to change scene name
        public bool IsRenameSceneActive = false;

        public SBScene(string scenePath, SceneAsset scene)
        {
            ScenePath = scenePath;
            Scene = scene;
        }
        /// <summary>
        /// Set new scene name
        /// </summary>
        /// <param name="newSceneName"></param>
        public void SetNewSceneName(string newSceneName)
        {
            var _OldName = Scene.name;
            // Set new scene name 
            AssetDatabase.RenameAsset(ScenePath, newSceneName);
            //Update path
            UpdatePath(_OldName, newSceneName);
            // Close
            DisableRename();
        }
        //Update path
        protected void UpdatePath(string oldName, string newNmae) => ScenePath = ScenePath.Replace(oldName, newNmae);
        // Disable rename
        internal void DisableRename() => IsRenameSceneActive = false;
    }
}
