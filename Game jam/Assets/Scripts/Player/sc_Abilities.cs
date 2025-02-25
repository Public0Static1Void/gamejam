using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Abilities : MonoBehaviour
{
    private bool active_levitate = false;
    private List<GameObject> enemy_targets = new List<GameObject>();
    public void LevitateEnemies(float force)
    {
        StartCoroutine(LevitateRoutine(force));
    }
    IEnumerator LevitateRoutine(float force)
    {
        while (ReturnScript.instance.returning)
        {
            // Espera a que el jugador haya acabado de usar la habilidad
            yield return null;
        }

        active_levitate = true;
        float timer = 0;
        while (active_levitate)
        {
            timer += Time.deltaTime;
            if (timer > 2)
            {
                active_levitate = false;
            }
            yield return null;
        }

        foreach (GameObject target in enemy_targets)
        {
            if (target != null)
            {
                target.GetComponent<EnemyLife>().Damage(ReturnScript.instance.damage / 2);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (active_levitate)
            {
                other.GetComponent<EnemyFollow>().AddForceToEnemy(Vector2.up * 10, ForceMode.Force);
                enemy_targets.Add(other.gameObject);
            }
        }
    }
}