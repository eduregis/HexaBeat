using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Unity.Cinemachine;

namespace HexaBit.Core {
    public class GameplayManager : MonoBehaviour {
        public static GameplayManager Instance { get; private set; }

        [Header("Hero Setup")]
        [SerializeField] private HeroController heroPrefab;      // Single Hero Prefab reference
        [SerializeField] private List<HeroData> heroesData;      // List of data to configure each hero
        [SerializeField] private List<Transform> spawnPoints;    // Spawn points for each hero
        private List<HeroController> activeHeroes = new List<HeroController>(); // Runtime list

        [Header("XP Settings")]
        [SerializeField] private int baseXPToLevel = 20;
        [SerializeField] private int xpPerLevelMultiplier = 15;

        [Header("Runtime Stats")]
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int totalKills = 0;

        [Header("Camera Setup")]
        [SerializeField] private CinemachineCamera vcam;

        [Header("Level Up UI")]
        [SerializeField] private GameObject levelUpPanelPrefab;

        [Header("Level Up Pool")]
        [SerializeField] private List<Object> upgradePool; // Drag both WeaponData and BuffData here

        public int CurrentXP => currentXP;
        public int CurrentLevel => currentLevel;
        public int TotalKills => totalKills;
        public int XPToNextLevel => baseXPToLevel + (currentLevel - 1) * xpPerLevelMultiplier;

        // Events
        public UnityEvent<int> OnXPChanged;
        public UnityEvent<int> OnLevelUp;
        public UnityEvent<int> OnKillCountChanged;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            // Instantiate heroes using the single prefab and the HeroData list
            for (int i = 0; i < heroesData.Count; i++) {
                Transform spawnPos = (i < spawnPoints.Count) ? spawnPoints[i] : null;
                Vector3 position = spawnPos != null ? spawnPos.position : Vector3.zero;

                // Instantiate the base prefab
                HeroController newHero = Instantiate(heroPrefab, position, Quaternion.identity);

                // Inject the specific HeroData from the array
                newHero.SetHeroData(heroesData[i]);

                activeHeroes.Add(newHero);
            }

            if (activeHeroes.Count == 0) Debug.LogError("No heroes were spawned! Check Hero Data list.");

            // --- CINEMACHINE SETUP ---
            if (activeHeroes.Count > 0) {
                // Find the vcam if you didn't assign it in the Inspector (fallback safety)
                if (vcam == null) {
                    vcam = FindFirstObjectByType<CinemachineCamera>();
                }

                if (vcam != null) {
                    // Set Follow and LookAt to the first hero that was spawned
                    vcam.Follow = activeHeroes[0].transform;
                    vcam.LookAt = activeHeroes[0].transform;
                } 
            }
        }

        // XP is shared and added directly to the pool.
        public void AddXP(int amount) {
            currentXP += amount;
            OnXPChanged?.Invoke(currentXP);

            while (currentXP >= XPToNextLevel) {
                currentXP -= XPToNextLevel;
                currentLevel++;

                // --- LEVEL UP REWARD ROLL ---
                // 1. Randomly select the hero who will receive the reward this time
                HeroController targetHero = activeHeroes[Random.Range(0, activeHeroes.Count)];

                // 2. Generate the options based SOLELY on the target hero's inventory
                // Inside AddXP(), when level up occurs:
                if (levelUpPanelPrefab != null) {
                    GameObject panelObj = Instantiate(levelUpPanelPrefab);
                    LevelUpUIManager uiManager = panelObj.GetComponent<LevelUpUIManager>();

                    List<LevelUpOption> options = GenerateChoices(3, targetHero);

                    // Pass the targetHero to the UI manager
                    uiManager.OpenWithOptions(options, targetHero, (int selectedIndex) => {
                        LevelUpOption chosenOption = options[selectedIndex];
                        chosenOption.onSelected.Invoke();
                    });
                }

                OnLevelUp?.Invoke(currentLevel);
                Debug.Log($"Level Up! Now Level {currentLevel}. Reward goes to: {targetHero.name}");
            }
        }

        public void AddKill() {
            totalKills++;
            OnKillCountChanged?.Invoke(totalKills);
        }

        public class LevelUpOption {
            public string displayName;
            public string description;
            public Sprite icon;
            public bool isWeapon; 
            public int targetLevel;
            public System.Action onSelected;
        }

        public HeroController GetActiveHero(int index) {
            if (index >= 0 && index < activeHeroes.Count)
                return activeHeroes[index];
            return null;
        }

        // Receives the specific 'hero' that was randomly drawn for this reward
        private List<LevelUpOption> GenerateChoices(int count, HeroController hero) {
            List<LevelUpOption> options = new List<LevelUpOption>();
            List<Object> shuffledPool = upgradePool.OrderBy(x => System.Guid.NewGuid()).ToList();

            foreach (var item in shuffledPool) {
                if (options.Count >= count) break;

                if (item is WeaponData weaponData) {
                    LevelUpOption option = weaponData.GetUpgradeOption(hero);
                    options.Add(option);
                } else if (item is BuffData buffData) {
                    LevelUpOption option = buffData.GetUpgradeOption(hero);
                    options.Add(option);
                } 
            }

            // Fill remaining slots with "Skip" option
            while (options.Count < count) {
                options.Add(new LevelUpOption {
                    displayName = "Skip",
                    description = "",
                    icon = null,
                    isWeapon = false,
                    targetLevel = 0,
                    onSelected = () => { Debug.Log("Executing: Skip"); }
                });
            }

            return options;
        }
    }
}