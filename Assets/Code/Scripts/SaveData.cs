using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalGold = 0;
    
    public int currentRoom = 1;  // Always Reset at Level 1 on death
    public List<string> collectedArmor = new(); // Cleared on death
}