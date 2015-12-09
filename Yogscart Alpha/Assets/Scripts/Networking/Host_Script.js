#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var nm : Network_Manager;

var WithBots : boolean;
var Automatic : boolean;
var conscious : boolean = true;
var MinPlayers : int;
var MaxGamemodePlayers : int = 12;

var RacingPlayers : NetworkedRacer[];
var PotentialPlayers : NetworkPlayer[];
var WaitingPlayers : NetworkPlayer[];

var serverType : ServerState;

var workingProcesses : int;

function Awake()
{
	gd = transform.GetComponent(CurrentGameData);
	im = transform.GetComponent(InputManager);
	nm = transform.GetComponent(Network_Manager);
}

function Reset()
{

	RacingPlayers = new NetworkedRacer[0];
	PotentialPlayers = new NetworkPlayer[0];
	WaitingPlayers = new NetworkPlayer[0];
	
	workingProcesses = 0;

}

function OnPlayerConnected(player: NetworkPlayer)
{

	
}

@RPC
function VersionUpdate(verText : String, info : NetworkMessageInfo)
{

	Debug.Log("Version Update!");

	if(verText != gd.version)
	{
		GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"Wrong Version! Please download the correct version!");
		Network.CloseConnection(info.sender,true);
	}
	else
	{
	
		var copy = new Array();
	
		if(WaitingPlayers != null)
			copy = WaitingPlayers;
		
		copy.Push(info.sender);
		
		WaitingPlayers = copy;
	
		if(serverType != ServerState.Lobby)
		{
			Debug.Log("But we're already racing...");
			GetComponent.<NetworkView>().RPC("SpectatePlease",info.sender);
		}
	}
}

