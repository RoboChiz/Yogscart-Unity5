﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public enum RaceType { GrandPrix, VSRace, TimeTrial };
public enum RaceGUI { Blank, CutScene, RaceInfo, Countdown, RaceGUI, Finish, ScoreBoard, NextMenu, Win};

/*
    Races Class v1.0
    Created by Robert (Robo_Chiz)
    FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
    Thanks
*/

public class Race : GameMode
{

    public RaceType raceType;
    public int currentCup = -1, currentTrack = -1, currentRace = 1;

    public RaceGUI currentGUI = RaceGUI.Blank;
    public float guiAlpha = 0f;
    public bool changingState = false;

    private TrackData td;

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

        for(int i = 0; i < racers.Count; i++)
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

        //Tidy Up

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

        while((Time.time - startTime) < clip.travelTime)
        {
            cam.position = Vector3.Lerp(clip.startPoint, clip.endPoint, (Time.time - startTime) / clip.travelTime);
            cam.rotation = Quaternion.Slerp(Quaternion.Euler(clip.startRotation), Quaternion.Euler(clip.endRotation), (Time.time - startTime) / clip.travelTime);
            yield return null;
        }
    }


    public IEnumerator ChangeState(RaceGUI nState)
    {
        if(currentGUI != nState && ! changingState)
        {
            changingState = true;

            float startTime = Time.time;
            const float changeTime = 0.5f;

            while((Time.time - startTime) < changeTime)
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

        switch(currentGUI)
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
            if(pf.lap >= td.Laps && !racers[i].finished)
            {
                racers[i].finished = true;
                racers[i].timer = timer;
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
}
