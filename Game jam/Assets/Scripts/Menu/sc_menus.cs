using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class menus : MonoBehaviour
{
    public static menus instance { get; private set; }
    private Animator anim;

    public Button button;

    private AudioSource audioSource;

    public List<AudioClip> audiosToPlay;

    [Header("UI clips")]
    public AudioClip clip_cantupgrade;
    public AudioClip clip_upgraded;
    public AudioClip clip_click;
    public AudioClip clip_selected;

    [Header("References")]
    public Canvas main_canvas;
    public TMP_Text txt_damage;
    public TMP_Text txt_speed;
    public TMP_Text txt_stamina;
    public TMP_Text txt_explosion_range;
    public TMP_Text txt_hp;

    public TMP_Text txt_skill_points;

    [SerializeField] Slider sliderVolume;
    [SerializeField] Toggle toggleScreenMode;

    private GameObject menuOpened;

    Scene actualScene;

    SaveManager sm;

    public Transform abilities_holder_ultimate;
    public Transform abilities_holder_basic;
    public Transform abilities_holder_passive;
    public GameObject prefab_ability_model;
    public TMP_Text txt_description_ability;

    [Header("Icons")]
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

    [Header("Abilities ref")]
    public ScrollRect ab_scroll;
    public float scrollSpeed;
    public TMP_Text ab_description_text;

    private bool close_ability_upgrade = false;
    public bool on_ability_menu = false;

    private LayoutElement layout_element;

    private List<Sprite> ab_sprites;

    private void Awake()
    {
        sm = GetComponent<SaveManager>();

        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        Time.timeScale = 1;

        LoadPlayerStats();

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;


        ab_sprites = new List<Sprite>
        {
            sprite_levitate,
            sprite_explodepath,
            sprite_group,
            sprite_mine,
            sprite_hook,
            sprite_stomp,
            sprite_byebye,
            sprite_bloodthirsty,
            sprite_hologram,
            sprite_monkey,
            sprite_killnspeed
        };

        LoadAbilities();
        LoadStatsTexts();
    }

    public PlayerData LoadPlayerStats()
    {
        PlayerData pd = sm.LoadSaveData();

        if (pd == null)
        {
            pd = new PlayerData();
            pd.damage = 8;
            pd.speed = 250;
            pd.stamina = 10;
            pd.hp = 15;
            pd.explosion_range = 4;
            pd.score = 0;

            sm.SaveGame(pd);
        }
        if (pd.hp < 0)
        {
            pd.hp = 15;
            sm.SaveGame(pd);
        }
        if (pd.score < 0)
        {
            pd.score = 0;
            sm.SaveGame(pd);
        }

        return pd;
    }
    public void LoadStatsTexts()
    {
        PlayerData pd = LoadPlayerStats();

        txt_damage.text = "DAMAGE: " + pd.damage.ToString();
        txt_speed.text = "SPEED: " + pd.speed.ToString();
        txt_stamina.text = "STAMINA: " + pd.stamina.ToString();
        txt_hp.text = "HP: " + pd.hp.ToString();
        txt_explosion_range.text = "EXPLOSION RANGE: " + pd.explosion_range.ToString();
        txt_skill_points.text = "SKILL POINTS: " + pd.score.ToString("F2");
    }

    public void UpgradeStat(GameObject stat)
    {
        PlayerData pd = sm.LoadSaveData();

        if (pd == null || pd.score < 1)
        {
            GameManager.gm.ShakeUIElement(stat.GetComponent<RectTransform>(), 0.5f, 30);
            SoundManager.instance.InstantiateSound(clip_cantupgrade, transform.position);
            return;
        }

        string name = stat.name;

        switch (name)
        {
            case "DAMAGE":
                pd.damage++;
                txt_damage.text = "Damage: " + pd.damage.ToString();
                break;
            case "SPEED":
                pd.speed += 10;
                txt_speed.text = "Speed: " + pd.speed.ToString();
                break;
            case "EXPLOSION RANGE":
                pd.explosion_range += 0.5f;
                txt_explosion_range.text = "Explosion range: " + pd.explosion_range.ToString();
                break;
            case "HP":
                pd.hp++;
                txt_hp.text = "HP: " + pd.hp.ToString();
                break;
            case "STAMINA":
                pd.stamina++;
                txt_stamina.text = "Stamina: " + pd.stamina.ToString();
                break;
        }

        pd.score--;
        txt_skill_points.text = "Skill points: " + pd.score.ToString();

        Debug.Log("Stat upgraded");
        sm.SaveGame(pd);
    }

    public Vector2 GetAnchoredPositionOnCanvas(Canvas canvas, Vector2 screenPosition)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 localPoint;

        // For Screen Space - Camera or World
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            cam,
            out localPoint
        );

        return localPoint; // This is the anchoredPosition you can assign
    }

    // Habilidades
    public void LoadAbilities()
    {
        SaveManager sm = GameManager.gm.saveManager;
        Ability[] ab_l = sm.LoadAbilitiesData();

        if (ab_l != null)
        {
            for (int i = 0; i < ab_l.Length; i++)
            {
                GameObject ob = Instantiate(prefab_ability_model);
                Transform parent = abilities_holder_ultimate;
                switch (ab_l[i].rarity)
                {
                    case AbilitiesSystem.AbilityType.PASSIVE:
                        parent = abilities_holder_passive;
                        break;
                    case AbilitiesSystem.AbilityType.BASIC:
                        parent = abilities_holder_basic;
                        break;
                    case AbilitiesSystem.AbilityType.ULTIMATE:
                        parent = abilities_holder_ultimate;
                        break;
                }
                ob.transform.SetParent(parent);

                ob.transform.localScale = Vector3.one * 3f;

                UnityEngine.UI.Image im = ob.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button btn = ob.GetComponentInChildren<UnityEngine.UI.Button>();
                TMP_Text txt = ob.GetComponentInChildren<TMP_Text>();

                im.sprite = ab_sprites[i];
                txt.text = ab_l[i].name;

                // Configura el evento de selección -----------------------------------
                EventTrigger event_tr = ob.GetComponentInChildren<EventTrigger>();
                if (event_tr == null)
                {
                    event_tr = ob.AddComponent<EventTrigger>();
                }

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Select;

                RectTransform ob_rect = ob.GetComponent<RectTransform>();
                entry.callback.AddListener((data) =>
                {
                    SoundManager.instance.InstantiateSound(clip_selected, transform.position);
                    StartCoroutine(RelocateScroll(ob_rect));
                });

                event_tr.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Deselect;
                entry.callback.AddListener((data) =>
                {
                    CloseAbilityDescription();
                });

                event_tr.triggers.Add(entry);

                // Configura el evento al hacer click
                string description = ab_l[i].description;
                btn.onClick.AddListener(() =>
                {
                    AbilityButton(ob, im, txt, btn, description, parent);
                });
            }
        }
    }

    private void CloseAbilityDescription()
    {
        close_ability_upgrade = true;
    }

    private void AbilityButton(GameObject ob, UnityEngine.UI.Image im, TMP_Text txt, UnityEngine.UI.Button btn, string description, Transform parent)
    {
        LayoutElement layout_el = btn.gameObject.GetComponent<LayoutElement>();

        int child_num = GameManager.GetChildIndex(btn.transform);
        

        if (layout_element != null)
        {
            layout_element.ignoreLayout = false;
        }
        layout_element = layout_el;
        layout_element.ignoreLayout = true;

        ab_description_text.text = description;

        ab_description_text.gameObject.SetActive(true);

        SoundManager.instance.InstantiateSound(clip_click, transform.position);

        StartCoroutine(RelocateAbility(ob, im, btn, txt, layout_element, child_num, parent));
    }

    private IEnumerator RelocateAbility(GameObject ob, UnityEngine.UI.Image icon, UnityEngine.UI.Button btn, TMP_Text title, LayoutElement layout_el, int child_num, Transform parent)
    {
        on_ability_menu = true;

        // Configura el nuevo evento del botón
        SaveManager sm = GameManager.gm.saveManager;
        Ability[] abs = sm.LoadAbilitiesData();
        /// Consigue la habilidad actual
        Ability ab = new Ability();
        for (int i = 0; i < abs.Length; i++)
        {
            if (abs[i].name == title.text)
            {
                ab = abs[i];
                break;
            }
        }

        float current_level = ab.ability_level; /// Guarda el nivel de la habilidad actual

        PlayerData pd = sm.LoadSaveData();


        UnityEngine.UI.Image bg = ob.GetComponent<UnityEngine.UI.Image>();

        Vector3 initial_size = bg.rectTransform.sizeDelta;
        Vector3 initial_scale = bg.rectTransform.localScale;


        HorizontalLayoutGroup grid = parent.GetComponent<HorizontalLayoutGroup>();
        grid.enabled = false;

        Transform new_parent = btn.transform.parent.parent.parent.parent.parent.parent;

        // Cambia el parent y lo reposiciona
        Vector3[] corners = new Vector3[4];
        bg.rectTransform.GetWorldCorners(corners);
        Vector3 child_center = (corners[0] + corners[2]) * 0.5f;

        btn.transform.SetParent(new_parent, false); /// Cambia el parent

        Vector3 worldPos = btn.transform.position;

        btn.transform.SetAsLastSibling();

        Vector2 screen_pos = RectTransformUtility.WorldToScreenPoint(null, child_center);
        Vector2 bg_start_anchor = GetAnchoredPositionOnCanvas(main_canvas, child_center);
        bg_start_anchor = new Vector2(bg_start_anchor.x, bg_start_anchor.y + 45);
        bg.rectTransform.anchoredPosition = bg_start_anchor;

        // Quita el evento anterior y añade uno nuevo
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            if (pd.score > 1)
            {
                pd.score--;
                sm.SaveGame(pd);
                Ability[] abilities = sm.LoadAbilitiesData();
                for (int i = 0; i < abilities.Length; i++)
                {
                    if (abilities[i].name == ab.name)
                    {
                        abilities[i].ability_level += 0.1f;
                        title.text = $"{ab.name}\n\nPress to upgrade!\nCurrent level: {abilities[i].ability_level.ToString("F2")}\nSkill points: {pd.score.ToString("F2")}";
                        ab = abilities[i];
                        break;
                    }
                }

                sm.SaveAbilities(abilities);
                SoundManager.instance.InstantiateSound(clip_upgraded, transform.position);
            }
            else // No tiene suficientes puntos de skill para mejorar
            {
                GameManager.gm.ShakeUIElement(bg.rectTransform, 0.5f, 120);
                SoundManager.instance.InstantiateSound(clip_cantupgrade, transform.position);
            }
        });


        // Imagen del fondo
        Vector2 scaled_bg = new Vector2(bg.rectTransform.sizeDelta.x * 2.5f, bg.rectTransform.sizeDelta.y * 2);

        Vector2 bg_new_pos = new Vector2(0, 0);
        bg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        bg.rectTransform.anchorMax = bg.rectTransform.anchorMin;
        bg.rectTransform.pivot = bg.rectTransform.anchorMax;

        // Icono de la habilidad
        Vector2 icon_previous_pos = icon.rectTransform.anchoredPosition;
        Vector2 icon_new_pos = new Vector2(-bg.rectTransform.rect.width + icon.rectTransform.rect.xMax / 2, bg.rectTransform.rect.height - icon.rectTransform.rect.yMax * 1.75f);

        // Título de la habilidad
        Vector2 txt_previous_pos = title.rectTransform.anchoredPosition;
        Vector2 txt_new_pos = new Vector2(-bg.rectTransform.rect.width + title.rectTransform.rect.xMax, 0.4f);
        Vector2[] txt_previous_anchors = new Vector2[]
        {
            title.rectTransform.anchorMin,
            title.rectTransform.anchorMax
        };
        txt_new_pos = Vector2.zero;
        title.alignment = TextAlignmentOptions.Left;
        title.rectTransform.anchorMin = new Vector2(0.05f, 0.05f);
        title.rectTransform.anchorMax = new Vector2(0.45f, 0.5f);

        // Texto de descripción
        ab_description_text.transform.SetParent(ob.transform, false);
        ab_description_text.rectTransform.anchorMin = new Vector2(0.5f, 0.1f);
        ab_description_text.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        ab_description_text.rectTransform.pivot = Vector2.one * 0.5f;

        string original_text = title.text;
        title.text = $"{ab.name}\n\nPress to upgrade!\nCurrent level: {ab.ability_level.ToString("F2")}\nSkill points: {pd.score.ToString("F2")}";

        close_ability_upgrade = false;

        // Mueve los elementos a su nueva posición y cambia el tamaño del fondo
        while (layout_el.ignoreLayout)
        {
            ab_description_text.gameObject.SetActive(true);

            bg.rectTransform.sizeDelta = Vector2.Lerp(bg.rectTransform.sizeDelta, scaled_bg, Time.deltaTime * 4);


            bg.rectTransform.anchoredPosition = Vector2.Lerp(bg.rectTransform.anchoredPosition, bg_new_pos, Time.deltaTime * 2);

            icon.rectTransform.anchoredPosition = Vector2.Lerp(icon.rectTransform.anchoredPosition, icon_new_pos, Time.deltaTime * 2);
            title.rectTransform.anchoredPosition = Vector2.Lerp(title.rectTransform.anchoredPosition, txt_new_pos, Time.deltaTime * 2);

            if (close_ability_upgrade) // Si el jugador le da al botón de volver mientras está en el menú se cerrará
                break;

            yield return null;
        }

        title.text = ab.name;

        // Pone todos los elementos en su disposición original
        bg.rectTransform.anchorMax = Vector2.one * 0.5f;
        bg.rectTransform.anchorMin = Vector2.one * 0.5f;
        bg.rectTransform.pivot = Vector2.one * 0.5f;

        icon.rectTransform.anchoredPosition = icon_previous_pos;

        /// Vuelve a poner el título como estaba
        title.rectTransform.anchoredPosition = txt_previous_pos;
        title.text = original_text;
        title.alignment = TextAlignmentOptions.Center;
        title.rectTransform.anchorMin = txt_previous_anchors[0];
        title.rectTransform.anchorMax = txt_previous_anchors[1];

        ab_description_text.gameObject.SetActive(false);

        // Quita todos los eventos al botón para evitar problemas
        btn.onClick.RemoveAllListeners();

        while (Vector2.Distance(bg.rectTransform.sizeDelta, initial_size) > 0.1f)
        {
            bg.rectTransform.sizeDelta = Vector2.Lerp(bg.rectTransform.sizeDelta, initial_size, Time.deltaTime * 10);
            bg.rectTransform.anchoredPosition = Vector2.Lerp(bg.rectTransform.anchoredPosition, bg_start_anchor, Time.deltaTime * 5);

            icon.rectTransform.anchoredPosition = Vector2.Lerp(icon.rectTransform.anchoredPosition, icon_previous_pos, Time.deltaTime * 5);
            title.rectTransform.anchoredPosition = Vector2.Lerp(title.rectTransform.anchoredPosition, txt_previous_pos, Time.deltaTime * 4.5f);

            yield return null;
        }

        close_ability_upgrade = false;
        if (layout_el.ignoreLayout)
            layout_el.ignoreLayout = false;

        SoundManager.instance.InstantiateSound(clip_selected, transform.position); /// Hace un sonido para indicar la deselección

        bg.rectTransform.sizeDelta = initial_size;
        bg.rectTransform.localScale = initial_scale;

        bg.transform.SetParent(parent, true);
        bg.transform.SetSiblingIndex(child_num);


        yield return null; // Espera un frame para que los cambios de apliquen

        grid.enabled = false;
        yield return new WaitForSeconds(0.1f); // Espera un frame para que los cambios de apliquen
        grid.enabled = true;

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);



        // Vuelve a añadir el evento del botón
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            AbilityButton(ob, icon, title, btn, ab.description, parent);
        });

        on_ability_menu = false;
    }

    // Recoloca el scroll de las habilidades
    private IEnumerator RelocateScroll(RectTransform rect)
    {
        RectTransform parent = rect.parent.parent.GetComponent<RectTransform>();

        Vector3[] childCorners = new Vector3[4];
        Vector3[] parentCorners = new Vector3[4];

        rect.GetWorldCorners(childCorners);
        parent.GetWorldCorners(parentCorners);

        Vector3 child_center = (childCorners[0] + childCorners[2]) * 0.5f;
        Debug.Log($"Center = {child_center}, parentC0 = {parentCorners[0]}, parentC2 = {parentCorners[2]}");


        float timer = 0;

        // izquierda
        if (child_center.x < parentCorners[0].x)
        {
            Debug.Log("Izquierda");
            while (child_center.x < parentCorners[0].x)
            {
                ab_scroll.horizontalNormalizedPosition -= Time.deltaTime * 0.5f;

                timer += Time.deltaTime;
                if (timer >= 2)
                    break;
                yield return null;
            }
        }
        // derecha
        else if (child_center.x > parentCorners[2].x)
        {
            Debug.Log("Derecha");

            while (child_center.x > parentCorners[2].x)
            {
                ab_scroll.horizontalNormalizedPosition += Time.deltaTime * 0.5f;

                timer += Time.deltaTime;
                if (timer >= 2)
                    break;
                yield return null;
            }
        }
        else
        {
            Debug.Log("Child is in center");
        }
    }

    public void PlayGame()
    {
        DontDestroyOnLoad(SoundManager.instance.InstantiateSound(audiosToPlay[0], transform.position));
        SceneManager.LoadScene("SampleScene");
    }

    public void ButtonSelected()
    {
        audioSource.clip = audiosToPlay[1];// Sonido de selección
        audioSource.loop = false;
        audioSource.Play();
    }

    public void SelectedChickenAnimation()
    {
        anim.Play("Menu_scream_animation");
        audioSource.clip = audiosToPlay[2];
        audioSource.loop = false;
        audioSource.Play();
    }

    IEnumerator WaitToContinue()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        UnityEngine.Application.Quit();
    }



    // Cambiar volumen
    public void ChangeVolume()
    {
        AudioListener.volume = sliderVolume.value;
    }
    public void ChangeScreenMode()
    {
        switch (toggleScreenMode.isOn)
        {
            case true:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case false:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }
    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }


    public void MoveAbilitiesScroll(InputAction.CallbackContext con)
    {
        if (con.performed && abilities_holder_ultimate.gameObject.activeSelf)
        {
            Vector2 dir = con.ReadValue<Vector2>();

            ab_scroll.horizontalNormalizedPosition -= dir.x * scrollSpeed;
        }
    }
    public void CloseAbilityInfo(InputAction.CallbackContext con)
    {
        if (con.performed && ab_description_text.gameObject.activeSelf)
        {
            CloseAbilityDescription();
        }
    }
}
