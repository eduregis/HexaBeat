using UnityEngine;

public class WeaponBase : MonoBehaviour {
    [Header("Weapon Data")]
    public WeaponData data;
    public int currentLevel = 1;

    private float timer;
    protected HeroController hero;

    private void Start() {
        hero = GetComponentInParent<HeroController>();
        if (hero == null) Debug.LogError("WeaponBase must be a Hero child!");
    }

    private void Update() {
        if (data == null || data.levels.Count == 0) return;

        timer += Time.deltaTime;
        float cooldown = data.GetCooldown(currentLevel);
        if (timer >= cooldown) {
            timer = 0f;
            Fire();
        }
    }

    protected virtual void Fire() {
        Debug.Log($"{data.weaponName} (Nv.{currentLevel}) Generic fire!");
        // Aqui entra a lógica de instanciar projétil, golpe, etc.
    }

    public void Initialize(WeaponData weaponData, int level = 1) {
        data = weaponData;
        currentLevel = Mathf.Clamp(level, 0, data.levels.Count - 1);
        gameObject.name = weaponData.weaponName;
    }

    //Level up.
    public void LevelUp() {
        if (currentLevel < data.levels.Count - 1) {
            currentLevel++;
            Debug.Log($"{data.weaponName} up to Lv.{currentLevel + 1}");
        } else {
            Debug.Log($"{data.weaponName} is in maximum level!");
        }
    }
}