using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI bestTimeText;

    void Start()
    {
        // Загружаем лучшее время из PlayerPrefs
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        int minutes = Mathf.FloorToInt(bestTime / 60);
        int seconds = Mathf.FloorToInt(bestTime % 60);
        bestTimeText.text = $"Best Time: {minutes:00}:{seconds:00}";
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene"); // Загружаем игровую сцену
    }

    public void ExitGame()
    {
        Application.Quit(); // Выход из игры
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened (пока пусто)");
        // Здесь позже можно добавить настройки
    }
}