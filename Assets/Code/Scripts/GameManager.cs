using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int totalLevels = 6;
    public string firstSceneName = "Level_1";

    private bool isReturningFromSettings = false; // Flag to track returning from settings

    void Awake()
    {
        // Singleton pattern - ensure only one GameManager exists
        if (Instance == null)
        {
            Debug.Log("GameManager Awake: Instance is null, setting Instance = this (" + gameObject.name + ")");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("GameManager Awake: Instance already exists (" + Instance.gameObject.name + "), destroying this object (" + gameObject.name + ")");
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isReturningFromSettings)
        {
            isReturningFromSettings = false; // Reset the flag
        }
    }

    public void PlayerDied()
    {
        Debug.Log("Player died! Resetting to Level 1");
        ClearAllKeys();
        PlayerPrefs.SetInt("LastCompletedLevel", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(firstSceneName);
    }

    public void ClearAllKeys()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.DeleteKey($"HasKey_Level_{i}");
        }
        PlayerPrefs.Save();
        Debug.Log("All keys cleared");
    }

    public bool HasKey(int keyID)
    {
        return PlayerPrefs.GetInt($"HasKey_Level_{keyID}", 0) == 1;
    }

    public int GetLastCompletedLevel()
    {
        return PlayerPrefs.GetInt("LastCompletedLevel", 0);
    }

    public void SetReturningFromSettings()
    {
        isReturningFromSettings = true;
    }
}