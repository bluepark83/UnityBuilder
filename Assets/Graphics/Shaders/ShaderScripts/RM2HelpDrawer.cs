using System.IO;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
public class HelpURLDecorator : MaterialPropertyDrawer
{
    private string url;
    private GUIContent buttonGUIContent;

    public HelpURLDecorator(string url)
    {
        this.url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h." + url;
        var helpIcon = EditorGUIUtility.FindTexture("_Help");
        buttonGUIContent = new GUIContent(helpIcon, "Open Online Documentation");
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        var headerPos = new Rect(position.x, position.y, position.width - 20, 20);
        var btnPos = new Rect(position.x + headerPos.width, position.y, 20, 20);
        GUI.Label(headerPos, new GUIContent("Help"), EditorStyles.boldLabel);


        if (GUI.Button(btnPos, buttonGUIContent, new GUIStyle("IconButton")))
        {
            Help.BrowseURL(url);
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 20;
    }

}
#endif