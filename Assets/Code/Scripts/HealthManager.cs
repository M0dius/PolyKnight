using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static int playerHealth = 100;

    Animator myAnimator;

    private void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    //when player collides with an enemy, player loses health
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            playerHealth -= 10;
            Debug.Log("Player Health: " + playerHealth);

            if (playerHealth == 0)
            {
                myAnimator.SetBool("isDead", true);
                Destroy(gameObject, myAnimator.GetCurrentAnimatorStateInfo(0).length);
            }
        }
    }
}
