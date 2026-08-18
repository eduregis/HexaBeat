using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelUpUIManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;   // The panel itself
    [SerializeField] private Button[] choiceButtons;    // The 3 buttons

    private void Start() {
        // 1. Pause the game
        Time.timeScale = 0f;

        // 2. Activate the panel
        levelUpPanel.SetActive(true);

        // 3. Set initial focus to the first button
        if (choiceButtons.Length > 0 && choiceButtons[0] != null) {
            EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
        }

        // 4. Automatically bind all button clicks to a single handler
        for (int i = 0; i < choiceButtons.Length; i++) {
            int buttonIndex = i; // Capture the current index for the closure
            choiceButtons[i].onClick.RemoveAllListeners(); // Clean previous listeners (safety)
            choiceButtons[i].onClick.AddListener(() => OnOptionSelected(buttonIndex));
        }
    }

    // Called when any of the 3 buttons are clicked
    private void OnOptionSelected(int index) {
        Debug.Log($"Option {index + 1} was selected!");
        CloseLevelUpMenu();
    }

    // Unpauses and destroys the menu
    public void CloseLevelUpMenu() {
        Time.timeScale = 1f;
        Destroy(gameObject);
    }

    // Safety net if destroyed externally
    private void OnDestroy() {
        Time.timeScale = 1f;
    }
}