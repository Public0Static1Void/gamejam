using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    public int max_hp;
    public float invunerable_time;
    public int hp;
    [Header("References")]
    public Image life_amount;
    private Image bg_life_amount;
    public AudioSource hearth_beat_as;

    private bool damaged;
    private float timer = 0;

    private Material m;
    private Color curr_color;

    // Shake effect
    [Header("Shake effect")]
    public float shakeAmount;

    private CameraRotation cameraRotation;

    [SerializeField] private List<AudioClip> damage_clips;
    void Start()
    {
        if (damage_clips.Count <= 0) Debug.LogWarning("Remember to add the damage sounds to the player!!");

        cameraRotation = Camera.main.GetComponent<CameraRotation>();
        m = GetComponent<MeshRenderer>().material;

        bg_life_amount = life_amount.transform.parent.GetComponent<Image>();

        curr_color = m.color;

        damaged = false;

        hearth_beat_as.loop = true;

        // Carga los datos guardados
        PlayerData pd = GameManager.gm.saveManager.LoadSaveData();

        if (pd != null )
        {
            max_hp = pd.hp;
            ReturnScript.instance.damage = pd.damage;
            ReturnScript.instance.explosion_range = pd.explosion_range;
            PlayerMovement.instance.speed = pd.speed;
            PlayerMovement.instance.max_stamina = pd.stamina;
        }

        hp = max_hp;
    }

    void Update()
    {
        if (damaged)
        {
            timer += Time.deltaTime;
            if (timer >= invunerable_time)
            {
                damaged = false;
                timer = 0;
            }
        }
    }

    public void Invulnerable()
    {
        damaged = true;
    }

    public void Damage(int value)
    {
        if ((ReturnScript.instance.returning && value > 0) || PlayerMovement.instance.slide)
            return;

        bg_life_amount.fillAmount = life_amount.fillAmount;

        hp -= value;
        if (hp > max_hp)
        {
            hp = max_hp;
            return; /// No hará más comprobaciones si no puede ganar más vida
        }

        /// Barra de vida
        life_amount.fillAmount = 1 - (1 - ((float)hp / (float)max_hp));
        StartCoroutine(ChangeBackgroundLife(life_amount.fillAmount));

        /// Sonido de latido, se acelera más cada cuánto menos vida tienes
        if (hp <= max_hp * 0.85f)
        {
            hearth_beat_as.Play();
        }
        else
        {
            hearth_beat_as.Stop();
        }
        hearth_beat_as.pitch = 1 + (1 - ((float)hp / (float)max_hp));

        // Guarda estadísticas de daño
        if (value < 0)
            GameManager.gm.damage_healed -= value;
        else
            GameManager.gm.damage_recieved += value;

        curr_color.r += value * 0.025f;
        curr_color.g -= value * 0.025f;
        curr_color.b -= value * 0.025f;
        m.color = curr_color;
        

        if (!SoundManager.instance.audioSource.isPlaying) /// Le hace un efecto de shake a la cámara y pone el sonido de daño
        {
            cameraRotation.ShakeCamera(1, shakeAmount);
            SoundManager.instance.InstantiateSound(damage_clips[Random.Range(0, damage_clips.Count)], transform.position);
            GameManager.gm.ShakeController(0.5f, 0.1f, 0.25f);
        }

        if (hp <= 0)
        {
            GameManager.gm.EndGame();
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.transform.tag == "Enemy" && !damaged)
        {
            Damage(2);
            damaged = true;
        }
    }

    private IEnumerator ChangeBackgroundLife(float new_value)
    {
        yield return new WaitForSeconds(0.25f);
        while (bg_life_amount.fillAmount > new_value)
        {
            bg_life_amount.fillAmount -= Time.deltaTime * 0.15f;
            yield return null;
        }
    }
}