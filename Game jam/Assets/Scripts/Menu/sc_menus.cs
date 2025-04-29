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

public class menus : MonoBehaviour
{
    private Animator anim;

    public Button button;

    private AudioSource audioSource;

    public List<AudioClip> audiosToPlay;


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

    public Transform abilities_holder;
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

    [Header("Abilities ref")]
    public ScrollRect ab_scroll;
    public float scrollSpeed;
    public TMP_Text ab_description_text;

    private LayoutElement layout_element;

    private List<Sprite> ab_sprites;

    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        sm = GetComponent<SaveManager>();

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
            sprite_monkey
        };

        LoadAbilities();
    }

    public void LoadStatsTexts()
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

        pd = sm.LoadSaveData();

        txt_damage.text = "DAMAGE: " + pd.damage.ToString();
        txt_speed.text = "SPEED: " + pd.speed.ToString();
        txt_stamina.text = "STAMINA: " + pd.stamina.ToString();
        txt_hp.text = "HP: " + pd.hp.ToString();
        txt_explosion_range.text = "EXPLOSION RANGE: " + pd.explosion_range.ToString();
        txt_skill_points.text = "SKILL POINTS: " + pd.score.ToString();
    }

    public void UpgradeStat(GameObject stat)
    {

        PlayerData pd = sm.LoadSaveData();

        if (pd == null || pd.score < 1) return;

        string name = stat.name;

        switch (name)
        {
            case "DAMAGE":
                pd.damage++;
                txt_damage.text = "DAMAGE: " + pd.damage.ToString();
                break;
            case "SPEED":
                pd.speed += 10;
                txt_speed.text = "SPEED: " + pd.speed.ToString();
                break;
            case "EXPLOSION RANGE":
                pd.explosion_range += 0.5f;
                txt_explosion_range.text = "EXPLOSION RANGE: " + pd.explosion_range.ToString();
                break;
            case "HP":
                pd.hp++;
                txt_hp.text = "HP: " + pd.hp.ToString();
                break;
            case "STAMINA":
                pd.stamina++;
                txt_stamina.text = "STAMINA: " + pd.stamina.ToString();
                break;
        }

        pd.score--;
        txt_skill_points.text = "SKILL POINTS: " + pd.score.ToString();

        Debug.Log("Stat upgraded");
        sm.SaveGame(pd);
    }


    // Habilidades
    public void LoadAbilities()
    {
        SaveManager sm = new SaveManager();
        Ability[] ab_l = sm.LoadAbilitiesData();

        if (ab_l != null)
        {
            for (int i = 0; i < ab_l.Length; i++)
            {
                GameObject ob = Instantiate(prefab_ability_model);
                ob.transform.SetParent(abilities_holder);
                ob.transform.localScale = Vector3.one * 0.3f;

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
                    StartCoroutine(RelocateScroll(ob_rect));
                });

                event_tr.triggers.Add(entry);

                // Configura el evento al hacer click
                string description = ab_l[i].description;
                btn.onClick.AddListener(() =>
                {
                    LayoutElement layout_el = btn.gameObject.GetComponent<LayoutElement>();

                    Transform parent = btn.transform;
                    int child_num = GameManager.GetChildIndex(btn.transform);
                    btn.transform.SetParent(btn.transform.parent.parent, true);
                    btn.transform.SetAsLastSibling();

                    if (layout_element != null)
                    {
                        layout_element.ignoreLayout = false;
                    }
                    layout_element = layout_el;
                    layout_element.ignoreLayout = true;

                    ab_description_text.text = description;

                    ab_description_text.gameObject.SetActive(true);

                    StartCoroutine(RelocateAbility(ob, im, btn, txt, layout_element, child_num, abilities_holder));
                });
            }
        }
    }

    private IEnumerator RelocateAbility(GameObject ob, UnityEngine.UI.Image icon, UnityEngine.UI.Button btn, TMP_Text title, LayoutElement layout_el, int child_num, Transform parent)
    {
        UnityEngine.UI.Image bg = ob.GetComponent<Image>();

        Vector2 scaled_bg = new Vector2(bg.rectTransform.sizeDelta.x * 2.5f, bg.rectTransform.sizeDelta.y * 2);

        Vector2 bg_new_pos = new Vector2(0, 0);
        bg.rectTransform.anchorMin = new Vector2(0.35f, 0.5f);
        bg.rectTransform.anchorMax = bg.rectTransform.anchorMin;
        bg.rectTransform.pivot = bg.rectTransform.anchorMax;

        Vector2 icon_previous_pos = icon.rectTransform.anchoredPosition;
        Vector2 icon_new_pos = new Vector2(-bg.rectTransform.rect.width + icon.rectTransform.rect.xMax / 2, bg.rectTransform.rect.height - icon.rectTransform.rect.yMax);

        Vector2 txt_previous_anchor = title.rectTransform.anchoredPosition;
        Vector2 txt_new_pos = new Vector2(-bg.rectTransform.rect.width + title.rectTransform.rect.xMax / 2, 0.5f);

        ab_description_text.transform.SetParent(ob.transform, false);
        ab_description_text.rectTransform.anchorMin = new Vector2(0.5f, 0.1f);
        ab_description_text.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        ab_description_text.rectTransform.pivot = Vector2.one * 0.5f;


        while (layout_el.ignoreLayout)
        {
            ab_description_text.gameObject.SetActive(true);

            bg.rectTransform.sizeDelta = Vector2.Lerp(bg.rectTransform.sizeDelta, scaled_bg, Time.deltaTime * 2);
            bg.rectTransform.anchoredPosition = Vector2.Lerp(bg.rectTransform.anchoredPosition, bg_new_pos, Time.deltaTime * 2);

            icon.rectTransform.anchoredPosition = Vector2.Lerp(icon.rectTransform.anchoredPosition, icon_new_pos, Time.deltaTime * 2);
            title.rectTransform.anchoredPosition = Vector2.Lerp(title.rectTransform.anchoredPosition, txt_new_pos, Time.deltaTime * 2);

            yield return null;
        }

        ab_description_text.gameObject.SetActive(false);
        ob.SetActive(false);

        bg.transform.SetParent(parent, false);
        bg.transform.SetSiblingIndex(child_num);

        yield return null;

        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        grid.enabled = false;
        yield return null;
        grid.enabled = true;

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);

        ob.SetActive(true);

        bg.rectTransform.anchorMax = Vector2.one * 0.5f;
        bg.rectTransform.anchorMin = Vector2.one * 0.5f;
        bg.rectTransform.pivot = Vector2.one * 0.5f;

        icon.rectTransform.anchoredPosition = icon_previous_pos;
        title.rectTransform.anchoredPosition = txt_previous_anchor;
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
        Application.Quit();
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
        if (con.performed && abilities_holder.gameObject.activeSelf)
        {
            Vector2 dir = con.ReadValue<Vector2>();

            ab_scroll.horizontalNormalizedPosition -= dir.y * scrollSpeed;
        }
    }
}
