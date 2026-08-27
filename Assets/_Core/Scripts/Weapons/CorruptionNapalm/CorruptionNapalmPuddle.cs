using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class CorruptionNapalmPuddle : MonoBehaviour {
        [Header("Puddle Settings")]
        [SerializeField] private CircleCollider2D puddleCollider;
        [SerializeField] private SpriteRenderer puddleOutline;

        private WeaponData data;
        private int level;
        private float duration;
        private float radius;
        private int damagePerTick;
        private float tickInterval;
        private float timer;
        private float lifetimeTimer;

        private Transform heroTransform;
        private List<EnemyController> enemiesInPuddle = new List<EnemyController>();

        public void Initialize(WeaponData weaponData, int levelIndex, Transform hero) {
            data = weaponData;
            level = levelIndex;
            heroTransform = hero;

            duration = data.GetFloat(level, DynamicParameter.Duration);
            radius = data.GetFloat(level, DynamicParameter.Radius);
            damagePerTick = Mathf.RoundToInt(data.GetDamage(level));
            tickInterval = data.GetFloat(level, DynamicParameter.Tick);

            if (puddleCollider != null)
                puddleCollider.radius = radius;

            if (puddleOutline != null) {
                float scale = radius * 2f;
                puddleOutline.transform.localScale = new Vector3(scale, scale, 1f);
            }

            timer = 0f;
            lifetimeTimer = 0f;
            enemiesInPuddle.Clear();

            Debug.Log($"CorruptionNapalm puddle created: radius={radius}, duration={duration}, tickInterval={tickInterval}");
        }

        private void Update() {
            lifetimeTimer += Time.deltaTime;

            if (lifetimeTimer >= duration) {
                enemiesInPuddle.Clear();
                Destroy(gameObject);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= tickInterval) {
                timer = 0f;
                ApplyDamageToEnemies();
            }
        }

        private void ApplyDamageToEnemies() {
            // Create a copy of the list to iterate safely
            List<EnemyController> enemiesCopy = new List<EnemyController>(enemiesInPuddle);

            foreach (var enemy in enemiesCopy) {
                if (enemy != null && !enemy.IsDead) {
                    enemy.TakeDamage(damagePerTick);
                    Debug.Log($"CorruptionNapalm dealt {damagePerTick} damage to {enemy.name}");
                }
            }

            // Remove dead enemies from the original list (safe to do after iteration)
            enemiesInPuddle.RemoveAll(e => e == null || e.IsDead);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInPuddle.Contains(enemy) && !enemy.IsDead) {
                    enemiesInPuddle.Add(enemy);
                    enemy.TakeDamage(damagePerTick);
                    Debug.Log($"CorruptionNapalm entered puddle: {enemy.name}");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemiesInPuddle.Contains(enemy)) {
                    enemiesInPuddle.Remove(enemy);
                }
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}