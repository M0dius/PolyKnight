using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevels = 6;
    public bool useSceneLoading = false; // Set to true if levels are separate scenes
    
    [Header("Level Objects")]
    public List<GameObject> levelObjects = new List<GameObject>(); // Level prefabs/objects
    
    [Header("Player References")]
    public Transform playerSpawnPoint;
    public GameObject playerPrefab; // Only needed if instantiating player
    
    [Header("Effects")]
    public GameObject levelTransitionEffect;
    public AudioClip levelTransitionSound;
    
    private GameObject currentLevelObject;
    private AudioSource audioSource;
    private GameObject player;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Load the initial level
        LoadLevel(currentLevel);
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > maxLevels)
        {
            Debug.LogError($"❌ Invalid level index: {levelIndex}");
            return;
        }
        
        currentLevel = levelIndex;
        Debug.Log($"🎮 Loading level {currentLevel}");
        
        // Play transition effects
        if (levelTransitionEffect != null)
        {
            Instantiate(levelTransitionEffect, Vector3.zero, Quaternion.identity);
        }
        
        if (audioSource != null && levelTransitionSound != null)
        {
            audioSource.PlayOneShot(levelTransitionSound);
        }
        
        // Handle level loading based on method
        if (useSceneLoading)
        {
            // Load level as a scene
            SceneManager.LoadScene($"Level_{levelIndex}");
        }
        else
        {
            // Load level as GameObject
            LoadLevelAsGameObject(levelIndex);
        }
    }
    
    private void LoadLevelAsGameObject(int levelIndex)
    {
        // Deactivate current level if it exists
        if (currentLevelObject != null)
        {
            Destroy(currentLevelObject);
        }
        
        // Check if we have a prefab for this level
        if (levelObjects.Count >= levelIndex && levelObjects[levelIndex - 1] != null)
        {
            // Instantiate new level
            currentLevelObject = Instantiate(levelObjects[levelIndex - 1]);
            
            // Move player to spawn point if available
            if (player != null)
            {
                // Find spawn point in the new level
                Transform newSpawnPoint = currentLevelObject.transform.Find("PlayerSpawn");
                if (newSpawnPoint != null)
                {
                    // Move player to spawn point
                    player.transform.position = newSpawnPoint.position;
                    player.transform.rotation = newSpawnPoint.rotation;
                    
                    // Reset player health
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.ResetHealth();
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ No spawn point found in level prefab!");
                }
            }
            else
            {
                Debug.LogError("❌ Player not found!");
            }
        }
        else
        {
            Debug.LogError($"❌ Level prefab for level {levelIndex} not assigned!");
        }
    }
    
    public void PlayerDied()
    {
        Debug.Log("☠️ Player died! Returning to level 1");
        
        // Return to level 1
        LoadLevel(1);
        
        // Clear player's keys
        if (player != null)
        {
            KeyManager keyManager = player.GetComponent<KeyManager>();
            if (keyManager != null)
            {
                keyManager.ClearAllKeys();
            }
        }
    }
}