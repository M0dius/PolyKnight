using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Needed for Slider


public class GoblinAI : MonoBehaviour
{
    [Header("Effects")]
    public ParticleSystem deathEffectPrefab;
    public Transform bloodSpawnPoint;
    [Tooltip("Rotation offset for the blood splatter effect")]
    public Vector3 bloodRotationOffset = new Vector3(-90, 0, 0); // Default for floor-oriented splatter
    
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Goblin Stats")]
   
    public float maxHealth = 30f;
    private float health;
    public float damage = 7f;
    public float detectionRange = 50f; // Increased detection range
    public float attackRange = 1.5f;
    public float attackCooldown = 2f; // Increased to make attacks more visible

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 120f;
    public float acceleration = 8f;

    public Slider healthSlider;
    public Transform healthUI;


    [Header("Coin Drop Settings")]
public GameObject coinPrefab;
public int minCoins = 1;
public int maxCoins = 3;
public float coinDropForce = 3f;
public float coinSpreadRadius = 1f;
public bool alwaysDropCoins = true;
public float coinDropChance = 1f; // 1f = 100% chanc


    [Header("Key Drop Settings")]
public bool isLastEnemy = false; // Set this to true for the last goblin in each level
public GameObject keyPrefab;
public bool useAutoLastEnemy = true; // Automatically determine if this is the last enemy
    
    // Public reference to player that can be set in inspector
    public Transform playerDirectReference;
    
    // Manual player position (use this instead of trying to find player)
    [Header("Manual Player Settings")]
    public Vector3 manualPlayerPosition = new Vector3(-8.582f, 1.15f, -1.0f);
    public bool useManualPosition = false; // Set to false to follow the actual player

    public enum PositionType { Defined, Random }
    public PositionType positionType = PositionType.Defined;

    [Header("Position Settings")]
    public Vector3 areaCenter;
    public Vector3 areaSize;
    public float minDistanceFromOtherEnemies = 2f;
    public float minDistanceFromPlayer = 5f;

    // State management
    private enum GoblinState { Idle, Chasing, Attacking }
    private GoblinState currentState = GoblinState.Idle;
    private float lastAttackTime = 0f;
    
public bool isDead = false;
    private bool isPositioned = true; // Changed to true by default to ensure movement
    private float nextAttackTime = 0f; // Time-based attack system instead of boolean flag

