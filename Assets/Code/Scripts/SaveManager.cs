using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SAVE_PATH = 
        Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        File.WriteAllText(SAVE_PATH, JsonUtility.ToJson(data));
    }

    public static SaveData Load()
    {
        return File.Exists(SAVE_PATH) ? 
            JsonUtility.FromJson<SaveData>(File.ReadAllText(SAVE_PATH)) : 
            new SaveData();
    }
}