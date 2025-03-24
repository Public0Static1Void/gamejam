using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float speed;
    public float detection_range;

    public LayerMask layer_enemy;

    private GameObject player;
    public GameObject target_hooked;

    public bool enemy_hooked = false;

    private sc_Abilities abs;
    void Start()
    {
        player = ReturnScript.instance.gameObject;
        abs = ReturnScript.instance.GetComponent<sc_Abilities>();

        transform.parent = null;
    }

    void Update()
    {
        if (enemy_hooked)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * speed);
            if (Vector3.Distance(transform.position, player.transform.position) < detection_range)
            {
                abs.active_hook = false;
                Destroy(this.gameObject);
            }
        }
        else
            transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }
    private void FixedUpdate()
    {
        if (enemy_hooked) return;

        Collider[] colls = Physics.OverlapSphere(transform.position, detection_range, layer_enemy);
        if (colls.Length > 0)
        {
            enemy_hooked = true;
            target_hooked = colls[0].transform.gameObject;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }
}