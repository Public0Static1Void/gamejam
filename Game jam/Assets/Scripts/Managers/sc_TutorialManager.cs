using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance { get; private set; }

    public bool onTutorial;

    [Header("References")]
    public TMP_Text init_text;
    public UnityEngine.UI.Image im_start_bg;
    public GameObject ob_main_ui;
    public AudioMixerGroup audioMixerGroup;
    public AudioClip clip_slider_change;

    [Header("Settings references")]
    public GameObject ob_audio_menu;
    public UnityEngine.UI.Slider sl_master_volume;
    public TMP_Text txt_master_value;
    public UnityEngine.UI.Slider sl_sfx_volume;
    public TMP_Text txt_sfx_value;

    [Header("Player bars")]
    public UnityEngine.UI.Image im_hp_bar;
    public UnityEngine.UI.Image im_stamina_bar;

    private bool showing_text = false;
    public bool showing_settings = true;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        /*
        string started_file_path = Path.Combine(Application.persistentDataPath, "started.txt");
        if (File.Exists(started_file_path))
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            File.Create(started_file_path);
        }
        */
        SetPlayerMovement(false);
        SetPlayerRotation(Quaternion.identity);

        ob_main_ui.SetActive(false);

        StartCoroutine(StartAnimation());
    }


    #region Player
    public void SetPlayerMovement(bool value)
    {
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.canMove = value;
        }
    }

    public void SetPlayerRotation(Quaternion rotation)
    {
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.transform.rotation = rotation;
        }
    }
    #endregion

    #region GameObjects
    public void TranslateObjectUp(Transform ob)
    {
        StartCoroutine(TranslateObjectRoutine(ob, Vector3.up, 2));
    }
    public void TranslateObjectDown(Transform ob)
    {
        StartCoroutine(TranslateObjectRoutine(ob, -Vector3.up, 2));
    }

    private IEnumerator TranslateObjectRoutine(Transform ob, Vector3 dir, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            ob.Translate(dir * Time.deltaTime * 2);
            timer += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region UI
    public void HideImage(UnityEngine.UI.Image image_to_hide)
    {
        StartCoroutine(HideImageCoroutine(image_to_hide));
    }
    public void ShowImage(UnityEngine.UI.Image image_to_show)
    {
        StartCoroutine(ShowImageCoroutine(image_to_show));
    }
    public IEnumerator HideImageCoroutine(UnityEngine.UI.Image image_to_hide, float hide_speed = 2)
    {
        Color col = image_to_hide.color;
        while (image_to_hide.color.a > 0)
        {
            image_to_hide.color = new Color(col.r, col.g, col.b, image_to_hide.color.a - Time.fixedDeltaTime * hide_speed);
            yield return null;
        }

        image_to_hide.gameObject.SetActive(false);
    }
    public IEnumerator ShowImageCoroutine(UnityEngine.UI.Image image_to_hide, float hide_speed = 2)
    {
        image_to_hide.gameObject.SetActive(true);

        Color col = image_to_hide.color;
        while (image_to_hide.color.a < 1)
        {
            image_to_hide.color = new Color(col.r, col.g, col.b, image_to_hide.color.a + Time.fixedDeltaTime * hide_speed);
            yield return null;
        }
    }

    // Anima las barras de vida y stamina del jugador al empezar
    public void AnimatePlayerBarsOnStart()
    {
        StartCoroutine(AnimatePlayerBarsOnStartCoroutine());
    }
    private IEnumerator AnimatePlayerBarsOnStartCoroutine()
    {
        while (im_hp_bar.fillAmount < 1 || im_stamina_bar.fillAmount < 1)
        {
            im_stamina_bar.fillAmount += Time.deltaTime * 0.4f;
            im_hp_bar.fillAmount += Time.deltaTime * 0.5f;
            yield return null;
        }
    }
    #endregion


    #region Settings
    public void SetMasterVolume()
    {
        audioMixerGroup.audioMixer.SetFloat("MasterVolume", sl_master_volume.value);
        txt_master_value.text = GameManager.GetPercentage(sl_master_volume.value, -80, 20).ToString("F0");
        SoundManager.instance.InstantiateSound(clip_slider_change, transform.position, 0.25f);
    }
    public void SetSFXvolume()
    {
        audioMixerGroup.audioMixer.SetFloat("SFXVolume", sl_sfx_volume.value);
        txt_sfx_value.text = GameManager.GetPercentage(sl_sfx_volume.value, -80, 20).ToString("F0");
        SoundManager.instance.InstantiateSound(clip_slider_change, transform.position, 0.25f);
    }
    #endregion

    private IEnumerator ShowText(string text, float wait_time = 0.01f)
    {
        showing_text = true;

        int curr_char = 0;

        float timer = 0;

        string curr_text = init_text.text;

        while (curr_char < text.Length)
        {
            if (text[curr_char] == '<')
            {
                if (text[curr_char + 1] == 'w')
                {
                    wait_time = float.Parse("" + text[curr_char + 3]);
                    curr_char = curr_char + 5;
                }
                else
                {
                    int tag_end = text.IndexOf('>', curr_char);
                    if (tag_end != -1)
                    {
                        curr_text += text.Substring(curr_char, tag_end - curr_char + 1);
                        curr_char = tag_end + 1;
                    }
                }

            }
            


            timer += Time.unscaledDeltaTime;
            if (timer > wait_time)
            {
                curr_text += text[curr_char];
                init_text.text = curr_text;
                wait_time = 0.01f;
                curr_char++;
                timer = 0;
            }
            yield return null;
        }

        showing_text = false;
    }

    private IEnumerator StartAnimation()
    {
        Time.timeScale = 0; // Detiene todo el movimiento
        PlayerMovement.instance.rb.isKinematic = true;

        string text = "Starting program.<w=1>.<w=1>.";
        StartCoroutine(ShowText(text));

        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        init_text.text += '\n'; /// Añade un salto de línea

        text = "Program started.";
        StartCoroutine(ShowText(text));

        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        init_text.text += '\n';
        yield return new WaitForSecondsRealtime(0.5f);


        text = "Initing player configuration...";
        StartCoroutine(ShowText(text));
        /// Espera a que termine la animación del texto
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.75f);
        init_text.text += '\n';


        text = "<color=red>[ERROR]</color> <w=1> Couldn't find a saved configuration.";
        StartCoroutine(ShowText(text));

        while (showing_text) 
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.1f);
        init_text.text += '\n';


        text = "Initing manual configuration.";
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        init_text.text += '\n';

        text = "<color=blue>[AUDIO] Set your preffered volume:";
        StartCoroutine(ShowText(text));


        // Set audio settings -----------------------------------------------------------------
        ob_audio_menu.SetActive(true);
        sl_master_volume.Select();

        while (showing_settings) /// Espera a que termine de poner las opciones
        {
            yield return null;
        }

        init_text.text += " <color=green>READY</color>\n";
    }

    public void SetOpenSettingsValue(bool value)
    {
        showing_settings = value;
    }
}