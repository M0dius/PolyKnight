using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinAI : MonoBehaviour
{
    // References
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    // Stats
    [Header("Goblin Stats")]
    public float health = 30f;
    public float damage = 7f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;

    // Position settings
  
    public enum PositionType { Defined, Random }
    public PositionType positionType = PositionType.Defined;
    [Header("Position Settings")]
    // For random position
    public Vector3 areaCenter;
    public Vector3 areaSize;
    public float minDistanceFromOtherEnemies = 2f;
    public float minDistanceFromPlayer = 5f;

    // State tracking
    private bool canAttack = true;
    private bool isDead = false;
    private bool isPositioned = false;

    // Animation parameter names - change these to match your model's animator
    private string walkParamName = "isWalking";
    private string attackParamName = "attack";
    private string dieParamName = "die";

    void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("No player found with 'Player' tag!");
        }

        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Set initial state
        if (agent != null)
        {
            agent.stoppingDistance = attackRange;
        }

        // Set initial position based on type
        if (positionType == PositionType.Random)
        {
            SetRandomPosition();
        }
        else
        {
            // Defined position is already set in the scene
            isPositioned = true;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Only start normal behavior once properly positioned
        if (!isPositioned) return;

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

    void SetRandomPosition()
    {
        if (agent == null) return;

        // Try to find a valid position
        for (int attempts = 0; attempts < 30; attempts++)
        {
            // Generate random position within area
            Vector3 randomPos = new Vector3(
                areaCenter.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
                areaCenter.y,
                areaCenter.z + Random.Range(-areaSize.z / 2, areaSize.z / 2)
            );

            // Check if on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                // Check distance from player
                if (player != null && Vector3.Distance(hit.position, player.position) < minDistanceFromPlayer)
                {
                    continue; // Too close to player, try again
                }

                // Check distance from other enemies
                bool tooCloseToOtherEnemy = false;
                foreach (GoblinAI otherGoblin in FindObjectsOfType<GoblinAI>())
                {
                    if (otherGoblin != this && Vector3.Distance(hit.position, otherGoblin.transform.position) < minDistanceFromOtherEnemies)
                    {
                        tooCloseToOtherEnemy = true;
                        break;
                    }
                }

                if (tooCloseToOtherEnemy)
                {
                    continue; // Too close to another enemy, try again
                }

                // Position is valid, use it
                agent.Warp(hit.position);
                isPositioned = true;
                return;
            }
        }

        // If we couldn't find a valid position after all attempts
        Debug.LogWarning("Could not find valid random position for goblin. Using current position.");
        isPositioned = true;
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
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Goblin died!");

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

    // Visualize detection and attack ranges in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw random position area
        if (positionType == PositionType.Random)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(areaCenter, areaSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}