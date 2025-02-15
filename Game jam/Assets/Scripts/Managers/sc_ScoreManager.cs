using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }
    public float score;

    public AudioClip score_clip;

    public GameObject text_to_show;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public void ChangeScore(float value, Vector3 sound_position, bool show_text)
    {
        score += value;
        if (score_clip != null) /// Instancia el sonido de score en la escena
            SoundManager.instance.InstantiateSound(score_clip, sound_position, 1);
        if (show_text) /// Si es true mostrará un texto de score en la posición del sonido
        {
            Vector3 dir_to_player = sound_position - PlayerMovement.instance.transform.position; /// Calcula la dirección entre el origen del sonido al player
            StartCoroutine(ShowScoreTextOnPosition("+" + value.ToString(), sound_position, dir_to_player.normalized));
        }
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
}