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

    public float range_interaction = 3;

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

    string jugg_phrase = "";
    string stam_phrase = "";

    PlayerInput pInput;
    private void Start()
    {
        pInput = PlayerMovement.instance.GetComponent<PlayerInput>();

        jugg_phrase = IdiomManager.instance.GetKeyText("Jugger text");
        stam_phrase = IdiomManager.instance.GetKeyText("Stam text");

        Debug.Log(jugg_phrase);
        Debug.Log(stam_phrase);
    }
    void Update()
    {
        if (perkStam == null) return;

        if (Vector3.Distance(perkStam.transform.position, player.position) < range_interaction)
        {
            player_on_rangee = true;
            canBuyJ = true;

            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, jugg_phrase, 1);
        }
        else if (Vector3.Distance(perkJugg.transform.position, player.position) < range_interaction)
        {
            player_on_rangee = true;
            canBuySpeed = true;

            GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, stam_phrase, 1);
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
