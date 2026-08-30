using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HexaBit.Core {
    public class LevelUpButton : MonoBehaviour {
        [Header("UI References")]
        public Image frameImage;
        public Image iconImage;
        public Image backgroundImage;
        public Image selectionGlow;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI levelText;

        public Button button;

        public void SetSelected(bool isSelected) {
            if (selectionGlow != null) {
                selectionGlow.gameObject.SetActive(isSelected);
            }
        }
    }
}