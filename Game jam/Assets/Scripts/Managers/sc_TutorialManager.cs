using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance { get; private set; }

    public bool onTutorial;

    [Header("References")]
    public UnityEngine.UI.Image im_start_bg;
    public GameObject ob_main_ui;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetPlayerMovement(false);
        SetPlayerRotation(Quaternion.identity);

        ob_main_ui.SetActive(false);
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
    #endregion
}
