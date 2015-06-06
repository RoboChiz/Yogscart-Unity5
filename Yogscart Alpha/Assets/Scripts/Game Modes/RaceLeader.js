﻿#pragma strict

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

//Single Player Variables
var race : int = 1; //Holds which race in a grand prix you are

//Used in multiplayer
private var Votes : Vector2[];

//Create Libaries
private var gd : CurrentGameData;
private var im : InputManager;
private var td : TrackData;
private var sm : Sound_Manager;
private var ss : SortingScript;
private var km : KartMaker;
private var rb : RaceBase;

function Start()
{
	LoadLibaries();
}

function LoadLibaries () {

	//Load Libaries
	gd = transform.GetComponent(CurrentGameData);
	im = transform.GetComponent(InputManager);
	
	if(GameObject.Find("Track Manager") != null)
		td = GameObject.Find("Track Manager").GetComponent(TrackData);
	
	sm = transform.gameObject.Find("Sound System").GetComponent(Sound_Manager); 
	ss = transform.GetComponent(SortingScript);
	km = transform.GetComponent(KartMaker);
	rb = transform.GetComponent(RaceBase);
	
}

function StartGame()
{
	//Load Level Select to Racing Players
	var racers : NetworkedRacer[] = transform.GetComponent(Host_Script).RacingPlayers;
	
	for(var i : int = 0; i < racers.Length; i++)
	{
	
		if(racers[i].networkplayer != GetComponent.<NetworkView>().owner)
		{
			GetComponent.<NetworkView>().RPC("ShowLevelSelect",racers[i].networkplayer);
			GetComponent.<NetworkView>().RPC("YourID",racers[i].networkplayer,i);
		}
		else
		{
			transform.GetComponent(RaceBase).ShowLevelSelect();
			transform.GetComponent(RaceBase).YourID(i);
		}
	}
	
	StartCoroutine("LevelSelectCountdown");
	
}

function StartSinglePlayer()//true if in Time Trial Mode
{
	
	LoadLibaries();
	
	if(type != RaceStyle.TimeTrial) //Setup a Grand Prix or VS Race
	{
		Racers = new Racer[12];

		var diff : int = gd.currentDifficulty;
		var minVal : int;
		var maxVal : int;
		
		switch(diff)
		{
			case 0:
				minVal = 10;
				maxVal = 7;
			break;
			case 1:
				minVal = 8;
				maxVal = 4;
			break;
			case 2:
				minVal = 5;
				maxVal = 0;
			break;
			case 3:
				minVal = 0;
				maxVal = 0;
			break;
		}
		
		var counter : int = minVal;
		
		for(var i : int = 0; i < 12; i++)
		{
			if(i < 12 - im.c.Length)
			{
				Racers[i] = new Racer(false,counter,Random.Range(0,gd.Characters.Length),Random.Range(0,gd.Hats.Length),Random.Range(0,gd.Karts.Length),Random.Range(0,gd.Wheels.Length),i);
				
				counter++;
				if(counter > maxVal)
					counter = minVal;
			}
			else
			{
				var p : int = 12 - (12-im.c.Length)-1;
				Racers[i] = new Racer(true,p,gd.currentChoices[p].character,gd.currentChoices[p].hat,gd.currentChoices[p].kart,gd.currentChoices[p].wheel,i);
			}
		}	
		
	}
	else
	{
		Racers = new Racer[1];
		Racers[0] = new Racer(true,-1,gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel,-1);
	}
	
	spStartRace();
	
}

function spStartRace()
{
	gd.BlackOut = true;
	yield WaitForSeconds(0.5);
	Application.LoadLevel(gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].SceneID);
	
	yield;
	yield;
	
	Debug.Log("Level Loaded!");
	
	while(GameObject.Find("Track Manager") == null)
	{
		yield;
	}
	
	OverallTimer = new Timer();
	LoadLibaries();
	
	yield;
	yield;
	
	StartRace();
	
}

