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

    private int currentSelection = 0, lastCurrentSelection = -1, lastloadedPicture = -1;

    public Texture2D sidePicture;
    public float sidePicAlpha = 1f, sidePicX = 0f;
    private int sidePicMoving = 0;
    private bool loadPicture = false, hidePicture = false;

    private bool moveTitle = false;

    [HideInInspector]
    public bool sliding = false;

    public List<MenuState> backStates = new List<MenuState>();
    public List<int> backSelection = new List<int>();

    public float SideAmount
    {
        get { return sideAmount; }
        set { }
    }
    private float sideAmount = 0f;

    private float guiAlpha = 1f, titleAlpha = 1f, mouseWait = 0f, nextAlpha = 0f, backAlpha = 0f;

    private float titleAmount = 10f;
    int randomImage;

    private string popupText = "";

    public enum MenuState {Start, Main, SinglePlayer, Difficulty, Multiplayer, CharacterSelect, LevelSelect, Online, Options, Popup, Credits };
    public MenuState state = MenuState.Start;
    private MenuState nextState = MenuState.Start;

    private float[] optionSizes;

    public static bool lockInputs = false;

    private bool mouseLastUsed = false;
    private Vector2 lastMousePos;

	// Use this for initialization
	IEnumerator Start ()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        //Update as more characters are added
        randomImage = Random.Range(0, 1);
        
        string randoImage = "UI/New Main Menu/Side Images/" + randomImage.ToString();
        sidePicture = Resources.Load<Texture2D>(randoImage);
        lastloadedPicture = 0;

        yield return new WaitForSeconds(0.5f);

        CurrentGameData.blackOut = false;
        InputManager.allowedToChange = true;

        if (menuMusic != null)
            sm.PlayMusic(menuMusic);

        lastMousePos = GUIHelper.GetMousePosition();

        InputManager.lockEverything = false;

    }
	
	// Update is called once per GUI Frame
	void OnGUI()
    {

        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        string[] options = new string[] { };

        GUI.color = new Color(1, 1, 1, sidePicAlpha);

        if (sidePicture != null)
            GUI.DrawTexture(new Rect((GUIHelper.width / 2f) + sidePicX, 40, GUIHelper.width / 2f - 30, GUIHelper.height - 20), sidePicture, ScaleMode.ScaleToFit);

        GUI.color = new Color(1, 1, 1, titleAlpha);

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

        //Draw Next and Back Button
        if (nextState == MenuState.Credits || nextState == MenuState.Online || nextState == MenuState.CharacterSelect || nextState == MenuState.LevelSelect || nextState == MenuState.Options || nextState == MenuState.Start)
        {
            if (nextAlpha > 0f)
                nextAlpha -= Time.deltaTime * 2f;
            else
                nextAlpha = 0f;
        }
        else if(state != MenuState.Credits && state != MenuState.Online && state != MenuState.CharacterSelect && state != MenuState.LevelSelect && state != MenuState.Options && state != MenuState.Start)
        {           
            if (nextAlpha < 1f)
                nextAlpha += Time.deltaTime * 2f;
            else
                nextAlpha = 1f;
        }

        //If Next Button Click
        bool nextButtonPressed = GUIHelper.DrawNext(nextAlpha);
        NetworkGUI ng = FindObjectOfType<NetworkGUI>();

        if (nextState == MenuState.Start || nextState == MenuState.Popup || ng.nextState != NetworkGUI.ServerState.ServerList)
        {
            if (backAlpha > 0f)
                backAlpha -= Time.deltaTime * 2f;
            else
                backAlpha = 0f;
        }
        else if (state != MenuState.Start && state != MenuState.Popup && (ng.state == NetworkGUI.ServerState.ServerList || (ng.nextState == NetworkGUI.ServerState.ServerList && ng.state == NetworkGUI.ServerState.Connecting)))
        {
            if (backAlpha < 1f)
                backAlpha += Time.deltaTime * 2f;
            else
                backAlpha = 1f;
        }

        //If Back Button Click
        if (state != MenuState.Options)
        {
            if (GUIHelper.DrawBack(backAlpha))
            {
                if (state == MenuState.CharacterSelect)
                    FindObjectOfType<CharacterSelect>().Back(0);
                else if (state == MenuState.LevelSelect)
                    FindObjectOfType<LevelSelect>().CancelLevelSelect();

                if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect)
                    BackMenu();
            }
        }

        GUIHelper.SetGUIAlpha(guiAlpha);

        //Draw Stuff
        Rect box = new Rect(sideAmount, 0, GUIHelper.width / 2f, GUIHelper.height);

        Vector2 newMousePos = GUIHelper.GetMousePosition();
        string randoImage = "UI/New Main Menu/Side Images/" + randomImage.ToString();
        string[] possibleSideImages = new string[] { randoImage, "UI/New Main Menu/Side Images/Multiplayer", "UI/New Main Menu/Side Images/Online", "UI/New Main Menu/Side Images/Options", randoImage, randoImage };

        if ((lastMousePos - newMousePos).sqrMagnitude > 10)
        {
            mouseLastUsed = true;
            lastMousePos = newMousePos;
        }

        if (state == MenuState.Main || state == MenuState.Start)
        {

            if (currentSelection != lastCurrentSelection)
            {
                mouseWait = 1f;

                if (lastCurrentSelection < 0 || possibleSideImages[currentSelection] != possibleSideImages[lastCurrentSelection])
                    loadPicture = true;

                lastCurrentSelection = currentSelection;
            }

            if (mouseWait > 0f)
            {
                mouseWait -= Time.deltaTime;
            }
            else if (loadPicture)
            {
                loadPicture = false;

                if(lastloadedPicture < 0 || possibleSideImages[currentSelection] != possibleSideImages[lastloadedPicture])
                    StartCoroutine(ActualChangePicture(Resources.Load<Texture2D>(possibleSideImages[currentSelection])));

                lastloadedPicture = currentSelection;
            }
                
        }
       
        switch (state)
        {
            case MenuState.Start:
             
                if (optionSizes == null || optionSizes.Length != 1)
                    optionSizes = new float[] { 1f };

                Rect startRect = GUIHelper.CentreRectLabel(new Rect(sideAmount + GUIHelper.width / 8f - 100, 700, 600, 100), optionSizes[0], "Press Start / Enter!", Color.white);
                if (mouseLastUsed)
                    optionSizes[0] = GUIHelper.SizeHover(startRect, optionSizes[0],1f,1.25f,4f);            

                if (!sliding && GUI.Button(startRect, ""))
                {
                    ChangeMenu(MenuState.Main);
                }

                GUI.Label(new Rect(210, 1010 - (sideAmount/4f), 1900, 60), gd.version);

                break;
            case MenuState.Main:
                options = new string[] { "Single Player", "Multiplayer", "Online", "Options", "Credits", "Quit" };
                InputManager.allowedToChange = true;              

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
                InputManager.allowedToChange = false;
            break;
            case MenuState.Options:            
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
                break;
            case MenuState.CharacterSelect:
                break;
        }

        if (state != MenuState.Start && (optionSizes == null || optionSizes.Length != options.Length))
        {
            optionSizes = new float[options.Length];

            for (int i = 0; i < optionSizes.Length; i++)
                optionSizes[i] = 1f;
        }


        float optionHeight = (box.height * (2/3f))/8f;
        bool mouseClick = false;

        GUI.BeginGroup(box);
            if (options != null && options.Length > 0)
            {
                //Single Player is the longest word in the menu and is 13 characters long			
                for (int i = 0; i < options.Length; i++)
				{
                if (currentSelection == i)
                {
                    if (optionSizes[i] < 1.5f)
                        optionSizes[i] += Time.unscaledDeltaTime * 4f;
                    else
                        optionSizes[i] = 1.5f;
                }
                else
                {
                    if (optionSizes[i] > 1)
                        optionSizes[i] -= Time.unscaledDeltaTime * 4f;
                    else
                        optionSizes[i] = 1f;
                }
                
                Rect optionRect = GUIHelper.LeftRectLabel(new Rect(40, 20 + (box.height / 3f) + (i * optionHeight), box.width - 20, optionHeight - 20), optionSizes[i], options[i], (currentSelection == i) ? Color.yellow : Color.white);

                if (!sliding)
                {
                    if (mouseLastUsed && optionRect.Contains(newMousePos))
                        currentSelection = i;

                    if (GUI.Button(optionRect, ""))
                    {
                        currentSelection = i;
                        mouseClick = true;
                    }
                }
                }
            }
        GUI.EndGroup();

        //Handle Inputs
        if (!sliding && InputManager.controllers.Count > 0)
        {
            int vertical = 0, horizontal = 0;
            bool submitBool = false;

            if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect && state != MenuState.Options && !lockInputs)
            {
                vertical = InputManager.controllers[0].GetMenuInput("MenuVertical");
                horizontal = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
                submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);

                if (vertical != 0 || horizontal != 0 || submitBool)
                    mouseLastUsed = false;

                if (nextButtonPressed)
                    submitBool = nextButtonPressed;
            }

            //Menu Navigation
            if (options != null && options.Length > 0)
            {
                currentSelection += vertical;
                if(currentSelection < 0 || currentSelection>= options.Length)
                    currentSelection = MathHelper.NumClamp(currentSelection, 0, options.Length);
            }

            if(state == MenuState.Start && !lockInputs && InputManager.controllers[0].GetInput("Submit") != 0)
                        ChangeMenu(MenuState.Main);

            if (submitBool || mouseClick)
            {

                if (lastloadedPicture < 0 ||(state == MenuState.Main && possibleSideImages[currentSelection] != possibleSideImages[lastloadedPicture]))
                    StartCoroutine(ActualChangePicture(Resources.Load<Texture2D>(possibleSideImages[currentSelection])));

                Race temp;

                switch (state)
                {
                    case MenuState.Main:
                        switch (options[currentSelection])
                        {
                            case "Single Player":
                                if (CurrentGameData.currentGamemode != null)
                                    DestroyImmediate(CurrentGameData.currentGamemode);

                                CurrentGameData.currentGamemode = gd.gameObject.AddComponent<Race>();
                                ChangeMenu(MenuState.SinglePlayer);
                                gd.GetComponent<InputManager>().RemoveOtherControllers();
                                break;
                            case "Multiplayer":
                                if (CurrentGameData.currentGamemode != null)
                                    DestroyImmediate(CurrentGameData.currentGamemode);

                                CurrentGameData.currentGamemode = gd.gameObject.AddComponent<Race>();
                                ChangeMenu(MenuState.Multiplayer);
                                break;
                            case "Online":
                                ChangeMenu(MenuState.Online);
                                gd.GetComponent<InputManager>().RemoveOtherControllers();
                                FindObjectOfType<NetworkGUI>().enabled = true;
                                FindObjectOfType<NetworkGUI>().ShowMenu();
                                break;
                            case "Options":
                                ChangeMenu(MenuState.Options);
                                FindObjectOfType<Options>().enabled = true;
                                FindObjectOfType<Options>().ShowOptions();
                                break;
                            case "Credits":
                                moveTitle = true;
                                ChangeMenu(MenuState.Credits);
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
                           /* case "Tournament":
                                temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.GrandPrix;

                                ChangeMenu(MenuState.Difficulty);
                                break;
                            case "VS Race":
                                temp = (Race)CurrentGameData.currentGamemode;
                                temp.raceType = RaceType.VSRace;

                                ChangeMenu(MenuState.Difficulty);
                                break;*/
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
                        /*
                       switch (options[currentSelection])
                       {
                            case "Tournament":
                               temp = (Race)CurrentGameData.currentGamemode;
                               temp.raceType = RaceType.GrandPrix;

                               ChangeMenu(MenuState.Difficulty);
                               break;
                           case "VS Race":
                               temp = (Race)CurrentGameData.currentGamemode;
                               temp.raceType = RaceType.VSRace;

                               ChangeMenu(MenuState.Difficulty);
                               break;
                       }*/
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
            if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect && state != MenuState.Options && !lockInputs && InputManager.controllers[0].GetMenuInput("Cancel") != 0)
            {
                BackMenu();
            }
        }
        GUI.color = Color.white;
    }

    private void StartGameMode()
    {
        ChangeMenu(MenuState.CharacterSelect);
        CurrentGameData.currentGamemode.StartGameMode();
    }

    //Go Back to Previous Menu
    public void BackMenu()
    {
        if (!sliding && backStates.Count > 0)
        {
            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/back"));

            if (state == MenuState.Credits && GetComponent<Credits>().enabled)
            {
                GetComponent<Credits>().StartCoroutine("StopCredits");
            }

            if(state == MenuState.Online && FindObjectOfType<NetworkGUI>().enabled)
            {
                FindObjectOfType<NetworkGUI>().CloseMenu();
            }

            if (state == MenuState.Options && FindObjectOfType<Options>().enabled)
            {
                FindObjectOfType<Options>().HideOptions();
            }

            StartCoroutine(ChangeMenuPhysical(backStates[backStates.Count - 1],true));

            backStates.RemoveAt(backStates.Count - 1);
           
        }
    }

    public void ChangeMenu(MenuState changeState)
    { 
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));

        backStates.Add(state);

        if(state != MenuState.Start)
            backSelection.Add(currentSelection);

        StartCoroutine(ChangeMenuPhysical(changeState, false));      
    }

    IEnumerator ChangeMenuPhysical(MenuState changeState, bool backing)
    {
        if(!sliding)
        {
            sliding = true;
            nextState = changeState;

            float startTime = Time.time;
            float travelTime = 0.5f;

            float endSideAmount = -GUIHelper.width/2f;

            if (changeState == MenuState.Credits || changeState == MenuState.CharacterSelect || changeState == MenuState.Online || changeState == MenuState.Options || changeState == MenuState.LevelSelect)
            {
                HideTitle();
                HidePicture();
            }

            //Slide Off //////////////////////////////
            if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect)//Nothing to fade out
            {
                sideAmount = 0f;
                guiAlpha = 1f;

                while (Time.time - startTime < travelTime)
                {
                    sideAmount = Mathf.Lerp(0f, endSideAmount, (Time.time - startTime) / travelTime);
                    guiAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
                    yield return null;
                }
            }

            //Pause at Top///////////////////////////////////////
            sideAmount = endSideAmount;
            guiAlpha = 0f;

            if (state != MenuState.Start && changeState != MenuState.Start && !backing)
                currentSelection = 0;

            if (backing && backSelection.Count > 1)
            {
                currentSelection = backSelection[backSelection.Count - 1];
                backSelection.RemoveAt(backSelection.Count - 1);
            }

            state = changeState;

            //Show Title of Hidden
            if (state != MenuState.Credits && state != MenuState.CharacterSelect && changeState != MenuState.Online && changeState != MenuState.Options && changeState != MenuState.LevelSelect && titleAlpha != 1)
            {
                ShowTitle();
                ShowPicture();
            }

            if (state != MenuState.Credits && state != MenuState.CharacterSelect && changeState != MenuState.Online && changeState != MenuState.Options && changeState != MenuState.LevelSelect)
            {
                moveTitle = false;
            }

            //Slide Down/////////////////////
            if (state != MenuState.CharacterSelect && state != MenuState.LevelSelect)//Nothing to fade in
            {
                startTime = Time.time;
                while (Time.time - startTime < travelTime)
                {
                    sideAmount = Mathf.Lerp(endSideAmount, 0f, (Time.time - startTime) / travelTime);
                    guiAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
                    yield return null;
                }

                sideAmount = 0f;
                guiAlpha = 1f;
            }

            sliding = false;
        }
    }

    private const float fadeTime = 0.5f;

    public void ShowTitle() {StartCoroutine(TitleFade(0f,1f));}
    public void HideTitle() {StartCoroutine(TitleFade(1f,0f)); }

    private IEnumerator TitleFade(float start, float finish)
    {
        if (titleAlpha != finish)
        {
            float startTime = Time.time;
            while (Time.time - startTime < fadeTime)
            {
                titleAlpha = Mathf.Lerp(start, finish, (Time.time - startTime) / fadeTime);
                yield return null;
            }

            titleAlpha = finish;
        }
    }

    private IEnumerator ActualChangePicture(Texture2D nTexture)
    {
        if (nTexture != sidePicture)
        {
            int myFunction = sidePicMoving + 1;
            yield return StartCoroutine(TransitionPicture(960, 0f));

            if (sidePicMoving == myFunction && !hidePicture)
            {
                sidePicture = nTexture;
                yield return StartCoroutine(TransitionPicture(0, 1f));
            }
        }
    }

    public void ShowPicture()
    {
        //If Hiding wait till end of Hiding
        StartCoroutine(TransitionPicture(0, 1f));
        hidePicture = false;
    }

    public void HidePicture()
    {
        StartCoroutine(TransitionPicture(960f, 0f));
        hidePicture = true;
    }

    private IEnumerator TransitionPicture(float endX, float endAlpha)
    {
        sidePicMoving ++;
        int myFunction = sidePicMoving;

        //Slide Picture Off
        float travelTime = 0.4f * Mathf.Abs((endX - sidePicX)/ 960f), startTime = Time.time, startX = sidePicX, startAlpha = sidePicAlpha;

        while (Time.time - startTime < travelTime && sidePicMoving == myFunction)
        {
            sidePicX = Mathf.Lerp(startX, endX, (Time.time - startTime) / travelTime);
            sidePicAlpha = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / travelTime);
            yield return null;
        }

        if (sidePicMoving == myFunction)
        {
            sidePicX = endX;
            sidePicAlpha = endAlpha;
        }
    }

    public void ReturnFromGame()
    {
        state = MenuState.Main;
        backStates.Add(MenuState.Start);
    }
}

