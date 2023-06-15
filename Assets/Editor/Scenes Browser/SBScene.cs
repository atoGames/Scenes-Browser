using UnityEditor;
using UnityEngine;

public class SBScene
{
    public string ScenePath;
    public SceneAsset Scene;
    // public int ChoisSelect;
    public bool Hide;
    public SBScene(string scenePath, SceneAsset scene, bool hide)
    {
        ScenePath = scenePath;
        Scene = scene;
        Hide = hide;
    }

}