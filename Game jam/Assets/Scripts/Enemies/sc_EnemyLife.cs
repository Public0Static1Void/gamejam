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
    public AudioClip clip_growl;

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

        // El enemigo hace un sonido de dañado
        enemyFollow.audioSource.clip = clip_damaged[Random.Range(0, clip_damaged.Count - 1)];
        enemyFollow.audioSource.loop = false;
        enemyFollow.audioSource.Play();
        if (hp <= 0)
        {
            ScoreManager.instance.AddMultiplier(0.1f);
            ScoreManager.instance.ChangeScore(amount, transform.position, true);
            Rounds.instance.enemies.Remove(this.gameObject);
            Destroy(gameObject);
        }
        StartCoroutine(ActivateKinematic());
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