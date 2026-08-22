using System;

namespace HexaBit.Core {

    public enum DynamicParameter {
        Damage,
        Cooldown,
        Projectiles,
        Speed,
        Range,
        Angle,
        Size,
        Knockback,
        ExtraTargets,
        SplashArea
    }

    public static class DynamicParameterExtensions {
        // Convert the DynamicParameter enum to the corresponding string
        public static string ToFieldName(this DynamicParameter parameter) {
            return parameter.ToString();
        }
    }

}