function StartRace () 
{

	Debug.Log("Starting StartRace");

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
	
		var SpawnPosition : Vector3;
		var rot : Quaternion;
		var racepos : int;
				
		var startPos : Vector3;
		var x2 : Vector3;
		var y2 : Vector3;
	
	
		if(type == RaceStyle.Online)
		{
			
			SendPositionUpdates();
			
			for(i = 0; i < NetworkRacers.Length;i++)
			{
				
				//Find Spawn Position
				rot  = td.spawnPoint.rotation;
				racepos = NetworkRacers[i].position;
				
				startPos = td.spawnPoint.position + (rot*Vector3.forward*(3*1.5f)*-1.5f);
				x2 = rot*(Vector3.forward*(racepos%3)*(3*1.5f)+(Vector3.forward*.75f*3));
				y2 = rot*(Vector3.right*(racepos + 1)* 3);
				SpawnPosition = startPos + x2 + y2;  
			
				if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
				GetComponent.<NetworkView>().RPC("SpawnMyKart",NetworkRacers[i].networkplayer,KartType.Online,SpawnPosition,rot * Quaternion.Euler(0,-90,0));
				else
				transform.GetComponent(Client_Script).SpawnMyKart(KartType.Online,SpawnPosition,rot * Quaternion.Euler(0,-90,0));
			}
			
			GetComponent.<NetworkView>().RPC("PlayCutscene",RPCMode.All);
			
			while(WaitForFinished())
				yield WaitForSeconds(0.5f);
			
			SetFinished(false);
			cutsceneWait = true;
			
			yield WaitForSeconds(3);
			
			GetComponent.<NetworkView>().RPC("Countdown",RPCMode.All);
			
			yield WaitForSeconds(3.8f);
			
			GetComponent.<NetworkView>().RPC("UnlockKart",RPCMode.All);	

		}
		else
		{ //GrandPrix, Custom Race & Time Trial
			for(i = 0; i < Racers.Length;i++)
			{
				
				//Find Spawn Position
				rot = td.spawnPoint.rotation;
				racepos = Racers[i].position;
				
				startPos = td.spawnPoint.position + (rot*Vector3.forward*(3*1.5f)*-1.5f);
				x2 = rot*(Vector3.forward*(racepos%3)*(3*1.5f)+(Vector3.forward*.75f*3));
				y2 = rot*(Vector3.right*(racepos + 1)* 3);
				SpawnPosition = startPos + x2 + y2;  
			
				Racers[i].ingameObj = km.SpawnKart(KartType.Local,SpawnPosition,rot * Quaternion.Euler(0,-90,0),Racers[i].kart,Racers[i].wheel,Racers[i].character,Racers[i].hat);
			
				if(Racers[i].human)
				{
					//Add Camera
					var IngameCam = Instantiate(Resources.Load("Prefabs/Cameras",Transform),SpawnPosition,Quaternion.identity);
					IngameCam.name = "InGame Cams";
					
					Racers[i].ingameObj.GetComponent(kartInput).InputNum = Racers[i].aiStupidity;
					Racers[i].ingameObj.GetComponent(kartInput).camLocked = true;
					Racers[i].ingameObj.GetComponent(kartInput).frontCamera = IngameCam.GetChild(1).GetComponent.<Camera>();
					Racers[i].ingameObj.GetComponent(kartInput).backCamera = IngameCam.GetChild(0).GetComponent.<Camera>();
					
					IngameCam.GetChild(1).tag = "MainCamera";

					IngameCam.GetChild(0).transform.GetComponent(Kart_Camera).Target = Racers[i].ingameObj;
					IngameCam.GetChild(1).transform.GetComponent(Kart_Camera).Target = Racers[i].ingameObj;
					Racers[i].cameras = IngameCam;
					
					//SetUpCameras
					var copy = new Array();
					copy.Push(IngameCam.GetChild(0).GetComponent.<Camera>());
					copy.Push(IngameCam.GetChild(1).GetComponent.<Camera>());
					Racers[i].ingameObj.GetComponent(kartInfo).cameras = copy;
					
					if(im.c.Length == 2)
					{
						if(Racers[i].aiStupidity == 0)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.Top;
						if(Racers[i].aiStupidity == 1)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.Bottom;
					}
					if(im.c.Length > 2)
					{
						if(Racers[i].aiStupidity == 0)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.TopLeft;
						if(Racers[i].aiStupidity == 1)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.TopLeft;
						if(Racers[i].aiStupidity == 2)		
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.BottomLeft;
						if(Racers[i].aiStupidity == 3)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.BottomRight;
					}
					
					Destroy(Racers[i].ingameObj.GetComponent(kartUpdate));			
				}
				else
				{
					Destroy(Racers[i].ingameObj.GetComponent(kartInput));
					Destroy(Racers[i].ingameObj.GetComponent(kartInfo));
					Racers[i].ingameObj.gameObject.AddComponent(Racer_AI);
					Racers[i].ingameObj.GetComponent(Racer_AI).Difficulty = Racers[i].aiStupidity;
				}
				
				
			}
			
			rb.PlayCutscene();
			
			while(WaitForFinished())
				yield WaitForSeconds(0.5f);
			
			SetFinished(false);
			cutsceneWait = true;
			
			yield WaitForSeconds(3);
			
			rb.Countdown();
			
			yield WaitForSeconds(3.8f);
			
			rb.UnlockKart();	

			
		}
		
		//During Race
		
		StartCoroutine("BeginTick");
		
		while(WaitForFinished())
			{
			
				if(!Network.isServer)
				{
					for(var r : int = 0; r < Racers.Length; r++)
					{
						Racers[r].TotalDistance = Racers[r].ingameObj.GetComponent(Position_Finding).currentTotal;
						Racers[r].NextDistance = Racers[r].ingameObj.GetComponent(Position_Finding).currentDistance;
					}
				}
				ss.CalculatePositions(Racers); //Racers refers to the same Racers as the Network Racer Array.
				
				if(Network.isServer)
					SendPositionUpdates();
				else
					LocalSendPositionUpdates();
				
				yield WaitForSeconds(0.5);
			}
			
		StopTick();
		
		EndGame();

	}
}

