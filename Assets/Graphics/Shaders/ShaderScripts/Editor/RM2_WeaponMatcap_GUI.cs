using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class RM2_WeaponMatcap_GUI : CustomShaderGUI
{
    // SurfaceOptions은 Shader Properties 항목 위에서부터 밑으로 쭉 긁어옴
    // SurfaceOptions.Name 은 이름
    // SurfaceOptions.Tooltip 은 이름으로 되어있음
    
    public static readonly string[] stencilPassNames = Enum.GetNames(typeof(StencilOp));
    public static readonly GUIContent stencilPassText = EditorGUIUtility.TrTextContent("Stencil Pass",
        "");
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        int surfaceOptionCount = 4;
        int surfaceInputsCount = 5 + surfaceOptionCount;
        int normalMaskInputsCount = 6 + surfaceInputsCount;
        int matcapInputsCount = 7 + normalMaskInputsCount;
        int dissolveInputsCount = 4 + matcapInputsCount;
        int fresnelEffectInputCount = 2 + dissolveInputsCount;
        
        Material targetMat = materialEditor.target as Material;


        Reference SurfaceOptions = new Reference() { headerName = "Surface Options" };
        Reference SurfaceInputs = new Reference() { headerName = "Surface Inputs" };
        Reference NormalMaskInputs = new Reference() { headerName = "NormalMask Inputs" };
        Reference MatcapInputs = new Reference() { headerName = "Matcap Inputs" };
        Reference DissolveInputs = new Reference() { headerName = "Dissolve Inputs" };
        Reference FresnelInputs = new Reference() { headerName = "Fresnel Inputs" };

        url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.1iwjol5qlkne";
        SetHelp(materialEditor, properties);
        materialEditor.SetDefaultGUIWidths();

        SurfaceOptions.SetReference(0, surfaceOptionCount,  properties, SurfaceOptions);
        SurfaceInputs.SetReference(surfaceOptionCount, surfaceInputsCount,  properties, SurfaceInputs);
        NormalMaskInputs.SetReference(surfaceInputsCount, normalMaskInputsCount,  properties, NormalMaskInputs);
        MatcapInputs.SetReference(normalMaskInputsCount, matcapInputsCount,  properties, MatcapInputs);
        DissolveInputs.SetReference(matcapInputsCount, dissolveInputsCount,  properties, DissolveInputs);
        FresnelInputs.SetReference(dissolveInputsCount,fresnelEffectInputCount,properties,FresnelInputs);


        // Surface Options
        MakeHeader(SurfaceOptions.headerName);
        CullModeField(materialEditor, properties);
        this.renderQueue = 2125;
        EditorGUILayout.Space(categorySpace - 15);

        MakeProperty(SurfaceOptions.refName[0], SurfaceOptions.disName[0], "그림자 범위\n 기본값 0.7", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[1], SurfaceOptions.disName[1], "그림자 부드러운 정도\n 기본값 0.2", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[2], SurfaceOptions.disName[2], "그림자 색상", materialEditor, properties);
        EditorGUILayout.Space(categorySpace - 15);

        MakeProperty(SurfaceOptions.refName[3], SurfaceOptions.disName[3], "글로벌 일루미네이션 세기 \n 기본값 0.15", materialEditor, properties);
        EditorGUILayout.Space(categorySpace - 15);

        AlphaClipField(materialEditor, properties, true);
        // MakeProperty(SurfaceOptions.refName[4], SurfaceOptions.disName[4], "알파 클리핑 범위", materialEditor, properties);
        
        EditorGUILayout.Space(categorySpace);


        //Surface Inputs
        MakeHeader(SurfaceInputs.headerName);

        MakeProperty(SurfaceInputs.refName[0], SurfaceInputs.disName[0], "기본 색상", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[1], SurfaceInputs.disName[1], "색상 채도 조절", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[2], SurfaceInputs.disName[2], materialEditor, properties, PropType.Texture);
        MakeProperty(SurfaceInputs.refName[3], SurfaceInputs.disName[3], "Specular 색상", materialEditor, properties);
        //3 번 Tilling으로 BaseMap에 합쳐짐
        EditorGUILayout.Space(categorySpace);

        //NormalMask Inputs
        MakeHeader(NormalMaskInputs.headerName);

        MakeProperty(NormalMaskInputs.refName[0], NormalMaskInputs.disName[0], "Mask Texture ON / OFF\n R은 기존의 Metallic 을 사용하고, G에는 기존 Normal의 G채널, B에는 Smoothness, A에는 기존 Normal의 R 채널을 입력합니다.", materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[1], NormalMaskInputs.disName[1], "Normal Map 세기", materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[2], NormalMaskInputs.disName[2], materialEditor, properties, PropType.TextureNoOption);
        MakeProperty(NormalMaskInputs.refName[3], NormalMaskInputs.disName[3], "Specular, Smoothness 조절", materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[4], NormalMaskInputs.disName[4], "Specular 범위 조정", materialEditor, properties);
        MakeProperty(NormalMaskInputs.refName[5], NormalMaskInputs.disName[5], "Specular 세기 조정", materialEditor, properties);
        EditorGUILayout.Space(categorySpace);

        //Matcap Inputs
        MakeHeader(MatcapInputs.headerName);

        MakeProperty(MatcapInputs.refName[0], MatcapInputs.disName[0], "Matcap 을 재질에 어떻게 섞을지 결정./n On인 경우 어두운 부분에도 적용/n Off인 경우 기존 Metallic과 비슷하게 블랜딩/n Off가 기본 값이며 보통의 반사가 있는 Metal 재질은 Off 상태를 추천", materialEditor, properties);
        MakeProperty(MatcapInputs.refName[1], MatcapInputs.disName[1], materialEditor, properties, PropType.TextureNoOption);
        MakeProperty(MatcapInputs.refName[2], MatcapInputs.disName[2], "Matcap 세기", materialEditor, properties);
        MakeProperty(MatcapInputs.refName[3], MatcapInputs.disName[3], "Matcap 색상", materialEditor, properties);
        MakeProperty(MatcapInputs.refName[4], MatcapInputs.disName[4], "Matcap 밝기", materialEditor, properties);
        MakeProperty(MatcapInputs.refName[5], MatcapInputs.disName[5], "Matcap 밉맵 디테일", materialEditor, properties);
        MakeProperty(MatcapInputs.refName[6], MatcapInputs.disName[6], "Matcap 회전", materialEditor, properties);

        EditorGUILayout.Space(categorySpace);

        //Dissolve Inputs
        MakeHeader(DissolveInputs.headerName);
        // MakeProperty(DissolveInputs.refName[0], DissolveInputs.disName[0], materialEditor, properties, PropType.Texture);
        MakeProperty(DissolveInputs.refName[0], DissolveInputs.disName[0], "디졸브 색상", materialEditor, properties);
        MakeProperty(DissolveInputs.refName[1], DissolveInputs.disName[1], "디졸브 범위 1일 때 사라짐",materialEditor, properties);
        MakeProperty(DissolveInputs.refName[2], DissolveInputs.disName[2], "디졸브 노이즈 크기", materialEditor, properties);
        MakeProperty(DissolveInputs.refName[3], DissolveInputs.disName[3], "디졸브 엣지 크기", materialEditor, properties);
        
        EditorGUILayout.Space(categorySpace);
        
        MakeHeader(FresnelInputs.headerName);
        MakeProperty(FresnelInputs.refName[0], FresnelInputs.disName[0], "프레넬 색상",materialEditor,properties);
        MakeProperty(FresnelInputs.refName[1], FresnelInputs.disName[1], "프레넬 강도",materialEditor,properties);
        
        //advenced Option
        EditorGUILayout.LabelField("Advanced Opitons", headerStyle);
        EditorGUILayout.Space(headerSpace);
        materialEditor.RenderQueueField();
        // materialEditor.EnableInstancingField();
        // materialEditor.DoubleSidedGIField();
        
        if (targetMat.HasProperty("_Ref")) 
        {
            targetMat.SetFloat("_Ref", 1);
            targetMat.SetFloat("_Pass", 2);
            targetMat.SetFloat("_WriteMask", 255);
            materialEditor.ShaderProperty(FindProperty("_Ref", properties), "Stencil Ref");
            materialEditor.PopupShaderProperty(FindProperty("_Pass", properties),stencilPassText, stencilPassNames);
            materialEditor.ShaderProperty(FindProperty("_WriteMask", properties), "Stencil WriteMask");    
        }

    }


}