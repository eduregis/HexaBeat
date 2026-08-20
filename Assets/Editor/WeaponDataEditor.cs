#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HexaBit.Core;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor {
    private WeaponData data;
    private bool showLevels = true;

    private void OnEnable() {
        data = (WeaponData)target;
        ValidateAndSyncLevels();
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // ---- SECTION: General Info ----
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
        data.weaponName = EditorGUILayout.TextField("Weapon Name", data.weaponName);

        // ADDED: Description Field
        EditorGUILayout.LabelField("Description");
        data.description = EditorGUILayout.TextArea(data.description, GUILayout.Height(60));

        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);
        data.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Attack Prefab", data.attackPrefab, typeof(GameObject), false);
        data.weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", data.weaponPrefab, typeof(GameObject), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Dynamic Fields (Optionals) ----
        EditorGUILayout.LabelField("Dynamic Fields (Optionals)", EditorStyles.boldLabel);
        DrawCustomFields();

        EditorGUILayout.Space(10);

        // ---- SECTION: Levels (Table) ----
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
        showLevels = EditorGUILayout.Foldout(showLevels, $"Levels ({data.levels.Count})");
        if (showLevels) {
            DrawLevelsTable();
        }

        // Button to add level
        EditorGUILayout.Space(5);
        if (GUILayout.Button("+ Add New Level (Lv." + (data.levels.Count + 1) + ")")) {
            AddNewLevel();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------
    // 1. RENDERS THE LIST OF DYNAMIC FIELDS
    // ------------------------------------------------------------
    private void DrawCustomFields() {
        for (int i = 0; i < data.customFields.Count; i++) {
            EditorGUILayout.BeginHorizontal();

            // Field name
            data.customFields[i].fieldName = EditorGUILayout.TextField(data.customFields[i].fieldName, GUILayout.Width(150));

            // Field type (dropdown)
            data.customFields[i].fieldType = (DynamicFieldType)EditorGUILayout.EnumPopup(data.customFields[i].fieldType, GUILayout.Width(80));

            // REMOVE button
            if (GUILayout.Button("X", GUILayout.Width(25))) {
                RemoveCustomField(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        // Button ADD FIELD
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Field", GUILayout.Width(120))) {
            AddCustomField();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void AddCustomField() {
        data.customFields.Add(new DynamicFieldDefinition { fieldName = "New Field", fieldType = DynamicFieldType.Float });
        ValidateAndSyncLevels();
        EditorUtility.SetDirty(data);
    }

    private void RemoveCustomField(int index) {
        string fieldName = data.customFields[index].fieldName;

        data.customFields.RemoveAt(index);

        // Removes the corresponding value from ALL levels
        foreach (var level in data.levels) {
            level.customValues.RemoveAll(v => v.fieldName == fieldName);
        }

        ValidateAndSyncLevels();
        EditorUtility.SetDirty(data);
    }

    // ------------------------------------------------------------
    // 2. DRAW THE LEVELS TABLE
    // ------------------------------------------------------------
    private void DrawLevelsTable() {
        if (data.levels.Count == 0) {
            EditorGUILayout.HelpBox("No levels created. Click 'Add New Level'.", MessageType.Info);
            return;
        }

        // --- HEADER ---
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Level index column
        GUILayout.Label("Level", EditorStyles.boldLabel, GUILayout.Width(50));

        // Fixed fields
        GUILayout.Label("Damage", EditorStyles.boldLabel, GUILayout.Width(60));
        GUILayout.Label("Cooldown", EditorStyles.boldLabel, GUILayout.Width(60));

        // Columns for dynamic fields
        foreach (var def in data.customFields) {
            string label = $"{def.fieldName} ({def.fieldType})";
            GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.MinWidth(60));
        }

        // Remove level button column
        GUILayout.Label("", GUILayout.Width(25));

        EditorGUILayout.EndHorizontal();

        // --- TABLE ROWS (EACH LEVEL) ---
        for (int i = 0; i < data.levels.Count; i++) {
            var level = data.levels[i];
            EditorGUILayout.BeginHorizontal();

            // Table of Contents (Lv. X) - non-editable
            GUILayout.Label($"Lv. {i + 1}", GUILayout.Width(50));

            // Fixed fields
            level.damage = EditorGUILayout.FloatField(level.damage, GUILayout.Width(60));
            level.cooldown = EditorGUILayout.FloatField(level.cooldown, GUILayout.Width(60));

            // Dynamic fields
            foreach (var def in data.customFields) {
                // Find the corresponding value at this level
                var value = level.customValues.Find(v => v.fieldName == def.fieldName);
                if (value == null) {
                    // If it doesn't exist, create a default value
                    value = new DynamicFieldValue { fieldName = def.fieldName, fieldType = def.fieldType };
                    level.customValues.Add(value);
                }

                // Draws the field according to the type
                switch (def.fieldType) {
                    case DynamicFieldType.Int:
                        value.intValue = EditorGUILayout.IntField(value.intValue, GUILayout.MinWidth(50));
                        break;
                    case DynamicFieldType.Float:
                        value.floatValue = EditorGUILayout.FloatField(value.floatValue, GUILayout.MinWidth(50));
                        break;
                    case DynamicFieldType.Bool:
                        value.boolValue = EditorGUILayout.Toggle(value.boolValue, GUILayout.MinWidth(40));
                        break;
                }
            }

            // Button to REMOVE this level
            if (GUILayout.Button("X", GUILayout.Width(25))) {
                RemoveLevel(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    // ------------------------------------------------------------
    // 3. AUXILIARY FUNCTIONS (SYNC AND CREATION)
    // ------------------------------------------------------------
    private void ValidateAndSyncLevels() {
        // For each level, ensure that it has a value for every field definition
        foreach (var level in data.levels) {
            // Remove undefined values
            level.customValues.RemoveAll(v => !data.customFields.Exists(d => d.fieldName == v.fieldName));

            // Add missing values
            foreach (var def in data.customFields) {
                if (!level.customValues.Exists(v => v.fieldName == def.fieldName)) {
                    level.customValues.Add(new DynamicFieldValue {
                        fieldName = def.fieldName,
                        fieldType = def.fieldType
                    });
                }
            }
        }

        EditorUtility.SetDirty(data);
    }

    private void AddNewLevel() {
        // Create a new level with default values
        WeaponLevelData newLevel = new WeaponLevelData();
        newLevel.damage = 0f;
        newLevel.cooldown = 0f;

        // Add values for each dynamic field defined
        foreach (var def in data.customFields) {
            newLevel.customValues.Add(new DynamicFieldValue {
                fieldName = def.fieldName,
                fieldType = def.fieldType
            });
        }

        data.levels.Add(newLevel);
        EditorUtility.SetDirty(data);
    }

    private void RemoveLevel(int index) {
        data.levels.RemoveAt(index);
        EditorUtility.SetDirty(data);
    }
}
#endif