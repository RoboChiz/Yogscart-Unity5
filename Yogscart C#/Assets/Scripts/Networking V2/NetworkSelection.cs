using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSelection : MonoBehaviour
{
    public enum MenuState { ServerList, Connecting, HostSetup, Lobby, CharacterSelect, Gamemode, PopUp};
    public MenuState state;

    public enum NetworkState { Host, ConsoleHost, PlayableHost, Client};
    public NetworkState networkState;

    private float guiAlpha = 0f;
    public GUISkin skin;

    //Menu Selection
    private List<MenuOption> optionsList;
    private int currentSelection = 0;
    public List<ServerInfo> servers { get; private set; }
    private Vector2 scrollPosition;

    public bool locked;

    private string popupMessage;

    private CurrentGameData gd;

    //Host Settings
    int serverPort = 25000;
    string serverPassword = "";

    //Server Settings
    private ServerSettings hostServerSettings;
    public YogscartNetwork.Host host;
    public YogscartNetwork.Client client;

    //Info to display
    public List<PlayerInfo> playerList;
    public int currentGamemode;

    //Textures
    private Texture2D xButton, yButton, qButton, eButton, connectionCircle;
    private float connectRot = 0f;

    public void Show() { StartCoroutine(FadeTo(1f)); currentSelection = 0; LoadServers(); gd = FindObjectOfType<CurrentGameData>(); }
    public void Hide() { StartCoroutine(FadeTo(0f)); }
    public bool isShowing { get { return guiAlpha > 0f; } }

    public void ChangeState(MenuState newState) { StartCoroutine(ActualChangeState(newState));}

    private IEnumerator ActualChangeState(MenuState newState)
    {
        yield return FadeTo(0f);

        state = newState;
        currentSelection = 0;

        yield return FadeTo(1f);
    }

    void Start()
    {
        xButton = Resources.Load<Texture2D>("UI/Options/X");
        yButton = Resources.Load<Texture2D>("UI/Options/Y");
        qButton = Resources.Load<Texture2D>("UI/Options/QNormal");
        eButton = Resources.Load<Texture2D>("UI/Options/ENormal");
        connectionCircle = Resources.Load<Texture2D>("UI/Main Menu/Connecting");
    }

    class MenuOption
    {
        public string display, id;
        public bool selectable;

        public MenuOption(string _id, string _display, bool _selectable)
        {
            id = _id;
            display = _display;
            selectable = _selectable;
        }

        public MenuOption(string _id, string _display) : this(_id, _display, true) { }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();
        optionsList = new List<MenuOption>();

        if (guiAlpha > 0f)
        {
            GUIHelper.SetGUIAlpha(guiAlpha);
            switch (state)
            {
                case MenuState.ServerList:

                    //Start Server / Add Server
                    optionsList.Add(new MenuOption("HostServer", "Host a Server"));
                    optionsList.Add(new MenuOption("AddServer", "Add a new Server"));

                    break;
                case MenuState.Connecting:
                    GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

                    float chunkSize = Mathf.Min(Screen.width, Screen.height) / 5f;
                    GUIUtility.RotateAroundPivot(connectRot, new Vector2(Screen.width / 2f, Screen.height / 2f));
                    GUI.DrawTexture(new Rect(Screen.width / 2f - chunkSize, Screen.height / 2f - chunkSize, 2f * chunkSize, 2f * chunkSize), connectionCircle);
                    connectRot += Time.deltaTime * -50f;
                    break;
                case MenuState.HostSetup:

                    optionsList.Add(new MenuOption("Port", "Port: " + serverPort));
                    optionsList.Add(new MenuOption("Gamemode", "Gamemode: " + gd.onlineGameModes[hostServerSettings.gamemode].gamemodeName));
                    optionsList.Add(new MenuOption("Automatic", "Automatic: " + (hostServerSettings.automatic ? "Yes" : "No")));

                    if (hostServerSettings.automatic)
                    {
                        optionsList.Add(new MenuOption("MinPlayers", "Minimum Players: " + hostServerSettings.minPlayers));
                    }

                    optionsList.Add(new MenuOption("HostAsPlayer", "Host as Player: " + (hostServerSettings.hostAsPlayer ? "Yes" : "No")));
                    optionsList.Add(new MenuOption("FillWithAI", "Fill empty spaces with AI: " + (hostServerSettings.fillWithAI ? "Yes" : "No")));

                    if (hostServerSettings.fillWithAI)
                    {
                        string difficultyString = "Insane";
                        switch (hostServerSettings.aiDifficulty)
                        {
                            case 0: difficultyString = "Easy"; break;
                            case 1: difficultyString = "Medium"; break;
                            case 2: difficultyString = "Hard"; break;
                        }
                        optionsList.Add(new MenuOption("AIDifficulty", "AI Difficulty: " + difficultyString));
                    }

                    optionsList.Add(new MenuOption("ConfirmStartServer", "Start Server"));

                    break;
                case MenuState.Lobby:

                    optionsList.Add(new MenuOption("", "Welcome to the Lobby!", false));
                    
                    //Client Specific GUI
                    if (networkState == NetworkState.Client)
                    {
                        optionsList.Add(new MenuOption("LeaveServer", "Leave the Server"));
                        //Change Character
                        if (client.isRacing)
                            optionsList.Add(new MenuOption("ChangeCharacter", "Change Character/Kart"));
                    }
                    else
                    {
                        //Host UI
                        optionsList.Add(new MenuOption("", "Players (Ready): " + host.finalPlayers.Count + " / " + "12", false));
                        optionsList.Add(new MenuOption("", "Players(Not Ready): " + host.possiblePlayers.Count, false));
                        optionsList.Add(new MenuOption("", "Spectators: " + (host.waitingPlayers.Count + host.rejectedPlayers.Count).ToString(), false));

                        optionsList.Add(new MenuOption("LeaveServer", "Close Server"));
                        optionsList.Add(new MenuOption("HostGamemode", "Gamemode: " + gd.onlineGameModes[host.currentGamemode].gamemodeName));
                        optionsList.Add(new MenuOption("StartGamemode", "Start Game"));

                        if (networkState == NetworkState.PlayableHost)
                        {
                            optionsList.Add(new MenuOption("ChangeCharacter", "Change Character/Kart"));
                        }
                    }

                    //Draw Player List
                    for(int i = 0; i <playerList.Count; i++)
                    {
                        PlayerInfo info = playerList[i];

                        float startY = 150 + (optionsList.Count * 60) + (i * 60);
                        float startX = 50;
                        
                        if(startY > 1080 - 60)
                        {
                            startY -= (startY - 960);
                            startX += 800;
                        }
                        
                            //Name
                        GUIHelper.LeftRectLabel(new Rect(startX, startY, 600, 50), 1f, "  " + info.displayName, Color.white);

                        //Icons
                        GUI.DrawTexture(new Rect(startX + 610, startY, 50, 50), gd.characters[info.character].icon);
                        GUI.DrawTexture(new Rect(startX + 670, startY, 50, 50), gd.hats[info.hat].icon);
                        GUI.DrawTexture(new Rect(startX + 730, startY, 50, 50), gd.karts[info.kart].icon);
                        GUI.DrawTexture(new Rect(startX + 790, startY, 50, 50), gd.wheels[info.wheel].icon);

                        //Ping
                        GUIHelper.LeftRectLabel(new Rect(1350, startY, 300, 50), 1f, "Ping: " + (info.ping > 0 ? info.ping + "ms" : "N/A"), Color.white);
                    }

                    //Draw Current Gamemode
                    GameModeInfo gamemodeInfo = gd.onlineGameModes[currentGamemode];

                    Rect gamemodeInfoRect = new Rect(1310, 50, 600, 300);
                    GUIShape.RoundedRectangle(gamemodeInfoRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));

                    GUIHelper.LeftRectLabel(new Rect(gamemodeInfoRect.x + 10, gamemodeInfoRect.y + 10, gamemodeInfoRect.width - 220, 100), 2.5f, gamemodeInfo.gamemodeName, Color.white);
                    GUIHelper.LeftRectLabel(new Rect(gamemodeInfoRect.x + 10, gamemodeInfoRect.y + 110, gamemodeInfoRect.width - 220, gamemodeInfoRect.height - 120), 1f, gamemodeInfo.description, Color.white);

                    GUI.DrawTexture(new Rect(gamemodeInfoRect.x + gamemodeInfoRect.width - 210, 60, 200, 280), gamemodeInfo.logo, ScaleMode.ScaleToFit);

                    break;
                case MenuState.CharacterSelect:
                case MenuState.Gamemode:

                    break;
                case MenuState.PopUp:
                    GUIStyle popupStyle = new GUIStyle(GUI.skin.label);
                    popupStyle.alignment = TextAnchor.MiddleCenter;

                    GUIShape.RoundedRectangle(new Rect(200, 200, 1580, 500), 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));
                    GUI.Label(new Rect(210, 210, 1560, 480), popupMessage, popupStyle);
                    break;
            }

            //Draw Options
            int selectableCount = 0;
            for (int i = 0; i < optionsList.Count; i++)
            {
                Rect optionRect = new Rect(50, 150 + (i * 60), 750, 50);

                if (optionsList[i].selectable)
                    GUIShape.RoundedRectangle(optionRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));

                GUIHelper.LeftRectLabel(GUIHelper.MoveRect(optionRect, new Vector4(50, 5, -50, -5)), 1f, optionsList[i].display, (optionsList[i].selectable && currentSelection == selectableCount) ? Color.yellow : Color.white);

                if (optionsList[i].selectable)
                {
                    if (Cursor.visible && optionRect.Contains(GUIHelper.GetMousePosition()))
                        currentSelection = selectableCount;

                    if (GUI.Button(optionRect, "") && !locked && guiAlpha == 1f)
                        DoSubmit(optionsList[i].id);

                    selectableCount++;
                }
            }

            //Do Serverlist last or ir breaks everything
            if(state == MenuState.ServerList)
            {
                //List of all available servers
                scrollPosition = GUIHelper.BeginScrollView(new Rect(50, 150 + (optionsList.Count * 60), 1800, 11f * 60f), scrollPosition, new Rect(0, 0, 1750, servers.Count * 60f));

                //Draw Servers
                for (int i = 0; i < servers.Count; i++)
                {
                    int height = i * 60;
                    float scaleText = 0.75f;

                    //Draw a Rectangle
                    Rect rectangleRect = new Rect(0, height, 1780, 50);
                    GUIShape.RoundedRectangle(rectangleRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));

                    Rect actualRectangleRect = GUIHelper.MoveRect(rectangleRect, new Vector4(50, 150 + (optionsList.Count * 60) - scrollPosition.y, 0f, 0f));
                    if (Cursor.visible && !locked && actualRectangleRect.Contains(GUIHelper.GetMousePosition()))
                        currentSelection = i + 2;

                    //Let user join server
                    Rect clickableRect = new Rect(rectangleRect);
                    clickableRect.width = 1410;
                    if (GUI.Button(clickableRect, "") && !locked && guiAlpha == 1f)
                    {
                        ServerSelected(servers[i]);
                    }

                    //Server Name
                    GUIHelper.LeftRectLabel(new Rect(0, height, 800, 50), scaleText, "   " + servers[i].serverName, (currentSelection == i + 2) ? Color.yellow : Color.white);

                    //Server IP
                    GUIHelper.LeftRectLabel(new Rect((800 * scaleText), height, 400, 50), scaleText, (gd.streamMode ? "***.***.***.***" : servers[i].ip), (currentSelection == i + 2) ? Color.yellow : Color.white);

                    //Server Port
                    GUIHelper.LeftRectLabel(new Rect((1200 * scaleText), height, 300, 50), scaleText, servers[i].port.ToString(), (currentSelection == i + 2) ? Color.yellow : Color.white);

                    //Password?
                    GUIHelper.LeftRectLabel(new Rect((1500 * scaleText), height, 400, 50), scaleText, (servers[i].password != "") ? "Password Protected" : "No Password", (currentSelection == i + 2) ? Color.yellow : Color.white);

                    Rect editRect = new Rect(1440, height, 150, 50);
                    Rect deleteRect = new Rect(1610, height, 150, 50);

                    GUIHelper.LeftRectLabel(editRect, 1f, "Edit", (currentSelection == i + 2) ? Color.yellow : Color.white);
                    GUIHelper.LeftRectLabel(deleteRect, 1f, "Delete", (currentSelection == i + 2) ? Color.yellow : Color.white);

                    if (!Cursor.visible)
                    {
                        //Edit & Delete Prompt
                        if (currentSelection == i + 2)
                        {
                            if (InputManager.controllers[0].inputType == InputType.Xbox360)
                            {
                                GUI.DrawTexture(new Rect(1510, height, 50, 50), xButton);
                                GUI.DrawTexture(new Rect(1720, height, 50, 50), yButton);
                            }
                            else
                            {
                                GUI.DrawTexture(new Rect(1510, height, 50, 50), qButton);
                                GUI.DrawTexture(new Rect(1720, height, 50, 50), eButton);
                            }
                        }
                    }
                    else
                    {
                        if (GUI.Button(editRect, "") && !locked && guiAlpha == 1f)
                        {
                            EditServer(servers[i]);
                        }

                        if (GUI.Button(deleteRect, "") && !locked && guiAlpha == 1f)
                        {
                            servers.RemoveAt(i);
                        }
                    }
                }

                GUIHelper.EndScrollView();
            }

            GUIHelper.ResetColor();
        }
    }

    void Update()
    {
        if (!Cursor.visible && guiAlpha == 1f && !locked && !CurrentGameData.blackOut)
        {
            int vertical = 0, horizontal = 0;
            bool submitBool = false, cancelBool = false, editBool = false, deleteBool = false;

            if (state != MenuState.Connecting && state != MenuState.CharacterSelect && state != MenuState.Gamemode)
            {
                vertical = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");
                horizontal = InputManager.controllers[0].GetIntInputWithLock("MenuHorizontal");
                submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
                cancelBool = InputManager.controllers[0].GetButtonWithLock("Cancel");
                editBool = InputManager.controllers[0].GetButtonWithLock("Edit");
                deleteBool = InputManager.controllers[0].GetButtonWithLock("Delete");
            }

            if (submitBool)
            {
                if(optionsList.Count > 0 && currentSelection < optionsList.Count)
                {
                    int selectableCount = 0;
                    for (int i = 0; i < optionsList.Count; i++)
                    {
                        if(optionsList[i].selectable)
                        { 
                            if (selectableCount == currentSelection)
                            {
                                DoSubmit(optionsList[i].id);
                                break;
                            }
                            selectableCount++;
                        }
                    }
                }
            }

            if (horizontal != 0)
            {
                switch (optionsList[currentSelection].id)
                {
                    case "Gamemode": hostServerSettings.gamemode = MathHelper.NumClamp(hostServerSettings.gamemode + horizontal, 0, gd.onlineGameModes.Length); break;
                    case "AIDifficulty": hostServerSettings.aiDifficulty = MathHelper.NumClamp(hostServerSettings.aiDifficulty + horizontal, 0, 4); break;
                    case "HostGamemode": host.ChangeGamemode(host.currentGamemode + 1); break;
                    case "MinPlayers": hostServerSettings.minPlayers = MathHelper.NumClamp(hostServerSettings.minPlayers + horizontal, 1, 13); break;
                }
            }

            //Allow menus to be scrolled through
            if (vertical != 0)
            {
                int selectableCount = 0;
                for (int i = 0; i < optionsList.Count; i++)
                {
                    if (optionsList[i].selectable)
                        selectableCount++;
                }
                if(state != MenuState.ServerList)
                    currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, selectableCount);
                else
                    currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, selectableCount + servers.Count);
            }

            switch (state)
            {
                case MenuState.ServerList:
                    //Move scroll view 
                    scrollPosition.y = (Mathf.Clamp(currentSelection, 2, servers.Count + 2) - 2f) * 60f;

                    //Allow menus to be scrolled through                  
                    if(cancelBool)
                    {
                        FindObjectOfType<MainMenu>().BackMenu();
                    }

                    if(editBool && currentSelection >= 2)
                    {
                        EditServer(servers[currentSelection - 2]);
                    }

                    if (deleteBool && currentSelection >= 2)
                    {
                        servers.RemoveAt(currentSelection - 2);
                        currentSelection = 0;
                    }

                    break;
                case MenuState.Connecting: break;
                case MenuState.HostSetup:
                    if (cancelBool)
                    {
                        ChangeState(MenuState.ServerList);
                    }

                    break;
                case MenuState.Lobby:

                    if (cancelBool)
                    {
                        if(networkState == NetworkState.Client)
                        {
                            LeaveServer();
                        }
                        else
                        {
                            CloseServer();
                        }
                    }

                    break;
                case MenuState.CharacterSelect:
                case MenuState.Gamemode:
                    break;

                case MenuState.PopUp:
                    if (!locked && guiAlpha == 1f)
                    {
                        if (submitBool || cancelBool)
                        {
                            popupMessage = "";
                            ChangeState(MenuState.ServerList);
                        }
                    }
                    break;
            }
        }
    }

    private void DoSubmit(string _currentSelection)
    {
        switch(_currentSelection)
        {
            case "HostServer": StartServer(); break;
            case "AddServer": RegisterServer(); break;
            case "ConfirmStartServer": HostGame(); break;
            case "Gamemode": hostServerSettings.gamemode = MathHelper.NumClamp(hostServerSettings.gamemode + 1, 0, gd.onlineGameModes.Length); break;
            case "Automatic": hostServerSettings.automatic = !hostServerSettings.automatic; break;
            case "HostAsPlayer" : hostServerSettings.hostAsPlayer = !hostServerSettings.hostAsPlayer; break;
            case "FillWithAI": hostServerSettings.fillWithAI = !hostServerSettings.fillWithAI; break;
            case "AIDifficulty": hostServerSettings.aiDifficulty = MathHelper.NumClamp(hostServerSettings.aiDifficulty + 1, 0, 4); break;
            case "LeaveServer": LeaveServer(); break;
            case "ChangeCharacter": ChangeLayout(); break;
            case "HostGamemode": host.ChangeGamemode(host.currentGamemode + 1); break;
            case "StartGamemode": host.StartGamemode(); break;
            case "MinPlayers": hostServerSettings.minPlayers = MathHelper.NumClamp(hostServerSettings.minPlayers + 1, 1, 13); break;
            default:
                Debug.Log(_currentSelection + " does not have any behaviour");
                break;
        }
    }

    private IEnumerator FadeTo(float value)
    {
        float startTime = Time.time, startVal = guiAlpha, travelTime = 0.5f;

        while(Time.time - startTime < travelTime)
        {
            guiAlpha = Mathf.Lerp(startVal, value, (Time.time - startTime) / travelTime);
            yield return null;
        }

        guiAlpha = value;
    }

    //A server has been selected from the list, attempt to join it
    private void ServerSelected(ServerInfo server)
    {
        StartCoroutine(ActualStartClient(server));
    }

    //Makes a popup appear
    public void Popup(string message)
    {
        popupMessage = message;
        ChangeState(MenuState.PopUp);
    }

    //Start a Server
    public void HostGame()
    {
        if (hostServerSettings != null)
        {
            StartCoroutine(ActualHostGame());
        }
    }

    public IEnumerator ActualHostGame()
    {
        MainMenu.lockInputs = true;
        networkState = NetworkState.Host;
        playerList = new List<PlayerInfo>();

        //Wait for fade
        yield return ActualChangeState(MenuState.Connecting);

        Debug.Log("Started Server!");
        //Pick a character before game starts
        if (hostServerSettings.hostAsPlayer)
        {
            CharacterSelect cs = FindObjectOfType<CharacterSelect>();
            cs.enabled = true;

            yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);

            //Wait until all characters have been selected
            while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
            {
                yield return null;
            }

            if (cs.State == CharacterSelect.csState.Off)
            {
                ChangeState(MenuState.ServerList);
                Debug.Log("Didn't make it through the Character Select!");
                yield break;
            }

            networkState = NetworkState.PlayableHost;
        }

        if (hostServerSettings.consoleMode)
        {
            networkState = NetworkState.ConsoleHost;
        }

        //Everything worked out perfect!
        Debug.Log("It worked");
        host = gd.gameObject.AddComponent<YogscartNetwork.Host>();
        host.playerPrefab = Resources.Load<GameObject>("Prefabs/Kart Maker/Network Kart");
        host.serverSettings = hostServerSettings;
        host.networkPort = serverPort;

        if(hostServerSettings.hostAsPlayer)
        {
            client = host;
        }

        try
        {
            host.StartHost();
            host.RegisterHandlers();
            host.ChangeGamemode(hostServerSettings.gamemode);
            ChangeState(MenuState.Lobby);
        }
        catch (System.Exception err)
        {
            Debug.Log("Error caught!");
            Popup(err.Message);
        }
    }

    private IEnumerator ActualStartClient(ServerInfo server)
    {
        MainMenu.lockInputs = true;
        currentSelection = 0;
        networkState = NetworkState.Client;
        playerList = new List<PlayerInfo>();

        //Wait for fade
        yield return ActualChangeState(MenuState.Connecting);

        client = gd.gameObject.AddComponent<YogscartNetwork.Client>();
        client.playerPrefab = Resources.Load<GameObject>("Prefabs/Kart Maker/Network Kart");

        Debug.Log("Connecting to " + server.ip + ":" + server.port + " Pass:" + server.password);

        client.networkAddress = server.ip;
        client.networkPort = server.port;

        try
        {
            if (server.ip == "" || server.ip == null)
                throw new System.Exception("No IP has been entered");

            client.StartClient();
            client.RegisterHandlers();
        }
        catch
        {
            Debug.Log("Error caught!");
            client.EndClient("Invalid Server IP");
        }
    }

    //Show Menu for starting a server
    private void StartServer()
    {
        ChangeState(MenuState.HostSetup);
        hostServerSettings = new ServerSettings();
        currentSelection = 0;
    }

    //Show Poup menu for creating a new server entry
    private void RegisterServer()
    {
        locked = true;
        EditServer editServer = gameObject.AddComponent<EditServer>();
        editServer.Setup(skin);
        editServer.Show();
    }

    //Show Poup menu for editing a server entry
    private void EditServer(ServerInfo server)
    {
        locked = true;
        EditServer editServer = gameObject.AddComponent<EditServer>();
        editServer.Setup(server, skin);
        editServer.Show();
    }

    //Save current list of servers
    public void SaveServers()
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

    //Load list of servers
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

    //Leave the Game
    void LeaveServer()
    {
        client.EndClient(null);
    }

    //Close the Game
    void CloseServer()
    {
        host.EndClient(null);
    }

    //Request a Character Change from the Server
    void ChangeLayout()
    {
        client.DoCharacterSelect();
    }
}

