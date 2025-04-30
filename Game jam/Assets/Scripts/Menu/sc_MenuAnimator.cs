using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

public class sc_MenuAnimator : MonoBehaviour
{
    public TMP_Text txt_pressanykey;

    public Animator menu_anim;

    public UnityEngine.UI.Button btn_play;

    private bool play_pressed = false;

    private void Start()
    {
        UIFadeOutFadeInRepeate(txt_pressanykey);
        btn_play.Select();
    }

    private void Update()
    {
        if (!play_pressed)
        {
            foreach (var gamepad in Gamepad.all)
            {
                // Check if any button was pressed this frame
                foreach (var control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                    {
                        Debug.Log($"Button {control.name} was pressed");
                        play_pressed = true;
                    }
                }
            }
        }
    }

    public void AnyKey(InputAction.CallbackContext con)
    {
        if (!play_pressed && con.performed)
        {
            play_pressed = true;
        }
    }

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