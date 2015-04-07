#pragma strict

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

class GameMode
{

	var name : String;
	var teamGame : boolean;

	var logo32 : Texture2D;
	var logo : Texture2D;
	
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

	GUI.skin = Resources.Load("Font/Menu",GUISkin);
	
	var fontSize = ((Screen.height + Screen.width)/2f)/40f;
	GUI.skin.label.fontSize = fontSize;

	var chunkSize = Screen.width/10f;
	
	if(popupText != null && popupText != "")
		state = ServerState.Popup;
	
	var nameList : Texture2D = Resources.Load("UI/Lobby/NamesList",Texture2D);
	var nameListRect : Rect = Rect(chunkSize,chunkSize/2f,chunkSize*3f,Screen.height - chunkSize*1.5f);
	var gapSize = chunkSize/2f;
	
	switch(state)
	{
	
	case ServerState.ServerList:
	
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
				
			if(Input.GetMouseButtonDown(0) && MouseIntersects(Rect(chunkSize + gapSize - 10,chunkSize/2f + gapSize + fontSize*2.2f + startHeight,nameListRect.width,fontSize*1.25f)))
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
	
	transform.GetComponent(Host_Script).Reset();
	
	Network.InitializeServer(25,hostPort,true);
	state = ServerState.Lobby;
	transform.GetComponent(Host_Script).enabled = true;
	
	var test = new NetworkMessageInfo();
	//test.sender = GetComponent.<NetworkView>().owner;
	
	transform.GetComponent(Host_Script).RecievedNewRacer(PlayerPrefs.GetString("playerName","Player"),0,0,0,0,test);//Add support for Character Select
	
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
}

function OnFailedToConnect(error: NetworkConnectionError) {
	ServerFinish(error.ToString());
}

@RPC
function CancelReason(info : String)
{
	popupText = info;
}

function ServerFinish(info : String)
{
	if(popupText == "")
		popupText = info;
	
	transform.GetComponent(Client_Script).enabled = false;
	transform.GetComponent(Host_Script).enabled = false;
	
	finalPlayers = new DisplayName[0];
}

function MouseIntersects(Area : Rect){
	if(Input.mousePosition.x >= Area.x && Input.mousePosition.x <= Area.x + Area.width 
	&&  Screen.height-Input.mousePosition.y >= Area.y &&  Screen.height-Input.mousePosition.y <= Area.y + Area.height)
		return true;
	else
		return false;
}