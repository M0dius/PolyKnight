using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinAI : MonoBehaviour
{

    [Header("Effects")]
    public ParticleSystem deathEffectPrefab;
    public Transform bloodSpawnPoint;
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

    // State tracking
    private bool canAttack = true;
    private bool isDead = false;

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