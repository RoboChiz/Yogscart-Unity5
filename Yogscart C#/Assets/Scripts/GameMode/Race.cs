using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public enum RaceType { GrandPrix, VSRace, TimeTrial, Online };
public enum RaceGUI { Blank, CutScene, RaceInfo, Countdown, RaceGUI, ScoreBoard, NextMenu, Win, LevelSelect };

/*
    Races Class v1.0
    Created by Robert (Robo_Chiz)
    FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
    Thanks
*/

public class Race : GameMode
{

    public RaceType raceType;
    public static int currentCup = -1, currentTrack = -1, currentRace = 1, lastcurrentRace;
    private int currentSelection;

    public RaceGUI currentGUI = RaceGUI.Blank;
    public float guiAlpha = 0f, mapAlpha = 0f;
    public bool changingState = false, showMap;

    protected TrackData td;

    private string rankString;
    private int bestPlace;

    protected bool raceFinished = false, lastLap, readyToLevelSelect = false;
    public int finishedCount = 0;

    private MapViewer mapViewer;

    public override void StartGameMode()
    {
        StartCoroutine("actualStartGamemode");
    }

    protected virtual IEnumerator actualStartGamemode()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        readyToLevelSelect = false;

        //Character Select & Level Select
        yield return StartCoroutine("PlayerSetup");

        mapViewer = gameObject.AddComponent<MapViewer>();

        //Setup the Racers for the Gamemode
        SetupRacers();

        yield return null;

        StartCoroutine("StartRace");

