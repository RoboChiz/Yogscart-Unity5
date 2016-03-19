using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{

    CurrentGameData gd;
    SoundManager sm;

    public GUISkin skin;

    public Texture2D logo;
    public AudioClip menuMusic;

    public Color selectedColor;
    int currentSelection;

    public Texture2D sidePicture;
    private float sidePictureAmount = 0;
    private float sidePictureFade = 1f;
    private bool pictureTransitioning = false, lockPicture = false;

    private bool moveTitle = false;


    [HideInInspector]
    public bool sliding = false;

    List<MenuState> backStates = new List<MenuState>();

    static public float SideAmount
    {
        get { return sideAmount; }
        set { }
    }
    static private float sideAmount = 0f;

    private float sideFade = 1f;
    private bool fadeTitle = false, showTitle = false;

    private float titleAmount = 10f;
    int randomImage;

    private string popupText = "";

    public enum MenuState {Start, Main, SinglePlayer, Difficulty, CharacterSelect, LevelSelect, Multiplayer, Online, Options, Popup, Credits };
    public MenuState state = MenuState.Start;

	// Use this for initialization
	IEnumerator Start ()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        sm = GameObject.FindObjectOfType<SoundManager>();

        //Update as more characters are added
        randomImage = Random.Range(0, 1);

        sidePicture = Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + randomImage.ToString());
        sidePictureAmount = GUIHelper.width / 2f;

        yield return new WaitForSeconds(0.5f);

        CurrentGameData.blackOut = false;
        InputManager.allowedToChange = true;

        if (menuMusic != null)
            sm.PlayMusic(menuMusic);

    }
	
	// Update is called once per GUI Frame
	void OnGUI()
    {

        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        string[] options = new string[] { };

        Color nGUI = Color.white;
        nGUI.a = sidePictureFade;
        GUI.color = nGUI;

        if (sidePicture != null)
            GUI.DrawTexture(new Rect(sidePictureAmount, 40, GUIHelper.width / 2f - 30, GUIHelper.height - 20), sidePicture, ScaleMode.ScaleToFit);

        GUI.color = Color.white;
        nGUI.a = sideFade;

        if (showTitle)
        {
            if (fadeTitle)
                GUI.color = nGUI;

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
        }

        GUI.color = nGUI;

        //Draw Stuff
        Rect box = new Rect(sideAmount, 0, GUIHelper.width / 2f, GUIHelper.height);
        float optionHeight = GUI.skin.label.fontSize + 10;
        switch (state)
        {
            case MenuState.Start:
                GUI.Label(new Rect(sideAmount + GUIHelper.width / 8f - 100, 680, 960, 400), "Press Start / Enter!");
                showTitle = true;
                break;
            case MenuState.Main:
                options = new string[] { "Single Player", "Multiplayer", "Online", "Options", "Credits", "Quit" };
                showTitle = true;
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
                showTitle = true;
                options = new string[] { "Tournament", "VS Race", "Time Trial" };
                InputManager.allowedToChange = false;
            break;
            case MenuState.Multiplayer:
                showTitle = true;
                options = new string[] { "Tournament", "VS Race" };
                InputManager.allowedToChange = true;
            break;
            case MenuState.Online:
                showTitle = false;
                InputManager.allowedToChange = true;
            break;
            case MenuState.Options:
                showTitle = false;
                options = new string[] { "Nothing Here Yet" };
                InputManager.allowedToChange = true;
            break;
            case MenuState.Difficulty:
                showTitle = true;
                //options = ["50cc - Only for little Babby!","100cc - You mother trucker!","150cc - Oh what big strong muscles!","Insane - Prepare your butts!","Back"];
                if (CurrentGameData.unlockedInsane)
                    options = new string[] { "50cc", "100cc", "150cc", "Insane" };
                else
                    options = new string[] { "50cc", "100cc", "150cc" };
            break;
            case MenuState.Popup:
                showTitle = true;
                options = new string[] { };
                GUI.Label(new Rect(box.x + 40, 20 + (box.height / 4f), box.width - 20, box.height - 20 - (box.height / 4f)), popupText);
             break;
            case MenuState.Credits:
                showTitle = false;
                if (sidePicture != null)
                {
                    StartCoroutine(ChangePicture(null));
                }
                break;
            case MenuState.CharacterSelect:
                showTitle = false;
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
            int vertical = 0, horizontal = 0;
            bool submitBool = false;

            if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect)
            {
                vertical = InputManager.controllers[0].GetMenuInput("MenuVertical");
                horizontal = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
                submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);
            }

            //Menu Navigation
            if (options != null && options.Length > 0)
            {
                currentSelection += vertical;
                if(currentSelection < 0 || currentSelection>= options.Length)
                    currentSelection = MathHelper.NumClamp(currentSelection, 0, options.Length);
            }

            if(state == MenuState.Start && InputManager.controllers[0].GetInput("Submit") != 0)
                        ChangeMenu(MenuState.Main);

            if (submitBool)
            {
                switch (state)
                {
                    case MenuState.Main:
                        switch (options[currentSelection])
                        {
                            case "Single Player":
                                CurrentGameData.currentGamemode = gd.gameObject.AddComponent<Race>();
                                ChangeMenu(MenuState.SinglePlayer);
                                gd.GetComponent<InputManager>().RemoveOtherControllers();
                                break;
                            case "Multiplayer":
                                CurrentGameData.currentGamemode = gd.gameObject.AddComponent<Race>();
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
                                Race temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.GrandPrix;

                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "VS Race":
                                temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.VSRace;

                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "Time Trial":
                                temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.TimeTrial;

                                CurrentGameData.difficulty = 1;
                                moveTitle = true;
                                StartGameMode();
                                break;
                        }
                    break;
                    case MenuState.Multiplayer:
                        switch (options[currentSelection])
                        {
                            case "Tournament":
                                Race temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.GrandPrix;

                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "VS Race":
                                temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.VSRace;

                                ChangeMenu(MenuState.Difficulty);
                                break;
                        }
                    break;
                    case MenuState.Difficulty:
                        switch (options[currentSelection])
                        {
                            case "50cc":
                                CurrentGameData.difficulty = 0;
                                moveTitle = true;
                                StartGameMode();
                                break;
                            case "100cc":
                                CurrentGameData.difficulty = 1;
                                moveTitle = true;
                                StartGameMode();
                                break;
                            case "150cc":
                                CurrentGameData.difficulty = 2;
                                moveTitle = true;
                                StartGameMode();
                                break;
                            case "Insane":
                                CurrentGameData.difficulty = 3;
                                moveTitle = true;                                
                                StartGameMode();
                                break;
                        }
                    break;
                    case MenuState.Popup:
                        if (submitBool)
                            BackMenu();
                    break;
                }
            }
            if (InputManager.controllers[0].GetInput("Cancel") != 0 && state != MenuState.CharacterSelect)
            {
                BackMenu();
            }
        }

        GUI.color = Color.white;
    }

    private void StartGameMode()
    {
        ChangeMenu(MenuState.CharacterSelect);
        StartCoroutine(ForcePicRemove());
        CurrentGameData.currentGamemode.StartGameMode();
    }

    //Go Back to Previous Menu
    public void BackMenu()
    {
        if (backStates.Count > 0)
        {

            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/back"));

            if (state == MenuState.Credits && transform.GetComponent<Credits>().enabled)
            {
                transform.GetComponent<Credits>().StartCoroutine("StopCredits");
                lockPicture = false;
            }

            if (state == MenuState.CharacterSelect)
            {
                lockPicture = false;
                StartCoroutine("ChangePicture", Resources.Load<Texture2D>("UI/New Main Menu/Side Images/" + randomImage.ToString()));
            }

            StartCoroutine("ChangeMenuPhysical", backStates[backStates.Count - 1]);
            backStates.RemoveAt(backStates.Count - 1);
        }
    }

    public void ChangeMenu(MenuState changeState)
    { 
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));
        StartCoroutine("ChangeMenuPhysical", changeState);
        backStates.Add(state);
    }

    IEnumerator ChangeMenuPhysical(MenuState changeState)
    {
        if(!sliding)
        {
            sliding = true;

            if (changeState == MenuState.CharacterSelect || changeState == MenuState.Credits)
                fadeTitle = true;
            else
                fadeTitle = false;

            float startTime = Time.time;
            float travelTime = 0.5f;

            float endSideAmount = -GUIHelper.width/2f;

            //Slide Off //////////////////////////////
            sideAmount = 0f;
            sideFade = 1f;

            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(0f, endSideAmount, (Time.time - startTime) / travelTime);
                sideFade = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            //Pause at Top///////////////////////////////////////
            sideAmount = endSideAmount;
            sideFade = 0f;

            if (state == MenuState.CharacterSelect || state == MenuState.Credits)
                fadeTitle = true;

            currentSelection = 0;
            state = changeState;

            if(state != MenuState.Credits && state != MenuState.CharacterSelect)
            {
                moveTitle = false;
            }

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                sideAmount = Mathf.Lerp(endSideAmount, 0f, (Time.time - startTime) / travelTime);
                sideFade = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            sideAmount = 0f;
            sideFade = 1f;

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

            float endSideAmount = GUIHelper.width;

            sidePictureFade = 1f;

            while (Time.time - startTime < travelTime)
            {
                sidePictureAmount = Mathf.Lerp(GUIHelper.width / 2f, endSideAmount, (Time.time - startTime) / travelTime);
                sidePictureFade = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            sidePicture = texture;
            sidePictureAmount = endSideAmount;
            sidePictureFade = 0f;

            if (!lockPicture)
            {
                startTime = Time.time;
                while (Time.time - startTime < travelTime)
                {
                    sidePictureAmount = Mathf.Lerp(endSideAmount, GUIHelper.width / 2f, (Time.time - startTime) / travelTime);
                    sidePictureFade = Mathf.Lerp(0f,1f, (Time.time - startTime) / travelTime);
                    yield return null;
                }

                sidePictureAmount = GUIHelper.width / 2f;
                sidePictureFade = 1f;
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

        float endSideAmount = GUIHelper.width;

        float travelTime = 0.35f * (startX / endSideAmount);

        while (Time.time - startTime < travelTime)
        {
            sidePictureAmount = Mathf.Lerp(startX, endSideAmount, (Time.time - startTime) / travelTime);
            sidePictureFade = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
            yield return null;
        }

        sidePicture = null;
        sidePictureAmount = endSideAmount;
        sidePictureFade = 0f;

        pictureTransitioning = false;
    }
}

