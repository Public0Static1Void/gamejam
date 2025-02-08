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

    private float timer = 0;

    private bool onRound = false;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        GameManager.gm.ShowText("Press space to return :D", 1);
    }

    void Update()
    {
        if (enemies.Count == 0 && !spawning)
        {
            StartCoroutine(SpawnLine());
            onRound = true;
            enemyRound *= 1.5f;
        }

        if (onRound && !spawning)
        {
            timer += Time.deltaTime;
            if (timer > 30)
            {
                StartCoroutine(SpawnLine());
                timer = 0;
            }
        }
    }

    private IEnumerator SpawnLine()
    {
        spawning = true;
        if (round > 0)
            ReturnScript.instance.RandomUpgrade();

        float wait_time = 10 - round * 0.1f; /// Función de espera entre rondas
        if (wait_time < 1) wait_time = 1;
        yield return new WaitForSeconds(wait_time);

        for (int i = 0; i < enemyRound; i++)
        {
            int randSpawn = Random.Range(0, SpawnPoint.Length); /// No aparecerán enemigos en la cara del player
            while (Vector3.Distance(PlayerMovement.instance.transform.position, SpawnPoint[randSpawn].position) < 10)
            {
                randSpawn = Random.Range(0, SpawnPoint.Length);
            }
            GameObject enemy_inst = Instantiate(enemy, SpawnPoint[randSpawn].position, transform.rotation);
            if (round > 0)
                enemy_hp *= 1.5f;
            enemy_inst.GetComponent<EnemyLife>().hp = (int)enemy_hp;
            enemies.Add(enemy_inst);
            yield return new WaitForSeconds(0.5f);
        }

        round++;

        GameManager.gm.ShowText("Starting round " + round + "!", 4);

        spawning = false;
    }
}
