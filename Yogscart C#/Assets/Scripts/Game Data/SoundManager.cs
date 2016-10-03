using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{

    public static float masterVolume = 100f, musicVolume = 100f, sfxVolume = 100f;
    const float fadeTime = 0.5f;

    private float lastMav = 1f;
    private AudioSource mSource, sfxSource, dSource;
    private bool mbeingUsed = false;

    // Use this for initialization
    void Awake ()
    {
        masterVolume = Mathf.Clamp(PlayerPrefs.GetInt("MAV", 100), 0, 100);
        musicVolume = Mathf.Clamp(PlayerPrefs.GetInt("MV", 50), 0, 100);
        sfxVolume = Mathf.Clamp(PlayerPrefs.GetInt("SFXV", 100), 0, 100);
        //DialogueVolume = Mathf.Clamp(PlayerPrefs.GetInt("DV",100),0,100);

        mSource = transform.FindChild("Music").GetComponent<AudioSource>();
        sfxSource = transform.FindChild("SFX").GetComponent<AudioSource>();
        //dSource = transform.FindChild("Dialogue").audio;

        mSource.loop = true;
        UpdateSound();
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateSound();

        AudioSource[] allSound = FindObjectsOfType<AudioSource>();

        foreach(AudioSource sound in allSound)
        {
            //If Audio Source is not one of ours, set it's volume to SFX volume
            if(sound != mSource && sound != sfxSource && sound != dSource)
            {
                sound.volume = sound.volume * sfxSource.volume;
            }
        }
    }

    void UpdateSound()
    {
        float mav = masterVolume / 100f;
        float mv = musicVolume / 100f;
        float sfxv = sfxVolume / 100f;

        if(!mbeingUsed && (mSource.volume != mv || lastMav != mav))
        {
            PlayerPrefs.SetInt("MAV", Mathf.RoundToInt(masterVolume));
            PlayerPrefs.SetInt("MV", Mathf.RoundToInt(musicVolume));
            mSource.volume = mav * mv;
        }

        if (sfxSource.volume != sfxv || lastMav != mav)
        {
            PlayerPrefs.SetInt("MAV", Mathf.RoundToInt(masterVolume));
            PlayerPrefs.SetInt("SFXV", Mathf.RoundToInt(sfxVolume));
            sfxSource.volume = mav * sfxv;
        }

        lastMav = mav;
    }

    public void PlaySFX(AudioClip nMusic)
    {
        if(sfxSource != null)
        {
            sfxSource.PlayOneShot(nMusic, 1f);
        }
    }

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
            float finalVolume = mSource.volume;

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
            mSource.volume = Mathf.Lerp(startVolume, endVolume, (Time.realtimeSinceStartup - startTime) / fadeTime);
            yield return null;
        }

    }
}
