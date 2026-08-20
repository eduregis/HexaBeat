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

        private Transform player;
        private Vector2 knockbackDirection;
        private float knockbackTimer;
        private bool isKnockbackActive = false;
        private bool isAttracted = false;
        private bool isCollected = false;

        private void Start() {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) {
                Debug.LogError("XPDrop: Player not found!");
                Destroy(gameObject);
                return;
            }

            // Inicialmente, o cristal fica parado (sem movimento)
            isKnockbackActive = false;
            isAttracted = false;
            isCollected = false;
        }

        private void Update() {
            if (player == null || isCollected) return;

            // Fase 1: Knockback (se ativo)
            if (isKnockbackActive) {
                knockbackTimer -= Time.deltaTime;
                transform.Translate(knockbackDirection * Time.deltaTime, Space.World);

                if (knockbackTimer <= 0f) {
                    isKnockbackActive = false;
                    isAttracted = true; // Começa a atração após o knockback
                }
                return;
            }

            // Fase 2: Atração (após o knockback)
            if (isAttracted) {
                Vector2 direction = (player.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, player.position);

                // Move em direção ao player
                transform.Translate(direction * attractSpeed * Time.deltaTime, Space.World);

                // Coleta se estiver perto
                if (distance < collectDistance) {
                    Collect();
                }
            }
            // Se não estiver em nenhuma fase, fica parado (esperando o player tocar)
        }

        // Quando o player tocar o cristal (trigger)
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") && !isCollected && !isKnockbackActive && !isAttracted) {
                // Aplica knockback para longe do player
                Vector2 directionFromPlayer = (transform.position - player.position).normalized;
                knockbackDirection = directionFromPlayer * knockbackForce;
                knockbackTimer = knockbackDuration;
                isKnockbackActive = true;
                // A atração começará após o knockback terminar (no Update)
            }
        }

        private void Collect() {
            if (isCollected) return;
            isCollected = true;

            // Adiciona XP ao GameplayManager
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.AddXP(xpValue);
            }

            // (Opcional) Efeito visual, partícula, som, etc.

            Destroy(gameObject);
        }
    }
}