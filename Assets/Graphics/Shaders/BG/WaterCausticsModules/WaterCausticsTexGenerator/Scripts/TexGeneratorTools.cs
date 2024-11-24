using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterCausticsModules
{
    public class TexGeneratorTools
    {
        public static float Round2Dec(float val) => (float)(System.Math.Round(val * 100f) * 0.01);
        public static float Round2Dec5(float val) => (float)(System.Math.Round(val * 20f) * 0.05);
        public static float WrapAngle180(float angle) => Mathf.Repeat(angle + 180f, 360f) - 180f;

        public static Vector2 DirToVec(float dir) =>
            new Vector2(Mathf.Sin(dir * Mathf.Deg2Rad), Mathf.Cos(-dir * Mathf.Deg2Rad));

        public static float VecToDir(Vector2 v) => Round2Dec5(WrapAngle180(Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg));
    }
}