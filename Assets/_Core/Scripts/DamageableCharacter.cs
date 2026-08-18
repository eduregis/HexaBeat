using UnityEngine;
using System.Collections;

public abstract class DamageableCharacter : MonoBehaviour {
    [Header("Health & Armor")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int armor = 0;

    [Header("Damage Feedback")]
    [SerializeField] protected GameObject damagePopupPrefab;

    protected int currentHealth;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Coroutine flashCoroutine;

    public System.Action OnHealthChanged;

    protected virtual void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Método para inicializar a vida e armadura a partir dos dados do SO
    protected void SetHealth(int health, int armorValue) {
        maxHealth = health;
        armor = armorValue;
        currentHealth = maxHealth;
    }

    // Public method to apply damage
    public virtual void TakeDamage(float damage) {
        int finalDamage = (int)Mathf.Max(1, damage - armor);
        currentHealth -= finalDamage;

        // Visual feedback (flash red)
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRed());

        // Show damage popup if prefab is assigned
        if (damagePopupPrefab != null) {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform.parent);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
                popupScript.SetDamage(finalDamage);
        }

        Debug.Log($"{gameObject.name} took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    protected virtual IEnumerator FlashRed() {
        if (spriteRenderer != null) {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }

    // Abstract method to be implemented by derived classes
    protected abstract void Die();
}