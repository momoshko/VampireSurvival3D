using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public Button[] upgradeButtons;
    private PlayerHealth playerHealthComponent;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        upgradePanel.SetActive(false);
        playerHealthComponent = GetComponent<PlayerHealth>();
        UpdateExpBar(playerAttack.experience, playerAttack.expToLevelUp);
        UpdateWeaponsList(playerAttack.activeWeapons);
        UpdateStatsList(); // Инициализируем список улучшений
    }

    void Update()
    {
        healthText.text = "Health: " + playerHealth.currentHealth;
        gameTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
        levelText.text = "Level: " + playerAttack.level;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0 && !gameOverPanel.activeSelf && !upgradePanel.activeSelf)
            {
                ResumeGame();
            }
            else if (!gameOverPanel.activeSelf && !upgradePanel.activeSelf)
            {
                PauseGame();
            }
        }
    }

    public void UpdateExpBar(int currentExp, int expToLevelUp)
    {
        float expFraction = (float)currentExp / expToLevelUp;
        expSlider.value = expFraction;
        expText.text = $"EXP: {currentExp} / {expToLevelUp}";
    }

    public void UpdateWeaponsList(Dictionary<string, int> activeWeapons)
    {
        string weaponsList = "Weapons:\n";
        foreach (var weapon in activeWeapons)
        {
            weaponsList += $"{weapon.Key} (Level {weapon.Value})\n";
        }
        weaponsListText.text = weaponsList;
    }

    public void UpdateStatsList()
    {
        string statsList = "Stats:\n";
        statsList += $"Damage: +{Mathf.RoundToInt((playerAttack.GetDamageMultiplier() - 1) * 100)}%\n";
        statsList += $"Attack Speed: +{Mathf.RoundToInt((1 - playerAttack.GetAttackRateMultiplier()) * 100)}%\n";
        statsListText.text = statsList;
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        gameOverText.text = $"Game Over!\nTime Survived: {minutes:00}:{seconds:00}";

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
        upgradePanel.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened (пока пусто)");
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ShowUpgradePanel()
    {
        Time.timeScale = 0;
        upgradePanel.SetActive(true);
        SetupUpgradeButtons();
    }

    void SetupUpgradeButtons()
    {
        if (upgradeButtons == null || upgradeButtons.Length != 3)
        {
            Debug.LogError("Upgrade buttons array is not properly set up in Inspector!");
            return;
        }

        List<string> upgrades = new List<string> {
            "Attack Speed +10%",
            "Max Health +20",
            "Bullet Damage +1",
            "Upgrade Spread Shot",
            "Upgrade Laser",
            "Upgrade Sword"
        };

        upgrades.RemoveAll(upgrade =>
            (upgrade == "Upgrade Spread Shot" && playerAttack.IsWeaponMaxed("Spread Shot")) ||
            (upgrade == "Upgrade Laser" && playerAttack.IsWeaponMaxed("Laser")) ||
            (upgrade == "Upgrade Sword" && playerAttack.IsWeaponMaxed("Sword")));

        List<string> selectedUpgrades = new List<string>();
        while (selectedUpgrades.Count < Mathf.Min(3, upgrades.Count) && upgrades.Count > 0)
        {
            int index = Random.Range(0, upgrades.Count);
            selectedUpgrades.Add(upgrades[index]);
            upgrades.RemoveAt(index);
        }

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] == null)
            {
                Debug.LogError($"UpgradeButton{i + 1} is not assigned!");
                continue;
            }

            string upgrade = i < selectedUpgrades.Count ? selectedUpgrades[i] : "No Upgrade Available";
            TextMeshProUGUI buttonText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = upgrade;
            }
            else
            {
                Debug.LogError($"No TextMeshProUGUI found in UpgradeButton{i + 1}!");
            }

            int index = i;
            upgradeButtons[i].onClick.RemoveAllListeners();
            if (i < selectedUpgrades.Count)
            {
                upgradeButtons[i].onClick.AddListener(() => ApplyUpgrade(selectedUpgrades[index]));
            }
        }
    }

    void ApplyUpgrade(string upgrade)
    {
        switch (upgrade)
        {
            case "Attack Speed +10%":
                playerAttack.UpgradeStat("AttackRate", 0.1f);
                break;
            case "Max Health +20":
                playerHealthComponent.maxHealth += 20;
                playerHealthComponent.currentHealth += 20;
                Debug.Log("Max Health increased!");
                break;
            case "Bullet Damage +1":
                playerAttack.UpgradeStat("Damage", 0.1f);
                break;
            case "Upgrade Spread Shot":
                playerAttack.UpgradeWeapon("Spread Shot");
                Debug.Log("Spread Shot upgraded or added!");
                break;
            case "Upgrade Laser":
                playerAttack.UpgradeWeapon("Laser");
                Debug.Log("Laser upgraded or added!");
                break;
            case "Upgrade Sword":
                playerAttack.UpgradeWeapon("Sword");
                Debug.Log("Sword upgraded or added!");
                break;
        }

        Time.timeScale = 1;
        upgradePanel.SetActive(false);
        UpdateStatsList(); // Обновляем список улучшений после апгрейда
    }
}