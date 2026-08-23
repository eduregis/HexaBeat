using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class EMPField : MonoBehaviour {
        [Header("References")]
        [SerializeField] private CircleCollider2D fieldCollider;
        [SerializeField] private SpriteRenderer fieldSprite;

        private StatusData slowStatusData;

        private WeaponData data;
        private int currentLevel;
        private HeroController hero;
        private float radius;
        private float slowAmount;
        private int damage;
        private float tickInterval;
        private float timer;

        // Enemies currently inside the field
        private List<EnemyController> enemiesInRange = new List<EnemyController>();

        public void Initialize(WeaponData weaponData, int level, HeroController heroController) {
            data = weaponData;
            currentLevel = level;
            hero = heroController;

            radius = data.GetFloat(currentLevel, DynamicParameter.Radius);
            slowAmount = data.GetFloat(currentLevel, DynamicParameter.SlowAmount);
            damage = Mathf.RoundToInt(data.GetDamage(currentLevel));
            tickInterval = data.GetCooldown(currentLevel); // Cooldown = tick interval

            if (fieldCollider != null)
                fieldCollider.radius = radius / 5f;

            if (fieldSprite != null) {
                float scale = radius * 2f;
                fieldSprite.transform.localScale = new Vector3(scale, scale, 1f);
            }

            // Setup slow status
            if (data.statusData != null) {
                slowStatusData = data.statusData;
                slowStatusData.slowPercentage = slowAmount;
                slowStatusData.duration = 0.5f; // Short duration, refreshed on each tick
            }

            timer = 0f;
            enemiesInRange.Clear();
        }

        private void Update() {
            if (hero == null || data == null) return;

            // Tick timer
            timer += Time.deltaTime;
            if (timer >= tickInterval) {
                timer = 0f;
                ApplyTickEffects();
            }
        }

        // Applies damage and slow to all enemies currently in the field
        private void ApplyTickEffects() {
            // Copy the list to avoid modification during iteration
            EnemyController[] enemiesCopy = enemiesInRange.ToArray();

            foreach (var enemy in enemiesCopy) {
                if (enemy != null && !enemy.IsDead && enemiesInRange.Contains(enemy)) {
                    // Apply damage
                    enemy.TakeDamage(damage);

                    // Reapply slow (resets duration, no accumulation)
                    if (slowAmount > 0 && slowStatusData != null) {
                        slowStatusData.slowPercentage = slowAmount;
                        enemy.ApplyStatus(slowStatusData);
                    }
                }
            }
        }

        // ---------- TRIGGER EVENTS ----------
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInRange.Contains(enemy)) {
                    enemiesInRange.Add(enemy);

                    // Apply slow immediately on entry (so it works before first tick)
                    if (slowAmount > 0 && slowStatusData != null) {
                        slowStatusData.slowPercentage = slowAmount;
                        enemy.ApplyStatus(slowStatusData);
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other) {
            // Ensure enemies are in the list (in case they were added via other means)
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInRange.Contains(enemy) && !enemy.IsDead) {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemiesInRange.Contains(enemy)) {
                    enemiesInRange.Remove(enemy);

                    // Remove slow status when enemy leaves
                    if (slowStatusData != null)
                        enemy.RemoveStatus(slowStatusData);
                }
            }
        }

        private void OnDestroy() {
            // Clean up slow from all enemies still in range
            foreach (var enemy in enemiesInRange) {
                if (enemy != null && slowStatusData != null)
                    enemy.RemoveStatus(slowStatusData);
            }
            enemiesInRange.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}