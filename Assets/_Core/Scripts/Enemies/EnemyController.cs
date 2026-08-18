using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour {
    [Header("Data")]
    [SerializeField] private EnemyData data;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool hasDeathAnimation = true;

    private int currentHealth;
    private float currentSpeed;
    private bool isDead = false;

    private Transform player;
    private Rigidbody2D rb;

    [Header("Damage Feedback")]
    [SerializeField] private GameObject damagePopupPrefab;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.5f; // Time between contact damage
    private float attackTimer = 0f;

    [Header("Events")]
    public UnityEvent OnDeath;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Initialize(EnemyData data, int wave) {
        this.data = data;
        currentHealth = data.baseHealth;
        currentSpeed = data.moveSpeed;

        // Scaling based on wave
        if (data.healthScalesWithPlayerLevel) {
            // Optional: use player level
        } else {
            currentHealth += Mathf.RoundToInt(data.baseHealth * 0.1f * wave);
            currentSpeed += data.moveSpeed * 0.05f * wave;
        }

        if (data.isBoss) {
            currentHealth *= 3;
            currentSpeed *= 1.2f;
        }

        isDead = false;
        attackTimer = 0f;
        if (animator != null) {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Respawn");
        }
    }

    public void SetPlayerReference(Transform playerTransform) {
        player = playerTransform;
    }

    private void OnEnable() {
        // Reset for pooling
        currentHealth = data.baseHealth;
        currentSpeed = data.moveSpeed;
        isDead = false;
        attackTimer = 0f;
        if (animator != null) {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Respawn");
        }
    }

    private void Update() {
        // Update attack cooldown timer
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    private void FixedUpdate() {
        if (isDead || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.fixedDeltaTime);

        // Flip horizontal
        if (direction.x != 0) {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Update animator
        if (animator != null) {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetFloat("DirectionX", direction.x);
        }
    }

    // Contact damage to the player
    private void OnTriggerStay2D(Collider2D other) {
        Debug.Log("Trigger: " + other.name);
        if (isDead || attackTimer > 0f) return;
        if (other.gameObject.CompareTag("Player")) {
            HeroController hero = other.gameObject.GetComponent<HeroController>();
            if (hero != null) {
                hero.TakeDamage(data.damage);
                attackTimer = attackCooldown;
                Debug.Log($"Enemy dealt {data.damage} damage to hero.");
            }
        }
    }

    public void TakeDamage(float damageAmount) {
        if (isDead) return;

        int damage = Mathf.RoundToInt(damageAmount);
        currentHealth -= damage;

        // Show damage popup
        if (damagePopupPrefab != null) {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform.parent);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) {
                popupScript.SetDamage(damage);
            }
        }

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();

        if (hasDeathAnimation && animator != null && animator.HasState(0, Animator.StringToHash("Death"))) {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
            Destroy(gameObject, 0.5f);
        } else {
            Destroy(gameObject);
        }
    }

    private void OnDisable() {
        OnDeath.RemoveAllListeners();
    }

    // Called by Animation Event
    public void OnDeathAnimationEnd() {
        Destroy(gameObject);
    }
}