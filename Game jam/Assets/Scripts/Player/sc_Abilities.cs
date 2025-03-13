using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Abilities : MonoBehaviour
{
    private bool active_levitate = false, active_group = false;
    private bool check_collisions = false;
    private List<GameObject> enemy_targets = new List<GameObject>();
    private List<EnemyFollow> enemies_mov = new List<EnemyFollow>();

    [Header("Audio clips")]
    public AudioClip levitate_enemy;
    public AudioClip ground_smash_from_air;

    Vector3 centroid;

    private bool CheckActiveAbilities()
    {
        if (active_group || active_levitate)
            return true;
        return false;
    }

    public void LevitateEnemies()
    {
        StartCoroutine(LevitateRoutine());
    }
    IEnumerator LevitateRoutine()
    {
        active_levitate = true; /// El jugador empezará a detectar si choca con los enemigos
        check_collisions = true;

        while (active_levitate)
        {
            // Compueba si el jugador ha acabado de volver en el tiempo
            if (!ReturnScript.instance.returning)
            {
                active_levitate = false; /// El jugador deja de detectar colisiones
                check_collisions = CheckActiveAbilities();

                yield return new WaitForSeconds(0.25f); /// Se espera un poco para que el jugador pueda ver como se estrellan los enemigos

                for (int i = 0; i < enemies_mov.Count; i++)
                {
                    // Añade una fuerza para arriba para justo después bajar (movimiento + natural)
                    enemies_mov[i].AddForceToEnemy(Vector3.up * (ReturnScript.instance.damage * 0.025f));
                    yield return new WaitForSeconds(0.025f);
                    if (enemies_mov[i] != null)
                    {
                        // Estrella a los enemigos contra el suelo
                        enemies_mov[i].rb.useGravity = true;
                        float fall_speed = ReturnScript.instance.damage * 3;
                        enemies_mov[i].AddForceToEnemy(new Vector3(0, -Mathf.Clamp(fall_speed, 0, 25), 0));
                        // Hace que el enemigo haga un sonido
                        SoundManager.instance.PlaySoundOnAudioSource(ground_smash_from_air, enemies_mov[i].audioSource);
                    }
                    if (enemy_targets[i] != null)
                        enemy_targets[i].GetComponent<EnemyLife>().Damage(0); /// Quita vida a los enemigos

                }
            }

            yield return null;
        }
        // Se limpian las listas
        if (!CheckActiveAbilities())
        {
            check_collisions = false;
            enemies_mov.Clear();
            enemy_targets.Clear();
        }
    }

    public void GroupEnemies()
    {
        StartCoroutine(GroupRoutine());
    }
    private IEnumerator GroupRoutine()
    {
        active_group = true;
        check_collisions = true;


        while (ReturnScript.instance.returning)
        {
            for (int i = 0; i < enemy_targets.Count; i++)
            {
                Vector3 dir = ReturnScript.instance.transform.position - enemy_targets[i].transform.position;
                dir = new Vector3(dir.x, dir.y + enemies_mov[i].rb.velocity.y, dir.z);
                enemies_mov[i].AddForceToEnemy(dir.normalized * Mathf.Clamp((enemy_targets.Count * 0.5f), 0.5f, 5));
            }

            yield return null;
        }


        foreach(EnemyFollow ef in enemies_mov)
        {
            ef.rb.useGravity = true;
        }

        active_group = false;

        // Se limpian las listas
        if (!CheckActiveAbilities())
        {
            check_collisions = false;
            enemies_mov.Clear();
            enemy_targets.Clear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (check_collisions)
            {
                // Se añade una fuerza para arriba al enemigo quitándole la gravedad
                EnemyFollow e_fl = other.GetComponent<EnemyFollow>();
                e_fl.agent.enabled = false;
                e_fl.rb.useGravity = false;
                /// Si la habilidad de levitar está activa añade una fuerza vertical
                if (active_levitate)
                    e_fl.AddForceToEnemy(new Vector3(e_fl.rb.velocity.x, ReturnScript.instance.damage * 0.05f, e_fl.rb.velocity.z));

                // Suena el sonido de levitación
                if (levitate_enemy != null)
                    SoundManager.instance.PlaySound(levitate_enemy);

                // Se añade el enemigo a la lista de los que se harán mover después
                enemies_mov.Add(e_fl);
                enemy_targets.Add(other.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(centroid, 2);
    }
}