function OnPlayerDisconnected(player: NetworkPlayer) {

		workingProcesses ++;

		Debug.Log("Clean up after player " +  player);
		Network.RemoveRPCs(player);
		
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

function Update()
{
	serverType = transform.GetComponent(Network_Manager).state;
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
	
	if((RacingPlayers != null && PotentialPlayers != null && RacingPlayers.Length + PotentialPlayers.Length < MaxGamemodePlayers) || 
	(RacingPlayers != null && PotentialPlayers == null && RacingPlayers.Length < MaxGamemodePlayers) ||
	(RacingPlayers == null && PotentialPlayers != null && PotentialPlayers.Length < MaxGamemodePlayers))
	{
	
		var playersAsked = RacingPlayers.Length + PotentialPlayers.Length;
		
		for(var i : int; i < (12 - playersAsked); i++)
		{
		
			if(WaitingPlayers != null && WaitingPlayers.Length > i)
			{
				//If a Player slot is available add them to the game
				GetComponent.<NetworkView>().RPC("QuizNewRacer",WaitingPlayers[i]);
				
				var copy = new Array();

				if(PotentialPlayers != null)
					copy = PotentialPlayers;
					
				copy.Add(WaitingPlayers[i]);
				PotentialPlayers = copy;
				
				copy = WaitingPlayers;
				copy.RemoveAt(i);
				WaitingPlayers = copy;
				
			}
		}
		
	}

	break;
	default:
	
	
		if(PotentialPlayers != null && PotentialPlayers.Length > 0)
		{	
			var copy2 = new Array();
			
			if(WaitingPlayers != null)
				copy2 = WaitingPlayers;
				
			if(PotentialPlayers != null)
				for(var k : int; k < PotentialPlayers.Length; k++)
				{
					copy2.Add(PotentialPlayers[k]);
				}
				
			WaitingPlayers = copy2;
			PotentialPlayers = new NetworkPlayer[0];
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

DoLobbyStuff();

workingProcesses --;

}

@RPC
function RecievedNewRacer(name : String, character : int, hat : int, kart : int, wheel : int,info : NetworkMessageInfo)
{

	if(serverType == ServerState.Lobby)
	{

		workingProcesses ++;

		if(character == -1 || hat == -1 || kart == -1 || wheel == -1 || character > gd.Characters.Length || hat > gd.Hats.Length || kart > gd.Karts.Length || wheel > gd.Wheels.Length)
		{
			GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"You haven't selected a character!");
			Network.CloseConnection(info.sender,true);
		}

		if(serverType == ServerState.Lobby)
		{

			var testVal : NetworkPlayer;

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

		AddRacer(name,character,hat,kart,wheel,testVal);

		workingProcesses --;
		
	}
}

function AddRacer(name : String, character : int, hat : int, kart : int, wheel : int, np : NetworkPlayer)
{
	//Update Racing Player
	var copy = new Array();

	if(RacingPlayers != null)
	copy = RacingPlayers;

	var nNetworkRacer = new NetworkedRacer(character,hat,kart,wheel,copy.length,name,np);

	copy.Push(nNetworkRacer);

	RacingPlayers = copy;

	GetComponent.<NetworkView>().group = RacingPlayers.Length + 1;

	yield;

	GetComponent.<NetworkView>().group = 0;
	
	DoLobbyStuff();
	
}

function DoLobbyStuff()
{
	StopCoroutine("BalanceTeams");
	StartCoroutine("BalanceTeams");
		
	yield WaitForSeconds(0.5f);
	
	StopCoroutine("UpdateLobbyNames");
	StartCoroutine("UpdateLobbyNames");
}

/*function OnGUI(){

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

}*/

class Team
{
	var name : String;
	var members : NetworkedRacer[];
	
	function Team(teamName : String)
	{
		name = teamName;
		members = new NetworkedRacer[0];
	}
}

function UpdateLobbyNames()
{

	Debug.Log("Starting UpdateLobbyNames");

	Network.RemoveRPCsInGroup(14);
	GetComponent.<NetworkView>().group = 14;

	GetComponent.<NetworkView>().RPC("ClearNames",RPCMode.AllBuffered);
	
	yield WaitForSeconds(0.2f);
	
	for(var i : int = 0; i < RacingPlayers.Length; i++)
	{
		GetComponent.<NetworkView>().RPC("NewPlayer",RPCMode.AllBuffered,RacingPlayers[i].name,RacingPlayers[i].character,RacingPlayers[i].team-1);
		
		if(RacingPlayers[i].networkplayer == GetComponent.<NetworkView>().owner)
			nm.finalPlayersIDUpdate(i);
		else
			GetComponent.<NetworkView>().RPC("finalPlayersIDUpdate",RacingPlayers[i].networkplayer,i);	
			
		Debug.Log("Sent " + RacingPlayers[i].name);
		yield;
	}
	
	GetComponent.<NetworkView>().group = 0;
	
	yield;
	
	Debug.Log("Finished UpdateLobbyNames");
}

function BalanceTeams()
{
	
	Debug.Log("Balancing Teams");
	if(nm.currentGamemode != -1)
	{
		if(gd.onlineGameModes[nm.currentGamemode].teamGame && gd.onlineGameModes[nm.currentGamemode].teams != null)
		{
		
			var gmT = gd.onlineGameModes[nm.currentGamemode].teams;
			var teams : Team[] = new Team[gmT.Length + 1];
			
			teams[0] = new Team("No Team");
			
			for(var i : int = 0; i < gmT.Length; i++)
			{
				teams[i+1] = new Team(gmT[i]);
			}
			
			for(i = 0; i < RacingPlayers.Length; i++)
			{
			
				var j : int = RacingPlayers[i].team;
				
				var copy = new Array();
				if(teams[j].members != null)
					copy = teams[j].members;
				copy.Add(RacingPlayers[i]);
				teams[j].members = copy;		

			}
			
			//Sort Teams
			var gmtLength : float = gmT.Length;
			var division : float = Mathf.Clamp(RacingPlayers.Length,gmT.Length,12f) / gmtLength;
			var avgAmount : int = Mathf.RoundToInt(division + 0.4);
			
			for(i = 1; i < teams.Length; i++)
			{
			
				Debug.Log("I:" + i);
				var changed : boolean = false;
			
				//Check if Team has too many members
				while(teams[i].members.Length > avgAmount)
				{
					var randomPlayer = Random.Range(0,teams[i].members.Length);
					
					var holder = teams[i].members[randomPlayer];
					
					//Remove random player from Team
					copy = new Array();
					copy = teams[i].members;
					copy.RemoveAt(randomPlayer);
					teams[i].members = copy;
					
					holder.team = 0; //0 represents No team
					
					//Add them to the NoTeam Array
					copy = new Array();
					if(teams[0].members != null)
						copy = teams[0].members;
					copy.Add(holder);
					teams[0].members = copy;
					
					changed = true;
					
				}
			}
			for(i = 1; i < teams.Length; i++)
			{
				//Check if Team has enough members
				while(teams[0].members.Length > 0 && teams[i].members.Length < avgAmount)
				{
					copy = new Array();
					if(teams[i].members != null)
						copy = teams[i].members;
					copy.Add(teams[0].members[0]);
					teams[i].members = copy;
					
					teams[0].members[0].team = i;
					
					Debug.Log(gd.Characters[teams[0].members[0].character].Name + " is now in " + teams[i].name);
					
					copy = teams[0].members;
					copy.RemoveAt(0);
					teams[0].members = copy;	
				}			
			}
		}		
	}
}



	
	