function SendPositionUpdates()
{
	for(var i : int = 0; i < NetworkRacers.Length; i++)
	{
		if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
		{
			if(NetworkRacers[i].connected)
				GetComponent.<NetworkView>().RPC("SetPosition",NetworkRacers[i].networkplayer,Racers[i].position);	
		}
		else
			transform.GetComponent(RaceBase).SetPosition(Racers[i].position);
	}
}

function LocalSendPositionUpdates()
{
	for(var i : int = 0; i < Racers.Length; i++)
	{
		Racers[i].ingameObj.GetComponent(Position_Finding).position = Racers[i].position;
		
		if(Racers[i].ingameObj.GetComponent(Position_Finding).Lap >= td.Laps && !Racers[i].finished)
		{
			Racers[i].finished = true;

			Racers[i].timer.Minute = OverallTimer.Minute;
			Racers[i].timer.Second = OverallTimer.Second;
			Racers[i].timer.milliSecond = OverallTimer.milliSecond;
			
			if(Racers[i].human)
				FinishRacer(i);
				
		}
		
	}
}
function FinishRacer(i : int)
{
	Racers[i].ingameObj.gameObject.AddComponent(Racer_AI);
	Destroy(Racers[i].ingameObj.GetComponent(kartInput));
	Racers[i].ingameObj.GetComponent(kartInfo).hidden = true;
		
	Racers[i].ingameObj.GetComponent(kartItem).locked = true;
	
	Racers[i].cameras.GetChild(0).GetComponent.<Camera>().enabled = false;
	Racers[i].cameras.GetChild(1).GetComponent.<Camera>().enabled = true;

	yield WaitForSeconds(2);

	while(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).Distance > -6.5){
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).Distance -= Time.fixedDeltaTime * 10;
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).Height = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).Height,1,Time.fixedDeltaTime);
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight,1,Time.fixedDeltaTime);
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount,-1.9,Time.fixedDeltaTime);
	yield;
	}
	
}

