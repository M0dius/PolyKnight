using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestInteraction : MonoBehaviour
{
      [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public string interactionButton = "E";

    [Header("Visuals")]
    public GameObject chestLid;
    public Vector3 openRotation = new Vector3(-90f, 0f, 0f);
    public float openSpeed = 5f;
    public GameObject interactionPromptUI;

    [Header("Reward")]
    public string keyName = "Chest Key";
    private bool keyGiven = false;

    [Header("Player Reference")]
    public Transform playerInteractionPoint;

    private Transform playerTransformRoot;
    private bool isInRange = false;
    private bool isOpened = false;
    private Quaternion closedRotation;

    // Static variable to track if the player has the key (accessible from other scripts if needed)
    public static bool PlayerHasKey = false;

    void Start()
    {
        playerTransformRoot = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransformRoot == null) Debug.LogError("Player not found.");

        if (playerInteractionPoint == null)
        {
            playerInteractionPoint = playerTransformRoot;
            Debug.LogWarning("Player Interaction Point not assigned, using Player root.");
        }

        if (interactionPromptUI != null)
        {
            Debug.Log("interactionPromptUI found in Start(), setting inactive."); // Added log
            interactionPromptUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("interactionPromptUI is null in Start()!"); // Added log
        }

        if (chestLid != null) closedRotation = chestLid.transform.localRotation;
    }

    void Update()
    {
        if (!isOpened && playerInteractionPoint != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerInteractionPoint.position);

            bool shouldShowPrompt = distanceToPlayer <= interactionRange;

            if (shouldShowPrompt && !isInRange)
            {
                isInRange = true;
                Debug.Log("Player entered interaction range - Showing Prompt.");
                if (interactionPromptUI != null) interactionPromptUI.SetActive(true);
            }
            else if (!shouldShowPrompt && isInRange)
            {
                isInRange = false;
                Debug.Log("Player exited interaction range - Hiding Prompt.");
                if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            }

            if (isInRange && Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
        }
    }

    void OpenChest()
    {
        isOpened = true;
        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
        if (chestLid != null) StartCoroutine(RotateLid(Quaternion.Euler(openRotation)));

        if (!keyGiven)
        {
            PlayerHasKey = true;
            keyGiven = true;
            Debug.Log("Player received the " + keyName + ".");
        }
        else
        {
            Debug.Log("Chest is already opened.");
        }
    }

    IEnumerator RotateLid(Quaternion targetRotation)
    {
        float time = 0;
        Quaternion startRotation = chestLid.transform.localRotation;
        while (time < 1)
        {
            chestLid.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, time);
            time += Time.deltaTime * openSpeed;
            yield return null;
        }
        chestLid.transform.localRotation = targetRotation;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
