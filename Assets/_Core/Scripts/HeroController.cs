using UnityEngine;
using UnityEngine.InputSystem;

public class HeroController : MonoBehaviour {
    [Header("Move")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Weapons Slots")]
    [SerializeField] private int maxWeaponSlots = 3;
    public WeaponBase[] weaponSlots;

    [Header("Initial Weapon")]
    [SerializeField] private WeaponData startingWeapon;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer; // Arraste o SpriteRenderer aqui

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("HeroController: Rigidbody2D not found!");

        // Se o spriteRenderer não foi atribuído, tenta pegar automaticamente
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (weaponSlots == null || weaponSlots.Length == 0)
            weaponSlots = new WeaponBase[maxWeaponSlots];
    }

    private void Start() {
        if (startingWeapon != null) {
            EquipWeapon(startingWeapon);
        } else {
            for (int i = 0; i < weaponSlots.Length; i++) {
                if (weaponSlots[i] != null && weaponSlots[i].data != null) {
                    weaponSlots[i].Initialize(weaponSlots[i].data, 0);
                    Debug.Log($"Slot {i + 1}: {weaponSlots[i].data.weaponName} equiped.");
                }
            }
        }
    }

    // --- Move (Input System) ---
    public void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>().normalized;
        if (moveInput != Vector2.zero) {
            FacingDirection = moveInput;
            UpdateFacingDirection();
        }
    }

    private void FixedUpdate() {
        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    // --- Flip Sprite ---
    private void UpdateFacingDirection() {
        if (spriteRenderer == null) return;

        // Se o herói estiver se movendo horizontalmente (esquerda ou direita)
        if (Mathf.Abs(FacingDirection.x) > 0.1f) {
            // Inverte o scale.x: -1 para esquerda, 1 para direita
            Vector3 scale = spriteRenderer.transform.localScale;
            scale.x = Mathf.Sign(FacingDirection.x) * Mathf.Abs(scale.x);
            spriteRenderer.transform.localScale = scale;
        }
        // Se o herói estiver parado ou se movendo verticalmente, mantém a última orientação
    }

    // --- Weapon Management ---
    public bool EquipWeapon(WeaponData weaponData) {
        for (int i = 0; i < weaponSlots.Length; i++) {
            if (weaponSlots[i] == null || weaponSlots[i].data == null) {
                if (weaponData.weaponPrefab == null) {
                    Debug.LogError($"WeaponData {weaponData.weaponName} não tem weaponPrefab!");
                    return false;
                }

                GameObject weaponGO = Instantiate(weaponData.weaponPrefab, transform);
                weaponGO.transform.localPosition = Vector3.zero;

                WeaponBase weapon = weaponGO.GetComponent<WeaponBase>();
                if (weapon == null) {
                    Debug.LogError($"weaponPrefab de {weaponData.weaponName} não tem WeaponBase!");
                    Destroy(weaponGO);
                    return false;
                }

                weapon.Initialize(weaponData, 0);
                weaponSlots[i] = weapon;

                Debug.Log($"Arma {weaponData.weaponName} equipada no slot {i + 1} (Nv.1)");
                return true;
            }
        }
        Debug.LogWarning("Todos os slots de arma estão ocupados!");
        return false;
    }

    public void SwapWeapon(int slotIndex, WeaponData newWeaponData) {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) {
            Debug.LogError("Invalid Slot!");
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

        Debug.Log($"Weapon {newWeaponData.weaponName} equiped in slot {slotIndex + 1} (Lv.1)");
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