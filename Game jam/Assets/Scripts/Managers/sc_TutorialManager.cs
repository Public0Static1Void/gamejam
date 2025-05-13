using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
    public AudioClip clip_type;

    [Header("Settings references")]
    public GameObject ob_audio_menu;
    public UnityEngine.UI.Slider sl_master_volume;
    public TMP_Text txt_master_value;
    public UnityEngine.UI.Slider sl_sfx_volume;
    public TMP_Text txt_sfx_value;

    public GameObject ob_idiom_menu;

    public GameObject ob_screen_menu;
    public GameObject ob_screen_resolutions_content;
    public TMP_Dropdown drop_resolutions;

    [Header("Player bars")]
    public UnityEngine.UI.Image im_hp_bar;
    public UnityEngine.UI.Image im_stamina_bar;

    private bool showing_text = false;
    public bool showing_settings = true;

    private string started_file_path = "";

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        started_file_path = Path.Combine(Application.persistentDataPath, "started.txt");

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

    public void HideText(TMP_Text text_to_hide)
    {
        StartCoroutine(HideTextCoroutine(text_to_hide));
    }
    public void ShowText(TMP_Text text_to_show)
    {
        StartCoroutine(ShowTextCoroutine(text_to_show));
    }
    public IEnumerator HideTextCoroutine(TMP_Text text_to_hide, float hide_speed = 2)
    {
        Color col = text_to_hide.color;
        while (text_to_hide.color.a > 0)
        {
            text_to_hide.color = new Color(col.r, col.g, col.b, text_to_hide.color.a - Time.fixedDeltaTime * hide_speed);
            yield return null;
        }

        text_to_hide.gameObject.SetActive(false);
    }
    public IEnumerator ShowTextCoroutine(TMP_Text text_to_hide, float hide_speed = 2)
    {
        text_to_hide.gameObject.SetActive(true);

        Color col = text_to_hide.color;
        while (text_to_hide.color.a < 1)
        {
            text_to_hide.color = new Color(col.r, col.g, col.b, text_to_hide.color.a + Time.fixedDeltaTime * hide_speed);
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

    public void SetSelectedResolution()
    {
        string input = drop_resolutions.options[drop_resolutions.value].text;
        Match match = Regex.Match(input, @"(\d+)\s*x\s*(\d+)");

        if (match.Success)
        {
            int width = int.Parse(match.Groups[1].Value);
            int height = int.Parse(match.Groups[2].Value);

            Screen.SetResolution(width, height, Screen.fullScreen);
        }

    }
    public void ChangeFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if (!Screen.fullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
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
                    int finish_pos = curr_char;
                    while (finish_pos < text.Length && text[finish_pos] != '>')
                    {
                        finish_pos++;
                    }

                    string num = "";
                    for (int i = curr_char; i < finish_pos; i++)
                    {
                        if (int.TryParse("" + text[i], out int integer))
                        {
                            num += integer.ToString();
                        }
                        else if (text[i] == ',')
                        {
                            num += ',';
                        }
                    }
                    if (num != "")
                    {
                        wait_time = float.Parse(num);
                        curr_char = finish_pos + 1;
                    }
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
            if (timer > wait_time && curr_char < text.Length)
            {
                SoundManager.instance.InstantiateSound(clip_type, transform.position, 0.1f);
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
        yield return new WaitForSecondsRealtime(0.25f);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        Time.timeScale = 0; // Detiene todo el movimiento
        PlayerMovement.instance.rb.isKinematic = true;

        string text = IdiomManager.instance.GetKeyText("Intro start program");
        StartCoroutine(ShowText(text));

        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.75f);
        init_text.text += '\n';

        init_text.text = init_text.text.Substring(0, init_text.text.Length - "...".Length);
        init_text.text += IdiomManager.instance.GetKeyText("Intro started");
        SoundManager.instance.InstantiateSound(clip_type, transform.position);

        yield return new WaitForSecondsRealtime(0.75f);
        init_text.text += '\n';

        text = IdiomManager.instance.GetKeyText("Intro initing configuration");
        StartCoroutine(ShowText(text));
        /// Espera a que termine la animación del texto
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.75f);
        init_text.text += "\n\n";


        text = IdiomManager.instance.GetKeyText("Intro configuration error");
        StartCoroutine(ShowText(text));

        while (showing_text) 
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.1f);
        init_text.text += '\n';

        text = IdiomManager.instance.GetKeyText("Intro save new configuration");
        StartCoroutine(ShowText(text));
        while (showing_text)
        {
            yield return null;
        }
        init_text.text += '\n';
        yield return new WaitForSecondsRealtime(0.15f);

        text = IdiomManager.instance.GetKeyText("Intro initing manual configuration");
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        init_text.text += "\n  ";

        // Set audio settings -----------------------------------------------------------------

        text = IdiomManager.instance.GetKeyText("Intro audio error");
        yield return new WaitForSecondsRealtime(1f);
        StartCoroutine(ShowText(text));

        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1);

        ob_audio_menu.SetActive(true);
        sl_master_volume.Select();

        while (showing_settings) /// Espera a que termine de poner las opciones
        {
            yield return null;
        }

        init_text.text = init_text.text.Substring(0, init_text.text.Length - "ERROR".Length);
        init_text.text += '\t';
        init_text.text += IdiomManager.instance.GetKeyText("Intro ready");

        // Set idiom settings ------------------------------------------------------------------

        init_text.text += "\n  ";

        text = IdiomManager.instance.GetKeyText("Intro idiom error");
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1);

        ob_idiom_menu.SetActive(true);
        ob_idiom_menu.GetComponentInChildren<UnityEngine.UI.Button>().Select();

        showing_settings = true;
        while (showing_settings) /// Espera a que termine de poner las opciones
        {
            yield return null;
        }
        init_text.text = init_text.text.Substring(0, init_text.text.Length - "ERROR".Length);
        init_text.text += '\t';
        init_text.text += IdiomManager.instance.GetKeyText("Intro ready");

        // Set screen settings ------------------------------------------------------------------

        init_text.text += "\n  ";

        Resolution[] resolutions = Screen.resolutions;
        GameObject resolutions_content = ob_screen_resolutions_content;

        /*
        /// Crea los botones de las resoluciones
        for (int i = 0; i < resolutions.Length; i++)
        {
            GameObject toggle = new GameObject($"Resolution {i}");
            toggle.transform.SetParent(resolutions_content.transform, false);
            toggle.AddComponent<UnityEngine.UI.Toggle>();

            GameObject resolution = new GameObject($"Resolution {i}");

            resolution.transform.SetParent(toggle.transform, false);

            TMP_Text txt = resolution.AddComponent<TextMeshProUGUI>();
            txt.text = $"{resolutions[i].width}x{resolutions[i].height}";

            UnityEngine.UI.Button btn = resolution.AddComponent<UnityEngine.UI.Button>();
            Resolution res = resolutions[i];
            btn.onClick.AddListener(() =>
            {
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            });
        }
        */
        yield return new WaitForSecondsRealtime(1);
        for (int i = 0; i < drop_resolutions.options.Count; i++)
        {
            resolutions_content.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = drop_resolutions.options[i].text;
        }

        text = IdiomManager.instance.GetKeyText("Intro screen error");
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1);

        ob_screen_menu.SetActive(true);
        ob_screen_menu.GetComponentInChildren<TMP_Dropdown>().Select();

        showing_settings = true;
        while (showing_settings) /// Espera a que termine de poner las opciones
        {
            yield return null;
        }
        init_text.text = init_text.text.Substring(0, init_text.text.Length - "ERROR".Length);
        init_text.text += '\t';
        init_text.text += IdiomManager.instance.GetKeyText("Intro ready");


        // Empieza el tutorial ------------------------------------------------------------------------
        init_text.text += "\n\n";

        text = IdiomManager.instance.GetKeyText("Intro configuration set");
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.5f);

        init_text.text += '\n';

        text = IdiomManager.instance.GetKeyText("Intro start program");
        StartCoroutine(ShowText(text));
        while (showing_text) /// Espera a que termine la animación del texto
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1;

        PlayerMovement.instance.rb.isKinematic = false;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        File.Create(started_file_path); // marca el tutorial como completado

        while (init_text.color.a > 0)
        {
            init_text.color = new Color(init_text.color.r, init_text.color.g, init_text.color.b, init_text.color.a - Time.deltaTime * 2);
            yield return null;
        }
        init_text.gameObject.SetActive(false);
    }

    public void SetOpenSettingsValue(bool value)
    {
        showing_settings = value;
    }
}