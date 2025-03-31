using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // NEW: Needed for LINQ methods like Shuffle

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI weaponsListText;
    public TextMeshProUGUI statsListText; // Текст для списка глобальных улучшений
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private float gameTime;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    private GameManager gameManager;

    public GameObject pausePanel;
    public GameObject upgradePanel;
    public Button[] upgradeButtons; // Should have 3 buttons assigned in Inspector
    private PlayerHealth playerHealthComponent;

    // NEW: List of all possible weapon names the player can acquire/upgrade
    private readonly List<string> allWeaponNames = new List<string> {
        "Basic Bullet", "Spread Shot", "Laser", "Sword", "Garlic Aura"
    };

    void Start()
    {
        // --- Existing Start logic ---
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>(); // Added null check
        if (gameManager == null) Debug.LogError("GameManager not found!");

        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        upgradePanel.SetActive(false);
        playerHealthComponent = GetComponent<PlayerHealth>(); // Already have playerHealth, maybe rename one?

        // Initial UI updates
        if (playerAttack != null && playerHealth != null)
        {
            UpdateExpBar(playerAttack.experience, playerAttack.expToLevelUp);
            UpdateWeaponsList(playerAttack.activeWeapons);
            UpdateStatsList();
            UpdateHealthText(); // Initial health update
        }
        else
        {
            Debug.LogError("PlayerAttack or PlayerHealth component missing!");
        }
        // --- End of Existing Start logic ---
    }

    void Update()
    {
        // Update health only if component exists
        if (playerHealth != null) UpdateHealthText();

        // Timer and Level updates (assuming playerAttack exists)
        if (playerAttack != null)
        {
            gameTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            levelText.text = "Level: " + playerAttack.level;
        }

        // Pause handling
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if panels are active *before* checking timescale
            bool isPanelOpen = pausePanel.activeSelf || upgradePanel.activeSelf || gameOverPanel.activeSelf;

            if (Time.timeScale == 0 && !isPanelOpen) // Should not happen often if panels control timescale
            {
                ResumeGame(); // Resume if paused without a panel? Unlikely case.
            }
            else if (Time.timeScale == 0 && pausePanel.activeSelf) // If paused via pause panel
            {
                ResumeGame();
            }
            else if (Time.timeScale == 1 && !isPanelOpen) // If game is running and no panels are open
            {
                PauseGame();
            }
            // Note: Pressing Esc while Upgrade or Game Over panel is open does nothing here.
        }
    }

    // NEW: Helper method for updating health text
    void UpdateHealthText()
    {
        healthText.text = "Health: " + playerHealth.currentHealth + " / " + playerHealth.maxHealth;
    }


    public void UpdateExpBar(int currentExp, int expToLevelUp)
    {
        if (expToLevelUp <= 0) return; // Avoid division by zero
        float expFraction = Mathf.Clamp01((float)currentExp / expToLevelUp); // Use Clamp01
        expSlider.value = expFraction;
        expText.text = $"EXP: {currentExp} / {expToLevelUp}";
    }

    public void UpdateWeaponsList(Dictionary<string, int> activeWeapons)
    {
        string weaponsList = "Weapons:\n";
        if (activeWeapons == null || activeWeapons.Count == 0)
        {
            weaponsList += "- None -";
        }
        else
        {
            foreach (var weapon in activeWeapons)
            {
                // Проверяем, что playerAttack не null перед вызовом IsWeaponMaxed
                bool isMaxed = (playerAttack != null) ? playerAttack.IsWeaponMaxed(weapon.Key) : false;
                weaponsList += $"- {weapon.Key} (Level {weapon.Value}{(isMaxed ? " MAX" : "")})\n";
            }
        }
        weaponsListText.text = weaponsList;
    }

    public void UpdateStatsList()
    {
        if (playerAttack == null) return; // Safety check

        string statsList = "Stats:\n";
        // Use formatting for percentages
        statsList += $"- Damage: +{Mathf.RoundToInt((playerAttack.GetDamageMultiplier() - 1f) * 100)}%\n";
        statsList += $"- Attack Speed: +{Mathf.RoundToInt((1f / playerAttack.GetAttackRateMultiplier() - 1f) * 100)}%\n"; // More intuitive speed display
        // Add other stats here if needed (e.g., Move Speed, Pickup Radius...)
        statsListText.text = statsList;
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        gameOverText.text = $"Game Over!\nTime Survived: {minutes:00}:{seconds:00}";

        // Save best time (remains the same)
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        if (gameTime > bestTime)
        {
            PlayerPrefs.SetFloat("BestTime", gameTime);
            PlayerPrefs.Save();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        upgradePanel.SetActive(false); // Ensure upgrade panel also closes
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened (Not Implemented)");
        // Implementation would likely involve another panel
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1; // Ensure time scale is reset before loading new scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Make sure "MainMenu" scene exists
    }

    public void ShowUpgradePanel()
    {
        Time.timeScale = 0; // Pause game
        upgradePanel.SetActive(true);
        SetupUpgradeButtons(); // Populate buttons with options
    }

    // MODIFIED: Reworked logic for selecting upgrades
    void SetupUpgradeButtons()
    {
        if (upgradeButtons == null || upgradeButtons.Length == 0)
        {
            Debug.LogError("Upgrade buttons array is not set up in Inspector!");
            upgradePanel.SetActive(false); // Hide panel if buttons missing
            Time.timeScale = 1; // Resume game
            return;
        }
        if (playerAttack == null || playerHealthComponent == null)
        {
            Debug.LogError("PlayerAttack or PlayerHealth component missing, cannot set up upgrades!");
            upgradePanel.SetActive(false);
            Time.timeScale = 1;
            return;
        }


        List<string> possibleUpgradeOptions = new List<string>();

        // 1. Add Stat Upgrade Options
        possibleUpgradeOptions.Add("Attack Speed +10%"); // String identifier for ApplyUpgrade
        possibleUpgradeOptions.Add("Damage +10%");      // String identifier for ApplyUpgrade
        possibleUpgradeOptions.Add("Max Health +20");   // String identifier for ApplyUpgrade
        // Add more stat upgrades here if desired (e.g., "Movement Speed +5%", "Pickup Radius +10%")

        // 2. Add Weapon Add/Upgrade Options
        bool canAddMoreWeapons = playerAttack.activeWeapons.Count < 5; // Check if player can hold more weapons (using hardcoded 5 based on PlayerAttack)

        foreach (string weaponName in allWeaponNames)
        {
            if (playerAttack.activeWeapons.ContainsKey(weaponName))
            {
                // Player has the weapon, check if it can be upgraded
                if (!playerAttack.IsWeaponMaxed(weaponName))
                {
                    int nextLevel = playerAttack.activeWeapons[weaponName] + 1;
                    possibleUpgradeOptions.Add($"Upgrade {weaponName}"); // Use base string for ApplyUpgrade logic
                }
                // If maxed, do not add upgrade option
            }
            else
            {
                // Player does not have the weapon, check if they can add it
                if (canAddMoreWeapons)
                {
                    possibleUpgradeOptions.Add($"Add {weaponName}"); // Use base string for ApplyUpgrade logic
                }
            }
        }

        // 3. Shuffle and Select Top Options (up to the number of buttons)
        System.Random rng = new System.Random();
        List<string> selectedUpgrades = possibleUpgradeOptions.OrderBy(x => rng.Next()).Take(upgradeButtons.Length).ToList();

        // 4. Configure Buttons
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            Button button = upgradeButtons[i];
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (button == null || buttonText == null)
            {
                Debug.LogError($"UpgradeButton {i} or its TextMeshProUGUI component is not assigned or found!");
                continue; // Skip this button if invalid
            }

            button.onClick.RemoveAllListeners(); // Clear previous listeners

            if (i < selectedUpgrades.Count)
            {
                string upgradeId = selectedUpgrades[i]; // The base ID like "Upgrade Garlic Aura" or "Add Laser"
                string displayText = upgradeId; // Start with the ID as default text

                // Customize display text for weapon upgrades
                if (upgradeId.StartsWith("Upgrade "))
                {
                    string weaponName = upgradeId.Substring("Upgrade ".Length);
                    if (playerAttack.activeWeapons.ContainsKey(weaponName)) // Check just in case
                    {
                        int nextLevel = playerAttack.activeWeapons[weaponName] + 1;
                        displayText = $"Upgrade {weaponName} (Lvl {nextLevel})";
                    }
                }
                else if (upgradeId.StartsWith("Add "))
                {
                    // Display text is already fine ("Add [WeaponName]")
                }
                // Add display text customization for stats if needed (e.g., showing current value)


                buttonText.text = displayText;
                button.gameObject.SetActive(true); // Make sure button is visible
                // Add listener using the base upgradeId
                button.onClick.AddListener(() => ApplyUpgrade(upgradeId));
            }
            else
            {
                // Not enough unique upgrades available, hide extra buttons
                buttonText.text = ""; // Clear text
                button.gameObject.SetActive(false); // Hide button
            }
        }
    }

    // MODIFIED: Reworked logic to handle specific upgrade strings
    void ApplyUpgrade(string upgradeId)
    {
        Debug.Log($"Applying upgrade: {upgradeId}");

        if (upgradeId.StartsWith("Upgrade ") || upgradeId.StartsWith("Add "))
        {
            // Handle Weapon Add/Upgrade
            string weaponName;
            if (upgradeId.StartsWith("Upgrade "))
            {
                weaponName = upgradeId.Substring("Upgrade ".Length);
            }
            else
            { // Starts with "Add "
                weaponName = upgradeId.Substring("Add ".Length);
            }

            // Check if the weapon name is valid before calling UpgradeWeapon
            if (allWeaponNames.Contains(weaponName))
            {
                playerAttack.UpgradeWeapon(weaponName); // PlayerAttack handles both adding and upgrading
            }
            else
            {
                Debug.LogError($"Invalid weapon name extracted from upgrade ID: {weaponName}");
            }
        }
        else
        {
            // Handle Stat Upgrades
            switch (upgradeId)
            {
                case "Attack Speed +10%":
                    playerAttack.UpgradeStat("AttackRate", 0.1f); // PlayerAttack handles the multiplier logic
                    break;
                case "Damage +10%":
                    playerAttack.UpgradeStat("Damage", 0.1f); // Apply 10% increase to multiplier
                    break;
                case "Max Health +20":
                    if (playerHealthComponent != null)
                    {
                        //playerHealthComponent.IncreaseMaxHealth(20); // Use a method if available, or modify directly
                         playerHealthComponent.maxHealth += 20;
                        //playerHealthComponent.Heal(20); // Heal by the increased amount
                        Debug.Log("Max Health increased!");
                    }
                    break;
                // Add cases for other stat upgrades here
                default:
                    Debug.LogWarning($"Unknown upgrade ID: {upgradeId}");
                    break;
            }
        }

        // Resume game and update UI after applying upgrade
        upgradePanel.SetActive(false); // Hide the panel
        Time.timeScale = 1; // Resume game
        UpdateWeaponsList(playerAttack.activeWeapons); // Update weapon list UI
        UpdateStatsList(); // Update stats list UI
        UpdateHealthText(); // Update health UI in case Max Health changed
    }
}
//```

