using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 1; // How much this coin is worth
    public float magnetRange = 2f; // Range for magnetic attraction to player
    public float magnetForce = 5f; // How fast it moves toward player
    public bool autoCollect = false; // Auto-collect when in range
    
    [Header("Animation Settings")]
    public float rotationSpeed = 180f; // Degrees per second
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public AnimationCurve bobCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    public float collectVolume = 1f;
    
    [Header("Despawn Settings")]
    public bool despawnAfterTime = true;
    public float despawnTime = 30f; // Seconds before despawn
    public float despawnBlinkTime = 5f; // Start blinking before despawn
    
    private Vector3 startPosition;
    private float bobTimer;
    private Transform player;
    private AudioSource audioSource;
    private bool isCollected = false;
    private Renderer coinRenderer;
    private float spawnTime;
    
    void Start()
    {
        startPosition = transform.position;
        bobTimer = Random.Range(0f, Mathf.PI * 2f); // Random starting bob phase
        
        // Set up components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        coinRenderer = GetComponent<Renderer>();
        if (coinRenderer == null)
        {
            coinRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Find player
        FindPlayer();
        
        // Set up trigger collider if missing
        SetupCollider();
        
        // Start despawn timer
        spawnTime = Time.time;
        if (despawnAfterTime)
        {
            StartCoroutine(DespawnTimer());
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Bob animation
        DoBobAnimation();
        
        // Rotate coin
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        
        // Check for player in range
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            
            // Magnetic attraction
            if (distance <= magnetRange)
            {
                MoveTowardPlayer();
                
                // Auto-collect if enabled
                if (autoCollect)
                {
                    CollectCoin();
                }
            }
        }
    }
    
    void DoBobAnimation()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = bobCurve.Evaluate((Mathf.Sin(bobTimer) + 1f) * 0.5f) * bobHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + bobOffset, startPosition.z);
    }
    
    void MoveTowardPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * magnetForce * Time.deltaTime;
        startPosition = transform.position; // Update start position for bobbing
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.5f;
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
    
    void FindPlayer()
    {
        // Try multiple methods to find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            return;
        }
        
        // Try finding by component
        var playerComponent = FindObjectOfType<Player>();
        if (playerComponent != null)
        {
            player = playerComponent.transform;
            return;
        }
        
        // Try finding by character controller
        var characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
        {
            player = characterController.transform;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectCoin();
        }
    }
    
    public void CollectCoin()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        Debug.Log($"Coin collected! Value: {coinValue}");
        
        // Add coin to coin manager
        CoinManager coinManager = FindObjectOfType<CoinManager>();
        if (coinManager != null)
        {
            coinManager.AddCoins(coinValue);
        }
        else
        {
            // Fallback: use PlayerPrefs
            int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            PlayerPrefs.SetInt("TotalCoins", currentCoins + coinValue);
            PlayerPrefs.Save();
        }
        
        // Play effects
        PlayCollectEffects();
        
        // Destroy coin
        Destroy(gameObject);
    }
    
    void PlayCollectEffects()
    {
        // Play collection effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Play collection sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.volume = collectVolume;
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
        }
    }
    
    IEnumerator DespawnTimer()
    {
        // Wait for despawn time minus blink time
        yield return new WaitForSeconds(despawnTime - despawnBlinkTime);
        
        // Start blinking
        if (coinRenderer != null)
        {
            float blinkInterval = 0.2f;
            float blinkTimer = 0f;
            
            while (blinkTimer < despawnBlinkTime)
            {
                coinRenderer.enabled = !coinRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
                blinkTimer += blinkInterval;
            }
        }
        
        // Destroy coin if not collected
        if (!isCollected)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw magnet range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}