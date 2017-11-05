using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace YogscartNetwork
{
    public enum GameState { Lobby, Loading, Game };
    public enum GamemodeEnum { Race, Battle, Yogball};

    public class Host : Client
    {
        public GameState currentState = GameState.Lobby;
        public GamemodeEnum currentGamemode { get; private set; }

        //Server Settings
        public ServerSettings serverSettings;

        //Players who will race in the next game
        public List<NetworkRacer> finalPlayers;
        //People who want to join the race but can't cause it's full or we're not in the lobby
        public List<NetworkConnection> waitingPlayers;
        //People who are choosing a character 
        public List<NetworkConnection> possiblePlayers;
        //People who have said they don't want to race, Will be put at the back of waiting list on next race
        public List<NetworkConnection> rejectedPlayers;

        public override void RegisterHandlers()
        {
            gd = FindObjectOfType<CurrentGameData>();
            networkSelection = FindObjectOfType<NetworkSelection>();

            //Reset Everything
            finalPlayers = new List<NetworkRacer>();
            waitingPlayers = new List<NetworkConnection>();
            possiblePlayers = new List<NetworkConnection>();
            rejectedPlayers = new List<NetworkConnection>();

            if (serverSettings.hostAsPlayer)
            {
                base.RegisterHandlers();

                ClientScene.Ready(client.connection);

                LoadOut loadOut = CurrentGameData.currentChoices[0];
                finalPlayers.Add(new NetworkRacer(gd.playerName, loadOut.character, loadOut.hat, loadOut.kart, loadOut.wheel, 0, true, null));

                //Update the Player Lists
                UpdatePlayerInfoList();

                isRacing = true;
            }

            //Register Host Specific Messages
            NetworkServer.RegisterHandler(UnetMessages.versionMsg, OnVersion);
            NetworkServer.RegisterHandler(UnetMessages.rejectPlayerUpMsg, OnRejection);
            NetworkServer.RegisterHandler(UnetMessages.playerInfoMsg, OnPlayerUp);
            NetworkServer.RegisterHandler(UnetMessages.playerInfoUpdateMsg, OnPlayerUpdate);
            NetworkServer.RegisterHandler(UnetMessages.pingMsg, OnServerPing);

            //Hide Input Manager
            InputManager.SetInputState(InputManager.InputState.Locked);
            InputManager.SetToggleState(InputManager.ToggleState.Locked);

            //Load the Lobby
            StartCoroutine(OnReturnLobby());
        }

        protected override IEnumerator DoLobbyLoop()
        {
            while (isRacing && SceneManager.GetActiveScene().name == "Lobby")
            {
                int[] pingData = new int[finalPlayers.Count];
                for (int i = 0; i < finalPlayers.Count; i++)
                    pingData[i] = finalPlayers[i].ping;

                NetworkServer.SendToAll(UnetMessages.pingMsg, new IntArrayMessage(pingData));
                yield return new WaitForSeconds(5f);
            }
        }

        public override void EndClient(string message)
        {
            StopHost();
            base.EndClient(message);
        }

        //------------------------------------------------------------------------------
        // Functionality
        //------------------------------------------------------------------------------

        public override void Update()
        {
            if (currentState == GameState.Lobby)
            {
                if(serverSettings.automatic && finalPlayers.Count >= serverSettings.minPlayers)
                {
                    StartGamemode();
                }
            }

            base.Update();
        }

        public void StartGamemode()
        {
            if (finalPlayers.Count > 0)
            {
                currentState = GameState.Loading;
                StartCoroutine(StartGame());
            }
        }

        private IEnumerator StartGame()
        {
            //Tell clients they have ten seconds left
            NetworkServer.SendToAll(UnetMessages.timerMsg, new IntMessage(10));
            OnTimer(10);

            networkSelection.ChangeState(NetworkSelection.MenuState.Loading);

            yield return new WaitForSeconds(10.2f);

            networkSelection.ChangeState(NetworkSelection.MenuState.Gamemode);

            //Tell clients to close everything
            NetworkServer.SendToAll(UnetMessages.clearMsg, new EmptyMessage());
            OnClear();

            //Wait for message to be sent
            yield return new WaitForSeconds(1f);

            //Clean up
            waitingPlayers.AddRange(possiblePlayers);
            waitingPlayers.AddRange(rejectedPlayers);

            possiblePlayers = new List<NetworkConnection>();
            rejectedPlayers = new List<NetworkConnection>();

            //Create Gamemode Objects on client and host
            currentState = GameState.Game;

            switch (currentGamemode)
            {
                case GamemodeEnum.Race:
                    NetworkServer.SendToAll(UnetMessages.raceGamemodeMsg, new EmptyMessage());
                    gameMode = OnGamemodeRace();
                    break;
            }

            //Pass global values to client and host

            //Wait for gamemode to finish
            while (!gameMode.finished)
            {
                yield return null;
            }

            currentState = GameState.Lobby;

            //Clean Up
            NetworkServer.SendToAll(UnetMessages.cleanUpMsg, new EmptyMessage());
            OnGamemodeCleanup();

            yield return new WaitForSeconds(0.5f);

            //Return to lobby
            NetworkServer.SendToAll(UnetMessages.returnLobbyMsg, new EmptyMessage());
            yield return OnReturnLobby();

            OnHostReturnToLobby();
        }

        //------------------------------------------------------------------------------
        // Server Function
        //------------------------------------------------------------------------------

        // Called when a client disconnects
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("Client disconnected! " + conn.address);    

            //Remove players for Final Players
            for (int i = 0; i < finalPlayers.Count; i++)
            {
                if (finalPlayers[i].conn == conn)
                {
                    finalPlayers.RemoveAt(i);

                    //Destroy any Player Items
                    NetworkServer.DestroyPlayersForConnection(conn);

                    CheckForSpaces();

                    //Update the Player Lists
                    UpdatePlayerInfoList();

                    //Tell the gamemode
                    gameMode.OnServerDisconnect(conn);

                    return;
                }
            }

            //Remove players for Possible Players
            for (int i = 0; i < possiblePlayers.Count; i++)
            {
                if (possiblePlayers[i] == conn)
                {
                    possiblePlayers.RemoveAt(i);

                    CheckForSpaces();
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

        //If there are waiting players, fill the spaces
        protected void CheckForSpaces()
        {
            if (waitingPlayers.Count > 0)
            {
                int currentPlayerCount = finalPlayers.Count + possiblePlayers.Count;
                if (currentPlayerCount < serverSettings.maxPlayers)
                {
                    for (int i = 0; i < Mathf.Min(serverSettings.maxPlayers - currentPlayerCount, waitingPlayers.Count); i++)
                    {
                        AcceptedMessage ackMsg = new AcceptedMessage();

                        // Tell Player they can join
                        ackMsg.playerUp = true;
                        possiblePlayers.Add(waitingPlayers[0]);

                        //Send the ACK
                        NetworkServer.SendToClient(waitingPlayers[0].connectionId, UnetMessages.acceptedMsg, ackMsg);

                        //Remove them from the waiting list
                        waitingPlayers.RemoveAt(0);

                    }
                }
            }
        }

        //------------------------------------------------------------------------------
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("OnServerAddPlayer " + conn.address);

            //Check for Client
            for (int i = 0; i < finalPlayers.Count; i++)
            {
                NetworkRacer nr = finalPlayers[i] as NetworkRacer;
                if ((nr.conn != null && nr.conn.connectionId == conn.connectionId) || (conn.hostId == client.connection.hostId))
                {
                    GameObject player = gameMode.OnServerAddPlayer(nr, playerPrefab);

                    //Set Transform and Player Name
                    nr.ingameObj = player.transform;
                    nr.ingameObj.GetComponent<KartNetworker>().kartPlayerName = nr.playerName;

                    NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
                    return;
                }
            }
        }      

        //------------------------------------------------------------------------------
        // Message Delegates
        //------------------------------------------------------------------------------

        // Called when a Version Message is recieved by a client
        private void OnVersion(NetworkMessage netMsg)
        {
            Debug.Log("Client Version Check! " + netMsg.conn.address);
            VersionMessage msg = netMsg.ReadMessage<VersionMessage>();

            //If the versions do not match kick client
            if (msg.version != gd.version)
            {
                string errorMessage = "Your version does not match the servers! Please update to " + gd.version;
                ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
                NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);
                Debug.Log("Client Failed Version Check! " + netMsg.conn.address);
            }
            else//If they do match add client to game
            {
                StartCoroutine(AcceptPlayer(netMsg.conn));
                Debug.Log("Client Passed Version Check! " + netMsg.conn.address);
            }
        }

        //------------------------------------------------------------------------------
        //Used to give add time between messages to avoid flooding the client
        private IEnumerator AcceptPlayer(NetworkConnection conn)
        {
            Debug.Log("AcceptPlayer " + conn.address);

            AcceptedMessage ackMsg = new AcceptedMessage();
       
            //Check to see if Client can join players
            if (currentState != GameState.Lobby || finalPlayers.Count >= 12)
            {
                //Add Player to Waiting List
                waitingPlayers.Add(conn);

                StartCoroutine(ActualUpdateAllClientPlayerListsOnConn(conn));
            }
            else
            {
                //Tell Player they can join
                ackMsg.playerUp = true;
                possiblePlayers.Add(conn);
            }

            //Wait for lobby message to be recieved
            yield return new WaitForSeconds(0.2f);

            //Send the ACK
            NetworkServer.SendToClient(conn.connectionId, UnetMessages.acceptedMsg, ackMsg);

            //Wait for message to be recieved
            yield return new WaitForSeconds(0.2f);

            //Handle Client's that Join
            switch (currentState)
            {
                case GameState.Lobby:
                    NetworkServer.SendToClient(conn.connectionId, UnetMessages.returnLobbyMsg, new EmptyMessage());
                    break;
                case GameState.Loading:
                    NetworkServer.SendToClient(conn.connectionId, UnetMessages.returnLobbyMsg, new EmptyMessage());
                    break;
                case GameState.Game:
                   
                    //Load whatever Gamemode we're on
                    switch (currentGamemode)
                    {
                        case GamemodeEnum.Race:
                            NetworkServer.SendToClient(conn.connectionId, UnetMessages.raceGamemodeMsg, new EmptyMessage());
                            break;
                    }

                    yield return new WaitForSeconds(0.5f);

                    //Load whatever level we're on now
                    NetworkServer.SendToClient(conn.connectionId, UnetMessages.loadLevelIDMsg, new StringMessage(SceneManager.GetActiveScene().name));             
                    yield return new WaitForSeconds(0.5f);

                    //Handle Spectator for this gamemode
                    gameMode.OnServerConnect(conn);
                    break;
            }
        }

        //------------------------------------------------------------------------------
        // Called when a client rejects selecting a character
        private void OnRejection(NetworkMessage netMsg)
        {
            Debug.Log("OnRejection " + netMsg.conn.address);
            if (!possiblePlayers.Contains(netMsg.conn))
            {
                KickPlayer(netMsg.conn);
            }

            StartCoroutine(ActualUpdateAllClientPlayerListsOnConn(netMsg.conn));

            //Add Player to Waiting List
            possiblePlayers.Remove(netMsg.conn);
            rejectedPlayers.Add(netMsg.conn);
        }

        //------------------------------------------------------------------------------
        // Called when a client finishes selecting a character
        private void OnPlayerUp(NetworkMessage netMsg)
        {
            Debug.Log(netMsg.conn.address + " has sent their Loadout!");

            if (!possiblePlayers.Contains(netMsg.conn))
            {
                KickPlayer(netMsg.conn);
            }
            possiblePlayers.Remove(netMsg.conn);

            PlayerInfoMessage msg = netMsg.ReadMessage<PlayerInfoMessage>();
            finalPlayers.Add(new NetworkRacer(msg.displayName, msg.character, msg.hat, msg.kart, msg.wheel, finalPlayers.Count, false, netMsg.conn));

            //Tell Client's to add Player
            UpdatePlayerInfoList();
        }

        //------------------------------------------------------------------------------
        // Called when a client updates their loadout
        private void OnPlayerUpdate(NetworkMessage netMsg)
        {
            Debug.Log(netMsg.conn.address + " has sent their Loadout!");

            int racerID = -1;

            //Check that Racer exists
            for(int i = 0; i < finalPlayers.Count; i++)
            {
                if(finalPlayers[i].conn == netMsg.conn)
                {
                    racerID = i;
                    break;
                }
            }

            if (racerID == -1)
            {
                KickPlayer(netMsg.conn);
            }

            PlayerInfoMessage msg = netMsg.ReadMessage<PlayerInfoMessage>();
            UpdatePlayerInfo(finalPlayers[racerID], new LoadOut(msg.character, msg.hat, msg.kart, msg.wheel));

            //Tell Client's to add Player
            UpdatePlayerInfoList();
        }

        //------------------------------------------------------------------------------
        // Useful Functions
        //------------------------------------------------------------------------------

        public void UpdatePlayerInfo(NetworkRacer _racer, LoadOut _loadout)
        {
            _racer.character = _loadout.character;
            _racer.hat = _loadout.hat;
            _racer.kart = _loadout.kart;
            _racer.wheel = _loadout.wheel;
        }

        public void UpdatePlayerInfoList()
        {
            networkSelection.playerList = new List<PlayerInfo>();
            foreach (NetworkRacer networkRacer in finalPlayers)
            {
                networkSelection.playerList.Add(new PlayerInfo(networkRacer.playerName, networkRacer.character, networkRacer.hat,
                    networkRacer.kart, networkRacer.wheel));
            }

            StartCoroutine(ActualUpdateAllClientPlayerLists());
        }

        private IEnumerator ActualUpdateAllClientPlayerLists()
        {
            NetworkServer.SendToAll(UnetMessages.clearPlayerInfo, new EmptyMessage());

            //Wait for message to send
            yield return new WaitForSeconds(0.2f);

            foreach(PlayerInfo playerInfo in networkSelection.playerList)
            {
                NetworkServer.SendToAll(UnetMessages.addPlayerInfo, new PlayerInfoMessage(playerInfo));
                yield return null;
            }
        }

        private IEnumerator ActualUpdateAllClientPlayerListsOnConn(NetworkConnection conn)
        {
            NetworkServer.SendToClient(conn.connectionId, UnetMessages.clearPlayerInfo, new EmptyMessage());

            //Wait for message to send
            yield return new WaitForSeconds(0.2f);

            foreach (PlayerInfo playerInfo in networkSelection.playerList)
            {
                NetworkServer.SendToClient(conn.connectionId, UnetMessages.addPlayerInfo, new PlayerInfoMessage(playerInfo));
                yield return null;
            }
        }

        public void ChangeGamemode(int _newGamemode)
        {
            currentGamemode = (GamemodeEnum)MathHelper.NumClamp(_newGamemode, 0, gd.onlineGameModes.Length); ;
            networkSelection.currentGamemode = (int)currentGamemode;

            NetworkServer.SendToAll(UnetMessages.changeGamemode, new IntMessage((int)currentGamemode));
        }

        public void KickPlayer(NetworkConnection _conn)
        {
            StartCoroutine(ActualKickPlayer(_conn));
        }

        private IEnumerator ActualKickPlayer(NetworkConnection _conn)
        {
            Debug.Log(_conn.address + " is a big cheater");
            string errorMessage = "'And if I ever see you here again, Wreck-It Ralph, I'll lock you in my Fungeon!'." + System.Environment.NewLine + "Error: Somethings gone wrong! You've sent a message to the server that you weren't suppose to. Either it's a bug or you're a dirty cheater. Eitherway you've been kicked.";
            ClientErrorMessage ackMsg = new ClientErrorMessage(errorMessage);
            NetworkServer.SendToClient(_conn.connectionId, UnetMessages.clientErrorMsg, ackMsg);

            yield return new WaitForSeconds(0.25f);

            //If they're still here kick them
            if(_conn.isConnected)
            {
                _conn.Disconnect();
            }
        }

        private void OnHostReturnToLobby()
        {
            CheckForSpaces();

            //Update the Player Lists
            UpdatePlayerInfoList();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("OnServerConnect " + conn.address);
        }

        private void OnServerPing(NetworkMessage netMsg)
        {
            int racerID = -1;

            //Check that Racer exists
            for (int i = 0; i < finalPlayers.Count; i++)
            {
                if (finalPlayers[i].conn == netMsg.conn)
                {
                    racerID = i;
                    break;
                }
            }

            if (racerID == -1)
            {
                KickPlayer(netMsg.conn);
            }

            IntMessage msg = netMsg.ReadMessage<IntMessage>();

            Debug.Log(netMsg.conn.address + " has sent their Ping! " + msg.value + "ms");

            finalPlayers[racerID].ping = msg.value;
            networkSelection.playerList[racerID].ping = msg.value;
        }
    }
}