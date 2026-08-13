using System;

public enum DynamicParameter {
    Damage,
    Cooldown,
    Projectiles,
    Speed,
    Range
}

public static class DynamicParameterExtensions {
    // Convert the DynamicParameter enum to the corresponding string
    public static string ToFieldName(this DynamicParameter parameter) {
        return parameter.ToString();
    }
}