using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyFollow))]
public class EnemyLife : MonoBehaviour
{
    public bool invulnerable = false;

    public int hp;
    public int max_hp;

    private Rigidbody rb;

    private EnemyFollow enemyFollow;
    public List<AudioClip> clip_damaged;
    public AudioClip clip_growl, blood_explosion;

    [Header("Particles")]
    public ParticleSystem particle_explosion;
    public ParticleSystem particle_hit;

    float random_pitch = 0;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyFollow = GetComponent<EnemyFollow>();

        hp = max_hp;

        random_pitch = Random.Range(0.5f, 1.5f);
        enemyFollow.audioSource.pitch = random_pitch;
        enemyFollow.audioSource.clip = clip_growl;
        enemyFollow.audioSource.loop = true;
        enemyFollow.audioSource.Play();
    }
    public void Damage(int amount)
    {
        Instantiate(particle_hit, transform.position, Quaternion.identity);
        // El enemigo hace un sonido de dañado
        if (enemyFollow.audioSource != null)
        {
            enemyFollow.audioSource.clip = clip_damaged[Random.Range(0, clip_damaged.Count - 1)];
            enemyFollow.audioSource.loop = false;
            enemyFollow.audioSource.Play();
        }

        if (invulnerable) return;

        hp -= amount;

        GameManager.gm.damage_done += amount;

        
        if (hp <= 0)
        {
            GameManager.gm.enemies_killed++;


            // Añade el multiplicador
            ScoreManager.instance.AddMultiplier(0.1f);
            // Suma la score
            ScoreManager.instance.ChangeScore(amount, transform.position, true);
            // Se quita de la lista de enemigos vivos y se destruye
            Rounds.instance.enemies.Remove(this.gameObject);
            // Añade xp al morirse
            if (TutorialManager.instance == null)
                AbilitiesSystem.instance.AddXP(max_hp * 0.05f);

            Vector3 dir = transform.position - PlayerMovement.instance.transform.position;
            ScoreManager.instance.InstantiateText($"XP +{max_hp * 0.05f}", transform.position, dir.normalized, 40, 2, Color.cyan);

            // Suma la velocidad si se puede
            sc_Abilities.instance.KillNSpeed(100);
            // Suma estamina si puede
            sc_Abilities.instance.Recharge();

            StartCoroutine(DestroyCooldown());
        }
        StartCoroutine(ActivateKinematic());
    }

    private IEnumerator DestroyCooldown()
    {
        SkinnedMeshRenderer mesh_r = GetComponentInChildren<SkinnedMeshRenderer>();

        Color[] col = new Color[8];
        for (int i = 0; i < col.Length; i++)
        {
            if (i % 2 != 0)
            {
                col[i] = Color.yellow;
            }
            else
            {
                col[i] = Color.red;
            }
        }

        int curr_color = 0;
        float timer = 0;
        float global_timer = 1;
        while (curr_color < col.Length)
        {
            mesh_r.material.color = Color.Lerp(mesh_r.material.color, col[curr_color], Time.deltaTime * 3);

            transform.Rotate(Random.Range(0, 50) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime);

            global_timer += Time.deltaTime * 5;
            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                curr_color++;
                timer = 0;
            }
            yield return null;
        }

        // Instancia las partículas de muerte
        Instantiate(particle_explosion, transform.position, particle_explosion.transform.rotation);
        // Genera un sonido de explosión
        SoundManager.instance.InstantiateSound(blood_explosion, transform.position, 0.2f);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);

        Rounds.instance.enemies.Remove(gameObject);
        Rounds.instance.enemies_follow.Remove(enemyFollow);
    }

    private IEnumerator ActivateKinematic()
    {
        yield return new WaitForSeconds(1.5f);
        enemyFollow.audioSource.clip = clip_growl;
        enemyFollow.audioSource.loop = true;
        enemyFollow.audioSource.Play();
        rb.isKinematic = true;
    }
}