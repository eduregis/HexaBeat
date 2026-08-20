using UnityEngine;

namespace HexaBit.Core {

    [CreateAssetMenu(fileName = "[buff] New Buff", menuName = "HexaBeat/Buff Data")]
    public class BuffData : ScriptableObject {
        public string buffName;
        [TextArea] public string description;
        public Sprite icon;
        public AbilityType type;

        public float[] values = new float[6];

        public float GetValue(int level) {
            if (level <= 0) return 0f;
            int index = Mathf.Clamp(level - 1, 0, values.Length - 1);
            return values[index];
        }

        public int MaxLevel => values.Length;
    }

}