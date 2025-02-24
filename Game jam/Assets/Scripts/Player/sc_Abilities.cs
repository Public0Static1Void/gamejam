using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Abilities : MonoBehaviour
{
    private bool active_levitate = false;
    public void LevitateEnemies()
    {
        StartCoroutine(LevitareRoutine());
    }
    IEnumerator LevitareRoutine()
    {
        while (active_levitate)
        {
            // Revisa si el jugador ha acabado de usar la habilidad
            if (!ReturnScript.instance.returning)
                active_levitate = false;

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (active_levitate)
            {
                other.GetComponent<EnemyFollow>().AddForceToEnemy(Vector2.up * 10, ForceMode.Force);
            }
        }
    }
}