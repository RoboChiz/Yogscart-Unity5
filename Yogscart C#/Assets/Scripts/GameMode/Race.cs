using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public enum RaceType { GrandPrix, VSRace, TimeTrial };
public enum RaceGUI { Blank, CutScene, RaceInfo, Countdown, RaceGUI, ScoreBoard, NextMenu, Win };

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
    public float guiAlpha = 0f;
    public bool changingState = false;

    private TrackData td;

    private string rankString;
    private int bestPlace;

    public override void StartGameMode()
    {
        StartCoroutine("actualStartGamemode");
    }

    public IEnumerator actualStartGamemode()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        sm = GameObject.FindObjectOfType<SoundManager>();

        //Character Select & Level Select
        yield return StartCoroutine("PlayerSetup");

        //Setup the Racers for the Gamemode
        SetupRacers();

        yield return null;

        StartCoroutine("StartRace");

        yield return null;
    }

    protected IEnumerator StartRace()
    {

        CurrentGameData.blackOut = true;

        yield return new WaitForSeconds(0.5f);

        //Tidy Up
        foreach (Racer r in racers)
        {
            r.finished = false;
            r.currentDistance = 0;
            r.totalDistance = 0;
            r.timer = 0;
        }

        lastcurrentRace = currentRace;


        //Load the Level
        SceneManager.LoadScene(gd.tournaments[currentCup].tracks[currentTrack].sceneID);
        yield return null;

        //Load the Track Manager
        td = GameObject.FindObjectOfType<TrackData>();

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
            }
            else
            {
                RacerAI ai = racers[i].ingameObj.gameObject.AddComponent<RacerAI>();
                ai.stupidity = racers[i].AiStupidity;
                ai.adjusterFloat = Mathf.Lerp(-5f, 5f, (i % 3) / 2f);
            }
        }

        yield return new WaitForSeconds(1f);

        //Do the intro to the Map
        yield return StartCoroutine("DoIntro");

        //Show what race we're on
        yield return StartCoroutine(ChangeState(RaceGUI.RaceInfo));

        yield return new WaitForSeconds(3f);

        kartInfo[] kies = GameObject.FindObjectsOfType<kartInfo>();
        foreach (kartInfo ki in kies)
            ki.hidden = false;

        kartInput[] kines = GameObject.FindObjectsOfType<kartInput>();
        foreach (kartInput ki in kines)
            ki.camLocked = false;

        yield return StartCoroutine(ChangeState(RaceGUI.Countdown));

        //Do the Countdown
        StartCoroutine("StartCountdown");
        yield return new WaitForSeconds(3.4f);

        StartTimer();

        //Unlock the karts
        kartScript[] kses = GameObject.FindObjectsOfType<kartScript>();
        foreach (kartScript ks in kses)
            ks.locked = false;

        //Wait for the gamemode to be over
        while (!finished && timer < 3600)
        {
            ClientUpdate();

            if (!clientOnly)
                HostUpdate();

            yield return new WaitForSeconds(0.25f);
        }

        //Show Results
        Debug.Log("It's over!");
        StopTimer();

        foreach (kartInput ki in kines)
            ki.camLocked = false;

        DisplayRacer[] sortedRacers = new DisplayRacer[racers.Count];

        while(sortedRacers.Length != racers.Count)
            yield return null;

        foreach (Racer r in racers)
        {
            r.points += 15 - r.position;
            sortedRacers[r.position] = new DisplayRacer(r);
        }

        if (currentRace == 4 || raceType == RaceType.TimeTrial)
            DetermineWinner();

        yield return new WaitForSeconds(2.5f);

        StartCoroutine(ChangeState(RaceGUI.ScoreBoard));

        Leaderboard lb = gameObject.AddComponent<Leaderboard>();

        lb.racers = new List<DisplayRacer>(sortedRacers);

        yield return null;

        if (raceType == RaceType.TimeTrial)
            lb.StartTimeTrial();
        else
            lb.StartLeaderBoard();

        //Tidy Up
        timer = 0;
        finished = false;

        yield return null;
    }

    //Do Character Select and Level Select
    private IEnumerator PlayerSetup()
    {
        CharacterSelect cs = GameObject.FindObjectOfType<CharacterSelect>();
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

            if (cs.State == CharacterSelect.csState.Off)
            {
                //Cancel everything!
                MainMenu mm = GameObject.FindObjectOfType<MainMenu>();
                if (mm != null && mm.enabled)
                    mm.BackMenu();

                //Stop all Gamemode Coroutines
                ForceStop();
                //Wait a Frame for Coroutines to stop
                yield return null;

                Debug.Log("It didn't worked");
            }

            //Everything worked out perfect!
            Debug.Log("It worked");
            LevelSelect ls = GameObject.FindObjectOfType<LevelSelect>();

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

                float idealWidth = Screen.width / 3f;
                Texture2D previewTexture = gd.tournaments[currentCup].tracks[currentTrack].preview;
                float previewRatio = idealWidth / previewTexture.width;
                Rect previewRect = new Rect(Screen.width - idealWidth - 20, Screen.height - (previewTexture.height * previewRatio * 2f), idealWidth, previewTexture.height * previewRatio);

                GUI.DrawTexture(previewRect, previewTexture);

                break;
            case RaceGUI.RaceInfo:
                Texture2D raceTexture;

                if (raceType == RaceType.TimeTrial)
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/TimeTrial");
                else
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/" + currentRace);

                GUI.DrawTexture(new Rect(10, 10, Screen.width - 20, Screen.height), raceTexture, ScaleMode.ScaleToFit);
                break;
            case RaceGUI.ScoreBoard:

                Leaderboard lb = GetComponent<Leaderboard>();

                if (!changingState && ( InputManager.controllers[0].GetMenuInput("Submit") != 0 || InputManager.GetClick()))
                {
                    if (lb.state != LBType.Points || currentRace == 1)
                    {
                        StartCoroutine(ChangeState(RaceGUI.NextMenu));
                        lb.hidden = true;
                    }
                    else
                    {
                        lb.SecondStep();
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
                
                    switch (options[currentSelection])
                    {
                        case "Quit":
                            StartCoroutine(ChangeState(RaceGUI.Blank));
                            EndGamemode();
                            break;
                        case "Next Race":
                            StartCoroutine(ChangeState(RaceGUI.Blank));
                            StartCoroutine("StartRace");
                            currentTrack++;
                            currentRace++;
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

        GUIHelper.ResetColor();
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
                if (racers[i].Human != -1)
                    StartCoroutine(FinishKart(racers[i]));
            }

            //Finish Race
            if (racers[i].Human != -1 && !racers[i].finished)
                allFinished = false;
        }

        SortingScript.CalculatePositions(racers);

        if (allFinished)
            finished = true;

    }

    public override void ClientUpdate()
    {

    }

    private IEnumerator FinishKart(Racer racer)
    {
        racer.ingameObj.gameObject.AddComponent<RacerAI>();
        Destroy(racer.ingameObj.GetComponent<kartInput>());
        //Hide Kart Item
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
            kc.sideAmount = Mathf.Lerp(0, -1.9f, percent);

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
            for(int i = 0; i < pointsSorted.Count; i++)
            {
                if(pointsSorted[i].Human != -1)
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
                if(raceType == RaceType.GrandPrix)
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
        
        if(raceType == RaceType.TimeTrial)
        {
            float bestTime = gd.tournaments[currentCup].tracks[currentTrack].bestTime;

            if(racers[0].timer <= bestTime || bestTime == 0)
            {
                gd.tournaments[currentCup].tracks[currentTrack].bestTime = racers[0].timer;

            }
        }

        gd.SaveGame();
    }

    void EndGamemode()
    {
        currentCup = -1;
        currentTrack = -1;
        currentRace = 1;
        lastcurrentRace = -1;
        StartCoroutine(QuitGame());
    }


}
