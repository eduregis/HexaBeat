using UnityEngine;

public class ActiveBuff {
    public BuffData data;
    public int currentLevel;

    public ActiveBuff(BuffData data, int level = 1) {
        this.data = data;
        this.currentLevel = level;
    }

    public void LevelUp() {
        if (currentLevel < data.MaxLevel) currentLevel++;
    }
}
