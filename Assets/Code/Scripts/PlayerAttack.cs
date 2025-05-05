using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public float attackDuration = 0.5f; // Adjust based on your animation length

    private bool isAttacking = false;
    private float attackTimer = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                EndAttack();
            }
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        animator.SetBool("isAttacking", true);
    }

    void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
}