using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance {  get; private set; }
    public AudioSource audioSource;
    public AudioMixerGroup audioMixerGroup;

    [Header("References")]
    public Slider slider_volume;
    public Slider slider_sfx_volume;

    public bool funnySounds;

    [Header("Stats")]
    public int pool_number_audiosources;
    private int current_audiosource;
    private List<AudioSource> buffer_audiosources = new List<AudioSource>();

    private void Start()
    {
        if (audioMixerGroup != null && slider_volume != null)
        {
            audioMixerGroup.audioMixer.GetFloat("MasterVolume", out float v); /// Pone el slider según el volumen
            slider_volume.value = v;
        }

        // Crea una pool de audiosources
        for (int i = 0; i < pool_number_audiosources; i++)
        {
            GameObject audioSource = new GameObject();
            audioSource.name = "Audiosource " + i.ToString();
            AudioSource audio = audioSource.AddComponent<AudioSource>();

            audio.outputAudioMixerGroup = audioMixerGroup;

            buffer_audiosources.Add(audio);
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    public void PlaySound(AudioClip clip, bool loop = false)
    {
        audioSource.loop = loop;

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
    }
    public void PlaySound(AudioClip clip)
    {
        audioSource.loop = false;
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Hace sonar un audioClip en la posición indicada
    /// </summary>
    public AudioSource InstantiateSound(AudioClip clip, Vector3 position, float volume = 0.5f, AudioMixerGroup mixer = null, bool play_on_creation = true)
    {
        AudioSource curr = buffer_audiosources[current_audiosource];

        curr.clip = clip;
        curr.transform.position = position;
        curr.outputAudioMixerGroup = mixer ?? audioMixerGroup; /// Si mixer no se pasa como parámetro se usará audioMixerGroup
        curr.volume = volume;
        curr.loop = false;
        if (play_on_creation)
            curr.Play();

        current_audiosource++;
        if (current_audiosource >= buffer_audiosources.Count - 1)
            current_audiosource = 0;

        return curr;
    }

    public void PlaySoundOnAudioSource(AudioClip clip, AudioSource audioSource, bool loop = false)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
        audioSource.clip = null;
    }


    public void SetVolume()
    {
        audioMixerGroup.audioMixer.SetFloat("MasterVolume", slider_volume.value);
        GameManager.ChangeUIPosition(slider_volume.handleRect.anchoredPosition, CameraRotation.instance.ui_sensivity_value_image.rectTransform, CameraRotation.instance.ui_sensivity_value.rectTransform);
    }
    public void SetSFXVolume()
    {
        audioMixerGroup.audioMixer.SetFloat("SFXVolume", slider_sfx_volume.value);
    }

    public void SetHighPassEffect(float cutoff_freq)
    {
        audioMixerGroup.audioMixer.SetFloat("Highpass_cutoff", cutoff_freq);
    }

    public void ChangeFunnySounds()
    {
        funnySounds = !funnySounds;
    }
}
