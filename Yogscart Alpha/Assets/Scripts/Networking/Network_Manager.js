
#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;

enum ServerState {ServerList,Connecting,Popup,Lobby,LoadingRace,LoadingLevel,Racing};
var state : ServerState;

var popupText : String;

var maxServerCount : int = 10;

var editServer : boolean;
var servers : ServerInfo[];
private var serverScroll : Vector2 = Vector2.zero;

private var checkingServer : boolean;

private var hostPort : int = 25000;
private var playerName : String;

var finalPlayers : DisplayName[];
private var sendingPing : boolean;

var currentSelection : int;

@HideInInspector
var SpectatorCam : GameObject;

private var connectRot : float;

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
	var character : int;
	var ping : int;
	var networkPlayer : NetworkPlayer;
	
	function DisplayName(n : String, c : int, np : NetworkPlayer)
	{
	name = n;
	networkPlayer = np;
	character = c;
	}
	
	function DisplayName(n : String,c : int)
	{
		name = n;
		character = c;
	}
	
	function DisplayName()
	{
		name = "";
		character = -1;
	}
}

@RPC
function NewPlayer(name : String, character : int)
{
	
	var found : boolean;
	
	var nDisplayName : DisplayName = new DisplayName(name,character);

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
	sm = transform.GetChild(0).GetComponent(Sound_Manager);
	
	playerName = PlayerPrefs.GetString("playerName","Player");
	
	LoadServers();
	
}

function Start()
{
	sm.PlayMusic(Resources.Load("Music & Sounds/YC Main Theme",AudioClip));
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
	
	checkingServer = false;

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
	
	if(im.c == null || im.c.Length == 0)
	{
	
		GUI.Label(Rect(Screen.width/2f - Screen.width/4f,Screen.height/2f - 200,Screen.width/2f,400),"Press Start / Enter on your controller of choice");
	
	}
	else
	{
	
		var xboxController : boolean = im.c[0].inputName != "Key_";
	
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
			
		if(currentSelection >= Mathf.Clamp(servers.Length,0,maxServerCount))
			currentSelection = 0;
		
		GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*1.5f,chunkSize*4,chunkSize/2f),"Port: ");
		int.TryParse(GUI.TextField(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.75f + gapSize*1.5f,chunkSize*4,chunkSize/4f),hostPort.ToString()),hostPort);
		
		GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*3.5f,chunkSize*4,chunkSize/2f),"Name (Temporary): ");
		playerName = GUI.TextField(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *3.75f + gapSize*1.5f,chunkSize*4,chunkSize/4f),playerName);
		
		GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*4.5f,chunkSize*4,chunkSize/2f),"Master Volume: ");
		sm.MasterVolume = GUI.HorizontalSlider(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*5f,chunkSize*4,chunkSize/2f),sm.MasterVolume,0,100);
		
		GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*5.5f,chunkSize*4,chunkSize/2f),"Music Volume: ");
		sm.MusicVolume = GUI.HorizontalSlider(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*6f,chunkSize*4,chunkSize/2f),sm.MusicVolume,0,100);
		
		GUI.Label(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*6.5f,chunkSize*4,chunkSize/2f),"SFX Volume: ");
		sm.SFXVolume = GUI.HorizontalSlider(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *2.5f + gapSize*7f,chunkSize*4,chunkSize/2f),sm.SFXVolume,0,100);

		var hostText : String = "Host Server";

		if(xboxController)
				{
					hostText = "Host Server   X";
				}
				
		if(GUI.Button(Rect(10 + Screen.width - chunkSize * 6f,chunkSize *3f + gapSize*1.5f,chunkSize*4,chunkSize/2f),hostText) || (xboxController && im.c[0].GetMenuInput("X")!= 0))
			{
				StartCoroutine("StartServer");
			}
			
		if(xboxController)
				GUI.DrawTexture(Rect(10 + Screen.width - chunkSize * 3.5f,chunkSize*3f + gapSize*1.7f,fontSize*1.5f,fontSize*1.5f),Resources.Load("UI/Main Menu/X",Texture2D));
		
		GUI.DrawTexture(nameListRect,nameList);
		
		nameListRect = Rect(chunkSize + gapSize - 10,chunkSize/2f + gapSize,chunkSize*3f - (gapSize*1.5f),Screen.height - chunkSize*1.5f - (gapSize*2f));
		
		GUI.BeginGroup(nameListRect);
		
		GUI.Label(Rect(10,0,nameListRect.width,fontSize*2f),"Servers");
		
		GUI.DrawTexture(Rect(10,fontSize*1.5f,nameListRect.width,fontSize*0.2f),Resources.Load("UI/Lobby/Line",Texture2D));
		
		serverScroll = GUI.BeginScrollView(Rect(0,fontSize*2.2f,nameListRect.width,nameListRect.height - 20 - fontSize*2.2f),serverScroll,Rect(0,0,nameListRect.width - 20,(Mathf.Clamp(servers.Length,10,50)+1) * fontSize*1.25f));
		
		if(servers != null && servers.Length > 0)
			for(var i : int; i < Mathf.Clamp(servers.Length,0,maxServerCount);i++)
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
			GUI.skin.button.fontSize = fontSize;
			GUI.skin.textArea.fontSize = fontSize;
			GUI.skin.textField.fontSize = fontSize;
			
			GUI.EndGroup();
			
			if(!editServer)
			{
				
				var joinText : String = "Join";
				
				if(xboxController)
				{
					joinText = "Join   A";
				}
				
				if(GUI.Button(Rect(10 + Screen.width - chunkSize * 6f,chunkSize*2f + gapSize*1.5f,chunkSize,chunkSize/2f),joinText) || (xboxController && im.c[0].GetMenuInput("Submit")!= 0))
				{
					ConnectToServer();
				}
				
				if(xboxController)
					GUI.DrawTexture(Rect(10 + Screen.width - chunkSize * 5.4f,chunkSize*2f + gapSize*1.7f,fontSize*1.5f,fontSize*1.5f),Resources.Load("UI/Main Menu/A",Texture2D));
				
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
				if(GUI.Button(Rect(10 + Screen.width - chunkSize * 5f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),"Cancel"))
					{
						editServer = false;
						LoadServers();
					}
			}
			
		}
		
		if(!editServer && servers.Length < maxServerCount)
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

					if(finalPlayers[i].networkPlayer != null && Network.isServer)
					{
						finalPlayers[i].ping = Network.GetAveragePing(finalPlayers[i].networkPlayer);		
						
						if(!sendingPing)
							SendPing();	
					}
						
					textSize = Mathf.Clamp(fontSize/2f,0,fontSize);
					GUI.skin.label.fontSize = textSize;
						
					GUI.Label(Rect(nameListRect.width - (textSize*5f),startHeight + textSize*0.5f,textSize*10f,fontSize*1.5f),finalPlayers[i].ping.ToString() + " Ping");

					GUI.skin.label.fontSize = fontSize;
					
				}
			
			GUI.EndGroup();
			
			var quitText : String = "Quit";
			if(xboxController)
				quitText = "Quit   B";
			
			if(GUI.Button(Rect(chunkSize ,nameListRect.x + nameListRect.height,chunkSize,chunkSize/2f),quitText) || im.c[0].GetMenuInput("Cancel") != 0)
				{
					Network.Disconnect();	
				}
				
			if(xboxController)
				GUI.DrawTexture(Rect(chunkSize * 1.6f,nameListRect.x + nameListRect.height + gapSize*0.2f,fontSize*1.5f,fontSize*1.5f),Resources.Load("UI/Main Menu/B",Texture2D));
				
			}
			
		if(Network.isServer)
		{	
		
			var startText : String = "Start";
			if(xboxController)
				startText = "Start   A";
		
			if(GUI.Button(Rect(chunkSize * 2f,nameListRect.x + nameListRect.height,chunkSize,chunkSize/2f),startText) || (im.c[0].GetMenuInput("Submit") != 0))
				{
					StartGame();
				}
			
			if(xboxController)
				GUI.DrawTexture(Rect(chunkSize * 2.65f,nameListRect.x + nameListRect.height + gapSize*0.2f,fontSize*1.5f,fontSize*1.5f),Resources.Load("UI/Main Menu/A",Texture2D));
				
				
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
			
			var okayText : String = "Okay";
			
			if(xboxController)
				okayText = "Okay   A";
			
			if(GUI.Button(Rect(Screen.width/2f - chunkSize/2f,chunkSize *2f + gapSize*1.5f,chunkSize,chunkSize/2f),okayText)  || im.c[0].GetMenuInput("Submit")!= 0)
			{
						popupText = "";
						state = ServerState.ServerList;
			}
			
			if(xboxController)
					GUI.DrawTexture(Rect(Screen.width/2f + chunkSize/8f,chunkSize *2f + gapSize*1.65f,fontSize*1.5f,fontSize*1.5f),Resources.Load("UI/Main Menu/A",Texture2D));
		
		break;
		case ServerState.Connecting:
		
			var connectionCircle : Texture2D = Resources.Load("UI Textures/Main Menu/Connecting",Texture2D);
			
			GUIUtility.RotateAroundPivot (connectRot, Vector2(Screen.width/2f,Screen.height/2f)); 
			GUI.DrawTexture(Rect(Screen.width/2f - chunkSize, Screen.height/2f - chunkSize,2f*chunkSize,2f*chunkSize),connectionCircle);
			
			connectRot += Time.deltaTime * -50f;
		
		break;
		
		}
	}
}

