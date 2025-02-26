using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Abilities : MonoBehaviour
{
    private bool active_levitate = false;
    private List<GameObject> enemy_targets = new List<GameObject>();
    private List<EnemyFollow> enemies_mov = new List<EnemyFollow>();

    public void LevitateEnemies()
    {
        StartCoroutine(LevitateRoutine());
    }
    IEnumerator LevitateRoutine()
    {
        active_levitate = true; /// Ahora el jugador empezará a detectar si choca con los enemigos

        while (active_levitate)
        {
            // Compueba si el jugador ha acabado de volver en el tiempo
            if (!ReturnScript.instance.returning)
            {
                active_levitate = false; /// El jugador deja de detectar colisiones
                yield return new WaitForSeconds(0.25f);
                for (int i = 0; i < enemies_mov.Count; i++)
                {
                    enemies_mov[i].AddForceToEnemy(Vector3.up * (ReturnScript.instance.damage * 0.025f));
                    yield return new WaitForSeconds(0.025f);
                    if (enemies_mov[i] != null)
                    {
                        enemies_mov[i].rb.useGravity = true;
                        float fall_speed = ReturnScript.instance.damage * 3;
                        enemies_mov[i].AddForceToEnemy(new Vector3(0, -Mathf.Clamp(fall_speed, 0, 25), 0));
                    }
                    if (enemy_targets[i] != null)
                        enemy_targets[i].GetComponent<EnemyLife>().Damage(0); /// Quita vida a los enemigos

                }
            }

            yield return null;
        }
        enemies_mov.Clear();
        enemy_targets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (active_levitate)
            {
                Debug.Log("Collision");
                enemy_targets.Add(other.gameObject);

                EnemyFollow e_fl = other.GetComponent<EnemyFollow>();
                e_fl.agent.enabled = false;
                e_fl.rb.useGravity = false;
                e_fl.AddForceToEnemy(new Vector3(0, ReturnScript.instance.damage * 0.05f, 0));

                enemies_mov.Add(e_fl);
            }
        }
    }
}