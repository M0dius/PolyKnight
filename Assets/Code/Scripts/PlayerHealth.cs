using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public Health healthBar;

    [Header("Effects")]
    public GameObject damageEffect;
    public AudioClip damageSound;

    private AudioSource audioSource;
    private bool isInvulnerable = false;
    private float invulnerabilityTime = 0.5f;

    // Public event that other scripts can subscribe to
    public event Action OnPlayerDeath;

    AudioManager audioManager;

    public void Awake()
    {
        try
        {
            // Try to find AudioManager using the singleton pattern first
            audioManager = AudioManager.Instance;
            
            // If that fails, try to find by tag
            if (audioManager == null)
            {
                GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
                if (audioObj != null)
                    audioManager = audioObj.GetComponent<AudioManager>();
            }
            
            if (audioManager == null)
                Debug.LogWarning("AudioManager not found for PlayerHealth - audio features will be disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error finding AudioManager: " + e.Message + " - audio features will be disabled");
        }
    }

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        Debug.Log("[PlayerHealth] Starting health: " + currentHealth);

        // Try to find health bar if not assigned
        if (healthBar == null)
        {
            // Look for Health component in the scene
            healthBar = FindObjectOfType<Health>();
            if (healthBar != null)
            {
                Debug.Log("[PlayerHealth] Found health bar in scene");
            }
        }

        // Set up the health bar if available
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
        else
        {
            Debug.LogWarning("Health bar UI not assigned to PlayerHealth script!");
        }

        // Get audio source for damage sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && damageSound != null)
        {
            // Add audio source if needed
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log("[PlayerHealth] TakeDamage called with damage: " + damageAmount);

        // If player is invulnerable, don't take damage
        if (isInvulnerable)
        {
            Debug.Log("[PlayerHealth] Player is invulnerable, ignoring damage");
            return;
        }

        // Convert float damage to int
        int damage = Mathf.RoundToInt(damageAmount);

        // Apply damage and ensure health doesn't go below 0
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Prevent negative health
        Debug.Log($"[PlayerHealth] Player took {damage} damage! Current health: {currentHealth}");

        // Update health bar
        if (healthBar != null)
        {
            Debug.Log("[PlayerHealth] Updating health bar to: " + currentHealth);
            healthBar.SetHealth(currentHealth);
        }
        else
        {
            Debug.LogError("[PlayerHealth] Health bar reference is missing!");
        }

        // Play damage effect if available
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }

        // Play damage sound if available
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Make player briefly invulnerable
        StartCoroutine(InvulnerabilityPeriod());

        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    void Die()
    {
        Debug.Log("Player died!");

        // Invoke the OnPlayerDeath event if there are any subscribers
        OnPlayerDeath?.Invoke();

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.playerDeath);

        // For now, just disable the player GameObject
        gameObject.SetActive(false);

        // If you want the GameManager to handle scene reloading later,
        // you can uncomment this section. For now, we're focusing on
        // the death screen appearing.
        //
        // // Notify the GameManager
        // GameManager gameManager = FindObjectOfType<GameManager>();
        // if (gameManager != null)
        // {
        //     gameManager.PlayerDied();
        // }
        // else
        // {
        //     Debug.LogError("❌ GameManager not found!");
        //     // As a fallback, restart the current scene
        //     UnityEngine.SceneManagement.SceneManager.LoadScene("Level_1");
        // }
    }

    // Optional: Add health pickup method
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }
}
