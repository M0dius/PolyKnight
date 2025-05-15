using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // ✅ This enables use of Slider


public class GoblinKingAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private List<GoblinAI> followers = new List<GoblinAI>();
    
    [Header("Goblin King Stats")]
    public float health = 100f;
    public float damage = 15f;
    public float detectionRange = 60f;
    public float attackRange = 2.0f;
    public float attackCooldown = 3f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 100f;
    public float acceleration = 6f;
    
    [Header("Special Abilities")]
    public bool canSummonMinions = true;
    public GameObject goblinPrefab;
    public int maxMinions = 3;
    public float summonCooldown = 10f;
    public float rageThreshold = 0.5f; // Percentage of health when rage mode activates
    
    [Header("References")]
    public Transform playerDirectReference;
    public Vector3 manualPlayerPosition = new Vector3(-8.582f, 1.15f, -1.0f);
    public bool useManualPosition = false;
    
    // State management
    private enum GoblinKingState { Idle, Chasing, Attacking, Summoning, Raging }
    private GoblinKingState currentState = GoblinKingState.Idle;
    private float lastAttackTime = 0f;
    private float lastSummonTime = 0f;
    private bool isDead = false;
    private bool isRaging = false;
    private float nextAttackTime = 0f;
    
    // Animator parameter names
    private string walkParam = "isWalking";
    private string sprintParam = "isSprinting";
    private string attackParam = "attack";
    private string dieParam = "die1";
    private string rageParam = "rage"; // Add this parameter to your animator if available

    [Header("Health UI")]
    public Canvas healthUI;
    public Slider healthSlider;


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
            Debug.LogError("❗ NavMeshAgent component missing on Goblin King!");
        }
        
        // Find player
        if (playerDirectReference != null) {
            player = playerDirectReference;
            Debug.Log("👑 Goblin King: Using direct player reference");
        } else {
            FindPlayer();
        }
        
        // Initialize
        lastSummonTime = -summonCooldown; // Allow immediate summon
        
        // Make the Goblin King larger
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        // Change the color to distinguish from regular goblins
        ChangeColor(new Color(0.7f, 0.2f, 0.2f)); // Reddish color for the king

        if (healthSlider != null)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Get target position
        Vector3 targetPosition;
        
        if (useManualPosition) {
            targetPosition = manualPlayerPosition;
        } else if (player != null) {
            targetPosition = player.position;
        } else {
            FindPlayer();
            return;
        }
        
        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Check if we should enter rage mode
        if (!isRaging && health / 100f <= rageThreshold) {
            EnterRageMode();
        }
        
        // Summon minions if possible
        if (canSummonMinions && followers.Count < maxMinions && Time.time >= lastSummonTime + summonCooldown) {
            SummonMinion();
        }
        
        // Attack when in range using time-based cooldown
        if (distance <= attackRange) {
            // Check if enough time has passed since last attack
            if (Time.time >= nextAttackTime) {
                Attack();
                // Set the next attack time
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        
        // Always chase the target
        if (agent != null && agent.enabled) {
            agent.SetDestination(targetPosition);
            
            // Set animation parameters
            if (animator != null) {
                float moveSpeed = agent.velocity.magnitude;
                bool isMoving = moveSpeed > 0.05f;
                
                animator.SetBool(walkParam, isMoving);
                animator.SetBool(sprintParam, isRaging && isMoving);
            }
        }

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (healthUI != null && Camera.main != null)
        {
            Vector3 offset = new Vector3(0, 2f, 0); // Adjust Y to float above head
            healthUI.transform.position = transform.position + offset;
            healthUI.transform.LookAt(Camera.main.transform);
            healthUI.transform.Rotate(0, 180, 0); // Flip to face the camera properly
        }


    }

    void FindPlayer()
    {
        // Try to find the Player component
        var playerComponent = FindObjectOfType<Player>();
        if (playerComponent != null) {
            if (playerComponent.Character != null) {
                player = playerComponent.Character.transform;
                Debug.Log("👑 Goblin King: Found player via Player.Character");
            } else {
                player = playerComponent.transform;
                Debug.Log("👑 Goblin King: Found player via Player component");
            }
        } else {
            // Try by tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                player = playerObj.transform;
                Debug.Log("👑 Goblin King: Found player by tag");
            } else {
                Debug.LogWarning("👑 Goblin King: Could not find player!");
            }
        }
    }
    
    void Attack()
    {
        Debug.Log($"👑 Goblin King: Attacking! Next attack in {attackCooldown}s");
        
        // Play attack animation
        if (animator != null) {
            animator.SetTrigger(attackParam);
        }
        
        // Apply damage to player
        if (player != null) {
            // Try to find PlayerHealth component
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealth>();
            
            if (playerHealth != null) {
                // Apply increased damage if raging
                float actualDamage = isRaging ? damage * 1.5f : damage;
                playerHealth.TakeDamage(actualDamage);
                Debug.Log($"👑 Goblin King: Applied {actualDamage} damage to player!");
            }
        }
    }
    
    void SummonMinion()
    {
        lastSummonTime = Time.time;
        
        // Play summoning animation/effect if available
        
        // Spawn a new goblin
        if (goblinPrefab != null) {
            // Spawn position around the king
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 2f;
            spawnPos.y = transform.position.y; // Keep same height
            
            // Check if position is on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 3f, NavMesh.AllAreas)) {
                GameObject newGoblin = Instantiate(goblinPrefab, hit.position, Quaternion.identity);
                
                // Get the GoblinAI component
                GoblinAI goblinAI = newGoblin.GetComponent<GoblinAI>();
                if (goblinAI != null) {
                    // Set the player reference
                    goblinAI.playerDirectReference = player;
                    
                    // Add to followers list
                    followers.Add(goblinAI);
                    
                    Debug.Log("👑 Goblin King: Summoned a new minion!");
                }
            }
        } else {
            Debug.LogWarning("👑 Goblin King: No goblin prefab assigned for summoning!");
        }
    }
    
    void EnterRageMode()
    {
        isRaging = true;
        
        // Increase speed and damage
        if (agent != null) {
            agent.speed = moveSpeed * 1.5f;
        }
        
        // Visual effect - make even larger and more red
        transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
        ChangeColor(new Color(0.9f, 0.1f, 0.1f)); // More intense red
        
        // Play rage animation if available
        if (animator != null && animator.parameters.Length > 0) {
            // Check if the rage parameter exists
            foreach (var param in animator.parameters) {
                if (param.name == rageParam) {
                    animator.SetTrigger(rageParam);
                    break;
                }
            }
        }
        
        Debug.Log("👑 Goblin King: ENTERED RAGE MODE!");
    }
    
    void ChangeColor(Color newColor)
    {
        // Change the color of the model
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            // Change the color of all materials
            foreach (Material material in renderer.materials) {
                // Check if the material has color property
                if (material.HasProperty("_Color")) {
                    material.color = newColor;
                }
            }
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        health -= amount;
        Debug.Log($"👑 Goblin King took {amount} damage! Health: {health}");
        
        // Check for rage mode threshold
        if (!isRaging && health / 100f <= rageThreshold) {
            EnterRageMode();
        }
        
        if (health <= 0) {
            Die();
        }

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

    }
    
    void Die()
    {
        isDead = true;
        Debug.Log("👑 Goblin King has been defeated!");
        
        // Play death animation
        if (animator != null) {
            animator.SetTrigger(dieParam);
        }
        
        // Disable components
        if (agent != null) agent.enabled = false;
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Optional: Drop loot, trigger level completion, etc.
        
        // Destroy after delay
        Destroy(gameObject, 5f);
    }
}