using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }
    public float score;

    public AudioClip score_clip;

    [Header ("References")]
    public GameObject text_to_show;
    public TMP_Text ui_score_text;
    public TMP_Text ui_plus_scoretext;
    public Transform ui_canvas;

    public List<TMP_Text> plus_scoretext_list;
    private int current_text;

    private bool spawning_number = false;

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

    public void ChangeScore(float value, Vector3 sound_position, bool show_text)
    {
        if (score < 9999999999)
            score += value;
        else
            value = 0;
        if (score_clip != null) /// Instancia el sonido de score en la escena
            SoundManager.instance.InstantiateSound(score_clip, sound_position, 1);

        string string_value = value.ToString();
        if (show_text) /// Si es true mostrará un texto de score en la posición del sonido
        {
            Vector3 dir_to_player = sound_position - PlayerMovement.instance.transform.position; /// Calcula la dirección entre el origen del sonido al player
            StartCoroutine(ShowScoreTextOnPosition("+" + string_value, sound_position, dir_to_player.normalized));
        }


        // Se cambia el número del score poniéndolo en un formato de 0000000000
        int number_length = (int)Mathf.Floor(Mathf.Log10(score)) + 1; /// Consigue los dígitos del número
        string number_to_show = "";
        for (int i = 0; i < 9 - number_length; i++)
        {
            number_to_show += "0";
        }
        number_to_show += score.ToString();
        ui_score_text.text = number_to_show;

        // Muestra un texto de suma en la ui
        plus_scoretext_list[current_text].text = "+" + string_value;
        StartCoroutine(MovePlusScoreText(plus_scoretext_list[current_text]));
        if (current_text < plus_scoretext_list.Count)
            current_text++;
        else
            current_text = 0;
    }

    private IEnumerator ShowScoreTextOnPosition(string phrase, Vector3 position, Vector3 face_direction)
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
        text.rectTransform.position = new Vector2(ui_score_text.rectTransform.position.x - 10, ui_score_text.rectTransform.position.y + 90);
        text.rectTransform.rotation = ui_score_text.rectTransform.rotation;
        
        text.gameObject.SetActive(true);

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        Color transparent = new Color(text.color.r, text.color.g, text.color.b, 0);

        float timer = 0;

        while (text.color.a > 0) /// Moverá el texto en una dirección aleatoria entre arriba e izquierda mientras hace un fade out
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                spawning_number = false;
            }
            text.rectTransform.Translate(rand_dir * Time.deltaTime * 250);
            text.color = Color.Lerp(text.color, transparent, Time.deltaTime * 1.5f);

            yield return null;
        }

        text.gameObject.SetActive(false);
    }
}