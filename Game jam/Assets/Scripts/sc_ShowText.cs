using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class sc_ShowText : MonoBehaviour
{
    public UnityEngine.UI.Image im_text_bg;
    public TMP_Text txt_phrase;
    public float text_speed = 0.025f;
    [TextArea(5, 10)]
    public string text_to_show;

    private bool showing_text = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!showing_text)
                StartCoroutine(ShowText());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            im_text_bg.gameObject.SetActive(false);
            showing_text = false;
        }
    }

    private IEnumerator ShowText()
    {
        showing_text = true;
        im_text_bg.color = new Color(im_text_bg.color.r, im_text_bg.color.g, im_text_bg.color.b, 0);
        txt_phrase.color = new Color(txt_phrase.color.r, txt_phrase.color.g, txt_phrase.color.b, 0);
        im_text_bg.gameObject.SetActive(true);

        txt_phrase.text = "";

        float timer = 0;
        int curr_char = 0;

        string curr_text = "";

        txt_phrase.text = text_to_show;
        im_text_bg.rectTransform.sizeDelta = new Vector2(im_text_bg.rectTransform.sizeDelta.x, txt_phrase.preferredHeight * 2.5f > 600 ? 600 : txt_phrase.preferredHeight * 2.5f);
        txt_phrase.text = "";

        while (txt_phrase.text.Length < text_to_show.Length && curr_char < text_to_show.Length)
        {
            if (text_to_show[curr_char] == '<')
            {
                int tag_end = text_to_show.IndexOf('>', curr_char);
                if (tag_end != -1)
                {
                    curr_text += text_to_show.Substring(curr_char, tag_end - curr_char + 1);
                    curr_char = tag_end + 1;
                }
            }

            timer += Time.deltaTime;
            if (timer >= text_speed && curr_char < text_to_show.Length)
            {
                curr_text += text_to_show[curr_char];
                txt_phrase.text = curr_text;
                curr_char++;
                timer = 0;
            }

            if (im_text_bg.color.a < 0.75f)
            {
                Color col = im_text_bg.color;
                im_text_bg.color = new Color(col.r, col.g, col.b, im_text_bg.color.a + Time.deltaTime);
                
            }
            if (txt_phrase.color.a < 1)
            {
                Color col = txt_phrase.color;
                txt_phrase.color = new Color(col.r, col.g, col.b, txt_phrase.color.a + Time.deltaTime);
            }

            yield return null;
        }
    }
}