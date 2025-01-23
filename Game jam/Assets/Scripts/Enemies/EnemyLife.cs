using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class EnemyLife : MonoBehaviour
{
    private int hp;
    public int max_hp;
    private void Start()
    {
        hp = max_hp;
    }
    public void Damage(int amount)
    {
        hp -= amount;
        if (amount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
