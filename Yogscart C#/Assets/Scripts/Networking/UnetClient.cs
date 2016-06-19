using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class UnetClient : NetworkManager
{

    //Register the handlers required for the client
    public virtual void RegisterHandlers()
    {
        client.RegisterHandler(UnetMessages.acceptedMsg, OnClientAccepted);
        client.RegisterHandler(UnetMessages.clientErrorMsg, OnCustomError);
        client.RegisterHandler(UnetMessages.playerUpMsg, OnPlayerUp);
        client.RegisterHandler(UnetMessages.displayNameUpdateMsg, OnDisplayNameUpdate);
    }

    // Called when connected to a server
    public override void OnClientConnect(NetworkConnection conn)
    {
        VersionMessage myMsg = new VersionMessage();
        myMsg.version = GetComponent<CurrentGameData>().version;
        client.Send(UnetMessages.versionMsg, myMsg);
    }

    // called when disconnected from a server
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        EndClient("You have disconnected from the server.");
    }

    // Called when a network error occurs
    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        EndClient("ERROR\n" + ((NetworkConnectionError)errorCode).ToString());
    }

    // Called when a host accepts the client (Correct Version .etc)
    public void OnClientAccepted(NetworkMessage netMsg)
    {
        ClientScene.Ready(netMsg.conn);

        AcceptedMessage msg = netMsg.ReadMessage<AcceptedMessage>();

        if (msg.playerUp)
        {
            FindObjectOfType<NetworkGUI>().PlayerUp();
        }
        else if (msg.currentState == UnetHost.GameState.Lobby)
        {
            FindObjectOfType<NetworkGUI>().state = NetworkGUI.ServerState.Lobby;
        }       
    }

    // Called when a custom network error occurs
    public void OnCustomError(NetworkMessage netMsg)
    {
        ClientErrorMessage msg = netMsg.ReadMessage<ClientErrorMessage>();
        EndClient(msg.message);
    }

    // Called when a client needs to pick a character
    public void OnPlayerUp(NetworkMessage netMsg)
    {
        FindObjectOfType<NetworkGUI>().PlayerUp();
    }

    // Called when a client needs to pick a character
    public void SendPlayerInfo(PlayerInfoMessage myMsg)
    {
        client.Send(UnetMessages.playerInfoMsg, myMsg);
    }

    // Called when server updates current racers
    public void OnDisplayNameUpdate(NetworkMessage netMsg)
    {
        Debug.Log("Recieved a Display Name Update");
        DisplayNameUpdateMessage msg = netMsg.ReadMessage<DisplayNameUpdateMessage>();

        List<DisplayName> nList = new List<DisplayName>();
        foreach(string val in msg.players)
        {
            DisplayName nName = new DisplayName();
            nName.ReadFromString(val);
            nList.Add(nName);
        }

        FindObjectOfType<NetworkGUI>().finalPlayers = nList;
    }

    public void EndClient(string message)
    {
        StopClient();

        //If not at Main Menu, Load it
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Main_Menu"))
            SceneManager.LoadScene("Main_Menu");

        //Turn the Main Menu off
        FindObjectOfType<MainMenu>().enabled = false;

        //Load the disconnect screen
        NetworkGUI gui = FindObjectOfType<NetworkGUI>();
        gui.enabled = true;
        gui.PopUp(message);

        //Kill itself
        DestroyImmediate(GetComponent<UnetClient>());
    }
}
