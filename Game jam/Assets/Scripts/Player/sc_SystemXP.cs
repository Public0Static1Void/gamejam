using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Ability
{
    public string name;
    public string description;
    public Sprite icon;
    public UnityEvent ability_event;
    public System.Action effect;
    public AbilitiesSystem.Abilities type;
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
    public static SystemXP instance { get; private set; }

    public Ability[] allAbilities;
    public GameObject abilityPanel;
    public Button[] abilityButtons;
    public Image[] abilityIcons;
    public Text[] abilityDescriptions;

    private Ability[] selectedAbilities;

    [Header("References")]
    public Sprite periodIcon, damageIcon, radiusIcon;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        abilityPanel.SetActive(false);
        //ConfigureAbilities();
    }

    public void ShowAbilities()
    {
        selectedAbilities = new Ability[3];
        for (int i = 0; i < 3; i++)
        {
            selectedAbilities[i] = allAbilities[Random.Range(0, allAbilities.Length - 1)];
        }

        for (int i = 0; i < abilityButtons.Length - 1; i++)
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
        allAbilities = new Ability[3];

        allAbilities[0] = new Ability
        {
            name = "Time Period",
            description = "Reduces teleportation cooldown time.",
            icon = periodIcon,
            effect = EkkoUlt.instance.DecreasePeriod
        };

        allAbilities[1] = new Ability
        {
            name = "Damage",
            description = "Increases your attack damage.",
            icon = damageIcon,
            effect = EkkoUlt.instance.IncreaseDamage
        };

        allAbilities[2] = new Ability
        {
            name = "Explosion Radius",
            description = "Increases your explosion range.",
            icon = radiusIcon,
            effect = EkkoUlt.instance.IncreaseExplosionRange
        };
    }
}