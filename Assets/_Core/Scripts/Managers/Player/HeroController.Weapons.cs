using UnityEngine;

public partial class HeroController {
    public bool EquipWeapon(WeaponData weaponData) {
        for (int i = 0; i < weaponSlots.Length; i++) {
            if (weaponSlots[i] == null || weaponSlots[i].data == null) {
                if (weaponData.weaponPrefab == null) {
                    Debug.LogError($"WeaponData {weaponData.weaponName} has no weaponPrefab!");
                    return false;
                }

                GameObject weaponGO = Instantiate(weaponData.weaponPrefab, transform);
                weaponGO.transform.localPosition = Vector3.zero;

                WeaponBase weapon = weaponGO.GetComponent<WeaponBase>();
                if (weapon == null) {
                    Debug.LogError($"weaponPrefab of {weaponData.weaponName} has no WeaponBase component!");
                    Destroy(weaponGO);
                    return false;
                }

                weapon.Initialize(weaponData, 0);
                weaponSlots[i] = weapon;
                weapon.SetHeroController(this);

                Debug.Log($"Weapon {weaponData.weaponName} equipped in slot {i + 1} (Nv.1)");
                return true;
            }
        }
        Debug.LogWarning("All weapon slots are occupied!");
        return false;
    }

    public void SwapWeapon(int slotIndex, WeaponData newWeaponData) {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) {
            Debug.LogError("Invalid slot index!");
            return;
        }

        if (weaponSlots[slotIndex] != null) {
            Destroy(weaponSlots[slotIndex].gameObject);
            weaponSlots[slotIndex] = null;
        }

        GameObject weaponGO = new GameObject(newWeaponData.weaponName);
        weaponGO.transform.SetParent(transform);
        weaponGO.transform.localPosition = Vector3.zero;

        WeaponBase newWeapon = weaponGO.AddComponent<WeaponBase>();
        newWeapon.Initialize(newWeaponData, 0);
        weaponSlots[slotIndex] = newWeapon;
        newWeapon.SetHeroController(this);

        Debug.Log($"Weapon {newWeaponData.weaponName} equipped in slot {slotIndex + 1} (Nv.1)");
    }

    public bool UpgradeWeapon(WeaponData weaponData) {
        foreach (var slot in weaponSlots) {
            if (slot != null && slot.data == weaponData) {
                slot.LevelUp();
                return true;
            }
        }
        Debug.LogWarning($"Weapon {weaponData.weaponName} not found to upgrade.");
        return false;
    }

    public bool HasWeapon(WeaponData weaponData) {
        foreach (var slot in weaponSlots)
            if (slot != null && slot.data == weaponData)
                return true;
        return false;
    }

    public int GetWeaponSlot(WeaponData weaponData) {
        for (int i = 0; i < weaponSlots.Length; i++)
            if (weaponSlots[i] != null && weaponSlots[i].data == weaponData)
                return i;
        return -1;
    }
}