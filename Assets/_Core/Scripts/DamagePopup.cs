using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float expandDuration = 0.2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private float timer;
    private Vector3 initialScale;

    private void Awake() {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        initialScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void OnEnable() {
        timer = 0f;
        transform.localScale = Vector3.zero;
    }

    public void SetDamage(int damage) {
        if (textMesh != null) textMesh.text = damage.ToString();
    }

    private void Update() {
        timer += Time.deltaTime;

        // Expande no começo
        if (timer < expandDuration) {
            float progress = timer / expandDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, progress);
        } else {
            // Sobe e faz fade
            float elapsed = timer - expandDuration;
            if (elapsed < fadeDuration) {
                float fadeProgress = elapsed / fadeDuration;
                transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);
                Color color = textMesh.color;
                color.a = Mathf.Lerp(1f, 0f, fadeProgress);
                textMesh.color = color;
            } else {
                Destroy(gameObject);
            }
        }
    }
}