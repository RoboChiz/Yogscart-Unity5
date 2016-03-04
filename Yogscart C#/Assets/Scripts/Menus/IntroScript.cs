using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroScript : MonoBehaviour {

    private bool showText = false,ending = false;

    public GUISkin skin;

        // Use this for initialization
        void Start ()
    {
        StartCoroutine("PlayIntro");
	}

    void Update()
    {
        if(showText && Input.anyKey && !ending)
        {
            StopCoroutine("PlayIntro");
            StartCoroutine("End");
        }
    }
	
    IEnumerator PlayIntro()
    {
        CurrentGameData.blackOut = true;
        showText = true;

        yield return new WaitForSeconds(1f);

        CurrentGameData.blackOut = false;

        yield return new WaitForSeconds(9f);

        StartCoroutine("End");

    }

    IEnumerator End()
    {
        ending = true;
        CurrentGameData.blackOut = true;

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Main_Menu");
    }
	
    void OnGUI()
    {
        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        if (showText)
        {
            float labelWidth = (GUIHelper.width - 20);
            float labelHeight = (GUIHelper.height - 20);

            GUI.Label(new Rect(10, 10, labelWidth, labelHeight),
                @"Yogscart is a Non-Profit fan game and is 
in no way affiliated with the Yogscast or
the Youth Olympic Games.
Please don't sue us! XXX

This is an Alpha build of the C# Version 
of the game for testing and debugging.
Not all feautres have been ported over! 
For a more complete version of the game, 
go play the offical release.

We hope you enjoy the Game!
From the Yogscart Team");
        }
    }
}
