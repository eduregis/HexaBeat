using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class EnemyController : DamageableCharacter {
        [Header("Data")]
        [SerializeField] private EnemyData data;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool hasDeathAnimation = true;

        private bool isDead = false;
        private Transform player;
        private Rigidbody2D rb;

        [Header("Attack")]
        [SerializeField] private float attackCooldown = 0.5f;
        private float attackTimer = 0f;

        [Header("Events")]
        public UnityEvent OnDeath;

        public EnemyData Data => data;
        public bool IsDead => isDead;

        private HeroController heroController;

        protected override void Awake() {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");
            if (animator == null) animator = GetComponent<Animator>();
        }

        public void Initialize(EnemyData enemyData, int wave) {
            data = enemyData;

            int scaledHealth = data.baseHealth;
            if (!data.healthScalesWithPlayerLevel) {
                scaledHealth += Mathf.RoundToInt(data.baseHealth * 0.1f * wave);
            }

            if (data.isBoss)
                scaledHealth *= 3;

            SetHealth(scaledHealth, 0);

            baseSpeed = data.moveSpeed;
            if (data.isBoss)
                baseSpeed *= 1.2f;
            else
                baseSpeed += data.moveSpeed * 0.05f * wave;

            CurrentSpeed = baseSpeed;
            isDead = false;
            attackTimer = 0f;
            if (animator != null) {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Respawn");
            }
        }

        public void SetPlayerReference(HeroController hero) {
            heroController = hero;
            player = hero != null ? hero.transform : null;
        }

        private void OnEnable() {
            if (data != null) {
                SetHealth(data.baseHealth, 0);
                baseSpeed = data.moveSpeed;
                CurrentSpeed = baseSpeed;
            } else {
                SetHealth(maxHealth, armor);
                baseSpeed = 3f;
                CurrentSpeed = baseSpeed;
            }

            isDead = false;
            attackTimer = 0f;
            if (animator != null) {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Respawn");
            }

            ClearAllStatuses();
        }

        private void Update() {
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            UpdateStatuses();
        }

        private void FixedUpdate() {
            if (isDead) return;
            if (heroController == null || heroController.IsDead) return;

            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * CurrentSpeed * Time.fixedDeltaTime);

            if (direction.x != 0) {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            if (animator != null) {
                animator.SetFloat("Speed", rb.linearVelocity.magnitude);
                animator.SetFloat("DirectionX", direction.x);
            }
        }

        // Contact damage
        private void OnTriggerStay2D(Collider2D other) {
            if (isDead || attackTimer > 0f) return;
            if (heroController == null || heroController.IsDead) return;
            if (other.CompareTag("Player")) {
                HeroController hero = other.GetComponent<HeroController>();
                if (hero != null) {
                    hero.TakeDamage(data.damage);
                    attackTimer = attackCooldown;
                    Debug.Log($"Enemy dealt {data.damage} damage to hero.");
                }
            }
        }

        protected override void AnimateTakingDamage() {
            if (animator == null) return;
            // Adding a damaged animation to enemies
            // animator.SetBool("Damaged", damaged);
        }

        protected override void Die() {
            if (isDead) return;
            isDead = true;

            ClearAllStatuses();
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

        public void OnDeathAnimationEnd() {
            Destroy(gameObject);
        }
    }
}