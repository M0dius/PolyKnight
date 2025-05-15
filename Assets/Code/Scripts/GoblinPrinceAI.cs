using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GoblinPrinceAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    
    [Header("Goblin Prince Stats")]
    public float health = 60f;
    public float damage = 10f;
    public float detectionRange = 55f;
    public float attackRange = 1.8f;
    public float attackCooldown = 2.5f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 110f;
    public float acceleration = 7f;

    [Header("Effects")]
    public ParticleSystem deathEffectPrefab;
    public Transform bloodSpawnPoint;
    [Tooltip("Rotation offset for the blood splatter effect")]
    public Vector3 bloodRotationOffset = new Vector3(-90, 0, 0);
    
    [Header("UI Elements")]
    public Slider healthSlider;
    public Transform healthUI;
    
    [Header("Coin Drop Settings")]
    public GameObject coinPrefab;
    public int minCoins = 3;
    public int maxCoins = 8;
    public float coinDropForce = 3f;
    public float coinSpreadRadius = 1f;
    public bool alwaysDropCoins = true;
    public float coinDropChance = 1f;
    
    [Header("Visual Settings")]
    public float princeScale = 1.3f;
    public bool shouldCreateCrown = true;
    public Vector3 crownPosition = new Vector3(0, 1.0f, 0);
    public Vector3 crownRotation = new Vector3(0, 0, 0);
    public Vector3 crownScale = new Vector3(0.5f, 0.5f, 0.5f);
    
    [Header("Key Drop Settings")]
    public bool isLastEnemy = false; // Set this to true for the last goblin in each level
    public GameObject keyPrefab;
    public bool useAutoLastEnemy = true; // Automatically determine if this is the last enemy
    
    // Public reference to player that can be set in inspector
    public Transform playerDirectReference;
    
    // Manual player position (use this instead of trying to find player)
    [Header("Manual Player Settings")]
    public Vector3 manualPlayerPosition = new Vector3(-8.582f, 1.15f, -1.0f);
    public bool useManualPosition = false; // Set to false to follow the actual player
    
    [Header("Audio Settings")]
    public AudioManager audioManager;
    
    // State management
    private enum PrinceState { Idle, Chasing, Attacking, Stunned }
    private PrinceState currentState = PrinceState.Idle;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    private float nextAttackTime = 0f;
    
    // Special ability cooldowns
    private float lastSpecialAttackTime = 0f;
    private float specialAttackCooldown = 8f;
    
    // Animator parameter names
    private string walkParam = "isWalking";
    private string sprintParam = "isSprinting";
    private string attackParam = "attack";
    private string dieParam = "die1";
    private string kickLeftParam = "kickLeft";
    private string kickRightParam = "kickRight";
    
    void Start()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Set up NavMeshAgent
        if (agent != null) {
            agent.speed = moveSpeed;
            agent.angularSpeed = rotationSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = attackRange * 0.8f;
        } else {
            Debug.LogError("❗ NavMeshAgent component missing on Goblin Prince!");
        }
        
        // Find audio manager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("⚠️ AudioManager not found in scene!");
            }
        }
        
        // Find player using a reliable method
        if (playerDirectReference != null) {
            player = playerDirectReference;
            Debug.Log("✅ Prince using direct player reference: " + player.name);
        } else {
            // Try to find by Player component
            var playerComponent = FindObjectOfType<Player>();
            if (playerComponent != null) {
                if (playerComponent.Character != null) {
                    player = playerComponent.Character.transform;
                } else {
                    player = playerComponent.transform;
                }
            } else {
                // Try to find by tag
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) {
                    player = playerObj.transform;
                } else {
                    // If all else fails, use manual position
                    if (useManualPosition) {
                        GameObject tempObj = new GameObject("TempPlayerPosition");
                        tempObj.transform.position = manualPlayerPosition;
                        player = tempObj.transform;
                        tempObj.hideFlags = HideFlags.HideAndDontSave;
                    } else {
                        Debug.LogError("❗ No player found for Goblin Prince to target!");
                    }
                }
            }
        }
        
        // Initialize health UI
        if (healthSlider != null) {
            healthSlider.maxValue = health;
            healthSlider.value = health;
        }
        
        // Apply visual changes to make the Prince distinct
        ApplyPrinceVisuals();
        
        // Create crown if needed
        if (shouldCreateCrown) {
            CreateCrown();
        }
        
        // Set initial state
        currentState = PrinceState.Idle;
        lastAttackTime = -attackCooldown; // Allow immediate attack
    }
    
    private void ApplyPrinceVisuals()
    {
        // Make the Prince larger than regular goblins
        transform.localScale = Vector3.one * princeScale;
        
        // Apply purple tint to make it visually distinct
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Skip if this is part of the crown
                if (renderer.transform.parent != null && 
                    renderer.transform.parent.name.Contains("Crown"))
                    continue;
                    
                // Completely change the goblin's skin color to blue
                renderer.material.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            }
        }
        
        Debug.Log("👑 Applied visual changes to Goblin Prince");
    }
    
    private void CreateCrown()
    {
        // Create crown game object
        GameObject crown = new GameObject("PrinceCrown");
        crown.transform.SetParent(transform);
        
        // Position the crown on the head
        crown.transform.localPosition = crownPosition;
        crown.transform.localRotation = Quaternion.Euler(crownRotation);
        crown.transform.localScale = crownScale;
        
        // Create the crown base
        GameObject crownBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crownBase.transform.SetParent(crown.transform);
        crownBase.transform.localPosition = Vector3.zero;
        crownBase.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        
        // Create crown spikes using capsules
        int spikes = 5;
        for (int i = 0; i < spikes; i++)
        {
            float angle = i * (360f / spikes);
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            spike.transform.SetParent(crown.transform);
            
            // Position spike around the crown
            float radius = 0.25f;
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            spike.transform.localPosition = new Vector3(x, 0.15f, z);
            
            // Rotate spike to point outward
            spike.transform.localRotation = Quaternion.Euler(90, 0, 0);
            spike.transform.Rotate(0, angle, 0);
            
            // Scale spike to make it look pointy
            spike.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
        }
        
        // Apply silver material to all crown parts
        Renderer[] renderers = crown.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material crownMaterial = new Material(Shader.Find("Standard"));
            crownMaterial.color = new Color(0.75f, 0.75f, 0.8f); // Silver color
            crownMaterial.SetFloat("_Metallic", 0.9f);
            crownMaterial.SetFloat("_Glossiness", 0.8f);
            renderer.material = crownMaterial;
        }
        
        Debug.Log("👑 Created crown for Goblin Prince");
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Get current player position
        Vector3 targetPosition;
        if (player != null) {
            targetPosition = player.position;
        } else if (useManualPosition) {
            targetPosition = manualPlayerPosition;
        } else {
            return; // No target to chase
        }
        
        // Update health UI position
        if (healthUI != null && Camera.main != null) {
            Vector3 offset = new Vector3(0f, 1.5f, 0f);
            healthUI.position = transform.position + offset;
            healthUI.LookAt(Camera.main.transform);
            healthUI.Rotate(0, 180, 0);
        }
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, targetPosition);
        
        // State machine
        switch (currentState) {
            case PrinceState.Idle:
                if (distanceToPlayer <= detectionRange) {
                    currentState = PrinceState.Chasing;
                }
                break;
                
            case PrinceState.Chasing:
                // Chase the player
                if (agent != null) {
                    agent.SetDestination(targetPosition);
                    
                    // Set animation parameters
                    if (animator != null) {
                        animator.SetBool(walkParam, true);
                        animator.SetBool(sprintParam, distanceToPlayer > 10f);
                    }
                }
                
                // Check if in attack range
                if (distanceToPlayer <= attackRange) {
                    currentState = PrinceState.Attacking;
                }
                break;
                
            case PrinceState.Attacking:
                // Face the player
                Vector3 direction = (targetPosition - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero) {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                }
                
                // Attack if cooldown elapsed
                if (Time.time >= nextAttackTime) {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                
                // Check special attack cooldown
                if (Time.time - lastSpecialAttackTime >= specialAttackCooldown) {
                    SpecialAttack();
                    lastSpecialAttackTime = Time.time;
                }
                
                // Return to chase if player moves out of range
                if (distanceToPlayer > attackRange * 1.2f) {
                    currentState = PrinceState.Chasing;
                }
                break;
                
            case PrinceState.Stunned:
                // Do nothing while stunned
                break;
        }
    }
    
    void Attack()
    {
        // Choose a random attack type
        int attackType = Random.Range(0, 3);
        Debug.Log($"[GoblinPrince] Attacking with type {attackType}");
        
        // Apply damage to player
        if (player != null)
        {
            // Try to find PlayerHealth component
            PlayerHealth playerHealth = null;
            
            // Check player GameObject first
            playerHealth = player.GetComponent<PlayerHealth>();
            
            // If not found, check parent
            if (playerHealth == null && player.parent != null)
                playerHealth = player.parent.GetComponent<PlayerHealth>();
                
            // If still not found, check children
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
                
            // If found, apply damage directly
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[GoblinPrince] Applied {damage} damage to player!");
            }
            else
            {
                // Last resort: try to find PlayerHealth in the scene
                playerHealth = FindObjectOfType<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        // Play attack animation
        if (animator != null)
        {
            switch (attackType)
            {
                case 0:
                    animator.SetTrigger(attackParam);
                    break;
                case 1:
                    animator.SetTrigger(kickLeftParam);
                    break;
                case 2:
                    animator.SetTrigger(kickRightParam);
                    break;
            }
        }
        
        // Visual effect for prince attack
        StartCoroutine(AttackVisualEffect());
    }
    
    void SpecialAttack()
    {
        // Prince's special attack - a ground pound that stuns nearby enemies
        Debug.Log("[GoblinPrince] Performing special attack!");
        
        // Play special attack animation if available, otherwise use regular attack
        if (animator != null)
        {
            animator.SetTrigger(attackParam);
        }
        
        // Visual effect for special attack
        StartCoroutine(SpecialAttackEffect());
        
        // Apply damage in an area around the prince
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange * 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            // Check if it's the player
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Apply extra damage for special attack
                playerHealth.TakeDamage(damage * 1.5f);
                Debug.Log($"[GoblinPrince] Special attack hit player for {damage * 1.5f} damage!");
            }
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("💢 Prince took " + amount + " damage. Health: " + health);

        // Update health UI
        if (healthSlider != null)
            healthSlider.value = health;

        // Visual feedback
        StartCoroutine(DamageFlash());

        if (health <= 0)
            Die();
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log("☠️ Goblin Prince has been defeated!");

        // Spawn blood effect
        SpawnBloodEffect();

        // Play death animation
        if (animator != null) animator.SetTrigger(dieParam);
        
        // Disable NavMeshAgent
        if (agent != null) agent.enabled = false;

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Drop coins
        DropCoins();
        
        // Check if this is the last enemy and drop key
        if (isLastEnemy || (useAutoLastEnemy && IsLastEnemyInLevel()))
        {
            DropKey();
        }
        
        // Hide health UI
        if (healthUI != null)
            healthUI.gameObject.SetActive(false);

        // Play death sound
        if (audioManager != null && audioManager.goblinDeath != null)
        {
            audioManager.PlaySFX(audioManager.goblinDeath);
        }

        // Death effect
        StartCoroutine(DeathEffect());

        // Destroy after delay
        Destroy(gameObject, 4f);
    }
    
    private bool IsLastEnemyInLevel()
    {
        // Find all goblins in the scene (both regular and princes)
        GoblinAI[] regularGoblins = FindObjectsOfType<GoblinAI>();
        GoblinPrinceAI[] princes = FindObjectsOfType<GoblinPrinceAI>();
        
        // Count alive enemies
        int aliveCount = 0;
        
        // Count alive regular goblins
        foreach (var goblin in regularGoblins)
        {
            if (goblin != null && !goblin.isDead)
                aliveCount++;
        }
        
        // Count alive princes (excluding this one since it's about to die)
        foreach (var prince in princes)
        {
            if (prince != null && prince != this && !prince.isDead)
                aliveCount++;
        }
        
        return aliveCount == 0;
    }
    
    private IEnumerator AttackVisualEffect()
    {
        // Flash the prince's color during attack
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];
        
        // Store original colors and set to intense color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
                renderers[i].material.color = Color.Lerp(renderers[i].material.color, Color.red, 0.3f);
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
                renderers[i].material.color = originalColors[i];
        }
    }
    
    private IEnumerator SpecialAttackEffect()
    {
        // Create a shockwave effect
        GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shockwave.transform.position = transform.position + Vector3.up * 0.1f;
        shockwave.GetComponent<Collider>().enabled = false;
        
        // Create a material for the shockwave
        Material shockwaveMaterial = new Material(Shader.Find("Standard"));
        shockwaveMaterial.color = new Color(0.7f, 0.3f, 0.7f, 0.5f); // Purple
        shockwaveMaterial.SetFloat("_Metallic", 0f);
        shockwaveMaterial.SetFloat("_Glossiness", 0.7f);
        shockwaveMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shockwaveMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shockwaveMaterial.SetInt("_ZWrite", 0);
        shockwaveMaterial.DisableKeyword("_ALPHATEST_ON");
        shockwaveMaterial.EnableKeyword("_ALPHABLEND_ON");
        shockwaveMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        shockwaveMaterial.renderQueue = 3000;
        
        // Apply the material
        Renderer shockwaveRenderer = shockwave.GetComponent<Renderer>();
        shockwaveRenderer.material = shockwaveMaterial;
        
        // Animate the shockwave
        float duration = 0.8f;
        float startTime = Time.time;
        float endTime = startTime + duration;
        
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            shockwave.transform.localScale = Vector3.one * t * 3f;
            shockwaveMaterial.color = new Color(0.7f, 0.3f, 0.7f, 0.5f * (1 - t));
            yield return null;
        }
        
        Destroy(shockwave);
    }
    
    private IEnumerator DamageFlash()
    {
        // Flash red when taking damage
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];
        
        // Store original colors and set to red
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
                renderers[i].material.color = Color.red;
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
                renderers[i].material.color = originalColors[i];
        }
    }
    
    private IEnumerator DeathEffect()
    {
        // Create a death effect
        GameObject deathEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        deathEffect.transform.position = transform.position;
        deathEffect.GetComponent<Collider>().enabled = false;
        
        // Create a material for the death effect
        Material deathMaterial = new Material(Shader.Find("Standard"));
        deathMaterial.color = new Color(0.7f, 0.3f, 0.7f, 0.7f); // Purple
        deathMaterial.SetFloat("_Metallic", 0f);
        deathMaterial.SetFloat("_Glossiness", 0.7f);
        deathMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        deathMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        deathMaterial.SetInt("_ZWrite", 0);
        deathMaterial.DisableKeyword("_ALPHATEST_ON");
        deathMaterial.EnableKeyword("_ALPHABLEND_ON");
        deathMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        deathMaterial.renderQueue = 3000;
        
        // Apply the material
        Renderer deathRenderer = deathEffect.GetComponent<Renderer>();
        deathRenderer.material = deathMaterial;
        
        // Animate the death effect
        float duration = 2.0f;
        float startTime = Time.time;
        float endTime = startTime + duration;
        
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            deathEffect.transform.localScale = Vector3.one * (1 + t * 3f);
            deathMaterial.color = new Color(0.7f, 0.3f, 0.7f, 0.7f * (1 - t));
            yield return null;
        }
        
        Destroy(deathEffect);
    }
    
    private void SpawnBloodEffect()
    {
        if (deathEffectPrefab != null)
        {
            // Determine spawn position
            Vector3 spawnPos;
            if (bloodSpawnPoint != null)
            {
                spawnPos = bloodSpawnPoint.position;
            }
            else
            {
                // Fallback: Use raycast to find ground below goblin
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
                {
                    // Position slightly above the hit point to avoid clipping
                    spawnPos = hit.point + hit.normal * 0.05f;
                    Debug.Log("💉 Blood effect spawned at ground position: " + spawnPos);
                }
                else
                {
                    // If no ground found, use a position slightly below the goblin
                    spawnPos = transform.position + Vector3.down * 0.5f;
                    Debug.Log("💉 Blood effect spawned at fallback position: " + spawnPos);
                }
            }

            // Create rotation that aligns with the ground or uses the specified offset
            Quaternion bloodRotation = Quaternion.Euler(bloodRotationOffset);

            // Instantiate and play the particle effect
            var bloodEffect = Instantiate(deathEffectPrefab, spawnPos, bloodRotation);
            bloodEffect.Play();

            // Calculate total lifetime including particle duration and max particle lifetime
            float maxLifetime = bloodEffect.main.duration;
            if (bloodEffect.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            {
                maxLifetime += bloodEffect.main.startLifetime.constant;
            }
            else if (bloodEffect.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            {
                maxLifetime += bloodEffect.main.startLifetime.constantMax;
            }

            // Add a small buffer to ensure all particles are gone
            maxLifetime += 1f;

            // Clean up the effect after it completes
            Destroy(bloodEffect.gameObject, maxLifetime);
            Debug.Log("💉 Blood effect will be destroyed after " + maxLifetime + " seconds");
        }
        else
        {
            Debug.LogWarning("⚠️ No deathEffectPrefab assigned for blood effect!");
        }
    }
    
    private void DropCoins()
    {
        // Check if we should drop coins
        if (!alwaysDropCoins && Random.Range(0f, 1f) > coinDropChance)
        {
            return;
        }
        
        if (coinPrefab == null)
        {
            Debug.LogWarning("⚠️ Coin prefab not assigned to GoblinPrince!");
            return;
        }
        
        // Determine number of coins to drop
        int coinsToDrop = Random.Range(minCoins, maxCoins + 1);
        
        Debug.Log($"💰 Dropping {coinsToDrop} coins from Goblin Prince");
        
        for (int i = 0; i < coinsToDrop; i++)
        {
            // Calculate random position around goblin
            Vector3 randomOffset = Random.insideUnitCircle * coinSpreadRadius;
            Vector3 coinPosition = transform.position + new Vector3(randomOffset.x, 0.5f, randomOffset.y);
            
            // Create coin
            GameObject coin = Instantiate(coinPrefab, coinPosition, Random.rotation);
            
            // Add some physics if the coin has a Rigidbody
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                Vector3 force = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized * coinDropForce;
                
                coinRb.AddForce(force, ForceMode.Impulse);
                
                // Add some random rotation
                coinRb.AddTorque(Random.insideUnitSphere * coinDropForce, ForceMode.Impulse);
            }
        }
    }

    void DropKey()
    {
        if (keyPrefab != null)
        {
            // Instantiate the key slightly above the goblin
            Vector3 keyPosition = transform.position + Vector3.up * 0.5f;
            GameObject key = Instantiate(keyPrefab, keyPosition, Quaternion.identity);
            
            // Set the key ID based on the current scene
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene.Contains("Level_"))
            {
                string levelNumber = currentScene.Replace("Level_", "");
                KeyItem keyItem = key.GetComponent<KeyItem>();
                if (keyItem != null)
                {
                    keyItem.keyID = int.Parse(levelNumber);
                }
            }
            
            Debug.Log("🔑 Key dropped by last goblin!");
        }
        else
        {
            Debug.LogWarning("⚠️ Key prefab not assigned to GoblinPrinceAI!");
        }
    }
}