using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class NetworkRaceClient : Race
{
    protected NetworkClient client;

    public static GameObject myKart;
    protected Racer myRacer;

    //Called by StartGamemode() void, Overides Offline Racer IEnumerator
    protected override IEnumerator actualStartGamemode()
    {
        Debug.Log("Client Race script started!");
        raceType = RaceType.Online;

        RegisterHandles();
        yield return null;
    }

    private void RegisterHandles()
    {
        client = NetworkClient.allClients[0];

        client.RegisterHandler(UnetMessages.showLvlSelectMsg, OnShowLevelSelect);
        client.RegisterHandler(UnetMessages.voteListUpdateMsg, OnVoteListUpdate);
        client.RegisterHandler(UnetMessages.startRollMsg, OnStartRoll);
        client.RegisterHandler(UnetMessages.forceLevelSelectMsg, OnForceLevelSelect);
        client.RegisterHandler(UnetMessages.allVoteListMsg, OnAllVoteList);
        client.RegisterHandler(UnetMessages.loadLevelMsg, OnLoadLevel);
        client.RegisterHandler(UnetMessages.spawnKartMsg, OnSpawnKart);
        client.RegisterHandler(UnetMessages.positionMsg, OnPosition);
        client.RegisterHandler(UnetMessages.finishRaceMsg, OnFinishRace);
    }

    private void OnShowLevelSelect(NetworkMessage netMsg)
    {
        FindObjectOfType<LevelSelect>().enabled = true;
        FindObjectOfType<LevelSelect>().ShowLevelSelect();
    }

    //Sends a vote to the Server
    public void SendVote(int cup, int track)
    {
        TrackVoteMessage msg = new TrackVoteMessage(cup,track);
        client.Send(UnetMessages.trackVoteMsg, msg);
    }

    //Sent by Server to update votes list
    private void OnVoteListUpdate(NetworkMessage netMsg)
    {
        TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>();

        //Check that Vote is in the Scene
        if(FindObjectOfType<VotingScreen>() == null)
        {
            gameObject.AddComponent<VotingScreen>();
            //Only show voting screen if not deciding on a track
            if (!FindObjectOfType<LevelSelect>().enabled)
            {
                FindObjectOfType<VotingScreen>().ShowScreen();
            }            
        }

        FindObjectOfType<VotingScreen>().AddVote(msg.cup, msg.track);
        Debug.Log("Recieved Vote From Server! Cup: " + msg.cup + " Track: " + msg.track);
    }

    private void OnStartRoll(NetworkMessage netMsg)
    {
        intMessage msg = netMsg.ReadMessage<intMessage>();
        FindObjectOfType<VotingScreen>().StartRoll(msg.value);
    }

    //Sent by Server, forces Level Select GUI to close
    private void OnForceLevelSelect(NetworkMessage netMsg)
    {
        if(FindObjectOfType<LevelSelect>().enabled)
        {
            FindObjectOfType<LevelSelect>().ForceFinishLevelSelect();
            FindObjectOfType<VotingScreen>().ShowScreen();
        }    
    }

    //Sent by a Server when a player joins after voting has started
    private void OnAllVoteList(NetworkMessage netMsg)
    {
        AllVoteMessage msg = netMsg.ReadMessage<AllVoteMessage>();

        if (FindObjectOfType<VotingScreen>() == null)
        {
            VotingScreen vs = gameObject.AddComponent<VotingScreen>();
            vs.ShowScreen();
        }

        List<Vote> votes = new List<Vote>();      
        for(int i = 0; i < msg.cups.Length; i++)
        {
            votes.Add(new Vote(msg.cups[i], msg.tracks[i]));
        }
        FindObjectOfType<VotingScreen>().votes = votes;

    }

    //Sent by a Server causes a track to be loaded
    private void OnLoadLevel(NetworkMessage netMsg)
    {
        TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>();
        currentCup = msg.cup;
        currentTrack = msg.track;

        //Clear away handlers that we don't need anymore
        client.UnregisterHandler(UnetMessages.showLvlSelectMsg);
        client.UnregisterHandler(UnetMessages.voteListUpdateMsg);
        client.UnregisterHandler(UnetMessages.startRollMsg);
        client.UnregisterHandler(UnetMessages.forceLevelSelectMsg);
        client.UnregisterHandler(UnetMessages.allVoteListMsg);

        StartCoroutine(ActualOnLoadLevel());
    }

    private IEnumerator ActualOnLoadLevel()
    {
        CurrentGameData.blackOut = true;

        yield return new WaitForSeconds(0.5f);

        //Clear away any unwanted Components
        if (FindObjectOfType<VotingScreen>() != null)
            DestroyImmediate(FindObjectOfType<VotingScreen>());

        SceneManager.LoadScene(gd.tournaments[currentCup].tracks[currentTrack].sceneID);
        yield return null;

        //Load the Track Manager
        td = FindObjectOfType<TrackData>();

        yield return new WaitForSeconds(1f);
       
        CurrentGameData.blackOut = false;
        yield return new WaitForSeconds(0.5f);

        //Do the intro to the Map
        yield return StartCoroutine("DoIntro");

        //Show what race we're on
        yield return StartCoroutine(ChangeState(RaceGUI.RaceInfo));

        yield return new WaitForSeconds(3f);

        kartInfo[] kies = GameObject.FindObjectsOfType<kartInfo>();
        foreach (kartInfo ki in kies)
            ki.hidden = false;

        kartItem[] kitemes = FindObjectsOfType<kartItem>();
        foreach (kartItem ki in kitemes)
            ki.hidden = false;

        kartInput[] kines = GameObject.FindObjectsOfType<kartInput>();
        foreach (kartInput ki in kines)
            ki.camLocked = false;

        yield return StartCoroutine(ChangeState(RaceGUI.Countdown));

        client.Send(UnetMessages.readyMsg, new EmptyMessage());

    }

    //Called when the server wants the client to spawn it's Kart
    private void OnSpawnKart(NetworkMessage netMsg)
    {
        intMessage msg = netMsg.ReadMessage<intMessage>();

        myRacer = new Racer(0, -1, CurrentGameData.currentChoices[0], msg.value);
        ClientScene.AddPlayer(0);        
    }

    //Called when the Server wants to update the Karts Position
    private void OnPosition(NetworkMessage netMsg)
    {
        intMessage msg = netMsg.ReadMessage<intMessage>();
        myKart.GetComponent<PositionFinding>().position = msg.value;
    }

    public void SetupCameras()
    {
        myRacer.ingameObj = myKart.transform;

        //Fix Rotation Issue
        myRacer.ingameObj.rotation = td.spawnPoint.rotation * Quaternion.Euler(0, -90, 0);

        if (myKart != null)
        {
            //Spawn the Camera .etc for this Kart
            Transform inGameCam = (Transform)Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), myKart.transform.position, Quaternion.identity);
            inGameCam.name = "InGame Cams";

            kartInput ki = myKart.AddComponent<kartInput>();
            ki.myController = 0;
            ki.camLocked = true;
            ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
            ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

            inGameCam.GetChild(1).tag = "MainCamera";

            inGameCam.GetChild(0).transform.GetComponent<kartCamera>().target = myRacer.ingameObj;
            inGameCam.GetChild(1).transform.GetComponent<kartCamera>().target = myRacer.ingameObj;
            myRacer.cameras = inGameCam;

            kartInfo kain = myKart.AddComponent<kartInfo>();
            kain.hidden = true;

            Camera[] cameras = new Camera[2];
            cameras[0] = myKart.GetComponent<kartInput>().frontCamera;
            cameras[1] = myKart.GetComponent<kartInput>().backCamera;
            kain.cameras = cameras;
        }     
    }

    private void OnFinishRace(NetworkMessage netMsg)
    {
        StartCoroutine(FinishKart(myRacer));
        StopTimer();
    }
}
