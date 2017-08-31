using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSelection : MonoBehaviour
{
    public enum MenuState { ServerList, Connecting, HostSetup, Lobby, CharacterSelect, PopUp};
    public MenuState state;

    private float guiAlpha = 0f;
    public GUISkin skin;

    //Menu Selection
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
    public UnetHost host;
    public UnetClient client;

    //Textures
    private Texture2D xButton, yButton, qButton, eButton, connectionCircle;
    private float connectRot = 0f;

    public void Show() { StartCoroutine(FadeTo(1f)); currentSelection = 0; LoadServers(); gd = FindObjectOfType<CurrentGameData>(); }
    public void Hide() { StartCoroutine(FadeTo(0f)); }
    public bool isShowing { get { return guiAlpha > 0f; } }

    public void ChangeState(MenuState newState) { StartCoroutine(ActualChangeState(newState)); }

    private IEnumerator ActualChangeState(MenuState newState)
    {
        yield return FadeTo(0f);
        state = newState;
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

    void OnGUI()
    {
        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();

        if (guiAlpha > 0f)
        {
            GUIHelper.SetGUIAlpha(guiAlpha);
            switch (state)
            {
                case MenuState.ServerList:

                    //Start Server
                    Rect startServerRect = new Rect(50, 50, 800, 50);
                    GUIShape.RoundedRectangle(startServerRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));
                    GUIHelper.LeftRectLabel(startServerRect, 1f, "  Host a Server", (currentSelection == 0) ? Color.yellow : Color.white);

                    if (Cursor.visible && startServerRect.Contains(GUIHelper.GetMousePosition()))
                        currentSelection = 0;

                    if (GUI.Button(startServerRect,"") && !locked && guiAlpha == 1f)
                        StartServer();

                    //Create a Server
                    Rect createServerRect = new Rect(50, 110, 800, 50);
                    GUIShape.RoundedRectangle(createServerRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));
                    GUIHelper.LeftRectLabel(createServerRect, 1f, "  Add a new Server", (currentSelection == 1) ? Color.yellow : Color.white);

                    if (Cursor.visible && !locked && createServerRect.Contains(GUIHelper.GetMousePosition()))
                        currentSelection = 1;

                    if (GUI.Button(createServerRect, "") && !locked && guiAlpha == 1f)
                        RegisterServer();

                    //List of all available servers
                    scrollPosition = GUIHelper.BeginScrollView(new Rect(50, 180, 1800, 500), scrollPosition, new Rect(0, 0, 1750, servers.Count * 60f));

                    //Draw Servers
                    for (int i = 0; i < servers.Count; i++)
                    {
                        int height = i * 60;
                        float scaleText = 0.75f;

                        //Draw a Rectangle
                        Rect rectangleRect = new Rect(0, height, 1780, 50);
                        GUIShape.RoundedRectangle(rectangleRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));

                        //Get actual ui position for highlighting by mouse
                        Rect actualRectangleRect = new Rect(rectangleRect);
                        actualRectangleRect.x += 50;
                        actualRectangleRect.y += 180 - scrollPosition.y;

                        Vector2 mousePos = GUIHelper.GetMousePosition();
                        if (Cursor.visible && !locked && mousePos.y > 180f && mousePos.y < 680f && actualRectangleRect.Contains(mousePos))
                            currentSelection = i + 2;

                        //Let user join server
                        Rect clickableRect = new Rect(rectangleRect);
                        clickableRect.width = 1410;
                        if(GUI.Button(clickableRect,"") && !locked && guiAlpha == 1f)
                            ServerSelected(servers[currentSelection - 2]);

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
                            if(GUI.Button(editRect, "") && !locked && guiAlpha == 1f)
                            {
                                currentSelection = i;
                                EditServer(servers[currentSelection]);
                            }

                            if (GUI.Button(deleteRect, "") && !locked && guiAlpha == 1f)
                            {
                                currentSelection = i;
                                servers.RemoveAt(currentSelection);
                            }
                        }            
                    }


                    GUIHelper.EndScrollView();

                    break;
                case MenuState.Connecting:
                    GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

                    float chunkSize = Mathf.Min(Screen.width, Screen.height) / 5f;
                    GUIUtility.RotateAroundPivot(connectRot, new Vector2(Screen.width / 2f, Screen.height / 2f));
                    GUI.DrawTexture(new Rect(Screen.width / 2f - chunkSize, Screen.height / 2f - chunkSize, 2f * chunkSize, 2f * chunkSize), connectionCircle);
                    connectRot += Time.deltaTime * -50f;
                    break;
                case MenuState.HostSetup:

              /*case 0: return gamemodeName;
                case 1: return maxPlayers.ToString();
                case 2: return automatic.ToString();
                case 3: return minPlayers.ToString();
                case 4: return consoleMode.ToString();
                case 5: return hostAsPlayer.ToString();
                case 6: return fillWithAI.ToString();
                case 7: return aiDifficulty.ToString();*/

                    string[] options = new string[] { "   Gamemode: ", "   Maximum Players: ", "   Automatic: ", "   Minimum Players: ", "   Console Mode: ", "   Host is Playing: ", "   Fill empty spaces with AI: ", "   AI Difficulty: " };

                    for (int i = 0; i < options.Length; i++)
                    {
                        if ((i != 3 && i != 5 && i != 7) || (i == 3 && hostServerSettings.automatic) || (i == 5 && !hostServerSettings.consoleMode) || (i == 7 && hostServerSettings.fillWithAI))
                        {
                            Rect optionRect = new Rect(50, 50 + (i * 60), 800, 50);

                            GUIShape.RoundedRectangle(optionRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));
                            GUIHelper.LeftRectLabel(optionRect, 1f, options[i] + hostServerSettings.GetString(i), (currentSelection == i) ? Color.yellow : Color.white);

                            if (Cursor.visible && optionRect.Contains(GUIHelper.GetMousePosition()))
                                currentSelection = i;

                            //Do Input
                            if (i == 2 || i == 4 || i == 5 || i == 6)
                            {
                                if (GUI.Button(optionRect, "") && !locked && guiAlpha == 1f)
                                {
                                    switch(i)
                                    {
                                        case 2: hostServerSettings.automatic = !hostServerSettings.automatic; break;
                                        case 4: hostServerSettings.consoleMode = !hostServerSettings.consoleMode; break;
                                        case 5: hostServerSettings.hostAsPlayer = !hostServerSettings.hostAsPlayer; break;
                                        case 6: hostServerSettings.fillWithAI = !hostServerSettings.fillWithAI; break;
                                    }
                                }
                            }
                        }
                    }

                    //Start Server!
                    startServerRect = new Rect(50, 50 + (9 * 60), 800, 50);
                    GUIShape.RoundedRectangle(startServerRect, 5, new Color(0.4f, 0.4f, 0.4f, 0.4f * guiAlpha));
                    GUIHelper.LeftRectLabel(startServerRect, 1f, "   Start Server", (currentSelection == 8) ? Color.yellow : Color.white);

                    if (Cursor.visible && startServerRect.Contains(GUIHelper.GetMousePosition()))
                        currentSelection = 8;

                    if (GUI.Button(startServerRect, "") && !locked && guiAlpha == 1f)
                        HostGame();

                    break;
                case MenuState.Lobby:

                    break;
                case MenuState.CharacterSelect:

                    break;
                case MenuState.PopUp:
                    GUI.Label(new Rect(200, 200, 1580, 500), popupMessage);
                    break;
            }

            GUIHelper.ResetColor();
        }
    }

    void Update()
    {
        if (!Cursor.visible && guiAlpha == 1f && !locked)
        {
            int vertical = 0, horizontal = 0;
            bool submitBool = false, cancelBool = false, editBool = false, deleteBool = false;

            if (state != MenuState.Connecting)
            {
                vertical = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");
                horizontal = InputManager.controllers[0].GetIntInputWithLock("MenuHorizontal");
                submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
                cancelBool = InputManager.controllers[0].GetButtonWithLock("Cancel");
                editBool = InputManager.controllers[0].GetButtonWithLock("Edit");
                deleteBool = InputManager.controllers[0].GetButtonWithLock("Delete");
            }

            switch (state)
            {
                case MenuState.ServerList:
                    //Move scroll view 
                    scrollPosition.y = (Mathf.Clamp(currentSelection, 2, servers.Count + 2) - 2f) * 60f;

                    //Allow menus to be scrolled through
                    currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, servers.Count + 2);

                    if(submitBool)
                    {
                        switch(currentSelection)
                        {
                            case 0: StartServer(); break;
                            case 1: RegisterServer(); break;
                            default:
                                ServerSelected(servers[currentSelection-2]);
                                break;
                        }
                    }

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
                    }

                    break;
                case MenuState.Connecting:

                    break;
                case MenuState.HostSetup:

                    //Allow menus to be scrolled through
                    if (vertical != 0)
                    {
                        int newVal = MathHelper.NumClamp(currentSelection + vertical, 0, 9);

                        //If options isn't a toggleable one pass value through
                        if (newVal != 3 && newVal != 5 && newVal != 7)
                            currentSelection = newVal;
                        else
                        {
                            //Otherwise check that option is visible. If it isn't skip it
                            if((newVal == 3 && !hostServerSettings.automatic) || (newVal == 5 && hostServerSettings.consoleMode) || (newVal == 7 && !hostServerSettings.fillWithAI))
                            {
                                if(newVal < currentSelection)
                                    currentSelection = MathHelper.NumClamp(newVal - 1, 0, 9);
                                else
                                    currentSelection = MathHelper.NumClamp(newVal + 1, 0, 9);
                            }
                            else
                                currentSelection = newVal;
                        }

                    }

                    if (cancelBool)
                    {
                        ChangeState(MenuState.ServerList);
                    }

                    if(submitBool)
                    {
                        switch (currentSelection)
                        {
                            case 2: hostServerSettings.automatic = !hostServerSettings.automatic; break;
                            case 4: hostServerSettings.consoleMode = !hostServerSettings.consoleMode; break;
                            case 5: hostServerSettings.hostAsPlayer = !hostServerSettings.hostAsPlayer; break;
                            case 6: hostServerSettings.fillWithAI = !hostServerSettings.fillWithAI; break;
                            case 8: HostGame(); break;
                        }
                    }

                    if(horizontal != 0)
                    {
                        switch (currentSelection)
                        {
                            case 1: hostServerSettings.maxPlayers = MathHelper.NumClamp(hostServerSettings.maxPlayers + horizontal, 2, 31); break;
                            case 3: hostServerSettings.minPlayers = MathHelper.NumClamp(hostServerSettings.minPlayers + horizontal, 1, 13); break;
                            case 7: hostServerSettings.aiDifficulty = MathHelper.NumClamp(hostServerSettings.aiDifficulty + horizontal, 0, 4); break;
                        }
                    }

                    break;
                case MenuState.Lobby:

                    break;
                case MenuState.CharacterSelect:

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

        //Wait for fade
        yield return ActualChangeState(MenuState.Connecting);

        Debug.Log("Started Server!");
        //Pick a character before game starts
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
            ChangeState(NetworkSelection.MenuState.ServerList);
            Debug.Log("Didn't make it through the Character Select!");
            yield break;
        }

        //Everything worked out perfect!
        Debug.Log("It worked");
        host = gd.gameObject.AddComponent<UnetHost>();
        host.playerPrefab = Resources.Load<GameObject>("Prefabs/Kart Maker/Network Kart");
        host.settings = hostServerSettings;
        host.networkPort = serverPort;
        try
        {
            host.StartHost();
            host.RegisterHandlers();
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

        //Wait for fade
        yield return ActualChangeState(MenuState.Connecting);

        client = gd.gameObject.AddComponent<UnetClient>();
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
}

public class ServerSettings
{
    //Start Game Mode (Can be changed later)
    public string gamemodeName = "Race";

    //Max Players (12 Max in Race, remaining are spectators)
    public int maxPlayers = 12;

    //Automatic (Games will start automatically when player total met in lobby)
    public bool automatic = false;

    //Minimum Players (How many players are needed for an automatic game)
    public int minPlayers = 2;

    //Console Mode (No Visual element is rendered, just a console)
    public bool consoleMode = false;

    //Spawn Host as Player
    public bool hostAsPlayer = false;

    //Fill spaces with AI
    public bool fillWithAI = false;

    //AI Difficulty 0 - 50cc 1 - 100cc 2 - 150cc 3 - Insane
    public int aiDifficulty = 0;

    public string password;

    public string GetString(int val)
    {
        switch (val)
        {
            case 0: return gamemodeName;
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
