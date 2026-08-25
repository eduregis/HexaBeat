using UnityEngine;

namespace HexaBit.Core {
    // Active instance of a status applied to a character
    public class StatusEffect {
        public StatusData data;
        public float remainingDuration;
        public float timer; // For tick effects
        public GameObject visualEffectInstance;

        // Creates a new active status from the given data
        public StatusEffect(StatusData statusData) {
            data = statusData;
            remainingDuration = statusData.duration;
            timer = 0f;
            visualEffectInstance = null;
        }

        // Returns true if the status has expired and should be removed
        public bool IsExpired => data.duration > 0 && remainingDuration <= 0f;

        // Updates the remaining duration and tick timer
        public void Update(float deltaTime) {
            if (data.duration > 0)
                remainingDuration -= deltaTime;
            timer += deltaTime;
        }

        // Resets the tick timer (used after each tick)
        public void ResetTickTimer() => timer = 0f;
    }
}