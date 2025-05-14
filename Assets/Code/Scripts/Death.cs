using UnityEngine.SceneManagement; // Add this line

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathScreenUI;

    [Header("Player Health Reference")]
    public PlayerHealth playerHealth;

    [Header("First Level Name")]
    public string firstLevelName = "Level/Scenes/Level_1";  // Changed to the correct scene name

    void Start()
    {
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Death Screen UI GameObject not assigned in the Death script!");
        }

        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += ShowDeathScreen;
        }
        else
        {
            Debug.LogError("PlayerHealth script not assigned in the Death script!");
        }
    }

    private void ShowDeathScreen()
    {
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelName);
    }

    public void GoToMainMenu(string mainMenuSceneName = "Level/Scenes/MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
