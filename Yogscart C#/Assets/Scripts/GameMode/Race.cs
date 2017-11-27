using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/*
    Races Class v2.0
    Created by Robert (Robo_Chiz)
    FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
    Thanks
*/
public abstract class Race : GameMode
{
    //What cup the current level is in
    public int currentCup { get; protected set; }
    //The current level we are racing on
    public int currentTrack { get; protected set; }
    //The variation of the current level
    public int currentVariation { get; protected set; }

    //How many races have taken place
    public int currentRace { get; protected set; }

    //Race children must say wheter we want AI
    protected abstract bool enableAI { get; }

    //List of Racers as they were (Used for Replays)
    protected List<ReplayRacer> preRaceState;

    protected bool raceFinished = false;
    protected int racersFinished = 0;
    protected bool lastLap = false;

    protected MapViewer mapViewer;
    protected TrackData td;

    public enum RaceState { Blank, CutScene, RaceInfo, Countdown, RaceGUI, ScoreBoard, NextMenu, Win, LevelSelect };
    public RaceState currentState { get; protected set; }
    protected bool changingState = false;

    //GUI Stuff
    private GUISkin skin;
    protected float guiAlpha = 0f;
    protected string raceName;
    protected Texture2D boardTexture;

    protected int nextMenuSelected = 0;
    protected string[] nextMenuOptions;
    private float[] nextMenuSizes;

    protected Coroutine currentGame;
    protected bool lockInputs = false;
    protected bool skipCS = false;

    public override void StartGameMode()
    {
        StartGameMode(false);
    }
    public void StartGameMode(bool _skipCS)
    {
        //Get important stuff
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        //Create a Map Viewer
        if (GetComponent<MapViewer>() == null)
        {
            mapViewer = gameObject.AddComponent<MapViewer>();
            mapViewer.HideMapViewer();
        }

        //Setup AI
        aiEnabled = enableAI;

        //Reset Stuff
        currentTrack = -1;
        currentCup = -1;
        currentVariation = -1;
        currentRace = 0;

        skin = Resources.Load<GUISkin>("GUISkins/Race");
        boardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");

        skipCS = _skipCS;

        StartCoroutine(ActualStartGameMode());
    }

    protected virtual IEnumerator ActualStartGameMode()
    {
        //Do Character Select
        yield return DoCharacterAndTrackSelect(skipCS);

        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        //Setup the Racers for the Gamemode
        SetupRacers();

        yield return null;
        yield return null;
        yield return null;

        StartRace();

    }

    protected IEnumerator DoCharacterAndTrackSelect()
    {
        yield return DoCharacterAndTrackSelect(false);
    }

    protected IEnumerator DoCharacterAndTrackSelect(bool skipCS)
    {
        CharacterSelect cs = FindObjectOfType<CharacterSelect>();
        bool firstTime = true;

        //Loop until Player has selected a track!
        while (currentTrack == -1 || currentCup == -1)
        {
            if (!skipCS)
            {
                cs.enabled = true;

                //First time so load first character select menu
                if (firstTime)
                {
                    firstTime = false;
                    yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);
                }
                else
                {
                    //Obviously returning from level select so go straight to kart
                    yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Kart);
                }

                //Wait for fade
                yield return new WaitForSeconds(0.5f);

                //Wait until all characters have been selected
                while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
                {
                    yield return null;
                }
            }
            else
            {
                skipCS = false;
                firstTime = false;
            }

            MainMenu mm = FindObjectOfType<MainMenu>();

            if (cs.State == CharacterSelect.csState.Off)
            {
                //Cancel everything!               
                if (mm != null)
                {
                    mm.BackMenu();
                }

                Debug.Log("It didn't worked");

                //Clean Up
                Destroy(mapViewer);

                //Stop all Gamemode Coroutines
                ForceStop();

                //Wait a Frame for Coroutines to stop
                yield return null;

            }

            //Everything worked out perfect!
            Debug.Log("It worked");
            LevelSelect ls = FindObjectOfType<LevelSelect>();

            if (ls != null)
            {
                ls.enabled = true;
                ls.ShowLevelSelect();
            }

