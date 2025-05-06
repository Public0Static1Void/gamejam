using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyFollow))]
public class EnemyLife : MonoBehaviour
{
    public int hp;
    public int max_hp;

    private Rigidbody rb;

    private EnemyFollow enemyFollow;
    public List<AudioClip> clip_damaged;
    public AudioClip clip_growl, blood_explosion;
    [Header("Particles")]
    public ParticleSystem particle_explosion;

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
        hp -= amount;

        GameManager.gm.damage_done += amount;

        // El enemigo hace un sonido de dañado
        if (enemyFollow.audioSource != null)
        {
            enemyFollow.audioSource.clip = clip_damaged[Random.Range(0, clip_damaged.Count - 1)];
            enemyFollow.audioSource.loop = false;
            enemyFollow.audioSource.Play();
        }
        if (hp <= 0)
        {
            GameManager.gm.enemies_killed++;

            // Instancia las partículas de muerte
            Instantiate(particle_explosion, transform.position, particle_explosion.transform.rotation);
            /// Genera un sonido de explosión
            SoundManager.instance.InstantiateSound(blood_explosion, transform.position, 0.1f);
            // Añade el multiplicador
            ScoreManager.instance.AddMultiplier(0.1f);
            // Suma la score
            ScoreManager.instance.ChangeScore(amount, transform.position, true);
            // Se quita de la lista de enemigos vivos y se destruye
            Rounds.instance.enemies.Remove(this.gameObject);
            // Añade xp al morirse
            AbilitiesSystem.instance.AddXP(max_hp * 0.05f);

            Vector3 dir = transform.position - PlayerMovement.instance.transform.position;
            ScoreManager.instance.InstantiateText($"XP +{max_hp * 0.05f}", transform.position, dir.normalized, 40, 2, Color.cyan);

            // Suma la velocidad si se puede
            sc_Abilities.instance.KillNSpeed(150);

            Destroy(gameObject);
        }
        StartCoroutine(ActivateKinematic());
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