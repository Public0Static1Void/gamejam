using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    public float speed;

    public Rigidbody rb;

    private Collider collider;

    private Vector3 original_position;

    private bool relocating = false;
    private Vector3 directionAwayFromPlayer;

    private float timer = 0;

    float relocate_timer = 0;

    [HideInInspector]
    public AudioSource audioSource;

    void Start()
    {
        target = PlayerMovement.instance.transform;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();

        original_position = transform.position;
    }

    public void AddForceToEnemy(Vector3 dir)
    {
        if (rb == null) return;

        rb.isKinematic = false;
        agent.enabled = false;
        collider.isTrigger = false;
        rb.velocity = dir;
    }

    private void FixedUpdate()
    {
        if (relocating) return;

        if (rb.velocity.magnitude > -0.05f && rb.velocity.magnitude < 0.05f)
        {
            if (Physics.Raycast(transform.position, Vector2.down, transform.localScale.y + 0.1f))
            {
                rb.isKinematic = true;
                agent.enabled = true;
                collider.isTrigger = true;
            }
            else
            {
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
            if (timer > 15 || rb.isKinematic) /// Si pasa cierto tiempo sin estar en la navmesh o es kinematic sin estar en ella se destruye
            {
                Destroy(this.gameObject);
            }
        }
        if (agent.isOnNavMesh && !rb.isKinematic)
            rb.velocity *= 0.75f;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= 3)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(((target.position - Vector3.up / 2) - transform.position).normalized), Time.deltaTime);
        }

        if (relocating && rb.velocity.magnitude < 0.01f && rb.velocity.magnitude > -0.01f)
        {
            Debug.Log(Vector3.Dot(PlayerMovement.instance.transform.right, directionAwayFromPlayer) > 0);
            Vector3 new_dir = (PlayerMovement.instance.transform.right * -Vector3.Dot(PlayerMovement.instance.transform.right, directionAwayFromPlayer)
                               + directionAwayFromPlayer * 5);
            transform.Translate(new_dir.normalized * Time.deltaTime * 12.5f);

            relocate_timer += Time.deltaTime;
            if (relocate_timer > 0.25f)
            {
                relocate_timer = 0;
                relocating = false;
            }
            return;
        }
        else if (PlayerMovement.instance.moving && Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) < 1.5f)
        {
            directionAwayFromPlayer = (-transform.position + PlayerMovement.instance.transform.position).normalized;
            relocating = true;
        }
    }
}