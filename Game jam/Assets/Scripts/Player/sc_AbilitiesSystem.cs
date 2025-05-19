using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilitiesSystem : MonoBehaviour
{
    public static AbilitiesSystem instance {  get; private set; }

    public List<Ability> abilities = new List<Ability>();
    public List<Ability> abilities_log;
    public List<Ability> abilities_equipped = new List<Ability>();
    List<int> rand_abilities_index = new List<int>();

    public bool gambling_open = false;

    public enum Abilities { LEVITATE, EXPLODE_PATH, GROUP, MINE, HOOK, STOMP, BYEBYE, BLOODTHIRSTY, HOLOGRAM_BODY, MONKEY_BAIT, KILLNSPEED, RECHARGE, LAST_NO_USE }
    public enum AbilityType { BASIC, ULTIMATE, PASSIVE, LAST_NO_USE }

    [Header("XP")]
    public float max_xp = 10;
    [HideInInspector]
    public float current_xp = 0;
    public UnityEngine.UI.Image im_xp_bar;
    public TMP_Text txt_xp;

    [Header("References")]
    public GameObject ob_gamblingparent;
    public GameObject ob_abilities_ui_holder;

    public GameObject ability1_slot;
    public GameObject ability2_slot;
    public GameObject ability3_slot;

    private List<Image> slots_images;
    public List<TMP_Text> slots_texts;
    private List<Button> slots_buttons;

    public Image im_description;
    public TMP_Text txt_description;

    public Transform attack_ab_chooser;
    public List<Button> attack_abilities_btn;
    private List<Image> attack_abilities_im;

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
    public Sprite sprite_byebye;
    public Sprite sprite_bloodthirsty;
    public Sprite sprite_hologram;
    public Sprite sprite_monkey;
    public Sprite sprite_killnspeed;
    public Sprite sprite_recharge;


    private Animator gambling_anim;

    private float timer = 0;

    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        LoadAbilitiesData();
        abilities_log = new List<Ability>(abilities); /// Copia la lista
    }

    void Start()
    {
        if (ob_gamblingparent != null)
        {
            gambling_anim = ob_gamblingparent.GetComponent<Animator>();
            gambling_anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        
        CloseGamblingMenu();

        if (ability1_slot == null)
        {
            Debug.LogWarning("No se han añadido todas las referencias a [sc_AbilitiesSystem.cs]");
            return;
        }



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


        // Referencias de los slots
        attack_abilities_btn = new List<Button>();
        attack_abilities_im = new List<Image>();
        for (int i = 0; i < attack_ab_chooser.childCount; i++)
        {
            if (attack_ab_chooser.GetChild(i).name.ToLower().Contains("title") || attack_ab_chooser.GetChild(i).name.ToLower().Contains("text")) continue;

            attack_abilities_btn.Add(attack_ab_chooser.GetChild(i).GetComponent<Button>());
            attack_abilities_im.Add(attack_ab_chooser.GetChild(i).GetComponent<Image>());
        }
    }

    public void LoadAbilitiesData()
    {
        SaveManager sm = GameManager.gm.saveManager;

        PlayerData pd = sm.LoadSaveData();

        Ability[] abs_level = sm.LoadAbilitiesData();
        if (abs_level != null)
        {
            abs_level = new Ability[(int)Abilities.LAST_NO_USE];
            for (int i = 0; i < (int)Abilities.LAST_NO_USE; i++)
            {
                abs_level[i] = new Ability();
                abs_level[i].ability_level = 1.0f;
            }
        }
        
        // Levitate ability
        Ability ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Levitate name");
        ab.description = IdiomManager.instance.GetKeyText("Levitate description");
        ab.icon = sprite_levitate;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Exploding path ability
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Exploding name");
        ab.description = IdiomManager.instance.GetKeyText("Exploding description");
        ab.icon = sprite_explodepath;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Group ability
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Group name");
        ab.description = IdiomManager.instance.GetKeyText("Group description");
        ab.icon = sprite_group;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Plant mine ability
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Mine name");
        ab.description = IdiomManager.instance.GetKeyText("Mine description");
        ab.icon = sprite_mine;
        ab.rarity = AbilityType.ULTIMATE;

        abilities.Add(ab);

        // Hook
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Hook name");
        ab.description = IdiomManager.instance.GetKeyText("Hook description");
        ab.icon = sprite_hook;
        ab.rarity = AbilityType.BASIC;
        ab.cooldown = 5;

        abilities.Add(ab);

        // Stomp
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Stomp name");
        ab.description = IdiomManager.instance.GetKeyText("Stomp description");
        ab.icon = sprite_stomp;
        ab.rarity = AbilityType.BASIC;
        ab.cooldown = 5;

        abilities.Add(ab);

        // Hit N' Byebye
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("HitNBye name");
        ab.description = IdiomManager.instance.GetKeyText("HitNBye description");
        ab.icon = sprite_byebye;
        ab.rarity = AbilityType.PASSIVE;

        abilities.Add(ab);

        // Bloodthirsty
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Bloodthirsty name");
        ab.description = IdiomManager.instance.GetKeyText("Bloodthirsty description");
        ab.icon = sprite_bloodthirsty;
        ab.rarity = AbilityType.PASSIVE;

        abilities.Add(ab);

        // Hologram body
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Hologram name");
        ab.description = IdiomManager.instance.GetKeyText("Hologram description");
        ab.icon = sprite_hologram;
        ab.rarity = AbilityType.BASIC;
        ab.cooldown = 10;

        abilities.Add(ab);

        // Monkey bait
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Monkey name");
        ab.description = IdiomManager.instance.GetKeyText("Monkey description");
        ab.icon = sprite_monkey;
        ab.rarity = AbilityType.BASIC;
        ab.cooldown = 15;

        abilities.Add(ab);
        
        // Kill N Speed
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("KillNSpeed name");
        ab.description = IdiomManager.instance.GetKeyText("KillNSpeed description");
        ab.icon = sprite_killnspeed;
        ab.rarity = AbilityType.PASSIVE;
        ab.cooldown = 0;

        abilities.Add(ab);

        // Recharge
        ab = new Ability();

        ab.name = IdiomManager.instance.GetKeyText("Recharge name");
        ab.description = IdiomManager.instance.GetKeyText("Recharge description");
        ab.icon = sprite_recharge;
        ab.rarity = AbilityType.PASSIVE;
        ab.cooldown = 0;

        abilities.Add(ab);


        /// ¡¡ Recuerda añadir la nueva habilidad al enum !! -------------------------------------------------------------


        // Asigna la información loopeable
        for (int i = 0; i < (int)Abilities.LAST_NO_USE; i++)
        {
            abilities[i].id = i;
            if (methods_abilities.Count > 0 && i < methods_abilities.Count)
                abilities[i].ability_event = methods_abilities[i];
            abilities[i].type = (Abilities)i;
            abilities[i].current_cooldown = abilities[i].cooldown;
        }

        if (abilities.Count != (int)Abilities.LAST_NO_USE)
            Debug.LogWarning("Las habilidades del enum no coinciden con las añadidas en el void Start");

        sm.SaveAbilities(abilities.ToArray());

        if (abs_level != null && abs_level.Length > 0)
        {
            for (int i = 0; i < abs_level.Length; i++)
            {
                abilities[i].ability_level = abs_level[i].ability_level;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetRandomAbilities();
        }

        if (gambling_open)
        {
            // Reinicia la animación si se queda estancada
            AnimatorStateInfo animStateInfo = gambling_anim.GetCurrentAnimatorStateInfo(0);
            if (animStateInfo.IsName("anim_open_gambling") && animStateInfo.normalizedTime < 1)
            {
                timer += Time.deltaTime;
                if (timer >= 1)
                {
                    gambling_anim.Play("anim_open_gambling");
                }
                else if (timer >= 3)
                {
                    gambling_anim.Play("anim_open_gambling_still");
                }
            }
        }
    }

    void AddAbilityIconOnUI(Ability ab)
    {
        GameObject ab_icon = new GameObject();
        ab_icon.name = ab.name;

        Image im = ab_icon.AddComponent<Image>();
        im.sprite = ab.icon;

        ab_icon.transform.SetParent(ob_abilities_ui_holder.transform);
    }

    /// <summary>
    /// Abre el menú de selección y muestra tres habilidades aleatorias
    /// </summary>
    public void GetRandomAbilities()
    {
        // If the player already has all abilities, return
        if (abilities.Count <= 0) return;

        txt_description.gameObject.SetActive(false);
        im_description.gameObject.SetActive(false);

        gambling_open = true;

        Time.timeScale = 0;

        slots_buttons[0].Select();

        const int ability_count = 3;
        Ability[] abilities_to_show = new Ability[ability_count];

        rand_abilities_index.Clear();
        HashSet<int> usedIndexes = new HashSet<int>();

        int max_attempts = abilities.Count * 10;
        int attempts = 0;
        int foundAbilities = 0;

        while (foundAbilities < ability_count && attempts < max_attempts)
        {
            int rand = Random.Range(0, abilities.Count);
            attempts++;

            if (usedIndexes.Contains(rand) || abilities_equipped.Exists(a => a.id == abilities[rand].id))
                continue;

            // Marca la habilidad como usada
            abilities_to_show[foundAbilities] = abilities[rand];
            usedIndexes.Add(rand);
            rand_abilities_index.Add(abilities[rand].id);
            foundAbilities++;
        }

        for (int i = 0; i < ability_count; i++)
        {
            for (int j = 0; j < abilities_equipped.Count; j++)
            {
                if (abilities_to_show[i] == null || abilities_to_show[i].id == abilities_equipped[j].id)
                {
                    abilities_to_show[i] = abilities[Random.Range(0, abilities.Count)];
                }
            }
        }

        // Asigna las habilidades seleccionadas a la UI
        for (int i = 0; i < ability_count; i++)
        {
            if (abilities_to_show[i] != null)
            {
                int ab_num = i;

                slots_buttons[i].onClick.RemoveAllListeners();
                slots_buttons[i].onClick.AddListener(() =>
                {
                    InitAbility(abilities_to_show[ab_num]);

                    Debug.Log("Ability selected: " + abilities_to_show[ab_num].name);

                    abilities_equipped.Add(abilities_to_show[ab_num]);

                    abilities.Remove(abilities_to_show[ab_num]);

                    CloseGamblingMenu(abilities_to_show[ab_num].rarity);

                    slots_buttons[ab_num].onClick.RemoveAllListeners();
                });


                EventTrigger bt_events;
                if (slots_buttons[i].gameObject.TryGetComponent<EventTrigger>(out bt_events))
                {
                    // Todo ok
                }
                else
                {
                    // Añade el componente si no lo tiene
                    bt_events = slots_buttons[i].gameObject.AddComponent<EventTrigger>();
                }


                // Configura el tipo de evento para entrada del ratón
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };

                // Muestra el texto de descripción
                entry.callback.AddListener((data) =>
                {
                    txt_description.text = abilities_to_show[ab_num].description;
                    StartCoroutine(MoveDescriptionUIToCursor(ab_num));
                });

                bt_events.triggers.Add(entry);
                
                // Configura el evento de selección para mando
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Select
                };

                entry.callback.AddListener((data) =>
                {
                    txt_description.text = abilities_to_show[ab_num].description;

                    RectTransform rect = slots_buttons[ab_num].gameObject.GetComponent<RectTransform>();
                    txt_description.rectTransform.anchoredPosition = rect.anchoredPosition;
                    txt_description.rectTransform.position = new Vector2(rect.position.x, rect.position.y + rect.rect.height);
                    txt_description.rectTransform.sizeDelta = new Vector2(txt_description.rectTransform.sizeDelta.x, txt_description.preferredHeight);

                    im_description.rectTransform.anchoredPosition = txt_description.rectTransform.anchoredPosition;
                    im_description.rectTransform.sizeDelta = txt_description.rectTransform.sizeDelta * 1.25f;

                    gambling_anim.enabled = false;

                    txt_description.gameObject.SetActive(true);
                    im_description.gameObject.SetActive(true);
                });

                bt_events.triggers.Add(entry);
                
                // Configura el evento de salida del ratón
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };

                entry.callback.AddListener((data) =>
                {
                    txt_description.gameObject.SetActive(false);
                    im_description.gameObject.SetActive(false);

                    StopAllCoroutines();
                });

                bt_events.triggers.Add(entry);
                
                // Configura el evento de deselección
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Deselect
                };

                entry.callback.AddListener((data) =>
                {
                    txt_description.gameObject.SetActive(false);
                    im_description.gameObject.SetActive(false);
                });

                bt_events.triggers.Add(entry);




                slots_images[i].sprite = abilities_to_show[i].icon;
                slots_texts[i].text = abilities_to_show[i].name;
            }
        }

        gambling_anim.enabled = true;
        ob_gamblingparent.SetActive(true);

        gambling_anim.Play("anim_open_gambling");

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    IEnumerator MoveDescriptionUIToCursor(int ab_num)
    {
        txt_description.gameObject.SetActive(true);
        im_description.gameObject.SetActive(true);

        Vector2 resolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        while (true)
        {

            RectTransform rect = slots_buttons[ab_num].gameObject.GetComponent<RectTransform>();

            txt_description.rectTransform.anchoredPosition = rect.anchoredPosition;
            txt_description.rectTransform.sizeDelta = new Vector2(txt_description.rectTransform.sizeDelta.x, txt_description.preferredHeight);

            txt_description.rectTransform.position = new Vector2(Input.mousePosition.x + txt_description.rectTransform.sizeDelta.x * 0.7f,
                                                                 Input.mousePosition.y + txt_description.rectTransform.sizeDelta.y * 0.7f);

            im_description.rectTransform.sizeDelta = txt_description.rectTransform.sizeDelta * 1.1f;

            txt_description.rectTransform.position = new Vector2(Mathf.Clamp(txt_description.rectTransform.position.x, 0, resolution.x - (im_description.rectTransform.sizeDelta.x / 2)),
                                                                 Mathf.Clamp(txt_description.rectTransform.position.y, 0, resolution.y - (im_description.rectTransform.sizeDelta.y / 2)));

            im_description.rectTransform.anchoredPosition = txt_description.rectTransform.anchoredPosition;
            yield return null;
        }
    }

    private void InitAbility(Ability ab)
    {
        switch (ab.rarity)
        {
            case AbilityType.ULTIMATE: // habilidades activadas al volver en el tiempo
                ReturnScript.instance.ability.AddListener(() => methods_abilities[(int)ab.type].Invoke());

                AddAbilityIconOnUI(ab);
                break;


            case AbilityType.BASIC: // habilidades melee
                if (AttackSystem.instance.equipped_attacks.Count >= 2)
                {
                    attack_ab_chooser.gameObject.SetActive(true);
                    // Selecciona el primer botón (navegación por mando)
                    attack_abilities_btn[0].Select();

                    // Inicializa la UI
                    for (int j = 0; j < attack_ab_chooser.childCount && j < AttackSystem.instance.equipped_attacks.Count; j++)
                    {
                        attack_abilities_im[j].sprite = AttackSystem.instance.equipped_attacks[j].icon;
                        int ab_id = j;
                        attack_abilities_btn[j].onClick.AddListener(() =>
                        {
                            // Quita la habilidad seleccionada y pone la nueva
                            AttackSystem.instance.equipped_attacks.RemoveAt(ab_id);
                            AttackSystem.instance.AddAttack(ab);

                            attack_ab_chooser.gameObject.SetActive(false);
                            CloseGamblingMenu();
                        });
                    }
                }
                else
                {
                    AttackSystem.instance.AddAttack(ab);
                    CloseGamblingMenu();
                }
                break;

            case AbilityType.PASSIVE:
                // Inicializa la habilidad pasiva
                ab.ability_event.Invoke();

                AddAbilityIconOnUI(ab);
                break;
        }
    }

    public void AddAbility(string name)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (name == abilities[i].name)
            {
                abilities_equipped.Add(abilities[i]);

                // Añade la habilidad
                InitAbility(abilities[i]);


                abilities.RemoveAt(i); /// Elimina la habilidad para que no pueda volver a tocar
            }
        }
    }

    public void CloseGamblingMenu(AbilityType selected_type = AbilityType.LAST_NO_USE)
    {
        if (selected_type == AbilityType.BASIC && AttackSystem.instance.equipped_attacks.Count >= 2 || ob_gamblingparent == null) return;

        gambling_open = false;

        ob_gamblingparent.SetActive(false);

        txt_description.gameObject.SetActive(false);
        im_description.gameObject.SetActive(false);

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        timer = 0;

        Time.timeScale = 1;
    }
    /// <summary>
    /// Añade experiencia para desbloquear una habilidad
    /// </summary>
    public void AddXP(float value)
    {
        if (instance == null) return;

        current_xp += value;

        if (current_xp >= max_xp)
        {
            current_xp = 0;
            max_xp *= 1.25f;

            GetRandomAbilities(); /// Cuando el jugador consiga suficiente experiencia conseguirá una habilidad
        }

        txt_xp.text = $"{current_xp} / {max_xp}";
        im_xp_bar.fillAmount = current_xp / max_xp;

        StartCoroutine(ShowXPBar());
    }

    private IEnumerator ShowXPBar()
    {
        UnityEngine.UI.Image im_parent = im_xp_bar.transform.parent.GetComponent<Image>();
        //GameManager.gm.UIGrowButton(im_parent);
        while (im_xp_bar.color.a < 1)
        {
            im_xp_bar.color = new Color(im_xp_bar.color.g, im_xp_bar.color.g, im_xp_bar.color.b, im_xp_bar.color.a + Time.deltaTime * 3);
            im_parent.color = new Color(im_parent.color.g, im_parent.color.g, im_parent.color.b, im_parent.color.a + Time.deltaTime * 3);
            txt_xp.color = new Color(txt_xp.color.g, txt_xp.color.g, txt_xp.color.b, txt_xp.color.a + Time.deltaTime * 3);
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        while (im_xp_bar.color.a > 0)
        {
            im_xp_bar.color = new Color(im_xp_bar.color.g, im_xp_bar.color.g, im_xp_bar.color.b, im_xp_bar.color.a - Time.deltaTime * 2);
            im_parent.color = new Color(im_parent.color.g, im_parent.color.g, im_parent.color.b, im_parent.color.a - Time.deltaTime * 2);
            txt_xp.color = new Color(txt_xp.color.g, txt_xp.color.g, txt_xp.color.b, txt_xp.color.a - Time.deltaTime * 2);
            yield return null;
        }
    }
}