        yield return null;
    }

    protected IEnumerator StartRace()
    {
        while (!gd.isBlackedOut)
        {
            CurrentGameData.blackOut = true;
            yield return null;
        }

        //Tidy Up
        foreach (Racer r in racers)
        {
            r.finished = false;
            r.currentDistance = 0;
            r.totalDistance = 0;
            r.timer = 0;
        }

        kartScript.raceStarted = false;
        kartScript.beQuiet = true;

        lastcurrentRace = currentRace;
        showMap = false;

        //Load the Level
        SceneManager.LoadScene(gd.tournaments[currentCup].tracks[currentTrack].sceneID);
        yield return null;

        readyToLevelSelect = true;
        finishedCount = 0;

        //Adjust Gravity depending on difficulty
        Physics.gravity = -Vector3.up * Mathf.Lerp(12f, 17f, CurrentGameData.difficulty / 3f);

        //Get rid of Item Boxes
        if (raceType == RaceType.TimeTrial)
        {
            ItemBox[] itemBoxes = FindObjectsOfType<ItemBox>();

            foreach (ItemBox ib in itemBoxes)
            {
                Destroy(ib.gameObject);
            }
        }

        //Load the Track Manager
        td = FindObjectOfType<TrackData>();

        //Spawn the karts
        if (raceType == RaceType.TimeTrial)
            SpawnLoneKart(td.spawnPoint.position, td.spawnPoint.rotation, 0);
        else
            SpawnAllKarts(td.spawnPoint.position, td.spawnPoint.rotation);

        for (int i = 0; i < racers.Count; i++)
        {
            if (racers[i].Human != -1)
            {
                kartInfo ki = racers[i].ingameObj.gameObject.AddComponent<kartInfo>();
                ki.hidden = true;

                Camera[] cameras = new Camera[2];
                cameras[0] = racers[i].ingameObj.GetComponent<kartInput>().frontCamera;
                cameras[1] = racers[i].ingameObj.GetComponent<kartInput>().backCamera;

                ki.cameras = cameras;

                if (raceType == RaceType.TimeTrial)
                {
                    racers[i].ingameObj.GetComponent<kartItem>().RecieveItem(2);
                }
            }
            else
            {
                AI ai = racers[i].ingameObj.gameObject.AddComponent<AI>();
                ai.intelligence = (AI.AIStupidity)racers[i].AiStupidity;
                ai.canDrive = false;
            }
        }

        if (InputManager.controllers.Count == 2)
        {
            racers[racers.Count - 1].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.Top;
            racers[racers.Count - 2].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.Bottom;
        }

        if (InputManager.controllers.Count >= 3)
        {
            racers[racers.Count - 1].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.TopLeft;
            racers[racers.Count - 2].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.TopRight;
            racers[racers.Count - 3].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.BottomLeft;
        }

        if (InputManager.controllers.Count == 4)
            racers[racers.Count - 4].ingameObj.GetComponent<kartInfo>().screenPos = ScreenType.BottomRight;

        yield return new WaitForSeconds(1f);

        //Setup Map Viewer
        mapViewer.objects = new List<MapObject>();

        foreach (Racer racer in racers)
            mapViewer.objects.Add(new MapObject(racer.ingameObj, gd.characters[racer.Character].icon, racer.position));

        yield return null;

        //Do the intro to the Map
        yield return StartCoroutine("DoIntro");

        //Show what race we're on
        kartScript.beQuiet = false;
        yield return StartCoroutine(ChangeState(RaceGUI.RaceInfo));

        kartInput[] kines = FindObjectsOfType<kartInput>();
        foreach (kartInput ki in kines)
            ki.camLocked = false;

        yield return new WaitForSeconds(3f);

        kartInfo[] kies = FindObjectsOfType<kartInfo>();
        foreach (kartInfo ki in kies)
            ki.hidden = false;

        kartItem[] kitemes = FindObjectsOfType<kartItem>();
        foreach (kartItem ki in kitemes)
            ki.hidden = false;

        //Show Map
        showMap = true;

        yield return StartCoroutine(ChangeState(RaceGUI.Countdown));

        //Do the Countdown
        StartCoroutine("StartCountdown");
        yield return new WaitForSeconds(3.4f);

        StartTimer();

        //Unlock the karts
        kartScript[] kses = FindObjectsOfType<kartScript>();
        foreach (kartScript ks in kses)
            ks.locked = false;

        foreach (kartItem ki in kitemes)
            ki.locked = false;

        //Unlock the Pause Menu
        PauseMenu.onlineGame = false;

        yield return StartCoroutine(ChangeState(RaceGUI.RaceGUI));
        yield return null;

        //Unlock the Pause Menu
        PauseMenu.canPause = true;

        //Wait for the gamemode to be over
        while (!raceFinished && timer < 3600)
        {
            ClientUpdate();

            if (!clientOnly)
                HostUpdate();

            yield return new WaitForSeconds(0.25f);
        }

        //Show Results
        Debug.Log("It's over!");
        finished = true;
        showMap = false;

        StopTimer();

        //Locl the Pause Menu
        PauseMenu.canPause = false;

        foreach (kartInput ki in kines)
            ki.camLocked = false;

        //Give any operations time to stop
        yield return new WaitForSeconds(1);

        DisplayRacer[] sortedRacers = new DisplayRacer[racers.Count];

        while (sortedRacers.Length != racers.Count)
            yield return null;

        for(int i = 0; i < racers.Count; i++)
        {
            Racer r = racers[i];
            r.points += 15 - r.position;

            sortedRacers[r.position] = new DisplayRacer(r);
            sortedRacers[r.position].finished = true;
        }

        if (currentRace == 4 || raceType == RaceType.TimeTrial)
            DetermineWinner();

        yield return new WaitForSeconds(2.5f);

        StartCoroutine(ChangeState(RaceGUI.ScoreBoard));

        Leaderboard lb = gameObject.AddComponent<Leaderboard>();
        lb.racers = new List<DisplayRacer>(sortedRacers);

        if (raceType == RaceType.TimeTrial)
            lb.StartTimeTrial();
        else
            lb.StartLeaderBoard();

        //Tidy Up
        timer = 0;
        finished = false;
        raceFinished = false;

        yield return null;
    }

    //Do Character Select and Level Select
    private IEnumerator PlayerSetup()
    {
        CharacterSelect cs = FindObjectOfType<CharacterSelect>();
        bool firstTime = true;

        //Setup AI
        if (raceType == RaceType.TimeTrial)
            aiEnabled = false;
        else
            aiEnabled = true;

        //Loop until Player has selected a track!
        while (currentTrack == -1 || currentCup == -1)
        {
            cs.enabled = true;

            if (firstTime)//First time so load first character select menu
            {
                firstTime = false;
                yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);
            }
            else//Obviously returning from level select so go straight to kart
            {
                yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Kart);
            }

            //Wait for fade
            yield return new WaitForSeconds(0.5f);

            //Wait until all characters have been selected
            while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
            {
                yield return null;
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
                if(mm != null)
                    mm.BackMenu();
            }
            else
            {
                Debug.Log("Beaten the Level Select!");
                Debug.Log("Cup:" + currentCup + " Track:" + currentTrack);

                CurrentGameData.blackOut = true;

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public IEnumerator DoIntro()
    {

        StartCoroutine(ChangeState(RaceGUI.CutScene));

        GameObject cutSceneCam = new GameObject();
        cutSceneCam.AddComponent<Camera>();
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

        StartCoroutine(ChangeState(RaceGUI.Blank));

        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        cutSceneCam.GetComponent<Camera>().depth = -5f;

        yield return new WaitForSeconds(0.5f);
        CurrentGameData.blackOut = false;

        Destroy(cutSceneCam);
        sm.PlayMusic(td.backgroundMusic);
    }

    public IEnumerator Play(Transform cam, CameraPoint clip)
    {
        float startTime = Time.time;

        while ((Time.time - startTime) < clip.travelTime)
        {
            cam.position = Vector3.Lerp(clip.startPoint, clip.endPoint, (Time.time - startTime) / clip.travelTime);
            cam.rotation = Quaternion.Slerp(Quaternion.Euler(clip.startRotation), Quaternion.Euler(clip.endRotation), (Time.time - startTime) / clip.travelTime);
            yield return null;
        }
    }

    public IEnumerator ChangeState(RaceGUI nState)
    {
        if (currentGUI != nState && !changingState)
        {
            changingState = true;

            float startTime = Time.time;
            const float changeTime = 0.5f;

            while ((Time.time - startTime) < changeTime)
            {
                guiAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / changeTime);
                yield return null;
            }
            guiAlpha = 0;

            startTime = Time.time;
            currentGUI = nState;

            while ((Time.time - startTime) < changeTime)
            {
                guiAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / changeTime);
                yield return null;
            }
            guiAlpha = 1f;

            changingState = false;
        }
    }

    // Update is called once per frame
    public override void OnGUI()
    {
        base.OnGUI();

        GUIHelper.SetGUIAlpha(guiAlpha);

        string[] options;

        switch (currentGUI)
        {
            case RaceGUI.CutScene:

                /*float idealWidth = Screen.width / 3f;
              
                float previewRatio = idealWidth / previewTexture.width;
                Rect previewRect = new Rect(Screen.width - idealWidth - 20, Screen.height - (previewTexture.height * previewRatio * 2f), idealWidth, previewTexture.height * previewRatio);

                GUI.DrawTexture(previewRect, previewTexture);*/

                Matrix4x4 original = GUI.matrix;
                GUI.matrix = GUIHelper.GetMatrix();

                //Background
                Texture2D background = Resources.Load<Texture2D>("UI/Level Selection/Levels/CheckerBoard");
                float time = (Time.time * 0.1f) % 0.5f;

                for (float x = GUIHelper.screenEdges.x; x < GUIHelper.screenEdges.x + GUIHelper.screenEdges.width; x += 400)
                {
                    GUI.DrawTextureWithTexCoords(new Rect(x, 780, 400, 200), background, new Rect(0, time, 1f, 0.5f));
                }

                Texture2D previewTexture = gd.tournaments[currentCup].tracks[currentTrack].preview;
                GUI.DrawTexture(GUIHelper.CentreRect(new Rect(0,780,1920,200), guiAlpha), previewTexture);

                GUI.matrix = original;

                break;
            case RaceGUI.RaceInfo:
                Texture2D raceTexture;

                if (raceType == RaceType.TimeTrial)
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/TimeTrial");
                else if (raceType == RaceType.Online)
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/Online");
                else
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/" + currentRace);

                GUI.DrawTexture(new Rect(10, 10, Screen.width - 20, Screen.height), raceTexture, ScaleMode.ScaleToFit);
                break;
            case RaceGUI.ScoreBoard:

                Leaderboard lb = GetComponent<Leaderboard>();

                if (raceType != RaceType.Online)
                {
                    if (InputManager.controllers[0].GetMenuInput("Submit") != 0 || InputManager.GetClick())
                    {
                        if (lb.state != LBType.AddedPoints && lb.state != LBType.AddingPoints)
                        {
                            StartCoroutine(ChangeState(RaceGUI.NextMenu));
                            lb.hidden = true;
                        }
                        else
                        {
                            lb.DoInput();
                        }
                    }
                }
                break;
            case RaceGUI.NextMenu:

                if (GetComponent<Leaderboard>())
                {
                    Destroy(GetComponent<Leaderboard>());
                }

                Texture2D BoardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");
                Rect BoardRect = new Rect(Screen.width / 2f - Screen.height / 16f, Screen.height / 16f, Screen.width / 2f, (Screen.height / 16f) * 14f);
                GUI.DrawTexture(BoardRect, BoardTexture);

                if (raceType != RaceType.TimeTrial)
                {
                    if (lastcurrentRace + 1 <= 4)
                        options = new string[] { "Next Race", "Quit" };
                    else
                        options = new string[] { "Finish" };
                }
                else
                {
                    options = new string[] { "Restart", "Quit" };
                }

                float IdealHeight = Screen.height / 8f;
                float ratio = IdealHeight / 100f;

                int vert = InputManager.controllers[0].GetMenuInput("MenuVertical");

                if (changingState)
                    vert = 0;

                if (vert != 0)
                    currentSelection -= vert;

                currentSelection = MathHelper.NumClamp(currentSelection, 0, options.Length);

                bool mouseSelecting = false;

                for (int k = 0; k < options.Length; k++)
                {

                    Texture2D optionTexture = Resources.Load<Texture2D>("UI/Next Menu/" + options[k]);
                    Texture2D optionTextureSel = Resources.Load<Texture2D>("UI/Next Menu/" + options[k] + "_Sel");
                    Rect optionRect = new Rect(BoardRect.x + BoardRect.width / 2f - ((300f * ratio) / 2f), BoardRect.y + (IdealHeight * (k + 1)), (300f * ratio), IdealHeight);

                    if (currentSelection == k)
                        GUI.DrawTexture(optionRect, optionTextureSel, ScaleMode.ScaleToFit);
                    else
                        GUI.DrawTexture(optionRect, optionTexture, ScaleMode.ScaleToFit);

                    if (InputManager.MouseIntersects(optionRect))
                    {
                        currentSelection = k;
                        mouseSelecting = true;
                    }
                }

                bool submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);


                if (!changingState && (submitBool || (mouseSelecting && InputManager.GetClick())))
                {
                    sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));

                    switch (options[currentSelection])
                    {
                        case "Quit":
                            StartCoroutine(ChangeState(RaceGUI.Blank));
                            EndGamemode();
                            break;
                        case "Next Race":                         
                            if (raceType == RaceType.GrandPrix)
                            {
                                StartCoroutine(ChangeState(RaceGUI.Blank));
                                StartCoroutine("StartRace");
                                currentTrack++;
                                currentRace++;
                            }
                            else if(raceType == RaceType.VSRace)
                            {
                                StartCoroutine(ChangeState(RaceGUI.LevelSelect));

                                LevelSelect ls = gameObject.AddComponent<LevelSelect>();
                                ls.ShowLevelSelect();
                            }
                            break;
                        case "Restart":
                            StartCoroutine(ChangeState(RaceGUI.Blank));
                            StartCoroutine("StartRace");
                            break;
                        case "Replay":
                            //Not implemented
                            break;
                        case "Finish":
                            StartCoroutine(ChangeState(RaceGUI.Win));
                            break;
                    }
                }

                break;
            case RaceGUI.Win:

                BoardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");
                BoardRect = new Rect(Screen.width / 2f - Screen.height / 16f, Screen.height / 16f, Screen.width / 2f, (Screen.height / 16f) * 14f);
                GUI.DrawTexture(BoardRect, BoardTexture);

                GUI.BeginGroup(BoardRect);

                var lineSize = GUI.skin.label.fontSize + 5;

                GUI.Label(new Rect(10, lineSize, BoardRect.width, lineSize), "Congratulations!");

                string positionString = "You came ";
                positionString += (bestPlace).ToString();

                if (bestPlace == 1)
                    positionString += "st!";
                else if (bestPlace == 2)
                    positionString += "nd!";
                else if (bestPlace == 3)
                    positionString += "rd!";
                else
                    positionString += "th!";

                GUI.Label(new Rect(10, lineSize * 2, BoardRect.width, lineSize), positionString);
                GUI.Label(new Rect(10, lineSize * 3, BoardRect.width, lineSize), "Rank:" + rankString);

                GUI.Label(new Rect(10, lineSize * 4, BoardRect.width, lineSize), "This menu is under Construction!");

                GUI.EndGroup();

                submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);

                if (submitBool)
                {
                    EndGamemode();
                }

                break;
        }

        if (showMap)
            mapAlpha = Mathf.Clamp(mapAlpha + (Time.deltaTime * 2f), 0f, 1f);
        else
            mapAlpha = Mathf.Clamp(mapAlpha - (Time.deltaTime * 2f), 0f, 1f);

        if (mapViewer != null)
        {
            if (mapAlpha > 0f && td.map != null)
            {
                mapViewer.mapAlpha = mapAlpha;

                for (int i = 0; i < racers.Count; i++)
                {
                    mapViewer.objects[i].depth = racers[i].position;
                }
            }
            else
            {
                mapViewer.mapAlpha = 0f;
            }
        }

        GUIHelper.ResetColor();
        GUI.depth = 0;
    }

    public void CancelLevelSelect()
    {
        if (readyToLevelSelect)
        {
            StartCoroutine(WaitforFade());
        }
    }

    public IEnumerator WaitforFade()
    {
        while(changingState)
            yield return null;

        StartCoroutine(ChangeState(RaceGUI.NextMenu));
        StartCoroutine(KillLevelSelect());
    }

    public IEnumerator KillLevelSelect()
    {
        while (GetComponent<LevelSelect>().enabled)
            yield return null;

        Destroy(GetComponent<LevelSelect>());
    }

    public void FinishLevelSelect(int _currentCup, int _currentTrack)
    {
        if (readyToLevelSelect)
        {
            currentTrack = _currentTrack;
            currentCup = _currentCup;

            currentRace++;
       
            StartCoroutine("StartRace");
            StartCoroutine(KillLevelSelect());
        }
    }

    public override void HostUpdate()
    {
        bool allFinished = true;

        for (int i = 0; i < racers.Count; i++)
        {
            PositionFinding pf = racers[i].ingameObj.GetComponent<PositionFinding>();
            racers[i].currentDistance = pf.currentDistance;
            racers[i].totalDistance = pf.currentTotal;
            pf.position = racers[i].position;

            //Finish Player
            if (pf.lap >= td.Laps && !racers[i].finished)
            {
                racers[i].finished = true;
                racers[i].timer = timer;

                racers[i].position = finishedCount;
                finishedCount++;

                if (racers[i].Human >= 0)
                    StartCoroutine(FinishKart(racers[i]));
            }

            //Finish Race
            if (racers[i].Human != -1 && !racers[i].finished)
                allFinished = false;

            //Change pitch of music for last lap
            if(pf.lap >= td.Laps - 1 && !lastLap)
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

    public override void ClientUpdate()
    {

    }

    protected IEnumerator FinishKart(Racer racer)
    {
        racer.ingameObj.gameObject.AddComponent<AI>();
        Destroy(racer.ingameObj.GetComponent<kartInput>());
        //Hide Kart Item
        if (racer.ingameObj.GetComponent<kartItem>() != null)
        {
            racer.ingameObj.GetComponent<kartItem>().locked = true;
            racer.ingameObj.GetComponent<kartItem>().hidden = true;
        }

        racer.ingameObj.GetComponent<kartInfo>().StartCoroutine("Finish");

        racer.cameras.GetChild(0).GetComponent<Camera>().enabled = false;
        racer.cameras.GetChild(1).GetComponent<Camera>().enabled = true;

        yield return new WaitForSeconds(2f);

        racer.ingameObj.GetComponent<kartInfo>().hidden = true;

        float startTime = Time.time;
        const float travelTime = 3f;
        kartCamera kc = racer.cameras.GetChild(1).GetComponent<kartCamera>();

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

    private void DetermineWinner()
    {
        if (raceType != RaceType.TimeTrial)
        {

            List<Racer> pointsSorted = SortingScript.CalculatePoints(racers);

            Racer bestHuman = racers[racers.Count - 1];
            for (int i = 0; i < pointsSorted.Count; i++)
            {
                if (pointsSorted[i].Human != -1)
                {
                    bestHuman = pointsSorted[i];
                    break;
                }
            }

            bestPlace = bestHuman.overallPosition + 1;
            int points = bestHuman.points;
            Rank currentRank = gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty];

            if (points == 60)
            {
                rankString = "Perfect";
                if (raceType == RaceType.GrandPrix)
                    gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty] = Rank.Perfect;
            }
            else if (bestHuman.overallPosition == 0)
            {
                rankString = "Gold";
                if (raceType == RaceType.GrandPrix && currentRank < Rank.Gold)
                    gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty] = Rank.Gold;
            }
            else if (bestHuman.overallPosition == 1)
            {
                rankString = "Silver";
                if (raceType == RaceType.GrandPrix && currentRank < Rank.Silver)
                    gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty] = Rank.Silver;
            }
            else if (bestHuman.overallPosition == 2)
            {
                rankString = "Bronze";
                if (raceType == RaceType.GrandPrix && currentRank < Rank.Bronze)
                    gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty] = Rank.Bronze;
            }
            else
            {
                rankString = "No Rank";
            }
        }

        if (raceType == RaceType.TimeTrial)
        {
            float bestTime = gd.tournaments[currentCup].tracks[currentTrack].bestTime;

            if (racers[0].timer <= bestTime || bestTime == 0)
            {
                gd.tournaments[currentCup].tracks[currentTrack].bestTime = racers[0].timer;

            }
        }

        gd.SaveGame();
    }

    public override void EndGamemode()
    {
        currentCup = -1;
        currentTrack = -1;
        currentRace = 1;
        lastcurrentRace = -1;

        Destroy(mapViewer);

        StartCoroutine(QuitGame());
    }

    //Called when a client disconnects from online gamemode (Not used as Race is Single Player Only)
    public override void OnServerDisconnect(NetworkConnection conn) { }
    //Called when a client connects to online gamemode  (Not used as Race is Single Player Only)
    public override void OnServerConnect(NetworkConnection conn) { }
    //Called when a clint requests a Player Object
    public override GameObject OnServerAddPlayer(NetworkRacer nPlayer, GameObject playerPrefab) { return null; }

    public override void OnEndGamemode()
    {
        throw new NotImplementedException();
    }
}
