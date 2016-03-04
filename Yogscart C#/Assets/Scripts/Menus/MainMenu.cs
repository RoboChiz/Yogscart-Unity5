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
    private bool pictureTransitioning = false ,lockPicture = false;

    private bool moveTitle = false;
    private bool sliding = false;
    List<MenuState> backStates = new List<MenuState>();
    private float sideAmount = 0f;
    private float titleAmount = 10f;
    int randomImage;

    private string popupText = "";

    public enum MenuState {Start, Main, SinglePlayer, Difficulty, CharacterSelect, LevelSelect, Multiplayer, Online, Options, Popup, Credits };
    private MenuState state = MenuState.Start;

	// Use this for initialization
	void Start ()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();

        //Update as more characters are added
        randomImage = Random.Range(0, 1);

        sidePicture = Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + randomImage.ToString());
        sidePictureAmount = GUIHelper.width / 2f;

        CurrentGameData.blackOut = false;
        InputManager.allowedToChange = true;
	}
	
	// Update is called once per GUI Frame
	void OnGUI()
    {

        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        string[] options = new string[] { };

        //Draw Title
        float titleWidth = (GUIHelper.width / 2f) - 10f;
        float ratio = titleWidth / logo.width;

        if (moveTitle)
        {
            if (sideAmount < titleAmount)
                titleAmount = sideAmount;
        }
        else
        {
            if (sideAmount > titleAmount)
                titleAmount = sideAmount;
        }            

        GUI.DrawTexture(new Rect(titleAmount, 10, titleWidth, logo.height * ratio), logo, ScaleMode.ScaleToFit);

        if (sidePicture != null)
        GUI.DrawTexture(new Rect(sidePictureAmount, 40, GUIHelper.width / 2f - 30, GUIHelper.height - 20), sidePicture, ScaleMode.ScaleToFit);

        //Draw Stuff
        Rect box = new Rect(sideAmount, 0, GUIHelper.width / 2f, GUIHelper.height);
        float optionHeight = GUI.skin.label.fontSize + 10;
        switch (state)
        {
            case MenuState.Start:
                GUI.Label(new Rect(sideAmount + GUIHelper.width / 8f - 100, 680, 960, 400), "Press Start / Enter!");
                break;
            case MenuState.Main:
                options = new string[] { "Single Player", "Multiplayer", "Online", "Options", "Credits", "Quit" };

                if (sidePicture != null)
                {
                    switch(options[currentSelection])
                    {
                        case "Multiplayer":
                            if (sidePicture.name != "Multiplayer")
                                StartCoroutine(ChangePicture(Resources.Load<Texture2D>("UI/New Main Menu/Side Images/Multiplayer")));
                            break;
                        case "Online":
                            if (sidePicture.name != "Online")
                                StartCoroutine(ChangePicture(Resources.Load<Texture2D>("UI/New Main Menu/Side Images/Online")));
                            break;
                        case "Options":
                            if (sidePicture.name != "Options")
                                StartCoroutine(ChangePicture(Resources.Load<Texture2D>("UI/New Main Menu/Side Images/Options")));
                            break;
                        default:
                            if (sidePicture.name != randomImage.ToString())
                                StartCoroutine("ChangePicture", Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + randomImage.ToString()));
                        break;
                    }
                }
                else
                {
                    StartCoroutine("ChangePicture", Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + randomImage.ToString()));
                }

                break;
            case MenuState.SinglePlayer:
                options = new string[] { "Tournament", "VS Race", "Time Trial" };
                InputManager.allowedToChange = false;
            break;
            case MenuState.Multiplayer:
                options = new string[] { "Tournament", "VS Race" };
                InputManager.allowedToChange = true;
            break;
            case MenuState.Online:
                InputManager.allowedToChange = true;
            break;
            case MenuState.Options:
                options = new string[] { "Nothing Here Yet" };
                InputManager.allowedToChange = true;
            break;
            case MenuState.Difficulty:
                //options = ["50cc - Only for little Babby!","100cc - You mother trucker!","150cc - Oh what big strong muscles!","Insane - Prepare your butts!","Back"];
                if (CurrentGameData.unlockedInsane)
                    options = new string[] { "50cc", "100cc", "150cc", "Insane" };
                else
                    options = new string[] { "50cc", "100cc", "150cc" };
            break;
            case MenuState.Popup:
                options = new string[] { };
                GUI.Label(new Rect(box.x + 40, 20 + (box.height / 4f), box.width - 20, box.height - 20 - (box.height / 4f)), popupText);
             break;
            case MenuState.Credits:
                if (sidePicture != null)
                {
                    StartCoroutine(ChangePicture(null));
                }
                break;
        }

        GUI.BeginGroup(box);
            if (options != null && options.Length > 0)
            {
                //Single Player is the longest word in the menu and is 13 characters long			
                for (int i = 0; i < options.Length; i++)
				{
                    if (currentSelection == i)
                        GUI.skin.label.normal.textColor = selectedColor;
                    else
                        GUI.skin.label.normal.textColor = Color.white;

                    var labelRect = new Rect(40, 20 + (box.height / 3f) + (i * optionHeight), box.width - 20, optionHeight);
                    GUI.Label(labelRect, options[i]);

                    labelRect.x += box.x;
                    labelRect.y += box.y;

                }
                GUI.skin.label.normal.textColor = Color.white;
            }
        GUI.EndGroup();

        //Handle Inputs
        if (!sliding && InputManager.controllers.Count > 0)
        {

            int vertical = InputManager.controllers[0].GetMenuInput("MenuVertical");
            int horizontal = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
            bool submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);

            //Menu Navigation
            if (options != null && options.Length > 0)
            {
                currentSelection += vertical;
                if(currentSelection < 0 || currentSelection>= options.Length)
                    currentSelection = MathHelper.NumClamp(currentSelection, 0, options.Length);
            }

            if (submitBool)
            {
                switch (state)
                {
                    case MenuState.Start:
                        ChangeMenu(MenuState.Main);
                        break;
                    case MenuState.Main:
                        switch (options[currentSelection])
                        {
                            case "Single Player":
                                ChangeMenu(MenuState.SinglePlayer);
                                gd.GetComponent<InputManager>().RemoveOtherControllers();
                                break;
                            case "Multiplayer":
                                ChangeMenu(MenuState.Multiplayer);
                                break;
                            case "Online":
                                ChangeMenu(MenuState.Online);
                                gd.GetComponent<InputManager>().RemoveOtherControllers();
                                break;
                            case "Options":
                                ChangeMenu(MenuState.Options);
                                break;
                            case "Credits":
                                moveTitle = true;
                                ChangeMenu(MenuState.Credits);
                                StartCoroutine(ForcePicRemove());
                                GetComponent<Credits>().enabled = true;
                                GetComponent<Credits>().StartCredits();
                                break;
                            case "Quit":
                                Application.Quit();
                                break;
                        }
                    break;
                    case MenuState.SinglePlayer:
                        switch (options[currentSelection])
                        {
                            case "Tournament":
                                //gd.GetComponent(Level_Select).GrandPrixOnly = true;
                                //gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "VS Race":
                                //gd.GetComponent(Level_Select).GrandPrixOnly = false;
                                //gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "Time Trial":                              
                                //gd.GetComponent(Level_Select).GrandPrixOnly = false;
                                //gd.GetComponent(RaceLeader).type = RaceStyle.TimeTrial;
                                //gd.difficulty = 1;
                                ChangeMenu(MenuState.CharacterSelect);
                                //StartCoroutine("StartCharacterSelect");
                                break;
                        }
                    break;
                    case MenuState.Multiplayer:
                        switch (options[currentSelection])
                        {
                            case "Tournament":
                                //gd.GetComponent(Level_Select).GrandPrixOnly = true;
                                //gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "VS Race":                         
                                //gd.GetComponent(Level_Select).GrandPrixOnly = false;
                                //gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
                                ChangeMenu(MenuState.Difficulty);
                                break;
                        }
                    break;
                    case MenuState.Difficulty:
                        switch (options[currentSelection])
                        {
                            case "50cc":
                                //gd.difficulty = 0;
                                ChangeMenu(MenuState.CharacterSelect);
                               // StartCoroutine("StartCharacterSelect");
                                break;
                            case "100cc":
                                //gd.difficulty = 1;
                                ChangeMenu(MenuState.CharacterSelect);
                               // StartCoroutine("StartCharacterSelect");
                                break;
                            case "150cc":
                                //gd.difficulty = 2;
                                ChangeMenu(MenuState.CharacterSelect);
                                //StartCoroutine("StartCharacterSelect");
                                break;
                            case "Insane":
                                //gd.difficulty = 3;
                                ChangeMenu(MenuState.CharacterSelect);
                                //StartCoroutine("StartCharacterSelect");
                                break;
                        }
                    break;
                    case MenuState.Popup:
                        if (submitBool)
                            BackMenu();
                    break;
                }
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
            if(state == MenuState.Credits && transform.GetComponent<Credits>().enabled)
            {
                transform.GetComponent<Credits>().StartCoroutine("StopCredits");
                lockPicture = false;
            }

            StartCoroutine("ChangeMenuPhysical", backStates[backStates.Count - 1]);
            backStates.RemoveAt(backStates.Count - 1);
        }
    }

    public void ChangeMenu(MenuState changeState)
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

            float actualWidth = Mathf.Clamp(GUIHelper.width * (Screen.height / GUIHelper.height), 0, Screen.width);
            float ratio = Screen.width / actualWidth;
            float endSideAmount = -(GUIHelper.width * ratio)/2f;

            //Slide Off //////////////////////////////
            sideAmount = 0f;

            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(0f, endSideAmount, (Time.time - startTime) / travelTime);
                yield return null;
            }

            //Pause at Top///////////////////////////////////////
            sideAmount = endSideAmount;
            currentSelection = 0;
            state = changeState;

            if(state == MenuState.Main)
            {
                moveTitle = false;
            }

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(endSideAmount, 0f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            sideAmount = 0f;

            sliding = false;
        }
    }

    IEnumerator ChangePicture(Texture2D texture)
    {
        if (!pictureTransitioning && !lockPicture)
        {
            pictureTransitioning = true;

            float startTime = Time.time;
            float travelTime = 0.35f;

            float actualWidth = Mathf.Clamp(GUIHelper.width * (Screen.height / GUIHelper.height), 0, Screen.width);
            float ratio = Screen.width / actualWidth;
            float endSideAmount = GUIHelper.width * ratio;

            while (Time.time - startTime < travelTime)
            {
                sidePictureAmount = Mathf.Lerp(GUIHelper.width / 2f, endSideAmount, (Time.time - startTime) / travelTime);
                yield return null;
            }

            sidePicture = texture;
            sidePictureAmount = endSideAmount;

            if (!lockPicture)
            {
                startTime = Time.time;
                while (Time.time - startTime < travelTime)
                {
                    sidePictureAmount = Mathf.Lerp(endSideAmount, GUIHelper.width / 2f, (Time.time - startTime) / travelTime);
                    yield return null;
                }

                sidePictureAmount = GUIHelper.width / 2f;
            }

            pictureTransitioning = false;
        }
    }

    IEnumerator ForcePicRemove()
    {
        StopCoroutine("ChangePicture");

        pictureTransitioning = true;
        lockPicture = true;

        float startTime = Time.time;
        float startX = sidePictureAmount; 

        float actualWidth = Mathf.Clamp(GUIHelper.width * (Screen.height / GUIHelper.height), 0, Screen.width);
        float ratio = Screen.width / actualWidth;
        float endSideAmount = GUIHelper.width * ratio;

        float travelTime = 0.35f * (startX / endSideAmount);

        while (Time.time - startTime < travelTime)
        {
            sidePictureAmount = Mathf.Lerp(startX, endSideAmount, (Time.time - startTime) / travelTime);
            yield return null;
        }

        sidePicture = null;
        sidePictureAmount = endSideAmount;

        pictureTransitioning = false;
    }
}

