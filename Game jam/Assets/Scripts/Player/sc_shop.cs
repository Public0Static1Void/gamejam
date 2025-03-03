using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_shop : MonoBehaviour
{
    public int speed_cost;
    public int er_cost;
    public int stamina_cost;


    public void Buy_speed()
    {
        if (ScoreManager.instance.score >= speed_cost)
        {
            ScoreManager.instance.ChangeScore(-speed_cost, transform.position, false);
            PlayerMovement.instance.speed += 2;
        }
    }

    public void Buy_ExplosionRange()
    {
        if(ScoreManager.instance.score >= er_cost) 
        {
            ScoreManager.instance.ChangeScore(-er_cost, transform.position, false);
            ReturnScript.instance.explosion_range += 2;
        }
    }
    
     public void Buy_stamina()
    {
        if(ScoreManager.instance.score >= stamina_cost)
        {
            ScoreManager.instance.ChangeScore(-stamina_cost, transform.position, false);
            PlayerMovement.instance.max_stamina += 2;
        }
    }
}
