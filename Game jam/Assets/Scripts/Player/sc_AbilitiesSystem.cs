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

    public enum Abilities { LEVITATE, EXPLODE_PATH, GROUP, MINE, HOOK, STOMP, LAST_NO_USE }
    public enum AbilityType { BASIC, ULTIMATE, LAST_NO_USE }

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
    public Sprite sprite_hook;
    public Sprite sprite_stomp;
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
        ab.icon = sprite_levitate;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Exploding path ability
        ab = new Ability();

        ab.name = "Exploding path";
        ab.description = "While you are returning the path you follow will start to explode, damaging and launching your enemies.";
        ab.icon = sprite_explodepath;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Group ability
        ab = new Ability();

        ab.name = "Group n' xplode";
        ab.description = "While you are returning, all the enemies you touch will start to levitate to you, grouping them. The more enemies you group, the faster will go so you can explode them!";
        ab.icon = sprite_group;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Plant mine ability
        ab = new Ability();

        ab.name = "Path mine";
        ab.description = "When you finish your return in time, mines will automatically plant on the path you made. Mines will explode when an enemy enters its range.";
        ab.icon = sprite_mine;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Hook
        ab = new Ability();

        ab.name = "Hook";
        ab.description = "Launch a hook forward and catch the first enemy hit and pulling it towards you. If you press the ability again, you are pulled to the enemy instead.";
        ab.icon = sprite_hook;
        ab.rarity = AbilityType.BASIC;

        abilities.Add(ab);

        // Stomp
        ab = new Ability();

        ab.name = "Stomp";
        ab.description = "[ON GROUND] Jump to immediately stomp the ground with your body and launch the enemies on the air" +
                         "[ON AIR] Quickly descend and smash the enemies on ground, doing extra damage scaling with the distance fell and launching them into the air";
        ab.icon = sprite_stomp;
        ab.rarity = AbilityType.BASIC;

        abilities.Add(ab);

        for (int i = 0; i < (int)Abilities.LAST_NO_USE; i++)
        {
            abilities[i].id = i;
            abilities[i].ability_event = methods_abilities[i];
            abilities[i].type = (Abilities)i;
        }

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetRandomAbilities();
        }
    }

    /// <summary>
    /// Abre el menú de selección y muestra tres habilidades aleatorias
    /// </summary>
    public void GetRandomAbilities()
    {
        // If the player already has all abilities, return
        if (abilities_equipped.Count >= abilities.Count) return;

        const int ability_count = 3;
        Ability[] abilities_to_show = new Ability[ability_count];

        rand_abilities_index.Clear();
        HashSet<int> usedIndexes = new HashSet<int>();

        int max_attempts = abilities.Count * 10; // Prevent infinite loops
        int attempts = 0;
        int foundAbilities = 0;

        while (foundAbilities < ability_count && attempts < max_attempts)
        {
            int rand = Random.Range(0, abilities.Count);
            attempts++;

            // Skip abilities already equipped or already chosen in this selection
            if (usedIndexes.Contains(rand) || abilities_equipped.Exists(a => a.id == abilities[rand].id))
                continue;

            // Assign ability and mark it as used
            abilities_to_show[foundAbilities] = abilities[rand];
            usedIndexes.Add(rand);
            rand_abilities_index.Add(abilities[rand].id);
            foundAbilities++;
        }

        // Assign the selected abilities to UI elements
        for (int i = 0; i < ability_count; i++)
        {
            if (abilities_to_show[i] != null)
            {
                int ab_num = i; // Necessary for lambda closure capture

                slots_buttons[i].onClick.RemoveAllListeners();
                slots_buttons[i].onClick.AddListener(() =>
                {
                    switch (abilities_to_show[ab_num].rarity)
                    {
                        case AbilityType.ULTIMATE: // habilidades activadas a lvolver en el tiempo
                            ReturnScript.instance.ability.AddListener(() => methods_abilities[(int)abilities_to_show[ab_num].type].Invoke());

                            GameObject ab_icon = new GameObject();
                            ab_icon.name = abilities_to_show[ab_num].name;

                            Image im = ab_icon.AddComponent<Image>();
                            im.sprite = abilities_to_show[ab_num].icon;

                            ab_icon.transform.SetParent(ob_abilities_ui_holder.transform);
                            break;


                        case AbilityType.BASIC: // habilidades melee
                            AttackSystem.instance.equipped_attacks.Add(abilities_to_show[ab_num]);
                            AttackSystem.instance.UpdateUI();
                            break;
                    }


                    Debug.Log("Ability selected: " + abilities_to_show[ab_num].name);

                    abilities_equipped.Add(abilities_to_show[ab_num]);

                    CloseGamblingMenu();
                    slots_buttons[ab_num].onClick.RemoveAllListeners();
                });

                slots_images[i].sprite = abilities_to_show[i].icon;
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