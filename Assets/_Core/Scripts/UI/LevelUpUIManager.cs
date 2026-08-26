using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class LevelUpUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private LevelUpButton[] choiceButtons;

        [Header("Visual Configuration")]
        [SerializeField] private Sprite[] levelFrames;
        [SerializeField] private Sprite defaultIcon;

        private System.Action<int> _onChoiceMade;
        private int _currentSelectedIndex = -1;

        public void OpenWithOptions(List<GameplayManager.LevelUpOption> options, System.Action<int> onChoiceMade) {
            _onChoiceMade = onChoiceMade;

            Time.timeScale = 0f;
            levelUpPanel.SetActive(true);

            for (int i = 0; i < choiceButtons.Length && i < options.Count; i++) {
                LevelUpButton btn = choiceButtons[i];
                GameplayManager.LevelUpOption opt = options[i];

                if (btn.frameImage != null && levelFrames != null && levelFrames.Length > 0) {
                    int frameIndex = Mathf.Clamp(opt.targetLevel - 1, 0, levelFrames.Length - 1);
                    btn.frameImage.sprite = levelFrames[frameIndex];
                }

                if (btn.iconImage != null)
                    btn.iconImage.sprite = opt.icon != null ? opt.icon : defaultIcon;

                if (btn.nameText != null)
                    btn.nameText.text = opt.displayName;

                if (btn.descriptionText != null)
                    btn.descriptionText.text = opt.description;

                if (btn.levelText != null) {
                    if (opt.targetLevel == 1)
                        btn.levelText.text = "NEW";
                    else if (opt.targetLevel >= 6)
                        btn.levelText.text = "MAX";
                    else
                        btn.levelText.text = $"Lv.{opt.targetLevel}";
                }

                if (btn.button != null) {
                    btn.button.onClick.RemoveAllListeners();
                    int capturedIndex = i;
                    btn.button.onClick.AddListener(() => {
                        _onChoiceMade?.Invoke(capturedIndex);
                        CloseLevelUpMenu();
                    });
                }

                btn.SetSelected(false);
                btn.gameObject.SetActive(true);
            }

            for (int i = options.Count; i < choiceButtons.Length; i++) {
                if (choiceButtons[i] != null)
                    choiceButtons[i].gameObject.SetActive(false);
            }

            // Setup selection event listeners (to detect focus changes via navigation)
            SetupButtonSelectionListeners();

            // Select first button by default
            if (choiceButtons.Length > 0 && choiceButtons[0] != null && choiceButtons[0].button != null) {
                SetSelectedButton(0);
                EventSystem.current.SetSelectedGameObject(choiceButtons[0].button.gameObject);
            }
        }

        private void SetupButtonSelectionListeners() {
            for (int i = 0; i < choiceButtons.Length; i++) {
                if (choiceButtons[i] == null || choiceButtons[i].button == null) continue;

                // Get or create EventTrigger
                EventTrigger trigger = choiceButtons[i].button.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = choiceButtons[i].button.gameObject.AddComponent<EventTrigger>();

                // Clear existing entries
                trigger.triggers.Clear();

                // Add OnSelect event
                EventTrigger.Entry selectEntry = new EventTrigger.Entry();
                selectEntry.eventID = EventTriggerType.Select;
                int capturedIndex = i;
                selectEntry.callback.AddListener((data) => {
                    SetSelectedButton(capturedIndex);
                    Debug.Log($"Focus changed to button {capturedIndex}: {choiceButtons[capturedIndex]?.name}");
                });
                trigger.triggers.Add(selectEntry);
            }
        }

        private void SetSelectedButton(int index) {
            if (_currentSelectedIndex == index) return;

            // Deselect previous
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < choiceButtons.Length) {
                if (choiceButtons[_currentSelectedIndex] != null) {
                    choiceButtons[_currentSelectedIndex].SetSelected(false);
                    Debug.Log($"Button {_currentSelectedIndex} deselected");
                }
            }

            _currentSelectedIndex = index;

            // Select new
            if (index >= 0 && index < choiceButtons.Length && choiceButtons[index] != null) {
                choiceButtons[index].SetSelected(true);
                Debug.Log($"Button {index} selected, glow active: {choiceButtons[index].selectionGlow?.gameObject.activeSelf}");
            }
        }

        public void CloseLevelUpMenu() {
            Time.timeScale = 1f;
            Destroy(gameObject);
        }

        private void OnDestroy() {
            Time.timeScale = 1f;
        }
    }
}