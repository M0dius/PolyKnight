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
    
    void Awake()
    {
        // Singleton pattern - ensure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayerDied()
    {
        Debug.Log("Player died! Resetting to Level 1");
        
        // Clear all key progress when player dies
        ClearAllKeys();
        
        // Reset last completed level
        PlayerPrefs.SetInt("LastCompletedLevel", 0);
        PlayerPrefs.Save();
        
        // Load first level
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
}