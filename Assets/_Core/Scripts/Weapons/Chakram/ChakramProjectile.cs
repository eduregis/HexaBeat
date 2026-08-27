using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class ChakramProjectile : WeaponEffect {
        [Header("Chakram Settings")]
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float hitCooldown = 0.3f;

        private Vector2 initialDirection;
        private Vector2 currentDirection;
        private float speed;
        private float maxDistance;
        private float hitboxRadius;
        private int damage;

        private Transform heroTransform;
        private float distanceTraveled = 0f;
        private bool isReturning = false;
        private float currentSpeed;
        private float lifetimeTimer;

        // Track enemies and their hit timers
        private Dictionary<EnemyController, float> hitTimers = new Dictionary<EnemyController, float>();

        private float returnAcceleration = 15f;

        public override void Initialize(WeaponData data, int levelIndex, Vector2 dir, Transform hero) {
            initialDirection = dir.normalized;
            currentDirection = initialDirection;
            heroTransform = hero;

            speed = data.GetFloat(levelIndex, DynamicParameter.Speed);
            maxDistance = data.GetFloat(levelIndex, DynamicParameter.MaxDistance);
            hitboxRadius = data.GetFloat(levelIndex, DynamicParameter.Hitbox);
            damage = Mathf.RoundToInt(data.GetDamage(levelIndex));

            currentSpeed = speed;
            distanceTraveled = 0f;
            isReturning = false;
            lifetimeTimer = 0f;
            hitTimers.Clear();

            // Set initial rotation
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Set collider radius
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = hitboxRadius;

            returnAcceleration = speed * 2.5f;

            Debug.Log($"Chakram launched: speed={speed}, maxDistance={maxDistance}, damage={damage}");
        }

        private void Update() {
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= maxLifetime) {
                DestroyProjectile();
                return;
            }

            if (heroTransform == null) {
                DestroyProjectile();
                return;
            }

            UpdateMovement();

            if (IsOutOfBounds()) {
                DestroyProjectile();
                return;
            }

            // Check if returned to hero (collected)
            if (isReturning) {
                float distToHero = Vector2.Distance(transform.position, heroTransform.position);
                if (distToHero < 0.8f) {
                    DestroyProjectile();
                    return;
                }
            }

            // Update hit timers
            UpdateHitTimers();
        }

        private void UpdateMovement() {
            if (!isReturning) {
                // Outward: decelerate from speed to 0 over maxDistance
                float progress = Mathf.Clamp01(distanceTraveled / maxDistance);
                currentSpeed = Mathf.Lerp(speed, 0f, progress * progress);

                transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);
                distanceTraveled += currentSpeed * Time.deltaTime;

                if (distanceTraveled >= maxDistance || currentSpeed < 0.05f) {
                    isReturning = true;
                    currentDirection = -initialDirection;
                    currentSpeed = 0f;
                    Debug.Log($"Chakram starting return journey");
                }
            } else {
                // Return: accelerate continuously
                currentSpeed = Mathf.Min(currentSpeed + returnAcceleration * Time.deltaTime, speed);
                transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);
            }
        }

        private void UpdateHitTimers() {
            List<EnemyController> toRemove = new List<EnemyController>();
            foreach (var kvp in hitTimers) {
                float newTime = kvp.Value - Time.deltaTime;
                if (newTime <= 0f)
                    toRemove.Add(kvp.Key);
                else
                    hitTimers[kvp.Key] = newTime;
            }
            foreach (var enemy in toRemove) {
                hitTimers.Remove(enemy);
            }
        }

        private bool IsOutOfBounds() {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return false;

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            float margin = 2f;
            return viewportPos.x < -margin || viewportPos.x > 1f + margin ||
                   viewportPos.y < -margin || viewportPos.y > 1f + margin ||
                   viewportPos.z < 0;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemy.IsDead) {
                    // Check if enemy is on cooldown
                    if (hitTimers.ContainsKey(enemy))
                        return;

                    // Apply damage
                    enemy.TakeDamage(damage);
                    hitTimers[enemy] = hitCooldown;

                    Debug.Log($"Chakram hit enemy for {damage} damage on {(isReturning ? "return" : "outward")} pass!");
                }
            } else if (other.CompareTag("Wall")) {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile() {
            hitTimers.Clear();
            if (heroTransform != null) {
                ChakramWeapon weapon = heroTransform.GetComponentInChildren<ChakramWeapon>();
                if (weapon != null)
                    weapon.OnChakramDestroyed();
            }
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hitboxRadius);
        }
    }
}