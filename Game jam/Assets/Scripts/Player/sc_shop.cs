using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_shop : MonoBehaviour
{
    public static sc_shop instance {  get; private set; }

    public InputAction buy;

    private bool canBuyJ = false;
    private bool canBuySpeed = false;
    private bool canBuyDmg = false;

    public int speed_cost;
    public int hp_cost;
    public int dmg_cost;
    public int er_cost;
    public int stamina_cost;

    public GameObject shop_object;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
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
        
    }

    public void Buy(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(canBuyJ)
            {
                if (ScoreManager.instance.score >= hp_cost)
                {
                    ScoreManager.instance.ChangeScore(-hp_cost, transform.position, false);
                    GetComponent<PlayerLife>().max_hp += 2;
                }
            }

            if(canBuySpeed)
            {
                if (ScoreManager.instance.score >= speed_cost)
                {
                    ScoreManager.instance.ChangeScore(-speed_cost, transform.position, false);
                    PlayerMovement.instance.speed += 2;
                }
            }

            if(canBuyDmg)
            {
                if (ScoreManager.instance.score >= dmg_cost)
                {
                    ScoreManager.instance.ChangeScore(-dmg_cost, transform.position, false);
                    //No se en que script esta
                }
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Jugger"))
        {
            canBuyJ = true;
        }

        if (collision.collider.CompareTag("Stamina"))
        {
            canBuySpeed = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Jagger"))
        {
            canBuyJ = false;
        }

        if(collision.collider.CompareTag("StaminUp"))
        {
            canBuySpeed = false;
        }
    }
}
