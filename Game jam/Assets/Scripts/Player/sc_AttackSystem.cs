using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class AttackSystem : MonoBehaviour
{
    public static AttackSystem instance { get; private set; }
    public List<Ability> equipped_attacks = new List<Ability>();

    private int current_attack = 0;

    public List<UnityEngine.UI.Image> slots_abilities;
    private Ability[] abilities_order;
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
        Debug.Log("Ab 0: " + abilities_order[0].name);
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
    }

    public void ChangeAttack()
    {
        if (!equipped_attacks[current_attack].onExecution)
        {
            // Cambia de ataque
            current_attack++;
            if (current_attack >= equipped_attacks.Count)
                current_attack = 0;

            UpdateUI();
        }
    }

    public void Attack(InputAction.CallbackContext con)
    {
        if (con.performed && equipped_attacks.Count > 0)
        {
            if (current_attack < equipped_attacks.Count)
                equipped_attacks[current_attack].ability_event.Invoke();

            ChangeAttack();
        }
    }
}