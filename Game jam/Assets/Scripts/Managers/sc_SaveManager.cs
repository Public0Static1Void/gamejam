using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/save_data.json", json);
    }

    public PlayerData LoadSaveData()
    {
        string path = Application.persistentDataPath + "/save_data.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        Debug.LogWarning("Save data not found");
        return null;
    }
}

[System.Serializable]
public class PlayerData
{
    public float score;
    public float xp;
     
    public float speed;
    public float stamina;
    public int damage;
    public float explosion_range;

    public int hp;
}
