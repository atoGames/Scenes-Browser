using UnityEditor;
using UnityEngine;
using System;
[Serializable]
public class SBScene
{
    public string ScenePath;
    public SceneAsset Scene;
    // public int ChoisSelect;
    public bool Hide = false;
    public bool Active = false;
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