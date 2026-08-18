using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeroController : DamageableCharacter {
    [Header("Character Data")]
    [SerializeField] private HeroData heroData;

    [Header("Weapons Slots")]
    [SerializeField] private int maxWeaponSlots = 3;
    public WeaponBase[] weaponSlots;

    [Header("UI")]
    [SerializeField] private HeroHPSlider hpSlider;

    // Public properties for weapons to access
    public float DamageMultiplier { get; private set; } = 1f;
    public float ProjectileSpeedMultiplier { get; private set; } = 1f;
    public float GrowthMultiplier { get; private set; } = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    protected override void Awake() {
        base.Awake(); // calls base Awake to set spriteRenderer and originalColor

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("HeroController: Rigidbody2D not found!");

        animator = GetComponent<Animator>();

        if (weaponSlots == null || weaponSlots.Length == 0)
            weaponSlots = new WeaponBase[maxWeaponSlots];
    }

    private void Start() {
        if (heroData != null) {
            SetHealth(heroData.maxHealth, heroData.armor);

            DamageMultiplier = heroData.damageMultiplier;
            ProjectileSpeedMultiplier = heroData.projectileSpeedMultiplier;
            GrowthMultiplier = heroData.growthMultiplier;

            if (heroData.startingWeapon != null)
                EquipWeapon(heroData.startingWeapon);

            if (animator != null && heroData.animatorController != null) {
                animator.runtimeAnimatorController = heroData.animatorController;
            } else if (animator == null) {
                Debug.LogWarning("HeroController: Animator component not found!");
            } else if (heroData.animatorController == null) {
                Debug.LogWarning($"HeroController: No Animator Controller assigned in HeroData for {heroData.heroName}!");
            }
        } else {
            Debug.LogWarning("HeroController: HeroData not assigned! Using default values.");
            // Fallback: uses default values from base class (100 HP, 0 armor)
            SetHealth(maxHealth, armor);
        }
        OnHealthChanged += UpdateHealthBar;
    }

    // --- Movement ---
    public void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>().normalized;
        if (moveInput != Vector2.zero) {
            FacingDirection = moveInput;
            UpdateAnimationDirection();
        } else {
            UpdateAnimationDirection();
        }
    }

    private void FixedUpdate() {
        float currentSpeed = heroData != null ? heroData.moveSpeed : 3.5f;
        Vector2 targetPosition = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    private void UpdateAnimationDirection() {
        if (animator == null) return;
        animator.SetFloat("DirectionX", FacingDirection.x);
        animator.SetFloat("Speed", moveInput.magnitude);
    }

    // --- Weapon Management ---
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

    // --- UI Management ---
    private void UpdateHealthBar() {
        if (hpSlider != null) {
            hpSlider.value = (float)currentHealth / maxHealth; // Value still works perfectly because HeroHPSlider inherits Slider
        }
    }

    // --- Override Die ---
    protected override void Die() {
        Debug.Log("Hero died!");
        gameObject.SetActive(false);
    }
}