function SendPing()
{	

	sendingPing = true;

	for(var i : int = 0; i < finalPlayers.Length; i++)
	{
		GetComponent.<NetworkView>().RPC("GetPing",RPCMode.Others,finalPlayers[i].ping,i);
		yield WaitForSeconds(0.5f);
	}
	
	yield WaitForSeconds(3f);
	
	sendingPing = false;
	
}


function ConnectToServer()
{

	im.allowedToChange = false;
	im.RemoveOtherControllers();

	PlayerPrefs.SetString("playerName",playerName);
	
	state = ServerState.Connecting;
	Network.Connect(servers[currentSelection].ip,servers[currentSelection].port);
}

function CancelStartServer()
{
	StopCoroutine("StartServer");
	
	if(!Network.isClient)
		state = ServerState.ServerList;
	
}

function StartServer()
{

	im.allowedToChange = false;
	im.RemoveOtherControllers();

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
		
		GetComponent.<NetworkView>().RPC("StartGamemode",RPCMode.AllBuffered,currentSelection);
		
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
	
	hs.CheckforLeavers();
	
	GetComponent.<NetworkView>().RPC("ClearNames",RPCMode.All);
	
	yield WaitForSeconds(0.5f);
	
	for(var i : int = 0; i < hs.RacingPlayers.Length; i++)
	{
		GetComponent.<NetworkView>().RPC("NewPlayer",RPCMode.AllBuffered,hs.RacingPlayers[i].name,hs.RacingPlayers[i].character);
	}
	
}

@RPC
function ClearNames()
{
	finalPlayers = new DisplayName[0];
}

@RPC
function GetPing(ping : int, toChange : int)
{
	finalPlayers[toChange].ping = ping;
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
	state = ServerState.Connecting;
		
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
	
	while(popupText != "")
		yield;
		
	im.allowedToChange = true;
}