using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HexaBit.Core {
    public class HUDButton : MonoBehaviour {
        [Header("UI References")]
        public Image backgroundImage;
        public Image iconImage;
        public Image selectionGlow;
        public TextMeshProUGUI labelText;

        public Button button;

        /// <summary>
        /// Enables or disables the selection glow to indicate focus.
        /// </summary>
        public void SetSelected(bool isSelected) {
            if (selectionGlow != null) {
                selectionGlow.gameObject.SetActive(isSelected);
            }
        }
    }
}