
using UnityEngine;

public class SceneStyle
{
    public static GUIStyle Style(Texture2D texBackground, Texture2D texBackgroundHover)
    {
        var m_SceneStyle = new GUIStyle(GUI.skin.button);
        m_SceneStyle.fontSize = 11;
        m_SceneStyle.wordWrap = true;
        m_SceneStyle.stretchWidth = true;
        m_SceneStyle.alignment = TextAnchor.MiddleCenter;
        m_SceneStyle.normal.textColor = Color.white;
        m_SceneStyle.normal.background = texBackground;
        m_SceneStyle.overflow = new RectOffset(-2, -2, 0, 0);
        m_SceneStyle.hover.background = texBackgroundHover;
        //m_SceneStyle.onHover.background = m_TexBackgroundHover;
        return m_SceneStyle;
    }
}