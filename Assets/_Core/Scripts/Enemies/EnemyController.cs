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

    [Header("Feedback")]
    [SerializeField] private GameObject damagePopupPrefab;

    [Header("Events")]
    public UnityEvent OnDeath;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Initialize(EnemyData data, int wave) {
        this.data = data;
        currentHealth = data.baseHealth;
        currentSpeed = data.moveSpeed;

        // Aplica scaling baseado na onda (ex: +10% de vida e velocidade por onda)
        if (data.healthScalesWithPlayerLevel) {
            // (Opcional) Pega o nível do jogador
            // int playerLevel = GameManager.Instance.PlayerLevel;
            // currentHealth += data.baseHealth * playerLevel;
        } else {
            // Scaling por onda
            currentHealth += Mathf.RoundToInt(data.baseHealth * 0.1f * wave);
            currentSpeed += data.moveSpeed * 0.05f * wave;
        }

        // Se for boss, aumenta mais
        if (data.isBoss) {
            currentHealth *= 3;
            currentSpeed *= 1.2f;
        }

        // Reseta flags
        isDead = false;
        if (animator != null) {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Respawn");
        }
    }

    private void OnEnable() {
        // Reset (pooling)
        currentHealth = data.baseHealth;
        currentSpeed = data.moveSpeed;
        isDead = false;
        if (animator != null) {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Respawn");
        }
    }

    private void FixedUpdate() {
        if (isDead || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.fixedDeltaTime);

        // Flip horizontal baseado na dire��o
        if (direction.x != 0) {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Atualiza parametros do Animator
        if (animator != null) {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetFloat("DirectionX", direction.x);
        }
    }

    public void TakeDamage(float damageAmount) {
        if (isDead) return;

        int damage = Mathf.RoundToInt(damageAmount);
        currentHealth -= damage;

        // Mostra popup de dano
        if (damagePopupPrefab != null) {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform.parent);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) {
                Debug.Log($"Generating Damage Popup ({damage})");
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

        // Solta XP, etc.
        // ...
    }

    private void OnDisable() {
        OnDeath.RemoveAllListeners();
    }

    // M�todo chamado por Animation Event (opcional) para destruir no fim da anima��o
    public void OnDeathAnimationEnd() {
        Destroy(gameObject);
    }
}