function EndGame()
{
	Debug.Log("The Race has ended!");
	if(type == RaceStyle.Online)
	{
		//Send Timer
		
		StopCoroutine("starttoendRace");
		
		Debug.Log("Sending ENCLIENT");
		GetComponent.<NetworkView>().RPC("EndClient",RPCMode.AllBuffered);
		
		GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,10);
		yield WaitForSeconds(10);	
			
		GetComponent.<NetworkView>().RPC("RaceEnded",RPCMode.AllBuffered);
		transform.GetComponent(Network_Manager).EndGame();
		
		this.enabled = false;
		
	}
	if(type == RaceStyle.TimeTrial)
	{
	
	}
	else
	{
		Debug.Log("Finished the Race");
		
		var fc = new DisplayName[12];
		
		for(var i : int = 0; i < fc.Length;i++)
		{
			fc[i] = new DisplayName(Racers[i].name,Racers[i].character,Racers[i].timer);
		}
		
		rb.finishedCharacters = fc;
		rb.ChangeState(GUIState.ScoreBoard);
		
	}
}

function starttoendRace()
{

	GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,30);
	yield WaitForSeconds(30);
	SetFinished(true);
		
}

function LevelSelectCountdown()
{
	GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,30);
	yield WaitForSeconds(30);
	DetermineLevel();
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

function OnPlayerDisconnected(player: NetworkPlayer) 
{
	if(this.enabled)
	{
		for(var i : int = 0; i < NetworkRacers.Length; i++)
		{
			if(NetworkRacers[i].networkplayer.guid == player.guid)
			{
				NetworkRacers[i].connected = false;		
				break;
			}
		}
		
		var racers = GameObject.FindObjectsOfType(kartScript);
		for(var j : int = 0; j < racers.Length; j++)
		{
			if(racers[j].transform.GetComponent(NetworkView).owner == player)
			{
				racers[j].gameObject.AddComponent(Racer_AI);
				racers[j].gameObject.GetComponent(kartUpdate).sending = true;
				NetworkRacers[i].ingameObj = racers[j].gameObject.transform;	
				break;
			}
		}
	}
}

function OnPlayerConnected(player: NetworkPlayer)
{
	for(var i : int = 0; i < NetworkRacers.Length; i++)
	{
		if(!NetworkRacers[i].connected)
		{
			GetComponent.<NetworkView>().RPC("SpawnMe",player,NetworkRacers[i].name,NetworkRacers[i].kart,NetworkRacers[i].wheel,NetworkRacers[i].character,NetworkRacers[i].hat,NetworkRacers[i].ingameObj.GetComponent.<NetworkView>().viewID);
		}
	}
}

function LocalPositionUpdate(toChange : int, total : int, next : float)
{
	Racers[toChange].TotalDistance = total;
	Racers[toChange].NextDistance = next;
	
	for(var i : int = 0; i < NetworkRacers.Length;i++)
	{
		if(!NetworkRacers[i].connected)
		{
			Racers[i].TotalDistance = Racers[i].ingameObj.GetComponent(Position_Finding).currentTotal;
			Racers[i].NextDistance = Racers[i].ingameObj.GetComponent(Position_Finding).currentDistance;
		}
	}
}

@RPC
function PositionUpdate(toChange : int, total : int, next : float, info : NetworkMessageInfo)
{
	if(toChange == -1 || NetworkRacers[toChange].networkplayer.guid != info.sender.guid)
		{
			Debug.Log("Cheating detected! " + info.sender.externalIP + " is changing someone else's position.");
			GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"You have been kicked for Cheating!");
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
			StartCoroutine("starttoendRace");
	}
	
	ScoreboardAdd(toChange);
	
}

