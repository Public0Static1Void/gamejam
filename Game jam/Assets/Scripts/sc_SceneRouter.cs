using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sc_SceneRouter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string started_file_path = Path.Combine(Application.persistentDataPath, "started.txt");
        if (File.Exists(started_file_path))
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            SceneManager.LoadScene("Tutorial");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
