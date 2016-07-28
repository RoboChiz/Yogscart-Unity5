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

    private int totalFinishedCount = 0;

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

        int cup, track;

        //Decide on Track and send it out
        if (votes.Count > 0)
        {
            int chosenVote = UnityEngine.Random.Range(0, votes.Count);
            NetworkServer.SendToAll(UnetMessages.startRollMsg, new intMessage(chosenVote));

            cup = votes[chosenVote].cup;
            track = votes[chosenVote].track;

            //Wait for Roll to finish and give players enough time to see Race
            yield return new WaitForSeconds(5f);
        }
        else
        {
            //Select a track at Random
            cup = UnityEngine.Random.Range(0, gd.tournaments.Length);
            track = UnityEngine.Random.Range(0, gd.tournaments[cup].tracks.Length);
        }

        serverState = ServerRaceState.Setup;       

        //Tell everyone to load the level
        NetworkServer.SendToAll(UnetMessages.loadLevelMsg, new TrackVoteMessage(cup, track));

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

        float timeLeftStart = 0;

        //Wait for the gamemode to be over
        while (!raceFinished && timer < 3600)
        {
            HostUpdate();

            //Send out the Racers Position
            foreach (NetworkRacer racer in racers)
            {
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.positionMsg, new intMessage(racer.position));

                if(racer.finished && !racer.ready)
                {
                    Debug.Log("Finish sent to racer!");
                    NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.finishRaceMsg, new EmptyMessage());
                    racer.ready = true;
                    //Tell all Clients about Finish
                    DisplayRacer displayRacer = new DisplayRacer(racer);
                    displayRacer.name = racer.name;
                    NetworkServer.SendToAll(UnetMessages.playerFinishedMsg, new stringMessage(displayRacer.ToString()));

                    totalFinishedCount++;
                }
                    
            }

            //Start countdown timer when 3 people have crossed the line
            int playersNeeded = (racers.Count > 3) ? 3 : 2;
            if(!raceFinished)
            {
                if (totalFinishedCount >= playersNeeded)
                {
                    timeLeftStart = timer;
                    NetworkServer.SendToAll(UnetMessages.timerMsg, new TimerMessage(30));
                    totalFinishedCount = -20;//Make totalFinishedCount a value which can never hit again
                }

                if(timeLeftStart != 0 && timer - timeLeftStart >= 30)
                {
                    raceFinished = true;
                    DoDNF();
                }
            }
                

            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log("It's over!");
        StopTimer();

        //Cancel DNF Timer
        if(timeLeftStart != 0)
        {
            NetworkServer.SendToAll(UnetMessages.timerMsg, new TimerMessage(-1));
        }

        //Wait for all Clients to finish what they're doing
        yield return new WaitForSeconds(1f);
        
        //Add Points .etc
        DisplayRacer[] sortedRacers = new DisplayRacer[racers.Count];
        while (sortedRacers.Length != racers.Count)
            yield return null;

        foreach (NetworkRacer r in racers)
        {
            if(r.finished)
            {
                r.points += 15 - r.position;               
            }

            sortedRacers[r.position] = new DisplayRacer(r);
            sortedRacers[r.position].name = r.name;

            if (!r.finished)
                sortedRacers[r.position].position = -1;
        }

        //Send Sorted Racers to all Clients
        List<string> toSend = new List<string>();
        foreach (DisplayRacer dr in sortedRacers)
        {
            toSend.Add(dr.ToString());
        }
        DisplayNameUpdateMessage msg = new DisplayNameUpdateMessage();
        msg.players = toSend.ToArray();
        NetworkServer.SendToAll(UnetMessages.allPlayerFinishedMsg, msg);

        yield return null;

        foreach (NetworkRacer racer in racers)
        {
            NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.leaderboardPosMsg, new intMessage(racer.position));
        }

        //Wait again for message to be recieved
        yield return new WaitForSeconds(1f);

        //Give them 10 seconds to look at the score
        NetworkServer.SendToAll(UnetMessages.timerMsg, new TimerMessage(10));
        yield return new WaitForSeconds(10f);

        Debug.Log("Race is done!");
        WrapUp();
        finished = true;
    }

    private void DoDNF()
    {
        foreach(NetworkRacer racer in racers)
        {
            if(!racer.finished)
            {
                Debug.Log("Sent DNF to racer!");
                NetworkServer.SendToClient(racer.conn.connectionId, UnetMessages.finishRaceMsg, new EmptyMessage());
            }
        }
    }

    private void WrapUp()
    {
        NetworkServer.UnregisterHandler(UnetMessages.trackVoteMsg);

        foreach (NetworkRacer r in racers)
        {
            r.finished = false;
            r.ready = false;
            r.timer = 0;
            r.currentDistance = 0;
            r.totalDistance = 0;         
        }
    }

    private void RegisterHandles()
    {
        NetworkServer.RegisterHandler(UnetMessages.trackVoteMsg, OnTrackVote);

        //Power Ups
        NetworkServer.RegisterHandler(UnetMessages.recieveItemMsg, OnRecieveItem);
        NetworkServer.RegisterHandler(UnetMessages.useItemMsg, OnUseItem);
        NetworkServer.RegisterHandler(UnetMessages.useShieldMsg, OnUseShield);
        NetworkServer.RegisterHandler(UnetMessages.dropShieldMsg, OnDropShield);
    }

    //Called when a client drops the shield they're holding
    private void OnDropShield(NetworkMessage netMsg)
    {
        //Find the Racer that sent the message
        foreach (NetworkRacer r in racers)
        {
            if (netMsg.conn.connectionId == r.conn.connectionId)
            {
                r.ingameObj.GetComponent<KartNetworker>().currentItemDir = netMsg.ReadMessage<floatMessage>().value;
                r.ingameObj.GetComponent<KartNetworker>().dropShield++;
                return;
            }
        }
    }
    //Called when a client uses the shield they're holding
    private void OnUseShield(NetworkMessage netMsg)
    {
        //Find the Racer that sent the message
        foreach (NetworkRacer r in racers)
        {
            if (netMsg.conn.connectionId == r.conn.connectionId)
            {
                var kn = r.ingameObj.GetComponent<KartNetworker>();
                kn.useShield++;

                //Spawn Item over Network if possible
                if (gd.powerUps[kn.currentItem].onlineModel != null)
                {
                    GameObject go = (GameObject)Instantiate(gd.powerUps[kn.currentItem].onlineModel.gameObject,
                        r.ingameObj.position - (r.ingameObj.forward * r.ingameObj.GetComponent<kartItem>().itemDistance),
                        r.ingameObj.rotation);
                    go.transform.parent = r.ingameObj;
                    go.GetComponent<Rigidbody>().isKinematic = true;

                    SpawnObjectToClient(go, r.conn);
                }
                return;
            }
        }
    }
    //Called when a client uses the item they're holding
    private void OnUseItem(NetworkMessage netMsg)
    {
        //Find the Racer that sent the message
        foreach (NetworkRacer r in racers)
        {
            if (netMsg.conn.connectionId == r.conn.connectionId)
            {
                var kn = r.ingameObj.GetComponent<KartNetworker>();
                kn.useItem++;

                //Spawn Item over Network if possible
                if (gd.powerUps[kn.currentItem].onlineModel != null)
                {
                    GameObject go = (GameObject)Instantiate(gd.powerUps[kn.currentItem].onlineModel.gameObject,
                        r.ingameObj.position - (r.ingameObj.forward * r.ingameObj.GetComponent<kartItem>().itemDistance),
                        r.ingameObj.rotation);
                    go.transform.parent = r.ingameObj;
                    SpawnObjectToClient(go, r.conn);
                }
                return;
            }
        }
    }
    //Called when a Client recieves an Item
    private void OnRecieveItem(NetworkMessage netMsg)
    {
        //Find the Racer that sent the message
        foreach (NetworkRacer r in racers)
        {
            if (netMsg.conn.connectionId == r.conn.connectionId)
            {
                int value = netMsg.ReadMessage<intMessage>().value;
                r.ingameObj.GetComponent<KartNetworker>().currentItem = value;
                r.ingameObj.GetComponent<KartNetworker>().recieveItem++;
                return;
            }
        }
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
        foreach(NetworkRacer r in racers)
        {
            if(r.conn == conn)
            {
                racers.Remove(r);
                break;
            }
        }
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

    private void SpawnObjectToClient(GameObject go, NetworkConnection conn)
    {
        if (NetworkServer.localClientActive && conn.connectionId != FindObjectOfType<UnetClient>().client.connection.connectionId)
        {
            NetworkServer.SpawnWithClientAuthority(go, conn);
        }
        else
        {
            //Spawn the Item giving the server control
            NetworkServer.SpawnWithClientAuthority(go, racers[0].ingameObj.gameObject);
        }
    }

    public override void EndGamemode()
    {
        currentCup = -1;
        currentTrack = -1;
        currentRace = 1;
        lastcurrentRace = -1;

        FindObjectOfType<UnetHost>().EndClient("Server Closed");
    }

}
