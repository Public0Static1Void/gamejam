using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{

    public Transform target;
    private NavMeshAgent agent;

    public float speed;

    private Rigidbody rb;

    private Vector3 original_position;
    void Start()
    {
        target = PlayerMovement.instance.transform;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        original_position = transform.position;
    }

    public void AddForceToEnemy(Vector3 dir, ForceMode mode)
    {
        rb.isKinematic = false;
        agent.enabled = false;
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
        if (!agent.isOnNavMesh && rb.isKinematic)
            Destroy(this.gameObject);
    }
}
