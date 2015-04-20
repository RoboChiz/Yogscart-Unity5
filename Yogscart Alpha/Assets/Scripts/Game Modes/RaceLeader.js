#pragma strict

/*
Race Leader
V1.0
Handles the loading and running of races in Single Player & Host.
*/

//Used to load specific content depending on the current game type
enum RaceStyle{GrandPrix,TimeTrial,CustomRace,Online};
var type : RaceStyle;

//Local
var Racers : Racer[];

//Online
var NetworkRacers : NetworkedRacer[];
var cutsceneWait : boolean;
var ending : boolean;

var OverallTimer : Timer;

//Create Libaries
private var gd : CurrentGameData;
private var im : InputManager;
private var td : TrackData;
private var sm : Sound_Manager;
private var ss : SortingScript;
private var km : KartMaker;

function Start()
{
	LoadLibaries();
}

function LoadLibaries () {

	//Load Libaries
	gd = transform.GetComponent(CurrentGameData);
	im = transform.GetComponent(InputManager);
	td = GameObject.Find("Track Manager").GetComponent(TrackData);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 
	ss = transform.GetComponent(SortingScript);
	km = transform.GetComponent(KartMaker);
	
}

function StartRace () {

	cutsceneWait = false;
	ending = false;
	SetFinished(false);
	
	//Setup Racer if networked
	if(type == RaceStyle.Online)
	{
		Racers = new Racer[NetworkRacers.Length];
		
		/* Classes are passed through reference. Therefore
		we can set Racers to the same data in memory as the racer
		component in NetworkRacer. From now on you can refer to 
		Racers instead of NetworkRacer.racer*/
		
		for(var i : int = 0; i < Racers.Length; i++)
		{
			Racers[i] = NetworkRacers[i];
		}
		
	}

	if(Racers == null || Racers.Length == 0)
		Debug.Log("Missing Racers! Something's gone horribly wrong!");
	else
	{
		if(type == RaceStyle.Online)
		{
			
			SendPositionUpdates();
			
			for(i = 0; i < NetworkRacers.Length;i++)
			{
				if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
				GetComponent.<NetworkView>().RPC("SpawnMyKart",NetworkRacers[i].networkplayer);
				else
				transform.GetComponent(RaceBase).SpawnMyKart();
			}
			
			GetComponent.<NetworkView>().RPC("PlayCutscene",RPCMode.All);
			
			while(WaitForFinished())
				yield WaitForSeconds(1);
			
			SetFinished(false);
			cutsceneWait = true;
			
			yield WaitForSeconds(3);
			
			GetComponent.<NetworkView>().RPC("Countdown",RPCMode.All);
			
			yield WaitForSeconds(3.3f);
			
			GetComponent.<NetworkView>().RPC("UnlockKart",RPCMode.All);	

		}
		else
		{ //GrandPrix, Custom Race & Time Trial
			
			//PORT LATER
			
		}
		
		//During Race
		
		StartCoroutine("BeginTick");
		
		while(WaitForFinished())
			{
				ss.CalculatePositions(Racers); //Racers refers to the same Racers as the Network Racer Array.
				
				if(Network.isServer)
				SendPositionUpdates();
				
				yield WaitForSeconds(0.5);
			}
			
		StopTick();
		
		EndRace();

	}
}

function SendPositionUpdates()
{
	for(var i : int = 0; i < NetworkRacers.Length; i++)
	{
		if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
			GetComponent.<NetworkView>().RPC("SetPosition",NetworkRacers[i].networkplayer,Racers[i].position);	
		else
			transform.GetComponent(RaceBase).SetPosition(Racers[i].position);
	}
}

function EndRace()
{
	Debug.Log("The Race has ended!");
	if(type == RaceStyle.Online)
	{
		//Send Timer
		GetComponent.<NetworkView>().RPC("RaceEnded",RPCMode.AllBuffered);
		
		GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,5);
		yield WaitForSeconds(5);	
			
		GetComponent.<NetworkView>().RPC("LoadNetworkLevel",RPCMode.AllBuffered,"Lobby",0);
		
		this.enabled = false;
		
	}
}

function starttoendRace()
{

	GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,30);
	yield WaitForSeconds(30);
	SetFinished(true);
		
}

function BeginTick(){
var foo1 = OverallTimer.Minute;
var foo2 = OverallTimer.Second;
var foo3 = OverallTimer.milliSecond;

while(true){

foo3 += Time.deltaTime * 1000f;

if(foo3 >= 1000){
foo3 -= 1000;
foo2 += 1;
}

if(foo2 >= 60){
foo2 -= 60;
foo1 += 1;
}

OverallTimer.Minute = foo1;
OverallTimer.Second = foo2;
OverallTimer.milliSecond = foo3;

yield;

}
}

function StopTick(){
StopCoroutine("BeginTick");
}

function OnPlayerDisconnected(player: NetworkPlayer) {

	for(var i : int = 0; i < NetworkRacers.Length; i++)
	{
		if(NetworkRacers[i].networkplayer.guid == player.guid)
			NetworkRacers[i].connected = false;
	}

}

function LocalPositionUpdate(toChange : int, total : int, next : float)
{
	Racers[toChange].TotalDistance = total;
	Racers[toChange].NextDistance = next;
}

@RPC
function PositionUpdate(toChange : int, total : int, next : float, info : NetworkMessageInfo)
{
	if(toChange == -1 || NetworkRacers[toChange].networkplayer.guid != info.sender.guid)
		{
			Debug.Log("Cheating detected! " + info.sender.externalIP + " is changing someone else's position.");
			GetComponent.<NetworkView>().RPC("DisconnectMessage",info.sender,"You have been kicked for Cheating!");
			Network.CloseConnection(info.sender,true);
		}
		else
		{
			Racers[toChange].TotalDistance = total;
			Racers[toChange].NextDistance = next;
		}
}

function LocalFinish(toChange : int)
{
	Racers[toChange].finished = true;
	
	if(cutsceneWait && !ending)
	{
			ending = true;
			Debug.Log("Starting coutndowner");
			starttoendRace();
	}
		
}

@RPC
function Finished(toChange : int, info : NetworkMessageInfo)
{

	if(toChange == -1 || NetworkRacers[toChange].networkplayer.guid != info.sender.guid)
	{
		Debug.Log("Cheating detected! " + info.sender.externalIP + " is finishing for someone else.");
		GetComponent.<NetworkView>().RPC("DisconnectMessage",info.sender,"You have been kicked for Cheating!");
		Network.CloseConnection(info.sender,true);
	}
	else
	{
		Racers[toChange].finished = true;
		
		if(cutsceneWait && !ending)
		{
			ending = true;
			Debug.Log("Starting coutndowner");
			starttoendRace();
		}
	}

}

function SetFinished(val : boolean)
{
	for(var i : int = 0; i < Racers.Length; i++)
	{
		Racers[i].finished = val;
	}
}

function WaitForFinished() : boolean
{

	var returnVal : boolean = false;

	for(var i : int = 0; i < Racers.Length; i++)
	{
		if(!Racers[i].finished)
		{
		returnVal = true;
		break;
		}
	}
	
	return returnVal;

}