using UnityEngine;
using System.Collections.Generic;

public class SledgeHammerEffect : WeaponEffect {
    [Header("Swing Settings")]
    [SerializeField] private float swingDuration = 0.3f;

    [Header("References")]
    [SerializeField] private Transform hammerPivot;
    [SerializeField] private GameObject hammerHead;

    private WeaponData data;
    private int level;
    private Vector2 direction;
    private float timer;
    private float totalAngle;
    private float radius;
    private int damage;
    private float knockback;
    private Transform heroTransform; // Referência ao herói

    private List<EnemyController> hitEnemies = new List<EnemyController>();

    public override void Initialize(WeaponData weaponData, int levelIndex, Vector2 dir) {
        data = weaponData;
        level = levelIndex;
        direction = -dir.normalized;

        totalAngle = data.GetFloat(level, DynamicParameter.Angle);
        radius = data.GetFloat(level, DynamicParameter.Radius);
        damage = Mathf.RoundToInt(data.GetDamage(level));
        knockback = data.GetFloat(level, DynamicParameter.Knockback);

        // Busca o herói
        GameObject heroObj = GameObject.FindGameObjectWithTag("Player");
        if (heroObj != null) heroTransform = heroObj.transform;

        if (hammerPivot != null) {
            hammerPivot.rotation = Quaternion.identity;
        }

        if (hammerHead != null) {
            hammerHead.transform.localScale = Vector3.one * radius;
        }

        SetupHammer();
        timer = 0f;
        hitEnemies.Clear();

        Debug.Log($"SledgeHammerEffect initialized: damage={damage}, angle={totalAngle}, radius={radius}, knockback={knockback}");
    }

    private void SetupHammer() {
        if (hammerPivot == null) return;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle + 90f - totalAngle / 2f;
        hammerPivot.rotation = Quaternion.Euler(0, 0, startAngle);
    }

    private void Update() {
        // Segue o herói
        if (hammerPivot != null && heroTransform != null) {
            hammerPivot.position = heroTransform.position;
        }

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / swingDuration);

        if (hammerPivot != null) {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle + 90f - totalAngle / 2f;
            float endAngle = baseAngle + 90f + totalAngle / 2f;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);
            hammerPivot.rotation = Quaternion.Euler(0, 0, currentAngle);
        }

        if (progress >= 1f) {
            Destroy(gameObject);
        }
    }

    public void OnHammerHit(Collider2D other) {
        Debug.Log("HammerHead collision detected with: " + other.name);

        if (other.CompareTag("Enemy")) {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !hitEnemies.Contains(enemy)) {
                enemy.TakeDamage(damage);
                if (knockback > 0) {
                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                    if (rb != null) {
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        rb.AddForce(knockbackDir * knockback, ForceMode2D.Impulse);
                    }
                }
                hitEnemies.Add(enemy);
                Debug.Log($"Hit enemy! Damage applied: {damage}");
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}