using UnityEditor;
using UnityEngine;

public class RM2_Grass_GUI : CustomShaderGUI
{

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        int surfaceOptionCount = 5;
        int surfaceInputsCount = 6 + surfaceOptionCount;
        int InteractiveOptionsCount = 2 + surfaceInputsCount;
        int GlobalInputsCount = 6 + InteractiveOptionsCount;
        

        Reference SurfaceOptions = new Reference() { headerName = "Surface Options" };
        Reference SurfaceInputs = new Reference() { headerName = "Surface Inputs" };
        Reference InteractiveOptions = new Reference() { headerName = "Interactive Options" };
        Reference GlobalInputs = new Reference() { headerName = "Global Inputs" };

        url = "https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.go1cqocgo8wk";
        SetHelp(materialEditor, properties);
        materialEditor.SetDefaultGUIWidths();

        SurfaceOptions.SetReference(0, surfaceOptionCount,  properties, SurfaceOptions);
        SurfaceInputs.SetReference(surfaceOptionCount, surfaceInputsCount,  properties, SurfaceInputs);
        InteractiveOptions.SetReference(surfaceInputsCount, InteractiveOptionsCount,  properties, InteractiveOptions);
        GlobalInputs.SetReference(InteractiveOptionsCount, GlobalInputsCount,  properties, GlobalInputs);

        // Surface Options
        MakeHeader(SurfaceOptions.headerName);
        MakeProperty(SurfaceOptions.refName[0], SurfaceOptions.disName[0], "메쉬 UV가 사각형에 맞춰있지 않은 경우 On \n 예를 들어 위에 다른 메쉬가 있는 꽃 같은 경우", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[1], SurfaceOptions.disName[1], "오브젝트의 크기를 비율로 변경", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[2], SurfaceOptions.disName[2], "그림자 색상", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[3], SurfaceOptions.disName[3], "글로벌 일루미네이션 세기 \n 기본값 1, 최대 2배로 밝게 설정 가능", materialEditor, properties);
        MakeProperty(SurfaceOptions.refName[4], SurfaceOptions.disName[4], "알파 클리핑 범위", materialEditor, properties);
        EditorGUILayout.Space(categorySpace);

        //Surface Inputs
        MakeHeader(SurfaceInputs.headerName);
        MakeProperty(SurfaceInputs.refName[0], SurfaceInputs.disName[0], "기본 색상", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[1], SurfaceInputs.disName[1], materialEditor, properties, PropType.Texture);
        MakeProperty(SurfaceInputs.refName[2], SurfaceInputs.disName[2], "지면 색상", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[3], SurfaceInputs.disName[3], "지면 색상 영향 범위", materialEditor, properties);
        MakeProperty(SurfaceInputs.refName[4], SurfaceInputs.disName[4], "지면 색상 영향 범위 부드러운 정도", materialEditor, properties);
        //5 번 Tilling으로 BaseMap에 합쳐짐
        EditorGUILayout.Space(categorySpace);

        MakeHeader(InteractiveOptions.headerName);
        MakeProperty(InteractiveOptions.refName[0], InteractiveOptions.disName[0], "풀이 바람에 받는 영향도", materialEditor, properties);
        MakeProperty(InteractiveOptions.refName[1], InteractiveOptions.disName[1], "풀이 충돌에 받는 영향도", materialEditor, properties);
        EditorGUILayout.Space(categorySpace);

        MakeHeader(GlobalInputs.headerName);
        MakeProperty(GlobalInputs.refName[0], GlobalInputs.disName[0], "바람 방향\n X축과 Z축만 사용", materialEditor, properties);
        MakeProperty(GlobalInputs.refName[1], GlobalInputs.disName[1], "바람 세기", materialEditor, properties);
        MakeProperty(GlobalInputs.refName[2], GlobalInputs.disName[2], "바람 속도", materialEditor, properties);
        MakeProperty(GlobalInputs.refName[3], GlobalInputs.disName[3], "글로벌 색상", materialEditor, properties);
        MakeProperty(GlobalInputs.refName[4], GlobalInputs.disName[4], "글로벌 색상 패턴 크기", materialEditor, properties);
        MakeProperty(GlobalInputs.refName[5], GlobalInputs.disName[5], "글로벌 색상과 기본 색상 블렌딩 정도", materialEditor, properties);
        EditorGUILayout.Space(categorySpace);
        
        //advenced Option
        EditorGUILayout.LabelField("Advanced Opitons", headerStyle);
        EditorGUILayout.Space(headerSpace);
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        // materialEditor.DoubleSidedGIField();

    }


}