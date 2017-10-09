using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
    int cup, track;

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
        if (!NetworkServer.active)
        {
            client.client.RegisterHandler(UnetMessages.showLvlSelectMsg, OnShowLevelSelect);
            client.client.RegisterHandler(UnetMessages.startRollMsg, OnRoll);
            client.client.RegisterHandler(UnetMessages.voteListUpdateMsg, OnAddToVoteList);
            client.client.RegisterHandler(UnetMessages.loadLevelMsg, OnLoadLevel);
        }
        else
        {
            NetworkServer.RegisterHandler(UnetMessages.trackVoteMsg, OnRecieveTrackVote);
        }
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
            OnLoadLevel(cup, track);
            NetworkServer.SendToAll(UnetMessages.loadLevelMsg, new TrackVoteMessage(cup, track));

            //Tell Client to setup kart

            //Setup AI
        }
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
        NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(15));
        client.OnTimer(15);

        //Wait 10 seconds or until a votes have been collected
        float startTime = Time.time;
        while (votes.Count < racers.Count && Time.time - startTime < 15)
        {
            yield return null;
        }

        //Cancel Clock if it is still there
        if (Time.time - startTime < 10)
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
        }
        else
        {
            //Select a track at Random
            cup = Random.Range(0, gd.tournaments.Length);
            track = Random.Range(0, gd.tournaments[cup].tracks.Length);

            NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, new TrackVoteMessage(cup, track));
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
        base.HostUpdate();
    }
    //Provides Update functionaliy for both client
    public override void ClientUpdate()
    {
        base.HostUpdate();
    }

    protected override void OnSpawnKart()
    {
        throw new System.NotImplementedException();
    }

    public override void NextRace()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLeaderboardUpdate(Leaderboard lb)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnRaceFinished()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnStartLeaderBoard(Leaderboard lb)
    {
        throw new System.NotImplementedException();
    }

    //Unused
    public override string[] GetNextMenuOptions() { return null; }

    //------------------------------------------------------------------------------
    //Sends client vote to server
    public void SendVote(int cup, int track)
    {
        Destroy(FindObjectOfType<LevelSelect>());

        if (!NetworkServer.active)
        {
            TrackVoteMessage msg = new TrackVoteMessage(cup, track);
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
        FindObjectOfType<LevelSelect>().enabled = true;
        FindObjectOfType<LevelSelect>().ShowLevelSelect();
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
            Debug.Log(netMsg.conn.address + " is a big cheater");
            string errorMessage = "'And if I ever see you here again, Wreck-It Ralph, I'll lock you in my Fungeon!'." + System.Environment.NewLine + "Error: Somethings gone wrong! You've sent a message to the server that you weren't suppose to. Either it's a bug or you're a dirty cheater. Eitherway you've been kicked.";
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
        }

        OnRecieveTrackVote(msg.cup, msg.track);
    }

    private void OnRecieveTrackVote(int _cup, int _track)
    {
        votes.Add(new Vote(_cup, track));

        //Actually process data
        NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, new TrackVoteMessage(_cup, _track));
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
            if (!FindObjectOfType<LevelSelect>().enabled)
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
    public void OnLoadLevel(NetworkMessage netMsg) { TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>(); OnLoadLevel(msg.cup, msg.track); }
    public void OnLoadLevel(int _cup, int _track) { StartCoroutine(ActualOnLoadLevel(_cup, _track)); }

    public IEnumerator ActualOnLoadLevel(int _cup, int _track)
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
        AsyncOperation sync = SceneManager.LoadSceneAsync(gd.tournaments[cup].tracks[track].sceneID);

        while (!sync.isDone)
            yield return null;

        //Let each gamemode do it's thing
        OnLevelLoad();

        //Load the Track Manager
        td = FindObjectOfType<TrackData>();

        //Wait for Spawn Messages to be recieved
        yield return new WaitForSeconds(1f);

        //Create Map Viewer
        mapViewer = gameObject.AddComponent<MapViewer>();
        mapViewer.HideMapViewer();

        //Update Local Version of Racers
        if(!NetworkServer.active)
        {
            //TODO
            racers = new List<Racer>();
        }

        //Update Map Viewer
        mapViewer.objects = new List<MapObject>();  
        foreach (Racer racer in racers)
            mapViewer.objects.Add(new MapObject(racer.ingameObj, gd.characters[racer.character].icon, racer.position));

        //Let Gamemode add Map Viewer Objects
        AddMapViewObjects();

        yield return new WaitForSeconds(0.5f);

        //Do the intro to the Map
        yield return StartCoroutine(DoIntro());

        //Show what race we're on
        KartMovement.beQuiet = false;
        raceName = GetRaceName();
        yield return ChangeState(RaceState.RaceInfo);

        //Get Local Kart Components for Race
        KartMovement[] kses = FindObjectsOfType<KartMovement>();
        KartInput[] kines = FindObjectsOfType<KartInput>();
        kartInfo[] kies = FindObjectsOfType<kartInfo>();
        KartItem[] kitemes = FindObjectsOfType<KartItem>();

        //Set Kart Components for Race
        foreach (KartInput ki in kines)
            ki.camLocked = false;

        //Let Gamemode make changes to karts
        OnPreKartStarting();

        yield return new WaitForSeconds(3f);

        foreach (kartInfo ki in kies)
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
            //TODO
        }
    }
}
