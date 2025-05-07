using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinAI : MonoBehaviour
{

// <<<<<<< Ahmed-Blood-Effect

//     [Header("Effects")]
//     public ParticleSystem deathEffectPrefab;
//     public Transform bloodSpawnPoint;
//     // References
// =======
// >>>>>>> main

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

// <<<<<<< Ahmed-Blood-Effect
//     // Stats
//     [Header("Goblin Stats")]
//     public float health = 30f;
//     public float damage = 7f;
//     public float detectionRange = 10f;
//     public float attackRange = 1.5f;
//     public float attackCooldown = 0.5f;

//     // State tracking
//     private bool canAttack = true;
//     private bool isDead = false;

//     // Animation parameter names - change these to match your model's animator
//     private string walkParamName = "isWalking";
//     private string attackParamName = "attack";
//     private string dieParamName = "die";

//     void Start()
//     {
//         // Find the player
//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj != null)
//         {
//             player = playerObj.transform;
//         }
//         else
//         {
//             Debug.LogError("No player found with 'Player' tag!");
//         }

//         // Get components
//         agent = GetComponent<NavMeshAgent>();
//         animator = GetComponent<Animator>();

//         // Set initial state
//         if (agent != null)
//         {
//             agent.stoppingDistance = attackRange;
//         }
// =======

    [Header("Goblin Stats")]
    public float health = 30f;
    public float damage = 7f;
    public float detectionRange = 50f; // Increased detection range
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 120f;
    public float acceleration = 8f;
    
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
    
    private bool canAttack = true;
    private bool isDead = false;
    private bool isPositioned = true; // Changed to true by default to ensure movement

    // Animator parameter names (must match Animator exactly!)
    private string walkParam = "isWalking1";
    private string sprintParam = "isSprinting";
    private string attackParam = "attack1";
    private string dieParam = "die1";
    private string kickLeftParam = "kickLeft";
    private string kickRightParam = "kickRight";

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
            Debug.LogError("❗ NavMeshAgent component missing!");
        }
        
        // IMPORTANT: Find player using a reliable method
        // First check if we have a direct reference
        if (playerDirectReference != null) {
            player = playerDirectReference;
            Debug.Log("✅ Using direct player reference: " + player.name);
        }
        // If no direct reference, try to find the Player component
        else {
            var playerComponent = FindObjectOfType<Player>();
            if (playerComponent != null) {
                // If the Player script has a Character reference, use that
                if (playerComponent.Character != null) {
                    player = playerComponent.Character.transform;
                    Debug.Log("✅ Found player via Player.Character: " + player.name);
                } 
                // Otherwise use the Player GameObject itself
                else {
                    player = playerComponent.transform;
                    Debug.Log("✅ Found player via Player component: " + player.name);
                }
            }
            // If no Player component, try to find by tag
            else {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) {
                    player = playerObj.transform;
                    Debug.Log("✅ Found player by tag: " + playerObj.name);
                }
                // If still not found, try to find KinematicCharacterMotor
                else {
                    var motor = FindObjectOfType<KinematicCharacterController.KinematicCharacterMotor>();
                    if (motor != null) {
                        player = motor.transform;
                        Debug.Log("✅ Found player by KinematicCharacterMotor: " + player.name);
                    }
                    // Last resort - try CharacterController
                    else {
                        var cc = FindObjectOfType<CharacterController>();
                        if (cc != null) {
                            player = cc.transform;
                            Debug.Log("✅ Found player by CharacterController: " + player.name);
                        }
                        // If all else fails, use manual position
                        else if (useManualPosition) {
                            GameObject tempObj = new GameObject("TempPlayerPosition");
                            tempObj.transform.position = manualPlayerPosition;
                            player = tempObj.transform;
                            tempObj.hideFlags = HideFlags.HideAndDontSave;
                            Debug.Log("⚠️ Using manual position as fallback");
                        }
                        else {
                            Debug.LogError("❗ No player found! Please assign player manually.");
                        }
                    }
                }
            }
        }
        
        // Log what we found
        if (player != null) {
            Debug.Log($"🔍 FOUND PLAYER: {player.name} at position {player.position}");
            
            // IMPORTANT: Check if we found the wrong object at origin (0,0,0)
            if (player.name == "Character" && player.position == Vector3.zero) {
                Debug.LogWarning("⚠️ Found 'Character' at (0,0,0) - this is likely the wrong object!");
                
                // If we have manual position enabled, use that instead
                if (useManualPosition) {
                    GameObject tempObj = new GameObject("TempPlayerPosition");
                    tempObj.transform.position = manualPlayerPosition;
                    player = tempObj.transform;
                    tempObj.hideFlags = HideFlags.HideAndDontSave;
                    Debug.Log("⚠️ Switched to manual position instead");
                }
                // Otherwise try other methods
                else {
                    // Try to find by Player component again
                    var playerComponent = FindObjectOfType<Player>();
                    if (playerComponent != null) {
                        player = playerComponent.transform;
                        Debug.Log("✅ Switched to Player component: " + player.name);
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
            Debug.LogError("❗ No player found by any method. Goblins won't move!");
        }
        
        // Force positioning to be true
        isPositioned = true;
        
        // If using random positioning, set it
        if (positionType == PositionType.Random) SetRandomPosition();

    }

    void Update()
    {

// Quick-test: kill goblin with Space key
        if (Input.GetKeyDown(KeyCode.Space) && !isDead)
        {
            Die();
            return; // skip the rest of Update this frame
        }

        if (isDead || player == null) return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Chase the player
            Chase();

            // Check if close enough to attack
            if (distanceToPlayer <= attackRange && canAttack)
            {
                Attack();
            }
        }
        else
        {
            // Stop moving if player is out of range
            if (agent != null)
            {
                agent.SetDestination(transform.position);

                // Update animation if you have walking animation
                if (animator != null)
                {
                    animator.SetBool(walkParamName, false);
                }
            }
        }
    }

    void Chase()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);

            // Update animation if you have walking animation
            if (animator != null && animator.parameters.Length > 0)
            {
                animator.SetBool(walkParamName, true);
            }
        }

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
                Debug.Log("✅ Using direct player reference");
            } else {
                // Try to find the player using the Player component
                var playerComponent = FindObjectOfType<Player>();
                if (playerComponent != null) {
                    // If the Player component has a Character reference, use that
                    if (playerComponent.Character != null) {
                        player = playerComponent.Character.transform;
                        Debug.Log("✅ Found player via Player.Character component");
                    } else {
                        // Otherwise use the Player GameObject itself
                        player = playerComponent.transform;
                        Debug.Log("✅ Found player via Player component");
                    }
                } else {
                    // Try to find by tag
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null) {
                        player = playerObj.transform;
                        Debug.Log("✅ Found player by tag");
                    } else {
                        // Try to find by KinematicCharacterMotor
                        var motor = FindObjectOfType<KinematicCharacterController.KinematicCharacterMotor>();
                        if (motor != null) {
                            player = motor.transform;
                            Debug.Log("✅ Found player by KinematicCharacterMotor");
                        } else {
                            // If all else fails, use manual position
                            if (useManualPosition) {
                                if (player == null) {
                                    GameObject tempObj = new GameObject("TempPlayerPosition");
                                    tempObj.transform.position = manualPlayerPosition;
                                    player = tempObj.transform;
                                    tempObj.hideFlags = HideFlags.HideAndDontSave;
                                }
                                Debug.LogWarning("⚠️ Using manual position as fallback");
                            } else {
                                Debug.LogError("❗ No player found in Update!");
                                return; // No player found
                            }
                        }
                    }
                }
            }
            
            // CRITICAL: Check if we found the wrong object at origin (0,0,0)
            if (player.name == "Character" && player.position == Vector3.zero) {
                Debug.LogWarning("⚠️ Found 'Character' at (0,0,0) - this is likely the wrong object!");
                
                // Try to find by Player component again
                var playerComponent = FindObjectOfType<Player>();
                if (playerComponent != null) {
                    player = playerComponent.transform;
                    Debug.Log("✅ Switched to Player component: " + player.name);
                }
            }
        }
        
        // ALWAYS use the player's actual position - ignore manual position setting
        // This is a temporary fix to ensure the orcs follow the player
        if (player != null) {
            targetPosition = player.position;
            
            // Debug what we're following
            if (Time.frameCount % 60 == 0) {
                Debug.Log($"🔍 FORCED FOLLOWING: {player.name} at position {player.position}");
            }
        } else {
            // Only use manual position as a last resort
            targetPosition = manualPlayerPosition;
            Debug.LogWarning($"⚠️ FALLBACK to manual position: {manualPlayerPosition}");
        }
        
        // Make sure agent is valid
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null) {
                Debug.LogError("❗ NavMeshAgent is missing!");
                return;
            }
        }
        
        // Force agent settings every frame
        agent.enabled = true;
        agent.isStopped = false;
        agent.autoBraking = false;
        
        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Log distance information occasionally
        if (Time.frameCount % 120 == 0) {
            Debug.Log($"🔍 Goblin distance to target: {distance:F2}, Agent speed: {agent.speed}");
        }

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

        // Only attack when in range
        if (distance <= attackRange && canAttack) {
            Attack();
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

        Debug.LogWarning("⚠️ Could not find valid random position.");
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
        // Trigger attack animation if you have one
        if (animator != null && animator.parameters.Length > 0)
        {
            animator.SetTrigger(attackParamName);
        }

        Debug.Log("Goblin attacks player for " + damage + " damage!");

        // Start cooldown

        canAttack = false;

        int attackType = Random.Range(0, 3);
        Debug.Log($"🗡️ Goblin attacking with type {attackType}");

        if (animator != null)
        {
            switch (attackType)
            {
                case 0:
                    animator.SetTrigger(attackParam);
                    Debug.Log("🔁 Triggered: " + attackParam);
                    break;
                case 1:
                    animator.SetTrigger(kickLeftParam);
                    Debug.Log("🔁 Triggered: " + kickLeftParam);
                    break;
                case 2:
                    animator.SetTrigger(kickRightParam);
                    Debug.Log("🔁 Triggered: " + kickRightParam);
                    break;
            }

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("🎞️ Current State: " + stateInfo.fullPathHash);
        }


        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {

        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // You can call this from player attacks
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("Goblin took " + amount + " damage! Health: " + health);

        // Check if dead
        if (health <= 0)
        {
            Die();
        }

        Debug.Log("💢 Goblin took " + amount + " damage.");

        if (health <= 0)
            Die();

    }

    void Die()
    {
        isDead = true;

        Debug.Log("Goblin died!");

        // 2) Spawn blood effect
        if (deathEffectPrefab != null)
        {
            Vector3 spawnPos;
            if (bloodSpawnPoint != null)
            {
                spawnPos = bloodSpawnPoint.position;
            }
            else
            {
                // fallback: slightly below origin
                spawnPos = transform.position + Vector3.down * 0.5f;
            }

            var bs = Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
            bs.Play();
            // cleanup after full lifetime
            float maxLife = bs.main.duration + bs.main.startLifetime.constantMax;
            Destroy(bs.gameObject, maxLife);
        }
        else
        {
            Debug.LogWarning("No deathEffectPrefab assigned!");
        }

        // Trigger death animation if you have one
        if (animator != null && animator.parameters.Length > 0)
        {
            animator.SetTrigger(dieParamName);
        }

        // Disable the NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destroy after delay
        Destroy(gameObject, 3f); // Adjust time based on death animation length
    }

    // Visualize detection range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

        Debug.Log("☠️ Goblin died.");
        if (animator != null) animator.SetTrigger(dieParam);
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
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
}
