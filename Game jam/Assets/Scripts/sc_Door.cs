using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_Door : MonoBehaviour
{
    public Transform player;
    public float range_interaction;
    public float door_cost;

    private bool text_shown = false;

    private bool player_on_range = false;

    private float timer = 0;
    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < range_interaction)
        {
            player_on_range = true;
            ScoreManager.instance.can_buy_door = true;
            ScoreManager.instance.current_door = this.transform;
            ScoreManager.instance.door_cost = door_cost;
            if (!text_shown)
            {
                Vector3 dir_to_player = (transform.position - player.position).normalized;
                StartCoroutine(ScoreManager.instance.ShowTextOnPosition("Press F to buy", transform.position + dir_to_player, dir_to_player, 120));
                text_shown = true;
            }
        }
        else if (player_on_range)
        {
            ScoreManager.instance.can_buy_door = false;
            player_on_range = false;
        }

        if (text_shown)
        {
            timer += Time.deltaTime;
            if (timer > 120)
            {
                text_shown = false;
                timer = 0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, range_interaction);
    }
}