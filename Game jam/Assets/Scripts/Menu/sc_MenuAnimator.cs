using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

public class sc_MenuAnimator : MonoBehaviour
{
    public TMP_Text txt_pressanykey;

    public Animator menu_anim;

    public UnityEngine.UI.Button btn_play;

    private bool play_pressed = false;

    [Header("Clips")]
    public AudioClip clip_game_started;
    public AudioClip clip_close_menu;


    private float timer = 0;
    private bool enter_pressed = false;

    [Header("References")]
    public UnityEngine.UI.Image im_input_icon;
    public Sprite spr_enter;
    public Sprite spr_R2;
    public Sprite spr_RT;

    [Header("Options")]
    public GameObject options_menu;

    public List<GameObject> menus_list;

    private string last_controller = "";
    private Vector2 im_input_startsize;


    private void Start()
    {
        UIFadeOutFadeInRepeate(txt_pressanykey);
        btn_play.Select();

        im_input_startsize = im_input_icon.rectTransform.sizeDelta;
    }

    private void Update()
    {
        string controller = GameManager.gm.GetCurrentControllerName();
        if (controller != last_controller)
        {
            if (controller.ToLower().Contains("dualshock") || controller.ToLower().Contains("dualsense"))
            {
                im_input_icon.sprite = spr_R2;
            }
            else if (controller.ToLower().Contains("xbox") || controller.ToLower().Contains("xinput"))
            {
                im_input_icon.sprite = spr_RT;
            }
            else if (controller.ToLower().Contains("keyboard"))
            {
                im_input_icon.sprite = spr_enter;
            }

            last_controller = controller; /// Evita hacer comprobaciones innecesarias si no se ha cambiado de control
        }


        if (!play_pressed)
        {
            foreach (var gamepad in Gamepad.all)
            {
                // Check if any button was pressed this frame
                foreach (var control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                    {
                        menu_anim.Play("play_pressed");
                        SoundManager.instance.InstantiateSound(clip_game_started, transform.position);
                        btn_play.Select(); /// Selecciona el botón de play
                        play_pressed = true;
                    }
                }
            }
        }

        if (enter_pressed)
        {
            timer += Time.deltaTime;

            im_input_icon.rectTransform.sizeDelta *= 1.005f;
            im_input_icon.color = new Color(im_input_icon.color.r + Time.deltaTime, im_input_icon.color.g + Time.deltaTime, im_input_icon.color.b, 1);

            if (timer > 1.5f)
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    #region Inputs
    public void AnyKey(InputAction.CallbackContext con)
    {
        if (!play_pressed && con.performed)
        {
            play_pressed = true;
            menu_anim.Play("play_pressed");
            SoundManager.instance.InstantiateSound(clip_game_started, transform.position);
        }
    }
    public void EnterGame(InputAction.CallbackContext con)
    {
        if (!im_input_icon.gameObject.activeSelf) return;

        if (con.performed)
        {
            enter_pressed = true;
            GameManager.gm.SpawnUICircle(im_input_icon.rectTransform, 2, Color.white, true, 6);
        }
        if (con.canceled)
        {
            enter_pressed = false;
            im_input_icon.color = Color.white;
            im_input_icon.rectTransform.sizeDelta = im_input_startsize;
            timer = 0;
        }
    }
    public void CloseMenus(InputAction.CallbackContext con)
    {
        if (!menus.instance.on_ability_menu && con.performed)
        {
            for (int i = 0; i < menus_list.Count; i++)
            {
                menus_list[i].SetActive(false);
                SoundManager.instance.InstantiateSound(clip_close_menu, transform.position);
            }
        }
    }

    // Options
    public void CloseOptions(InputAction.CallbackContext con)
    {
        if (options_menu.activeSelf && con.performed)
        {
            menu_anim.Play("anim_play_menu_still");
            //RewindAnimation("anim_open_settings");
            btn_play.Select();
        }
    }
    #endregion


    public void RewindAnimation(string animation)
    {
        StartCoroutine(RewindAnimationRoutine(animation));
    }
    private IEnumerator RewindAnimationRoutine(string animation)
    {
        menu_anim.speed = -0.5f;
        menu_anim.SetFloat("_speed", -1);
        menu_anim.Play(animation);

        while (!menu_anim.GetCurrentAnimatorStateInfo(0).IsName(animation))
        {
            yield return null;
        }

        // Espera a que la animación acabe
        while (menu_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        menu_anim.speed = 1;
    }

    public void StartAnimation(string name)
    {
        menu_anim.Play(name);
    }
    public void SetAnimationSpeed(float speed)
    {
        menu_anim.speed = speed;
        menu_anim.SetFloat("_speed", speed);
    }

    #region ChangeUISize
    public void UIGrowButton(UnityEngine.UI.Image image)
    {
        StartCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 1.5f, image.rectTransform.sizeDelta.y * 1.25f)));
    }
    public void UIShrinkButton(UnityEngine.UI.Image image)
    {
        StartCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 0.5f, image.rectTransform.sizeDelta.y * 0.75f)));
    }
    private IEnumerator UIChangeButtonSizeCoroutine(UnityEngine.UI.Image image, Vector2 new_size)
    {
        while (Vector2.Distance(image.rectTransform.sizeDelta, new_size) > 0.1f)
        {
            image.rectTransform.sizeDelta = Vector2.Lerp(image.rectTransform.sizeDelta, new_size, Time.deltaTime * 2);
            yield return null;
        }
    }
    #endregion

    #region FadeInFadeOut
    public void UIFadeOut(TMP_Text text)
    {
        StartCoroutine(UIFadeOutRoutine(text));
    }
    public void UIFadeOut(UnityEngine.UI.Image image)
    {
        StartCoroutine(UIFadeOutRoutine(image));
    }

    public void UIFadeIN(TMP_Text text)
    {
        StartCoroutine(UIFadeINRoutine(text));
    }
    public void UIFadeIN(UnityEngine.UI.Image image)
    {
        StartCoroutine(UIFadeINRoutine(image));
    }

    public void UIFadeOutFadeIn(TMP_Text text)
    {
        StartCoroutine(UIFadeOutRoutine(text));
    }
    public void UIFadeOutFadeIn(UnityEngine.UI.Image image)
    {
        StartCoroutine(UIFadeOutRoutine(image));
    }
    public void UIFadeOutFadeInRepeate(TMP_Text text)
    {
        StartCoroutine(UIFadeOutFadeInRoutineRepeat(text));
    }                                  
    public void UIFadeOutFadeInRepeate(UnityEngine.UI.Image image)
    {                                  
        StartCoroutine(UIFadeOutFadeInRoutineRepeat(image));
    }


    private IEnumerator UIFadeOutRoutine(TMP_Text text)
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;

            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

            yield return null;
        }
    }
    private IEnumerator UIFadeOutRoutine(UnityEngine.UI.Image image)
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;

            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

            yield return null;
        }
    }
    private IEnumerator UIFadeINRoutine(TMP_Text text)
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime;

            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

            yield return null;
        }
    }
    private IEnumerator UIFadeINRoutine(UnityEngine.UI.Image image)
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime;

            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

            yield return null;
        }
    }

    private IEnumerator UIFadeOutFadeInRoutine(TMP_Text text)
    {
        UIFadeOut(text);
        yield return new WaitForSeconds(1);
        UIFadeIN(text);
    }
    private IEnumerator UIFadeOutFadeInRoutine(UnityEngine.UI.Image image)
    {
        UIFadeOut(image);
        yield return new WaitForSeconds(1);
        UIFadeIN(image);
    }
    private IEnumerator UIFadeOutFadeInRoutineRepeat(TMP_Text text)
    {
        while (true)
        {
            UIFadeOut(text);
            yield return new WaitForSeconds(1);
            UIFadeIN(text);
            yield return new WaitForSeconds(1);
        }
    }
    private IEnumerator UIFadeOutFadeInRoutineRepeat(UnityEngine.UI.Image image)
    {
        while (true)
        {
            UIFadeOut(image);
            yield return new WaitForSeconds(1);
            UIFadeIN(image);
            yield return new WaitForSeconds(1);
        }
    }
    #endregion
}