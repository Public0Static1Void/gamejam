using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Rounds : MonoBehaviour
{
    public List<Transform> SpawnPoint;
    public GameObject enemy;
    public static Rounds instance { get; private set; }

    public List<GameObject> enemies, enemy_list;
    [HideInInspector]
    public List<EnemyFollow> enemies_follow;

    public float enemyRound;

    private bool spawning = false;
    private int round = 0;

    float enemy_hp = 10;
    float enemy_speed = 1;

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

        enemies_follow = new List<EnemyFollow>();

        if (TutorialManager.instance != null) Destroy(gameObject);
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
        AbilitiesSystem.instance.UnlockAbility();
        StartCoroutine(SpawnLine());
    }

    private IEnumerator SpawnLine()
    {
        spawning = true;

        //AbilitiesSystem.instance.GetRandomAbilities(); Ahora se consiguen habilidades llenando la barra de experiencia

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
            int randSpawn = Random.Range(0, SpawnPoint.Count); /// No aparecerán enemigos en la cara del player
            while (Vector3.Distance(PlayerMovement.instance.transform.position, SpawnPoint[randSpawn].position) < 10)
            {
                randSpawn = Random.Range(0, SpawnPoint.Count);
            }

            GameObject enemy_inst = Instantiate(GetRandomEnemy(), SpawnPoint[randSpawn].position, transform.rotation);
            enemy_inst.name += " " + round.ToString() + " " + i.ToString();
            /// Suma vida a los enemigos tras cada ronda
            EnemyLife enemy_life = enemy_inst.transform.GetChild(0).GetComponent<EnemyLife>();
            if (round > 0 && enemy_life != null)
            {
                enemy_hp = enemy_life.max_hp * 1.25f;
            }

            enemy_inst.transform.GetChild(0).GetComponent<NavMeshAgent>().speed *= enemy_speed;

            enemy_life.max_hp = (int)enemy_hp;

            enemies.Add(enemy_inst);
            enemies_follow.Add(enemy_inst.GetComponentInChildren<EnemyFollow>());
            yield return new WaitForSeconds(0.5f);
        }

        round++;

        spawning = false;
    }

    private GameObject GetRandomEnemy()
    {
        GameObject result;
        int[] frange = new int[enemy_list.Count];
        int total_probs = 0;
        for (int i = 0; i < enemy_list.Count; i++)
        {
            EnemyLife ef = enemy_list[i].transform.GetComponentInChildren<EnemyLife>();
            frange[i] = total_probs + ef.probability;
            total_probs += ef.probability;
        }
        int rand_enemy = Random.Range(0, total_probs);

        for (int i = 0, min = 0; i < frange.Length && i < enemy_list.Count; i++)
        {
            if (rand_enemy > min && rand_enemy < frange[i])
            {
                rand_enemy = i;
                break;
            }
            if (min + frange[i] < frange[frange.Length - 1])
                min += frange[i];
        }
        Debug.Log("Enemy selected: " + rand_enemy);

        if (round >= 3 && rand_enemy < enemy_list.Count) /// Hasta la ronda 3 no podrán aparecer enemigos especiales
        {
            result = enemy_list[rand_enemy];
        }
        else
        {
            result = enemy_list[0];
        }

        return result;
    }
}
