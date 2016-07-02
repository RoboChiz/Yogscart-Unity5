using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class NetworkRaceHost : Race
{
    private List<Vote> votes;

    public enum ServerRaceState {Voting, Setup, Racing, After};
    public ServerRaceState serverState = ServerRaceState.Voting;

    //Called by StartGamemode() void, Overides Offline Racer IEnumerator
    protected override IEnumerator actualStartGamemode()
    {
        Debug.Log("Host Race script started!");
        raceType = RaceType.Online;
        racers = new List<Racer>();
        votes = new List<Vote>();

        RegisterHandles();

        //Get Racers List from Host Script
        var oldList = FindObjectOfType<UnetHost>().finalPlayers;
        foreach(NetworkRacer netRacer in oldList)
        {
            racers.Add(netRacer);
        }

        //Send Level Select Request to Racers
        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.showLvlSelectMsg, new EmptyMessage());
        }

        //Send Vote Request to everyone else
        NetworkServer.SendToAll(UnetMessages.timerMsg, new TimerMessage(10));

        //Wait 10 seconds or until a votes have been collected
        float startTime = Time.time;
        while(votes.Count < racers.Count && Time.time - startTime < 10)
        {
            yield return null;
        }

        if (Time.time - startTime < 10)
            NetworkServer.SendToAll(UnetMessages.timerMsg, new TimerMessage(-1));

        //Close all Level Selects
        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.forceLevelSelectMsg, new EmptyMessage());
        }

        yield return new WaitForSeconds(1f);

        //Decide on Track and send it out
        int chosenVote = UnityEngine.Random.Range(0, votes.Count);

        NetworkServer.SendToAll(UnetMessages.startRollMsg, new intMessage(chosenVote));

        serverState = ServerRaceState.Setup;

        //Wait for Roll to finish and give players enough time to see Race
        yield return new WaitForSeconds(5f);

        //Tell everyone to load the level
        NetworkServer.SendToAll(UnetMessages.loadLevelMsg, new TrackVoteMessage(votes[chosenVote].cup, votes[chosenVote].track));

        yield return null;

        //Wait for Level to load
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByName(gd.tournaments[currentCup].tracks[currentTrack].sceneID))
            yield return null;

        td = FindObjectOfType<TrackData>();

        //Tell client to load kart
        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.spawnKartMsg, new intMessage(racer.position));
        }

        bool allReady = false;

        do
        {
            allReady = true;
            //Wait for Ready's to come in
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < racers.Count; i++)
            {
                NetworkRacer racer = racers[i] as NetworkRacer;
                if (!racer.ready)
                    allReady = false;
            }

        } while (!allReady);

        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.positionMsg, new intMessage(racer.position));
            racer.ready = false;
        }

        //Wait a Frame for Clients to get their Positions
        yield return null;

        //Send & wait for Countdown
        NetworkServer.SendToAll(UnetMessages.countdownMsg, new EmptyMessage());
        yield return new WaitForSeconds(3.3f);

        //Start the Gamemode's Timer
        StartTimer();

        //Tell all Client's to unlock their karts
        NetworkServer.SendToAll(UnetMessages.unlockKartMsg, new EmptyMessage());

        //Wait for the gamemode to be over
        while (!finished && timer < 3600)
        {
            HostUpdate();

            //Send out the Racers Position
            foreach (NetworkRacer racer in racers)
            {
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.positionMsg, new intMessage(racer.position));

                if(racer.finished && !racer.ready)
                {
                    NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.finishRaceMsg, new EmptyMessage());
                    racer.ready = true;
                }
                    
            }

            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log("It's over!");
        StopTimer();

    }

    private void RegisterHandles()
    {
        NetworkServer.RegisterHandler(UnetMessages.trackVoteMsg, OnTrackVote);
    }

    //Votes sent by client to be processed
    private void OnTrackVote(NetworkMessage netMsg)
    {
        TrackVoteMessage msg = netMsg.ReadMessage<TrackVoteMessage>();

        Debug.Log("Recieved Vote! Cup: " + msg.cup + " Track: " + msg.track);

        if (msg.cup >= 0 && msg.cup < gd.tournaments.Length && msg.track >= 0 && msg.track < gd.tournaments[msg.cup].tracks.Length)
        {
            votes.Add(new Vote(msg.cup, msg.track));
            NetworkServer.SendToAll(UnetMessages.voteListUpdateMsg, msg);
        }
        else
        {
            string errorMessage = "You've asked to play a track that dosen't exist!";
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
        }
    }

    //Called when a client disconnects from online gamemode
    public override void OnServerDisconnect(NetworkConnection conn)
    {

    }
    //Called when a client connects to online gamemode
    public override void OnServerConnect(NetworkConnection conn)
    {
        if(serverState == ServerRaceState.Voting)
        {
            AllVoteMessage msg = new AllVoteMessage();

            List<int> cups = new List<int>();
            List<int> tracks = new List<int>();

            foreach(Vote v in votes)
            {
                cups.Add(v.cup);
                tracks.Add(v.track);
            }

            msg.cups = cups.ToArray();
            msg.tracks = tracks.ToArray();

            NetworkServer.SendToClient(conn.connectionId, UnetMessages.allVoteListMsg, msg);
        }
    }

    //Called when a client asks to spawn
    public override GameObject OnServerAddPlayer(NetworkRacer nPlayer, GameObject playerPrefab)
    {
        int racePos = nPlayer.position;
        Vector3 spawnPosition = td.spawnPoint.position;
        Quaternion spawnRotation = td.spawnPoint.rotation;

        Vector3 startPos = spawnPosition + (spawnRotation * Vector3.forward * (3f * 1.5f) * -1.5f);
        Vector3 x2 = spawnRotation * (Vector3.forward * (racePos % 3) * (3 * 1.5f) + (Vector3.forward * .75f * 3));
        Vector3 y2 = spawnRotation * (Vector3.right * (racePos + 1) * 3);
        startPos += x2 + y2;

        var gameObject = (GameObject)Instantiate(playerPrefab, startPos, spawnRotation * Quaternion.Euler(0, -90, 0));
        nPlayer.ingameObj = gameObject.transform;

        gameObject.GetComponent<KartNetworker>().currentChar = nPlayer.Character;
        gameObject.GetComponent<KartNetworker>().currentHat = nPlayer.Hat;
        gameObject.GetComponent<KartNetworker>().currentKart = nPlayer.Kart;
        gameObject.GetComponent<KartNetworker>().currentWheel = nPlayer.Wheel;

        return gameObject;
    }

}
