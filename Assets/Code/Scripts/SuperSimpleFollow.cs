using UnityEngine;

public class SuperSimpleFollow : MonoBehaviour
{
    // The player GameObject - drag this in the inspector
    public GameObject playerObject;
    
    // Movement speed
    public float speed = 3.0f;
    
    // Minimum distance to maintain from player
    public float minDistance = 1.5f;
    
    // MANUALLY SET THIS TO THE PLAYER'S POSITION
    public Vector3 manualPlayerPosition = new Vector3(-8.582f, 1.15f, -1.0f);
    public bool useManualPosition = true;
    
    // Animation parameters
    private Animator animator;
    private string walkParam = "isWalking1";
    private string attackParam = "attack1";
    
    void Start()
    {
        // Get animator component
        animator = GetComponent<Animator>();
        
        // DEBUG: List all GameObjects in the scene
        Debug.Log("===== ALL OBJECTS IN SCENE =====");
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            Debug.Log($"Object: {obj.name}, Tag: {obj.tag}, Active: {obj.activeInHierarchy}");
        }
        Debug.Log("=================================");
        
        // If player not assigned, try to find it
        if (playerObject == null)
        {
            // Try to find the GameObject named "Character"
            playerObject = GameObject.Find("Character");
            if (playerObject != null)
                Debug.Log("Found by name 'Character': " + playerObject.name);
            
            // If not found, try to find by tag
            if (playerObject == null)
            {
                playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                    Debug.Log("Found by tag 'Player': " + playerObject.name);
            }
                
            // If still not found, try to find the Player component
            if (playerObject == null)
            {
                var playerComponent = FindObjectOfType<Player>();
                if (playerComponent != null)
                {
                    playerObject = playerComponent.gameObject;
                    Debug.Log("Found by Player component: " + playerObject.name);
                }
            }
            
            // If still not found, try to find CharacterController
            if (playerObject == null)
            {
                var characterController = FindObjectOfType<CharacterController>();
                if (characterController != null)
                {
                    playerObject = characterController.gameObject;
                    Debug.Log("Found by CharacterController component: " + playerObject.name);
                }
            }
        }
        
        // Log whether we found the player
        if (playerObject != null)
        {
            Debug.Log("SuperSimpleFollow: Found player: " + playerObject.name);
            Debug.Log("Player position: " + playerObject.transform.position);
            Debug.Log("Player parent: " + (playerObject.transform.parent != null ? playerObject.transform.parent.name : "none"));
        }
        else
        {
            Debug.LogError("SuperSimpleFollow: Could not find player!");
        }
    }
    
    void Update()
    {
        Vector3 targetPosition;
        
        // Use manual position if enabled, otherwise use player object
        if (useManualPosition)
        {
            targetPosition = manualPlayerPosition;
            
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("USING MANUAL POSITION: " + targetPosition);
                Debug.Log("MY POSITION: " + transform.position);
                Debug.Log("MY NAME: " + gameObject.name);
            }
        }
        else if (playerObject != null)
        {
            targetPosition = playerObject.transform.position;
            
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("TARGET OBJECT: " + playerObject.name);
                Debug.Log("TARGET POSITION: " + targetPosition);
                Debug.Log("MY POSITION: " + transform.position);
                Debug.Log("MY NAME: " + gameObject.name);
            }
        }
        else
        {
            // No target available
            return;
        }
        
        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Log distance occasionally
        if (Time.frameCount % 120 == 0)
            Debug.Log("SuperSimpleFollow: Distance to target: " + distance);
        
        // If we're far enough away, move toward target
        if (distance > minDistance)
        {
            // Calculate direction to target (ignore Y axis)
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            direction.Normalize();
            
            // Debug the direction
            if (Time.frameCount % 60 == 0)
                Debug.Log("MOVING DIRECTION: " + direction);
            
            // Rotate toward target
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
            }
            
            // Move toward target
            transform.position += direction * speed * Time.deltaTime;
            
            // Set walking animation
            if (animator != null)
                animator.SetBool(walkParam, true);
        }
        else
        {
            // We're close enough to attack
            if (animator != null)
            {
                animator.SetBool(walkParam, false);
                animator.SetTrigger(attackParam);
            }
        }
    }
}
