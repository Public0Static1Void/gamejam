using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class AttackSystem : MonoBehaviour
{
    public static AttackSystem instance { get; private set; }

    public List<Ability> equipped_attacks = new List<Ability>();

    private int current_attack = 0;

    public List<UnityEngine.UI.Image> slots_abilities;
    public List<UnityEngine.UI.Image> slots_cooldowns;

    private Ability[] abilities_order;

    private List<Vector2> ui_positions = new List<Vector2>();

    private bool moving_ui = false;

    [Header("Icon")]
    public UnityEngine.UI.Image im_attack_icon;
    public Sprite spr_left_click;
    public Sprite spr_R2;
    public Sprite spr_RT;
    Vector2[] dir;

    private string last_controller = "";

    private Animator anim;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        abilities_order = new Ability[slots_abilities.Count];

        dir = new Vector2[slots_cooldowns.Count];

        for (int i = 0; i < slots_cooldowns.Count; i++)
        {
            ui_positions.Add(slots_cooldowns[i].rectTransform.anchorMax);

            Color col = Color.white;
            slots_abilities[i].color = new Color(col.r, col.g, col.b, 0);
            slots_cooldowns[i].color = new Color(col.r, col.g, col.b, 0);
        }

        for (int i = 0; i < dir.Length; i++)
        {
            int target = i + 1;
            if (target >= dir.Length)
                target = 0;

            dir[i] = (ui_positions[target] - ui_positions[i]).normalized;
        }

        im_attack_icon.gameObject.SetActive(false);
    }

    private void Update()
    {
        string controller = GameManager.gm.GetCurrentControllerName();
        if (controller != last_controller)
        {
            if (controller.ToLower().Contains("dualshock") || controller.ToLower().Contains("dualsense"))
            {
                im_attack_icon.sprite = spr_R2;
            }
            else if (controller.ToLower().Contains("xbox") || controller.ToLower().Contains("xinput"))
            {
                im_attack_icon.sprite = spr_RT;
            }
            else if (controller.ToLower().Contains("keyboard"))
            {
                im_attack_icon.sprite = spr_left_click;
            }

            last_controller = controller; /// Evita hacer comprobaciones innecesarias si no se ha cambiado de control
        }
    }


    public Ability GetCurrentAbility()
    {
        return equipped_attacks[current_attack];
    }

    private IEnumerator MoveUIElements()
    {
        moving_ui = true;

        Vector2[] sizes = new Vector2[slots_cooldowns.Count];

        for (int i = 0; i < sizes.Length; i++)
        {
            sizes[i] = slots_cooldowns[i].rectTransform.localScale;
        }

        float timer = 0;
        while (true)
        {
            for (int i = 0; i < slots_cooldowns.Count; i++)
            {
                int target = i + 1;
                if (target >= dir.Length)
                    target = 0;
                slots_cooldowns[i].rectTransform.localScale = Vector2.Lerp(slots_cooldowns[i].rectTransform.localScale, sizes[target], Time.deltaTime);

                slots_cooldowns[i].rectTransform.anchoredPosition += dir[i] * Time.deltaTime * 100;
            }

            timer += Time.deltaTime;
            if (timer > 1)
            {
                break;
            }

            yield return null;
        }

        List<UnityEngine.UI.Image> copy = new List<UnityEngine.UI.Image>(slots_cooldowns);
        for (int i = 0; i < dir.Length; i++)
        {
            int t = i + 1;
            if (t >= dir.Length)
                t = 0;
            slots_cooldowns[i] = copy[t];
        }

        if (GetCurrentAbility().current_cooldown >= GetCurrentAbility().cooldown)
        {
            slots_cooldowns[0].fillAmount = 1;
        }

        moving_ui = false;
    }

    public void UpdateUI()
    {
        abilities_order[0] = equipped_attacks[current_attack];
        for (int i = 0, j = 1; i < equipped_attacks.Count; i++)
        {
            if (abilities_order[0] != equipped_attacks[i])
            {
                abilities_order[j] = equipped_attacks[i];
                j++;
            }
        }

        // Cambia los sprites de los slots según la habilidad que se pueda usar
        for (int i = 0; i <  abilities_order.Length; i++)
        {
            if (abilities_order[i] == null) continue;

            slots_abilities[i].sprite = abilities_order[i].icon;
            Color col = slots_abilities[i].color;
            slots_abilities[i].color = new Color(col.r, col.g, col.b, 1);
            slots_cooldowns[i].color = new Color(col.r, col.g, col.b, 1);
        }

        // Esconde todos los slots que no tengan una habilidad
        for (int i = 0; i < slots_abilities.Count; i++)
        {
            if (slots_abilities[i].sprite == null)
            {
                /// Hace el sprite invisible
                Color col = slots_abilities[i].color;
                slots_abilities[i].color = new Color(col.r, col.g, col.b, 0);
                slots_cooldowns[i].color = new Color(col.r, col.g, col.b, 0);
            }
        }
    }

    public void ChangeAttack()
    {
        if (!equipped_attacks[current_attack].onExecution)
        {
            if (equipped_attacks[current_attack].onCooldown)
                GameManager.gm.ColorPulse(slots_cooldowns[current_attack], Color.red, 10);
            // Cambia de ataque
            current_attack++;
            if (current_attack >= equipped_attacks.Count)
                current_attack = 0;

            if (equipped_attacks.Count > 1 && !moving_ui)
                StartCoroutine(MoveUIElements());
        }
    }



    public void StartCooldowns()
    {
        // Pone el cooldown actual de cada slot
        for (int i = 0; i < slots_cooldowns.Count && i < abilities_order.Length; i++)
        {
            if (abilities_order[i] == null) continue;

            StartCoroutine(AbilityCooldown(slots_cooldowns[i], abilities_order[i]));
        }


        if (equipped_attacks.Count > 1 && !moving_ui)
            StartCoroutine(MoveUIElements());
    }
    private IEnumerator AbilityCooldown(UnityEngine.UI.Image im, Ability ab)
    {
        im.color = new Color(im.color.r, im.color.g, im.color.b, 1);

        ab.onCooldown = true;


        while (ab.current_cooldown < ab.cooldown)
        {
            ab.current_cooldown += Time.deltaTime;

            im.fillAmount = ab.current_cooldown / ab.cooldown;
            yield return null;
        }

        ab.onCooldown = false;
        ab.onExecution = false;

        im.fillAmount = 1;
    }


    public void Attack(InputAction.CallbackContext con)
    {
        if (con.performed && equipped_attacks.Count > 0)
        {
            if (current_attack < equipped_attacks.Count && !equipped_attacks[current_attack].onCooldown)
            {
                equipped_attacks[current_attack].ability_event.Invoke();
            }

            ChangeAttack();
        }
    }

    public void AddAttack(Ability ability)
    {
        if (!im_attack_icon.gameObject.activeSelf)
            im_attack_icon.gameObject.SetActive(true);
        // A partir de 2 habilidades añadidas se cambiará la primera por la nueva
        if (equipped_attacks.Count < 2)
        {
            equipped_attacks.Add(ability);
        }
        else
        {
            equipped_attacks[1] = equipped_attacks[0];
            equipped_attacks[0] = ability;
        }

        UpdateUI();
    }

    public void DisableAnimator()
    {
        anim.enabled = false;
    }
}