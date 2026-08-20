using UnityEngine;

namespace HexaBit.Core {
    public class SledgeHammerWeapon : WeaponBase {
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            GameObject effectGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
            SledgeHammerEffect effect = effectGO.GetComponent<SledgeHammerEffect>();
            if (effect != null) {
                Vector2 direction = hero.FacingDirection;
                effect.Initialize(data, currentLevel, direction, hero.transform);
            }
        }
    }
}