using System.Collections;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    public bool useTimeInterval = true;
    public float engagedDuration = 2f;
    public float disengagedDuration = 3f;
    public int damageAmount = 10;
    
    public Collider trapCollider;
    public GameObject trapVisual;
    
    private bool isEngaged = false;
    private Coroutine timerCoroutine;
    private bool playerInTriggerZone = false;
    
    void Start()
    {
        if (trapCollider == null)
        {
            trapCollider = GetComponent<Collider>();
            if (trapCollider == null)
            {
                Debug.LogError("No collider assigned to trap and none found on GameObject!");
            }
        }
        
        SetTrapState(false);
        
        if (useTimeInterval)
        {
            timerCoroutine = StartCoroutine(TimerCycle());
        }
    }
    
    private IEnumerator TimerCycle()
    {
        while (true)
        {
            SetTrapState(true);
            yield return new WaitForSeconds(engagedDuration);
            
            SetTrapState(false);
            yield return new WaitForSeconds(disengagedDuration);
        }
    }
    
    private void SetTrapState(bool engaged)
    {
        isEngaged = engaged;
        
        if (trapCollider != null)
        {
            trapCollider.enabled = engaged;
        }
        
        if (trapVisual != null)
        {
            trapVisual.SetActive(engaged);
        }
        
        Debug.Log($"Trap {gameObject.name} {(engaged ? "engaged" : "disengaged")}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useTimeInterval && other.CompareTag("Player"))
        {
            playerInTriggerZone = true;
            SetTrapState(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!useTimeInterval && other.CompareTag("Player"))
        {
            playerInTriggerZone = false;
            SetTrapState(false);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (isEngaged && collision.gameObject.CompareTag("Player"))
        {
            ApplyDamageToPlayer(collision.gameObject);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (isEngaged && other.CompareTag("Player"))
        {
            ApplyDamageToPlayer(other.gameObject);
        }
    }
    
    private void ApplyDamageToPlayer(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"Player took {damageAmount} damage from trap!");
        }
        else
        {
            Debug.LogWarning("Player has no PlayerHealth component to take damage!");
        }
    }
    
    private void OnDestroy()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }
}