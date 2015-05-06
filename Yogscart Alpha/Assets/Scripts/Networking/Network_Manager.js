﻿#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;

enum ServerState {ServerList,Connecting,Popup,Lobby,LoadingRace,LoadingLevel,Racing};
var state : ServerState;

var popupText : String;

var editServer : boolean;
var servers : ServerInfo[];
private var serverScroll : Vector2 = Vector2.zero;;

private var hostPort : int = 25000;
private var playerName : String;

var finalPlayers : DisplayName[];

var currentSelection : int;

@HideInInspector
var SpectatorCam : GameObject;

class GameMode
{

	var name : String;
	var teamGame : boolean;
	var logo : Texture2D;
	
	var hostScript : MonoBehaviour;
	var baseScript : MonoBehaviour;
	
}

class ServerInfo
{

	var name : String;
	var description : String;
	var publicServer : boolean;
	
	var maxPlayers : int;
	var currentGameMode : int;
	
	var ip : String;
	var port : int;
	
	function ServerInfo()
	{
		name = "New Server";
		description = "";
		publicServer = false;
		maxPlayers = 12;
		currentGameMode = 0;
		ip = "127.0.0.1";
		port = 25000;
	}
	
	function ToString()
	{
		var returnString : String;
		
		returnString += name + ",";
		returnString += description + ",";
		returnString += ip + ",";
		returnString += port + ";";
		
		return returnString;
		
	}
	
	function LoadFromString(val : String)
	{
		
		var parts : String[] = val.Split(","[0]);
		
		name = parts[0];
		description = parts[1];
		publicServer = false;
		ip = parts[2];
		int.TryParse(parts[3],port);
		
	}
	
}

class DisplayName
{
	var name : String;
	var ping : int;
	var networkPlayer : NetworkPlayer;
	
	function DisplayName(n : String, np : NetworkPlayer)
	{
	name = n;
	networkPlayer = np;
	}
}

@RPC
function NewPlayer(name : String, np : NetworkPlayer)
{
	
	var nDisplayName = new DisplayName(name,np);

	var copy = new Array();
	
	if(finalPlayers != null)
		copy = finalPlayers;
		
	copy.Add(nDisplayName);
	finalPlayers = copy;
}

function OnPlayerDisconnected(player: NetworkPlayer) 
{

	for(var i : int; i < finalPlayers.Length; i++)
	{
		if(finalPlayers[i].networkPlayer == player)
			{
				var copy = new Array();	
				copy = finalPlayers;
				copy.RemoveAt(i);
				finalPlayers = copy;
				
				if(Network.isServer)
					Network.RemoveRPCsInGroup(i+1);
				
				break;
			}
	}

}

function Awake()
{
	im = transform.GetComponent(InputManager);
	gd = transform.GetComponent(CurrentGameData);
	
	playerName = PlayerPrefs.GetString("playerName","Player");
	
	LoadServers();
	
}

function SaveServers()
{
	var savedServers : String;
	
	if(servers != null && servers.Length > 0)
	{
		for(var i : int = 0; i < servers.Length; i++)
		{
			savedServers += servers[i].ToString();
		}
		
		savedServers = savedServers.Remove(savedServers.Length-1);
	}
	
	PlayerPrefs.SetString("YogscartServers",savedServers);

}

function LoadServers()
{
	var savedServers : String = PlayerPrefs.GetString("YogscartServers","");
	
	try
	{
		var serverStrings = savedServers.Split(";"[0]);
		
		var arr = new Array();
		
		for(var i : int = 0; i < serverStrings.Length; i++)
		{
			var nServer = new ServerInfo();
			nServer.LoadFromString(serverStrings[i]);
			
			arr.Add(nServer);
		}
		
		servers = arr;
	}
	catch(err)
	{
	Debug.Log("Servers List string not in the correct format.");
	}
}

