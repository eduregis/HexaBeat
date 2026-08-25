using UnityEngine;
using System.Collections;

namespace HexaBit.Core {
    public class LightningBoltEffect : WeaponEffect {
        [Header("Visual")]
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private float damageDelay = 0.3f;

        private WeaponData data;
        private int level;
        private float splashArea;
        private int damage;
        private Transform target;
        private bool isDead = false;

        public override void Initialize(WeaponData weaponData, int levelIndex, Vector2 direction, Transform heroTransform) {
            data = weaponData;
            level = levelIndex;
            splashArea = data.GetFloat(level, DynamicParameter.SplashArea);
            damage = Mathf.RoundToInt(data.GetDamage(level));
        }

        public void SetTarget(Transform targetTransform) {
            target = targetTransform;
            if (target == null) {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position;

            float baseArea = 1.5f; // Rescale sprite
            float scaleFactor = splashArea / baseArea;
            transform.localScale = Vector3.one * scaleFactor;

            // Apply damage after delay
            StartCoroutine(ApplyDamageAfterDelay());
        }

        private IEnumerator ApplyDamageAfterDelay() {
            yield return new WaitForSeconds(damageDelay);

            if (isDead || target == null) yield break;

            // Splash damage
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, splashArea);
            foreach (var collider in hitColliders) {
                if (collider.CompareTag("Enemy")) {
                    EnemyController enemy = collider.GetComponent<EnemyController>();
                    if (enemy != null) {
                        enemy.TakeDamage(damage);
                    }
                }
            }

            if (impactParticles != null) {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }

            isDead = true;
            Destroy(gameObject, 0.2f);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, splashArea);
        }
    }
}