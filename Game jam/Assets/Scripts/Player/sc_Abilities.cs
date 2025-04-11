using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_Abilities : MonoBehaviour
{
    // Booleanos que detectan colisiones
    private bool active_levitate = false, active_group = false, active_byebye = false;


    [HideInInspector]
    public bool active_hook = false;
    private bool check_collisions = false;
    private List<GameObject> enemy_targets = new List<GameObject>();
    private List<EnemyFollow> enemies_mov = new List<EnemyFollow>();
    private List<GameObject> placed_mines = new List<GameObject>();

    [Header("Audio clips")]
    public AudioClip levitate_enemy;
    public AudioClip levitate_player;
    public AudioClip ground_smash_from_air;
    public AudioClip stomp_on_ground;

    [Header("References")]
    public GameObject prefab_mine;
    public GameObject prefab_hook;
    public AudioClip clip_plant_mine;
    public LayerMask layer_enemy;

    public GameObject pr_HitNByeParticles;

    Vector3 centroid;

    //Hook
    private Hook spawned_hook;

    void Start()
    {
        spawned_hook = Instantiate(prefab_hook).GetComponent<Hook>();
    }

    private bool CheckActiveAbilities()
    {
        if (active_group || active_levitate)
            return true;
        return false;
    }

    #region Levitate
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
    #endregion

    #region Group
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
            for (int i = 0; i < enemy_targets.Count && i < enemies_mov.Count; i++)
            {
                if (enemy_targets[i] == null || enemies_mov[i] == null) continue;

                Vector3 dir = ReturnScript.instance.transform.position - enemy_targets[i].transform.position;
                dir = new Vector3(dir.x, dir.y + enemies_mov[i].rb.velocity.y, dir.z);
                enemies_mov[i].AddForceToEnemy(dir.normalized * Mathf.Clamp(enemy_targets.Count, 2, 16));
            }

            yield return null;
        }


        foreach(EnemyFollow ef in enemies_mov)
        {
            if (ef == null) continue;
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
    #endregion

    #region PlantMines
    public void PlantMines()
    {
        StartCoroutine(PlantMinesRoutine());
    }
    private IEnumerator PlantMinesRoutine()
    {
        // Destruye las minas colocadas anteriormente
        for (int i = 0; i < placed_mines.Count; i++)
        {
            Destroy(placed_mines[i]);
        }
        
        while (ReturnScript.instance.returning)
        {
            // A los 30 segundos de crearse la mina se destruirá
            GameObject mine = Instantiate(prefab_mine, ReturnScript.instance.transform.position, prefab_mine.transform.rotation);
            SoundManager.instance.InstantiateSound(clip_plant_mine, ReturnScript.instance.transform.position);
            Destroy(mine, 30);
            placed_mines.Add(mine);

            // Plantará una mina cada segundo mientras vuelves
            yield return new WaitForSeconds(1);
        }
    }
    #endregion

    // Launch hook
    public void LaunchHook()
    {
        if (!active_hook)
        {
            spawned_hook.Launch();
            active_hook = true;
        }
        else
        {
            spawned_hook.LaunchPlayer();
        }
    }

    #region Stomp
    // Stomp on ground
    public void StompOnGround()
    {
        StartCoroutine(StompRoutine());
    }
    private IEnumerator StompRoutine()
    {
        /// Cantidad de fuerza que se aplicará
        int force = 10;

        Rigidbody rb = PlayerMovement.instance.rb;

        float past_target_speed = PlayerMovement.instance.target_speed;
        PlayerMovement.instance.target_speed = PlayerMovement.instance.speed * 0.25f;
        PlayerMovement.instance.current_speed = PlayerMovement.instance.target_speed;

        if (PlayerMovement.instance.onGround)
        {
            PlayerMovement.instance.rb.velocity = Vector3.up * force;
            PlayerMovement.instance.rb.useGravity = true;
            SoundManager.instance.InstantiateSound(levitate_player, transform.position);

            // Espera a que el jugador deje de subir
            while (rb.velocity.y > 0)
            {
                yield return null;
            }
        }

        Physics.Raycast(transform.position, Vector2.down, out RaycastHit hit);

        // Hace que el jugador caiga rápidamente al suelo
        rb.AddForce(Vector3.down * (force * 2), ForceMode.VelocityChange);


        // Espera a que el jugador toque el suelo
        while (!PlayerMovement.instance.onGround)
            yield return null;


        PlayerMovement.instance.target_speed = past_target_speed;
        PlayerMovement.instance.current_speed = PlayerMovement.instance.target_speed;

        rb.velocity *= 0.5f;
        SoundManager.instance.InstantiateSound(stomp_on_ground, transform.position);

        // Detecta a todos los enemigos alcanzados y los envía por los aires
        Collider[] colls = Physics.OverlapSphere(transform.position, 5, layer_enemy);
        foreach (Collider coll in colls)
        {
            coll.GetComponent<EnemyFollow>().AddForceToEnemy(Vector3.up * ((force + hit.distance * 1.5f) * 0.5f));
            coll.GetComponent<EnemyLife>().Damage((int)((hit.distance) * 0.25f));
        }
    }
    #endregion

    #region HitNByebye
    // Byebye ability
    public void HitNByeBye()
    {
        if (active_byebye && enemies_mov.Count > 0 && enemies_mov[enemies_mov.Count - 1] != null)
        {
            StartCoroutine(ByeByeRoutine(enemies_mov[enemies_mov.Count - 1], enemies_mov.Count - 1));
        }
        else
            active_byebye = true;
    }

    /// <summary>
    /// Empujará a los enemigos que se encuentre e_fl mientras este no sea kinematic
    /// </summary>
    private IEnumerator ByeByeRoutine(EnemyFollow e_fl, int enemy_id)
    {
        if (e_fl != null)
        {
            Vector3 d = (e_fl.transform.position - transform.position).normalized;
            e_fl.agent.enabled = false;
            e_fl.rb.isKinematic = false;
            e_fl.AddForceToEnemy(d * (ReturnScript.instance.damage * 0.5f));

            EnemyFollow ef = e_fl;
            while (ef != null && !ef.rb.isKinematic)
            {
                Collider[] colls = Physics.OverlapSphere(ef.transform.position, 1.25f, layer_enemy);
                if (colls.Length > 0)
                {
                    foreach (Collider coll in colls)
                    {
                        if (coll.transform.parent.name == e_fl.transform.parent.name) continue;

                        if (ef != null)
                        {
                            Vector3 dir = (coll.transform.position - ef.transform.position).normalized;
                            ef.agent.enabled = false;
                            ef.rb.isKinematic = false;
                            Debug.Log(Mathf.Abs(ef.rb.velocity.x) + Mathf.Abs(ef.rb.velocity.z));
                            coll.GetComponent<EnemyFollow>().AddForceToEnemy((dir + d) * ((ReturnScript.instance.damage * 0.25f) * (Mathf.Abs(ef.rb.velocity.x) + Mathf.Abs(ef.rb.velocity.z))));

                            // Por cada choque se irá frenando
                            ef.rb.velocity *= 0.5f;
                        }
                    }
                }

                ef.rb.velocity *= 0.99f;

                yield return null;
            }

            if (!CheckActiveAbilities() && enemy_id < enemies_mov.Count)
                enemies_mov.RemoveAt(enemy_id);
        }
    }
    #endregion

    /// <summary>
    /// El jugador empezará al curarse al dañar a los enemigos
    /// </summary>
    public void BloodThirsty()
    {
        ReturnScript.instance.can_heal = true;
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

            if (!ReturnScript.instance.returning && !PlayerMovement.instance.slide && active_byebye)
            {
                // Cada vez que golpeen al jugador tiene una posibilidad de empujarlos
                int rand = 0;
                if (rand == 0)
                {
                    Vector3 dir = (other.transform.position - transform.position).normalized;

                    Destroy(Instantiate(pr_HitNByeParticles, transform.position + dir * transform.localScale.x, Quaternion.LookRotation(dir, Vector3.up)), 1);

                    EnemyFollow e_fl = other.GetComponent<EnemyFollow>();

                    if (e_fl != null && !enemies_mov.Contains(e_fl))
                    {
                        enemies_mov.Add(other.GetComponent<EnemyFollow>());
                        enemy_targets.Add(other.gameObject);

                        HitNByeBye();
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(centroid, 2);
    }
}