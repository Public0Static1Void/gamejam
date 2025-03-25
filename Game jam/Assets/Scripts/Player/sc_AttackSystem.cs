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

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public Ability GetCurrentAbility()
    {
        return equipped_attacks[current_attack];
    }

    public void UpdateUI()
    {
        // Cambia los sprites de los slots según la habilidad que se pueda usar
        for (int i = current_attack, j = 0; i < slots_abilities.Count && i < equipped_attacks.Count; i++)
        {
            /// Cambia el sprite y le quita la transparencia
            slots_abilities[i].sprite = equipped_attacks[i].icon;
            Color col = slots_abilities[i].color;
            slots_abilities[i].color = new Color(col.r, col.g, col.b, 1);

            if (j < current_attack && j < equipped_attacks.Count)
            {
                slots_abilities[j].sprite = equipped_attacks[j].icon;
                col = slots_abilities[j].color;
                slots_abilities[j].color = new Color(col.r, col.g, col.b, 1);
                j++;
            }
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

            Debug.Log(equipped_attacks[current_attack].name);

            UpdateUI();
        }
    }

    public void Attack(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            equipped_attacks[current_attack].ability_event.Invoke();

            ChangeAttack();
        }
    }
}