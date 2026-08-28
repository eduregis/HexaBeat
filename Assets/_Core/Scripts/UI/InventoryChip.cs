using UnityEngine;
using UnityEngine.UI;

namespace HexaBit.Core {
    public class InventoryChip : MonoBehaviour {
        [Header("UI References")]
        public Image frameImage;
        public Image iconImage;

        public void SetChip(Sprite icon, Sprite frameSprite = null) {
            // Show the chip
            gameObject.SetActive(true);

            // Set frame sprite (if provided)
            if (frameImage != null && frameSprite != null)
                frameImage.sprite = frameSprite;
        }

        public void ClearChip() {
            // Hide the chip entirely
            gameObject.SetActive(false);

            // Clear references to avoid lingering data
            if (iconImage != null)
                iconImage.sprite = null;
        }
    }
}