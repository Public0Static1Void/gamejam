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


    private bool moving_ui = false;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
        abilities_order = new Ability[slots_abilities.Count];
    }


    public Ability GetCurrentAbility()
    {
        return equipped_attacks[current_attack];
    }

    private IEnumerator MoveUIElements()
    {
        moving_ui = true;

        List<Vector2> start_positons = new List<Vector2>();
        for (int i = 0; i < slots_abilities.Count; i++)
        {
            start_positons.Add(slots_abilities[i].rectTransform.anchoredPosition);
        }
        List<Vector2> start_sizes = new List<Vector2>();
        for (int i = 0; i < slots_abilities.Count; i++)
        {
            start_sizes.Add(slots_abilities[i].rectTransform.localScale);
        }

        while (true)
        {
            for (int i = 0; i < abilities_order.Length; i++)
            {
                int new_pos = i + 1;
                if (new_pos >= abilities_order.Length)
                {
                    new_pos = 0;
                }

                // Cambia la posición lentamente
                slots_abilities[i].rectTransform.anchoredPosition = Vector2.Lerp(slots_abilities[i].rectTransform.anchoredPosition,
                                                                                 start_positons[new_pos],
                                                                                 Time.deltaTime);
                // Cambia el tamaño
                slots_abilities[i].rectTransform.localScale = Vector2.Lerp(slots_abilities[i].rectTransform.localScale,
                                                                           start_sizes[new_pos],
                                                                           Time.deltaTime);
            }

            Debug.Log(Vector2.Distance(start_positons[0], slots_abilities[slots_abilities.Count - 1].rectTransform.anchoredPosition));
            if (Vector2.Distance(start_positons[0], slots_abilities[slots_abilities.Count - 1].rectTransform.anchoredPosition) < 0.05f)
            {
                break;
            }

            yield return null;
        }

        // Vuelve a ordenar la lista
        List<UnityEngine.UI.Image> backup_list = new List<UnityEngine.UI.Image>(slots_abilities);
        List<UnityEngine.UI.Image> backup_cooldowns = new List<UnityEngine.UI.Image>(slots_cooldowns);
        for (int i = 0; i < backup_list.Count; i++)
        {
            int new_i = i + 1;
            if (new_i >= backup_list.Count)
                new_i = 0;
            slots_abilities[i] = backup_list[new_i];
            slots_cooldowns[i] = backup_cooldowns[new_i];
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
        }

        // Esconde todos los slots que no tengan una habilidad
        for (int i = 0; i < slots_abilities.Count; i++)
        {
            if (slots_abilities[i].sprite == null)
            {
                /// Hace el sprite invisible
                Color col = slots_abilities[i].color;
                slots_abilities[i].color = new Color(col.r, col.g, col.b, 0);
            }
        }

        if (equipped_attacks.Count > 1 && !moving_ui)
            StartCoroutine(MoveUIElements());
    }

    public void ChangeAttack()
    {
        // La habilidad no está ejecutándose
        if (!equipped_attacks[current_attack].onExecution)
        {
            // Cambia de ataque
            current_attack++;
            if (current_attack >= equipped_attacks.Count)
                current_attack = 0;

            UpdateUI();
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
}