using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic; // For List

namespace HexaBit.Core {
    public partial class HeroController : DamageableCharacter {
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
        private float currentSpeed;

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
                SetHealth(maxHealth, armor);
            }

            currentSpeed = heroData != null ? heroData.moveSpeed : 3.5f;
            OnHealthChanged += UpdateHealthBar;

            // Initial refresh of stats (in case there are no buffs yet)
            RefreshStats();
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
            Vector2 targetPosition = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }

        private void UpdateAnimationDirection() {
            if (animator == null) return;
            animator.SetFloat("DirectionX", FacingDirection.x);
            animator.SetFloat("Speed", moveInput.magnitude);
        }

        public void SetHeroData(HeroData data) {
            heroData = data;
        }

        // --- UI Management ---
        private void UpdateHealthBar() {
            if (hpSlider != null) {
                hpSlider.value = (float)currentHealth / maxHealth;
            }
        }

        // --- Override Die ---
        protected override void Die() {
            Debug.Log("Hero died!");
            gameObject.SetActive(false);
        }
    }
}