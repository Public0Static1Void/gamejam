using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilitiesSystem : MonoBehaviour
{
    public static AbilitiesSystem instance {  get; private set; }
    protected List<Ability> abilities = new List<Ability>();
    public enum Abilities { LEVITATE, EXPLODE_PATH, LAST_NO_USE }

    [Header("References")]
    public GameObject ob_gamblingparent;

    public GameObject ability1_slot;
    private UnityEngine.UI.Image ability1_sprite;
    private UnityEngine.UI.Text ability1_text;

    public GameObject ability2_slot;
    private UnityEngine.UI.Image ability2_sprite;
    private UnityEngine.UI.Text ability2_text;

    public GameObject ability3_slot;
    private UnityEngine.UI.Image ability3_sprite;
    private UnityEngine.UI.Text ability3_text;

    [Header("Abilities Methods")]
    public List<UnityEvent> methods_abilities;

    [Header("Sprites")]
    public Sprite sprite_levitate;
    public Sprite sprite_explodepath;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        Ability ab = new Ability();

        ab.name = "Levitate";
        ab.description = ":)";
        ab.ability_event = methods_abilities[(int)Abilities.LEVITATE];

        abilities.Add(ab);

        ab.name = "Exploding path";
        ab.description = "Boom boom on path";
        ab.ability_event = methods_abilities[(int)Abilities.EXPLODE_PATH];

        abilities.Add(ab);
    }

    public void GetRandomAbilities()
    {
        Ability[] abilities_to_show = new Ability[3];

        abilities_to_show[0] = abilities[Random.Range(0, abilities.Count - 1)];
        abilities_to_show[1] = abilities[Random.Range(0, abilities.Count - 1)];
        abilities_to_show[2] = abilities[Random.Range(0, abilities.Count - 1)];

        ability1_sprite.sprite = abilities_to_show[0].icon;
        ability2_sprite.sprite = abilities_to_show[1].icon;
        ability3_sprite.sprite = abilities_to_show[2].icon;

        ability1_text.text = abilities_to_show[0].name;
        ability2_text.text = abilities_to_show[1].name;
        ability3_text.text = abilities_to_show[2].name;

        ob_gamblingparent.SetActive(true);
    }

    public void CloseGamblingMenu()
    {
        ob_gamblingparent.SetActive(false);
    }
}