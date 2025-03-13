using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilitiesSystem : MonoBehaviour
{
    public static AbilitiesSystem instance {  get; private set; }
    protected List<Ability> abilities = new List<Ability>();
    public enum Abilities { LEVITATE, EXPLODE_PATH, LAST_NO_USE }

    [Header("References")]
    public GameObject ob_gamblingparent;

    public GameObject ability1_slot;
    public GameObject ability2_slot;
    public GameObject ability3_slot;

    private List<Image> slots_images;
    public List<Text> slots_texts;
    private List<Button> slots_buttons;

    [Header("Abilities Methods")]
    public List<UnityEvent> methods_abilities;
    public List<UnityAction> actions;

    [Header("Sprites")]
    public Sprite sprite_levitate;
    public Sprite sprite_explodepath;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        CloseGamblingMenu();

        Ability ab = new Ability();

        ab.name = "Levitate";
        ab.description = "When you are returning in time, all the enemies that you touch will start to levitate and crushed on the ground when you finish returning.";
        ab.ability_event = methods_abilities[(int)Abilities.LEVITATE];
        ab.type = Abilities.LEVITATE;
        ab.icon = sprite_levitate;

        abilities.Add(ab);

        ab = new Ability();

        ab.name = "Exploding path";
        ab.description = "While you are returning the path you follow will start to explode, damaging and launching your enemies.";
        ab.ability_event = methods_abilities[(int)Abilities.EXPLODE_PATH];
        ab.type = Abilities.EXPLODE_PATH;
        ab.icon = sprite_explodepath;

        abilities.Add(ab);

        // Referencias de los slots de habilidades

        slots_buttons = new List<Button>();
        slots_images = new List<Image>();

        slots_buttons.Add(ability1_slot.GetComponent<Button>());
        slots_buttons.Add(ability2_slot.GetComponent<Button>());
        slots_buttons.Add(ability3_slot.GetComponent<Button>());

        slots_images.Add(ability1_slot.GetComponent<Image>());
        slots_images.Add(ability2_slot.GetComponent<Image>());
        slots_images.Add(ability3_slot.GetComponent<Image>());
    }

    public void GetRandomAbilities()
    {
        const int ability_count = 2; /// Número de habilidades a mostrar
        Ability[] abilities_to_show = new Ability[ability_count];

        for (int i = 0; i < ability_count; i++)
        {
            int rand = Random.Range(0, abilities.Count);
            if (i > 0)
            {
                while (abilities_to_show[rand] == abilities_to_show[i - 1])
                    rand = Random.Range(0, abilities.Count);
            }
            abilities_to_show[i] = abilities[rand];
            Debug.Log(abilities_to_show[i].name);
        }
        for (int i = 0; i < ability_count; i++)
        {
            // Añade el evento de añadir la habilidad al botón
            int ab_num = i;
            //Debug.Log("Selected: " + abilities_to_show[ab_num].name);

            slots_buttons[i].onClick.AddListener(() => {

                ReturnScript.instance.ability.AddListener(() => methods_abilities[(int)abilities_to_show[ab_num].type].Invoke());
                CloseGamblingMenu();

            });
            // Cambia la imagen del sprite
            slots_images[i].sprite = abilities_to_show[i].icon;
        }

        ob_gamblingparent.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseGamblingMenu()
    {
        ob_gamblingparent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}