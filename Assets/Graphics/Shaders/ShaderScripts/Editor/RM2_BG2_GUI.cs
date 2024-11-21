using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class RM2_BG2_GUI : CustomShaderGUI
{
    // SurfaceOptions은 Shader Properties 항목 위에서부터 밑으로 쭉 긁어옴
    // SurfaceOptions.Name 은 이름
    // SurfaceOptions.Tooltip 은 이름으로 되어있음
    
    public static readonly string[] stencilPassNames = Enum.GetNames(typeof(StencilOp));
    public static readonly GUIContent stencilPassText = EditorGUIUtility.TrTextContent("Stencil Pass",
        "");
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        renderQueue = 2005;
        int surfaceOptionCount = 7;
        int surfaceInputsCount = 3 + surfaceOptionCount;
        int normalMaskInputsCount = 4 + surfaceInputsCount;
        int specularInputsCount = 5 + normalMaskInputsCount;
        int matcapInputsCount = 8 + specularInputsCount;
        int EmissiveInputsCount = 8 + matcapInputsCount;
        int AdvancedMaskInputsCount = 11 + EmissiveInputsCount;
        int YPlanarCount = 12 + AdvancedMaskInputsCount;

        Reference SurfaceOptions = new Reference() { headerName = "Surface Options" };
        Reference SurfaceInputs = new Reference() { headerName = "Surface Inputs" };
        Reference NormalMaskInputs = new Reference() { headerName = "NormalMask Inputs" };
        Reference SpecularInputs = new Reference() { headerName = "Specular Inputs" };
        Reference MatcapInputs = new Reference() { headerName = "Matcap Inputs" };
        Reference EmissiveInputs = new Reference() { headerName = "Emissive Inputs" };
        Reference AdvancedMaskInputs = new Reference() {headerName = "- Advenced Mask Inputs"};
        Reference YPlanarInputs = new Reference() { headerName = "Y Planar Inputs"};
        
        Material targetMat = materialEditor.target as Material;

        url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.ao5p2oykiewo";
        SetHelp(materialEditor, properties);
        materialEditor.SetDefaultGUIWidths();

        SurfaceOptions.SetReference(0, surfaceOptionCount, properties, SurfaceOptions);
        SurfaceInputs.SetReference(surfaceOptionCount, surfaceInputsCount,  properties, SurfaceInputs);
        NormalMaskInputs.SetReference(surfaceInputsCount, normalMaskInputsCount,  properties, NormalMaskInputs);
        SpecularInputs.SetReference(normalMaskInputsCount, specularInputsCount,  properties, SpecularInputs);
        MatcapInputs.SetReference(specularInputsCount, matcapInputsCount,  properties, MatcapInputs);
        EmissiveInputs.SetReference(matcapInputsCount, EmissiveInputsCount,  properties, EmissiveInputs);
        AdvancedMaskInputs.SetReference(EmissiveInputsCount, AdvancedMaskInputsCount, properties, AdvancedMaskInputs);
        YPlanarInputs.SetReference(AdvancedMaskInputsCount, YPlanarCount,  properties, YPlanarInputs);
        
        // Surface Options
        MakeHeader(SurfaceOptions.headerName);
        CullModeField(materialEditor, properties);
        EditorGUILayout.Space(categorySpace - 15);
        
        MakeProperty(SurfaceOptions.refName[0], SurfaceOptions.disName[0], SurfaceOptions.tooltip[0], materialEditor, properties);

        var enableGlobalShadow = FindProperty("_Using_Global_Shadow_Color", properties).floatValue == 0;
        // bool enableGlobalShadow = ArrayUtility.IndexOf(targetMat.GetPropertyNames(MaterialPropertyType.Float), "_Using_Global_Shadow_Color") != 0;
        if(enableGlobalShadow)
        {
            MakeProperty(SurfaceOptions.refName[1], SurfaceOptions.disName[1], SurfaceOptions.tooltip[1], materialEditor, properties);
        }
        MakeProperty(SurfaceOptions.refName[2], SurfaceOptions.disName[2],  materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[3], SurfaceOptions.disName[3], SurfaceOptions.tooltip[3], materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[4], SurfaceOptions.disName[4], SurfaceOptions.tooltip[4], materialEditor, properties);
        EditorGUILayout.Space(categorySpace - 15);
        
        MakeProperty(SurfaceOptions.refName[5], SurfaceOptions.disName[5], SurfaceOptions.tooltip[5], materialEditor, properties);
        EditorGUILayout.Space(categorySpace - 15);
        
        // AlphaClipField(materialEditor, properties);
        MaterialProperty alphaclipping = FindProperty("_AlphaClipping", properties);
        materialEditor.ShaderProperty(alphaclipping, "Alpha Clipping");
        if (alphaclipping.floatValue != 0)
        {
            targetMat.EnableKeyword("_ALPHATEST_ON");
        }
        else
        {
            targetMat.DisableKeyword("_ALPHATEST_ON");
        }
        
        bool enableAlphaTest = ArrayUtility.IndexOf(targetMat.shaderKeywords, "_ALPHATEST_ON") != -1;
        if (enableAlphaTest)
        {
            MakeProperty(SurfaceOptions.refName[6], SurfaceOptions.disName[6], SurfaceOptions.tooltip[6], materialEditor, properties);
        }
        EditorGUILayout.Space(categorySpace);


        //Surface Inputs
        MakeHeader(SurfaceInputs.headerName);
        MakeProperty(SurfaceInputs.refName[0], SurfaceInputs.disName[0],SurfaceInputs.tooltip[0], materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[1], SurfaceInputs.disName[1],SurfaceInputs.tooltip[1], materialEditor, properties);

        MakeProperty(SurfaceInputs.refName[2], SurfaceInputs.disName[2], materialEditor, properties);
        EditorGUILayout.Space(categorySpace);

        //NormalMask Inputs
        MakeHeader(NormalMaskInputs.headerName);
        MakeProperty(NormalMaskInputs.refName[0], NormalMaskInputs.disName[0], NormalMaskInputs.tooltip[0], materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[1], NormalMaskInputs.disName[1], materialEditor, properties, PropType.TextureNoOption);
        MakeProperty(NormalMaskInputs.refName[2], NormalMaskInputs.disName[2], NormalMaskInputs.tooltip[2], materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[3], NormalMaskInputs.disName[3], NormalMaskInputs.tooltip[3], materialEditor, properties);
        EditorGUILayout.Space(categorySpace);
        
        
        //Specular Inputs
        MakeHeader(SpecularInputs.headerName);

        MakeProperty(SpecularInputs.refName[0], SpecularInputs.disName[0], SpecularInputs.tooltip[0], materialEditor, properties);
        MakeProperty(SpecularInputs.refName[1], SpecularInputs.disName[1],SpecularInputs.tooltip[1], materialEditor, properties);
        MakeProperty(SpecularInputs.refName[2], SpecularInputs.disName[2], SpecularInputs.tooltip[2], materialEditor, properties);
        MakeProperty(SpecularInputs.refName[3], SpecularInputs.disName[3], SpecularInputs.tooltip[3], materialEditor, properties);
        MakeProperty(SpecularInputs.refName[4], SpecularInputs.disName[4], SpecularInputs.tooltip[4], materialEditor, properties);

        EditorGUILayout.Space(categorySpace);
        

        //Matcap Inputs
        MakeHeader(MatcapInputs.headerName);

        MakeProperty(MatcapInputs.refName[0], MatcapInputs.disName[0], MatcapInputs.tooltip[0], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[1], MatcapInputs.disName[1], MatcapInputs.tooltip[1], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[2], MatcapInputs.disName[2], MatcapInputs.tooltip[2], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[3], MatcapInputs.disName[3], MatcapInputs.tooltip[3], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[4], MatcapInputs.disName[4], MatcapInputs.tooltip[4], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[5], MatcapInputs.disName[5], MatcapInputs.tooltip[5], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[6], MatcapInputs.disName[6], MatcapInputs.tooltip[6], materialEditor, properties);
        MakeProperty(MatcapInputs.refName[7], MatcapInputs.disName[7], MatcapInputs.tooltip[7], materialEditor, properties);
        EditorGUILayout.Space(categorySpace);
        
        MakeHeader(EmissiveInputs.headerName);
        bool enableEmissive = FindProperty("_ENABLE_EMISSIVE", properties).floatValue == 0;
        if (enableEmissive)
        {
            MakeProperty(EmissiveInputs.refName[0], EmissiveInputs.disName[0], EmissiveInputs.tooltip[0], materialEditor, properties);
        }
        else
        {
            for (int i = 0; i < EmissiveInputs.Count; i++)
            {
                MakeProperty(EmissiveInputs.refName[i], EmissiveInputs.disName[i], EmissiveInputs.tooltip[i], materialEditor, properties);
            }
            
            bool enableAdvancedMask = FindProperty("_ENABLE_ADVANCED_MASK", properties).floatValue == 0;
            if (enableAdvancedMask)
            {
                 MakeProperty(AdvancedMaskInputs.refName[0], AdvancedMaskInputs.disName[0], AdvancedMaskInputs.tooltip[0], materialEditor, properties);  
            }
            else
            {
                MakeHeader(AdvancedMaskInputs.headerName);
                for(int i= 0; i < AdvancedMaskInputs.Count; i++)
                {
                    MakeProperty(AdvancedMaskInputs.refName[i], AdvancedMaskInputs.disName[i], AdvancedMaskInputs.tooltip[i], materialEditor, properties);
                }
            }
        }
        EditorGUILayout.Space(categorySpace);        
        
        MakeHeader(YPlanarInputs.headerName);
        var enableYPlanar = FindProperty("_ENABLE_Y_PLANAR", properties).floatValue == 0;
        if(enableYPlanar)
        {
            MakeProperty(YPlanarInputs.refName[0], YPlanarInputs.disName[0],
                YPlanarInputs.tooltip[0], materialEditor, properties);
        }
        else
        {
            for (int i = 0; i < YPlanarInputs.Count; i++)
            {
                MakeProperty(YPlanarInputs.refName[i], YPlanarInputs.disName[i], YPlanarInputs.tooltip[i], materialEditor, properties);
            }    
        }
        EditorGUILayout.Space(categorySpace);
        
        
        
        //advenced Option
        EditorGUILayout.LabelField("Advanced Opitons", headerStyle);
        EditorGUILayout.Space(headerSpace);
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
        
        if (targetMat.HasProperty("_Ref")) 
        {
            materialEditor.ShaderProperty(FindProperty("_Ref", properties), "Stencil Ref");
            materialEditor.PopupShaderProperty(FindProperty("_Pass", properties),stencilPassText, stencilPassNames);
            materialEditor.ShaderProperty(FindProperty("_WriteMask", properties), "Stencil WriteMask");    
        }
    }
}