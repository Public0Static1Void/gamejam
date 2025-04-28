using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Jugador
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

    // Habilidades
    public void SaveAbilities(Ability[] data)
    {
        AbilitiesDataWrapper ab_array = new AbilitiesDataWrapper();
        ab_array.abilities = data;
        string json = JsonUtility.ToJson(ab_array, true); // pretty print optional
        File.WriteAllText(Application.persistentDataPath + "/abilities_data.json", json);
    }

    public Ability[] LoadAbilitiesData()
    {
        string path = Application.persistentDataPath + "/abilities_data.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            AbilitiesDataWrapper ab_array = JsonUtility.FromJson<AbilitiesDataWrapper>(json);
            return ab_array.abilities;
        }
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
[System.Serializable]
public class AbilitiesLevel
{
    public int id;
    public float level;
}
[System.Serializable]
public class AbilitiesDataWrapper
{
    public Ability[] abilities;
}