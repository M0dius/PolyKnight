using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;

    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

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
                Debug.LogWarning("AudioManager not found for PlayerAttack - audio features will be disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error finding AudioManager: " + e.Message + " - audio features will be disabled");
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[PlayerAttack] Animator not found on Player!");
        }
    }

    void Update()
    {
        // Check if the game is paused. If it is, don't process attack input.
        if (PauseMenu.GameIsPaused)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (animator != null)
            {
                // Play attack sound if audio manager is available
                if (audioManager != null)
                    audioManager.PlaySFX(audioManager.playerAttack);
                
                // Always trigger the attack animation
                animator.SetTrigger("isAttacking");
            }

            Attack();

            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Collider[] enemiesHit = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in enemiesHit)
        {
            GoblinAI goblinAI = enemy.GetComponent<GoblinAI>();
            if (goblinAI != null)
            {
                goblinAI.TakeDamage(attackDamage);
                Debug.Log("Attacked " + enemy.name + " for " + attackDamage + " damage.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        // First check specifically for the Goblin King (regardless of tag)
        GoblinKingAI king = other.GetComponent<GoblinKingAI>();
        if (king != null)
        {
            king.TakeDamage(attackDamage);
            Debug.Log("Goblin King hit! Dealing " + attackDamage + " damage.");
            return;
        }
        
        // Then check for the Goblin Prince (regardless of tag)
        GoblinPrinceAI prince = other.GetComponent<GoblinPrinceAI>();
        if (prince != null)
        {
            prince.TakeDamage(attackDamage);
            Debug.Log("Goblin Prince hit! Dealing " + attackDamage + " damage.");
            return;
        }
        
        // Finally check for regular goblins with the Goblin tag
        if (other.CompareTag("Goblin"))
        {
            GoblinAI goblin = other.GetComponent<GoblinAI>();
            if (goblin != null)
            {
                goblin.TakeDamage(attackDamage);
                Debug.Log("Goblin hit! Dealing " + attackDamage + " damage.");
                return;
            }
        }
    }
}