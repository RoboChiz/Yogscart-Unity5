using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System;

public class UnetHost : UnetClient
{
    public enum GameState { Lobby, Loading, Race };
    public GameState currentState = GameState.Lobby;

    public bool runClient = true;

    //Players who will race in the next game
    public List<NetworkRacer> finalPlayers;
    //People who want to join the race but can't cause it's full or we're not in the lobby
    public List<NetworkConnection> waitingPlayers;
    //People who are choosing a character 
    public List<NetworkConnection> possiblePlayers;
    //People who have said they don't want to race, Will be put at the back of waiting list on next race
    public List<NetworkConnection> rejectedPlayers;

    private List<DisplayName> displayNames;

    private GameMode hostGamemode;
    private int gamemodeInt;

    public int choosingCount { get; private set; } //Used to tell Host how many people have not selected a character

    public ServerSettings settings;
    public NetworkSelection networkUI;

    public override void RegisterHandlers()
    {
        if (runClient)
        {
            base.RegisterHandlers();
            ClientScene.Ready(client.connection);
        }
            
        NetworkServer.RegisterHandler(UnetMessages.versionMsg, OnVersion);
        NetworkServer.RegisterHandler(UnetMessages.playerInfoMsg, OnPlayerInfo);
        NetworkServer.RegisterHandler(UnetMessages.rejectPlayerUpMsg, OnRejectPlayerUp);
        NetworkServer.RegisterHandler(UnetMessages.readyMsg, OnReady);
    }

    public override NetworkClient StartHost()
    {
        var output = base.StartHost();
        
        if(output == null)
        {
            //Error Server Not Started!
            FindObjectOfType<NetworkGUI>().CloseServer();
            FindObjectOfType<NetworkGUI>().PopUp("Error! Server could not be started");
            return null;
        }       

        finalPlayers = new List<NetworkRacer>();
        waitingPlayers = new List<NetworkConnection>();
        possiblePlayers = new List<NetworkConnection>();
        rejectedPlayers = new List<NetworkConnection>();
        displayNames = new List<DisplayName>();
        choosingCount = 0;

        NetworkRacer newRacer = new NetworkRacer(-2, -1, CurrentGameData.currentChoices[0], 0);
        newRacer.name = PlayerPrefs.GetString("playerName", "Player");
        newRacer.conn = client.connection;

        finalPlayers.Add(newRacer);

        UpdateDisplayNames();
        //Manual override to avoid sending message to yourself
        FindObjectOfType<NetworkGUI>().finalPlayers = displayNames;

        return output;
    } 

    // Called when a Version Message is recieved by a client
    private void OnVersion(NetworkMessage netMsg)
    {
        VersionMessage msg = netMsg.ReadMessage<VersionMessage>();

        //If the versions do not match kick client
        if (msg.version != GetComponent<CurrentGameData>().version)
        {
            string errorMessage = "Your version does not match the servers! Please update to " + GetComponent<CurrentGameData>().version;
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
        }
        else//If they do match add client to game
        {
            StartCoroutine(AcceptPlayer(netMsg.conn));
        }
    }

    //Used to give add time between messages to avoid flooding the client
    private IEnumerator AcceptPlayer(NetworkConnection conn)
    {
        AcceptedMessage ackMsg = new AcceptedMessage();
        ackMsg.currentState = (YogscartNetwork.GameState)currentState;

        //Check to see if Client can join players
        if (currentState != GameState.Lobby || finalPlayers.Count >= 12)
        {
            //Add Player to Waiting List
            waitingPlayers.Add(conn);
        }
        else
        {
            //Tell Player they can join
            ackMsg.playerUp = true;
            possiblePlayers.Add(conn);

            choosingCount++;
        }

        NetworkServer.SendToClient(conn.connectionId, UnetMessages.acceptedMsg, ackMsg);

        //Wait for message to be recieved
        yield return new WaitForSeconds(0.2f);

        if (currentState == GameState.Race && hostGamemode != null)
        {
            //Tell the client what the gamemode is
            LoadGamemodeMessage gmMsg = new LoadGamemodeMessage();
            gmMsg.gamemode = gamemodeInt;
            NetworkServer.SendToAll(UnetMessages.loadGamemodeMsg, gmMsg);

            //Wait for message to be recieved
            yield return new WaitForSeconds(0.2f);

            //Tell Host Script that a Player's joined
            hostGamemode.OnServerConnect(conn);
        }

    }

