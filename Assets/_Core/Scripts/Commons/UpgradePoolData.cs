using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "UpgradePool", menuName = "HexaBit/Upgrade Pool Data")]
    public class UpgradePoolData : ScriptableObject {
        [Header("Weapons")]
        public List<WeaponData> weapons = new List<WeaponData>();

        [Header("Buffs")]
        public List<BuffData> buffs = new List<BuffData>();

        // Combined pool for convenience (returns all items as Object list)
        public List<Object> GetAllItems() {
            List<Object> all = new List<Object>();
            all.AddRange(weapons);
            all.AddRange(buffs);
            return all;
        }

        // Filter weapons that are NOT at max level for a given hero
        public List<WeaponData> GetAvailableWeapons(HeroController hero) {
            List<WeaponData> available = new List<WeaponData>();
            foreach (var weapon in weapons) {
                if (hero.HasWeapon(weapon)) {
                    int currentLv = hero.weaponSlots.First(x => x?.data == weapon).currentLevel;
                    int maxLv = weapon.levels.Count;
                    if (currentLv < maxLv)
                        available.Add(weapon);
                } else {
                    available.Add(weapon);
                }
            }
            return available;
        }

        // Filter buffs that are NOT at max level for a given hero
        public List<BuffData> GetAvailableBuffs(HeroController hero) {
            List<BuffData> available = new List<BuffData>();
            foreach (var buff in buffs) {
                ActiveBuff existing = hero.activeBuffs.Find(b => b.data == buff);
                if (existing != null) {
                    if (existing.currentLevel < buff.MaxLevel)
                        available.Add(buff);
                } else {
                    available.Add(buff);
                }
            }
            return available;
        }

        // Full filtered pool for a specific hero
        public List<Object> GetAvailableItems(HeroController hero) {
            List<Object> available = new List<Object>();
            available.AddRange(GetAvailableWeapons(hero));
            available.AddRange(GetAvailableBuffs(hero));
            return available;
        }
    }
}