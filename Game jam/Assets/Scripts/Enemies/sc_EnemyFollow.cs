using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    public bool can_move = true;

    public float speed;
    public float mass = 1;

    public float attack_distance;
    private bool attacking = false;

    public Rigidbody rb;

    private Collider collider;

    private Vector3 original_position;

    private bool relocating = false;
    private Vector3 directionAwayFromPlayer;

    private float timer = 0;

    private float relocate_timer = 0;

    public AudioSource audioSource;

    private float last_distance = 0;

    PlayerMovement pm;

    void Start()
    {
        pm = PlayerMovement.instance;
        target = pm.transform;

        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();

        original_position = transform.position;

        last_distance = Vector3.Distance(transform.position, target.position);
    }

    public void AddForceToEnemy(Vector3 dir)
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.freezeRotation = false;
        rb.useGravity = true;

        if (agent != null)
            agent.enabled = false;
        collider.isTrigger = false;
        rb.velocity = dir / mass;
    }

    private void FixedUpdate()
    {
        if (relocating || !can_move) return;

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
                    collider.isTrigger = true;

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

            agent.enabled = false;
            rb.isKinematic = false;

            Vector3 dir = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir);

            rb.freezeRotation = true;

            float timer = 0;
            while (timer < attack_distance)
            {
                Debug.DrawRay(transform.position, dir, Color.yellow);
                //transform.Translate(transform.forward * Time.fixedDeltaTime * 2);
                rb.AddForce(dir * Time.fixedDeltaTime * 2, ForceMode.Acceleration);
                timer += Time.deltaTime * 2;
                yield return null;
            }

            timer = 0;
            while (timer < 1.5f)
            {
                rb.AddForce(-dir * Time.fixedDeltaTime * 3, ForceMode.Acceleration);
                //transform.Translate(-transform.forward * Time.fixedDeltaTime * 2);
                timer += Time.deltaTime;
            }

            yield return new WaitForSeconds(3); /// Esperarán 2 segundos antes de volver a atacar

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