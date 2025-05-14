using UnityEngine.SceneManagement; // Add this line

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathScreenUI; // Assign your DeathCanvas GameObject here

    [Header("Player Health Reference")]
    public PlayerHealth playerHealth; // Assign your Player GameObject with the PlayerHealth script here

    void Start()
    {
        // Ensure the death screen UI is initially hidden
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Death Screen UI GameObject not assigned in the Death script!");
        }

        // Subscribe to the player's death event
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += ShowDeathScreen;
        }
        else
        {
            Debug.LogError("PlayerHealth script not assigned in the Death script!");
        }
    }

    // This method will be called when the player dies
    private void ShowDeathScreen()
    {
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
            // Optional: Pause the game time when the death screen appears
            Time.timeScale = 0f;
        }
    }

    // Called when the "Restart" button is clicked
    public void RestartGame()
    {
        // Resume game time
        Time.timeScale = 1f;
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called when the "Main Menu" button is clicked
    public void GoToMainMenu(string mainMenuSceneName = "MainMenu") // Set your main menu scene name here
    {
        // Resume game time
        Time.timeScale = 1f;
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
