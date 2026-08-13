using UnityEngine;

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

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        // Reset ao reativar (para pooling)
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

        // Atualiza par�metros do Animator
        if (animator != null) {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetFloat("DirectionX", direction.x);
        }
    }

    public void TakeDamage(float damage) {
        if (isDead) return;

        currentHealth -= Mathf.RoundToInt(damage);
        if (currentHealth <= 0) {
            Die();
        } else {
            // animator?.SetTrigger("Hit");
        }
    }

    private void Die() {
        if (isDead) return;
        isDead = true;

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

    // M�todo chamado por Animation Event (opcional) para destruir no fim da anima��o
    public void OnDeathAnimationEnd() {
        Destroy(gameObject);
    }
}