    // Called when a Player Info Message is recieved by a client
    private void OnPlayerInfo(NetworkMessage netMsg)
    {
        PlayerInfoMessage msg = netMsg.ReadMessage<PlayerInfoMessage>();

        bool playerFound = false;

        for(int i = 0; i < possiblePlayers.Count; i++)
        {
            if(possiblePlayers[i] == netMsg.conn)
            {
                possiblePlayers.RemoveAt(i);
                choosingCount--;
                playerFound = true;
                break;
            }
        }

        if(!playerFound)
        {
            string errorMessage = "'And if I ever see you here again, Wreck-It Ralph, I'll lock you in my Fungeon!'." + System.Environment.NewLine + "Error: Somethings gone wrong! You've sent a message to the server that you weren't suppose to. Either it's a bug or you're a dirty cheater. Eitherway you've been kicked.";
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
        }

        NetworkRacer newRacer = new NetworkRacer(-2, -1, msg.character, msg.hat, msg.kart, msg.wheel, finalPlayers.Count);
        newRacer.conn = netMsg.conn;
        newRacer.name = msg.displayName;

        finalPlayers.Add(newRacer);

        if (currentState == GameState.Lobby)
        {
            UpdateDisplayNames();
            SendDisplayNames();
        }
    }
    // Called when a Player Up Message is rejected
    private void OnRejectPlayerUp(NetworkMessage netMsg)
    {
        for(int i = 0; i < possiblePlayers.Count; i++)
        {
            if(possiblePlayers[i] == netMsg.conn)
            {
                //Send the Display Names list to client
                SendDisplayNames(possiblePlayers[i]);
                //Add them to rejected Players List
                rejectedPlayers.Add(possiblePlayers[i]);
                possiblePlayers.RemoveAt(i);
                Debug.Log("Possible Player " + i.ToString() + " has rejected a player up. Rejected Players count:" + rejectedPlayers.Count.ToString());

                break;
            }
        }
    }       

    // Called when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {       
        //Remove players for Final Players
        for (int i = 0; i < finalPlayers.Count; i++)
        {
            if (finalPlayers[i].conn == conn)
            {
                finalPlayers.RemoveAt(i);
                //Destroy any Player Items
                NetworkServer.DestroyPlayersForConnection(conn);

                if (currentState != GameState.Race)
                {
                    //Only update Display Names if Player mattered
                    UpdateDisplayNames();
                    SendDisplayNames();                  
                }
                else
                {
                    hostGamemode.OnServerDisconnect(conn);
                }
                return;
            }
        }

        //Remove players for Possible Players
        for (int i = 0; i < possiblePlayers.Count; i++)
        {
            if(possiblePlayers[i] == conn)
            {
                possiblePlayers.RemoveAt(i);
                return;
            }
        }

        //Remove players for Rejected Players
        for (int i = 0; i < rejectedPlayers.Count; i++)
        {
            if (rejectedPlayers[i] == conn)
            {
                rejectedPlayers.RemoveAt(i);
                return;
            }
        }

