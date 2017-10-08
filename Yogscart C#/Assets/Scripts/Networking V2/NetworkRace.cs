using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
            client.client.RegisterHandler(UnetMessages.showLvlSelectMsg, OnTrackVote);
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

            //Tell Client to fade to black
            CurrentGameData.blackOut = true;
            yield return new WaitForSeconds(0.5f);

            //Tell Client to load level

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
                OnTrackVote(); //This is the host
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
            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(-1));

        //Close all Level Selects
        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.clearMsg, new EmptyMessage());
        }

        yield return new WaitForSeconds(1f);

        //Decide on Track and send it out
        if (votes.Count > 0)
        {
            int chosenVote = Random.Range(0, votes.Count);
            NetworkServer.SendToAll(UnetMessages.startRollMsg, new IntMessage(chosenVote));

            cup = votes[chosenVote].cup;
            track = votes[chosenVote].track;

            //Wait for Roll to finish and give players enough time to see Race
            yield return new WaitForSeconds(5f);
        }
        else
        {
            //Select a track at Random
            cup = Random.Range(0, gd.tournaments.Length);
            track = Random.Range(0, gd.tournaments[cup].tracks.Length);
        }
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
    // Message Delegates
    //------------------------------------------------------------------------------

    // Called when a Version Message is recieved by a client
    private void OnTrackVote(NetworkMessage netMsg) { OnTrackVote(); }
    private void OnTrackVote()
    {
        FindObjectOfType<LevelSelect>().enabled = true;
        FindObjectOfType<LevelSelect>().ShowLevelSelect();
    }

    //------------------------------------------------------------------------------
    // Called when a Version Message is recieved by a client
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

        //Actually process data
        NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, msg);
        OnRecieveTrackVote(msg.cup, msg.track);
    }

    private void OnRecieveTrackVote(int cup, int track)
    {
        votes.Add(new Vote(cup, track));
    }

    //------------------------------------------------------------------------------
    //Sent by Server to update votes list
    private void OnVoteListUpdate(NetworkMessage netMsg)
    {
        TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>();

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

        FindObjectOfType<VotingScreen>().AddVote(msg.cup, msg.track);
        Debug.Log("Recieved Vote From Server! Cup: " + msg.cup + " Track: " + msg.track);
    }

    //------------------------------------------------------------------------------
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
}
