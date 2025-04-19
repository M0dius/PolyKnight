using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static SaveData Data { get; private set; }

    void Awake()
    {
        Data = SaveManager.Load();
    }

    // Call when passing through a door
    public void ProgressToRoom(int nextRoom, string sceneName)
    {
        Data.currentRoom = nextRoom;
        SaveManager.Save(Data);
        SceneManager.LoadScene(sceneName);
    }

    // Call when player dies
    public void HandleDeath()
    {
        Data.collectedArmor.Clear();
        Data.currentRoom = 1; //  Reset at Level 1 
        SaveManager.Save(Data);
        SceneManager.LoadScene("Room1");
    }

    // Call when purchasing upgrades
    public bool PurchaseUpgrade(int cost)
    {
        if (Data.totalGold >= cost)
        {
            Data.totalGold -= cost;
            SaveManager.Save(Data);
            return true;
        }
        return false;
    }
}