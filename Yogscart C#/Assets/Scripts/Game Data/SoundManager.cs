﻿using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{

    public static float masterVolume = 1f, musicVolume = 1f, sfxVolume = 1f;
    private float lastMav = -1f, lastMuv = -1f, lastsfV = -1f;

    const float fadeTime = 0.5f;

    
    private AudioSource mSource, sfxSource, dSource;
    private AudioSourceInfo mSourceInfo, sfxSourceInfo;

    private bool mbeingUsed = false;

    // Use this for initialization
    void Awake ()
    {
        masterVolume = Mathf.Clamp(PlayerPrefs.GetFloat("MAV", 1), 0, 1);
        musicVolume = Mathf.Clamp(PlayerPrefs.GetFloat("MV", 0.5f), 0, 1);
        sfxVolume = Mathf.Clamp(PlayerPrefs.GetFloat("SFXV", 1), 0, 1);

        lastMav = masterVolume;
        lastMuv = musicVolume;
        lastsfV = sfxVolume;

        mSource = transform.FindChild("Music").GetComponent<AudioSource>();
        mSourceInfo = transform.FindChild("Music").GetComponent<AudioSourceInfo>();
        mSourceInfo.idealVolume = musicVolume;

        sfxSource = transform.FindChild("SFX").GetComponent<AudioSource>();
        sfxSourceInfo = transform.FindChild("SFX").GetComponent<AudioSourceInfo>();
        sfxSourceInfo.idealVolume = sfxVolume;

        mSource.loop = true;
    }

    // Update is called once per frame
    void Update ()
    {
        if(masterVolume != lastMav)
        {
            PlayerPrefs.SetFloat("MAV", masterVolume);
            lastMav = masterVolume;
        }

        if (musicVolume != lastMuv)
        {
            PlayerPrefs.SetFloat("MV", musicVolume);
            lastMuv = musicVolume;
        }

        if (sfxVolume != lastsfV)
        {
            PlayerPrefs.SetFloat("SFXV", sfxVolume);
            lastsfV = sfxVolume;
        }
    }


    public void PlaySFX(AudioClip nMusic, float volumeScale)
    {
        if(sfxSource != null)
        {
            sfxSource.PlayOneShot(nMusic, volumeScale);
        }
    }

    public void PlaySFX(AudioClip nMusic) { PlaySFX(nMusic, 1f); }

    public void PlayMusic(AudioClip nMusic)
    {
        StartCoroutine(ActualPlayMusic(nMusic));
    }

    private IEnumerator ActualPlayMusic(AudioClip nMusic)
    {
        if (mSource != null)
        {
            //Wait for current track swap to finish
            while (mbeingUsed)
                yield return null;

            mbeingUsed = true;
            float finalVolume = mSourceInfo.idealVolume;

            if (mSource.isPlaying)
                yield return StartCoroutine("TransitionVolume",0f);

            mSource.Stop();
            mSource.clip = nMusic;
            mSource.Play();

            yield return StartCoroutine("TransitionVolume", finalVolume);

            mbeingUsed = false;
        }
    }

    public void StopMusic()
    {
        StartCoroutine(ActualStopMusic());
    }

    private IEnumerator ActualStopMusic()
    {
        //Wait for current track swap to finish
        while (mbeingUsed)
            yield return null;

        mbeingUsed = true;

        if (mSource.isPlaying)
            yield return StartCoroutine("TransitionVolume", 0f);

        mSource.Stop();
        mbeingUsed = false;
    }

    private IEnumerator TransitionVolume(float endVolume)
    {
        float startTime = Time.realtimeSinceStartup;
        float startVolume = mSource.volume;

        while ((Time.realtimeSinceStartup - startTime) < fadeTime)
        {
            mSourceInfo.idealVolume = Mathf.Lerp(startVolume, endVolume, (Time.realtimeSinceStartup - startTime) / fadeTime);
            yield return null;
        }

    }

    public void SetMusicPitch(float value)
    {
        mSource.pitch = value;
    }
}
