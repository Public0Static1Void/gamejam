using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class EnemyFollow : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public LayerMask layer_player;
    public NavMeshAgent agent;

    public bool can_move = true;

    public float speed;
    public float mass = 1;

    public float attack_distance;
    private bool attacking = false;

    public Rigidbody rb;

    private Vector3 original_position;

    private bool relocating = false;
    private Vector3 directionAwayFromPlayer;

    private float timer = 0, enabled_timer = 0;

    private float relocate_timer = 0;

    public AudioSource audioSource;

    private float last_distance = 0;

    PlayerMovement pm;


    private Animator anim;
    void Start()
    {
        pm = PlayerMovement.instance;
        target = pm.transform;

        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        audioSource = GetComponent<AudioSource>();

        anim = GetComponent<Animator>();

        original_position = transform.position;

        last_distance = Vector3.Distance(transform.position, target.position);
    }

    public void AddForceToEnemy(Vector3 dir)
    {
        if (rb == null) return;

        anim.SetBool("_Attack", false);

        rb.isKinematic = false;
        rb.freezeRotation = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        if (agent != null)
            agent.enabled = false;

        rb.velocity = dir / mass;
    }

    private void FixedUpdate()
    {
        Debug.Log("Attacking: " + attacking);
        if (relocating || !can_move || attacking) return;

        if (rb.IsSleeping())
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (hit.distance < transform.localScale.y + 1 && !rb.isKinematic)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    if (agent != null)
                        agent.enabled = true;

                    timer = 0;
                }
            }
            else
            {
                Debug.DrawRay(transform.position, Vector3.down, Color.red);
                rb.isKinematic = false;
            }
        }

        if (target != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.transform.position);
            /*Vector3 dir = target.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = rot;

            if (Vector3.Distance(transform.position, target.position) > EkkoUlt.instance.transform.localScale.x / 2)
            {
                rb.velocity = new Vector3(transform.forward.x * speed * Time.deltaTime, rb.velocity.y, transform.forward.z * speed * Time.deltaTime);
                //transform.Translate(transform.forward * 10 * Time.deltaTime);
            }*/
        }
        if (!agent.isOnNavMesh)
        {
            timer += Time.deltaTime;
            if (timer > 20)
            {
                Destroy(this.gameObject);
            }
        }
        else if (!agent.enabled && !rb.isKinematic)
        {
            enabled_timer += Time.deltaTime;
            if (enabled_timer > 5)
            {
                agent.enabled = true;
                enabled_timer = 0;
            }
        }

        if (agent.isOnNavMesh && !rb.isKinematic)
            rb.velocity *= 0.7f;
    }

    private void Update()
    {
        if (!can_move) return;


        // Aparta al enemigo del camino del jugador
        Vector3 p_dir = target.position - transform.position;
        Vector3 p_vel = pm.rb.velocity.normalized;

        float dot = Vector3.Dot(p_vel, -p_dir.normalized);
        if (dot < -0.3f || pm.slide)
        {
            // El jugador se está alejando del enemigo
            relocating = false;
            return;
        }

        // Comprueba si se tiene que alejar del camino del jugador
        if (!attacking && !relocating && pm.moving && Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            // Calcula la dirección para alejarse
            directionAwayFromPlayer = (transform.position - target.position).normalized;

            directionAwayFromPlayer += target.transform.right * Vector3.Dot(-p_dir.normalized, target.right) * 1.5f;

            if (agent.enabled)
                agent.enabled = false;

            relocating = true;
            relocate_timer = 0;
        }

        // Aleja al enemigo del camino del jugador
        if (relocating)
        {
            transform.Translate(directionAwayFromPlayer * Time.deltaTime * 4, Space.World);

            relocate_timer += Time.deltaTime;
            if (relocate_timer >= 0.25f)
            {
                relocating = false;
                relocate_timer = 0f;

                if (!agent.enabled)
                    agent.enabled = true;

                last_distance = Vector3.Distance(transform.position, target.position);
            }
        }

        if (!relocating && !attacking && Vector3.Distance(transform.position, target.position) < attack_distance)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (agent.enabled)
        {
            Debug.Log("Attacking");
            attacking = true;
            anim.SetBool("_Attack", true);

            agent.enabled = false;
            rb.isKinematic = false;
            rb.constraints |= RigidbodyConstraints.FreezePositionY;

            Vector3 dir = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir);

            rb.freezeRotation = true;

            float timer = 0;

            // Va hacia adelante
            rb.AddForce(dir * 6, ForceMode.VelocityChange);
            while (timer < attack_distance)
            {
                Debug.DrawRay(transform.position, dir, Color.yellow);
                Collider[] colls = Physics.OverlapSphere(transform.position, transform.localScale.y + 0.1f, layer_player);
                if (colls.Length > 0)
                {
                    break;
                }
                //transform.Translate(transform.forward * Time.fixedDeltaTime * 2);
                timer += Time.deltaTime * 3;
                yield return null;
            }

            rb.constraints = RigidbodyConstraints.FreezeRotation;

            timer = 0;
            // Vuelve para atrás
            rb.AddForce(-dir * 2, ForceMode.VelocityChange);
            while (timer < 1.5f)
            {
                //transform.Translate(-transform.forward * Time.fixedDeltaTime * 2);
                timer += Time.deltaTime;
            }

            rb.constraints = RigidbodyConstraints.None;

            anim.SetBool("_Attack", false);

            yield return new WaitForSeconds(2.5f); /// Esperarán 2.5f segundos antes de volver a atacar

            agent.enabled = true;
            rb.isKinematic = false;

            attacking = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, attack_distance);
    }
}