using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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
    float enemy_speed = 3.5f;

    private float timer = 0;

    private bool onRound = false;

    private List<GameObject> enemy_pool;
    private int current_enemy = 0;

    [Header("Sounds")]
    public AudioClip clip_roundstart;
    [Header("References")]
    public TMP_Text txt_round;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void Update()
    {
        if (enemies.Count == 0 && !spawning)
        {
            StartRound();
        }

        timer += Time.deltaTime;

        if (onRound && !spawning)
        {
            if (timer > Mathf.Clamp(30 - round, 10, 30))
            {
                StartRound();
                timer = 0;
            }
        }
        
        if (timer >= 60)
        {
            StartRound();
            timer = 0;
        }
    }

    private void StartRound()
    {
        onRound = true;
        enemyRound *= 1.5f;
        enemy_speed *= 1.05f;
        StartCoroutine(SpawnLine());
    }

    private IEnumerator SpawnLine()
    {
        spawning = true;

        AbilitiesSystem.instance.GetRandomAbilities();

        float wait_time = 12.5f - round * 0.25f; /// Función de espera entre rondas (cuánto más tiempo jugado más rápido pasarán)
        if (wait_time < 3) wait_time = 3;

        yield return new WaitForSeconds(wait_time);

        // Muestra en texto por que ronda vas y suena un sonido para indicar la nueva ronda
        txt_round.text = (round + 1).ToString();
        GameManager.gm.ShowText(string.Format("Round {0}", round + 1));


        // Hace sonar el sonido de inicio de ronda
        SoundManager.instance.InstantiateSound(clip_roundstart, ReturnScript.instance.transform.position);


        for (int i = 0; i < enemyRound; i++)
        {
            int randSpawn = Random.Range(0, SpawnPoint.Length); /// No aparecerán enemigos en la cara del player
            while (Vector3.Distance(PlayerMovement.instance.transform.position, SpawnPoint[randSpawn].position) < 10)
            {
                randSpawn = Random.Range(0, SpawnPoint.Length);
            }

            int rand_enemy = Random.Range(0, enemy_list.Count);
            if (round > 5) /// Hasta la ronda 5 no podrá aparecer el boss
            {
                enemy = enemy_list[rand_enemy];
            }
            else
            {
                enemy = enemy_list[0];
            }
            GameObject enemy_inst = Instantiate(enemy, SpawnPoint[randSpawn].position, transform.rotation);
            enemy_inst.name += " " + round.ToString() + " " + i.ToString();

            EnemyLife enemy_life = enemy_inst.transform.GetChild(0).GetComponent<EnemyLife>();
            if (round > 0 && enemy_life != null)
            {
                enemy_hp = enemy_life.max_hp * 1.25f;
            }

            enemy_inst.transform.GetChild(0).GetComponent<NavMeshAgent>().speed = enemy_speed;

            enemy_life.max_hp = (int)enemy_hp;

            enemies.Add(enemy_inst);
            yield return new WaitForSeconds(0.5f);
        }

        round++;

        spawning = false;
    }
}
