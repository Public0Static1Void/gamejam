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

    public RectTransform main_canvas;
    public List<Sprite> blood_splashes;

    private List<Image> blood_images = new List<Image>();
    private int curr_blood = 0;

    private bool damaged;
    private float timer = 0;

    private Material m;
    private Color curr_color;

    // Shake effect
    [Header("Shake effect")]
    public float shakeAmount;

    private CameraRotation cameraRotation;

    [SerializeField] private List<AudioClip> damage_clips;


    private bool god_mode = false;

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

        // Genera las imágenes de blood
        for (int i = 0; i < blood_splashes.Count; i++)
        {
            GameObject ob = new GameObject();
            ob.name = "Blood splash " + i.ToString();

            RectTransform rect = ob.AddComponent<RectTransform>();
            Image im = ob.AddComponent<Image>();
            im.sprite = blood_splashes[Random.Range(0, blood_splashes.Count)];

            rect.localScale = Vector3.one * 8;

            ob.transform.SetParent(main_canvas.transform);
            ob.transform.SetAsFirstSibling();

            ob.SetActive(false);

            blood_images.Add(im);
        }
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

        if (Input.GetKeyDown(KeyCode.G))
        {
            god_mode = !god_mode;
            if (god_mode)
            {
                GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "God mode: ON", 2);
            }
            else
            {
                GameManager.gm.ShowText(GameManager.TextPositions.CENTER_LOWER, "God mode: OFF", -1);
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

        if (!god_mode)
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

    public void SplashBloodOnScreen(bool hit_from_right)
    {
        StartCoroutine(SplashBlood(hit_from_right));
    }
    private IEnumerator SplashBlood(bool hit_from_right)
    {
        Image curr_image = blood_images[curr_blood]; // Guarda la referencia a la imagen actual
        
        /// Consigue un tamaño aleatorio
        int rs = Random.Range(3, 8);
        Vector2 rand_size = new Vector2(rs, rs);
        curr_image.rectTransform.localScale = rand_size;

        /// Consigue una posición y rotación aleatorias
        Vector2 rand_pos;
        if (hit_from_right)
        {
            rand_pos = new Vector2(Random.Range(curr_image.rectTransform.rect.width * rs, main_canvas.rect.width / 2),
                                   Random.Range(-main_canvas.rect.height / 2 + curr_image.rectTransform.rect.height, main_canvas.rect.height - curr_image.rectTransform.rect.height * rs));
        }
        else
        {
            rand_pos = new Vector2(Random.Range(-main_canvas.rect.width / 2, -curr_image.rectTransform.rect.width * rs),
                                   Random.Range(-main_canvas.rect.height / 2 + curr_image.rectTransform.rect.height, main_canvas.rect.height - curr_image.rectTransform.rect.height * rs));
        }

        Quaternion rand_rotation = Quaternion.Euler(Random.Range(-45, 45), 0, Random.Range(-45, 45));

        curr_image.rectTransform.anchoredPosition = rand_pos;
        curr_image.rectTransform.rotation = rand_rotation;

        curr_image.gameObject.SetActive(true);

        float alpha = 1;
        Color col = curr_image.color;

        while (curr_image.color.a > 0)
        {
            alpha -= Time.deltaTime;

            curr_image.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }

        curr_image.gameObject.SetActive(false);
        curr_image.color = col;

        curr_blood++;
        if (curr_blood >= blood_images.Count)
            curr_blood = 0;
    }



    private void OnTriggerEnter(Collider coll)
    {
        if (coll.transform.tag == "Enemy" && !damaged)
        {
            Vector3 dir = (transform.position - coll.transform.position).normalized;
            Debug.Log(!(Vector3.Dot(transform.right, dir) > 0));
            SplashBloodOnScreen(!(Vector3.Dot(transform.right, dir) > 0));
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