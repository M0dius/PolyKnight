using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Goblin Stats")]
    public float health = 30f;
    public float damage = 7f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;

    public enum PositionType { Defined, Random }
    public PositionType positionType = PositionType.Defined;

    [Header("Position Settings")]
    public Vector3 areaCenter;
    public Vector3 areaSize;
    public float minDistanceFromOtherEnemies = 2f;
    public float minDistanceFromPlayer = 5f;

    private bool canAttack = true;
    private bool isDead = false;
    private bool isPositioned = false;

    // Animator parameter names (must match Animator exactly!)
    private string walkParam = "isWalking1";
    private string sprintParam = "isSprinting";
    private string attackParam = "attack1";
    private string dieParam = "die1";
    private string kickLeftParam = "kickLeft";
    private string kickRightParam = "kickRight";

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogError("❗ No player with tag 'Player' found.");

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null) agent.stoppingDistance = attackRange;

        if (animator == null)
        {
            Debug.LogError("❗ Animator not found on this object.");
        }
        else
        {
            Debug.Log("✅ Animator found. Parameters available:");
            foreach (var p in animator.parameters)
                Debug.Log($"- {p.name} ({p.type})");
        }

        if (positionType == PositionType.Random) SetRandomPosition();
        else isPositioned = true;
    }

    void Update()
    {
        if (isDead || player == null || !isPositioned) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool near = distance <= detectionRange;

        float moveSpeed = agent.velocity.magnitude;
        bool isMoving = moveSpeed > 0.05f;
        bool shouldSprint = isMoving && GameObject.FindObjectsOfType<GoblinAI>().Length <= 3;

        animator?.SetBool(walkParam, isMoving);
        animator?.SetBool(sprintParam, shouldSprint);

        if (near)
        {
            Chase();

            if (distance <= attackRange && canAttack)
                Attack();
        }
        else
        {
            agent?.SetDestination(transform.position);
            animator?.SetBool(walkParam, false);
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

    void Chase()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
            animator?.SetBool(walkParam, true);
        }
    }

    void Attack()
    {
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
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("💢 Goblin took " + amount + " damage.");

        if (health <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
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
