using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {

    [CreateAssetMenu(fileName = "[weapon] New Weapon", menuName = "HexaBeat/Weapon Data")]
    public class WeaponData : ScriptableObject {
        [Header("General Info")]
        public string weaponName;
        [TextArea(3, 10)] public string description;
        public Sprite icon;
        public GameObject attackPrefab;
        public GameObject weaponPrefab;

        [Header("Dynamic Fields (Optionals)")]
        public List<DynamicFieldDefinition> customFields = new List<DynamicFieldDefinition>();

        [Header("Levels (Lv.1 ao Lv.6)")]
        public List<WeaponLevelData> levels = new List<WeaponLevelData>();

        // Access methods to facilitate code readability
        public float GetDamage(int levelIndex) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].damage;
        }

        public float GetCooldown(int levelIndex) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].cooldown;
        }

        public int GetInt(int levelIndex, DynamicParameter param) {
            return GetInt(levelIndex, param.ToFieldName());
        }

        public float GetFloat(int levelIndex, DynamicParameter param) {
            return GetFloat(levelIndex, param.ToFieldName());
        }

        public bool GetBool(int levelIndex, DynamicParameter param) {
            return GetBool(levelIndex, param.ToFieldName());
        }

        public int GetInt(int levelIndex, string fieldName) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0;
            return levels[levelIndex].GetInt(fieldName);
        }

        public float GetFloat(int levelIndex, string fieldName) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].GetFloat(fieldName);
        }

        public bool GetBool(int levelIndex, string fieldName) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return false;
            return levels[levelIndex].GetBool(fieldName);
        }
    }

    [System.Serializable]
    public class DynamicFieldDefinition {
        public string fieldName;
        public DynamicFieldType fieldType;
    }

    [System.Serializable]
    public class WeaponLevelData {
        // Mandatory fixed fields
        public float damage;
        public float cooldown;

        // Values for dynamic fields (optional)
        public List<DynamicFieldValue> customValues = new List<DynamicFieldValue>();

        public int GetInt(string fieldName) {
            foreach (var v in customValues)
                if (v.fieldName == fieldName && v.fieldType == DynamicFieldType.Int)
                    return v.intValue;
            return 0;
        }

        public float GetFloat(string fieldName) {
            foreach (var v in customValues)
                if (v.fieldName == fieldName && v.fieldType == DynamicFieldType.Float)
                    return v.floatValue;
            return 0f;
        }

        public bool GetBool(string fieldName) {
            foreach (var v in customValues)
                if (v.fieldName == fieldName && v.fieldType == DynamicFieldType.Bool)
                    return v.boolValue;
            return false;
        }
    }

    [System.Serializable]
    public class DynamicFieldValue {
        public string fieldName;
        public DynamicFieldType fieldType;
        public int intValue;
        public float floatValue;
        public bool boolValue;
    }

}