function OnGUI()
{

	GUI.skin = Resources.Load("GUISkins/Menu",GUISkin);
	
	var fontSize = ((Screen.height + Screen.width)/2f)/40f;
	GUI.skin.label.fontSize = fontSize;

	var chunkSize = Screen.width/10f;
	
	if(popupText != null && popupText != "")
		state = ServerState.Popup;
	
	var nameList : Texture2D = Resources.Load("UI/Lobby/NamesList",Texture2D);
	var nameListRect : Rect = Rect(chunkSize,chunkSize/2f,chunkSize*3f,Screen.height - chunkSize*1.5f);
	var gapSize = chunkSize/2f;
	
	//Render Background
	var BackgroundTexture : Texture2D = Resources.Load("UI/Main Menu/TempBackground",Texture2D);
	//GUI.DrawTexture(Rect(0,0,Screen.width,Screen.height),BackgroundTexture,ScaleMode.ScaleAndCrop);
	
	switch(state)
	{
	
	case ServerState.ServerList:
	
	if(!editServer)
	{
			var vert : float = im.c[0].GetMenuInput("Vertical");
				
			if(vert > 0)
			{
				currentSelection -= 1;
			}
			
			if(vert < 0)
			{
				currentSelection += 1;
			}
	}
	
	if(currentSelection < 0)
			currentSelection = Mathf.Clamp(servers.Length-1,0,49);
		
	if(currentSelection >= Mathf.Clamp(servers.Length,0,50))
		currentSelection = 0;
	
	GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*1.5f,chunkSize*4,chunkSize/2f),"Port: ");
	int.TryParse(GUI.TextField(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.75f + gapSize*1.5f,chunkSize*4,chunkSize/4f),hostPort.ToString()),hostPort);
	
	GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*3.5f,chunkSize*4,chunkSize/2f),"Name (Temporary): ");
	playerName = GUI.TextField(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *3.75f + gapSize*1.5f,chunkSize*4,chunkSize/4f),playerName);
	
	if(GUI.Button(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *3f + gapSize*1.5f,chunkSize*4,chunkSize/2f),"Host Server"))
		{
			StartServer();
		}
	
	GUI.DrawTexture(nameListRect,nameList);
	
	nameListRect = Rect(chunkSize + gapSize - 10,chunkSize/2f + gapSize,chunkSize*3f - (gapSize*1.5f),Screen.height - chunkSize*1.5f - (gapSize*2f));
	
	GUI.BeginGroup(nameListRect);
	
	GUI.Label(Rect(10,0,nameListRect.width,fontSize*2f),"Servers");
	
	GUI.DrawTexture(Rect(10,fontSize*1.5f,nameListRect.width,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
	
	serverScroll = GUI.BeginScrollView(Rect(0,fontSize*2.2f,nameListRect.width,nameListRect.height - 20 - fontSize*2.2f),serverScroll,Rect(0,0,nameListRect.width - 20,(Mathf.Clamp(servers.Length,10,50)+1) * fontSize*1.25f));
	
	if(servers != null && servers.Length > 0)
		for(var i : int; i < Mathf.Clamp(servers.Length,0,50);i++)
		{
		
			var startHeight = (i*fontSize*1.25f);
			var textSize = fontSize;
			
			GUI.skin.label.fontSize = textSize;
			GUI.skin.textField.fontSize = textSize;
			
			if(currentSelection != i || !editServer )
				GUI.Label(Rect(10,startHeight,nameListRect.width - 20 - fontSize*2f,fontSize*1.25f),servers[i].name);
			else
				servers[i].name = GUI.TextField(Rect(10,startHeight,nameListRect.width- 20 - fontSize*2f,fontSize*1.25f),servers[i].name);
			
			if(servers[i].publicServer)
				GUI.DrawTexture(Rect(nameListRect.width - (fontSize*5f),startHeight,fontSize*2.5f,fontSize*1.25f),Resources.Load("UI/Lobby/Public",Texture2D));
			
			GUI.DrawTexture(Rect(nameListRect.width - 10 - fontSize*2f,startHeight,fontSize*1.25f,fontSize*1.25f),Resources.Load("UI/Lobby/NoConnection",Texture2D));
				
			GUI.skin.label.fontSize = fontSize;
			GUI.skin.textField.fontSize = fontSize;
			
			if(i == currentSelection)
				GUI.DrawTexture(Rect(0,startHeight,nameListRect.width,fontSize*1.25f),Resources.Load("UI/Lobby/Selected",Texture2D));
				
			if(Input.GetMouseButtonDown(0) && im.MouseIntersects(Rect(chunkSize + gapSize - 10,chunkSize/2f + gapSize + fontSize*2.2f + startHeight,nameListRect.width,fontSize*1.25f)))
				currentSelection = i;
			
		}
	
	GUI.EndScrollView();
	
	GUI.EndGroup();
	
	//Additional Buttons
	
	// Server Information
	if(servers != null && servers.Length > 0)
	{
		var serverInfoRect = Rect(Screen.width - chunkSize * 6f,chunkSize/2f + gapSize*1.5f,chunkSize*5f,chunkSize * 1.5f);
		var serverInfo : Texture2D = Resources.Load("UI/Lobby/ServerInfo",Texture2D);
		
		GUI.DrawTexture(serverInfoRect,serverInfo);
		
		GUI.BeginGroup(serverInfoRect);
		
		GUI.Label(Rect(20,10,serverInfoRect.width-40,fontSize*2f),"Server Info");
		
		GUI.DrawTexture(Rect(20,10 + fontSize*1.25f,serverInfoRect.width - 40,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
		
		textSize = Mathf.Clamp(fontSize / 1.5f,0,fontSize);
		GUI.skin.label.fontSize = textSize;
		GUI.skin.textArea.fontSize = textSize;
		GUI.skin.textField.fontSize = textSize;
		
		if(!editServer)
		{
			GUI.Label(Rect(20, 10 + fontSize*2f,serverInfoRect.width - 40, serverInfoRect.height - fontSize*4.5f),servers[currentSelection].description);
			GUI.Label(Rect(20, serverInfoRect.height - fontSize*1.5f,serverInfoRect.width/2f,fontSize),"IP: " + servers[currentSelection].ip);
			GUI.Label(Rect(30 + serverInfoRect.width/2f, serverInfoRect.height - fontSize*1.5f,serverInfoRect.width/2f, fontSize),"Port: " + servers[currentSelection].port.ToString());
		}
		else
		{
			servers[currentSelection].description = GUI.TextArea(Rect(20, 10 + fontSize*2f,serverInfoRect.width - 40, serverInfoRect.height - fontSize*4.5f),servers[currentSelection].description);
			
			GUI.Label(Rect(20, serverInfoRect.height - fontSize*1.5f,serverInfoRect.width/2f,fontSize),"IP: ");
			servers[currentSelection].ip = GUI.TextField(Rect(20 + (textSize*1.5f), serverInfoRect.height - fontSize*1.5f,serverInfoRect.width/2f - textSize*1.5f,fontSize),servers[currentSelection].ip);
			
			GUI.Label(Rect(30 + serverInfoRect.width/2f, serverInfoRect.height - fontSize*1.5f,serverInfoRect.width/2f, fontSize),"Port: ");
			int.TryParse(GUI.TextField(Rect(30+ (textSize*2.75f) + serverInfoRect.width/2f, serverInfoRect.height - fontSize*1.5f,textSize*8f, fontSize),servers[currentSelection].port.ToString()),servers[currentSelection].port);
		}
			
		GUI.skin.label.fontSize = fontSize;
		GUI.skin.textArea.fontSize = fontSize;
		GUI.skin.textField.fontSize = fontSize;
		
		GUI.EndGroup();
		
		if(!editServer)
			if(GUI.Button(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Join"))
			{
				ConnectToServer();
			}
		
		if(!servers[currentSelection].publicServer && !editServer)
		{
			
			if(GUI.Button(Rect(10 + Screen.width - chunkSize * 5f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Edit"))
			{
				editServer = true;
			}
			
			if(GUI.Button(Rect(10 + Screen.width - chunkSize * 4f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Delete"))
			{
				var copy = new Array();
				copy = servers;
				copy.RemoveAt(currentSelection);
				servers = copy;
				SaveServers();
			}
			
		}
		
		if(editServer)
		{
			if(GUI.Button(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Save"))
				{
					editServer = false;
					SaveServers();
				}
		}
		
	}
	
	if(!editServer && servers.Length < 50)
		if(GUI.Button(Rect(10 + Screen.width - chunkSize * 3f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Add New"))
			{
				editServer = true;
				
				var nServer = new ServerInfo();
				
				copy = new Array();
				copy = servers;
				copy.Add(nServer);
				servers = copy;
				
				currentSelection = servers.Length - 1;
			}
	
	break;
	
	case ServerState.Lobby:
	
	if(currentSelection < 0)
			currentSelection = gd.onlineGameModes.Length-1;
		
	if(currentSelection >= gd.onlineGameModes.Length)
		currentSelection = 0;
	
	if(GameObject.Find("Menu Holder") != null)
		if(!GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled){
		
			GUI.DrawTexture(nameListRect,nameList);
			
			nameListRect = Rect(chunkSize + gapSize,chunkSize/2f + gapSize,chunkSize*3f - (gapSize*2f),Screen.height - chunkSize*1.5f - (gapSize*2f));

			GUI.BeginGroup(nameListRect);
			
			GUI.Label(Rect(0,0,nameListRect.width,fontSize*2f),"Lobby");
			
			GUI.DrawTexture(Rect(0,fontSize*1.5f,nameListRect.width - 10,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
			
			for(i = 0; i < finalPlayers.Length;i++)
			{
			
				startHeight = fontSize*2f + (i*fontSize*1.25f);
			
				textSize = Mathf.Clamp(fontSize / (finalPlayers[i].name.Length /10f),0,fontSize);	
				GUI.skin.label.fontSize = textSize;
				
				GUI.Label(Rect(0,startHeight,nameListRect.width,fontSize*1.25f),finalPlayers[i].name);

				if(finalPlayers[i].networkPlayer != null)
					finalPlayers[i].ping = Network.GetAveragePing(finalPlayers[i].networkPlayer);		
					
				textSize = Mathf.Clamp(fontSize/2f,0,fontSize);
				GUI.skin.label.fontSize = textSize;
					
				GUI.Label(Rect(nameListRect.width - (textSize*5f),startHeight + textSize*0.5f,textSize*10f,fontSize*1.5f),finalPlayers[i].ping.ToString() + " Ping");

				GUI.skin.label.fontSize = fontSize;
				
			}
		
		GUI.EndGroup();
		
		if(GUI.Button(Rect(chunkSize ,nameListRect.x + nameListRect.height,chunkSize,chunkSize/2f),"Quit"))
			{
				Network.Disconnect();	
			}
		}
		
	if(Network.isServer)
	{	
		if(GUI.Button(Rect(chunkSize * 2f,nameListRect.x + nameListRect.height,chunkSize,chunkSize/2f),"Start"))
			{
				StartGame();
			}
		
		var hostInfoRect = Rect(Screen.width - chunkSize * 6f,chunkSize/2f + gapSize*1.5f,chunkSize*5f,chunkSize * 2.5f);
		serverInfo = Resources.Load("UI/Lobby/ServerInfo",Texture2D);
		GUI.DrawTexture(hostInfoRect,serverInfo);
		
		GUI.BeginGroup(hostInfoRect);
		
		GUI.Label(Rect(20,10,hostInfoRect.width-40,fontSize*2f),"Host Options");
		GUI.DrawTexture(Rect(20,10 + fontSize*1.25f,hostInfoRect.width - 40,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
		
		GUI.Label(Rect(20,10+ fontSize*1.5f,hostInfoRect.width-40,fontSize*2f),"Game Mode: ");
		serverScroll = GUI.BeginScrollView(Rect(20,10 + fontSize*2.6f,hostInfoRect.width/2f - 20,hostInfoRect.height - fontSize*2.8f - 20),serverScroll,Rect(0,0,hostInfoRect.width/2f - 40,Mathf.Clamp(gd.onlineGameModes.Length,6,Mathf.Infinity) * fontSize*1.25f));
		
		for(i = 0; i < gd.onlineGameModes.Length; i++)
		{
			GUI.Label(Rect(10,i * fontSize * 1.25f,hostInfoRect.width - 20 - fontSize*2f,fontSize*1.25f),gd.onlineGameModes[i].name);
			
			if(i == currentSelection)
				GUI.DrawTexture(Rect(0,i * fontSize * 1.25f,nameListRect.width,fontSize*1.25f),Resources.Load("UI/Lobby/Selected",Texture2D));
				
				if(Input.GetMouseButtonDown(0) && im.MouseIntersects(Rect(Screen.width - (chunkSize * 6f) + 20,chunkSize/2f + gapSize*1.5f + 10 + fontSize*2.6f + (i * fontSize * 1.25f),nameListRect.width,fontSize*1.25f)))
					currentSelection = i;
				
		}
		
		GUI.EndScrollView();
		GUI.EndGroup();
		
	}
	
	break;
	
	case ServerState.Popup:
	
		var popupRect = Rect(Screen.width/2f - (chunkSize*5f)/2f,chunkSize/2f + gapSize*1.5f,chunkSize*5f,chunkSize * 1.5f);
		serverInfo = Resources.Load("UI/Lobby/ServerInfo",Texture2D);
		
		GUI.DrawTexture(popupRect,serverInfo);
		
		GUI.BeginGroup(popupRect);
		
		GUI.Label(Rect(20,10,popupRect.width-40,fontSize*2f),"Network Message");
		GUI.DrawTexture(Rect(20,10 + fontSize*1.25f,popupRect.width - 40,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
		
		GUI.Label(Rect(20, 10 + fontSize*2f,popupRect.width - 40, popupRect.height - fontSize*4.5f),popupText);
		
		GUI.EndGroup();
		
		if(GUI.Button(Rect(Screen.width/2f - chunkSize/2f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Okay"))
		{
					popupText = "";
					state = ServerState.ServerList;
		}
	
	break;
	}
}

function ConnectToServer()
{

	PlayerPrefs.SetString("playerName",playerName);
	
	state = ServerState.Connecting;
	Network.Connect(servers[currentSelection].ip,servers[currentSelection].port);
}

function StartServer()
{

	PlayerPrefs.SetString("playerName",playerName);
	
	serverScroll = Vector2.zero;
	currentSelection = 0;
	
	transform.GetComponent(Host_Script).Reset();
	
	state = ServerState.Connecting;
	
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled = true;
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).ResetEverything();
	
	while(GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled)
		yield;

	Network.InitializeServer(25,hostPort,true);
	transform.GetComponent(Host_Script).enabled = true;
	
	var test = new NetworkMessageInfo();
	//test.sender = GetComponent.<NetworkView>().owner;
	
	transform.GetComponent(Host_Script).RecievedNewRacer(PlayerPrefs.GetString("playerName","Player"),gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel,test);//Add support for Character Select
	
	GetComponent.<NetworkView>().RPC("LoadNetworkLevel",RPCMode.AllBuffered,"Lobby",0);
	
	yield WaitForSeconds(0.5f);
	state = ServerState.Lobby;
	
}

function StartGame()
{
	if(gd.onlineGameModes[currentSelection].baseScript != null)
	{
		
		GetComponent.<NetworkView>().RPC("StartGamemode",RPCMode.All,currentSelection);
		
		if(gd.onlineGameModes[currentSelection].hostScript != null)
		{
			gd.onlineGameModes[currentSelection].hostScript.enabled = true;
			gd.onlineGameModes[currentSelection].hostScript.StartCoroutine("StartGame");
		}
			
	}
	else
	{
		Debug.Log("The selected gamemode is missing a base script. This is required for multiplayer");
	}
}

@RPC
function StartGamemode(i : int)
{
	state = ServerState.Racing;
	gd.onlineGameModes[i].baseScript.enabled = true;
	gd.onlineGameModes[i].baseScript.StartCoroutine("StartGame");
}

function EndGame()
{

	GetComponent.<NetworkView>().RPC("LoadNetworkLevel",RPCMode.AllBuffered,"Lobby",0);
	
	while(Application.loadedLevelName != "Lobby")
		yield;
	
	Network.RemoveRPCs(GetComponent.<NetworkView>().owner);	
				
	state = ServerState.Lobby;
	
	var hs = transform.GetComponent(Host_Script);
	
	GetComponent.<NetworkView>().RPC("ClearNames",RPCMode.All);
		
	for(var i : int = 0; i < hs.RacingPlayers.Length; i++)
	{
		GetComponent.<NetworkView>().RPC("NewPlayer",RPCMode.AllBuffered,hs.RacingPlayers[i].name,hs.RacingPlayers[i].networkplayer);
	}
	
}

@RPC
function ClearNames()
{
	finalPlayers = new DisplayName[0];
}
		

function OnConnectedToServer() 
{
	state = ServerState.Lobby;
		
	transform.GetComponent(Client_Script).enabled = true;
	GetComponent.<NetworkView>().RPC("VersionUpdate",RPCMode.Server,gd.version);
}

function OnDisconnectedFromServer(info : NetworkDisconnection) 
{
	ServerFinish(info.ToString());
	finalPlayers = new DisplayName[0];
	
	while(popupText != "")
		yield;
		
	Destroy(SpectatorCam);
		
	gd.Exit();
	
	this.enabled = false;
	
}

function OnFailedToConnect(error: NetworkConnectionError) {
	ServerFinish(error.ToString());
}

@RPC
function CancelReason(info : String)
{
	popupText = info;
}

@RPC
function SpectatePlease()
{

	state = ServerState.Racing;
	//Genereate Spectator Cam
		SpectatorCam = new GameObject();
		SpectatorCam.name = "SpectatorCam";
		SpectatorCam.AddComponent(Camera);
		SpectatorCam.AddComponent(AudioListener);
		SpectatorCam.AddComponent(GUILayer);
		SpectatorCam.AddComponent(FlareLayer);
		SpectatorCam.AddComponent(SpectatorCamera);
		SpectatorCam.AddComponent(Kart_Camera);
		
		SpectatorCam.GetComponent(Kart_Camera).smoothTime = 3f;
		SpectatorCam.GetComponent(Kart_Camera).Height = 3f;
		
		SpectatorCam.tag = "MainCamera";
		DontDestroyOnLoad(SpectatorCam);
	//SpectatorCam.AddComponent();Add DynamiCam
		
}

function ServerFinish(info : String)
{
	if(popupText == "")
		popupText = info;
	
	transform.GetComponent(Client_Script).enabled = false;
	transform.GetComponent(Host_Script).enabled = false;
	
	if(Network.isServer)
	{
		finalPlayers = new DisplayName[0];
		currentSelection = 0;
	}
	
	serverScroll = Vector2.zero;
}