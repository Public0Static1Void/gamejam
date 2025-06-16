using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class sc_shop : MonoBehaviour
{
    public static sc_shop instance {  get; private set; }

    public InputAction buy;

    public Transform player;
    public Transform perkStam;
    public Transform perkJugg;

    private float range_interaction = 3;

    private bool player_on_rangee = false;


    public bool canBuyJ = false;
    public bool canBuySpeed;
    private bool canBuyDmg = false;

    public int speed_cost;
    public int hp_cost;
    public int dmg_cost;
    public int er_cost;
    public int stamina_cost;

    public GameObject shop_object;

    PlayerInput pInput;
    private void Start()
    {
        pInput = PlayerMovement.instance.GetComponent<PlayerInput>();
    }
    void Update()
    {
        if (Vector3.Distance(perkStam.transform.position, player.position) < range_interaction)
        {
            player_on_rangee = true;
            canBuyJ = true;
            //ScoreManager.instance.door_cost = door_cost;

            //// Muestra el texto de compra
            //string door_text = "Press F to buy";
            //if (pInput.currentControlScheme == "Gamepad")
            //{
            //    if (Gamepad.current != null)
            //    {
            //        string controller_name = Gamepad.current.device.displayName;
            //        if (controller_name.ToLower().Contains("dualshock") || controller_name.ToLower().Contains("dualsense"))
            //        {
            //            door_text = "Press X to buy";
            //        }
            //        else if (controller_name.ToLower().Contains("xbox") || controller_name.ToLower().Contains("xinput"))
            //        {
            //            door_text = "Press A to buy";
            //        }
            //    }
            //}
            //door_text += $" ({door_cost} cost)";
            //GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, door_text, 1);
            //text_shown = true;
        }
        else if(Vector3.Distance(perkJugg.transform.position, player.position) < range_interaction)
        {
            player_on_rangee = true;
            canBuySpeed = true;
        }
        else if (player_on_rangee)
        {

            canBuySpeed = false;
            canBuyJ = false;
            // Esconde el texto de compra
            Debug.Log("Hiding text");
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
            //text_shown = false;
            player_on_rangee = false;
        }
    }
}
