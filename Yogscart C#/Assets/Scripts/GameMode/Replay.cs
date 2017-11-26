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

    enum InputState { Locked, Replay, Menu, Options}
    private InputState inputState = InputState.Replay;

    private CurrentGameData gd;
    private Race currentRace;
    private TrackData td;

    //Camera Stuff
    private bool loadedRace = false;
    private Camera replayCamera;
    private KartCamera replayKartCamera;
    private OrbitCam orbitCam;
    private FreeCam freeCam;

    private float controlAlpha = 0f;
    private bool showUI;
    private int target;

    public enum CameraMode {PlayerCam, TargetCam, FreeCam, TrackCam}
    public CameraMode cameraMode = CameraMode.PlayerCam;

    public void Setup(Race _race, List<ReplayRacer> _racers)
    {
        gd = FindObjectOfType<CurrentGameData>();

        currentRace = _race;
        racers = _racers;

        loadedRace = false;

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
        AsyncOperation sync = SceneManager.LoadSceneAsync(gd.tournaments[currentRace.currentCup].tracks[currentRace.currentTrack].sceneIDs[currentRace.currentVariation]);

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
            rr.ingameObj = FindObjectOfType<KartMaker>().SpawnKart(KartType.Replay, td.spawnPoint.position, td.spawnPoint.rotation, rr.character, rr.hat, rr.kart, rr.wheel);
            rr.ingameObj.GetComponent<KartReplayer>().replayData = rr.ghostData;          
        }

        //Create Debug Kart Camera
        GameObject camera = new GameObject("Camera");

        replayKartCamera = camera.AddComponent<KartCamera>();
        target = racers.Count - 1;

        camera.AddComponent<AudioListener>();
        replayCamera = camera.GetComponent<Camera>();
        
        //Make a Kart Camera Rotater and turn it off
        orbitCam = camera.AddComponent<OrbitCam>();
        orbitCam.enabled = false;

        //Make a Free Cam
        freeCam = camera.AddComponent<FreeCam>();
        freeCam.enabled = false;

        //Turn on effects
        racers[target].ingameObj.GetComponent<KartMovement>().toProcess.Add(replayCamera);

        yield return null;

        isPlaying = true;

        yield return new WaitForSeconds(1.5f);

        loadedRace = true;
        showUI = true;

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

        //Show Controls
        if(controlAlpha > 0f)
        {
            GUIHelper.SetGUIAlpha(controlAlpha);

            GUIStyle label = new GUIStyle(GUI.skin.label);
            label.fontSize = (int)(label.fontSize * 0.7f);

            //Cam Mode
            GUI.Label(new Rect(10, 10, 1900, 50), "Camera Mode: " + cameraMode.ToString(), label);

            if(cameraMode != CameraMode.FreeCam)
                GUI.Label(new Rect(10, 60, 1900, 50), "Tracking: " + gd.characters[racers[target].character].name + ((racers[target].Human == -1) ? " (AI)" 
                    : " (Player #" + (racers[target].Human + 1).ToString() + ")"), label);

            //Controls
            if (InputManager.controllers[0].inputType == InputType.Xbox360)
            {
                switch (cameraMode)
                {
                    case CameraMode.PlayerCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: Y     Change Target: LB/RB     Change Camera Mode: A", label);
                        break;
                    case CameraMode.TargetCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: Y     Change Target: LB/RB     Change Camera Mode: A       Rotate Camera: RS      Zoom: LS", label);
                        break;
                    case CameraMode.TrackCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: Y     Change Target: LB/RB     Change Camera Mode: A", label);
                        break;
                    case CameraMode.FreeCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: Y     Change Camera Mode: A       Move Camera : LS LT/RT        Rotate Camera: RS", label);
                        break;
                }
                
            }
            else if (InputManager.controllers[0].inputType == InputType.Keyboard)
            {
                switch (cameraMode)
                {
                    case CameraMode.PlayerCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: H     Change Target: Z/X     Change Camera Mode: Return", label);
                        break;
                    case CameraMode.TargetCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: H     Change Target: Z/X     Change Camera Mode: Return       Rotate Camera: RMB      Zoom: Mouse Wheel", label);
                        break;
                    case CameraMode.TrackCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: H     Change Target: Z/X     Change Camera Mode: Return", label);
                        break;
                    case CameraMode.FreeCam:
                        GUI.Label(new Rect(10, 1000, 1900, 50), "Hide UI: H     Change Camera Mode: Return       Move Camera : WASDQE         Rotate Camera: RMB", label);
                        break;
                }

            }
        }

    }

    void Update()
    {
        if (loadedRace)
        {
            bool submitBool = false, cancelBool = false, pauseBool = false, hideBool = false;
            int vert = 0, tabChange = 0;

            if(inputState != InputState.Options)
                pauseBool = InputManager.controllers[0].GetButtonWithLock("Pause");

            if (inputState == InputState.Menu && guiAlpha == 1f)
            {
                submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
                cancelBool = InputManager.controllers[0].GetButtonWithLock("Cancel");
                vert = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");            
            }
            else if (inputState == InputState.Replay)
            {
                submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
                hideBool = InputManager.controllers[0].GetButtonWithLock("HideUI");
                tabChange = InputManager.controllers[0].GetIntInputWithLock("ChangeTarget");              
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
                        nextMenuOptions = new List<string>() { "Resume" };
                        nextMenuOptions.AddRange(currentRace.GetNextMenuOptions());
                        nextMenuOptions.Remove("Replay");
                        nextMenuOptions.Remove("Save Ghost");
                        nextMenuOptions.Insert(nextMenuOptions.Count-1,"Options");

                        optionsSize = new float[nextMenuOptions.Count];

                        orbitCam.enabled = false;
                        freeCam.enabled = false;

                        currentSelection = 0;
                    }

                    if (submitBool)
                    {
                        //Change Camera Mode
                        cameraMode = (CameraMode)(MathHelper.NumClamp((int)cameraMode + 1, 0, 3));
                        ActivateCameraMode();
                    }

                    if (tabChange != 0)
                    {
                        //Remove Camera to new target
                        racers[target].ingameObj.GetComponent<KartMovement>().toProcess.Remove(replayCamera);

                        //Swap Target
                        target = MathHelper.NumClamp(target + tabChange, 0, racers.Count);

                        //Add Camera to new target
                        racers[target].ingameObj.GetComponent<KartMovement>().toProcess.Add(replayCamera);
                    }

                    if (hideBool)
                    {
                        //Hide UI
                        showUI = !showUI;
                    }
                    break;
                case InputState.Menu:
                    if (submitBool)
                    {
                        bool killSelf = false;

                        switch (nextMenuOptions[currentSelection])
                        {
                            case "Resume":
                                inputState = InputState.Replay;
                                ActivateCameraMode();
                                break;
                            case "Next Race":
                            case "Restart":
                                currentRace.NextRace();
                                killSelf = true;
                                break;
                            case "Quit":
                                currentRace.EndGamemode();
                                killSelf = true;
                                break;
                            case "Finish":
                                TournamentRace tRace = currentRace as TournamentRace;
                                tRace.StartCoroutine(tRace.DoEnd());
                                killSelf = true;
                                break;
                            case "Options":
                                inputState = InputState.Options;

                                Debug.Log("Load the options menu");
                                GetComponent<Options>().enabled = true;
                                GetComponent<Options>().ShowOptions();
                                break;
                            case "Change Track":
                                currentRace.ChangeTrack();
                                StartCoroutine(KillSelf());
                                break;
                            case "Change Character":
                                currentRace.ChangeCharacter();
                                StartCoroutine(KillSelf());
                                break;
                        }

                        if (killSelf)
                        {
                            if (!(currentRace is VSRace))
                                StartCoroutine(KillSelf());
                            else
                                inputState = InputState.Locked;
                        }
                    }

                    if (vert != 0)
                        currentSelection = MathHelper.NumClamp(currentSelection + vert, 0, nextMenuOptions.Count);

                    if (pauseBool || cancelBool)
                    {
                        inputState = InputState.Replay;
                        ActivateCameraMode();
                    }
                    break;
                case InputState.Options:
                    if (GetComponent<Options>().enabled == false)
                        inputState = InputState.Menu;
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

            //Show Controls UI
            if (showUI)
                controlAlpha = Mathf.Clamp(controlAlpha + (Time.deltaTime * 3f), 0f, 1f);
            else
                controlAlpha = Mathf.Clamp(controlAlpha - (Time.deltaTime * 3f), 0f, 1f);

            //Control Camera Targets
            replayKartCamera.target = racers[target].ingameObj.GetComponent<KartMovement>().kartBody;
            replayKartCamera.rotTarget = racers[target].ingameObj;

            orbitCam.target = racers[target].ingameObj.GetComponent<KartMovement>().kartBody;

            //Camera Controls
            switch (cameraMode)
            {
                case CameraMode.PlayerCam:
                    replayKartCamera.distance = 6;
                    replayKartCamera.height = 2;
                    replayKartCamera.playerHeight = 2;
                    replayKartCamera.angle = 0;
                    replayKartCamera.sideAmount = 0;
                    break;
            }
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

    private void ActivateCameraMode()
    {
        //Turn on/off target cam
        if (cameraMode == CameraMode.TargetCam)
            orbitCam.enabled = true;
        else
            orbitCam.enabled = false;

        if (cameraMode == CameraMode.PlayerCam)
            replayKartCamera.enabled = true;
        else
            replayKartCamera.enabled = false;

        if (cameraMode == CameraMode.FreeCam)
        {
            freeCam.enabled = true;
            freeCam.SetStartRotation();

            //Remove self from target
            racers[target].ingameObj.GetComponent<KartMovement>().toProcess.Remove(replayCamera);

            //Turn off effects
            FindObjectOfType<EffectsManager>().ToggleReapply();
        }
        else if(freeCam.enabled)
        {
            freeCam.enabled = false;

            //Add self from target
            racers[target].ingameObj.GetComponent<KartMovement>().toProcess.Add(replayCamera);
        }
    }
}

