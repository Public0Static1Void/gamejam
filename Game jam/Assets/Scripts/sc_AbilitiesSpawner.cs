using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_AbilitiesSpawner : MonoBehaviour
{
    public GameObject pr_ui;
    public int buy_cost;
    public float buy_range;
    public List<Transform> spawn_positions;
    private int current_pos = 0, last_pos = 0;

    private bool can_buy = false;

    private string buy_phrase = "ONGAME Buy Ability";
    void Start()
    {
        if (GameManager.gm.GetCurrentControllerName().ToLower() != "keyboard")
        {
            buy_phrase += " gamepad";
            if (GameManager.gm.GetCurrentControllerName().ToLower().Contains("dualshock") || GameManager.gm.GetCurrentControllerName().ToLower().Contains("dualsense"))
            {
                buy_phrase += " PS";
            }
            else
            {
                buy_phrase += " XB";
            }
        }
        else
        {
            buy_phrase += " keyboard";
        }
        buy_phrase = IdiomManager.instance.GetKeyText(buy_phrase);

        buy_phrase = Regex.Replace(buy_phrase, @"\d+", buy_cost.ToString());

        spawn_positions[current_pos].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(spawn_positions[current_pos].position, PlayerMovement.instance.transform.position) <= buy_range)
        {
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, buy_phrase, 15);
            can_buy = true;
        }
        else if (can_buy)
        {
            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
            can_buy = false;
        }
    }

    public void BuyAbility(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            if (can_buy && ScoreManager.instance.score - buy_cost >= 0)
            {
                can_buy = false;
                /// Resta la score
                ScoreManager.instance.ChangeScore(-buy_cost, spawn_positions[current_pos].position, false);

                /// Desbloquea una habilidad random
                Ability ab = AbilitiesSystem.instance.UnlockRandomAbility();

                if (ab == null) return;

                // Muestra la habilidad desbloqueada
                /// Crea el objeto
                Vector3 dir_player = (PlayerMovement.instance.transform.position - spawn_positions[current_pos].position).normalized;
                Quaternion look_rot = Quaternion.LookRotation(-dir_player, Vector3.up);
                GameObject ob = Instantiate(pr_ui, spawn_positions[current_pos].position, look_rot);

                UnityEngine.UI.Image icon = ob.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>();
                icon.sprite = ab.icon;

                TMP_Text ab_name = ob.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
                ab_name.text = ab.name;

                ob.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = ab.rarity.ToString();

                Destroy(ob, 20);

                /// Cambia la posición de la caja a otra aleatoria
                spawn_positions[current_pos].gameObject.SetActive(false);

                current_pos = Random.Range(0, spawn_positions.Count);
                if (spawn_positions.Count > 1)
                {
                    while (current_pos == last_pos)
                    {
                        current_pos = Random.Range(0, spawn_positions.Count);
                    }
                    last_pos = current_pos;
                }
                

                spawn_positions[current_pos].gameObject.SetActive(true);

                /// Quita el texto de compra
                GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "", -15);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        foreach (Transform tr in spawn_positions)
        {
            Gizmos.DrawWireSphere(tr.position, buy_range);
        }
    }
}