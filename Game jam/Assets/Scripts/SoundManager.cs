using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance {  get; private set; }
    public AudioSource audioSource;

    public bool funnySounds;

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
        audioSource.Play();
    }

    public void InstantiateSound(AudioClip clip, Vector3 position, float seconds_to_destroy)
    {
        GameObject ob = new GameObject();

        ob.transform.position = position;
        AudioSource audio = ob.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.Play();

        Destroy(ob, seconds_to_destroy);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void ChangeFunnySounds()
    {
        funnySounds = !funnySounds;
    }
}
