using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Replay : MonoBehaviour
{
    public List<ReplayRacer> racers { get; private set; }

    public int frameCount = 0;
    public bool isPlaying = false;
    private bool startBlackout = false;

    private int maxFrames, currentSelection = 0;

    private float blackOut = 1f, guiAlpha = 0f;

    private List<String> nextMenuOptions;
    private float[] optionsSize;

    enum InputState { Locked, Replay, Menu}
    private InputState inputState = InputState.Replay;

    private CurrentGameData gd;
    private Race currentRace;
    private TrackData td;

    public void Setup(Race _race, List<ReplayRacer> _racers)
    {
        gd = FindObjectOfType<CurrentGameData>();

        currentRace = _race;
        racers = _racers;

        StartCoroutine(StartReplay());
    }

    private IEnumerator StartReplay()
    {
        yield return new WaitForSeconds(1f);

        //Load Level
        while (!gd.isBlackedOut)
        {
            CurrentGameData.blackOut = true;
            yield return null;
        }

        startBlackout = true;

        //Tidy Up
        foreach (Racer r in racers)
        {
            r.finished = false;
            r.currentPercent = 0;
            r.lap = 0;
            r.timer = 0;
        }

        //Set static values for Karts
        KartMovement.raceStarted = false;
        KartMovement.beQuiet = true;

        //Load the Level
        AsyncOperation sync = SceneManager.LoadSceneAsync(gd.tournaments[currentRace.currentCup].tracks[currentRace.currentTrack].sceneID);

        while (!sync.isDone)
            yield return null;

        //Let each gamemode do it's thing
        currentRace.OnLevelLoad();

        td = FindObjectOfType<TrackData>();

        //Found out how many frames there are
        maxFrames = racers[0].ghostData.Count;

        //Spawn Karts
        foreach(ReplayRacer rr in racers)
        {
            rr.ingameObj = FindObjectOfType<KartMaker>().SpawnKart(KartType.Replay, td.spawnPoint.position, td.spawnPoint.rotation, rr.Character, rr.Hat, rr.Kart, rr.Wheel);
            rr.ingameObj.GetComponent<KartReplayer>().replayData = rr.ghostData;          
        }

        //Create Debug Kart Camera
        GameObject camera = new GameObject();
        kartCamera kc = camera.AddComponent<kartCamera>();
        kc.target = racers[racers.Count - 1].ingameObj;
        camera.AddComponent<AudioListener>();

        yield return null;

        isPlaying = true;

        yield return new WaitForSeconds(0.5f);

        CurrentGameData.blackOut = false;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //Send frame to kart replayer
        if (isPlaying)
        {
            //Force kart replays to follow you
            foreach (ReplayRacer rr in racers)
                rr.ingameObj.GetComponent<KartReplayer>().SetFrame(frameCount);

            frameCount = MathHelper.NumClamp(frameCount + 1, 0, maxFrames);
        }

        //If frameCount < 10 or bigger than max - 10, fade to black to hide transition back to start
        int frameOffset = 40;
        float half = frameOffset / 2f;

        if (frameCount <= half || frameCount >= maxFrames - half)
            blackOut = 1f;
        else if (frameCount <= frameOffset)
            blackOut = Mathf.Lerp(1f, 0f, (frameCount - half) / half);
        else if (frameCount >= maxFrames - frameOffset)
            blackOut = Mathf.Lerp(0f, 1f, (frameCount - (maxFrames - frameOffset)) / half);
        else
            blackOut = 0f;
    }

    void OnGUI()
    {      
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Leaderboard");
   
        //Fade to black at end of frames
        if (startBlackout && blackOut > 0f)
        GUIShape.RoundedRectangle(GUIHelper.screenEdges, 0, new Color(0f, 0f, 0f, blackOut));

        GUIHelper.SetGUIAlpha(guiAlpha);

        //Show Controls

        if (guiAlpha > 0)
        {
            //Show Pause Menu
            Texture2D boardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");
            GUI.DrawTexture(new Rect(660, 100, 600, 800), boardTexture);

            for (int i = 0; i < nextMenuOptions.Count; i++)
            {
                if (currentSelection == i)
                {
                    if (optionsSize[i] < 1.5f)
                        optionsSize[i] += Time.unscaledDeltaTime * 4f;
                    else
                        optionsSize[i] = 1.5f;
                }
                else
                {
                    if (optionsSize[i] > 1f)
                        optionsSize[i] -= Time.unscaledDeltaTime * 4f;
                    else
                        optionsSize[i] = 1f;
                }

                float yCentre = 500 - ((nextMenuOptions.Count * 100) / 2f);

                GUIHelper.CentreRectLabel(new Rect(670, yCentre + (i * 100), 580, 100), optionsSize[i], nextMenuOptions[i], (currentSelection == i) ? Color.yellow : Color.white);
            }
        }
    }

    void Update()
    {
        bool submitBool = false, cancelBool = false, pauseBool = false;
        int vert = 0;

        pauseBool = InputManager.controllers[0].GetMenuInput("Pause") != 0;

        if (inputState != InputState.Locked && guiAlpha == 1f)
        {            
            submitBool = InputManager.controllers[0].GetMenuInput("Submit") != 0;
            cancelBool = InputManager.controllers[0].GetMenuInput("Cancel") != 0;       
            vert = InputManager.controllers[0].GetMenuInput("MenuVertical");
        }

        if (inputState == InputState.Menu)
            guiAlpha = Mathf.Clamp(guiAlpha + (Time.deltaTime * 3f), 0f, 1f);
        else
            guiAlpha = Mathf.Clamp(guiAlpha - (Time.deltaTime * 3f), 0f, 1f);

        switch (inputState)
        {
            case InputState.Replay:
                if (pauseBool)
                {
                    inputState = InputState.Menu;
                    nextMenuOptions = currentRace.GetNextMenuOptions().ToList();
                    nextMenuOptions.Remove("Replay");
                    nextMenuOptions.Remove("Save Ghost");

                    optionsSize = new float[nextMenuOptions.Count];

                    currentSelection = 0;
                }
                break;
            case InputState.Menu:
                if(submitBool)
                {
                    switch(nextMenuOptions[currentSelection])
                    {
                        case "Next Race":
                        case "Restart":
                            currentRace.NextRace();
                            break;
                        case "Quit":
                            currentRace.EndGamemode();
                            break;
                        case "Finish":
                            TournamentRace tRace = currentRace as TournamentRace;
                            tRace.StartCoroutine(tRace.DoEnd());
                            break;
                    }

                    if (!(currentRace is VSRace))
                        StartCoroutine(KillSelf());
                    else
                        inputState = InputState.Locked;
                }

                if (vert != 0)
                    currentSelection = MathHelper.NumClamp(currentSelection + vert, 0, nextMenuOptions.Count);

                if (pauseBool || cancelBool)
                    inputState = InputState.Replay;
                break;
        }

        KartMovement[] kses = FindObjectsOfType<KartMovement>();
        KartItem[] kitemes = FindObjectsOfType<KartItem>();

        //Lock / Unlock Karts
        if (frameCount < (int)(40 * 7.4f))
        {
            //Unlock the karts      
            foreach (KartMovement ks in kses)
                ks.locked = true;

            foreach (KartItem ki in kitemes)
                ki.locked = true;
        }
        else
        {
            //Unlock the karts      
            foreach (KartMovement ks in kses)
                ks.locked = false;

            foreach (KartItem ki in kitemes)
                ki.locked = false;
        }

        if (frameCount > (int)(40 * 4f) && frameCount < (int)(40 * 7.4f))
        {
            KartMovement.startBoostVal = 3 - (((40 * 8) - frameCount) / 40);
        }
        else
        {
            KartMovement.startBoostVal = -1;
        }
    }

    public void GoBack()
    {
        inputState = InputState.Menu;
    }

    private IEnumerator KillSelf()
    {
        inputState = InputState.Locked;
        isPlaying = false;

        yield return new WaitForSeconds(0.5f);
        Destroy(this);
    }
}

