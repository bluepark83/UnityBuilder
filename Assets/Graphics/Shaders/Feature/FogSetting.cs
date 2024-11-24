using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "RM2 FogSetting", menuName = "ScriptableObjects/Create FogSetting Data")]
[HelpURL("https://docs.google.com/document/d/1hH0Pu2U617zoR3HQAaXG97jRjfcca3WIpcrGICBSYh4/edit#heading=h.lduwzk94p3ru")]
public class FogSetting : ScriptableObject
{
    
    [Header("RM2 Fog")]
    public bool enableDistance = true;
    public Gradient distanceGradient;
    public float fogNear = 0;
    public float fogFar = 100;
    [Range(0, 1)] public float fogIntensity = 1.0f;
    public bool enableOnSky = true;

    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public bool applyInSceneView = true;

}
