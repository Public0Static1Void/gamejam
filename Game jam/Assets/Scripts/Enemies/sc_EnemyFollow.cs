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

    private float timer = 0;

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
            if (timer > 10 || rb.isKinematic) /// Si pasa cierto tiempo sin estar en la navmesh o es kinematic sin estar en ella se destruye
            {
                Destroy(this.gameObject);
            }
        }
        if (agent.isOnNavMesh && !rb.isKinematic)
            rb.velocity *= 0.5f;
    }
}