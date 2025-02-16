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

    public bool funnySounds;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioMixerGroup.audioMixer.GetFloat("MasterVolume", out float v); /// Pone el slider según el volumen
        slider_volume.value = v;
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
    /// <summary>
    /// Instancia un sonido por el tiempo mandado en la posición indicada
    /// </summary>
    public void InstantiateSound(AudioClip clip, Vector3 position, float seconds_to_destroy, AudioMixerGroup mixer = null)
    {
        GameObject ob = new GameObject();

        ob.transform.position = position;
        AudioSource audio = ob.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.outputAudioMixerGroup = mixer ?? audioMixerGroup; /// Si mixer no se pasa como parámetro se usará audioMixerGroup
        audio.Play();

        Destroy(ob, seconds_to_destroy);
    }

    public void SetVolume()
    {
        audioMixerGroup.audioMixer.SetFloat("MasterVolume", slider_volume.value);
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