@RPC
function Finished(toChange : int, info : NetworkMessageInfo)
{

	if(toChange == -1 || NetworkRacers[toChange].networkplayer.guid != info.sender.guid)
	{
		Debug.Log("Cheating detected! " + info.sender.externalIP + " is finishing for someone else.");
		GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"You have been kicked for Cheating!");
		Network.CloseConnection(info.sender,true);
	}
	else
	{
		Racers[toChange].finished = true;
		
		if(cutsceneWait && !ending)
		{
			ending = true;
			Debug.Log("Starting coutndowner");
			StartCoroutine("starttoendRace");
		}
		
		ScoreboardAdd(toChange);

	}

}
function ScoreboardAdd(toChange : int)
{
		if(cutsceneWait && ending)
		{
			Debug.Log(NetworkRacers[toChange].character + " has come " + NetworkRacers[toChange].position + "th");
			GetComponent.<NetworkView>().RPC("ScoreBoardAdd",RPCMode.AllBuffered,NetworkRacers[toChange].character,NetworkRacers[toChange].name,NetworkRacers[toChange].position,NetworkRacers.Length);
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
		if(Racers[i].human)
		{
			if(!Racers[i].finished && ((!Network.isServer && !Network.isClient) || NetworkRacers[i].connected))
			{
				returnVal = true;
				break;
			}
		}
	}
	
	return returnVal;

}

@RPC
function LevelChoose(cup : int, track : int){

	var AddTo : int;
	for(var i : int = 0; i < cup; i++)
		AddTo += gd.Tournaments[i].Tracks.Length;
	
	AddTo += track;

	var copy = new Array();

	if(Votes != null)
		copy = Votes;

	copy.Push(Vector2(cup,track));
	Votes = copy;

	GetComponent.<NetworkView>().RPC ("VoteUpdate", RPCMode.All,AddTo);

	if(Votes.Length >= transform.GetComponent(Host_Script).RacingPlayers.Length){
		Debug.Log("Votes: " + Votes.Length + " RacingPlayers: " + transform.GetComponent(Host_Script).RacingPlayers.Length);
		StopCoroutine("LevelSelectCountdown");
		GetComponent.<NetworkView>().RPC ("Countdowner", RPCMode.All,5);
		DetermineLevel();
	}

}

function DetermineLevel(){
	
		var cup : int;
		var track : int;
	
		if(Votes == null || Votes.Length == 0)
		{
			cup = Random.Range(0,gd.Tournaments.Length);
			track  = Random.Range(0,gd.Tournaments[cup].Tracks.Length);
		}
		else
		{
			var toRace : int = Random.Range(0,Votes.Length);
			cup = Votes[toRace].x;
			track = Votes[toRace].y;
		}
		
		GetComponent.<NetworkView>().RPC ("StartRoll",RPCMode.All,toRace);
		yield WaitForSeconds(5.1);

		Network.RemoveRPCs(GetComponent.<NetworkView>().owner);
		
		GetComponent.<NetworkView>().RPC ("SetupGDTracks",RPCMode.AllBuffered,cup,track);
		GetComponent.<NetworkView>().RPC ("LoadNetworkLevel", RPCMode.AllBuffered, gd.Tournaments[cup].Tracks[track].SceneID,0);
		
		
		Debug.Log("Level Loaded!");
		
		while(GameObject.Find("Track Manager") == null)
			yield;
		
		NetworkRacers = transform.GetComponent(Host_Script).RacingPlayers;
		type = RaceStyle.Online;
		OverallTimer = new Timer();
		LoadLibaries();
		
		yield;
		yield;
		
		StartRace();
		
		Votes = new Vector2[0];

}

/*This function is called by the host script if the autobalance function has swapped a 
player’s team. The autobalance function should be called in StartGame function if you need 
it. The player is the record in the “RacingPlayers” array in the host script that has been 
swapped.*/
function TeamSwap(player : int)
{

}