//**Ключевые изменения: **

//1.  * *`allWeaponNames`:**Добавлен список `allWeaponNames`, содержащий имена всех возможных оружий, включая "Garlic Aura".
//2.  **`SetupUpgradeButtons` (Переработан):**
//    *Теперь метод сначала собирает *все* возможные варианты улучшений (`possibleUpgradeOptions`), включая:
//        *Стандартные улучшения статов("Attack Speed +10%", "Damage +10%", "Max Health +20").
//        *Опции для* каждого* оружия из `allWeaponNames`:
//            *Если оружие есть и не максимального уровня -> добавляется опция `"Upgrade [Имя Оружия]"`.
//            * Если оружия нет и есть свободные слоты -> добавляется опция `"Add [Имя Оружия]"`.
//    * Затем этот список перемешивается (`OrderBy(x => rng.Next())`) и выбираются первые N опций (где N - количество кнопок).
//    * При настройке текста кнопки:
//        *Для опций "Upgrade" добавляется информация о следующем уровне (например, "Upgrade Garlic Aura (Lvl 2)").
//        * Для опций "Add" текст остается простым ("Add Garlic Aura").
//    * Кнопкам назначается обработчик `ApplyUpgrade`, который получает базовый идентификатор улучшения (например, `"Upgrade Garlic Aura"` или `"Add Garlic Aura"`).
//3.  **`ApplyUpgrade` (Переработан):**
//    *Теперь метод проверяет, начинается ли строка `upgradeId` с `"Upgrade "` или `"Add "`.
//    * Извлекает имя оружия из строки.
//    * Вызывает `playerAttack.UpgradeWeapon(weaponName)`, который сам разберется, добавить новое оружие или улучшить существующее.
//    * Логика для улучшения статов осталась в `switch`, но теперь использует строки типа `"Damage +10%"`.
//4.  **Мелкие улучшения:**Добавлены проверки на null, улучшено отображение скорости атаки в `UpdateStatsList`, обновляется `maxHealth` в тексте здоровья.

//**Что нужно проверить:**

//*Убедитесь, что в инспекторе для объекта игрока в компоненте `PlayerUI` в массив `Upgrade Buttons` перетащены все 3 кнопки с панели улучшений.
//* Убедитесь, что максимальное количество оружия в `PlayerAttack` (переменная `maxWeapons`) соответствует вашим ожиданиям (в коде `PlayerAttack` это было 5).
//* Протестируйте процесс повышения уровня: должны предлагаться как добавление "Garlic Aura", так и его улучшение после добавления, пока не будет достигнут максимальный урове