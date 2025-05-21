using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class sc_EnemyCharger : MonoBehaviour
{
    [Header("References")]
    public EnemyFollow enemyFollow;
    public LayerMask layer_collisions, layer_player;
    public GameObject air_particles;

    [Header("Stats")]
    public float detection_range, collision_range;
    public int charge_damage;

    private bool on_charge = false, on_cooldown = false;

    private float timer = 0;

    [Header("Clips")]
    public AudioClip clip_scream;
    public AudioClip clip_charge;
    public AudioClip clip_collision;
    public AudioClip clip_stomp;
    public AudioClip clip_stomp_heavy;


    void Update()
    {
        if (!on_charge && !on_cooldown)
        {
            if (Vector3.Distance(transform.position, enemyFollow.target.position) < detection_range)
            {
                if (Physics.Raycast(transform.position, (enemyFollow.target.position - transform.position).normalized, detection_range, layer_player))
                {
                    StartCoroutine(Charge());
                }
            }
        }
        else if (on_cooldown)
        {
            // Cooldown de su habilidad
            timer += Time.deltaTime;
            if (timer >= 10)
            {
                on_cooldown = false;
                on_charge = false;
                timer = 0;
            }
        }
    }

    private IEnumerator Charge()
    {
        if (!on_charge && enemyFollow.agent.isOnNavMesh)
        {
            on_charge = true;
            SoundManager.instance.InstantiateSound(clip_scream, transform.position, 1);

            float start_speed = enemyFollow.agent.speed;
            float start_angular_speed = enemyFollow.agent.angularSpeed;
            enemyFollow.agent.enabled = false;

            // Evita que el enemigo se mueva
            enemyFollow.can_move = false;

            yield return new WaitForSeconds(clip_scream.length);

            Transform tr = transform.parent;

            transform.parent = null;
            tr.position = transform.position;

            transform.SetParent(tr, true);

            // Calcula la dirección hacia el jugador
            Vector3 dir_player = (enemyFollow.target.position - tr.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir_player, Vector3.up);

            enemyFollow.rb.isKinematic = false;
            enemyFollow.rb.freezeRotation = true;

            Vector3 last_pos = tr.position;

            float timer = 0;
            float timer_vibrations = 0;
            while (Vector3.Distance(transform.position, enemyFollow.target.position) > transform.localScale.x && timer < 3)
            {
                timer += Time.deltaTime; /// Al pasar 3 segundos de carga se cancela automáticamente
                timer_vibrations += Time.deltaTime;

                tr.Translate(dir_player * (start_speed * 5) * Time.deltaTime);



                // Revisa si choca contra algún enemigo mientras carga
                Collider[] colls = Physics.OverlapSphere(transform.position, collision_range, layer_collisions);
                if (colls.Length > 0)
                {
                    for (int i = 0; i < colls.Length; i++)
                    {
                        if (colls[i].transform.parent.name == transform.parent.name) continue; /// Ignora las colisiones contra sí mismo

                        Vector3 dir = (transform.position - colls[i].transform.position).normalized;
                        EnemyFollow ef = colls[i].GetComponent<EnemyFollow>();
                        if (ef != null)
                        {
                            ef.AddForceToEnemy(-dir * (enemyFollow.mass * 0.1f));
                        }
                    }
                }
                // Vibraciones de cámara
                if (timer_vibrations >= 0.35f)
                {
                    /// Sonarán sonidos de pasa y vibrará la cámara si está en el suelo
                    if (Physics.Raycast(tr.position, Vector2.down, transform.localScale.y + 1))
                    {
                        SoundManager.instance.InstantiateSound(clip_stomp, transform.position);
                        float distance = Vector3.Distance(transform.position, enemyFollow.target.position);
                        if (distance < detection_range * 1.25f)
                        {
                            CameraRotation.instance.ShakeCamera(0.05f + distance * 0.25f, 0.25f);
                        }
                        timer_vibrations = 0;
                    }

                    if (Vector3.Distance(tr.position, last_pos) < 0.25f)
                    {
                        // El enemigo se ha quedado atascado
                        Debug.Log("Charger atascado");
                        break;
                    }
                    else
                    {
                        last_pos = tr.position;
                    }
                }
                yield return null;
            }


            GameManager.gm.SpawnShpereRadius(transform.position, detection_range, Color.red, true, 25);

            /// Crea las partículas
            Instantiate(air_particles, tr.position, air_particles.transform.rotation);

            SoundManager.instance.InstantiateSound(clip_stomp_heavy, tr.position, 1);

            // Mira si el jugador está en su rango
            Collider[] collss = Physics.OverlapSphere(transform.position, collision_range, layer_player);
            if (collss.Length > 0)
            {
                foreach (Collider coll in collss)
                {
                    PlayerLife pl = coll.GetComponent<PlayerLife>();
                    if (pl != null)
                        pl.Damage(charge_damage);
                    PlayerMovement pm = coll.GetComponent<PlayerMovement>();
                    if (pm != null)
                        pm.rb.AddForce(Vector3.up * (enemyFollow.mass * 0.05f), ForceMode.Impulse);
                }
            }

            enemyFollow.can_move = true;

            yield return new WaitForSeconds(2);


            enemyFollow.rb.velocity = Vector3.zero;

            enemyFollow.rb.freezeRotation = false;
            enemyFollow.agent.enabled = true;

            on_charge = false;
            on_cooldown = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collision_range);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }
}