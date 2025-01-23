using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rounds : MonoBehaviour
{
    public Transform[] SpawnPoint;
    public GameObject enemy;
    public static Rounds instance { get; private set; }

    public List<GameObject> enemies;

    public float enemyRound = 10;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {

        if (enemies.Count == 0)
        {
            for (int i = 0;i < enemyRound; i++)
            {
                int randSpawn = Random.Range(0, SpawnPoint.Length);

                enemies.Add(Instantiate(enemy, SpawnPoint[randSpawn].position, transform.rotation));
            }
            
            enemyRound *= 1.2f;
        }
    }
}
