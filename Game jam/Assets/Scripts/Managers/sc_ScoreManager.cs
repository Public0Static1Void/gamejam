using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }
    public float score;
    public float score_multiplier = 1;

    public AudioClip score_clip;

    [Header ("References")]
    public GameObject text_to_show;
    public TMP_Text ui_score_text;
    public TMP_Text ui_plus_scoretext;
    public Image ui_multiplier_timer;
    public TMP_Text ui_multiplier_text;

    public Transform ui_canvas;
    public AudioSource score_audioSource;

    public List<TMP_Text> plus_scoretext_list;
    private int current_text;

    [Header("Colors")]
    public Color color_base_score;
    public Color color_got_score;

    private bool spawning_number = false;

    [HideInInspector]
    public bool can_buy_door = false;
    [HideInInspector]
    public float door_cost;
    [HideInInspector]
    public Transform current_door;


    private float multiplier_timer = 0, timer = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
        plus_scoretext_list = new List<TMP_Text>();
        for (int i = 0; i < 50; i++)
        {
            plus_scoretext_list.Add(Instantiate(ui_plus_scoretext));
            plus_scoretext_list[i].transform.SetParent(ui_canvas);
            plus_scoretext_list[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        ui_multiplier_text.text = "x" + score_multiplier.ToString("F2");


        if (score_multiplier > 1)
        {
            timer += Time.deltaTime;
            multiplier_timer += Time.deltaTime * (timer * 0.25f);
            ui_multiplier_timer.fillAmount = 1 - (multiplier_timer / 10);
            if (multiplier_timer > 10)
            {
                score_multiplier /= 2;
                multiplier_timer = 0;
                ui_multiplier_timer.fillAmount = 1;


                if (score_multiplier < 1)
                {
                    timer = 0;
                    score_multiplier = 1;
                }
            }
        }
    }

    public void AddMultiplier(float value)
    {
        score_multiplier += value;

        if (score_multiplier > 20)
            score_multiplier = 20;

        timer = 0;
        multiplier_timer = 0;
        ui_multiplier_timer.fillAmount = 1;
    }
    /// <summary>
    /// Suma la cantidad indicada a score y hace un sonido. Si se marca true, show_text mostrará en texto la cantidad sumada
    /// </summary>
    public void ChangeScore(float value, Vector3 sound_position, bool show_text)
    {
        if (value > 0)
        {
            value *= score_multiplier;
            value = (int)value;
        }
            

        score += value;
        if (score > 9999999999)
        {
            score = 9999999999;
            value = 0;
        }
        else if (score < 0)
        {
            score = 0;
        }

        if (score_clip != null && value > 0)
        {
            score_audioSource.clip = score_clip;
            score_audioSource.Play();
        }

        string string_value = value.ToString();
        char sum_or_not = string_value[0] == '-' ? ' ' : '+';
        if (show_text) /// Si es true mostrará un texto de score en la posición del sonido
        {
            Vector3 dir_to_player = sound_position - PlayerMovement.instance.transform.position; /// Calcula la dirección entre el origen del sonido al player
            StartCoroutine(ShowTextOnPosition(sum_or_not + string_value, sound_position, dir_to_player.normalized, 0));
        }


        // Se cambia el número del score poniéndolo en un formato de 0000000000
        int number_length = (int)Mathf.Floor(Mathf.Log10(score)) + 1; /// Consigue los dígitos del número
        string number_to_show = "";
        for (int i = 0; i < 10 - number_length; i++)
        {
            number_to_show += "0";
        }
        number_to_show += score.ToString();
        ui_score_text.text = number_to_show;

        // Muestra un texto de suma en la ui
        plus_scoretext_list[current_text].text = sum_or_not + string_value;
        StartCoroutine(MovePlusScoreText(plus_scoretext_list[current_text]));
        if (current_text < plus_scoretext_list.Count - 1)
            current_text++;
        else
            current_text = 0;
    }

    public IEnumerator ShowTextOnPosition(string phrase, Vector3 position, Vector3 face_direction, float duration)
    {
        float timer = 0;
        Quaternion look_rotation = Quaternion.LookRotation(face_direction, Vector3.up);
        Text text = Instantiate(text_to_show, position, look_rotation).transform.GetChild(0).GetComponent<Text>(); /// Instancia el texto en la posición del sonido y mirando al player
        text.text = phrase;

        Color col = text.color;
        text.color = new Color(col.r, col.g, col.b, 0);
        while (text.color.a < 1) /// Hace un fade in del texto
        {
            timer += Time.deltaTime;
            text.color = new Color(col.r, col.g, col.b, timer * 1.5f);
            yield return null;
        }

        yield return new WaitForSeconds(duration); /// Espera para empezar a desaparecer el texto

        while (text.color.a > 0) /// Hace un fade out del texto
        {
            timer -= Time.deltaTime;
            text.color = new Color(col.r, col.g, col.b, timer * 1.5f);
            yield return null;
        }

        Destroy(text.transform.parent.gameObject);
    }
    private IEnumerator MovePlusScoreText(TMP_Text text)
    {
        while (spawning_number) /// Espera un poco para que no se solapen los números
        {
            yield return null;
        }
        spawning_number = true;

        Vector2 rand_dir = new Vector2(Random.Range(-1, 0), Random.Range(-0.5f, 0.76f)); /// Consigue una dirección random
        text.rectTransform.anchoredPosition = ui_score_text.rectTransform.anchoredPosition; /// Cambia la posición del text
        text.rectTransform.position = new Vector2(ui_score_text.rectTransform.position.x + 100, ui_score_text.rectTransform.position.y + 90);
        text.rectTransform.rotation = ui_score_text.rectTransform.rotation;
        
        text.gameObject.SetActive(true);

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        Color col = text.color;

        float timer = 0, alpha = col.a;

        StartCoroutine(ChangeScoreTextColor()); /// Cambia el color del texto de score

        while (alpha > 0) /// Moverá el texto en una dirección aleatoria entre arriba e izquierda mientras hace un fade out
        {
            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                spawning_number = false;
            }
            text.rectTransform.Translate(rand_dir * Time.deltaTime * 250);
            alpha -= Time.deltaTime;
            text.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }

        text.gameObject.SetActive(false);
    }

    private IEnumerator ChangeScoreTextColor()
    {
        float timer = 0;
        while (timer < 0.1f)
        {
            ui_score_text.color = Color.Lerp(ui_score_text.color, color_got_score, Time.deltaTime * 10);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < 1)
        {
            ui_score_text.color = Color.Lerp(ui_score_text.color, color_base_score, Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        ui_score_text.color = color_base_score;
    }

    public void BuyDoor(InputAction.CallbackContext con)
    {
        if (con.performed && can_buy_door)
        {
            if (score >= door_cost)
            {
                ChangeScore(-door_cost, transform.position, true);
                // Desbloquea los spawners asignados a la puerta
                Rounds.instance.SpawnPoint.AddRange(current_door.GetComponent<sc_Door>().Spawner);

                StartCoroutine(OpenDoor(current_door.gameObject));
            }
            
        }
    }
    private IEnumerator OpenDoor(GameObject door)
    {
        GameObject ob = door;

        ob.GetComponent<Collider>().enabled = false;

        Material[] m_ob = ob.GetComponent<MeshRenderer>().materials;
        Color m_color = m_ob[0].color;

        float alpha = m_ob[0].color.a;

        while (m_ob[0].color.a > 0)
        {
            ob.transform.Translate(Vector3.up * 2 * Time.deltaTime);

            alpha -= Time.deltaTime;
            m_ob[0].color = new Color(m_color.r, m_color.g, m_color.b, alpha);

            yield return null;
        }

        Destroy(ob);
    }
}