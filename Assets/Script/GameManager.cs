using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1; // Возвращаем время
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Перезагружаем сцену
    }
}