    // Animator parameter names (must match Animator exactly!)
    private string walkParam = "isWalking";
    private string sprintParam = "isSprinting";
    private string attackParam = "attack";
    private string dieParam = "die1";
    private string kickLeftParam = "kickLeft";
    private string kickRightParam = "kickRight";

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
                Debug.LogWarning("AudioManager not found for GoblinAI - audio features will be disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error finding AudioManager: " + e.Message + " - audio features will be disabled");
        }
    }

    void Start()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        

        // Set up NavMeshAgent
        if (agent != null) {
            agent.speed = moveSpeed;
            agent.angularSpeed = rotationSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = attackRange * 0.8f;
        } else {
            // Debug.LogError("❗ NavMeshAgent component missing!");
        }
        
        // IMPORTANT: Find player using a reliable method
        // First check if we have a direct reference
        if (playerDirectReference != null) {
            player = playerDirectReference;
            // Debug.Log("✅ Using direct player reference: " + player.name);
        }
        // If no direct reference, try to find the Player component
        else {
            var playerComponent = FindObjectOfType<Player>();
            if (playerComponent != null) {
                // If the Player script has a Character reference, use that
                if (playerComponent.Character != null) {
                    player = playerComponent.Character.transform;
                    // Debug.Log("✅ Found player via Player.Character: " + player.name);
                } 
                // Otherwise use the Player GameObject itself
                else {
                    player = playerComponent.transform;
                    // Debug.Log("✅ Found player via Player component: " + player.name);
                }
            }
            // If no Player component, try to find by tag
            else {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) {
                    player = playerObj.transform;
                    // Debug.Log("✅ Found player by tag: " + playerObj.name);
                }
                // If still not found, try to find KinematicCharacterMotor
                else {
                    var motor = FindObjectOfType<KinematicCharacterController.KinematicCharacterMotor>();
                    if (motor != null) {
                        player = motor.transform;
                        // Debug.Log("✅ Found player by KinematicCharacterMotor: " + player.name);
                    }
                    // Last resort - try CharacterController
                    else {
                        var cc = FindObjectOfType<CharacterController>();
                        if (cc != null) {
                            player = cc.transform;
                            // Debug.Log("✅ Found player by CharacterController: " + player.name);
                        }
                        // If all else fails, use manual position
                        else if (useManualPosition) {
                            GameObject tempObj = new GameObject("TempPlayerPosition");
                            tempObj.transform.position = manualPlayerPosition;
                            player = tempObj.transform;
                            tempObj.hideFlags = HideFlags.HideAndDontSave;
                            // Debug.Log("⚠️ Using manual position as fallback");
                        }
                        else {
                            // Debug.LogError("❗ No player found! Please assign player manually.");
                        }
                    }
                }
            }
        }
        
        // Log what we found
        if (player != null) {
            // Debug.Log($"🔍 FOUND PLAYER: {player.name} at position {player.position}");
            
            // IMPORTANT: Check if we found the wrong object at origin (0,0,0)
            if (player.name == "Character" && player.position == Vector3.zero) {
                // Debug.LogWarning("⚠️ Found 'Character' at (0,0,0) - this is likely the wrong object!");
                
                // If we have manual position enabled, use that instead
                if (useManualPosition) {
                    GameObject tempObj = new GameObject("TempPlayerPosition");
                    tempObj.transform.position = manualPlayerPosition;
                    player = tempObj.transform;
                    tempObj.hideFlags = HideFlags.HideAndDontSave;
                    // Debug.Log("⚠️ Switched to manual position instead");
                }
                // Otherwise try other methods
                else {
                    // Try to find by Player component again
                    var playerComponent = FindObjectOfType<Player>();
                    if (playerComponent != null) {
                        player = playerComponent.transform;
                        // Debug.Log("✅ Switched to Player component: " + player.name);
                    }
                }
            }
        }
        
        // Set initial state
        currentState = GoblinState.Idle;
        lastAttackTime = -attackCooldown; // Allow immediate attack
        
        // Set initial position based on type
        if (positionType == PositionType.Random) {
            Vector3 randomPos = transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas)) {
                transform.position = hit.position;
            }
        }
        else if (player == null) {
            // Debug.LogError("❗ No player found by any method. Goblins won't move!");
        }
        
        // Force positioning to be true
        isPositioned = true;
        
        // If using random positioning, set it
        if (positionType == PositionType.Random) SetRandomPosition();

        health = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    void Update()
    {
        // Early return checks with detailed logging
        if (isDead) {
            return;
        }
        
        // Get the current player position
        Vector3 targetPosition;
        
        // Always try to find the actual player first
        if (player == null || Time.frameCount % 60 == 0) { // Re-check periodically
            // Try direct reference first
            if (playerDirectReference != null) {
                player = playerDirectReference;
                // Debug.Log("✅ Using direct player reference");
            } else {
                // Try to find the player using the Player component
                var playerComponent = FindObjectOfType<Player>();
                if (playerComponent != null) {
                    // If the Player component has a Character reference, use that
                    if (playerComponent.Character != null) {
                        player = playerComponent.Character.transform;
                        // Debug.Log("✅ Found player via Player.Character component");
                    } else {
                        // Otherwise use the Player GameObject itself
                        player = playerComponent.transform;
                        // Debug.Log("✅ Found player via Player component");
                    }
                } else {
                    // Try to find by tag
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null) {
                        player = playerObj.transform;
                        // Debug.Log("✅ Found player by tag");
                    } else {
                        // Try to find by KinematicCharacterMotor
                        var motor = FindObjectOfType<KinematicCharacterController.KinematicCharacterMotor>();
                        if (motor != null) {
                            player = motor.transform;
                            // Debug.Log("✅ Found player by KinematicCharacterMotor");
                        } else {
                            // If all else fails, use manual position
                            if (useManualPosition) {
                                if (player == null) {
                                    GameObject tempObj = new GameObject("TempPlayerPosition");
                                    tempObj.transform.position = manualPlayerPosition;
                                    player = tempObj.transform;
                                    tempObj.hideFlags = HideFlags.HideAndDontSave;
                                }
                                // Debug.LogWarning("⚠️ Using manual position as fallback");
                            } else {
                                // Debug.LogError("❗ No player found in Update!");
                                return; // No player found
                            }
                        }
                    }
                }
            }
            
            // CRITICAL: Check if we found the wrong object at origin (0,0,0)
            if (player.name == "Character" && player.position == Vector3.zero) {
                // Debug.LogWarning("⚠️ Found 'Character' at (0,0,0) - this is likely the wrong object!");
                
                // Try to find by Player component again
                var playerComponent = FindObjectOfType<Player>();
                if (playerComponent != null) {
                    player = playerComponent.transform;
                    // Debug.Log("✅ Switched to Player component: " + player.name);
                }
            }
        }
        
        // ALWAYS use the player's actual position - ignore manual position setting
        // This is a temporary fix to ensure the orcs follow the player
        if (player != null) {
            targetPosition = player.position;
            
            // Only log when debugging is needed
            // Debug.Log($"Following: {player.name}");
        } else {
            // Only use manual position as a last resort
            targetPosition = manualPlayerPosition;
            // Debug.LogWarning($"⚠️ FALLBACK to manual position: {manualPlayerPosition}");
        }
        
        // Make sure agent is valid
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null) {
                // Debug.LogError("❗ NavMeshAgent is missing!");
                return;
            }
        }
        
        // Force agent settings every frame
        agent.enabled = true;
        agent.isStopped = false;
        agent.autoBraking = false;
        
        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Reduced logging
        // if (Time.frameCount % 600 == 0) {
        //     Debug.Log($"Goblin distance: {distance:F2}");
        // }

        // ALWAYS chase the target - no distance check
        agent.SetDestination(targetPosition);
        
        // Set animation parameters
        if (animator != null) {
            float moveSpeed = agent.velocity.magnitude;
            bool isMoving = moveSpeed > 0.05f;
            bool shouldSprint = isMoving && GameObject.FindObjectsOfType<GoblinAI>().Length <= 3;
            
            animator.SetBool(walkParam, isMoving);
            animator.SetBool(sprintParam, shouldSprint);
        }

        // Attack when in range using time-based cooldown
        if (distance <= attackRange) {
            // Check if enough time has passed since last attack
            if (Time.time >= nextAttackTime) {
                Attack();
                // Set the next attack time
                nextAttackTime = Time.time + attackCooldown;
            }
            
            // Debug the attack state
            if (Time.frameCount % 60 == 0) {
                float remainingCooldown = Mathf.Max(0, nextAttackTime - Time.time);
                // Debug.Log($"[GoblinAI] In attack range, cooldown remaining: {remainingCooldown:F1}s");
            }
        }
        if (healthUI != null && Camera.main != null)
        {
            // Fixed position above goblin
            Vector3 offset = new Vector3(0f, 1f, 0f); // You can tweak 2.2 to be higher/lower
            healthUI.position = transform.position + offset;

            // Face the camera
            healthUI.LookAt(Camera.main.transform);
            healthUI.Rotate(0, 180, 0);
        }
    }

    void SetRandomPosition()
    {
        if (agent == null) return;

        for (int i = 0; i < 30; i++)
        {
            Vector3 pos = areaCenter + new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                0,
                Random.Range(-areaSize.z / 2, areaSize.z / 2)
            );

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                if (player != null && Vector3.Distance(hit.position, player.position) < minDistanceFromPlayer)
                    continue;

                bool tooClose = false;
                foreach (GoblinAI other in FindObjectsOfType<GoblinAI>())
                {
                    if (other != this && Vector3.Distance(hit.position, other.transform.position) < minDistanceFromOtherEnemies)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    agent.Warp(hit.position);
                    isPositioned = true;
                    return;
                }
            }
        }

        // Debug.LogWarning("⚠️ Could not find valid random position.");
        isPositioned = true;
    }

    // We're now handling chase logic directly in Update
    void Chase()
    {
        // This method is kept for compatibility but not used
        // Chase logic is now in Update for more direct control
    }

    void Attack()
    {
        int attackType = Random.Range(0, 3);
        // Debug.Log($"[GoblinAI] Attacking with type {attackType}, next attack in {attackCooldown}s");
        
        // APPLY DAMAGE TO PLAYER
        if (player != null)
        {
            // Try to find PlayerHealth component on the player or parent/children
            PlayerHealth playerHealth = null;
            
            // Try direct component
            playerHealth = player.GetComponent<PlayerHealth>();
            
            // If not found, check parent
            if (playerHealth == null && player.parent != null)
                playerHealth = player.parent.GetComponent<PlayerHealth>();
                
            // If still not found, check children
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
                
            // Last resort: try to find PlayerHealth in the scene
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
                Debug.Log("[GoblinAI] Attempting to find PlayerHealth component in scene");
            }
                
            // If found, apply damage directly
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[GoblinAI] Applied {damage} damage to player health!");
            }
            else
            {
                Debug.LogWarning("[GoblinAI] Could not find PlayerHealth component!");
            }
        }

        // Play attack animation
        if (animator != null)
        {
            switch (attackType)
            {
                case 0:
                    animator.SetTrigger(attackParam);
                    break;
                case 1:
                    animator.SetTrigger(kickLeftParam);
                    break;
                case 2:
                    animator.SetTrigger(kickRightParam);
                    break;
            }
        }

        StartCoroutine(AttackCooldown());
    }

    // No longer needed - using time-based system instead
    IEnumerator AttackCooldown()
    {
        yield break;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("💢 Goblin took " + amount + " damage.");

        if (healthSlider != null)
            healthSlider.value = health;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("☠️ Goblin died.");

        // Spawn blood effect
        SpawnBloodEffect();

        // Play death animation
        if (animator != null) animator.SetTrigger(dieParam);
        
        // Disable NavMeshAgent
        if (agent != null) agent.enabled = false;

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.coinDrop);
        DropCoins();

        
        // Check if this is the last enemy in the level
        if (isLastEnemy || (useAutoLastEnemy && IsLastEnemyInLevel()))
        {
            DropKey();
        }
        if (healthUI != null)
        healthUI.gameObject.SetActive(false);
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.goblinDeath);


        // Destroy the goblin after delay
        Destroy(gameObject, 3f);
    }

    // Spawns the blood effect at the appropriate position and rotation
    private void SpawnBloodEffect()
    {
        if (deathEffectPrefab != null)
        {
            // Determine spawn position
            Vector3 spawnPos;
            if (bloodSpawnPoint != null)
            {
                spawnPos = bloodSpawnPoint.position;
            }
            else
            {
                // Fallback: Use raycast to find ground below goblin
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
                {
                    // Position slightly above the hit point to avoid clipping
                    spawnPos = hit.point + hit.normal * 0.05f;
                    Debug.Log("💉 Blood effect spawned at ground position: " + spawnPos);
                }
                else
                {
                    // If no ground found, use a position slightly below the goblin
                    spawnPos = transform.position + Vector3.down * 0.5f;
                    Debug.Log("💉 Blood effect spawned at fallback position: " + spawnPos);
                }
            }

            // Create rotation that aligns with the ground or uses the specified offset
            Quaternion bloodRotation = Quaternion.Euler(bloodRotationOffset);

            // Instantiate and play the particle effect
            var bloodEffect = Instantiate(deathEffectPrefab, spawnPos, bloodRotation);
            bloodEffect.Play();

            // Calculate total lifetime including particle duration and max particle lifetime
            float maxLifetime = bloodEffect.main.duration;
            if (bloodEffect.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            {
                maxLifetime += bloodEffect.main.startLifetime.constant;
            }
            else if (bloodEffect.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            {
                maxLifetime += bloodEffect.main.startLifetime.constantMax;
            }

            // Add a small buffer to ensure all particles are gone
            maxLifetime += 1f;

            // Clean up the effect after it completes
            Destroy(bloodEffect.gameObject, maxLifetime);
            Debug.Log("💉 Blood effect will be destroyed after " + maxLifetime + " seconds");
        }
        else
        {
            Debug.LogWarning("⚠️ No deathEffectPrefab assigned for blood effect!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (positionType == PositionType.Random)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(areaCenter, areaSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }



bool IsLastEnemyInLevel()
{
    // Count active goblins in the scene
    GoblinAI[] goblins = FindObjectsOfType<GoblinAI>();
    int activeGoblins = 0;
    
    foreach (GoblinAI goblin in goblins)
    {
        if (goblin != this && !goblin.isDead)
        {
            activeGoblins++;
        }
    }
    
    // If this is the last one, return true
    return activeGoblins == 0;
}


void DropCoins()
{
    // Check if we should drop coins
    if (!alwaysDropCoins && Random.Range(0f, 1f) > coinDropChance)
    {
        return;
    }
    
    if (coinPrefab == null)
    {
        Debug.LogWarning("⚠️ Coin prefab not assigned to GoblinAI!");
        return;
    }
    
    // Determine number of coins to drop
    int coinsTooDrop = Random.Range(minCoins, maxCoins + 1);
    
    Debug.Log($"💰 Dropping {coinsTooDrop} coins from goblin");
    
    for (int i = 0; i < coinsTooDrop; i++)
    {
        // Calculate random position around goblin
        Vector3 randomOffset = Random.insideUnitCircle * coinSpreadRadius;
        Vector3 coinPosition = transform.position + new Vector3(randomOffset.x, 0.5f, randomOffset.y);
        
        // Create coin
        GameObject coin = Instantiate(coinPrefab, coinPosition, Random.rotation);
        
        // Add some physics if the coin has a Rigidbody
        Rigidbody coinRb = coin.GetComponent<Rigidbody>();
        if (coinRb != null)
        {
            Vector3 force = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * coinDropForce;
            
            coinRb.AddForce(force, ForceMode.Impulse);
            
            // Add some random rotation
            coinRb.AddTorque(Random.insideUnitSphere * coinDropForce, ForceMode.Impulse);
        }
    }
}

void DropKey()
{
    if (keyPrefab != null)
    {
        // Instantiate the key slightly above the goblin
        Vector3 keyPosition = transform.position + Vector3.up * 0.5f;
        GameObject key = Instantiate(keyPrefab, keyPosition, Quaternion.identity);
        
        // Set the key ID based on the current scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene.Contains("Level_"))
        {
            string levelNumber = currentScene.Replace("Level_", "");
            KeyItem keyItem = key.GetComponent<KeyItem>();
            if (keyItem != null)
            {
                keyItem.keyID = int.Parse(levelNumber);
            }
        }
        
        Debug.Log("🔑 Key dropped by last goblin!");
    }
    else
    {
        Debug.LogWarning("⚠️ Key prefab not assigned to GoblinAI!");
    }
}



}