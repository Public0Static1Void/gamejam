using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public int max_hp;
    public float invunerable_time;
    public int hp;

    private bool damaged;
    private float timer = 0;

    private Material m;
    private Color curr_color;
    void Start()
    {
        m = GetComponent<MeshRenderer>().material;
        curr_color = m.color;
        hp = max_hp;

        damaged = false;
    }

    void Update()
    {
        if (damaged)
        {
            timer += Time.deltaTime;
            if (timer >= invunerable_time)
            {
                damaged = false;
                timer = 0;
            }
        }
    }

    public void Damage(int value)
    {
        hp -= value;

        curr_color.r += value * 0.025f;
        curr_color.g -= value * 0.025f;
        curr_color.b -= value * 0.025f;

        m.color = curr_color;
        if (hp <= 0)
        {
            GameManager.gm.LoadScene("Menu");
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.transform.tag == "Enemy" && !damaged)
        {
            Damage(1);
            damaged = true;
        }
    }
}
