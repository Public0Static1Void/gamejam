using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_bate : MonoBehaviour
{

    public void bate(InputAction.CallbackContext con)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy")) // Mientras se desliza empujar� a los enemigos
        {
            Vector3 dir = (other.transform.position - transform.position).normalized; /// Calcula la direcci�n entre t� y el enemigo
            Vector3 force_dir = new Vector3(dir.x, 0.5f, dir.z);

            other.gameObject.GetComponent<EnemyFollow>().AddForceToEnemy(force_dir * 10f);
        }
    }
}
