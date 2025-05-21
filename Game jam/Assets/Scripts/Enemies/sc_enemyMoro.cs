using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_enemyMoro : MonoBehaviour
{

    float timer = 0;
    public LayerMask player;
    public LayerMask enemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > 5)
        {
            GameManager.gm.SpawnShpereRadius(gameObject.transform.position, 29, Color.yellow, true);
            Collider[] calls = Physics.OverlapSphere(transform.position, 29, enemy);
            if (calls.Length > 0) 
            {
                foreach(Collider c in calls) 
                {
                    Vector3 dir = c.transform.position - transform.position;
                    c.GetComponent<EnemyFollow>().AddForceToEnemy(dir.normalized * 5);
                }
            }
            Destroy(gameObject);
            //Alluahakbar
        }
    }
}
