using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class IdiomManager : MonoBehaviour
{
    public static IdiomManager instance { get; private set; }

    private Dictionary<string, string> language_dictionary;

    public string current_language = "Spanish";

    public List<GameObject> menu_objects;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Añade los objetos a actualizar a la instancia ya creada
            if (menu_objects.Count > 0)
                instance.menu_objects.AddRange(menu_objects);
            // Selecciona el mismo idioma que la instancia
            current_language = instance.current_language;
            Destroy(gameObject);
        }

        SetIdiom(current_language);
    }
    
    public void LoadIdiom(string language)
    {
        language_dictionary = new Dictionary<string, string>();

        string path = Path.Combine(Application.streamingAssetsPath, "idioms.csv");
        if (File.Exists(path))
        {
            string[] data = File.ReadAllLines(path);
            string[] headers = data[0].Split(';');

            int lang_index = System.Array.IndexOf(headers, language);
            if (lang_index <= -1) return; // El idioma no está en el .csv

            for (int i = 1; i < data.Length; i++)
            {
                string[] row = data[i].Split(";");
                string key = row[0];
                string value = row[lang_index];
                language_dictionary[key] = value;
            }

            current_language = language;
        }
        else
        {
            Debug.LogError("Couldn't find the language file");
        }
    }

    /// <summary>
    /// Devuelve el texto en el idioma actual relacionado con la llave
    /// </summary>
    public string GetKeyText(string key)
    {
        if (language_dictionary.ContainsKey(key))
            return language_dictionary[key];

        return null;
    }

    public void SetIdiom(string language)
    {
        LoadIdiom(language);

        foreach(sc_ShowText show_text in FindObjectsOfType<sc_ShowText>())
        {
            show_text.LoadText();
        }
        /// Consigue la frase según el nombre del objeto
        UpdateMenuTexts();
    }

    void UpdateMenuTexts()
    {
        foreach (GameObject obj in menu_objects)
        {
            if (obj != null)
                UpdateTextsRecursively(obj.transform);
        }
    }

    void UpdateTextsRecursively(Transform current)
    {
        TMP_Text txt = current.GetComponent<TMP_Text>();
        if (txt != null)
        {
            string key_text = GetKeyText(current.name);
            if (key_text != null)
            {
                txt.text = GetKeyText(current.name);
            }
        }

        foreach (Transform child in current)
        {
            UpdateTextsRecursively(child);
        }
    }
}