using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilitiesSystem : MonoBehaviour
{
    public static AbilitiesSystem instance {  get; private set; }

    protected List<Ability> abilities = new List<Ability>();
    private List<Ability> abilities_equipped = new List<Ability>();
    List<int> rand_abilities_index = new List<int>();

    public enum Abilities { LEVITATE, EXPLODE_PATH, GROUP, MINE, LAST_NO_USE }

    [Header("References")]
    public GameObject ob_gamblingparent;
    public GameObject ob_abilities_ui_holder;

    public GameObject ability1_slot;
    public GameObject ability2_slot;
    public GameObject ability3_slot;

    private List<Image> slots_images;
    public List<TMP_Text> slots_texts;
    private List<Button> slots_buttons;

    [Header("Abilities Methods")]
    public List<UnityEvent> methods_abilities;
    public List<UnityAction> actions;

    [Header("Sprites")]
    public Sprite sprite_levitate;
    public Sprite sprite_explodepath;
    public Sprite sprite_group;
    public Sprite sprite_mine;
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

        ab.id = (int)Abilities.LEVITATE;
        ab.name = "Levitate";
        ab.description = "When you are returning in time, all the enemies that you touch will start to levitate and crushed on the ground when you finish returning.";
        ab.ability_event = methods_abilities[(int)Abilities.LEVITATE];
        ab.type = Abilities.LEVITATE;
        ab.icon = sprite_levitate;

        abilities.Add(ab);

        // Exploding path ability
        ab = new Ability();

        ab.id = (int)Abilities.EXPLODE_PATH;
        ab.name = "Exploding path";
        ab.description = "While you are returning the path you follow will start to explode, damaging and launching your enemies.";
        ab.ability_event = methods_abilities[(int)Abilities.EXPLODE_PATH];
        ab.type = Abilities.EXPLODE_PATH;
        ab.icon = sprite_explodepath;

        abilities.Add(ab);

        // Group ability
        ab = new Ability();

        ab.id = (int)Abilities.GROUP;
        ab.name = "Group n' xplode";
        ab.description = "While you are returning, all the enemies you touch will start to levitate to you, grouping them. The more enemies you group, the faster will go so you can explode them!";
        ab.ability_event = methods_abilities[(int)Abilities.GROUP];
        ab.type = Abilities.GROUP;
        ab.icon = sprite_group;

        abilities.Add(ab);

        // Plant mine ability
        ab = new Ability();

        ab.id = (int)Abilities.MINE;
        ab.name = "Path mine";
        ab.description = "When you finish your return in time, mines will automatically plant on the path you made. Mines will explode when an enemy enters its range.";
        ab.ability_event = methods_abilities[(int)Abilities.MINE];
        ab.type = Abilities.MINE;
        ab.icon = sprite_mine;

        abilities.Add(ab);

        if (abilities.Count != (int)Abilities.LAST_NO_USE)
            Debug.LogWarning("Las habilidades del enum no coinciden con las añadidas en el void Start");


        // Referencias de los slots de habilidades

        slots_buttons = new List<Button>();
        slots_images = new List<Image>();
        slots_texts = new List<TMP_Text>();

        slots_buttons.Add(ability1_slot.GetComponent<Button>());
        slots_buttons.Add(ability2_slot.GetComponent<Button>());
        slots_buttons.Add(ability3_slot.GetComponent<Button>());

        slots_images.Add(ability1_slot.transform.GetChild(0).GetComponent<Image>());
        slots_images.Add(ability2_slot.transform.GetChild(0).GetComponent<Image>());
        slots_images.Add(ability3_slot.transform.GetChild(0).GetComponent<Image>());
        
        slots_texts.Add(ability1_slot.transform.GetChild(1).GetComponent<TMP_Text>());
        slots_texts.Add(ability2_slot.transform.GetChild(1).GetComponent<TMP_Text>());
        slots_texts.Add(ability3_slot.transform.GetChild(1).GetComponent<TMP_Text>());
    }

    /// <summary>
    /// Abre el menú de selección y muestra tres habilidades aleatorias
    /// </summary>
    public void GetRandomAbilities()
    {
        // Si el jugador ya tiene todas las habilidades no saldrán más
        if (abilities_equipped.Count >= abilities.Count) return;

        const int ability_count = 3;
        Ability[] abilities_to_show = new Ability[ability_count];

        rand_abilities_index.Clear();
        List<int> usedIndexes = new List<int>();

        int repeated_ability = 0;
        Debug.Log("Abs: " + abilities.Count);


        if (abilities_equipped.Count < abilities.Count - 2)
        {
            for (int i = 0; i < ability_count; i++)
            {
                int rand = Random.Range(0, abilities.Count);


                // Evita habilidades repetidas (ya equipadas o en la misma selección)
                while ((usedIndexes.Contains(rand) || abilities_equipped.Exists(a => a.id == abilities[rand].id)) &&
                       repeated_ability <= abilities.Count * 20)
                {
                    rand = Random.Range(0, abilities.Count);
                    repeated_ability++;
                }

                if (repeated_ability >= abilities.Count * 20) break;
                if (abilities[rand] == null) continue;

                // Comprueba que la habilidad random no esté ya equipada
                bool has_repeated = false;
                for (int j = 0; j < abilities_equipped.Count; j++)
                {
                    Debug.Log("Equipped: " + abilities_equipped[j].name + ", Random: " + abilities[rand].name);

                    if (abilities[rand].name == abilities_equipped[j].name)
                    {
                        Debug.Log("Repeated");
                        has_repeated = true;
                        break;
                    }
                    Debug.Log("Not repeated");
                }
                if (has_repeated) continue;

                abilities_to_show[i] = abilities[rand];
                usedIndexes.Add(rand);
                rand_abilities_index.Add(abilities[rand].id);
            }
        }
            

        // Asigna los métodos a los botones
        for (int i = 0; i < ability_count; i++)
        {

            if (slots_buttons[i] != null)
            {
                int ab_num = i;

                slots_buttons[i].onClick.RemoveAllListeners();
                slots_buttons[i].onClick.AddListener(() => {
                    if (abilities_to_show[ab_num] == null || abilities_to_show[ab_num].name == "") return;

                    ReturnScript.instance.ability.AddListener(() => methods_abilities[(int)abilities_to_show[ab_num].type].Invoke());
                    Debug.Log("Ability selected: " + abilities_to_show[ab_num].name);

                    abilities_equipped.Add(abilities_to_show[ab_num]);

                    GameObject ab_icon = new GameObject();
                    ab_icon.name = abilities_to_show[ab_num].name;

                    Image im = ab_icon.AddComponent<Image>();
                    im.sprite = abilities_to_show[ab_num].icon;

                    ab_icon.transform.SetParent(ob_abilities_ui_holder.transform);

                    CloseGamblingMenu();
                    slots_buttons[ab_num].onClick.RemoveAllListeners();
                });
            }

            if (abilities_to_show[i] != null)
            {
                if (slots_images[i] != null)
                    slots_images[i].sprite = abilities_to_show[i].icon;

                if (slots_texts[i] != null)
                    slots_texts[i].text = abilities_to_show[i].name;
            }
            
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