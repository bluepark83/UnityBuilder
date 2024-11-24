using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class CustomShaderGUI : ShaderGUI
{
    public enum PropType
    {
        Float = 2,
        Texture = 4,
        Int = 5,
        Vector2 = 6,
        Vector3 = 7,
        TextureNoOption = 9
    }
    public enum CullMode
    {
        Off = 0,
        Front = 1,
        Back = 2
    }

    public string url;
    public CullMode cullMode = CullMode.Back;
    public int renderQueue = 2005;
    // public GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold };
    public GUIStyle headerStyle;
    public void SetHelp(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (headerStyle == null) headerStyle = new GUIStyle(EditorStyles.boldLabel);
        var helpIcon = EditorGUIUtility.FindTexture("_Help");
        GUIContent buttonGUIContent = new GUIContent(helpIcon, url);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Help"), headerStyle);
        if (GUILayout.Button(buttonGUIContent, new GUIStyle("IconButton")))
        {
            Help.BrowseURL(url);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(20);
    }


    public int headerSpace = 5;
    public int categorySpace = 20;

    public class Reference
    {
        public string headerName;
        public List<string> refName = new List<string>();
        public List<string> disName = new List<string>();
        public List<string> tooltip = new List<string>();
        public int Count = 0;
        public void SetReference(int start, int end, MaterialProperty[] properties, Reference reference)
        {
            for (int i = start; i < end; i++)
            {
                reference.refName.Add(properties[i].name);
                reference.disName.Add(properties[i].displayName);
                reference.tooltip.Add("");
                Count++;
            }
        }
    }

    public void MakeProperty(string propName, string setName, string tooltip, MaterialEditor materialEditor, MaterialProperty[] properties, PropType type = PropType.Float)
    {
        MaterialProperty tempMatProp = FindProperty(propName, properties);

        switch (type)
        {
            case PropType.Float:
                materialEditor.ShaderProperty(tempMatProp, MakeToolTip(setName, tooltip));
                break;
            case PropType.Texture:
                materialEditor.TextureProperty(tempMatProp, setName, true);
                break;
            case PropType.TextureNoOption:
                materialEditor.TextureProperty(tempMatProp, setName, false);
                break;
            case PropType.Int:
                MakeIntProperty(tempMatProp, setName);
                break;
            case PropType.Vector2:
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 300f + EditorGUIUtility.fieldWidth;
                MakeVector2Property(tempMatProp, setName);
                materialEditor.SetDefaultGUIWidths();                
                break;
            case PropType.Vector3:
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 300f + EditorGUIUtility.fieldWidth;
                MakeVector3Property(tempMatProp, setName);
                materialEditor.SetDefaultGUIWidths();                
                break;
            default:
                materialEditor.ShaderProperty(tempMatProp, MakeToolTip(setName, tooltip));
                break;
        }
    }
    public void MakeProperty(string propName, string setName, MaterialEditor materialEditor, MaterialProperty[] properties, PropType type = PropType.Float)
    {
        MakeProperty(propName, setName, "", materialEditor, properties, type);
    }
    public void MakeIntProperty(MaterialProperty intProp, string setName)
    {
        intProp.floatValue = EditorGUILayout.IntSlider(setName, (int)intProp.floatValue, 0, 10, GUILayout.ExpandWidth(false));
    }

    public void MakeVector2Property(MaterialProperty prop, string setName)
    {
        prop.vectorValue = EditorGUILayout.Vector2Field(setName, prop.vectorValue);
    }
    public void MakeVector3Property(MaterialProperty prop, string setName)
    {
        prop.vectorValue = EditorGUILayout.Vector3Field(setName, prop.vectorValue);
    }


    public GUIContent MakeToolTip(string setName, string toolTip)
    {
        return EditorGUIUtility.TrTextContent(setName, toolTip);
    }

    public void MakeHeader(string headerName)
    {
        if (headerStyle == null) headerStyle = new GUIStyle(EditorStyles.boldLabel);

        EditorGUILayout.LabelField(headerName, headerStyle);
        EditorGUILayout.Space(headerSpace);
    }

    public void CullModeField(MaterialEditor materialEditor, MaterialProperty[] prop)
    {
        MaterialProperty cullProp = FindProperty("_Cull", prop);
        Material targetMat = materialEditor.target as Material;
        bool alphaTest = ArrayUtility.IndexOf(targetMat.shaderKeywords, "_ALPHATEST_ON") != -1;
        
        cullMode = (CullMode)cullProp.floatValue;
        EditorGUI.BeginChangeCheck();
        cullMode = (CullMode)EditorGUILayout.EnumPopup("Cull Mode", cullMode);

        if (EditorGUI.EndChangeCheck())
        {
            AutoRenderQueue(targetMat, cullProp, alphaTest);
        }
    }
    public void AlphaClipField(MaterialEditor materialEditor, MaterialProperty[] prop, bool skip = false)
    {
        MaterialProperty cullProp = FindProperty("_Cull", prop);
        Material targetMat = materialEditor.target as Material;

        bool alphaTest = true;
        if (!skip)
        {
            alphaTest= ArrayUtility.IndexOf(targetMat.shaderKeywords, "_ALPHATEST_ON") != -1;
            EditorGUI.BeginChangeCheck();
            alphaTest = EditorGUILayout.Toggle("Alpha Clipping", alphaTest);
            if (EditorGUI.EndChangeCheck())
            {
                AutoRenderQueue(targetMat, cullProp, alphaTest);
            }
        }
        else
        {
            AutoRenderQueue(targetMat, cullProp, true);
        }
        
    }
    public void AutoRenderQueue(Material targetMat, MaterialProperty cullProp, bool alphaTest)
    {
        cullProp.floatValue = (float)cullMode;

        if (alphaTest)
        {
            targetMat.EnableKeyword("_ALPHATEST_ON");

            switch (cullProp.floatValue)
            {

                case 2:
                    targetMat.renderQueue = renderQueue;
                    break;
                case 1:
                    targetMat.renderQueue = renderQueue + 10;
                    break;
                case 0:
                    targetMat.renderQueue = renderQueue + 20;
                    break;
            }
            targetMat.renderQueue += 5;
        }
        else
        {
            targetMat.DisableKeyword("_ALPHATEST_ON");
            switch (cullProp.floatValue)
            {

                case 2:
                    targetMat.renderQueue = renderQueue;
                    break;
                case 1:
                    targetMat.renderQueue = renderQueue + 10;
                    break;
                case 0:
                    targetMat.renderQueue = renderQueue + 20;
                    break;
            }
        }
    }

    public void DeleteShaderKeyword(MaterialEditor materialEditor)
    {
        Material targetMat = materialEditor.target as Material;
        targetMat.shaderKeywords = new string[0];
    }

}

