using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class sc_door : MonoBehaviour
{

    public int valueD;

    public InputAction controls;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Buy(InputAction.CallbackContext con)
    {
        if(con.performed) 
        { 

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>().CompareTag("Player"))
        {
            if (ScoreManager.instance.score > valueD)
            {

            }
        }
    }
}
