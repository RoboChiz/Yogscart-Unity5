using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

public class UnetHost : UnetClient
{
    public enum GameState { Lobby, Loading, Race };
    public GameState currentState = GameState.Lobby;

    public List<NetworkRacer> finalPlayers;
    public List<NetworkConnection> waitingPlayers;
    public List<NetworkConnection> possiblePlayers;

    public override void RegisterHandlers()
    {
        NetworkServer.RegisterHandler(UnetMessages.versionMsg, OnVersion);
        NetworkServer.RegisterHandler(UnetMessages.playerInfoMsg, OnPlayerInfo);
    }

    public override NetworkClient StartHost()
    {
        var output = base.StartHost();

        finalPlayers = new List<NetworkRacer>();
        NetworkRacer newRacer = new NetworkRacer(0, -1, CurrentGameData.currentChoices[0], 0);
        newRacer.name = PlayerPrefs.GetString("playerName", "Player");
        finalPlayers.Add(newRacer);
        UpdateDisplayNames();

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
            AcceptedMessage ackMsg = new AcceptedMessage();
            ackMsg.currentState = currentState;

            //Check to see if Client can join players
            if (currentState != GameState.Lobby || finalPlayers.Count >= 12)
            {
                //Add Player to Waiting List
                waitingPlayers.Add(netMsg.conn);
            }
            else
            {
                //Tell Player they can join
                ackMsg.playerUp = true;
            }

            NetworkServer.SendToClient(netMsg.conn.connectionId, UnetMessages.acceptedMsg, ackMsg);
        }
    }

    // Called when a Player Info Message is recieved by a client
    private void OnPlayerInfo(NetworkMessage netMsg)
    {
        PlayerInfoMessage msg = netMsg.ReadMessage<PlayerInfoMessage>();

        NetworkRacer newRacer = new NetworkRacer(0, -1, msg.character, msg.hat, msg.kart, msg.wheel, finalPlayers.Count);
        newRacer.conn = netMsg.conn;
        newRacer.name = msg.displayName;

        finalPlayers.Add(newRacer);

        UpdateDisplayNames();
    }

    // Called when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);

        //Remove players for Final Players
        for (int i = 0; i < finalPlayers.Count; i++)
        {
            if (finalPlayers[i].conn == conn)
            {
                finalPlayers.RemoveAt(i);
                break;
            }
        }

        UpdateDisplayNames();
    }

    //Send a message with the current players
    private void UpdateDisplayNames()
    {
        List<DisplayName> nDisplayList = new List<DisplayName>();
        List<string> toSend = new List<string>();

        foreach (NetworkRacer racer in finalPlayers)
        {
            int ping = -1;
            nDisplayList.Add(new DisplayName(racer.name, racer.Character, ping, racer.team, racer.points));
            toSend.Add(nDisplayList[nDisplayList.Count - 1].ToString());
        }

        DisplayNameUpdateMessage msg = new DisplayNameUpdateMessage();
        msg.players = toSend.ToArray();
        NetworkServer.SendToAll(UnetMessages.displayNameUpdateMsg, msg);

        Debug.Log("Sent a Display Name Update");
        FindObjectOfType<NetworkGUI>().finalPlayers = nDisplayList;
    }


    void Update()
    {

    }
}

[System.Serializable]
public class NetworkRacer : Racer
{
    public NetworkConnection conn;
    //The name of the Player
    public string name = "";

    public NetworkRacer(int hum, int ais, LoadOut lo, int p) : base(hum, ais, lo, p){}
    public NetworkRacer(int hum, int ais, int ch, int h, int k, int w, int p) : base(hum, ais, ch, h, k, w, p){}
}