using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    public int keyID = 1; // Unique ID for each key
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    private Vector3 startPosition;
    private AudioSource audioSource;

    AudioManager audioManager;

    public void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        startPosition = transform.position;
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Make sure it has a collider
        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }
    
    void Update()
    {
        // Rotate the key
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player collected key with ID: {keyID}");
            
            // Save key collection to PlayerPrefs
            PlayerPrefs.SetInt($"HasKey_Level_{keyID}", 1);
            PlayerPrefs.Save();
            
            // Play effects
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            if (audioSource != null && pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            audioManager.PlaySFX(audioManager.Collect);
            // Destroy the key
            Destroy(gameObject);
        }
    }
}