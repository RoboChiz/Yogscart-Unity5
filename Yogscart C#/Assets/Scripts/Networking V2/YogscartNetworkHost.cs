using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System;

namespace YogscartNetwork
{
    public enum GameState { Lobby, Loading, Game };
   
    public class Host : Client
    {
        public GameState currentState = GameState.Lobby;

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
                finalPlayers.Add(new NetworkRacer(0, -1, 0, 0, 0, 0, 0)); // TODO: Fix this!
            }

            //Register Host Specific Messages
            NetworkServer.RegisterHandler(UnetMessages.versionMsg, OnVersion);
            NetworkServer.RegisterHandler(UnetMessages.rejectPlayerUpMsg, OnRejection);
        }

        public override void EndClient(string message)
        {
            StopHost();
            base.EndClient(message);
        }

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
            int currentPlayerCount = finalPlayers.Count + possiblePlayers.Count;
            if (currentPlayerCount < serverSettings.maxPlayers)
            {
                for (int i = 0; i < serverSettings.maxPlayers - currentPlayerCount; i++)
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
                Debug.Log("Client Passed Version Check! " + netMsg.conn.address);
            }
            else//If they do match add client to game
            {
                StartCoroutine(AcceptPlayer(netMsg.conn));
                Debug.Log("Client Failed Version Check! " + netMsg.conn.address);
            }
        }

        //------------------------------------------------------------------------------
        //Used to give add time between messages to avoid flooding the client
        private IEnumerator AcceptPlayer(NetworkConnection conn)
        {
            AcceptedMessage ackMsg = new AcceptedMessage();

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
            }

            //Pass current server state info to Player


            //Send the ACK
            NetworkServer.SendToClient(conn.connectionId, UnetMessages.acceptedMsg, ackMsg);

            //Wait for message to be recieved
            yield return new WaitForSeconds(0.2f);

        }

        // Called when a client rejects selecting a character
        private void OnRejection(NetworkMessage netMsg)
        {
            //Add Player to Waiting List
            rejectedPlayers.Add(netMsg.conn);
        }
    }
}