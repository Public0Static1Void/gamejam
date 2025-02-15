using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class EnemyLife : MonoBehaviour
{
    public int hp;
    public int max_hp;

    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        hp = max_hp;
    }
    public void Damage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            ScoreManager.instance.ChangeScore(amount, transform.position, true);
            Rounds.instance.enemies.Remove(this.gameObject);
            Destroy(gameObject);
        }
        StartCoroutine(ActivateKinematic());
    }

    private IEnumerator ActivateKinematic()
    {
        yield return new WaitForSeconds(1.5f);
        rb.isKinematic = true;
    }
}