using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{

    CurrentGameData gd;

    public GUISkin skin;

    public Texture2D logo;
    AudioClip menuMusic;

    public Color selectedColor;
    int currentSelection;

    public Texture2D sidePicture;
    private float sidePictureAmount = 0;

    private bool sliding = false;
    List<MenuState> backStates = new List<MenuState>();
    private float sideAmount = 0f;

    enum MenuState {Start, Main, SinglePlayer, Difficulty, CharacterSelect, LevelSelect, Multiplayer, Online, Options, Popup, Credits };
    private MenuState state = MenuState.Start;

	// Use this for initialization
	void Start ()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();

        //Update as more characters are added
        sidePicture = Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + Random.Range(0,1).ToString());
        sidePictureAmount = GUIHelper.width / 2f;

        CurrentGameData.blackOut = false;
        InputManager.allowedToChange = true;
	}
	
	// Update is called once per GUI Frame
	void OnGUI()
    {

        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        string[] options;

        //Draw Title
        float titleWidth = (GUIHelper.width / 2f) - 10f;
        float ratio = titleWidth / logo.width;
        GUI.DrawTexture(new Rect(10, 10, titleWidth, logo.height * ratio), logo, ScaleMode.ScaleToFit);

        //Draw Side Image
        GUI.DrawTexture(new Rect(sidePictureAmount, 40, GUIHelper.width / 2f - 30, GUIHelper.height - 20), sidePicture, ScaleMode.ScaleToFit);

        //Draw Stuff
        switch (state)
        {
            case MenuState.Start:
                GUI.Label(new Rect(sideAmount + GUIHelper.width / 8f, 680, 960, 400), "Press Start / Enter!");
                break;
            case MenuState.Main:
                GUI.Label(new Rect(sideAmount + GUIHelper.width / 8f, 680, 960, 400), "Yeah there's nothing here!");
                break;
        }

        //Handle Inputs
        if (!sliding && InputManager.controllers.Count > 0)
        {
            switch (state)
            {
                case MenuState.Start:

                    if (InputManager.controllers[0].GetInput("Submit") != 0)
                        ChangeMenu(MenuState.Main);

                    break;
                case MenuState.Main:

                    break;
            }

            if (InputManager.controllers[0].GetInput("Cancel") != 0)
            {
                BackMenu();
            }
        }
    }

    //Go Back to Previous Menu
    void BackMenu()
    {
        if (backStates.Count > 0)
        {
            StartCoroutine("ChangeMenuPhysical", backStates[backStates.Count - 1]);
            backStates.RemoveAt(backStates.Count - 1);
        }
    }

    void ChangeMenu(MenuState changeState)
    {
        StartCoroutine("ChangeMenuPhysical", changeState);
        backStates.Add(state);
    }

    IEnumerator ChangeMenuPhysical(MenuState changeState)
    {
        if(!sliding)
        {
            sliding = true;

            float startTime = Time.time;
            float travelTime = 0.5f;

            //Slide Off //////////////////////////////
            sideAmount = 0f;

            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(0f, -Screen.width, (Time.time - startTime) / travelTime);
                yield return null;
            }

            //Pause at Top///////////////////////////////////////
            sideAmount = -Screen.width;
                     
            state = changeState;

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(-Screen.width, 0f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            sideAmount = 0f;

            sliding = false;
        }
    }
}
