using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SaveManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager gm { get; private set; }



    private float alpha = 0;
    private bool show_announce = false;
    private float txt_show_speed;
    private bool pause = false;

    public TMP_Text message_text;

    public GameObject pause_menu;

    private CursorLockMode previous_lockmode;

    public enum TextPositions { CENTER, CENTER_LOWER, LAST_NO_USE }

    [Header("References")]
    public List<TMP_Text> screen_texts;

    public Button btn_resume, btn_continue;

    public GameObject stats_resume_holder;

    public TMP_Text txt_enemies_killed;
    public TMP_Text txt_damage_done;
    public TMP_Text txt_damage_recieved;
    public TMP_Text txt_damage_healed;
    public TMP_Text txt_total_score_points;
    public TMP_Text txt_converted_score_points;

    [Header("Stats")]
    public int enemies_killed = 0;
    public int damage_done = 0;
    public int damage_recieved = 0;
    public int damage_healed = 0;



    public SaveManager saveManager;

    public PlayerInput playerInput;

    public string controller_name;

    void Awake()
    {
        if (gm == null)
            gm = this;
        else
            Destroy(this.gameObject);

        Application.targetFrameRate = 60;
        ShakeController(0, 0, 0);

        saveManager = GetComponent<SaveManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        // Fade del texto
        if (show_announce)
        {
            alpha += Time.deltaTime * txt_show_speed;
            if (alpha < 1)
            {
                Color col = message_text.color;
                message_text.color = new Color(col.r, col.g, col.b, alpha);
            }
            else if (alpha / 3 > 2)
            {
                show_announce = false;
            }
        }
        else if (alpha > 0)
        {
            alpha -= Time.deltaTime * txt_show_speed;
            Color col = message_text.color;
            message_text.color = new Color(col.r, col.g, col.b, alpha);
        }
    }

    public string GetCurrentControllerName()
    {
        string name = "Keyboard";
        if (playerInput.currentControlScheme == "Gamepad")
        {
            if (Gamepad.current != null)
            {
                name = Gamepad.current.displayName;
            }
        }

        return name;
    }

    public void ShakeController(float time, float low_frequency, float high_frequency)
    {
        // Solo vibrar� el mando cuando se est� usando
        if (GetCurrentControllerName() == "Keyboard") return;

        StartCoroutine(ControllerShake(time, low_frequency, high_frequency));
    }
    private IEnumerator ControllerShake(float time, float low_frequency, float high_frequency)
    {
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            pad.SetMotorSpeeds(low_frequency, high_frequency);
            yield return new WaitForSeconds(time);
            pad.SetMotorSpeeds(0, 0);
        }
    }

    public void ResumeGame()
    {
        PlayerMovement.instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

        Time.timeScale = previous_lockmode == CursorLockMode.None ? 0 : 1;

        Cursor.lockState = previous_lockmode;
        Cursor.visible = previous_lockmode == CursorLockMode.None ? true : false;

        pause_menu.SetActive(false);
        SoundManager.instance.SetHighPassEffect(10);
    }
    public void PauseGame()
    {
        if (!pause_menu.activeSelf)
            previous_lockmode = Cursor.lockState;

        btn_resume.Select();

        pause_menu.SetActive(true);
        ShakeController(0, 0, 0);
        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerMovement.instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        StartCoroutine(InterpolateHighPass(1000));
    }

    private IEnumerator InterpolateHighPass(float value)
    {
        float target_highpass_freq = value;
        float current_freq = 10;
        while (current_freq < target_highpass_freq)
        {
            if (!pause_menu.activeSelf) /// Si el men� de pausa se cierra se pone la frecuencia a 10 y se sale del bucle
            {
                SoundManager.instance.SetHighPassEffect(10);
                break;
            }
            current_freq += Time.unscaledDeltaTime * 250;
            SoundManager.instance.SetHighPassEffect(current_freq);
            yield return null;
        }
    }

    public void ShowOrHideGameobject(GameObject ob)
    {
        ob.SetActive(!ob.gameObject.activeSelf);
    }

    public void SelectUIButton(Button button)
    {
        button.Select();
    }

    public void InputPause(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            pause = !pause;
            if (pause)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void ShowText(string text, int show_speed = 3)
    {
        message_text.text = text;
        txt_show_speed = show_speed;
        show_announce = true;
    }
    /// <summary>
    /// Cambia la opacidad y el contenido del texto que le indiques, seg�n si le das una velocidad positiva o negativa
    /// </summary>
    public void ShowText(TextPositions text_position, string text, float showspeed)
    {
        StartCoroutine(ShowTextCoroutine(screen_texts[(int)text_position], text,  showspeed));
    }
    
    private IEnumerator ShowTextCoroutine(TMP_Text text_reference, string text, float show_speed)
    {
        text_reference.text = text;

        float alpha = 0;
        float timer = 0;
        Color col = text_reference.color;

        int multiplier = 1;
        if (show_speed < 0)
        {
            multiplier = -1;
            show_speed *= -1;
        }

        while (timer < 1)
        {
            timer += Time.deltaTime * show_speed;
            
            /// Cambia el color del texto
            alpha += (Time.deltaTime * show_speed) * multiplier;
            text_reference.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public IEnumerator HideImage(float hide_speed, UnityEngine.UI.Image image_to_hide, TMP_Text text_to_hide = null)
    {
        Color col = image_to_hide.color;
        while (image_to_hide.color.a > 0)
        {
            image_to_hide.color = new Color(col.r, col.g, col.b, image_to_hide.color.a - Time.fixedDeltaTime * hide_speed);
            if (text_to_hide != null)
            {
                text_to_hide.color = new Color(text_to_hide.color.r, text_to_hide.color.g, text_to_hide.color.b, image_to_hide.color.a);
            }
            yield return null;
        }
        image_to_hide.rectTransform.anchoredPosition = Vector2.zero;
        image_to_hide.rectTransform.position = Vector2.zero;
        if (text_to_hide != null)
        {
            text_to_hide.rectTransform.anchoredPosition = Vector2.zero;
            text_to_hide.rectTransform.position = Vector2.zero;
        }
    }

    public void ChangeUIPosition(Vector2 position, RectTransform element1, RectTransform element2 = null)
    {
        element1.anchoredPosition = position;

        Vector2 anch_pos = element1.anchoredPosition;
        Vector2 pos = element1.position;
        if (element2 != null)
        {
            element2.anchoredPosition = anch_pos;
            element2.position = new Vector2(pos.x - element1.rect.width / 3, pos.y);
        }
    }

    public void SaveGame()
    {
        // Estructura los datos para guardarlos
        PlayerData playerData = new PlayerData();

        playerData.stamina = PlayerMovement.instance.max_stamina;
        playerData.speed = PlayerMovement.instance.speed;
        playerData.damage = ReturnScript.instance.damage;
        playerData.explosion_range = ReturnScript.instance.damage;
        playerData.hp = ReturnScript.instance.GetComponent<PlayerLife>().max_hp;
        playerData.score = ScoreManager.instance.score;

        saveManager.SaveGame(playerData);
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(10);
        LoadScene("Menu");
    }

    public void EndGame()
    {
        // Mostrar UI con resumen de estad�sticas de la partida
        StartCoroutine(EndGameCoroutine());

        SelectUIButton(btn_continue);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShakeController(0, 0, 0);

        Time.timeScale = 0;

        txt_damage_done.text = "Damage dealt: " + damage_done.ToString();
        txt_damage_healed.text = "Damage healed: " + damage_healed.ToString();
        txt_damage_recieved.text = "Damage recieved: " + damage_recieved.ToString();

        txt_enemies_killed.text = "Enemies killed: " + enemies_killed.ToString();

        txt_total_score_points.text = "Score: " + ScoreManager.instance.score.ToString();
        txt_converted_score_points.text = "Skill points: " + (ScoreManager.instance.score / 100).ToString();

        stats_resume_holder.SetActive(true);

        ScoreManager.instance.score += ScoreManager.instance.score / 100;

        SaveGame();
    }
}