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
    void Start()
    {
        target = EkkoUlt.instance.transform;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = rot;

            if (Vector3.Distance(transform.position, target.position) > 0.5f)
            {
                rb.velocity = new Vector3(transform.forward.x * speed * Time.deltaTime, rb.velocity.y, transform.forward.z * speed * Time.deltaTime);
                //transform.Translate(transform.forward * 10 * Time.deltaTime);
            }
        }
    }
}
