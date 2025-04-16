using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float detection_range;
    public float cooldown;


    [Header("References")]
    public LayerMask layer_enemy, layer_player;

    private GameObject player;
    private Vector3 launch_point;
    public GameObject target_hooked;

    public bool enemy_hooked = false;

    private EnemyFollow enemyFollow;

    private sc_Abilities abs;

    private float timer = 0;

    private bool launched = false, can_launch = true;
    public bool launch_player = false;
    private bool added_force_to_player = false;

    private MeshRenderer ob_renderer;

    public Ability ab_hook;

    private AudioSource audioSource;
    public AudioClip clip_chainmoving, clip_chainpulled;

    public Animator anim;

    private PlayerLife playerLife;

    float gravity_timer = 0;

    private void Start()
    {
        player = ReturnScript.instance.gameObject;

        abs = ReturnScript.instance.GetComponent<sc_Abilities>();

        ob_renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        ob_renderer.enabled = false;

        Debug.Log("[sc_Hook.cs] Remember to put the left hand of the player as the second child");
        anim = ReturnScript.instance.transform.GetChild(1).GetComponent<Animator>();

        playerLife = ReturnScript.instance.gameObject.GetComponent<PlayerLife>();
    }

    // Se configuran las variables del hook para posteriormente lanzarse
    public void Launch()
    {
        if (!can_launch || (ab_hook != null && ab_hook.onExecution)) return;

        abs.active_hook = true;

        transform.parent = null;

        transform.position = player.transform.position - player.transform.right;
        if (PlayerMovement.instance.onGround)
            transform.rotation = player.transform.rotation;
        else
            transform.rotation = Camera.main.transform.rotation;

        launch_point = transform.position;


        timer = 0;
        gravity_timer = 0;


        ab_hook = AttackSystem.instance.GetCurrentAbility();
        if (ab_hook.type != AbilitiesSystem.Abilities.HOOK)
            Debug.LogWarning("The ability type on sc_hook dosen't coincide!!");
        
        ab_hook.onExecution = true;
        ab_hook.current_cooldown = 0;

        audioSource = SoundManager.instance.InstantiateSound(clip_chainmoving, transform.position);
        audioSource.loop = true;

        anim.SetBool("Launch", true);

        ob_renderer.enabled = true;
        enemy_hooked = false;
        added_force_to_player = false;

        launched = true;
    }

    void Update()
    {
        // Cooldown de la abilidad
        if (!can_launch)
        {
            if (ab_hook.current_cooldown < cooldown)
                ab_hook.current_cooldown += Time.deltaTime;
            else
                can_launch = true;
        }

        if (!launched) return;

        audioSource.transform.position = player.transform.position;

        if (enemy_hooked)
        {
            timer += Time.deltaTime;
            if (enemyFollow == null) timer = 1; /// Si no se ha enganchado a un enemigo se salta la espera
            if (timer > 0.75f)
            {
                // El jugador no se acercará al enemigo con un salto
                if (!launch_player)
                {
                    PullHook();
                }
                else if (target_hooked != null && enemyFollow != null) // El jugador se acercará al enemigo con un salto
                {
                    if (!added_force_to_player)
                    {
                        enemyFollow.agent.enabled = false;

                        Vector3 dir = (target_hooked.transform.position - transform.position).normalized;
                        float dist = Vector3.Distance(player.transform.position, target_hooked.transform.position);

                        PlayerMovement.instance.canMove = false;
                        PlayerMovement.instance.onGround = false;

                        player.GetComponent<Rigidbody>().AddForce((-dir + Vector3.up * 2) * (dist / 2), ForceMode.VelocityChange);

                        audioSource.Stop();
                        SoundManager.instance.InstantiateSound(clip_chainpulled, player.transform.position);
                        
                        added_force_to_player = true;
                    }

                    if (PlayerMovement.instance.rb.velocity.y < -0.25f && PlayerMovement.instance.onGround)
                    {
                        HideHook();
                    }
                }
                else
                {
                    PullHook();
                }
                
            }
        }
        else
        {
            gravity_timer += Time.deltaTime;
            transform.Translate(((transform.forward * speed) + (-Vector3.up * (0.5f + gravity_timer))) * Time.deltaTime, Space.World);
            // Si no ha enganchado nada por x metros vuelve
            if (Vector3.Distance(transform.position, launch_point) > 15) enemy_hooked = true;
        }
    }
    private void FixedUpdate()
    {
        if (enemy_hooked || !launched) return;

        Collider[] colls = Physics.OverlapSphere(transform.position, detection_range, layer_enemy);
        if (colls.Length > 0)
        {
            enemy_hooked = true;
            target_hooked = colls[0].transform.gameObject;
            enemyFollow = target_hooked.GetComponent<EnemyFollow>();
            enemyFollow.rb.useGravity = false;
            enemyFollow.agent.enabled = false;

            transform.position = target_hooked.transform.position + (target_hooked.transform.right / (target_hooked.transform.localScale.x * 2));
            enemyFollow.transform.SetParent(transform);
        }

        colls = Physics.OverlapSphere(transform.position, 0.15f);
        if (colls.Length > 0)
        {
            foreach (Collider coll in colls)
            {
                if (coll.gameObject.layer != layer_player)
                {
                    enemy_hooked = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Si el jugador ha vuelto a pulsar el input dentro del margen de 0.75 segundos saltará hacia el enemigo
    /// </summary>
    public void LaunchPlayer()
    {
        if (!launched) return;

        if (timer < 0.75f && enemy_hooked)
        {
            launch_player = true;
        }
    }

    void PullHook()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * (speed * 0.25f));
        if (Vector3.Distance(transform.position, player.transform.position - player.transform.right) < detection_range * 2)
        {
            HideHook();
        }
    }

    void HideHook()
    {
        abs.active_hook = false;
        launched = false;
        ob_renderer.enabled = false;

        // El jugador tiene una ventana de tiempo para atacar sin hacerse daño
        playerLife.Invulnerable();

        anim.SetBool("Launch", false);

        if (enemyFollow != null)
        {
            enemyFollow.transform.parent = null;
            enemyFollow.rb.useGravity = true;
            enemyFollow.AddForceToEnemy(Vector3.zero);
        }

        if (added_force_to_player)
        {
            PlayerMovement.instance.canMove = true;
            added_force_to_player = false;
        }

        launch_player = false;
        audioSource.Stop();

        timer = 0;
        can_launch = false;

        AttackSystem.instance.StartCooldowns();
        ab_hook.onExecution = false;
        AttackSystem.instance.ChangeAttack();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }
}