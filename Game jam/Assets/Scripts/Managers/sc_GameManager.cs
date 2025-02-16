using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gm { get; private set; }



    private float alpha = 0;
    private bool show_announce = false;
    private float txt_show_speed;
    private bool pause = false;

    public TMP_Text message_text;

    public GameObject pause_menu;

    void Awake()
    {
        if (gm == null)
            gm = this;
        else
            Destroy(this.gameObject);

        Application.targetFrameRate = 60;
        ShakeController(0, 0, 0);
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
    public void ShakeController(float time, float low_frequency, float high_frequency)
    {
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

        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pause_menu.SetActive(false);
        SoundManager.instance.SetHighPassEffect(10);
    }
    public void PauseGame()
    {
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
            if (!pause_menu.activeSelf) /// Si el menú de pausa se cierra se pone la frecuencia a 10 y se sale del bucle
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

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}