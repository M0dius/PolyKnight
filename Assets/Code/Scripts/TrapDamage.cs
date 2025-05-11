using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [Header("Trap Settings")]
    public int trapDamage = 20;
    public bool oneTimeUse = false;
    public float resetTime = 3.0f;
    
    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionRadius = 1.0f;
    public float damageInterval = 1.0f;

    [Header("Effects")]
    public GameObject trapActivationEffect;
    public AudioClip trapSound;
    
    private bool isActive = true;
    private float lastDamageTime = -10f;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && trapSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (GetComponent<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1, 0.2f, 1);
        }
        
        Collider col2 = GetComponent<Collider>();
        if (col2 != null && !col2.isTrigger)
        {
            col2.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (!isActive) return;
        
        if (Time.time < lastDamageTime + damageInterval) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(playerTag))
            {
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    ActivateTrap(playerHealth);
                    return;
                }
            }
        }
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 2.0f))
        {
            if (hit.collider.CompareTag(playerTag))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    ActivateTrap(playerHealth);
                    return;
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                ActivateTrap(playerHealth);
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (!isActive) return;
        
        if (Time.time < lastDamageTime + damageInterval) return;
        
        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                ActivateTrap(playerHealth);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2.0f);
    }
    
    void ActivateTrap(PlayerHealth playerHealth)
    {
        lastDamageTime = Time.time;
        
        playerHealth.TakeDamage(trapDamage);
        
        if (trapActivationEffect != null)
        {
            Instantiate(trapActivationEffect, transform.position, Quaternion.identity);
        }
        
        if (audioSource != null && trapSound != null)
        {
            audioSource.PlayOneShot(trapSound);
        }
        
        if (oneTimeUse)
        {
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(ResetTrap());
        }
    }
    
    IEnumerator ResetTrap()
    {
        isActive = false;
        
        yield return new WaitForSeconds(resetTime);
        
        isActive = true;
    }
    
    public void TestDamagePlayer(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ActivateTrap(ph);
        }
    }
}