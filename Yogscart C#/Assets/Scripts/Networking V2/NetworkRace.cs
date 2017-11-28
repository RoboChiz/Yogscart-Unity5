using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class NetworkRace : Race
{
    public bool useAI = false;
    public bool isHost;

    public YogscartNetwork.Client client;
    public YogscartNetwork.Host host;

    //Functionality from Race
    protected override bool enableAI { get { return useAI; } }
    protected override string GetRaceName() { return "Online Race"; }

    //Needed for Race
    private List<Vote> votes;
    int cup, track, variation;

    const int voteTimer = 15;

    //Local Racer
    public Racer localRacer;
    public bool doneFinish;

    public bool lastSixtySecondsSent = false;
    public float lastSixtySecondStartTime;

    //Used by Spectators
    private bool isSpectator;
    private Camera replayCamera;
    private KartCamera replayKartCamera;
    private OrbitCam orbitCam;
    private FreeCam freeCam;
    private List<KartMovement> spectatorTargets;

    private int target;
    private float controlAlpha = 0f;
    private bool showUI, createdSpectator;

    public enum CameraMode { PlayerCam, TargetCam, FreeCam, TrackCam }
    public CameraMode cameraMode = CameraMode.PlayerCam;

    //Used to setup the gamemodes
    public override void StartGameMode()
    {
        Debug.Log("Started Race Gamemode!");

        //Setup Network Stuff
        isHost = NetworkServer.active;
        client = FindObjectOfType<YogscartNetwork.Client>();
        
        if(isHost)
        {
            host = client as YogscartNetwork.Host;
            useAI = host.serverSettings.fillWithAI;
            Debug.Log("I'M A HOST?!");
        }

        RegisterHandlers();
        votes = new List<Vote>();
        Debug.Log("Registered Handlers!");

        base.StartGameMode();
    }

    public void RegisterHandlers()
    {
        //Stop Host from registering Client messages it should never use
        if (!isHost)
        {
            client.client.RegisterHandler(UnetMessages.showLvlSelectMsg, OnShowLevelSelect);
            client.client.RegisterHandler(UnetMessages.startRollMsg, OnRoll);
            client.client.RegisterHandler(UnetMessages.voteListUpdateMsg, OnAddToVoteList);
            client.client.RegisterHandler(UnetMessages.loadLevelMsg, OnLoadLevel);
            client.client.RegisterHandler(UnetMessages.spawnKartMsg, OnRecieveSpawnKart);
            client.client.RegisterHandler(UnetMessages.countdownMsg, OnCountdown);
            client.client.RegisterHandler(UnetMessages.countdownStateMsg, OnCountdownState); 
            client.client.RegisterHandler(UnetMessages.unlockKartMsg, OnUnlockKart);
            client.client.RegisterHandler(UnetMessages.positionMsg, OnRecievePosition);
            client.client.RegisterHandler(UnetMessages.finishRaceMsg, OnFinishRace);
            client.client.RegisterHandler(UnetMessages.leaderboardPosMsg, OnLeaderboardPos);
            client.client.RegisterHandler(UnetMessages.spectateMsg, OnSpectator);
        }
        else
        {
            NetworkServer.RegisterHandler(UnetMessages.trackVoteMsg, OnRecieveTrackVote);
            NetworkServer.RegisterHandler(UnetMessages.readyMsg, OnPlayerReady);
        }
    }

    public override void CleanUp()
    {
        //Cleanup Listeners
        if (!isHost)
        {
            client.client.UnregisterHandler(UnetMessages.showLvlSelectMsg);
            client.client.UnregisterHandler(UnetMessages.startRollMsg);
            client.client.UnregisterHandler(UnetMessages.voteListUpdateMsg);
            client.client.UnregisterHandler(UnetMessages.loadLevelMsg);
            client.client.UnregisterHandler(UnetMessages.spawnKartMsg);
            client.client.UnregisterHandler(UnetMessages.countdownMsg);
            client.client.UnregisterHandler(UnetMessages.countdownStateMsg);
            client.client.UnregisterHandler(UnetMessages.unlockKartMsg);
            client.client.UnregisterHandler(UnetMessages.positionMsg);
            client.client.UnregisterHandler(UnetMessages.finishRaceMsg);
            client.client.UnregisterHandler(UnetMessages.leaderboardPosMsg);
            client.client.UnregisterHandler(UnetMessages.spectateMsg);
        }
        else
        {
            NetworkServer.UnregisterHandler(UnetMessages.trackVoteMsg);
            NetworkServer.UnregisterHandler(UnetMessages.readyMsg);
        }

        if (isHost)
        {
            foreach (NetworkRacer racer in racers)
            {
                racer.finished = false;
                racer.timer = 0f;
            }
        }
    }

    public override void OnReturnLobby()
    {
        Destroy(FindObjectOfType<Leaderboard>());
        Destroy(FindObjectOfType<LevelSelect>());
    }

    protected override IEnumerator ActualStartGameMode()
    {
        if (isHost)
        {
            Debug.Log("Started ActualStartGameMode!");
            racers = new List<Racer>();

            //Do Track Vote
            yield return DoTrackSelect();

            Debug.Log("Done the Track Select!");

            yield return null;

            Debug.Log("Sending Level Load");

            //Tell Client to load level           
            OnLoadLevel(cup, track, variation);
            NetworkServer.SendToAll(UnetMessages.loadLevelMsg, new TrackVoteMessage(cup, track, variation));

            yield return WaitForAllReady();

            //Do Countdown State Change  
            NetworkServer.SendToAll(UnetMessages.countdownStateMsg, new EmptyMessage());
            OnCountdownState(null);

            yield return WaitForAllReady();

            NetworkServer.SendToAll(UnetMessages.countdownMsg, new EmptyMessage());
            yield return new WaitForSeconds(0.05f);
            OnCountdown(null);

            yield return new WaitForSeconds(3.4f);

            NetworkServer.SendToAll(UnetMessages.unlockKartMsg, new EmptyMessage());
            yield return new WaitForSeconds(0.05f);
            OnUnlockKart(null);     

            //Wait for the gamemode to be over
            while ((!raceFinished && timer < 1800f))
            {
                HostUpdate();

                //Finish Game Early
                if (lastSixtySecondsSent && Time.realtimeSinceStartup - lastSixtySecondStartTime >= 60f)
                {
                    raceFinished = true;
                }
                else
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }

            //End of Race
            NetworkServer.SendToAll(UnetMessages.finishRaceMsg, new EmptyMessage());

            //Hide 60 Second Timer
            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(-1));
            client.OnTimer(-1);

            yield return new WaitForSeconds(0.5f);

            //Tell Clients to Show Leaderboard
            List<NetworkRacer> toSend = new List<NetworkRacer>();
            foreach (NetworkRacer racer in racers)
                toSend.Add(racer);

            while (toSend.Count > 0)
            {
                NetworkRacer next = toSend[0];
                for(int i = 1; i < toSend.Count; i++)
                {
                    if(toSend[i].position < next.position)
                    {
                        next = toSend[i];
                    }
                }

                //Add Points
                next.points += 15 - next.position;

                DisplayRacerMessage msg = new DisplayRacerMessage(new DisplayRacer(next));

                NetworkServer.SendToAll(UnetMessages.leaderboardPosMsg, msg);
                OnLeaderboardPos(msg);

                toSend.Remove(next);

                yield return new WaitForSeconds(0.1f);

            }

            //Show Timer
            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(15));
            client.OnTimer(15);

            yield return new WaitForSeconds(15f);

            //End Gamemode
            finished = true;
        }
    }

    private IEnumerator WaitForAllReady()
    {
        //Wait for all Players to be Ready
        while (true)
        {
            bool allReady = true;

            foreach (NetworkRacer racer in racers)
            {
                if (!racer.ready)
                    allReady = false;
            }

            if (allReady)
            {
                break;
            }

            //Try again in half a second
            yield return new WaitForSeconds(0.5f);
        }

        //Reset Ready
        foreach (NetworkRacer racer in racers)
            racer.ready = false;
    }

    //Do Track Vote & Selection
    private IEnumerator DoTrackSelect()
    {
        //Tell Racing Players to pick a map
        foreach(NetworkRacer racer in host.finalPlayers)
        {
            racers.Add(racer);

            if (racer.conn != null)
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.showLvlSelectMsg, new EmptyMessage());
             else
                OnShowLevelSelect(); //This is the host
        }
        Debug.Log("Sent Track vote msg!");

        //Wait for messages to send
        yield return new WaitForSeconds(0.2f);

        //Send Timer
        NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(voteTimer));
        client.OnTimer(voteTimer);

        //Wait 10 seconds or until a votes have been collected
        float startTime = Time.time;
        while (votes.Count < racers.Count && Time.time - startTime < voteTimer)
        {
            yield return null;
        }

        //Cancel Clock if it is still there
        if (Time.time - startTime < voteTimer)
        {
            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(-1));
            client.OnTimer(-1);
        }

        //Close all Level Selects
        foreach (NetworkRacer racer in racers)
        {
            if (racer.conn != null)
            {
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.clearMsg, new EmptyMessage());
            }
            else
            {
                client.OnClear();
            }
        }

        yield return new WaitForSeconds(1f);

        //Decide on Track and send it out
        int chosenVote = 0;
        if (votes.Count > 0)
        {
            chosenVote = Random.Range(0, votes.Count);         

            cup = votes[chosenVote].cup;
            track = votes[chosenVote].track;
            variation = gd.GetRandomLevelForTrack(cup, track);
        }
        else
        {
            //Select a track at Random
            cup = Random.Range(0, gd.tournaments.Length);
            track = Random.Range(0, gd.tournaments[cup].tracks.Length);
            variation = gd.GetRandomLevelForTrack(cup, track);

            NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, new TrackVoteMessage(cup, track, variation));
            OnAddToVoteList(cup, track);

            //Wait for message to send
            yield return new WaitForSeconds(0.2f);
        }

        NetworkServer.SendToAll(UnetMessages.startRollMsg, new IntMessage(chosenVote));
        OnRoll(chosenVote);

        //Wait for Roll to finish and give players enough time to see Race
        yield return new WaitForSeconds(8f);
    }

    //Provides Update functionaliy for server
    public override void HostUpdate()
    {
        bool allFinished = true;
        bool anyPlayerFinished = false;

        foreach (NetworkRacer racer in racers)
        {
            //Update Position Finding
            PositionFinding pf = racer.ingameObj.GetComponent<PositionFinding>();
            racer.currentPercent = pf.currentPercent;
            racer.lap = pf.lap;
            pf.racePosition = racer.position;

            if (racer.conn != null)
            {
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.positionMsg, new IntMessage(racer.position));
            }
            else
            {
                OnRecievePosition(racer.position);
            }

            //Finish Player
            if (pf.lap >= td.Laps && !racer.finished)
            {
                racer.finished = true;
                racer.timer = timer;

                if (racer.conn != null)
                {
                    NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.finishRaceMsg, new EmptyMessage());
                }
                else
                {
                    OnFinishRace();
                }

                racer.position = racersFinished;
                racersFinished++;
            }

            if(racer.finished)
            {
                anyPlayerFinished = true;
            }

            //Finish Race
            if (!racer.finished)
                allFinished = false;
        }

        SortingScript.CalculatePositions(racers);

        if (allFinished)
        {
            raceFinished = true;

            //Change Pitch Back
            FindObjectOfType<SoundManager>().SetMusicPitch(1f);

            NetworkServer.SendToAll(UnetMessages.finishRaceMsg, new EmptyMessage());
        }
        else if (anyPlayerFinished && !lastSixtySecondsSent)
        {
            //When a player has finished, everyone else gets 60 seconds
            lastSixtySecondsSent = true;
            lastSixtySecondStartTime = Time.realtimeSinceStartup;

            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(60));
            client.OnTimer(60);
        }
    }

    //Provides Update functionaliy for both client
    public override void ClientUpdate()
    {
        //Change pitch of music for last lap
        if (localRacer != null && localRacer.ingameObj.GetComponent<PositionFinding>().lap >= td.Laps - 1 && !lastLap)
        {
            lastLap = true;
            FindObjectOfType<SoundManager>().SetMusicPitch(td.lastLapPitch);
        }
    }
    //Unused
    public override string[] GetNextMenuOptions() { return null; }
    public override void NextRace() { throw new System.NotImplementedException(); }
    protected override void OnLeaderboardUpdate(Leaderboard lb) { throw new System.NotImplementedException(); }
    protected override void OnRaceFinished() { throw new System.NotImplementedException(); }
    protected override void OnStartLeaderBoard(Leaderboard lb) { throw new System.NotImplementedException(); }

    //------------------------------------------------------------------------------
    //Sends client vote to server
    public void SendVote(int cup, int track)
    {
        Destroy(FindObjectOfType<LevelSelect>());

        if (!isHost)
        {
            TrackVoteMessage msg = new TrackVoteMessage(cup, track, 0);
            client.client.Send(UnetMessages.trackVoteMsg, msg);
        }
        else
        {
            OnRecieveTrackVote(cup, track);
        }
    }

    //------------------------------------------------------------------------------
    // Message Delegates
    //------------------------------------------------------------------------------

    // Called when a Version Message is recieved by a client
    private void OnShowLevelSelect(NetworkMessage netMsg) { OnShowLevelSelect(); }
    private void OnShowLevelSelect()
    {
        LevelSelect ls = FindObjectOfType<LevelSelect>();

        if(ls == null)
        {
            ls = gameObject.AddComponent<LevelSelect>();
        }

        ls.enabled = true;
        ls.ShowLevelSelect();
    }

    //------------------------------------------------------------------------------
    // Called when a track is sent to the server by client
    private void OnRecieveTrackVote(NetworkMessage netMsg)
    {
        TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>();

        int racerID = -1;
        //Check that Racer exists
        for (int i = 0; i < host.finalPlayers.Count; i++)
        {
            if (host.finalPlayers[i].conn == netMsg.conn)
            {
                racerID = i;
                break;
            }
        }

        if (racerID == -1)
        {
            host.KickPlayer(netMsg.conn);
        }

        OnRecieveTrackVote(msg.cup, msg.track);
    }

    private void OnRecieveTrackVote(int _cup, int _track)
    {
        votes.Add(new Vote(_cup, _track));

        //Actually process data
        NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, new TrackVoteMessage(_cup, _track, 0));
        OnAddToVoteList(_cup, _track);
    }

    //------------------------------------------------------------------------------
    //Sent by Server to update votes list
    private void OnAddToVoteList(NetworkMessage netMsg) { TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>(); OnAddToVoteList(msg.cup, msg.track); }
    private void OnAddToVoteList(int _cup, int _track)
    {
        //Check that Vote is in the Scene
        if (FindObjectOfType<VotingScreen>() == null)
        {
            gameObject.AddComponent<VotingScreen>();
            //Only show voting screen if not deciding on a track
            if (FindObjectOfType<LevelSelect>() == null || !FindObjectOfType<LevelSelect>().enabled)
            {
                FindObjectOfType<VotingScreen>().ShowScreen();
            }
        }

        FindObjectOfType<VotingScreen>().AddVote(_cup, _track);
        Debug.Log("Recieved Vote From Server! Cup: " + _cup + " Track: " + _track);
    }

    //------------------------------------------------------------------------------
    public void OnRoll(NetworkMessage netMsg) { IntMessage msg = netMsg.ReadMessage<IntMessage>(); OnRoll(msg.value); }
    public void OnRoll(int _rollValue)
    {
        FindObjectOfType<VotingScreen>().StartRoll(_rollValue);
    }

    //------------------------------------------------------------------------------
    public void OnLoadLevel(NetworkMessage netMsg) { TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>(); OnLoadLevel(msg.cup, msg.track, msg.variation); }
    public void OnLoadLevel(int _cup, int _track, int _variation) { StartCoroutine(ActualOnLoadLevel(_cup, _track, _variation)); }

    public IEnumerator ActualOnLoadLevel(int _cup, int _track, int _variation)
    {
        Debug.Log("DO LEVEL LOAD!");

        currentCup = _cup;
        currentTrack = _track;

        //Fade to black
        CurrentGameData.blackOut = true;

        //Delete Vote Screen
        if (FindObjectOfType<VotingScreen>() != null)
            FindObjectOfType<VotingScreen>().HideScreen();

        yield return new WaitForSeconds(1f);

        //Clear away handlers that we don't need anymore
        client.client.UnregisterHandler(UnetMessages.showLvlSelectMsg);
        client.client.UnregisterHandler(UnetMessages.startRollMsg);
        client.client.UnregisterHandler(UnetMessages.voteListUpdateMsg);
        client.client.UnregisterHandler(UnetMessages.loadLevelMsg);
        client.client.UnregisterHandler(UnetMessages.allVoteListMsg);

         //Set static values for Karts
        KartMovement.raceStarted = false;
        KartMovement.beQuiet = true;

        //Change Pitch Back
        FindObjectOfType<SoundManager>().SetMusicPitch(1f);

        //Load the Level
        AsyncOperation sync = SceneManager.LoadSceneAsync(gd.tournaments[currentCup].tracks[currentTrack].sceneIDs[_variation]);

        while (!sync.isDone)
            yield return null;

        //Let each gamemode do it's thing
        OnLevelLoad();

        //Load the Track Manager
        td = FindObjectOfType<TrackData>();

        //Tell client to load kart
        if (isHost)
        {          
            foreach (NetworkRacer racer in racers)
            {
                if (racer.conn != null)
                {
                    NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.spawnKartMsg, new IntMessage(racer.position));
                }
                else
                {
                    OnRecieveSpawnKart(racer.position);
                }
            }
        }

        //Tell Host we're Ready
        ClientScene.AddPlayer(0);

        //Wait for Spawn Messages to be recieved
        yield return new WaitForSeconds(2f);

        //Create Map Viewer
        mapViewer = gameObject.AddComponent<MapViewer>();
        mapViewer.HideMapViewer();

        //Update Local Version of Racers
        if(!isHost)
        {
            racers = null;
        }

        //Update Map Viewer
        mapViewer.objects = new List<MapObject>();  
        foreach (KartMovement racer in FindObjectsOfType<KartMovement>())
            mapViewer.objects.Add(new MapObject(racer.transform, gd.characters[racer.characterID].icon, racer.hatID));

        //Let Gamemode add Map Viewer Objects
        AddMapViewObjects();

        yield return new WaitForSeconds(0.5f);

        //If we Spectators
        if (localRacer == null)
        {
            CreateSpectator();
        }

        //Do the intro to the Map
        yield return StartCoroutine(DoIntro());

        //If we Spectators
        if (localRacer == null)
        {
            yield return StartCoroutine(ActualOnSpectator());
        }

        //Show what race we're on
        KartMovement.beQuiet = false;
        raceName = GetRaceName();
        yield return ChangeState(RaceState.RaceInfo);

        //Get Local Kart Components for Race
        KartMovement[] kses = FindObjectsOfType<KartMovement>();
        KartInput[] kines = FindObjectsOfType<KartInput>();
        KartInfo[] kies = FindObjectsOfType<KartInfo>();
        KartItem[] kitemes = FindObjectsOfType<KartItem>();

        //Set Kart Components for Race
        foreach (KartInput ki in kines)
            ki.camLocked = false;

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
        
        //Send Ready Message if racing
        if(client.isRacing)
        {
            if (!isHost)
            {
                client.client.Send(UnetMessages.readyMsg, new EmptyMessage());
            }
            else
            {
                OnHostReady();
            }
        }
    }

    //------------------------------------------------------------------------------
    public void OnRecieveSpawnKart(NetworkMessage netMsg) { IntMessage msg = netMsg.ReadMessage<IntMessage>(); OnRecieveSpawnKart(msg.value); }
    public void OnRecieveSpawnKart(int _position)
    {
        localRacer = new Racer(0, -1, CurrentGameData.currentChoices[0], _position);   
    }

    public override GameObject OnServerAddPlayer(NetworkRacer nPlayer, GameObject playerPrefab)
    {
        Vector3 spawnPosition = td.spawnPoint.position;
        Quaternion spawnRotation = td.spawnPoint.rotation;

        Vector3 startPos = spawnPosition + (spawnRotation * Vector3.forward * (3f * 1.5f) * -1.5f);
        Vector3 x2 = spawnRotation * (Vector3.forward * (nPlayer.position % 3) * (3 * 1.5f) + (Vector3.forward * .75f * 3));
        Vector3 y2 = spawnRotation * (Vector3.right * (nPlayer.position + 1) * 3);
        startPos += x2 + y2;

        GameObject gameObject = Instantiate(playerPrefab, startPos, spawnRotation * Quaternion.Euler(0, -90, 0));
        nPlayer.ingameObj = gameObject.transform;

        gameObject.GetComponent<KartNetworker>().currentChar    = nPlayer.character;
        gameObject.GetComponent<KartNetworker>().currentHat     = nPlayer.hat;
        gameObject.GetComponent<KartNetworker>().currentKart    = nPlayer.kart;
        gameObject.GetComponent<KartNetworker>().currentWheel   = nPlayer.wheel;

        return gameObject;
    }

    //------------------------------------------------------------------------------
    public void SetupCameras()
    {
        Transform inGameCam = Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), localRacer.ingameObj.transform.position, Quaternion.identity);
        inGameCam.name = "InGame Cams";

        KartInput ki = localRacer.ingameObj.gameObject.AddComponent<KartInput>();
        ki.myController = 0;
        ki.camLocked = true;
        ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
        ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

        inGameCam.GetChild(1).tag = "MainCamera";

        inGameCam.GetChild(0).transform.GetComponent<KartCamera>().target = localRacer.ingameObj.GetComponent<KartMovement>().kartBody;
        inGameCam.GetChild(1).transform.GetComponent<KartCamera>().target = localRacer.ingameObj.GetComponent<KartMovement>().kartBody;

        inGameCam.GetChild(0).transform.GetComponent<KartCamera>().rotTarget = localRacer.ingameObj;
        inGameCam.GetChild(1).transform.GetComponent<KartCamera>().rotTarget = localRacer.ingameObj;
        localRacer.cameras = inGameCam;

        localRacer.ingameObj.gameObject.AddComponent<KartInfo>();
        localRacer.ingameObj.gameObject.GetComponent<PositionFinding>().racePosition = localRacer.position;

        if(isHost)
        {
            localRacer.ingameObj.GetComponent<KartNetworker>().OnStartHost();
            localRacer.ingameObj.GetComponent<KartPositionPass>().OnStartHost();
        }

        localRacer.ingameObj.GetComponent<KartNetworker>().kartPlayerName = FindObjectOfType<CurrentGameData>().playerName;
    }

    //------------------------------------------------------------------------------
    public void OnPlayerReady(NetworkMessage netMsg)
    {
        int racerID = -1;
        //Check that Racer exists
        for (int i = 0; i < host.finalPlayers.Count; i++)
        {
            if (host.finalPlayers[i].conn == netMsg.conn)
            {
                racerID = i;
                break;
            }
        }

        if (racerID == -1)
        {
            host.KickPlayer(netMsg.conn);
        }

        host.finalPlayers[racerID].ready = true;
    }

    public void OnHostReady()
    {
        for (int i = 0; i < host.finalPlayers.Count; i++)
        {
            if (host.finalPlayers[i].conn == null)
            {
                host.finalPlayers[i].ready = true;
                return;
            }
        }
    }

    //------------------------------------------------------------------------------
    public override void OnServerDisconnect(NetworkConnection _conn)
    {
        base.OnServerDisconnect(_conn);

        foreach(NetworkRacer racer in racers.ToArray())
        {
            //Remove Racer if they are not longer racing
            if(racer.conn == _conn)
            {
                racers.Remove(racer);
            }

            //Clean up anything they've touched
        }
    }

    //------------------------------------------------------------------------------
    public void OnCountdown(NetworkMessage netMsg)
    {
        StartCountdown();
    }

    //------------------------------------------------------------------------------
    public void OnCountdownState(NetworkMessage netMsg)
    {
        StartCoroutine(ActualOnCountdownState());
    }
    private IEnumerator ActualOnCountdownState()
    {
        //Do the Countdown
        yield return ChangeState(RaceState.Countdown);

        //Send Ready Message if racing
        if (client.isRacing)
        {
            if (!isHost)
            {
                client.client.Send(UnetMessages.readyMsg, new EmptyMessage());
            }
            else
            {
                OnHostReady();
            }
        }
    }

    //------------------------------------------------------------------------------
    public void OnUnlockKart(NetworkMessage netMsg)
    {
        //Start the timer
        StartTimer();

        //Unlock the karts      
        foreach (KartMovement ks in FindObjectsOfType<KartMovement>())
            ks.locked = false;

        foreach (KartItem ki in FindObjectsOfType<KartItem>())
            ki.locked = false;

        KartMovement.raceStarted = true;

        StartCoroutine(ChangeState(RaceState.RaceGUI));

        //Unlock the Pause Menu
        PauseMenu.canPause = true;

        //If is client, do Client Update
        if(NetworkClient.active)
        {
            StartCoroutine(DoClientUpdate());
        }
    }

    public IEnumerator DoClientUpdate()
    {
        //Wait for the gamemode to be over
        while (!raceFinished && timer < 1800f)
        {
            ClientUpdate();
            yield return new WaitForSeconds(0.25f);

            if (currentState != RaceState.RaceGUI)
            {
                StartCoroutine(ChangeState(RaceState.RaceGUI));
            }
        }

        //Change Pitch Back
        FindObjectOfType<SoundManager>().SetMusicPitch(1f);
    }

    //------------------------------------------------------------------------------
    public void OnRecievePosition(NetworkMessage netMsg) { IntMessage msg = netMsg.ReadMessage<IntMessage>(); OnRecievePosition(msg.value); }
    public void OnRecievePosition(int position)
    {
        localRacer.position = position;
        localRacer.ingameObj.GetComponent<PositionFinding>().racePosition = position;
    }

    //------------------------------------------------------------------------------
    public void OnFinishRace(NetworkMessage netMsg) { if (!isHost) { raceFinished = true; } OnFinishRace(); }
    public void OnFinishRace()
    {
        if (!doneFinish)
        {
            doneFinish = true;

            //Tidy Up Local Racer
            if (localRacer != null)
            {
                StartCoroutine(TidyRacer());
            }

            FindObjectOfType<MapViewer>().HideMapViewer();

        }
    }

    private IEnumerator TidyRacer()
    {
        gd.overallLapisCount += localRacer.ingameObj.GetComponent<KartMovement>().lapisAmount;

        SaveDataManager saveDataManager = FindObjectOfType<SaveDataManager>();
        saveDataManager.SetLapisAmount(gd.overallLapisCount);
        saveDataManager.Save();

        localRacer.ingameObj.gameObject.AddComponent<AI>();
        Destroy(localRacer.ingameObj.GetComponent<KartInput>());

        //Hide Kart Item
        if (localRacer.ingameObj.GetComponent<KartItem>() != null)
        {
            localRacer.ingameObj.GetComponent<KartItem>().locked = true;
            localRacer.ingameObj.GetComponent<KartItem>().hidden = true;
        }

        if (localRacer.ingameObj.GetComponent<KartInfo>() != null)
            localRacer.ingameObj.GetComponent<KartInfo>().StartCoroutine("Finish");

        if (localRacer.cameras != null)
        {
            localRacer.cameras.GetChild(0).GetComponent<Camera>().enabled = false;
            localRacer.cameras.GetChild(1).GetComponent<Camera>().enabled = true;

            yield return new WaitForSeconds(2f);

            if (localRacer.ingameObj.GetComponent<KartInfo>() != null)
                localRacer.ingameObj.GetComponent<KartInfo>().hidden = true;

            float startTime = Time.time;
            const float travelTime = 3f;
            KartCamera kc = localRacer.cameras.GetChild(1).GetComponent<KartCamera>();

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

    //------------------------------------------------------------------------------
    public void OnLeaderboardPos(NetworkMessage netMsg)
    {
        DisplayRacerMessage msg = netMsg.ReadMessage<DisplayRacerMessage>();
        OnLeaderboardPos(msg);
    }
    public void OnLeaderboardPos(DisplayRacerMessage _displayRacer)
    {
        //Lock the Pause Menu
        PauseMenu.canPause = false;

        Leaderboard lb = FindObjectOfType<Leaderboard>();
        if (lb == null)
        {
            lb = gameObject.AddComponent<Leaderboard>();
            lb.racers = new List<DisplayRacer>();
            lb.StartLeaderBoard(this);
        }

        DisplayRacer displayRacer = new DisplayRacer(lb.racers.Count, _displayRacer.displayName, _displayRacer.character, _displayRacer.points, _displayRacer.timer);
        lb.racers.Add(displayRacer);
    }

    //------------------------------------------------------------------------------
    //If a client joins during this gamemode
    public override void OnServerConnect(NetworkConnection conn)
    {
        StartCoroutine(ActualOnServerConnect(conn));
    }

    private IEnumerator ActualOnServerConnect(NetworkConnection conn)
    {

        yield return new WaitForSeconds(0.5f);

        //Tell Client to Load Spectator System
        NetworkServer.SendToClient(conn.connectionId, UnetMessages.spectateMsg, new EmptyMessage());
    }

    //------------------------------------------------------------------------------
    public void OnSpectator(NetworkMessage netMsg)
    {
        if (localRacer == null)
        {
            KartMovement.raceStarted = true;
            OnUnlockKart(null);
            StartCoroutine(ActualOnSpectator());
        }
    }

    public void CreateSpectator()
    {
        createdSpectator = true;

        //Create Debug Kart Camera
        GameObject camera = new GameObject("Camera");
        camera.tag = "MainCamera";

        camera.AddComponent<AudioListener>();
        replayKartCamera = camera.AddComponent<KartCamera>();

        replayCamera = camera.GetComponent<Camera>();
        target = 0;

        //Make a Kart Camera Rotater and turn it off
        orbitCam = camera.AddComponent<OrbitCam>();
        orbitCam.enabled = false;

        //Make a Free Cam
        freeCam = camera.AddComponent<FreeCam>();
        freeCam.enabled = false;
    }

    private IEnumerator ActualOnSpectator()
    {
        CurrentGameData.blackOut = true;

        //Set IsSpectator
        isSpectator = true;

        //Wait for level to load
        while (!FindObjectOfType<YogscartNetwork.Client>().levelSync.isDone)
            yield return null;

        if(!createdSpectator)
        {
            CreateSpectator();
        }

        //Get all Targets
        while (spectatorTargets == null || spectatorTargets.Count == 0)
        {
            spectatorTargets = FindObjectsOfType<KartMovement>().ToList();
            yield return new WaitForSeconds(0.25f);
        }

        //Turn on effects
        spectatorTargets[target].toProcess.Add(replayCamera);
        showUI = false;

        PauseMenu.canPause = true;
        showUI = true;
        StartCoroutine(LoopSpectator());

        yield return new WaitForSeconds(0.5f);

        CurrentGameData.blackOut = false;
        yield return new WaitForSeconds(0.5f);

        showUI = true;
    }

    private IEnumerator LoopSpectator()
    {
        while (!finished)
        {
            SpectatorUpdate();
            yield return null;
        }
    }

    //------------------------------------------------------------------------------
    public void SpectatorUpdate()
    {
        bool submitBool = false, hideBool = false;
        int tabChange = 0;

        submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
        hideBool = InputManager.controllers[0].GetButtonWithLock("HideUI");
        tabChange = InputManager.controllers[0].GetIntInputWithLock("ChangeTarget");

        if (submitBool)
        {
            //Change Camera Mode
            cameraMode = (CameraMode)(MathHelper.NumClamp((int)cameraMode + 1, 0, 3));
            ActivateCameraMode();
        }

        if (tabChange != 0)
        {
            //Remove Camera to new target
            spectatorTargets[target].toProcess.Remove(replayCamera);

            //Swap Target
            target = MathHelper.NumClamp(target + tabChange, 0, spectatorTargets.Count);

            //Add Camera to new target
            spectatorTargets[target].toProcess.Add(replayCamera);
        }

        if (hideBool)
        {
            //Hide UI
            showUI = !showUI;

            if (mapViewer != null)
            {
                if (showUI)
                    mapViewer.ShowMapViewer();
                else
                    mapViewer.HideMapViewer();
            }
        }

        //Show Controls UI
        if (showUI)
            controlAlpha = Mathf.Clamp(controlAlpha + (Time.deltaTime * 3f), 0f, 1f);
        else
            controlAlpha = Mathf.Clamp(controlAlpha - (Time.deltaTime * 3f), 0f, 1f);

        //Control Camera Targets
        replayKartCamera.target = spectatorTargets[target].kartBody;
        replayKartCamera.rotTarget = spectatorTargets[target].transform;

        orbitCam.target = spectatorTargets[target].kartBody;

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
            spectatorTargets[target].toProcess.Remove(replayCamera);

            //Turn off Chromatic Aberration
            PostProcessingBehaviour postProcess = replayCamera.GetComponent<PostProcessingBehaviour>();
            if (postProcess != null)
            {
                ChromaticAberrationModel.Settings cab = postProcess.profile.chromaticAberration.settings;
                cab.intensity = 0f;
                postProcess.profile.chromaticAberration.settings = cab;
            }
        }
        else if (freeCam.enabled)
        {
            freeCam.enabled = false;

            //Add self from target
            spectatorTargets[target].toProcess.Add(replayCamera);
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (isSpectator)
        {
            GUI.matrix = GUIHelper.GetMatrix();
            GUI.skin = Resources.Load<GUISkin>("GUISkins/Leaderboard");

            //Show Controls
            if (controlAlpha > 0f)
            {
                GUIHelper.SetGUIAlpha(controlAlpha);

                GUIStyle label = new GUIStyle(GUI.skin.label);
                label.fontSize = (int)(label.fontSize * 0.7f);

                //Cam Mode
                GUI.Label(new Rect(10, 10, 1900, 50), "Camera Mode: " + cameraMode.ToString(), label);

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
    }

    protected override void OnSpawnKart()
    {
       
    }
}
