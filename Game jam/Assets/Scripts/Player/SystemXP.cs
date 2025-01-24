using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability
{
    public string name;
    public string description;
    public Sprite icon;
    public System.Action effect;
}

public class Player : MonoBehaviour
{
    public float period = 5f;
    public int damage = 10;

    public void ImproveSpeed()
    {
        period -= 0.5f;
        Debug.Log("Speed improved to: " + period);
    }

    public void IncreaseDamage()
    {
        damage += 5;
        Debug.Log("Damage increased to: " + damage);
    }
}

public class SystemXP : MonoBehaviour
{
    public Ability[] allAbilities;
    public GameObject abilityPanel;
    public Button[] abilityButtons;
    public Image[] abilityIcons;
    public Text[] abilityDescriptions;

    private Ability[] selectedAbilities;

    void Start()
    {
        abilityPanel.SetActive(false);
    }

    public void ShowAbilities()
    {
        selectedAbilities = new Ability[3];
        for (int i = 0; i < 3; i++)
        {
            selectedAbilities[i] = allAbilities[Random.Range(0, allAbilities.Length)];
        }

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            abilityIcons[i].sprite = selectedAbilities[i].icon;
            abilityDescriptions[i].text = selectedAbilities[i].description;

            int index = i;
            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => SelectAbility(index));
        }

        abilityPanel.SetActive(true);
    }

    public void SelectAbility(int index)
    {
        selectedAbilities[index].effect?.Invoke();
        abilityPanel.SetActive(false);
    }

    void ConfigureAbilities()
    {
        allAbilities[0] = new Ability
        {
            name = "Time Period",
            description = "Reduces teleportation cooldown time.",
            icon = speedIcon,
            effect = player.ImproveSpeed
        };

        allAbilities[1] = new Ability
        {
            name = "Damage",
            description = "Increases your attack damage.",
            icon = damageIcon,
            effect = player.IncreaseDamage
        };

        allAbilities[2] = new Ability
        {
            name = "Explosion Radius",
            description = "Increases your explosion range.",
            icon = damageIcon,
            effect = player.IncreaseDamage
        };
    }
}