public class ServerSettings
{
    public ServerSettings(int _gamemode, int _maxPlayers, int _minPlayers,
        bool _automatic, bool _consoleMode, bool _hostAsPlayer, bool _fillWithAI,
        int _aiDifficulty, string _password)
    {
        gamemode = _gamemode;
        maxPlayers = _maxPlayers;
        minPlayers = _minPlayers;
        consoleMode = _consoleMode;
        hostAsPlayer = _hostAsPlayer;
        fillWithAI = _fillWithAI;
        aiDifficulty = _aiDifficulty;
        password = _password;
    }

    public ServerSettings() { }

    //Start Game Mode (Can be changed later)
    public int gamemode = 0;

    //Max Players (12 Max in Race, remaining are spectators)
    public int maxPlayers = 12;

    //Automatic (Games will start automatically when player total met in lobby)
    public bool automatic = false;

    //Minimum Players (How many players are needed for an automatic game)
    public int minPlayers = 2;

    //Console Mode (No Visual element is rendered, just a console)
    public bool consoleMode = false;

    //Spawn Host as Player
    public bool hostAsPlayer = true;

    //Fill spaces with AI
    public bool fillWithAI = false;

    //AI Difficulty 0 - 50cc 1 - 100cc 2 - 150cc 3 - Insane
    public int aiDifficulty = 0;

    public string password;

    public string GetString(int val)
    {
        switch (val)
        {
            case 0: return "";
            case 1: return maxPlayers.ToString();
            case 2: return automatic.ToString();
            case 3: return minPlayers.ToString();
            case 4: return consoleMode.ToString();
            case 5: return hostAsPlayer.ToString();
            case 6: return fillWithAI.ToString();
            case 7:
                string[] difficulties = new string[] { "50cc", "100cc", "150cc", "Insane" };
                return difficulties[aiDifficulty];
            case 8:
                return password;
        }

        return "";
    }
}

public class PlayerInfo
{
    public string displayName;
    public int character, hat, kart, wheel;
    public int ping;

    public PlayerInfo (string _displayName, int _character, int _hat, int _kart, int _wheel)
    {
        displayName = _displayName;
        character = _character;
        hat = _hat;
        kart = _kart;
        wheel = _wheel;
    }

    public PlayerInfo(PlayerInfoMessage _message)
    {
        displayName = _message.displayName;
        character = _message.character;
        hat = _message.hat;
        kart = _message.kart;
        wheel = _message.wheel;
    }

}
