using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

public class NetworkGUI : MonoBehaviour
{
    CurrentGameData gd;

    private float guiAlpha = 0f, fadeTime = 0.5f;

    public enum ServerState { ServerList, Connecting, Popup, Lobby, LoadingRace, LoadingLevel, Racing };
    public ServerState state, nextState;

    public string popupText;

    const int maxServerCount = 10;
    public Color currentPlayerColour;
    public Color teamColour;

    private bool editServer;
    private List<ServerInfo> servers;
    private Vector2 serverScroll = Vector2.zero;

    private int hostPort = 25000;
    private string portString = "";

    public string playerName { get; private set; }
    private int finalPlayersID = -1;

    public List<DisplayName> finalPlayers;

    public int currentSelection;
    public int currentGamemode = -1;

    //@HideInInspector
    //var SpectatorCam : GameObject;

    private float connectRot;

    public bool Automatic = true;

    public int maxPlayers = 30;

    private float waitTime;
    private int minPlayers = 2;

    private Texture2D serverInfo, nameList, connectionCircle;

    public UnetClient myUnet;
    private bool inputLock = false;

    // Use this for initialization
    void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        playerName = PlayerPrefs.GetString("playerName", "Player");

        serverInfo = Resources.Load<Texture2D>("UI/Lobby/ServerInfo");
        nameList = Resources.Load<Texture2D>("UI/Lobby/NamesList");
        connectionCircle = Resources.Load<Texture2D>("UI/Main Menu/Connecting"); ;
    }

    void SaveServers()
    {
        string savedServers = "";

        if (servers != null && servers.Count > 0)
        {
            for (int i = 0; i < servers.Count; i++)
                savedServers += servers[i].ToString();
            savedServers = savedServers.Remove(savedServers.Length - 1);
        }

        PlayerPrefs.SetString("YogscartServers", savedServers);
    }

    void LoadServers()
    {
        string savedServers = PlayerPrefs.GetString("YogscartServers", "");
        servers = new List<ServerInfo>();

        try
        {
            string[] serverStrings = savedServers.Split(";"[0]);
            for (int i = 0; i < serverStrings.Length; i++)
            {
                ServerInfo nServer = new ServerInfo();
                nServer.LoadFromString(serverStrings[i]);
                servers.Add(nServer);
            }
        }
        catch
        {
            Debug.Log("Servers List string not in the correct format.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Automatic Stuff
        if (state == ServerState.Lobby && Network.isServer && Automatic)
        {

            waitTime -= Time.deltaTime;

            if (finalPlayers.Count == 12)
            {
                //StartGame();
            }
            else if (waitTime <= 0 && finalPlayers.Count >= minPlayers)
            {
                //StartGame();
            }
            else if (waitTime <= 0)
            {
                waitTime = 31f;
                //GetComponent.< NetworkView > ().RPC("Countdowner", RPCMode.All, 30);
            }
        }      
    }

    //Do the GUI
    void OnGUI()
    {

        GUI.color = new Color(1, 1, 1, guiAlpha);
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Online");
        GUI.matrix = GUIHelper.GetMatrix();

        float fontSize = GUI.skin.label.fontSize;

        if (popupText != null && popupText != "" && guiAlpha == 1 && state != ServerState.Popup)
            ChangeState(ServerState.Popup);

        Rect nameListRect = new Rect(190, 95, 570, 795);

        bool xboxController = InputManager.controllers[0].controllerName != "Key_";

        switch (state)
        {

            case ServerState.ServerList:

                if (!editServer)
                {
                    float vert = InputManager.controllers[0].GetMenuInput("MenuVertical");

                    if (vert > 0)
                    {
                        currentSelection -= 1;
                    }

                    if (vert < 0)
                    {
                        currentSelection += 1;
                    }
                }

                if (currentSelection < 0)
                    currentSelection = Mathf.Clamp(servers.Count - 1, 0, 49);

                if (currentSelection >= Mathf.Clamp(servers.Count, 0, maxServerCount))
                    currentSelection = 0;

                GUI.DrawTexture(nameListRect, nameList);

                nameListRect = new Rect(275, 190, 425, 605);

                GUI.BeginGroup(nameListRect);

                GUI.Label(new Rect(10, 0, nameListRect.width, fontSize * 2f), "Servers");

                GUI.DrawTexture(new Rect(10, fontSize * 1.5f, nameListRect.width, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                serverScroll = GUI.BeginScrollView(new Rect(0, fontSize * 2.2f, nameListRect.width, nameListRect.height - 20 - fontSize * 2.2f), serverScroll, new Rect(0, 0, nameListRect.width - 20, (Mathf.Clamp(servers.Count, 10, 50) + 1) * fontSize * 1.25f));

                if (servers != null && servers.Count > 0)
                    for (int i = 0; i < Mathf.Clamp(servers.Count, 0, maxServerCount); i++)
                    {

                        var startHeight = (i * fontSize * 1.25f);

                        if (currentSelection != i || !editServer)
                            GUI.Label(new Rect(10, startHeight, nameListRect.width - 20 - fontSize * 2f, fontSize * 1.25f), servers[i].serverName);
                        else
                        {
                            string newServerName = servers[i].serverName;
                            newServerName = GUI.TextField(new Rect(10, startHeight, nameListRect.width - 20 - fontSize * 2f, fontSize * 1.25f), newServerName);

                            if (GUIHelper.CheckString(newServerName, 20))
                                servers[i].serverName = newServerName;
                        }


                        if (servers[i].publicServer)
                            GUI.DrawTexture(new Rect(nameListRect.width - (fontSize * 5f), startHeight, fontSize * 2.5f, fontSize * 1.25f), Resources.Load<Texture2D>("UI/Lobby/Public"));

                        GUI.DrawTexture(new Rect(nameListRect.width - 10 - fontSize * 2f, startHeight, fontSize * 1.25f, fontSize * 1.25f), Resources.Load<Texture2D>("UI/Lobby/NoConnection"));

                        if (i == currentSelection)
                            GUI.DrawTexture(new Rect(0, startHeight, nameListRect.width, fontSize * 1.25f), Resources.Load<Texture2D>("UI/Lobby/Selected"));

                        if (Input.GetMouseButtonDown(0) && InputManager.MouseIntersects(new Rect(275, 190 + (fontSize * 2.2f) + startHeight, nameListRect.width, fontSize * 1.25f)))
                            currentSelection = i;

                    }

                GUI.EndScrollView();

                GUI.EndGroup();

                //Additional Buttons	

                // Server Information
                if (servers != null && servers.Count > 0)
                {
                    Rect serverInfoRect = new Rect(780, 95, 950, 285);
                    GUI.DrawTexture(serverInfoRect, serverInfo);

                    GUI.BeginGroup(serverInfoRect);

                    GUI.Label(new Rect(20, 10, serverInfoRect.width - 40, fontSize * 2f), "Server Info");

                    GUI.DrawTexture(new Rect(20, 10 + fontSize * 1.25f, serverInfoRect.width - 40, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                    if (!editServer)
                    {
                        GUI.Label(new Rect(20, 10 + fontSize * 2f, serverInfoRect.width - 40, serverInfoRect.height - fontSize * 4.5f), servers[currentSelection].description);
                        GUI.Label(new Rect(20, serverInfoRect.height - fontSize * 1.5f, serverInfoRect.width / 2f, fontSize), "IP: " + servers[currentSelection].ip);
                        GUI.Label(new Rect(30 + serverInfoRect.width / 2f, serverInfoRect.height - fontSize * 1.5f, serverInfoRect.width / 2f, fontSize), "Port: " + servers[currentSelection].port.ToString());
                    }
                    else
                    {

                        string newDesc = servers[currentSelection].description;

                        newDesc = GUI.TextArea(new Rect(20, 10 + fontSize * 2f, serverInfoRect.width - 40, serverInfoRect.height - fontSize * 4.5f), newDesc);

                        if (GUIHelper.CheckString(newDesc, 115))
                        {
                            servers[currentSelection].description = newDesc;
                        }

                        if (!servers[currentSelection].publicServer)
                        {
                            GUI.Label(new Rect(20, serverInfoRect.height - fontSize * 1.5f, serverInfoRect.width / 2f, fontSize), "IP: ");

                            string newIp = servers[currentSelection].ip;

                            newIp = GUI.TextField(new Rect(20 + (fontSize * 1.5f), serverInfoRect.height - fontSize * 1.5f, serverInfoRect.width / 2f - fontSize * 1.5f, fontSize), newIp);

                            if (GUIHelper.CheckString(newDesc, 0))
                            {
                                servers[currentSelection].ip = newIp;
                            }

                            GUI.Label(new Rect(30 + serverInfoRect.width / 2f, serverInfoRect.height - fontSize * 1.5f, serverInfoRect.width / 2f, fontSize), "Port: ");
                            portString = GUI.TextField(new Rect(30 + (fontSize * 2.75f) + serverInfoRect.width / 2f, serverInfoRect.height - fontSize * 1.5f, fontSize * 8f, fontSize), portString);

                            if (portString != null && portString != "")
                            {
                                int newPort = -1;
                                if(int.TryParse(portString, out newPort))
                                {
                                    servers[currentSelection].port = newPort;
                                }

                            }
                                
                        }
                    }

                    GUI.EndGroup();

                    if (!editServer)
                    {

                        string joinText = "Join";

                        if (xboxController)
                        {
                            joinText = "Join   A";
                        }

                        if ((GUI.Button(new Rect(795, 390, 190, 95), joinText) || (xboxController && InputManager.controllers[0].GetMenuInput("Submit") != 0)) && !inputLock)
                        {
                            StartClient();
                        }

                        if (xboxController)
                            GUI.DrawTexture(new Rect(910, 407, fontSize * 1.5f, fontSize * 1.5f), Resources.Load<Texture2D>("UI/Main Menu/A"));

                    }

                    if (!servers[currentSelection].publicServer && !editServer)
                    {

                        if (GUI.Button(new Rect(1005, 390, 190, 95), "Edit") && !inputLock)
                        {
                            editServer = true;
                            portString = servers[currentSelection].port.ToString();
                        }

                        if (GUI.Button(new Rect(1215, 390, 190, 95), "Delete") && !inputLock)
                        {
                            servers.RemoveAt(currentSelection);
                            SaveServers();
                        }

                    }

                    if (editServer)
                    {
                        if (GUI.Button(new Rect(795, 390, 190, 95), "Save") && !inputLock)
                        {
                            editServer = false;
                            SaveServers();
                        }
                        if (GUI.Button(new Rect(1005, 390, 190, 95), "Cancel") && !inputLock)
                        {
                            editServer = false;
                            LoadServers();
                        }
                    }

                }

                if (!editServer && servers.Count < maxServerCount)
                    if (GUI.Button(new Rect(1425, 390, 190, 95), "Add New") && !inputLock)
                    {
                        editServer = true;

                        var nServer = new ServerInfo();
                        servers.Add(nServer);
                        currentSelection = servers.Count - 1;

                        portString = servers[currentSelection].port.ToString();
                    }

                //Port
                GUI.Label(new Rect(800, 505, 760, 50), "Port: ");
                int.TryParse(GUI.TextField(new Rect(800, 555, 760, 50), hostPort.ToString()), out hostPort);

                //Username
                string newPlayerName = playerName;
                GUI.Label(new Rect(800, 625, 760, 50), "Name: ");
                newPlayerName = GUI.TextField(new Rect(800, 675, 760, 50), newPlayerName, 16);

                if (GUIHelper.CheckString(newPlayerName, 16))
                    playerName = newPlayerName;

                string hostText = "Host Server";

                if (xboxController)
                    hostText = "Host Server   X";

                if (GUI.Button(new Rect(800, 745, 760, 95), hostText) && !inputLock)
                {
                    StartServer();
                }

                if (xboxController)
                    GUI.DrawTexture(new Rect(1275, 765, fontSize * 1.5f, fontSize * 1.5f), Resources.Load<Texture2D>("UI/Main Menu/X"));

                if (FindObjectOfType<MainMenu>().SideAmount == 0 && InputManager.controllers[0].GetMenuInput("Cancel") != 0)
                {
                    PlayerPrefs.SetString("playerName", playerName);
                    FindObjectOfType<MainMenu>().BackMenu();
                    MainMenu.lockInputs = false;
                }

                break;

            case ServerState.Lobby:

                if (currentSelection < 0)
                    currentSelection = gd.onlineGameModes.Length - 1;

                if (currentSelection >= gd.onlineGameModes.Length)
                    currentSelection = 0;

                GUI.DrawTexture(nameListRect, nameList);

                nameListRect = new Rect(220, 190, 520, 700);

                GUI.BeginGroup(nameListRect);

                GUI.Label(new Rect(0, 0, nameListRect.width, fontSize * 2f), "Lobby");

                GUI.DrawTexture(new Rect(0, fontSize * 1.5f, nameListRect.width - 10, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                if (currentGamemode == -1 || !gd.onlineGameModes[currentGamemode].teamGame)
                {
                    //Non Team Game GUI
                    for (int i = 0; i < finalPlayers.Count; i++)
                    {
                        float startHeight = fontSize * 2f + (i * fontSize * 1.25f);
                        float eighth = nameListRect.width / 8f;
                        int holder = GUI.skin.label.fontSize;

                        if (finalPlayersID == i)
                            GUI.skin.label.normal.textColor = currentPlayerColour;
                        else
                            GUI.skin.label.normal.textColor = Color.white;

                        GUI.skin.label.fontSize = (int)(holder * 0.8f);
                        GUI.Label(new Rect(0, startHeight, eighth * 6f, fontSize * 1.25f), finalPlayers[i].displayName);

                        //Calculate Ping
                        GUI.skin.label.fontSize = (int)(holder * 0.5f);
                        GUI.Label(new Rect(eighth * 6f,startHeight, eighth, fontSize * 1.5f), finalPlayers[i].ping.ToString() + " Ping");

                        GUI.skin.label.fontSize = holder;

                        GUI.DrawTexture(new Rect(eighth * 7f, startHeight, fontSize * 1.5f, fontSize * 1.5f), gd.characters[finalPlayers[i].character].icon, ScaleMode.ScaleToFit);

                    }
                }
                else
                {
                    //Team Game GUI
                    int count = 0;
                    string[] gdTeams = gd.onlineGameModes[currentGamemode].teams;

                    for (int t = 0; t < gdTeams.Length; t++)
                    {
                        float startHeight = fontSize * 2f + (count * fontSize * 1.25f);
                        float textSize = Mathf.Clamp(fontSize / (gdTeams[t].Length / 10f), 0, fontSize);
                        GUI.skin.label.fontSize = (int)textSize;

                        GUI.skin.label.normal.textColor = teamColour;

                        GUI.Label(new Rect(0, startHeight, nameListRect.width, fontSize * 1.25f), gdTeams[t]);

                        count++;

                        for (int i = 0; i < finalPlayers.Count; i++)
                        {
                            if (finalPlayers[i].team == t)
                            {
                                startHeight = fontSize * 2f + (count * fontSize * 1.25f);
                                textSize = Mathf.Clamp(fontSize / (finalPlayers[i].displayName.Length / 10f), 0, fontSize);
                                GUI.skin.label.fontSize = (int)textSize;

                                if (finalPlayersID == i)
                                    GUI.skin.label.normal.textColor = currentPlayerColour;
                                else
                                    GUI.skin.label.normal.textColor = Color.white;

                                GUI.Label(new Rect(0, startHeight, nameListRect.width, fontSize * 1.25f), finalPlayers[i].displayName);
                                count++;
                            }

                        }

                    }
                }

                GUI.skin.label.fontSize = (int)fontSize;
                GUI.skin.label.normal.textColor = Color.white;

                GUI.EndGroup();

                string quitText = "Quit";
                if (xboxController)
                    quitText = "Quit   B";

                if ((GUI.Button(new Rect(190, nameListRect.y + nameListRect.height + 20, 190, 95), quitText) || InputManager.controllers[0].GetMenuInput("Cancel") != 0) && !inputLock)
                {
                    //Disconnect
                    if(NetworkServer.active)//Client is server
                    {
                        myUnet.StopHost();
                    }
                    else
                    {
                        myUnet.StopClient();
                    }

                    CloseServer();
                }

                if (xboxController)
                    GUI.DrawTexture(new Rect(304, nameListRect.y + nameListRect.height + 40, fontSize * 1.5f, fontSize * 1.5f), Resources.Load<Texture2D>("UI/Main Menu/B"));

                if (currentGamemode != -1)
                {
                    Rect gamemodeInfoRect = new Rect(780, 95, 950, 285);
                    GUI.DrawTexture(gamemodeInfoRect, serverInfo);

                    GUI.BeginGroup(gamemodeInfoRect);

                    GUI.Label(new Rect(20, 10, gamemodeInfoRect.width - 40, fontSize * 2f), gd.onlineGameModes[currentGamemode].gamemodeName);
                    GUI.DrawTexture(new Rect(20, 10 + fontSize * 1.25f, gamemodeInfoRect.width - 40, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                    GUI.Label(new Rect(20, 10 + fontSize * 1.3f, gamemodeInfoRect.width - 40, gamemodeInfoRect.height - 10 - fontSize * 1.3f), gd.onlineGameModes[currentGamemode].description);

                    GUI.EndGroup();
                }

                if (NetworkServer.active)
                {

                    if (!Automatic)
                    {
                        string startText = "Start";
                        if (xboxController)
                            startText = "Start   A";

                        if ((GUI.Button(new Rect(400, nameListRect.y + nameListRect.height + 20, 190, 95), startText) || (InputManager.controllers[0].GetMenuInput("Submit") != 0)) && !inputLock)
                        {
                            StartHostGame();
                        }

                        if (xboxController)
                            GUI.DrawTexture(new Rect(525, nameListRect.y + nameListRect.height + 40, fontSize * 1.5f, fontSize * 1.5f), Resources.Load<Texture2D>("UI/Main Menu/A"));
                    }

                    Rect hostInfoRect = new Rect(780, 380, 950, 475);
                    GUI.DrawTexture(hostInfoRect, serverInfo);

                    GUI.BeginGroup(hostInfoRect);

                    GUI.Label(new Rect(20, 10, hostInfoRect.width - 40, fontSize * 2f), "Host Options");
                    GUI.DrawTexture(new Rect(20, 10 + fontSize * 1.25f, hostInfoRect.width - 40, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                    int choosingCount = FindObjectOfType<UnetHost>().choosingCount;
                    string ccString = "";

                    if (choosingCount == 1)
                        ccString = "1 Player choosing Layout";
                    else if (choosingCount > 1)
                        ccString = choosingCount.ToString() + " Players choosing Layout";

                    GUI.Label(new Rect(20, 10 + fontSize * 1.5f, hostInfoRect.width - 40, fontSize * 2f), ccString);

                    GUI.Label(new Rect(20, 10 + fontSize * 2.5f, hostInfoRect.width - 40, fontSize * 2f), "Game Mode: ");
                    serverScroll = GUI.BeginScrollView(new Rect(20, 10 + fontSize * 3.6f, hostInfoRect.width / 2f - 20, hostInfoRect.height - fontSize * 2.8f - 20), serverScroll, new Rect(0, 0, hostInfoRect.width / 2f - 40, Mathf.Clamp(gd.onlineGameModes.Length, 6, Mathf.Infinity) * fontSize * 1.25f));

                    for (int i = 0; i < gd.onlineGameModes.Length; i++)
                    {
                        GUI.Label(new Rect(10, i * fontSize * 1.25f, hostInfoRect.width - 20 - fontSize * 2f, fontSize * 1.25f), gd.onlineGameModes[i].gamemodeName);

                        if (i == currentSelection)
                            GUI.DrawTexture(new Rect(0, i * fontSize * 1.25f, nameListRect.width, fontSize * 1.25f), Resources.Load<Texture2D>("UI/Lobby/Selected"));

                        if ((Input.GetMouseButtonDown(0) && InputManager.MouseIntersects(new Rect(hostInfoRect.x + 20, hostInfoRect.y + 10 + fontSize * 2.6f + (i * fontSize * 1.25f), nameListRect.width, fontSize * 1.25f))) && !inputLock)
                            currentSelection = i;
                    }
                    
                    if (currentSelection != currentGamemode)
                    {
                        currentGamemode = currentSelection;
                    }

                    GUI.EndScrollView();
                    GUI.EndGroup();
                }

                break;
            case ServerState.Popup:

                var popupRect = new Rect(485, 240, 950, 285);
                GUI.DrawTexture(popupRect, serverInfo);

                GUI.BeginGroup(popupRect);

                GUI.Label(new Rect(20, 10, popupRect.width - 40, fontSize * 2f), "Network Message");
                GUI.DrawTexture(new Rect(20, 10 + fontSize * 1.25f, popupRect.width - 40, fontSize * 0.2f), Resources.Load<Texture2D>("UI/Lobby/Line"));

                GUI.Label(new Rect(20, 10 + fontSize * 2f, popupRect.width - 40, popupRect.height - fontSize * 4.5f), popupText);

                GUI.EndGroup();

                string okayText = "Okay";

                if (xboxController)
                    okayText = "Okay   A";

                if ((GUI.Button(new Rect(865, 525, 190, 95), okayText) || InputManager.controllers[0].GetMenuInput("Submit") != 0) && !inputLock)
                {
                    popupText = "";
                    ChangeState(ServerState.ServerList);
                }

                if (xboxController)
                    GUI.DrawTexture(new Rect(985, 535, fontSize * 1.5f, fontSize * 1.5f), Resources.Load<Texture2D>("UI/Main Menu/A"));

                break;
            case ServerState.Connecting:

                GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

                float chunkSize = Mathf.Min(Screen.width, Screen.height) / 5f;

                GUIUtility.RotateAroundPivot(connectRot, new Vector2(Screen.width / 2f, Screen.height / 2f));
                GUI.DrawTexture(new Rect(Screen.width / 2f - chunkSize, Screen.height / 2f - chunkSize, 2f * chunkSize, 2f * chunkSize), connectionCircle);

                connectRot += Time.deltaTime * -50f;

                if(FindObjectOfType<CharacterSelect>() != null)
                {
                    if(GUIHelper.DrawBack(guiAlpha))
                    {
                        FindObjectOfType<CharacterSelect>().Back(0);
                    }
                }

                break;
        }

        GUI.color = Color.white;
    }
  
    public void ShowMenu()
    {
        LoadServers();
        StartCoroutine(ActualShowMenu());
    }

    private IEnumerator ActualShowMenu()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ChangeGUIAlpha(0f, 1f));
    }

    public void CloseMenu()
    {
        StartCoroutine(ActualCloseMenu());
    }

    private IEnumerator ActualCloseMenu()
    {
        yield return StartCoroutine(ChangeGUIAlpha(1f, 0f));
        enabled = false;
    }

    private IEnumerator ChangeGUIAlpha(float start, float end)
    {   
        if(start == 1f)
            inputLock = true;

        float startTime = Time.time;
        while (Time.time - startTime < fadeTime)
        {
            guiAlpha = Mathf.Lerp(start, end, (Time.time - startTime) / fadeTime);
            yield return null;
        }

        guiAlpha = end;

        if(end == 1)
            inputLock = false;
    }

    public void ChangeState(ServerState nState)
    {      
        StartCoroutine(ActualChangeState(nState));
    }

    private IEnumerator ActualChangeState(ServerState nState)
    {
        nextState = nState;

        if (guiAlpha != 0)
            yield return StartCoroutine(ChangeGUIAlpha(guiAlpha, 0f));

        state = nState;

        yield return StartCoroutine(ChangeGUIAlpha(0f, 1f));
    }

    public void StartServer()
    {
        MainMenu.lockInputs = true;
        PlayerPrefs.SetString("playerName", playerName);

        StartCoroutine("ActualStartServer");
    }

    private IEnumerator ActualStartServer()
    {
        Debug.Log("Started Server!");

        //Pick a character before game starts
        CharacterSelect cs = FindObjectOfType<CharacterSelect>();
        cs.enabled = true;

        yield return StartCoroutine(ActualChangeState(ServerState.Connecting));

        yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);

        //Wait until all characters have been selected
        while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
        {
            yield return null;
        }

        if (cs.State == CharacterSelect.csState.Off)
        {
           yield return StartCoroutine(ReloadServerList());
            Debug.Log("It didn't worked");
        }

        //Everything worked out perfect!
        Debug.Log("It worked");
        myUnet = gd.gameObject.AddComponent<UnetHost>();
        myUnet.playerPrefab = Resources.Load<GameObject>("Prefabs/Kart Maker/Network Kart");
        myUnet.networkPort = hostPort;
        try
        {
            myUnet.StartHost();
            myUnet.RegisterHandlers();
            ChangeState(ServerState.Lobby);
        }
        catch
        {
            Debug.Log("Error caught!");
        }

    }

    public void PlayerUp()
    {
        StartCoroutine(ActualPlayerUp());
    }

    private IEnumerator ActualPlayerUp()
    {
        //Pick a character before game starts
        CharacterSelect cs = FindObjectOfType<CharacterSelect>();
        cs.enabled = true;

        Debug.Log("Character Select found:" + (cs != null).ToString());
        yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);

        //Wait for fade
        yield return new WaitForSeconds(0.5f);

        //Wait until all characters have been selected
        while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
        {
            yield return null;
        }

        if (cs.State == CharacterSelect.csState.Finished)
        {
            //Everything worked out perfect!
            Debug.Log("It worked");       
            FindObjectOfType<UnetClient>().SendPlayerInfo();
        }
        else
        {
            myUnet.SendRejection();
        }

        ChangeState(ServerState.Lobby);
    }

    public void CloseServer()
    {
        StartCoroutine(ReloadServerList());
    }

    private IEnumerator ReloadServerList()
    {
        DestroyImmediate(myUnet);

        //Wait for fade
        yield return StartCoroutine(ActualChangeState(ServerState.ServerList));

        MainMenu.lockInputs = false;

        //Stop all Gamemode Coroutines
        StopAllCoroutines();
        //Wait a Frame for Coroutines to stop
        yield return null;
    }

    public void StartClient()
    {
        PlayerPrefs.SetString("playerName", playerName);        
        StartCoroutine(ActualStartClient());
    }

    private IEnumerator ActualStartClient()
    {

        MainMenu.lockInputs = true;

        //Wait for fade
        yield return StartCoroutine(ActualChangeState(ServerState.Connecting));

        finalPlayers = new List<DisplayName>();

        myUnet = gd.gameObject.AddComponent<UnetClient>();
        myUnet.playerPrefab = Resources.Load<GameObject>("Prefabs/Kart Maker/Network Kart");

        Debug.Log("Connecting to " + servers[currentSelection].ip + ":" + servers[currentSelection].port);

        myUnet.networkAddress = servers[currentSelection].ip;
        myUnet.networkPort = servers[currentSelection].port;
     
        try
        {

            if (servers[currentSelection].ip == "" || servers[currentSelection].ip == null)
                throw new Exception("No Ip has been entered");

            myUnet.StartClient();
            myUnet.RegisterHandlers();
        }
        catch
        {
            Debug.Log("Error caught!");

            myUnet.EndClient("Invalid Server IP");
        }

    }

    private void StartHostGame()
    {
        UnetHost host = myUnet as UnetHost;
        host.StartGame(currentGamemode);
    }

    public void StartClientGame()
    {
        inputLock = true;
        ChangeState(ServerState.Connecting);
    }

    public void PopUp(string message)
    {
        inputLock = false;
        popupText = message;
    }

}

[System.Serializable]
public class GameModeInfo
{
    public string gamemodeName;
    public Texture2D logo;
    public string description;

    public bool teamGame;
    public string[] teams;

    public MonoBehaviour hostScript;
    public MonoBehaviour baseScript;

}

[System.Serializable]
public class ServerInfo
{

    public string serverName;
    public string description;
    public bool publicServer { get; private set; }

    public int currentPlayers;
    public int currentGameMode { get; private set; }

    public string ip;
    public int port;

    public ServerInfo()
    {
        serverName = "New Server";
        description = "";
        publicServer = false;
        currentPlayers = 0;
        currentGameMode = 0;
        ip = "127.0.0.1";
        port = 25000;
    }

    public override string ToString()
    {
        string returnString = "";

        returnString += serverName + ",";
        returnString += description + ",";
        returnString += ip + ",";
        returnString += port + ";";

        return returnString;

    }

    public ServerInfo(string ipS, int portS, string n, string d, int cp)
    {
        serverName = n;
        description = d;
        publicServer = true;
        currentPlayers = cp;
        currentGameMode = 0;
        ip = ipS;
        port = portS;
    }

    public void LoadFromString(string val)
    {
        string[] parts = val.Split(","[0]);

        serverName = parts[0];
        description = parts[1];
        publicServer = false;
        ip = parts[2];

        int outPort = port;
        int.TryParse(parts[3], out outPort);
        port = outPort;

    }

}

[System.Serializable]
public class DisplayName
{
    public string displayName { get; private set; }
    public int character { get; private set; }
    public int ping { get; private set; }
    public int team { get; private set; }
    public int points { get; private set; }

    public DisplayName(string n, int c, int p, int t, int po)
    {
        displayName = n;
        character = c;
        ping = p;
        team = t;
        points = po;
    }

    public DisplayName(string n, int c, int p, int po) : this(n,c,p,-1,po){}
    public DisplayName(string n, int c, int po) : this(n, c, -1, -1, po) { }

    public DisplayName() { }

    public override string ToString()
    {
        return displayName + ";" + character + ";" + ping + ";" + team + ";" + points;
    }

    public void ReadFromString(string val)
    {
        string[] splitup = val.Split(";"[0]);

        displayName = splitup[0];
        character = int.Parse(splitup[1]);
        ping = int.Parse(splitup[2]);
        team = int.Parse(splitup[3]);
        points = int.Parse(splitup[4]);
    }

}