using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used in conjunction with Sound Manager to allow Player to change Volume
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourceInfo : MonoBehaviour {

    public enum AudioType { Music, SFX }

    public AudioType audioType;
    public float idealVolume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

	// Update is called once per frame
	void Update ()
    {
        float modifier = 1f;

        if (audioType == AudioType.Music)
            modifier = SoundManager.musicVolume * SoundManager.masterVolume;
        else
            modifier = SoundManager.sfxVolume * SoundManager.masterVolume;

        audioSource.volume = idealVolume * modifier;
        audioSource.dopplerLevel = 0f;
    }
}
