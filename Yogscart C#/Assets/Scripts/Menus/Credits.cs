﻿using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour
{
    private MainMenu mm;
    private SoundManager sm;

    public AudioClip creditsMusic;
    public GUISkin skin;

    public static bool isPlaying = false;
    private float actualCreditsHeight = 0f;
    private const float scrollSpeed = 50f;
    private float creditsAlpha = 0f;

    public void StartCredits()
    {
        actualCreditsHeight = 0;

        mm = transform.GetComponent<MainMenu>();
        sm = GameObject.FindObjectOfType<SoundManager>();
        sm.PlayMusic(creditsMusic);

        isPlaying = true;
    }

    public IEnumerator StopCredits()
    {
        isPlaying = false;

        yield return new WaitForSeconds(0.5f);

        sm.PlayMusic(mm.menuMusic);

        enabled = false;
    }

    private void Update()
    {
        if (isPlaying)
        {
            actualCreditsHeight += Time.deltaTime * scrollSpeed;
            creditsAlpha = Mathf.Lerp(creditsAlpha, 1f, Time.deltaTime * 5f);
        }
        else
        {
            creditsAlpha = Mathf.Lerp(creditsAlpha, 0f, Time.deltaTime * 5f);
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        Color white = Color.white;
        white.a = creditsAlpha;
        GUI.color = white;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,Vector3.one);

        float creditsHeight = Mathf.Floor(actualCreditsHeight);

        float logoHeight = 0;
        if (mm != null)
        {
            logoHeight = (Screen.width / 2f) / mm.logo.width * mm.logo.height;
            GUI.DrawTexture(new Rect(Screen.width / 2f - Screen.width / 4f, Screen.height - creditsHeight, Screen.width / 2f, logoHeight), mm.logo, ScaleMode.ScaleToFit);
        }

        string[] credits = new string[] { "Created By", "Team Yogscart",
    "Programing", "Robo_Chiz",
    "A bit of Everything", "Ross",
    "3D / 2D Art", "Beardbotnik",
    "Graphics Dude", "Mysca",
    "Other Graphics Dude", "LinkTCOne",
    "Trophy Design", "Duck",
    "Music By", "Pico",
    "Yogscast Outro performed by", "Ben Binderow",
    "Testing", "Thorn",
    "Additional Music By", "Kevin MacLeod (incompetech.com) \n Licensed under Creative Commons: By Attribution 3.0 \n http://creativecommons.org/licenses/by/3.0/",
    "", "",
    "", "Thanks for Playing!",
    "", "Yogscart is a non-profit fan game and is in no way \n affiliated with the Yogscast or the youth olympic games. \n Please don't sue us! XXX"};

        float startY = Screen.height - creditsHeight + logoHeight;

        for (int i = 0; i < credits.Length; i += 2)
        {
            GUI.skin.label.fontSize = (int)(Mathf.Min(Screen.width, Screen.height) / 40f);
            GUI.Label(new Rect(0, startY + ((logoHeight / 3f) * i), Screen.width, logoHeight), credits[i]);

            int secHeight = Mathf.RoundToInt(startY + ((logoHeight / 3f) * (i + 0.4f)));

            GUI.skin.label.fontSize = (int)(Mathf.Min(Screen.width, Screen.height) / 20f);
            GUI.Label(new Rect(0, secHeight, Screen.width, logoHeight), credits[i + 1]);

            if (isPlaying && i == credits.Length - 2 && secHeight <= 0)
            {
                mm.BackMenu();
                StartCoroutine(StopCredits());
            }
        }

        GUI.color = Color.white;

    }
}
