using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
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

        [Header("UI Prefabs")]
        [SerializeField] private GameObject pausePanelPrefab;
        [SerializeField] private GameObject gameOverPanelPrefab;

        [Header("Upgrade Pool")]
        [SerializeField] private UpgradePoolData upgradePoolData;

        [Header("Timer")]
        [SerializeField] private float currentTime = 0f;
        private bool isTimerPaused = false;

        // Cached UI Input Module from the scene's EventSystem
        private InputSystemUIInputModule cachedUIModule;

        // Flag to skip the first sceneLoaded event (since Start already handles it)
        private bool _initialized = false;

        public int CurrentXP => currentXP;
        public int CurrentLevel => currentLevel;
        public int TotalKills => totalKills;
        public int XPToNextLevel => baseXPToLevel + (currentLevel - 1) * xpPerLevelMultiplier;
        public float CurrentTime => currentTime;
        public bool IsTimerPaused => isTimerPaused;

        // Events
        public UnityEvent<int> OnXPChanged;
        public UnityEvent<int> OnLevelUp;
        public UnityEvent<int> OnKillCountChanged;
        public UnityEvent<float> OnTimerUpdated;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cache the UI Input Module from the scene's EventSystem
            CacheUIModule();
        }

        /// <summary>
        /// Finds and caches the InputSystemUIInputModule from the scene's EventSystem.
        /// </summary>
        private void CacheUIModule() {
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null) {
                cachedUIModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (cachedUIModule == null) {
                    Debug.LogWarning("GameplayManager: InputSystemUIInputModule not found on EventSystem!");
                }
            } else {
                Debug.LogWarning("GameplayManager: EventSystem not found in the scene!");
            }
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start() {
            InitializeForScene();
            _initialized = true;
        }

        /// <summary>
        /// Called every time a new scene is loaded. Skips the first load (handled by Start).
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (!_initialized) return;
            InitializeForScene();
        }

        /// <summary>
        /// Resets stats, clears old heroes, and spawns new ones.
        /// Called on first Start and on every scene reload.
        /// </summary>
        private void InitializeForScene() {
            // Reset stats
            currentXP = 0;
            currentLevel = 1;
            totalKills = 0;
            currentTime = 0f;
            isTimerPaused = false;

            OnXPChanged?.Invoke(currentXP);
            OnLevelUp?.Invoke(currentLevel);
            OnKillCountChanged?.Invoke(totalKills);
            OnTimerUpdated?.Invoke(currentTime);

            // Clear old heroes (they were destroyed on scene reload)
            activeHeroes.Clear();

            // Clear old subscriptions to avoid duplicates (static event)
            HeroController.OnHeroDied -= OnHeroDied;
            HeroController.OnHeroDied += OnHeroDied;

            // Cache the UI Input Module from the scene's EventSystem
            CacheUIModule();

            // Instantiate heroes using the single prefab and the HeroData list
            for (int i = 0; i < heroesData.Count; i++) {
                Transform spawnPos = (i < spawnPoints.Count) ? spawnPoints[i] : null;
                Vector3 position = spawnPos != null ? spawnPos.position : Vector3.zero;

                // Instantiate the base prefab
                HeroController newHero = Instantiate(heroPrefab, position, Quaternion.identity);

                // Inject the specific HeroData from the array
                newHero.SetHeroData(heroesData[i]);

                // Assign the UI Input Module to the hero's PlayerInput
                AssignUIModuleToHero(newHero);

                activeHeroes.Add(newHero);
            }

            if (activeHeroes.Count == 0) Debug.LogError("No heroes were spawned! Check Hero Data list.");

            // --- CINEMACHINE SETUP ---
            if (activeHeroes.Count > 0) {
                if (vcam == null) {
                    vcam = FindFirstObjectByType<CinemachineCamera>();
                }

                if (vcam != null) {
                    vcam.Follow = activeHeroes[0].transform;
                    vcam.LookAt = activeHeroes[0].transform;
                }
            }

            // Instantiate the Pause UI (singleton persists across the session)
            if (pausePanelPrefab != null) {
                if (PauseUIManager.Instance == null) {
                    Instantiate(pausePanelPrefab);
                }
            } else {
                Debug.LogWarning("GameplayManager: pausePanelPrefab is not assigned!");
            }

            // Instantiate the Game Over UI (singleton persists across the session)
            if (gameOverPanelPrefab != null) {
                if (GameOverUIManager.Instance == null) {
                    Instantiate(gameOverPanelPrefab);
                }
            } else {
                Debug.LogWarning("GameplayManager: gameOverPanelPrefab is not assigned!");
            }
        }

        /// <summary>
        /// Assigns the cached UI Input Module to the given hero's PlayerInput component.
        /// </summary>
        private void AssignUIModuleToHero(HeroController hero) {
            if (hero == null) return;

            PlayerInput playerInput = hero.GetComponent<PlayerInput>();
            if (playerInput != null) {
                if (cachedUIModule != null) {
                    playerInput.uiInputModule = cachedUIModule;
                    Debug.Log($"GameplayManager: UI Input Module assigned to {hero.name}");
                } else {
                    Debug.LogWarning($"GameplayManager: Cannot assign UI Input Module to {hero.name} - module is null!");
                }
            } else {
                Debug.LogWarning($"GameplayManager: PlayerInput not found on {hero.name}!");
            }
        }

        /// <summary>
        /// Called when a hero dies. Checks if all heroes are dead and opens the game over menu.
        /// </summary>
        private void OnHeroDied() {
            // Check if ALL heroes are dead
            bool allDead = true;
            foreach (var hero in activeHeroes) {
                if (hero != null && !hero.IsDead) {
                    allDead = false;
                    break;
                }
            }

            if (allDead) {
                // --- Game Center Integration ---
                if (GameCenterManager.Instance != null) {
                    // Fire and forget: send scores asynchronously
                    _ = GameCenterManager.Instance.SubmitScore(currentTime, totalKills);
                } else {
                    Debug.LogWarning("GameplayManager: GameCenterManager instance not found!");
                }

                if (GameOverUIManager.Instance != null) {
                    GameOverUIManager.Instance.OpenGameOverMenu(totalKills, currentTime);
                }
            }
        }

        private void Update() {
            if (!isTimerPaused) {
                currentTime += Time.deltaTime;
                OnTimerUpdated?.Invoke(currentTime);
            }
        }

        /// <summary>
        /// Pauses or resumes the game timer.
        /// </summary>
        public void SetTimerPaused(bool paused) {
            isTimerPaused = paused;
        }

        // XP is shared and added directly to the pool.
        public void AddXP(int amount, HeroController hero) {

            currentXP += amount;
            OnXPChanged?.Invoke(currentXP);

            while (currentXP >= XPToNextLevel) {
                currentXP -= XPToNextLevel;
                currentLevel++;

                // --- LEVEL UP REWARD ROLL ---
                // 1. Randomly select the hero who will receive the reward this time
                HeroController targetHero = activeHeroes[Random.Range(0, activeHeroes.Count)];

                // 2. Generate the options based SOLELY on the target hero's inventory
                if (levelUpPanelPrefab != null) {
                    GameObject panelObj = Instantiate(levelUpPanelPrefab);
                    LevelUpUIManager uiManager = panelObj.GetComponent<LevelUpUIManager>();

                    List<LevelUpOption> options = GenerateChoices(3, targetHero);

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

            if (upgradePoolData == null) {
                Debug.LogError("GameplayManager: upgradePoolData is null! Please assign it in the Inspector.");
                return options;
            }

            // Get available items for this specific hero
            List<Object> availableItems = upgradePoolData.GetAvailableItems(hero);

            if (availableItems.Count == 0) {
                Debug.Log("No available upgrades for hero " + hero.name);
                return options;
            }

            // Shuffle the available items
            List<Object> shuffledPool = availableItems.OrderBy(x => System.Guid.NewGuid()).ToList();


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
                Debug.Log($"GenerateChoices: Adding Skip option (slot {options.Count + 1})");
                options.Add(new LevelUpOption {
                    displayName = "Skip",
                    description = "",
                    icon = null,
                    isWeapon = false,
                    targetLevel = 0,
                    onSelected = () => { Debug.Log("Executing: Skip"); }
                });
            }

            // Log final summary
            for (int i = 0; i < options.Count; i++) {
                var opt = options[i];
                Debug.Log($"  [{i}] {opt.displayName} | isWeapon={opt.isWeapon} | targetLevel={opt.targetLevel}");
            }

            return options;
        }
    }
}