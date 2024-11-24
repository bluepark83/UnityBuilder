using UnityEditor;
using UnityEngine;

public class InteractiveGrass_GUI : CustomShaderGUI
{
    // SRP & GPU 동시 지원
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty hideGlobalInputs = FindProperty("_HideGlobalInputs", properties);
        bool isHideGlobal = hideGlobalInputs.floatValue == 0 ? true :false;
        int surfaceOptionCount = 8;
        int surfaceInputsCount = 5 + surfaceOptionCount;
        int GlobalInputsCount = 1 + surfaceInputsCount;
        if(isHideGlobal)GlobalInputsCount = 8 + surfaceInputsCount;
        
        Reference SurfaceOptions = new Reference() { headerName = "Surface Options" };
        Reference SurfaceInputs = new Reference() { headerName = "Surface Inputs" };
        Reference GlobalInputs = new Reference() { headerName = "Global Inputs" };

        url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.go1cqocgo8wk";
        SetHelp(materialEditor, properties);
        materialEditor.SetDefaultGUIWidths();

        SurfaceOptions.SetReference(0, surfaceOptionCount, properties, SurfaceOptions);
        SurfaceInputs.SetReference(surfaceOptionCount, surfaceInputsCount, properties, SurfaceInputs);
        GlobalInputs.SetReference(surfaceInputsCount, GlobalInputsCount, properties, GlobalInputs);

        // Surface Options
        MakeHeader(SurfaceOptions.headerName);
        MakeProperty(SurfaceOptions.refName[0], SurfaceOptions.disName[0], "상호작용에 사용할 UV 선택", materialEditor, properties, PropType.Int);
        MakeProperty(SurfaceOptions.refName[1], SurfaceOptions.disName[1], "오브젝트 넓이 최소값, 최대값", materialEditor, properties, PropType.Vector2);
        MakeProperty(SurfaceOptions.refName[2], SurfaceOptions.disName[2], "오브젝트 높이 최소값, 최대값", materialEditor, properties, PropType.Vector2);
        MakeProperty(SurfaceOptions.refName[3], SurfaceOptions.disName[3], "오브젝트 텐션, 높을 수록 끝 부분만 휘어짐", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[4], SurfaceOptions.disName[4], "글로벌 일루미네이션 세기 \n 기본값 1, 최대 2배로 밝게 설정 가능", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[5], SurfaceOptions.disName[5], "알파 클리핑 범위", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[6], SurfaceOptions.disName[6], "그림자 받을지 결정", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[7], SurfaceOptions.disName[7], "그림자 색상", materialEditor, properties);


        EditorGUILayout.Space(categorySpace);

        //Surface Inputs
        MakeHeader(SurfaceInputs.headerName);
        MakeProperty(SurfaceInputs.refName[0], SurfaceInputs.disName[0], materialEditor, properties, PropType.Texture);
        MakeProperty(SurfaceInputs.refName[1], SurfaceInputs.disName[1], "기본 색상", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[2], SurfaceInputs.disName[2], "지면 색상", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[3], SurfaceInputs.disName[3], "지면 색상 영향 범위", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[4], SurfaceInputs.disName[4], "지면 색상 영향 범위 부드러운 정도", materialEditor, properties);
        EditorGUILayout.Space(categorySpace);

        if (isHideGlobal)
        {
            MakeHeader(GlobalInputs.headerName);
            MakeProperty(GlobalInputs.refName[0], GlobalInputs.disName[0], "글로벌 색상", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[1], GlobalInputs.disName[1], "글로벌 색상 패턴 크기", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[2], GlobalInputs.disName[2], "글로벌 색상과 기본 색상 블렌딩 정도", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[3], GlobalInputs.disName[3], "바람 방향", materialEditor, properties, PropType.Vector2);
            MakeProperty(GlobalInputs.refName[4], GlobalInputs.disName[4], "바람 세기", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[5], GlobalInputs.disName[5], "바람 속도", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[6], GlobalInputs.disName[6], "바람 패턴 크기", materialEditor, properties);
            MakeProperty(GlobalInputs.refName[7], GlobalInputs.disName[7], "인터렉티브 On / Off", materialEditor, properties);
            EditorGUILayout.Space(categorySpace);    
        }
        else
        {
            MakeProperty(GlobalInputs.refName[0], GlobalInputs.disName[0], "인터렉티브 On / Off", materialEditor, properties);
            EditorGUILayout.Space(categorySpace);
        }
        
        
        //advenced Option
        EditorGUILayout.LabelField("Advanced Opitons", headerStyle);
        EditorGUILayout.Space(headerSpace);
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        // materialEditor.DoubleSidedGIField();

    }


}