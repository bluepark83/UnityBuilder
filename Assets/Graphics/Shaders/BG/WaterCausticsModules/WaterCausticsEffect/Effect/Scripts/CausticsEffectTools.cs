using UnityEngine;

namespace WaterCausticsModules
{
    public class CausticsEffectTools
    {
        // ----------------------------------------------------------- Tools
        public static float round2Dec5 (float val) => (float) (System.Math.Round (val * 20f) * 0.05);
        public static float wrapAngle180 (float angle) => Mathf.Repeat (angle + 180f, 360f) - 180f;
        public static  Vector2 dirToVec (float dir) => new Vector2 (Mathf.Sin (dir * Mathf.Deg2Rad), Mathf.Cos (-dir * Mathf.Deg2Rad));
        public static float vecToDir (Vector2 v) => round2Dec5 (wrapAngle180 (Mathf.Atan2 (v.x, v.y) * Mathf.Rad2Deg));

        public static void SetMatKeyword (Material mat, bool isEnable, string keyword)
        {
            if (isEnable)
                mat.EnableKeyword (keyword);
            else
                mat.DisableKeyword (keyword);
        }
        
        public static float CalcFadeCoef (float depth) => 1f / Mathf.Max (depth, 0.0000001f);
    }
}