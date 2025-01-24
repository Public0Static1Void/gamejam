using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menus : MonoBehaviour
{
    private Animator anim;

    public Button button;

    private AudioSource audioSource;

    public List<AudioClip> audiosToPlay;


    [SerializeField] Slider sliderVolume;
    [SerializeField] Toggle toggleScreenMode;

    private GameObject menuOpened;

    Scene actualScene;

    private void Start()
    {
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Sample Scene");
    }

    public void ButtonSelected()
    {
        audioSource.clip = audiosToPlay[1];// Sonido de selección
        audioSource.loop = false;
        audioSource.Play();
    }

    public void SelectedChickenAnimation()
    {
        anim.Play("Menu_scream_animation");
        audioSource.clip = audiosToPlay[2];// Chillido del gallo
        audioSource.loop = false;
        audioSource.Play();
    }

    IEnumerator WaitToContinue()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Level 2");
    }

    public void ExitGame()
    {
        Application.Quit();
    }



    // Cambiar volumen
    public void ChangeVolume()
    {
        AudioListener.volume = sliderVolume.value;
    }
    public void ChangeScreenMode()
    {
        switch (toggleScreenMode.isOn)
        {
            case true:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case false:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }
    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
