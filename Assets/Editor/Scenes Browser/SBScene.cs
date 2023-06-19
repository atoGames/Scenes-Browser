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
    public SBScene(string scenePath, SceneAsset scene)
    {
        ScenePath = scenePath;
        Scene = scene;
    }

}