using UnityEditor;
using UnityEngine;

public class RM2_Eye_GUI : CustomShaderGUI
{
    // SurfaceOptions은 Shader Properties 항목 위에서부터 밑으로 쭉 긁어옴
    // SurfaceOptions.Name 은 이름
    // SurfaceOptions.Tooltip 은 이름으로 되어있음

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        int optionCount = 15;
        url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.t61mmn78std6";
        
        Reference Options = new Reference() { headerName = "Options" };

        Material t = materialEditor.target as Material;
        if(t.shader.name == "RM2/Character/RM2_Lobby_Eye"){
            optionCount = 18;
            url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.mpa4gzjq848b";
        };

        SetHelp(materialEditor, properties);
        materialEditor.SetDefaultGUIWidths();

        Options.SetReference(0, optionCount, properties, Options);


        // Surface Options
        MakeHeader(Options.headerName);

        for (int i = 0; i < optionCount; i++)
        {
            MakeProperty(Options.refName[i], Options.disName[i], materialEditor, properties);
        }

        EditorGUILayout.Space(categorySpace);

        //advenced Option
        EditorGUILayout.LabelField("Advanced Opitons", headerStyle);
        EditorGUILayout.Space(headerSpace);
        materialEditor.RenderQueueField();
        // materialEditor.EnableInstancingField();
        // materialEditor.DoubleSidedGIField();

    }


}