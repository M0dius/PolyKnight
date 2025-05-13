using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey = false;

    public bool CheckForKey()
    {
        return HasKey;
    }
}