        //Remove players for Waiting Players
        for (int i = 0; i < waitingPlayers.Count; i++)
        {
            if (waitingPlayers[i] == conn)
            {
                waitingPlayers.RemoveAt(i);
                return;
            }
        }
    }

    //Update the Display Names List
    private void UpdateDisplayNames()
    {
        //Reset displayNames
        displayNames = new List<DisplayName>();

        foreach (NetworkRacer racer in finalPlayers)
        {
            displayNames.Add(new DisplayName(racer.name, racer.Character, racer.ping, racer.team, racer.points));
        }
    }

    //Send a message with the current players to all Players
    private void SendDisplayNames()
    {
        List<string> toSend = new List<string>();
        foreach (DisplayName dn in displayNames)
        {
            toSend.Add(dn.ToString());
        }

        DisplayNameUpdateMessage msg = new DisplayNameUpdateMessage();
        msg.players = toSend.ToArray();
        NetworkServer.SendToAll(UnetMessages.displayNameUpdateMsg, msg);
    }

    //Send a message with the current players to a specific Player
    private void SendDisplayNames(NetworkConnection conn)
    {
        List<string> toSend = new List<string>();
        foreach (DisplayName dn in displayNames)
        {
            toSend.Add(dn.ToString());
        }

        DisplayNameUpdateMessage msg = new DisplayNameUpdateMessage();
        msg.players = toSend.ToArray();
        NetworkServer.SendToClient(conn.connectionId,UnetMessages.displayNameUpdateMsg, msg);
    }

    public override void Update()
    {
        base.Update();

        if(currentState == GameState.Lobby)
        {
            while(waitingPlayers.Count > 0 && finalPlayers.Count + waitingPlayers.Count < 12)
            {          
                //Move player to new list  
                possiblePlayers.Add(waitingPlayers[0]);
                waitingPlayers.RemoveAt(0);
                //Send a player up request to the person
                SendPlayerUp(possiblePlayers[possiblePlayers.Count - 1]);
                Debug.Log("Waiting Players Count:" + waitingPlayers.Count.ToString() + " Possible Player count: " + possiblePlayers.Count.ToString());
            }
        }
    }

    private void SendPlayerUp(NetworkConnection conn)
    {
        EmptyMessage msg = new EmptyMessage();
        NetworkServer.SendToClient(conn.connectionId, UnetMessages.playerUpMsg, msg);
    }

    public void StartGame(int gamemode)
    {
        currentState = GameState.Loading;   
        StartCoroutine(ActualStartGame(gamemode));
    }

    private IEnumerator ActualStartGame(int gamemode)
    {
        //Force all Players to send their Player Info
        EmptyMessage msg = new EmptyMessage();
        foreach (NetworkConnection conn in possiblePlayers)
        {
            NetworkServer.SendToClient(conn.connectionId, UnetMessages.forceCharacterSelectMsg, msg);
        }

        //Wait a frame
        yield return null;

        //Move all rejected players to the back of the waiting list
        waitingPlayers.AddRange(rejectedPlayers);
        rejectedPlayers = new List<NetworkConnection>();

        //While until all Possible Players have replied, wait for 1 second max
        float startTime = Time.time;
        while(possiblePlayers.Count > 0 && Time.time - startTime < 1)
        {
            yield return null;
        }

        //Kick any one who hasn't replied
        string errorMessage = "You haven't provided the server with a character choice fast enough. So you've been kicked... Sorry";
        foreach (NetworkConnection conn in possiblePlayers)
        {        
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
        }

        //Wait a frame
        yield return null;

        //Reset Network Racers values
        foreach(NetworkRacer r in finalPlayers)
        {
            r.ready = false;
            r.finished = false;
        }

        //Start Gamemode in Host Script
        hostGamemode = OnlineGameModeScripts.AddHostScript(gamemode);
        gamemodeInt = gamemode;

        //Tell all clients to load the appropriate Client Scripts
        LoadGamemodeMessage gmMsg = new LoadGamemodeMessage();
        gmMsg.gamemode = gamemode;
        NetworkServer.SendToAll(UnetMessages.loadGamemodeMsg, gmMsg);

        //Wait a second for message to be recieved
        yield return new WaitForSeconds(1f);

        currentState = GameState.Race;

        //Start host script
        hostGamemode.StartGameMode();

        //Wait for the Gamemode to Finish
        while(!hostGamemode.finished)
        {
            yield return null;
        }

        //Tell the clients to clean up
        NetworkServer.SendToAll(UnetMessages.returnLobbyMsg, new EmptyMessage());

        yield return null;

        //Delete the host gamemode if it isn't a client (No more cleanup needed)
        if (client != null)
            Destroy(hostGamemode);

        ResetLobby();
    }

    private void ResetLobby()
    {
        UpdateDisplayNames();
        SendDisplayNames();

        choosingCount = 0;
        currentState = GameState.Lobby;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        for(int i = 0; i < finalPlayers.Count; i++)
        {
            NetworkRacer nr = finalPlayers[i] as NetworkRacer;
            if(nr.conn.connectionId == conn.connectionId)
            {
                GameObject player = hostGamemode.OnServerAddPlayer(nr,playerPrefab);
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
                break;
            }
        }       
    }

    private void OnReady(NetworkMessage netMsg)
    {
        for(int i = 0; i < finalPlayers.Count; i++)
        {
            NetworkRacer racer = finalPlayers[i] as NetworkRacer;
            if(racer.conn.connectionId == finalPlayers[i].conn.connectionId)
            {
                racer.ready = true;
            }            
        }
    }

    public override void EndClient(string message)
    {
        StopHost();
        base.EndClient(message);
    }

}

[System.Serializable]
public class NetworkRacer : Racer
{
    public NetworkConnection conn;
    public bool ready = false;
    public int ping = 9999;

    //The name of the Player
    public string name = "";

    public NetworkRacer(int hum, int ais, LoadOut lo, int p) : base(hum, ais, lo, p){}
    public NetworkRacer(int hum, int ais, int ch, int h, int k, int w, int p) : base(hum, ais, ch, h, k, w, p){}
}