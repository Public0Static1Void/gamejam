using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class menus : MonoBehaviour
{
    private Animator anim;

    public Button button;

    private AudioSource audioSource;

    public List<AudioClip> audiosToPlay;


    public TMP_Text txt_damage;
    public TMP_Text txt_speed;
    public TMP_Text txt_stamina;
    public TMP_Text txt_explosion_range;
    public TMP_Text txt_hp;

    public TMP_Text txt_skill_points;

    [SerializeField] Slider sliderVolume;
    [SerializeField] Toggle toggleScreenMode;

    private GameObject menuOpened;

    Scene actualScene;

    SaveManager sm;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        sm = GetComponent<SaveManager>();
    }

    public void LoadStatsTexts()
    {
        PlayerData pd = sm.LoadSaveData();

        if (pd == null)
        {
            pd = new PlayerData();
            pd.damage = 8;
            pd.speed = 250;
            pd.stamina = 10;
            pd.hp = 15;
            pd.explosion_range = 4;
            pd.score = 0;

            sm.SaveGame(pd);
        }
        if (pd.hp < 0)
        {
            pd.hp = 15;
            sm.SaveGame(pd);
        }
        if (pd.score < 0)
        {
            pd.score = 0;
            sm.SaveGame(pd);
        }

        pd = sm.LoadSaveData();

        txt_damage.text = "DAMAGE: " + pd.damage.ToString();
        txt_speed.text = "SPEED: " + pd.speed.ToString();
        txt_stamina.text = "STAMINA: " + pd.stamina.ToString();
        txt_hp.text = "HP: " + pd.hp.ToString();
        txt_explosion_range.text = "EXPLOSION RANGE: " + pd.explosion_range.ToString();
        txt_skill_points.text = "SKILL POINTS: " + pd.score.ToString();
    }

    public void UpgradeStat(GameObject stat)
    {

        PlayerData pd = sm.LoadSaveData();

        if (pd == null || pd.score < 1) return;

        string name = stat.name;

        switch (name)
        {
            case "DAMAGE":
                pd.damage++;
                txt_damage.text = "DAMAGE: " + pd.damage.ToString();
                break;
            case "SPEED":
                pd.speed += 10;
                txt_speed.text = "SPEED: " + pd.speed.ToString();
                break;
            case "EXPLOSION RANGE":
                pd.explosion_range += 0.5f;
                txt_hp.text = "EXPLOSION RANGE: " + pd.explosion_range.ToString();
                break;
            case "HP":
                pd.hp++;
                txt_hp.text = "HP: " + pd.hp.ToString();
                break;
            case "STAMINA":
                pd.stamina++;
                txt_stamina.text = "STAMINA: " + pd.stamina.ToString();
                break;
        }

        pd.score--;
        txt_skill_points.text = "SKILL POINTS: " + pd.score.ToString();

        Debug.Log("Stat upgraded");
        sm.SaveGame(pd);
    }

    public void PlayGame()
    {
        DontDestroyOnLoad(SoundManager.instance.InstantiateSound(audiosToPlay[0], transform.position));
        SceneManager.LoadScene("SampleScene");
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
        audioSource.clip = audiosToPlay[2];
        audioSource.loop = false;
        audioSource.Play();
    }

    IEnumerator WaitToContinue()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("SampleScene");
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
