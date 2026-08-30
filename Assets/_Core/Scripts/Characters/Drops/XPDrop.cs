using UnityEngine;

namespace HexaBit.Core {
    public class XPDrop : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] private float attractSpeed = 5f;
        [SerializeField] private float knockbackForce = 3f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private float collectDistance = 0.5f;

        [Header("Reward")]
        [SerializeField] private int xpValue = 10;

        private Transform targetHeroTransform;
        private HeroController targetHeroController;

        private Vector2 knockbackDirection;
        private float knockbackTimer;
        private bool isKnockbackActive = false;
        private bool isAttracted = false;
        private bool isCollected = false;

        public bool IsCollected => isCollected;

        private void Start() {
            isKnockbackActive = false;
            isAttracted = false;
            isCollected = false;
        }

        private void Update() {
            if (isCollected) return;

            // Knockback phase
            if (isKnockbackActive) {
                knockbackTimer -= Time.deltaTime;
                transform.Translate(knockbackDirection * Time.deltaTime, Space.World);

                if (knockbackTimer <= 0f) {
                    isKnockbackActive = false;
                    // Only start attraction if we have a target hero
                    if (targetHeroController != null) {
                        isAttracted = true;
                    }
                }
                return;
            }

            // Attraction phase
            if (isAttracted && targetHeroTransform != null) {
                Vector2 direction = (targetHeroTransform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, targetHeroTransform.position);

                transform.Translate(direction * attractSpeed * Time.deltaTime, Space.World);

                if (distance < collectDistance) {
                    Collect();
                }
            }
        }

        /// <summary>
        /// Sets the target hero for this drop (called by DropCollector).
        /// Does NOT start attraction immediately - waits for knockback.
        /// </summary>
        public void SetTargetHero(HeroController hero) {
            if (isCollected) return;
            if (hero == null || hero.IsDead) return;

            targetHeroController = hero;
            targetHeroTransform = hero.transform;

            // If the drop is not in knockback and not already attracted, we can start attraction
            // But we want to wait for knockback to happen first.
            // If there's no knockback active, we can start attraction immediately.
            if (!isKnockbackActive && !isAttracted) {
                isAttracted = true;
            }
        }

        /// <summary>
        /// Triggered when the player physically touches the crystal.
        /// Applies knockback away from the player.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") && !isCollected && !isKnockbackActive) {
                HeroController touchingHero = other.GetComponent<HeroController>();
                if (touchingHero != null) {
                    // Apply knockback away from the player
                    Vector2 directionFromPlayer = (transform.position - other.transform.position).normalized;
                    knockbackDirection = directionFromPlayer * knockbackForce;
                    knockbackTimer = knockbackDuration;
                    isKnockbackActive = true;

                    // Store the hero as target for when knockback ends
                    targetHeroController = touchingHero;
                    targetHeroTransform = touchingHero.transform;

                    // Disable attraction during knockback
                    isAttracted = false;
                }
            }
        }

        private void Collect() {
            if (isCollected) return;
            isCollected = true;

            int finalXP = xpValue;
            if (targetHeroController != null) {
                finalXP = Mathf.RoundToInt(xpValue * targetHeroController.GlobalXPMultiplier);
            }

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.AddXP(finalXP, targetHeroController);
            }

            Destroy(gameObject);
        }
    }
}