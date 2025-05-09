using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGate : MonoBehaviour
{
    [Header("Gate Settings")]
    public int requiredKeyID = 1;
    public int targetLevel = 2;
    public float transitionDelay = 1.5f;
    
    [Header("Effects")]
    public GameObject gateOpenEffect;
    public AudioClip gateOpenSound;
    public GameObject invalidAttemptEffect;
    public AudioClip invalidAttemptSound;
    
    [Header("References")]
    public Animator gateAnimator; // Optional: If you have a gate animation
    public string openAnimationTrigger = "Open";
    
    private bool isOpen = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set up animator reference if not assigned
        if (gateAnimator == null)
        {
            gateAnimator = GetComponent<Animator>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            // Check if player has the required key
            KeyManager keyManager = other.GetComponent<KeyManager>();
            if (keyManager == null)
            {
                keyManager = other.GetComponentInParent<KeyManager>();
            }
            
            if (keyManager != null && keyManager.HasKey(requiredKeyID))
            {
                OpenGate(other.gameObject);
            }
            else
            {
                // Player doesn't have the key
                ShowInvalidAttempt();
            }
        }
    }
    
    void OpenGate(GameObject player)
    {
        isOpen = true;
        Debug.Log($"🚪 Opening gate to level {targetLevel}");
        
        // Play effects
        if (gateOpenEffect != null)
        {
            Instantiate(gateOpenEffect, transform.position, Quaternion.identity);
        }
        
        if (audioSource != null && gateOpenSound != null)
        {
            audioSource.PlayOneShot(gateOpenSound);
        }
        
        // Play animation if available
        if (gateAnimator != null)
        {
            gateAnimator.SetTrigger(openAnimationTrigger);
        }
        
        // Load next level after delay
        StartCoroutine(LoadNextLevelAfterDelay(player));
    }
    
    void ShowInvalidAttempt()
    {
        Debug.Log("🔒 Cannot open gate - missing required key");
        
        // Play effects
        if (invalidAttemptEffect != null)
        {
            Instantiate(invalidAttemptEffect, transform.position, Quaternion.identity);
        }
        
        if (audioSource != null && invalidAttemptSound != null)
        {
            audioSource.PlayOneShot(invalidAttemptSound);
        }
    }
    
    IEnumerator LoadNextLevelAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(transitionDelay);
        
        // Find the level manager
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.LoadLevel(targetLevel);
        }
        else
        {
            Debug.LogError("❌ LevelManager not found in scene!");
        }
    }
}