using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public bool HasKey = false;

    public bool CheckForKey()
    {
        return HasKey;
    }
}
