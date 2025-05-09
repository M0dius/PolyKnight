using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    // List of collected key IDs
    private List<int> collectedKeys = new List<int>();
    
    // UI references (optional)
    public GameObject keyIconPrefab;
    public Transform keyUIParent;
    
    // Events
    public delegate void KeyCollectedHandler(int keyID);
    public event KeyCollectedHandler OnKeyCollected;
    
    public void AddKey(int keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"Collected key with ID: {keyID}");
            
            // Trigger event
            if (OnKeyCollected != null)
            {
                OnKeyCollected(keyID);
            }
            
            // Update UI (optional)
            UpdateKeyUI();
        }
    }
    
    public bool HasKey(int keyID)
    {
        return collectedKeys.Contains(keyID);
    }
    
    public void RemoveKey(int keyID)
    {
        if (collectedKeys.Contains(keyID))
        {
            collectedKeys.Remove(keyID);
            
            // Update UI (optional)
            UpdateKeyUI();
        }
    }
    
    public void ClearAllKeys()
    {
        collectedKeys.Clear();
        
        // Update UI (optional)
        UpdateKeyUI();
    }
    
    private void UpdateKeyUI()
    {
        // Optional: Update UI to show collected keys
        if (keyUIParent != null && keyIconPrefab != null)
        {
            // Clear existing icons
            foreach (Transform child in keyUIParent)
            {
                Destroy(child.gameObject);
            }
            
            // Create new icons for each key
            foreach (int keyID in collectedKeys)
            {
                GameObject keyIcon = Instantiate(keyIconPrefab, keyUIParent);
                // You can customize the icon based on keyID if needed
            }
        }
    }
}