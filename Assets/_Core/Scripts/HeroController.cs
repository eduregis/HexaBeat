using UnityEngine;
using UnityEngine.InputSystem;

public class HeroController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Weapons Slots")]
    [SerializeField] private int maxWeaponSlots = 3;
    public WeaponBase[] weaponSlots;

    [Header("Initial Weapon")]
    [SerializeField] private WeaponData startingWeapon;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("HeroController: Rigidbody2D not found!");

        // Initializes the array of slots
        if (weaponSlots == null || weaponSlots.Length == 0)
            weaponSlots = new WeaponBase[maxWeaponSlots];
    }

    private void Start()
    {
        // If a starting weapon is defined, equip it in the first empty slot
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
        else
        {
            // Otherwise, check if any slot has already been filled in the Inspector
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].data != null)
                {
                    weaponSlots[i].Initialize(weaponSlots[i].data, 0);
                    Debug.Log($"Slot {i + 1}: {weaponSlots[i].data.weaponName} equiped.");
                }
            }
        }
    }

    // --- Move (Input System) ---
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>().normalized;
        if (moveInput != Vector2.zero) FacingDirection = moveInput;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    // --- Weapon Management ---
    // Equip a new weapon in the first empty slot
    public bool EquipWeapon(WeaponData weaponData)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null || weaponSlots[i].data == null)
            {
                // Creates a child GameObject for the weapon.
                GameObject weaponGO = new GameObject(weaponData.weaponName);
                weaponGO.transform.SetParent(transform);
                weaponGO.transform.localPosition = Vector3.zero;

                WeaponBase weapon = weaponGO.AddComponent<WeaponBase>();
                weapon.Initialize(weaponData, 0);
                weaponSlots[i] = weapon;

                Debug.Log($"Weapon {weaponData.weaponName} equiped in slot {i + 1} (Lv.1)");
                return true;
            }
        }
        Debug.LogWarning("All weapon slots are occupied!");
        return false;
    }

    // Replaces the weapon in a specific slot with a new one
    public void SwapWeapon(int slotIndex, WeaponData newWeaponData)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            Debug.LogError("Invalid Slot!");
            return;
        }

        // Remove the old weapon (if it exists)
        if (weaponSlots[slotIndex] != null)
        {
            Destroy(weaponSlots[slotIndex].gameObject);
            weaponSlots[slotIndex] = null;
        }

        // Create a new
        GameObject weaponGO = new GameObject(newWeaponData.weaponName);
        weaponGO.transform.SetParent(transform);
        weaponGO.transform.localPosition = Vector3.zero;

        WeaponBase newWeapon = weaponGO.AddComponent<WeaponBase>();
        newWeapon.Initialize(newWeaponData, 0);
        weaponSlots[slotIndex] = newWeapon;

        Debug.Log($"Weapon {newWeaponData.weaponName} equiped in slot {slotIndex + 1} (Lv.1)");
    }

    // Increases the level of an already equipped weapon (if found)
    public bool UpgradeWeapon(WeaponData weaponData)
    {
        foreach (var slot in weaponSlots)
        {
            if (slot != null && slot.data == weaponData)
            {
                slot.LevelUp();
                return true;
            }
        }
        Debug.LogWarning($"Weapon {weaponData.weaponName} not found to upgrade.");
        return false;
    }

    // Checks if a weapon is already equipped
    public bool HasWeapon(WeaponData weaponData)
    {
        foreach (var slot in weaponSlots)
            if (slot != null && slot.data == weaponData)
                return true;
        return false;
    }

    // Returns the index of the slot where a weapon is equipped, or -1 if it is not
    public int GetWeaponSlot(WeaponData weaponData)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
            if (weaponSlots[i] != null && weaponSlots[i].data == weaponData)
                return i;
        return -1;
    }
}