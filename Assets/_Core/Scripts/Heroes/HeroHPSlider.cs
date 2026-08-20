using UnityEngine;
using UnityEngine.UI;

namespace HexaBit.Core {
    public class HeroHPSlider : Slider {
        [Header("HP Colors")]
        [SerializeField] private Color colorGreen = Color.green;
        [SerializeField] private Color colorYellow = Color.yellow;
        [SerializeField] private Color colorRed = Color.red;

        protected override void OnEnable() {
            base.OnEnable();
            // Add listener to auto-update color whenever the slider value changes
            onValueChanged.AddListener(UpdateColor);

            // Force the initial color update based on the starting HP value
            UpdateColor(value);
        }

        protected override void OnDisable() {
            base.OnDisable();
            onValueChanged.RemoveListener(UpdateColor);
        }

        private void UpdateColor(float _) {
            // Use normalizedValue (0 to 1) to get the exact percentage
            float percent = normalizedValue;

            Color newColor;

            if (percent >= 0.5f) {
                // Interpolate from Yellow (50%) to Green (100%)
                float t = (percent - 0.5f) / 0.5f; // Maps 0.5 to 0, and 1.0 to 1
                newColor = Color.Lerp(colorYellow, colorGreen, t);
            } else if (percent >= 0.2f) {
                // Interpolate from Red (20%) to Yellow (50%)
                float t = (percent - 0.2f) / 0.3f; // Maps 0.2 to 0, and 0.5 to 1
                newColor = Color.Lerp(colorRed, colorYellow, t);
            } else {
                // Below 20%, keep it pure Red (as requested)
                newColor = colorRed;
            }

            // Apply the calculated color to the Slider's Fill Image
            if (fillRect != null) {
                Image fillImage = fillRect.GetComponent<Image>();
                if (fillImage != null) {
                    fillImage.color = newColor;
                }
            }
        }
    }
}