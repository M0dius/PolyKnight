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
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
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
                audioManager.PlaySFX(audioManager.playerAttack);
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
        if (other.CompareTag("Goblin"))
        {
            GoblinAI goblin = other.GetComponent<GoblinAI>();
            if (goblin != null)
            {
                goblin.TakeDamage(attackDamage);
                Debug.Log("Goblin hit! Dealing " + attackDamage + " damage.");
            }
        }
    }
}