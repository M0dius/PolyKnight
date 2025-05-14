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
    public string firstLevelName = "Level/Scenes/Level_1";

    private bool isDead = false; // Track death state

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
            playerHealth.OnPlayerDeath += HandlePlayerDeath; // Use a new method
        }
        else
        {
            Debug.LogError("PlayerHealth script not assigned in the Death script!");
        }
    }

    // New method to handle player death
    private void HandlePlayerDeath()
    {
        if (!isDead) // Only do this once
        {
            isDead = true;
            ShowDeathScreen();
            StartCoroutine(UnlockCursorDelayed()); // Unlock cursor with delay
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
        isDead = false; // Reset death state
    }

    public void GoToMainMenu(string mainMenuSceneName = "Level/Scenes/MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
        isDead = false; // Reset death state
    }

    // Coroutine to unlock cursor after a delay
    private IEnumerator UnlockCursorDelayed()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Short delay, independent of timeScale
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
