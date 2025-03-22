using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class sc_Door : MonoBehaviour
{
    public Transform player;
    public float range_interaction;
    public float door_cost;

    private bool player_on_range = false;

    private bool text_shown = false;

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < range_interaction)
        {
            player_on_range = true;
            ScoreManager.instance.can_buy_door = true;
            ScoreManager.instance.current_door = this.transform;
            ScoreManager.instance.door_cost = door_cost;
            // Muestra el texto de compra
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "Press F to buy", 1);
            text_shown = true;
        }
        else if (player_on_range)
        {
            ScoreManager.instance.can_buy_door = false;
            // Esconde el texto de compra
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "Press F to buy", -15);
            text_shown = false;
            player_on_range = false;
        }
    }

    private void OnDestroy()
    {
        if (text_shown)
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "Press F to buy", -15);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, range_interaction);
    }
}