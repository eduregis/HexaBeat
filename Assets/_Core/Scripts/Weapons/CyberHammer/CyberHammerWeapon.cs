using UnityEngine;

namespace HexaBit.Core {
    public class CyberHammerWeapon : WeaponBase {
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            GameObject effectGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
            CyberHammerEffect effect = effectGO.GetComponent<CyberHammerEffect>();
            if (effect != null) {
                Vector2 direction = hero.FacingDirection;
                effect.Initialize(data, currentLevel, direction, hero);
            }
        }
    }
}