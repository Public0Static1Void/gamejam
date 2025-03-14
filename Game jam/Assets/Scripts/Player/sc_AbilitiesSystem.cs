using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilitiesSystem : MonoBehaviour
{
    public static AbilitiesSystem instance {  get; private set; }

    protected List<Ability> abilities = new List<Ability>();
    private List<Ability> abilities_equipped = new List<Ability>();

    public enum Abilities { LEVITATE, EXPLODE_PATH, GROUP, LAST_NO_USE }

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
    public Sprite sprite_group;
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

        // Levitate ability
        Ability ab = new Ability();

        ab.name = "Levitate";
        ab.description = "When you are returning in time, all the enemies that you touch will start to levitate and crushed on the ground when you finish returning.";
        ab.ability_event = methods_abilities[(int)Abilities.LEVITATE];
        ab.type = Abilities.LEVITATE;
        ab.icon = sprite_levitate;

        // Exploding path ability
        abilities.Add(ab);

        ab = new Ability();

        ab.name = "Exploding path";
        ab.description = "While you are returning the path you follow will start to explode, damaging and launching your enemies.";
        ab.ability_event = methods_abilities[(int)Abilities.EXPLODE_PATH];
        ab.type = Abilities.EXPLODE_PATH;
        ab.icon = sprite_explodepath;

        abilities.Add(ab);

        // Group ability
        ab = new Ability();

        ab.name = "Group n' xplode";
        ab.description = "While you are returning, all the enemies you touch will start to levitate to you, grouping them. The more enemies you group, the faster will go so you can explode them!";
        ab.ability_event = methods_abilities[(int)Abilities.GROUP];
        ab.type = Abilities.GROUP;
        ab.icon = sprite_group;

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
        const int ability_count = 3; /// Número de habilidades a mostrar
        Ability[] abilities_to_show = new Ability[ability_count];

        // Asigna habilidades aleatorias
        int repeated_ability = 0;
        for (int i = 0; i < ability_count; i++)
        {
            int rand = Random.Range(0, abilities.Count);
            /// Buscará una habilidad random y comprueba si el jugador ya las tiene todas
            while (abilities_equipped.Contains(abilities[rand]) && repeated_ability <= abilities.Count)
            {
                rand = Random.Range(0, abilities.Count);
                repeated_ability++;
            }
            /// Si el jugador las tiene todas sale del bucle
            if (repeated_ability >= abilities.Count)
                break;
            abilities_to_show[i] = abilities[rand];

            Debug.Log(abilities_to_show[i].name);
        }

        for (int i = 0; i < ability_count; i++)
        {
            // Añade el evento de añadir la habilidad al botón
            int ab_num = i;

            if (slots_buttons[i] != null)
            {
                slots_buttons[i].onClick.AddListener(() => {

                    ReturnScript.instance.ability.AddListener(() => methods_abilities[(int)abilities_to_show[ab_num].type].Invoke());
                    Debug.Log("Ability selected: " + abilities_to_show[ab_num].name);
                    
                    abilities_equipped.Add(abilities_to_show[ab_num]);

                    for (int i = 0; i < abilities_equipped.Count; i++)
                    {
                        Debug.Log(abilities_equipped[i].name);

                    }

                    CloseGamblingMenu();

                    slots_buttons[ab_num].onClick.RemoveAllListeners();


                });
            }

            // Cambia la imagen del sprite
            if (slots_images[i] != null && abilities_to_show[i] != null)
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