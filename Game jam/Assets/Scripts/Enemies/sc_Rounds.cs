using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Rounds : MonoBehaviour
{
    public Transform[] SpawnPoint;
    public GameObject enemy;
    public static Rounds instance { get; private set; }

    public List<GameObject> enemies, enemy_list;

    public float enemyRound;

    private bool spawning = false;
    private int round = 0;

    float enemy_hp = 10;

    private float timer = 0;

    private bool onRound = false;

    private List<GameObject> enemy_pool;
    private int current_enemy = 0;

    [Header("Sounds")]
    public AudioClip clip_roundstart;
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

        AbilitiesSystem.instance.GetRandomAbilities();

        float wait_time = 12.5f - round * 0.1f; /// Función de espera entre rondas (cuánto más tiempo jugado más rápido pasarán)
        if (wait_time < 2) wait_time = 2;
        yield return new WaitForSeconds(wait_time);

        // Muestra en texto por que ronda vas y suena un sonido para indicar la nueva ronda
        GameManager.gm.ShowText(string.Format("Round {0}", round + 1));
        SoundManager.instance.InstantiateSound(clip_roundstart, ReturnScript.instance.transform.position);

        for (int i = 0; i < enemyRound; i++)
        {
            int randSpawn = Random.Range(0, SpawnPoint.Length); /// No aparecerán enemigos en la cara del player
            while (Vector3.Distance(PlayerMovement.instance.transform.position, SpawnPoint[randSpawn].position) < 10)
            {
                randSpawn = Random.Range(0, SpawnPoint.Length);
            }
            int rand_enemy = Random.Range(0, round);
            if (round > 10) /// Hasta la ronda 10 no podrá aparecer el boss
            {
                enemy = enemy_list[rand_enemy];
            }
            else
            {
                enemy = enemy_list[0];
            }
            GameObject enemy_inst = Instantiate(enemy, SpawnPoint[randSpawn].position, transform.rotation);
            if (round > 0)
                enemy_hp *= 1.5f;
            enemy_inst.GetComponent<EnemyLife>().hp = (int)enemy_hp;
            enemies.Add(enemy_inst);
            yield return new WaitForSeconds(0.5f);
        }

        round++;

        spawning = false;
    }
}
