using UnityEngine;
using System.Collections;

namespace HexaBit.Core {
    public abstract partial class DamageableCharacter : MonoBehaviour {
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
            if (spriteRenderer != null) {
                originalColor = spriteRenderer.color;
            }
        }

        protected void SetHealth(int health, int armorValue) {
            maxHealth = health;
            armor = armorValue;
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float damage) {
            float effectiveDamage = damage * DamageMultiplier;
            int finalDamage = (int)Mathf.Max(1, effectiveDamage - armor);
            currentHealth -= finalDamage;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRed());

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
                Color currentColor = spriteRenderer.color;
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = currentColor;
                AnimateTakingDamage(false);
            }
            flashCoroutine = null;
        }

        public virtual void Heal(float amount) {
            currentHealth = Mathf.Min(currentHealth + Mathf.RoundToInt(amount), maxHealth);
            OnHealthChanged?.Invoke();
        }

        protected abstract void AnimateTakingDamage(bool damaged);
        protected abstract void Die();
    }
}