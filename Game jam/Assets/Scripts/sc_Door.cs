using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_Door : MonoBehaviour
{
    public Transform player;
    public float range_interaction;
    public float door_cost;

    private bool player_on_range = false;

    private bool text_shown = false;

    PlayerInput pInput;
    public List<Spawner> canSpawn;

    private void Start()
    {
        pInput = PlayerMovement.instance.GetComponent<PlayerInput>();
    }
    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < range_interaction)
        {
            player_on_range = true;
            ScoreManager.instance.can_buy_door = true;
            ScoreManager.instance.current_door = this.transform;
            ScoreManager.instance.door_cost = door_cost;

            // Muestra el texto de compra
            string door_text = "Press F to buy";
            if (pInput.currentControlScheme == "Gamepad")
            {
                if (Gamepad.current != null)
                {
                    string controller_name = Gamepad.current.device.displayName;
                    if (controller_name.ToLower().Contains("dualshock") || controller_name.ToLower().Contains("dualsense"))
                    {
                        door_text = "Press X to buy";
                    }
                    else if (controller_name.ToLower().Contains("xbox") || controller_name.ToLower().Contains("xinput"))
                    {
                        door_text = "Press A to buy";
                    }
                }
            }
            door_text += $" ({door_cost} cost)";
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, door_text, 1);
            text_shown = true;
        }
        else if (player_on_range)
        {
            ScoreManager.instance.can_buy_door = false;
            // Esconde el texto de compra
            Debug.Log("Hiding text");
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
            text_shown = false;
            player_on_range = false;
        }
    }

    private void OnDestroy()
    {
        if (text_shown)
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, range_interaction);
    }
}