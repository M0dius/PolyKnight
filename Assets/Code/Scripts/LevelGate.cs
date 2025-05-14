using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGate : MonoBehaviour
{
    [Header("Gate Settings")]
    public int requiredKeyID = 1;
    public string nextSceneName = "Level_2";
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
            bool hasKey = PlayerPrefs.GetInt($"HasKey_Level_{requiredKeyID}", 0) == 1;
            
            if (hasKey)
            {
                Debug.Log($"Player has key {requiredKeyID}! Opening gate.");
                OpenGate();
            }
            else
            {
                Debug.Log($"Player doesn't have key {requiredKeyID}");
                ShowInvalidAttempt();
            }
        }
    }
    
    void OpenGate()
    {
        isOpen = true;
        Debug.Log($"🚪 Opening gate to scene {nextSceneName}");
        
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
        
        // Load next scene after delay
        StartCoroutine(LoadNextSceneAfterDelay());
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
    
    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(transitionDelay);
        
        // Save the current scene as last completed level
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Contains("Level_"))
        {
            string levelNumber = currentScene.Replace("Level_", "");
            PlayerPrefs.SetInt("LastCompletedLevel", int.Parse(levelNumber));
            PlayerPrefs.Save();
        }
        
        Debug.Log($"Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}