using UnityEngine;

public class SledgeHammerWeapon : WeaponBase {
    protected override void Fire() {
        if (data.attackPrefab == null) return;

        // Instancia o efeito do golpe (attackPrefab)
        GameObject effectGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
        SledgeHammerEffect effect = effectGO.GetComponent<SledgeHammerEffect>();
        if (effect != null) {
            // Passa a direção atual do herói
            Vector2 direction = hero.FacingDirection;
            effect.Initialize(data, currentLevel, direction);
        }
    }
}