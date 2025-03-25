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

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public void Attack(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            equipped_attacks[current_attack].ability_event.Invoke();

            current_attack++;
            if (current_attack >= equipped_attacks.Count - 1)
                current_attack = 0;
        }
    }
}