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
    public ScreenFader screenFader; // Reference to the ScreenFader

    private bool isOpen = false;
    private AudioSource audioSource;

    AudioManager audioManager;

    public void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

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

        // Find the ScreenFader.  Important
        screenFader = FindObjectOfType<ScreenFader>();
        if (screenFader == null)
        {
            Debug.LogError("ScreenFader not found in the scene!  Make sure you have a FaderCanvas with ScreenFader on it.");
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
                audioManager.PlaySFX(audioManager.doorTouch);
                audioManager.PlaySFX(audioManager.Transition);
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

        // Start the fade out and then load the scene
        if (screenFader != null)
        {
            screenFader.StartFadeOut();
            StartCoroutine(LoadNextSceneAfterDelay()); //modified
        }
        else
        {
            SceneManager.LoadScene(nextSceneName); //load scene if screenFader is missing
        }



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
        // Wait for fade out to complete
        if (screenFader != null)
        {
            while (screenFader.IsFading())
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(transitionDelay); // Add a delay, so the player can see

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