            while (ls.enabled)
            {
                yield return null;
            }

            if (currentCup == -1 || currentTrack == -1)
            {
                Debug.Log("Back out of Level Select");
                if (mm != null)
                    mm.BackMenu();
            }
            else
            {
                Debug.Log("Beaten the Level Select!");
                Debug.Log("Cup:" + currentCup + " Track:" + currentTrack + " Varation:" + currentVariation);

                InputManager.SetInputState(InputManager.InputState.Locked);
                InputManager.SetToggleState(InputManager.ToggleState.Locked);
            }
        }
    }

    protected void StartRace()
    {
        currentGame = StartCoroutine(DoRace());
    }

    private IEnumerator DoRace()
    {
        //Force Screen to black out
        while (!gd.isBlackedOut)
        {
            CurrentGameData.blackOut = true;
            yield return null;
        }

        //Tidy Up
        foreach (Racer r in racers)
        {
            r.finished = false;
            r.currentPercent = 0;
            r.lap = 0;
            r.timer = 0;
        }

        raceFinished = false;
        racersFinished = 0;
        lastLap = false;

        Replay replay = FindObjectOfType<Replay>();
        if (replay != null)
            Destroy(replay);

        //Set static values for Karts
        KartMovement.raceStarted = false;
        KartMovement.beQuiet = true;

        //Change Pitch Back
        FindObjectOfType<SoundManager>().SetMusicPitch(1f);

        //Load the Level
        AsyncOperation sync = SceneManager.LoadSceneAsync(gd.tournaments[currentCup].tracks[currentTrack].sceneIDs[currentVariation]);

        while (!sync.isDone)
            yield return null;

        //Let each gamemode do it's thing
        OnLevelLoad();

        //Load the Track Manager
        td = FindObjectOfType<TrackData>();

        //Save current state of racers for replay
        preRaceState = new List<ReplayRacer>();
        foreach (Racer racer in racers)
            preRaceState.Add(new ReplayRacer(racer));

        //Spawn Karts
        OnSpawnKart();

        //Add Racer Specific Components
        foreach(Racer racer in racers)
        {
            if(racer.Human != -1)
            {
                KartInfo ki = racer.ingameObj.gameObject.AddComponent<KartInfo>();
                ki.hidden = true;

                Camera[] cameras = new Camera[2];
                cameras[0] = racer.ingameObj.GetComponent<KartInput>().frontCamera;
                cameras[1] = racer.ingameObj.GetComponent<KartInput>().backCamera;

                ki.cameras = cameras;
            }
            else
            {
                AI ai = racer.ingameObj.gameObject.AddComponent<AI>();
                ai.intelligence = (AI.AIStupidity)racer.AiStupidity;
                ai.canDrive = false;
            }

            //Add a Recorder so we can have a replay later
            racer.ingameObj.gameObject.AddComponent<KartRecorder>();
        }

        //Set Up Screen Space
        if (InputManager.controllers.Count == 2)
        {
            racers[racers.Count - 1].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.Top;
            racers[racers.Count - 2].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.Bottom;
        }

        if (InputManager.controllers.Count >= 3)
        {
            racers[racers.Count - 1].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.TopLeft;
            racers[racers.Count - 2].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.TopRight;
            racers[racers.Count - 3].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.BottomLeft;
        }

        if (InputManager.controllers.Count == 4)
            racers[racers.Count - 4].ingameObj.GetComponent<KartInfo>().screenPos = ScreenType.BottomRight;

        yield return new WaitForSeconds(1f);

        //Setup Map Viewer
        mapViewer.objects = new List<MapObject>();

        foreach (Racer racer in racers)
            mapViewer.objects.Add(new MapObject(racer.ingameObj, gd.characters[racer.character].icon, racer.position));

        //Let Gamemode add Map Viewer Objects
        AddMapViewObjects();

        yield return null;

        //Do the intro to the Map
        yield return StartCoroutine(DoIntro());

        //Show what race we're on
        KartMovement.beQuiet = false;
        raceName = GetRaceName();
        yield return ChangeState(RaceState.RaceInfo);

        //Get Kart Components for Race
        KartMovement[] kses = FindObjectsOfType<KartMovement>();
        KartInput[] kines = FindObjectsOfType<KartInput>();
        KartRecorder[] kartRecorders = FindObjectsOfType<KartRecorder>();
        KartInfo[] kies = FindObjectsOfType<KartInfo>();
        KartItem[] kitemes = FindObjectsOfType<KartItem>();

        //Set Kart Components for Race
        foreach (KartInput ki in kines)
            ki.camLocked = false;
        
        foreach (KartRecorder kr in kartRecorders)
            kr.Record();

        //Let Gamemode make changes to karts
        OnPreKartStarting();

        yield return new WaitForSeconds(3f);
       
        foreach (KartInfo ki in kies)
            ki.hidden = false;
   
        foreach (KartItem ki in kitemes)
            ki.hidden = false;

        //Let Gamemode make changes to karts
        OnPostKartStarting();

        //Show Map
        mapViewer.ShowMapViewer();

        //Do the Countdown
        yield return ChangeState(RaceState.Countdown);

        StartCountdown();
        yield return new WaitForSeconds(3.4f);

        //Start the timer
        StartTimer();

        //Unlock the karts      
        foreach (KartMovement ks in kses)
            ks.locked = false;

        foreach (KartItem ki in kitemes)
            ki.locked = false;

        yield return ChangeState(RaceState.RaceGUI);
        yield return null;

        //Unlock the Pause Menu
        PauseMenu.canPause = true;

        //Wait for the gamemode to be over
        while (!raceFinished && timer < 1800f)
        {
            ClientUpdate();

            if (!clientOnly)
                HostUpdate();

            yield return new WaitForSeconds(0.25f);
        }

        //Show Results
        Debug.Log("It's over!");
        finished = true;
        mapViewer.HideMapViewer();

        //Stop the Timer
        StopTimer();

        //Lock the Pause Menu
        PauseMenu.canPause = false;

        //Turn off Kart Components
        foreach (KartInput ki in kines)
            ki.camLocked = false;

        //Send over replay data
        for(int i = 0; i < racers.Count; i++)
            preRaceState[i].ghostData = racers[i].ingameObj.GetComponent<KartRecorder>().actions;

        //Give any operations time to stop
        yield return new WaitForSeconds(1);

        //Setup Leaderboard
        DisplayRacer[] sortedRacers = new DisplayRacer[racers.Count];

        foreach(Racer racer in racers)
        {
            racer.points += 15 - racer.position;
            sortedRacers[racer.position] = new DisplayRacer(racer);

            if(racer.Human != -1)
            {
                gd.overallLapisCount += racer.ingameObj.GetComponent<KartMovement>().lapisAmount;
            }
        }

        SaveDataManager saveDataManager = FindObjectOfType<SaveDataManager>();
        saveDataManager.SetLapisAmount(gd.overallLapisCount);
        saveDataManager.Save();

        //Increment the race count
        currentRace++;

        //Let each Game Mode do it's own thing
        OnRaceFinished();

        yield return new WaitForSeconds(2.5f);

        foreach (KartRecorder kr in kartRecorders)
            kr.Pause();

        StartCoroutine(ChangeState(RaceState.ScoreBoard));

        Leaderboard lb = gameObject.AddComponent<Leaderboard>();
        lb.racers = new List<DisplayRacer>(sortedRacers);

        //Let each Game Mode do it's own thing
        OnStartLeaderBoard(lb);
        lb.hidden = false;

        //Tidy Up
        timer = 0;
        finished = false;
        raceFinished = false;

        while (!lb.showing)
            yield return null;

        //Get Options for Next Menu
        nextMenuOptions = GetNextMenuOptions();
        nextMenuSizes = new float[nextMenuOptions.Length];

        //Wait for Leaderboard to be over
        while (lb.showing)
        {
            OnLeaderboardUpdate(lb);
            yield return null;
        }

        Destroy(lb);

        yield return OnEndLeaderBoard();
    }

    public virtual void OnLevelLoad() { }
    protected abstract void OnSpawnKart();
    protected virtual void AddMapViewObjects() { }
    protected abstract string GetRaceName();
    protected virtual void OnPreKartStarting() { }
    protected virtual void OnPostKartStarting() { }
    protected abstract void OnRaceFinished();
    protected abstract void OnStartLeaderBoard(Leaderboard lb);
    protected abstract void OnLeaderboardUpdate(Leaderboard lb);

    protected virtual IEnumerator OnEndLeaderBoard()
    {
        yield return ChangeState(RaceState.NextMenu);

        while (currentState == RaceState.NextMenu && !changingState)
        {
            if (!lockInputs && InputManager.controllers[0].GetButtonWithLock("Submit"))
                NextMenuSelection();

            int vert = 0;
            if(!lockInputs)
                vert = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");

            if (vert != 0)
                nextMenuSelected = MathHelper.NumClamp(nextMenuSelected + vert, 0, nextMenuOptions.Length);

            yield return null;
        }
    }

    public abstract string[] GetNextMenuOptions();

    public override void EndGamemode()
    {
        currentCup = -1;
        currentTrack = -1;
        currentRace = 1;
        currentVariation = -1;

        Destroy(mapViewer);
        StartCoroutine(QuitGame());
    }

    public override void HostUpdate()
    {
        bool allFinished = true;

       foreach(Racer racer in racers)
        {
            //Update Position Finding
            PositionFinding pf = racer.ingameObj.GetComponent<PositionFinding>();
            racer.currentPercent = pf.currentPercent;
            racer.lap = pf.lap;
            pf.racePosition = racer.position;

            //Finish Player
            if (pf.lap >= td.Laps && !racer.finished)
            {
                racer.finished = true;
                racer.timer = timer;

                PlayerFinished(racer);

                racer.position = racersFinished;
                racersFinished++;

                if (racer.Human >= 0)
                    StartCoroutine(FinishKart(racer));
            }

            //Finish Race
            if (racer.Human != -1 && !racer.finished)
                allFinished = false;

            //Change pitch of music for last lap
            if (pf.lap >= td.Laps - 1 && !lastLap)
            {
                lastLap = true;
                FindObjectOfType<SoundManager>().SetMusicPitch(td.lastLapPitch);
            }
        }

        SortingScript.CalculatePositions(racers);

        if (allFinished)
        {
            raceFinished = true;

            //Change Pitch Back
            FindObjectOfType<SoundManager>().SetMusicPitch(1f);
        }
    }

    protected virtual void PlayerFinished(Racer racer) { }

    protected IEnumerator FinishKart(Racer racer)
    {
        racer.ingameObj.gameObject.AddComponent<AI>();
        Destroy(racer.ingameObj.GetComponent<KartInput>());

        //Hide Kart Item
        if (racer.ingameObj.GetComponent<KartItem>() != null)
        {
            racer.ingameObj.GetComponent<KartItem>().locked = true;
            racer.ingameObj.GetComponent<KartItem>().hidden = true;
        }

        if (racer.ingameObj.GetComponent<KartInfo>() != null)
            racer.ingameObj.GetComponent<KartInfo>().StartCoroutine("Finish");

        if (racer.cameras != null)
        {
            racer.cameras.GetChild(0).GetComponent<Camera>().enabled = false;
            racer.cameras.GetChild(1).GetComponent<Camera>().enabled = true;

            yield return new WaitForSeconds(2f);

            if (racer.ingameObj.GetComponent<KartInfo>() != null)
                racer.ingameObj.GetComponent<KartInfo>().hidden = true;

            float startTime = Time.time;
            const float travelTime = 3f;
            KartCamera kc = racer.cameras.GetChild(1).GetComponent<KartCamera>();

            while (Time.time - startTime < travelTime)
            {
                float percent = (Time.time - startTime) / travelTime;

                kc.angle = Mathf.Lerp(0f, 180f, percent);
                kc.height = Mathf.Lerp(2f, 1f, percent);
                kc.playerHeight = Mathf.Lerp(2f, 1f, percent);
                kc.sideAmount = Mathf.Lerp(0, -1.9f, percent * 4f);

                yield return null;
            }

            kc.angle = 180f;
            kc.height = 1f;
            kc.playerHeight = 1f;
            kc.sideAmount = -1.9f;
        }
    }

    /// <summary>
    /// Increments to the next available race
    /// </summary>
    public abstract void NextRace();

    public virtual void FinishLevelSelect(int _currentCup, int _currentTrack)
    {
        currentTrack = _currentTrack;
        currentCup = _currentCup;

        currentVariation = gd.GetRandomLevelForTrack(currentCup, currentTrack);
    }

    public IEnumerator DoIntro()
    {
        StartCoroutine(ChangeState(RaceState.CutScene));

        //Turn off all camera
        foreach (Camera camera in FindObjectsOfType<Camera>())
            camera.enabled = false;
        foreach (AudioListener listener in FindObjectsOfType<AudioListener>())
            listener.enabled = false;
        KartInput.overrideCamera = true;

        GameObject cutSceneCam = new GameObject();
        cutSceneCam.tag = "MainCamera";

        cutSceneCam.AddComponent<Camera>();
        cutSceneCam.AddComponent < AudioListener>();
        cutSceneCam.tag = "MainCamera";

        sm.PlayMusic(Resources.Load<AudioClip>("Music & Sounds/RaceStart"));

        if (td.introPans != null && td.introPans.Count > 0)
        {
            cutSceneCam.transform.position = td.introPans[0].startPoint;
            cutSceneCam.transform.rotation = Quaternion.Euler(td.introPans[0].startRotation);

            CurrentGameData.blackOut = false;
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < td.introPans.Count; i++)
                yield return StartCoroutine(Play(cutSceneCam.transform, td.introPans[i]));
        }

        StartCoroutine(ChangeState(RaceState.Blank));

        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        cutSceneCam.GetComponent<Camera>().depth = -5f;

        yield return new WaitForSeconds(0.5f);

        //Turn on all cameras
        foreach (Camera camera in FindObjectsOfType<Camera>())
            camera.enabled = true;
        foreach (AudioListener listener in FindObjectsOfType<AudioListener>())
            listener.enabled = true;

        KartInput.overrideCamera = false;

        CurrentGameData.blackOut = false;

        Destroy(cutSceneCam);


        sm.PlayMusic(td.backgroundMusic);
    }

    public IEnumerator Play(Transform cam, CameraPoint clip)
    {
        float startTime = Time.time;

        while ((Time.time - startTime) < clip.travelTime)
        {
            float percent = (Time.time - startTime) / clip.travelTime;
            cam.position = Vector3.Lerp(clip.startPoint, clip.endPoint, percent);
            cam.rotation = Quaternion.Slerp(Quaternion.Euler(clip.startRotation), Quaternion.Euler(clip.endRotation), percent);
            yield return null;
        }

        cam.position = clip.endPoint;
        cam.rotation = Quaternion.Euler(clip.endRotation);
    }

    public IEnumerator ChangeState(RaceState nState)
    {
        if (currentState != nState && !changingState)
        {
            changingState = true;

            yield return FadeAlphaTo(0f);
            
            currentState = nState;

            yield return FadeAlphaTo(1f);

            changingState = false;
        }
    }

    public IEnumerator FadeAlphaTo(float finalVal)
    {
        float startTime = Time.time, startVal = guiAlpha, travelTime = 0.5f;

        while ((Time.time - startTime) < travelTime)
        {
            guiAlpha = Mathf.Lerp(startVal, finalVal, (Time.time - startTime) / travelTime);
            yield return null;
        }
        guiAlpha = finalVal;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        GUI.skin = skin;
        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();

        switch(currentState)
        {
            case RaceState.CutScene:
                //Background
                DrawCheckboard();

                //Track Name
                Texture2D previewTexture = gd.tournaments[currentCup].tracks[currentTrack].preview;
                GUI.DrawTexture(GUIHelper.CentreRect(new Rect(0, 780, 1920, 200), guiAlpha), previewTexture);
                break;
            case RaceState.RaceInfo:
                //Race Name
                GUIHelper.CentreRectLabel(new Rect(0, 780, 1920, 200), guiAlpha, "-  " + raceName + "  -", new Color(1f,1f,1f,guiAlpha));

                break;
            case RaceState.NextMenu:

                //Background
                GUI.DrawTexture(new Rect(990, 100, 900, 880), boardTexture, ScaleMode.ScaleToFit);

                //Render Options
                float halfSize = nextMenuOptions.Length / 2f;

                for(int i = 0; i < nextMenuOptions.Length; i++)
                {
                    Color textColor = new Color(1f, 1f, 1f, guiAlpha);

                    if (i == nextMenuSelected)
                    {
                        textColor = Color.yellow;
                        textColor.a = guiAlpha;

                        nextMenuSizes[i] = Mathf.Clamp(nextMenuSizes[i] + (Time.deltaTime * 5f), 0.7f, 0.9f);
                    }
                    else
                        nextMenuSizes[i] = Mathf.Clamp(nextMenuSizes[i] - (Time.deltaTime * 5f), 0.7f, 0.9f);

                    float labelSize = 800f / (nextMenuOptions.Length + 2);
                    Rect labelRect = new Rect(1000, 550 - (halfSize * labelSize) + (i * labelSize), 880, labelSize - 20);
                    GUIHelper.CentreRectLabel(labelRect, nextMenuSizes[i], nextMenuOptions[i], textColor);

                    if (Cursor.visible && !lockInputs)
                    {
                        if (labelRect.Contains(GUIHelper.GetMousePosition()))
                            nextMenuSelected = i;

                        if (GUI.Button(labelRect, ""))
                        {
                            nextMenuSelected = i;
                            NextMenuSelection();
                        }
                    }
                }

                break;
        }
    }

    public void DrawCheckboard()
    {
        //Background
        Texture2D background = Resources.Load<Texture2D>("UI/Level Selection/Levels/CheckerBoard");
        float time = (Time.time * 0.1f) % 0.5f;

        for (float x = GUIHelper.screenEdges.x; x < GUIHelper.screenEdges.x + GUIHelper.screenEdges.width; x += 400)
        {
            GUI.DrawTextureWithTexCoords(new Rect(x, 780, 400, 200), background, new Rect(0, time, 1f, 0.5f));
        }
    }

    protected virtual void NextMenuSelection()
    {
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));

        switch (nextMenuOptions[nextMenuSelected])
        {
            case "Next Race":
            case "Restart":
                NextRace();
                break;
            case "Replay":
                //Launch Replay GameMode
                Replay replay = gameObject.AddComponent<Replay>();
                replay.Setup(this, preRaceState);
                StartCoroutine(ChangeState(RaceState.Blank));
                break;
            case "Change Character":
                ChangeCharacter();
                break;
            case "Change Track":
                ChangeTrack();
                break;
            case "Quit":
                StartCoroutine(ChangeState(RaceState.Blank));
                EndGamemode();
                break;
        }
    }

    public void ChangeCharacter()
    {
        StartCoroutine(ChangeState(RaceState.Blank));     
        StartCoroutine(ActualChange(false));
    }

    public void ChangeTrack()
    {
        StartCoroutine(ChangeState(RaceState.Blank));      
        StartCoroutine(ActualChange(true));
    }

    private IEnumerator ActualChange(bool skipCharacterSelect)
    {
        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        //Change Pitch Back
        FindObjectOfType<SoundManager>().SetMusicPitch(1f);

        //Load the Level
        AsyncOperation sync = SceneManager.LoadSceneAsync("Main_Menu");

        while (!sync.isDone)
            yield return null;

        if (!skipCharacterSelect)
            FindObjectOfType<MainMenu>().ReturnToCharacterSelect();
        else
            FindObjectOfType<MainMenu>().ReturnToLevelSelect();

        yield return new WaitForSeconds(0.5f);
        StartGameMode(skipCharacterSelect);

        yield return new WaitForSeconds(0.3f);
        CurrentGameData.blackOut = false;
    }

    //Not Called
    public override void ClientUpdate() { }
    public override void OnEndGamemode() { }
    public override GameObject OnServerAddPlayer(NetworkRacer nPlayer, GameObject playerPrefab)
    {
        throw new NotImplementedException();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {

    }
}
