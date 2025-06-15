using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class sc_shop : MonoBehaviour
{
    public static sc_shop instance {  get; private set; }

    public InputAction buy;

    private bool canBuyJ = false;
    private bool canBuySpeed;
    private bool canBuyDmg = false;

    public int speed_cost;
    public int hp_cost;
    public int dmg_cost;
    public int er_cost;
    public int stamina_cost;

    public GameObject shop_object;

    void Update()
    {
        //if (Vector3.Distance(transform.position, player.position) < range_interaction)
        //{
        //    player_on_range = true;
        //    ScoreManager.instance.can_buy_door = true;
        //    ScoreManager.instance.current_door = this.transform;
        //    ScoreManager.instance.door_cost = door_cost;

        //    // Muestra el texto de compra
        //    string door_text = "Press F to buy";
        //    if (pInput.currentControlScheme == "Gamepad")
        //    {
        //        if (Gamepad.current != null)
        //        {
        //            string controller_name = Gamepad.current.device.displayName;
        //            if (controller_name.ToLower().Contains("dualshock") || controller_name.ToLower().Contains("dualsense"))
        //            {
        //                door_text = "Press X to buy";
        //            }
        //            else if (controller_name.ToLower().Contains("xbox") || controller_name.ToLower().Contains("xinput"))
        //            {
        //                door_text = "Press A to buy";
        //            }
        //        }
        //    }
        //    door_text += $" ({door_cost} cost)";
        //    GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, door_text, 1);
        //    text_shown = true;
        //}
        //else if (player_on_range)
        //{
        //    ScoreManager.instance.can_buy_door = false;
        //    // Esconde el texto de compra
        //    Debug.Log("Hiding text");
        //    GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
        //    text_shown = false;
        //    player_on_range = false;
        //}
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("StaminUp"))
          canBuySpeed = true;
    }

    private void OntriggerExit(Collider collision)
    {
        

        if(collision.CompareTag("StaminUp"))
        {
            canBuySpeed = false;
        }
    }
}
