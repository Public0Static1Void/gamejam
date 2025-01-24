using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Rounds : MonoBehaviour
{
    public Transform[] SpawnPoint;
    public GameObject enemy;
    public static Rounds instance { get; private set; }

    public List<GameObject> enemies;

    public float enemyRound;

    private bool spawning = false;
    private int round = 0;

    float enemy_hp = 10;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void Update()
    {
        if (enemies.Count == 0 && !spawning)
        {
            StartCoroutine(SpawnLine());
            
            enemyRound *= 1.2f;
        }
    }

    private IEnumerator SpawnLine()
    {
        spawning = true;
        if (round > 0)
            EkkoUlt.instance.RandomUpgrade();

        yield return new WaitForSeconds(10);

        for (int i = 0; i < enemyRound; i++)
        {
            int randSpawn = Random.Range(0, SpawnPoint.Length);
            GameObject enemy_inst = Instantiate(enemy, SpawnPoint[randSpawn].position, transform.rotation);
            if (round > 0)
                enemy_hp *= 1.2f;
            enemy_inst.GetComponent<EnemyLife>().hp = (int)enemy_hp;
            enemies.Add(enemy_inst);
            yield return new WaitForSeconds(0.5f);
        }

        round++;

        spawning = false;
    }
}
