#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;

var WithBots : boolean;
var Automatic : boolean;
var conscious : boolean = true;
var MinPlayers : int;
var MaxGamemodePlayers : int = 12;

var RacingPlayers : NetworkedRacer[];
var PotentialPlayers : NetworkPlayer[];
var WaitingPlayers : NetworkPlayer[];

var serverType : ServerState = ServerState.Lobby;

var workingProcesses : int;

function Awake()
{
	gd = transform.GetComponent(CurrentGameData);
	im = transform.GetComponent(InputManager);
}

function Reset()
{

	RacingPlayers = new NetworkedRacer[0];
	PotentialPlayers = new NetworkPlayer[0];
	WaitingPlayers = new NetworkPlayer[0];
	
	serverType = ServerState.Lobby;
	
	workingProcesses = 0;

}

function OnPlayerConnected(player: NetworkPlayer)
{
	var copy = new Array();
	
	if(WaitingPlayers != null)
		copy = WaitingPlayers;
		
	copy.Push(player);
	
	WaitingPlayers = copy;
	
}

@RPC
function VersionUpdate(verText : String,info : NetworkMessageInfo)
{
	if(verText != gd.version)
	{
		GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"Wrong Version! Please download the correct version!");
		Network.CloseConnection(info.sender,true);
	}
	else
	{
		if((RacingPlayers != null && PotentialPlayers != null && RacingPlayers.Length + PotentialPlayers.Length < MaxGamemodePlayers) || 
		(RacingPlayers != null && PotentialPlayers == null && RacingPlayers.Length < MaxGamemodePlayers) ||
		(RacingPlayers == null && PotentialPlayers != null && PotentialPlayers.Length < MaxGamemodePlayers))
		{
			//If a Player slot is available add them to the game
			GetComponent.<NetworkView>().RPC("QuizNewRacer",info.sender);
			
			var copy = new Array();
	
			if(PotentialPlayers != null)
				copy = PotentialPlayers;
				
			copy.Add(info.sender);
			PotentialPlayers = copy;
			
			copy = WaitingPlayers;
			copy.Remove(info.sender);
			WaitingPlayers = copy;
			
		}
	}
}

function OnPlayerDisconnected(player: NetworkPlayer) {

		workingProcesses ++;

		Debug.Log("Clean up after player " +  player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
		
		var copy = new Array();

		if(RacingPlayers != null)
		{
		
		for(var i : int = 0; i < RacingPlayers.Length; i++)
		{
		if(RacingPlayers[i].networkplayer == player)
		{
		RacingPlayers[i].connected = false;
		break;
		}
		}
		
		if(serverType == ServerState.Lobby)
		CheckforLeavers();

		}
		
		if(PotentialPlayers != null)
		{
		copy = PotentialPlayers;
		copy.Remove(player);
		PotentialPlayers = copy;
		}		
		
		if(WaitingPlayers != null)
		{
		copy = WaitingPlayers;
		copy.Remove(player);
		WaitingPlayers = copy;
		}
		
		workingProcesses --;
}

function FixedUpdate () {

switch (serverType)
{
	case ServerState.Lobby:
			
			
	var cancelInput : float = im.c[0].GetMenuInput("Cancel");
	var cancelBool = (cancelInput != 0);

	if(cancelBool)
	{
	Network.Disconnect();
	}


	break;

}
}

function CheckforLeavers()
{

workingProcesses ++;

var copy = new Array();

for(var i : int = 0; i < RacingPlayers.Length; i++)
{
if(RacingPlayers[i].connected)
copy.Push(RacingPlayers[i]);
}

RacingPlayers = copy;

workingProcesses --;

}

@RPC
function RecievedNewRacer(name : String, character : int, hat : int, kart : int, wheel : int,info : NetworkMessageInfo)
{

workingProcesses ++;

if(serverType == ServerState.Lobby)
{

	var testVal : NetworkPlayer;

	if(info.sender != null)
	{
		testVal = info.sender;

		for(var i : int = 0; i < PotentialPlayers.Length; i++)
		{
			if(PotentialPlayers[i] == testVal)
			{
				//Update Potential Player
				var copy = new Array();

				if(PotentialPlayers != null)
				copy = PotentialPlayers;

				copy.RemoveAt(i);

				PotentialPlayers = copy;

				break;
			}
		}
	}
}
else
{
	testVal = GetComponent.<NetworkView>().owner;
}

AddRacer(name,character,hat,kart,wheel,testVal);

workingProcesses --;

}

function AddRacer(name : String, character : int, hat : int, kart : int, wheel : int, np : NetworkPlayer)
{
//Update Racing Player
var copy = new Array();

if(RacingPlayers != null)
copy = RacingPlayers;

var nRacer = new Racer(true,-1,character,hat,kart,wheel,copy.length); 
var nNetworkRacer = new NetworkedRacer(nRacer,np);

copy.Push(nNetworkRacer);

RacingPlayers = copy;

GetComponent.<NetworkView>().group = RacingPlayers.Length;

GetComponent.<NetworkView>().RPC("NewPlayer",RPCMode.AllBuffered,name,np);

GetComponent.<NetworkView>().group = 0;

}

function OnGUI(){

	var playerString : String = "Total Players : ";
	playerString += (Network.connections.Length+1);

	if(RacingPlayers != null)
		playerString += " Racing Players : " + RacingPlayers.Length;

	if(PotentialPlayers != null)
		playerString += " Potential Players: " + PotentialPlayers.Length;

	if(WaitingPlayers != null)
		playerString += " Waiting Players: " + WaitingPlayers.Length;

	GUI.Label(Rect(10,10,Screen.width-20,25),playerString);

	for(var i : int = 0; i < RacingPlayers.Length; i++)
	{
		//if(RacingPlayers[i].Human == true)
		GUI.Label(Rect(10,10 + 25 + (25*i),250,25),"[CLIENT]");
		//else
		//GUI.Label(Rect(10,10 + 25 + (25*i),250,25),"[BOT]");
	}

}


	
	