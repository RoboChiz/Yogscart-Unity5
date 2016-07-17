using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class UnetClient : NetworkManager
{

    protected CurrentGameData gd;
    protected GameMode clientGamemode;

    //Used by Timer
    public float timeLeft = -1f, rotation = 0f;

    public virtual void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            rotation += Time.deltaTime * 50f;
        }          
    }

    public void OnGUI()
    {
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Online");
        Texture2D TimerIcon = Resources.Load<Texture2D>("UI/Main Menu/Timer");

        if (timeLeft > 0)
        {
            GUIUtility.RotateAroundPivot(rotation, new Vector2(60, 35));
            GUI.DrawTexture(new Rect(20, -5, 80, 80), TimerIcon);
            GUIUtility.RotateAroundPivot(-rotation, new Vector2(60, 35));

            GUIHelper.OutLineLabel(new Rect(50, 20, 75, 75), ((int)timeLeft).ToString(), 1,Color.black);
        }
    }

    //Register the handlers required for the client
    public virtual void RegisterHandlers()
    {
        gd = FindObjectOfType<CurrentGameData>();

        //Stop Host from registering Client messages it should never use
        if(!NetworkServer.active)
        {
            client.RegisterHandler(UnetMessages.acceptedMsg, OnClientAccepted);
            client.RegisterHandler(UnetMessages.clientErrorMsg, OnCustomError);
            client.RegisterHandler(UnetMessages.playerUpMsg, OnPlayerUp);
            client.RegisterHandler(UnetMessages.forceCharacterSelectMsg, OnForceCharacterSelect);
        }

        //Messages for All Clients and Host
        client.RegisterHandler(UnetMessages.displayNameUpdateMsg, OnDisplayNameUpdate);
        client.RegisterHandler(UnetMessages.loadGamemodeMsg, OnLoadGamemode);
        client.RegisterHandler(UnetMessages.timerMsg, OnTimer);
        client.RegisterHandler(UnetMessages.countdownMsg, OnCountdown);
        client.RegisterHandler(UnetMessages.unlockKartMsg, OnUnlockKart);
        client.RegisterHandler(UnetMessages.returnLobbyMsg, OnReturnLobby);
        client.RegisterHandler(UnetMessages.loadLevelID, OnLoadLevelID);
    }

    // Called when connected to a server
    public override void OnClientConnect(NetworkConnection conn)
    {
        //Stop Host from sending information it dosen't need to...
        if (!NetworkServer.active)
        {
            VersionMessage myMsg = new VersionMessage();
            myMsg.version = gd.version;
            client.Send(UnetMessages.versionMsg, myMsg);
        }
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
    public void SendPlayerInfo()
    {
        //Send PlayerInfo to Server
        PlayerInfoMessage msg = new PlayerInfoMessage();
        msg.displayName = FindObjectOfType<NetworkGUI>().playerName;
        msg.character = CurrentGameData.currentChoices[0].character;
        msg.hat = CurrentGameData.currentChoices[0].hat;
        msg.kart = CurrentGameData.currentChoices[0].kart;
        msg.wheel = CurrentGameData.currentChoices[0].wheel;

        client.Send(UnetMessages.playerInfoMsg, msg);
    }

    // Called when a client dosen't want to pick a character
    public void SendRejection()
    {
        EmptyMessage myMsg = new EmptyMessage();
        client.Send(UnetMessages.rejectPlayerUpMsg, myMsg);
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

    //Forces the client to send it current Character Selection Wheter or not if it has finished
    public void OnForceCharacterSelect(NetworkMessage netMsg)
    {
        //Check that something has been chosen for each loadout option
        LoadOut toSend = CurrentGameData.currentChoices[0];

        if (toSend.character < 0)
            toSend.character = 0;

        if (toSend.kart < 0)
            toSend.kart = 0;

        if (toSend.hat < 0)
            toSend.hat = 0;

        if (toSend.wheel < 0)
            toSend.wheel = 0;

        FindObjectOfType<NetworkGUI>().StopAllCoroutines();

        SendPlayerInfo();

        //Close the Character Select
        FindObjectOfType<CharacterSelect>().Cancel();
    }

    //Sent by server, loads the client script for the new gamemode
    public void OnLoadGamemode(NetworkMessage netMsg)
    {
        LoadGamemodeMessage msg = netMsg.ReadMessage<LoadGamemodeMessage>();

        clientGamemode = OnlineGameModeScripts.AddClientScript(msg.gamemode);
        clientGamemode.StartGameMode(); 

        FindObjectOfType<NetworkGUI>().StartClientGame();
    }

    //Sent by Server, makes a timer appear
    public void OnTimer(NetworkMessage netMsg)
    {
        TimerMessage msg = netMsg.ReadMessage<TimerMessage>();
        timeLeft = msg.time;
    }

    //Called when Client needs to start Countdown
    private void OnCountdown(NetworkMessage netMsg)
    {
        clientGamemode.StartCountdown();
    }

    //Called when Client needs to unlock all Karts
    private void OnUnlockKart(NetworkMessage netMsg)
    {
        //Unlock the karts
        kartScript[] kses = FindObjectsOfType<kartScript>();
        foreach (kartScript ks in kses)
            ks.locked = false;

        kartItem[] kitemes = FindObjectsOfType<kartItem>();
        foreach (kartItem ki in kitemes)
            ki.locked = false;
    }

    //Called when the gamemode has finished
    private void OnReturnLobby(NetworkMessage netMsg)
    {
        //Tell the client gamemode to clear up
        clientGamemode.OnEndGamemode();
        StartCoroutine(ReloadLobby());

    }

    private IEnumerator ReloadLobby()
    {

        yield return StartCoroutine(LoadMainMenu(NetworkGUI.ServerState.Lobby));

        //Delete it for good measure
        Destroy(clientGamemode);       
    }

    private IEnumerator LoadMainMenu(NetworkGUI.ServerState newState)
    {
        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        //Load the Lobby
        SceneManager.LoadScene("Main_Menu");
        yield return null;

        //Reload the Lobby
        FindObjectOfType<NetworkGUI>().state = newState;

        //Turn the Main Menu off
        FindObjectOfType<MainMenu>().state = MainMenu.MenuState.Online;

        FindObjectOfType<MainMenu>().backStates = new List<MainMenu.MenuState>();
        FindObjectOfType<MainMenu>().backStates.Add(MainMenu.MenuState.Start);
        FindObjectOfType<MainMenu>().backStates.Add(MainMenu.MenuState.Main);
        FindObjectOfType<MainMenu>().HideTitle();

        //Clear away any Components it may of gained
        Component[] comps = GetComponents(typeof(Component));
        foreach (Component comp in comps)
        {
            if (!(comp is Transform || comp is CurrentGameData || comp is InputManager || comp is SoundManager || comp is KartMaker || comp is CollisionHandler || comp is NetworkGUI || comp is UnetClient || comp is UnetHost))
            {
                Destroy(comp);
            }
        }

        yield return new WaitForSeconds(0.5f);

        CurrentGameData.blackOut = false;
    }

    //Called when a Server wants a client to load a level by ID
    private void OnLoadLevelID(NetworkMessage netMsg)
    {
        stringMessage msg = netMsg.ReadMessage<stringMessage>();
        SceneManager.LoadScene(msg.value);
    }

    public void EndClient(string message)
    {
        StartCoroutine(ActualEndClient(message));
    }

    private IEnumerator ActualEndClient(string message)
    {
        StopClient();

        yield return StartCoroutine(LoadMainMenu(NetworkGUI.ServerState.ServerList));

        //Kill itself
        DestroyImmediate(GetComponent<UnetClient>());
    }



}
