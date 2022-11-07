using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "SBD", menuName = "SBD/New data", order = 1)]
namespace ScenesBrowser
{
    public class SBD : ScriptableObject
    {
        public bool m_AutoFindScene = true;
        // True = Left , false = Rigth
        public bool m_IsLeft = true;
        public bool m_ShowQuickAccess = true;
        public string m_ScenePath = string.Empty;


    }
}
