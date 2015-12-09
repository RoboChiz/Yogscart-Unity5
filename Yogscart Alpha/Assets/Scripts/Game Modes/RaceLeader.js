#pragma strict
/*
Race Leader
V1.0
Handles the loading and running of races in Single Player & Host.
*/

import System.Collections.Generic; //Cause lazy
import System.Linq; //Cause lazy

//Used to load specific content depending on the current game type
enum RaceStyle{GrandPrix,TimeTrial,CustomRace,Online,DailyChallenge};
var type : RaceStyle;

//Local
var Racers : Racer[];

//Online
var NetworkRacers : NetworkedRacer[];
var cutsceneWait : boolean;
var ending : boolean;

var startTimer : float = -1;
var raceTimer : float;
//Single Player Variables
var race : int = 1; //Holds which race in a grand prix you are

var inRace : boolean = false;

var waitFinished : boolean = true;

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
	
	if(transform.GetComponent(Network_Manager).Automatic)
	{
		gameObject.AddComponent(VotingScreen);
	}
	
	StartCoroutine("LevelSelectCountdown");
	
}

function StartSinglePlayer()//true if in Time Trial Mode
{
	
	LoadLibaries();
	
	race = 1;
	
	if(type != RaceStyle.TimeTrial && type != RaceStyle.DailyChallenge) //Setup a Grand Prix or VS Race
	{
		Racers = new Racer[12];

		var diff : int = gd.difficulty;
		var minVal : int;
		var maxVal : int;
		
		switch(diff)
		{
			case 0:
				maxVal = 10;
				minVal = 7;
			break;
			case 1:
				maxVal = 8;
				minVal = 4;
			break;
			case 2:
				maxVal = 5;
				minVal = 0;
			break;
			case 3:
				maxVal = 3;
				minVal = 0;
			break;
		}
		
		var counter : int = minVal;
		var humanCount : int = im.c.Length - 1;
		
		var charactersShuffle : Array = new Array();
		
		if(gd.Characters.Length <= 12 - im.c.Length)
		{
			for(var i : int = 0; i < 12 - im.c.Length; i++)
			{
				charactersShuffle.Push(i%gd.Characters.Length);
			}
		}
		else
		{
			for(i = 0; i < gd.Characters.Length; i++)
				charactersShuffle.Push(i);
		}
		
		charactersShuffle = ShuffleArray(charactersShuffle);
		
		var availableHats = new Array();
		for(i = 0; i < gd.Hats.length; i++)
		{
			if(gd.Hats[i].Unlocked)
			{
				availableHats.Add(i);
			}
		}
		
		for(i = 0; i < 12; i++)
		{
			if(i < 12 - im.c.Length)
			{		
				
				Racers[i] = new Racer(false,counter,charactersShuffle[i],availableHats[Random.Range(0,availableHats.length)],Random.Range(0,gd.Karts.Length),Random.Range(0,gd.Wheels.Length),i);
				
				counter++;
				if(counter > maxVal)
					counter = minVal;
			}
			else
			{
				Racers[i] = new Racer(true,humanCount,gd.currentChoices[humanCount].character,gd.currentChoices[humanCount].hat,gd.currentChoices[humanCount].kart,gd.currentChoices[humanCount].wheel,i);
				humanCount--;
			}
		}	
		
	}
	else
	{
		Racers = new Racer[1];
		Racers[0] = new Racer(true,0,gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel,-1);
		Random.seed = 01264646231;
	}
	
	spStartRace();
	
}

function ShuffleArray(arr : Array)
{
	var i1 : int;
	var i2 : int;
	
	for(var i : int = 0; i < arr.length*2; i++)
	{
		 i1 = Random.Range(0, arr.length);
         i2 = Random.Range(0, arr.length);
         
         var holder : int = arr[i1];
         arr[i1] = arr[i2];
         arr[i2] = holder;
	}

	return arr;

}

function spStartRace()
{

	raceTimer = 0;

	if(type == RaceStyle.CustomRace && race > 1)
	{
		transform.GetComponent(Level_Select).enabled = true;
		transform.GetComponent(Level_Select).hidden = false;
		
		while(transform.GetComponent(Level_Select).enabled)
			yield;
	}

	gd.BlackOut = true;
	yield WaitForSeconds(0.5);
	
	if(type != RaceStyle.DailyChallenge)
		Application.LoadLevel(gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].SceneID);
	else
		Application.LoadLevel(CurrentGameData.ChallengeScene);
		
	yield;
	yield;
	
	while(GameObject.Find("Track Manager") == null)
	{
		yield;
	}
	
	if(type == RaceStyle.TimeTrial || type == RaceStyle.DailyChallenge)
	{
		var itemCrates = GameObject.FindObjectsOfType(Item_Box);
		
		for(var i : int = 0; i < itemCrates.Length;i++)
		{
			Destroy(itemCrates[i].gameObject);
		}
	}

	LoadLibaries();
	
	yield;
	yield;
	
	StartRace();
	
}

function StartRace () 
{

	cutsceneWait = false;
	ending = false;
	SetFinished(false);
	
	rb.numberofRaces = race;
	waitFinished = true;
	
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
			
			if(NetworkRacers[i].networkplayer == GetComponent.<NetworkView>().owner)
			{
				transform.GetComponent(RaceBase).myRacer = Racers[i];
				transform.GetComponent(Client_Script).myRacer = Racers[i];
			}
			
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
				
				//2 represents KartType.Online
			
				if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
					GetComponent.<NetworkView>().RPC("SpawnMyKart",NetworkRacers[i].networkplayer,2,SpawnPosition,rot * Quaternion.Euler(0,-90,0));
				else
					transform.GetComponent(Client_Script).SpawnMyKart(2,SpawnPosition,rot * Quaternion.Euler(0,-90,0));
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
				
				if(type == RaceStyle.TimeTrial || type == RaceStyle.DailyChallenge)
				{
					//Find Spawn Position
					rot = td.spawnPoint.rotation;
					racepos = Racers[i].position;
					
					startPos = td.spawnPoint.position + (rot*Vector3.forward*(3*1.5f)*-1.5f);
					x2 = rot*(Vector3.forward*4.5f)+(Vector3.forward*.75f*3);
					y2 = rot*(Vector3.right*6);
					SpawnPosition = startPos + x2 + y2;  
				}
				else
				{
					//Find Spawn Position
					rot = td.spawnPoint.rotation;
					racepos = Racers[i].position;
					
					startPos = td.spawnPoint.position + (rot*Vector3.forward*(3*1.5f)*-1.5f);
					x2 = rot*(Vector3.forward*(racepos%3)*(3*1.5f)+(Vector3.forward*.75f*3));
					y2 = rot*(Vector3.right*(racepos + 1)* 3);
					SpawnPosition = startPos + x2 + y2;  
				}
			
				Racers[i].ingameObj = km.SpawnKart(KartType.Local,SpawnPosition,rot * Quaternion.Euler(0,-90,0),Racers[i].kart,Racers[i].wheel,Racers[i].character,Racers[i].hat);
			
				switch(gd.difficulty)
				{
					case 0:
					Racers[i].ingameObj.GetComponent(kartScript).maxSpeed = 18;
					break;
					case 1:
					Racers[i].ingameObj.GetComponent(kartScript).maxSpeed = 20;
					break;
					case 2:
					Racers[i].ingameObj.GetComponent(kartScript).maxSpeed = 22;
					break;
					case 3:
					Racers[i].ingameObj.GetComponent(kartScript).maxSpeed = 22;
					break;
				
				}
			
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

					IngameCam.GetChild(0).transform.GetComponent(Kart_Camera).target = Racers[i].ingameObj;
					IngameCam.GetChild(1).transform.GetComponent(Kart_Camera).target = Racers[i].ingameObj;
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
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.TopRight;
						if(Racers[i].aiStupidity == 2)		
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.BottomLeft;
						if(Racers[i].aiStupidity == 3)
							Racers[i].ingameObj.GetComponent(kartInfo).screenPos = ScreenType.BottomRight;
					}
					
					Destroy(Racers[i].ingameObj.GetComponent(kartUpdate));		
					
					if(type == RaceStyle.TimeTrial || type == RaceStyle.DailyChallenge)
							Racers[i].ingameObj.GetComponent(kartItem).RecieveItem(2);
							
				}
				else
				{
					Destroy(Racers[i].ingameObj.GetComponent(kartInput));
					Destroy(Racers[i].ingameObj.GetComponent(kartInfo));
					Racers[i].ingameObj.gameObject.AddComponent(NewAI);
					Racers[i].ingameObj.GetComponent(NewAI).Stupidity = Racers[i].aiStupidity;
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
		
		startTimer = Time.time;
		inRace = true;
		
		//During Race
		
		while(waitFinished && raceTimer < 3600) //If race has been going on elss than an hour
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
			
			yield WaitForSeconds(0.25);
		}
	}
}

function SendPositionUpdates()
{
	for(var i : int = 0; i < NetworkRacers.Length; i++)
	{
		/*if(NetworkRacers[i].networkplayer.guid != GetComponent.<NetworkView>().owner.guid)
		{
			if(NetworkRacers[i].connected)
				GetComponent.<NetworkView>().RPC("SetPosition",NetworkRacers[i].networkplayer,Racers[i].position);	
		}
		else
			transform.GetComponent(RaceBase).SetPosition(Racers[i].position);*/	
			
			
		if(Racers[i].ingameObj != null)
		{
			GetComponent.<NetworkView>().RPC("SetKartPos",RPCMode.All,Racers[i].ingameObj.GetComponent.<NetworkView>().viewID,Racers[i].position);	
		}
		
	}
}

@RPC
function myIngame(id : NetworkViewID, toChange : int, info : NetworkMessageInfo)
{
	if(NetworkRacers != null && toChange < NetworkRacers.Length )
	{
		if(toChange == -1 || NetworkRacers[toChange].networkplayer.guid != info.sender.guid)
		{
			Debug.Log("Cheating detected! " + info.sender.externalIP + " is sending myIngame for someone else.");
			GetComponent.<NetworkView>().RPC("CancelReason",info.sender,"You have been kicked for Cheating!");
			Network.CloseConnection(info.sender,true);
		}
		else
		{
		
			Racers[toChange].ingameObj = NetworkView.Find(id).transform;
		
		}
	}
}

function Update()
{
	if(startTimer != -1)
	{
		raceTimer = Time.time - startTimer;
		waitFinished = WaitForFinished();
		
		if(!waitFinished || raceTimer >= 3600f)
		{			
			startTimer = -1;				
			EndGame();
		}
	}
}

function FixedUpdate()
{

	if(rb.currentGUI == GUIState.RaceGUI && !Network.isServer && !Network.isClient)
		for(var i : int = 0; i < Racers.Length; i++)
		{
			if(Racers[i].ingameObj.GetComponent(Position_Finding).Lap >= td.Laps && !Racers[i].finished)
			{
				Racers[i].finished = true;

				Racers[i].timer = raceTimer;
				
				if(!Network.isClient && !Network.isServer && Racers[i].human)
				{
					FinishLocalRacer(i);
				}	
			}
		}
}

function LocalSendPositionUpdates()
{
	for(var i : int = 0; i < Racers.Length; i++)
	{
		Racers[i].ingameObj.GetComponent(Position_Finding).position = Racers[i].position;	
	}
}

function FinishLocalRacer(i : int)
{

	Debug.Log("Player " + i + " has finished");
	
	Racers[i].ingameObj.GetComponent(kartInfo).Finish();
	
	Racers[i].ingameObj.gameObject.AddComponent(NewAI);
	Destroy(Racers[i].ingameObj.GetComponent(kartInput));
	Racers[i].ingameObj.GetComponent(kartInfo).hidden = true;
		
	Racers[i].ingameObj.GetComponent(kartItem).locked = true;
	
	Racers[i].cameras.GetChild(0).GetComponent.<Camera>().enabled = false;
	Racers[i].cameras.GetChild(1).GetComponent.<Camera>().enabled = true;

	yield WaitForSeconds(2);

	while(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).angle < 180){
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).angle += Time.fixedDeltaTime * 30;
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).height = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).height,1,Time.fixedDeltaTime);
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).playerHeight = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).playerHeight,1,Time.fixedDeltaTime);
	Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount = Mathf.Lerp(Racers[i].cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount,-1.9,Time.fixedDeltaTime);
	yield;
	}
	
}

function EndGame()
{
	Debug.Log("The Race has ended!");
	rb.numberofRaces = race;

	if(type == RaceStyle.Online)
	{
		//Send Timer
		StopCoroutine("starttoendRace");
		
		var allFinished : boolean = true;
		
		for(var g : int = 0; g < Racers.Length; g++)
		{		
			if(!Racers[g].finished)
				allFinished = false;
		}
		
		if(!allFinished)
		{
			Debug.Log("Not everyone has finished");
			GetComponent.<NetworkView>().RPC("EndClient",RPCMode.AllBuffered);
		}
		
		GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,10);
		yield WaitForSeconds(10);	
			
		GetComponent.<NetworkView>().RPC("RaceEnded",RPCMode.AllBuffered);
		transform.GetComponent(Network_Manager).StartCoroutine("EndGame");
		
		NetworkRacers = new NetworkedRacer[0];
		inRace = false;
		
		this.enabled = false;
		
	}
	else if(type == RaceStyle.TimeTrial)
	{
	
		//Do Timer Stuff I guess
		var BestTimer = gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].BestTrackTime; 		
		var racerTime = Racers[0].timer;		
		
		if(racerTime < BestTimer || BestTimer <= 0f)
		{
			PlayerPrefs.SetFloat(gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].Name,racerTime);
			gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].BestTrackTime = racerTime;
		}
		
		rb.lb.AddLBRacer(0, Racers[0].character, Racers[0].points, Racers[0].timer);
				
		rb.WrapUp();
	}
	else if(type == RaceStyle.DailyChallenge) //Daily Challenge
	{
		rb.lb.AddLBRacer(0, Racers[0].character, Racers[0].points, Racers[0].timer);
		
		//Tell Server About Time
		var dcm : DailyChallengeMenu = gameObject.AddComponent(DailyChallengeMenu);
		dcm.scoring = true;
		dcm.SetupClient();
		
		var timerString : String = TimeManager.TimerToString(Racers[0].timer);
		dcm.SetScore(PlayerPrefs.GetString("playerName","Player"),timerString);
		
		Debug.Log("Before:" + Racers[0].timer + " After:" + TimeManager.Parse(timerString));
		
		rb.WrapUp();
	}
	else
	{
		Debug.Log("Finished the Race");
		
		var holderList : DisplayRacer[] = new DisplayRacer[Racers.Length];
		
		for(var i : int = 0; i < Racers.Length;i++)
		{
			Racers[i].points += 15 - Racers[i].position;
			if(Racers[i].human)
				holderList[Racers[i].position] = new DisplayRacer(Racers[i].position,Racers.Length-1-i, Racers[i].character, Racers[i].points, Racers[i].timer);
			else
				holderList[Racers[i].position] = new DisplayRacer(Racers[i].position,-1, Racers[i].character, Racers[i].points, Racers[i].timer);
		}
		
		rb.lb.Racers = holderList.ToList();
		
		var holder = SortingScript.SortRacersPoints(Racers);
		for(i = 0; i < holder.Count; i++)
		{
			holder[i].overallPosition = i;
		}
		
		rb.WrapUp();
		
		if(race == 4 && type == RaceStyle.GrandPrix)
		{
			var bestHuman : int = -1;
			//Determine the Winner and stuff
			if(Racers != null)
			{
				for(i = 0; i < Racers.Length; i++)
				{
					if(Racers[i].human && (bestHuman == -1 || (Racers[i].points > Racers[bestHuman].points)))
					{
						bestHuman = i;
					}
				}
				
				rb.bestPosition = Racers[bestHuman].overallPosition;
				
				var rankString : String = gd.Tournaments[gd.currentCup].LastRank[gd.difficulty];
				
				if(Racers[bestHuman].points == 60)
					rankString = "Perfect";
				else if(Racers[bestHuman].overallPosition == 0)
				{
					if(rankString != "Perfect")
						rankString = "Gold";
				}
				else if(Racers[bestHuman].overallPosition == 1)
				{
					if(rankString != "Perfect" && rankString != "Gold")
						rankString = "Silver";
				}
				else if(Racers[bestHuman].overallPosition == 2)
					if(rankString != "Perfect" && rankString != "Gold" && rankString != "Silver")
						rankString = "Bronze";
				else
				{
					if(rankString != "Perfect" && rankString != "Gold" && rankString != "Silver" && rankString != "Bronze")
						rankString = "No Rank";
				}
				
				switch(gd.difficulty)
				{
					case 0:
						PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[50cc]",rankString);
					break;
					case 1:
						PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[100cc]",rankString);
					break;
					case 2:
						PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[150cc]",rankString);
					break;
					case 3:
						PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[Insane]",rankString);
					break;
				}
			}	
		}
		
	}
	
	inRace = false;
	
}	

function starttoendRace()
{
	if(waitFinished)
	{
		GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,30);
		yield WaitForSeconds(30);
		SetFinished(true);
	}	
}

function LevelSelectCountdown()
{
	GetComponent.<NetworkView>().RPC("Countdowner",RPCMode.All,30);
	yield WaitForSeconds(30);
	DetermineLevel();
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
				racers[j].gameObject.AddComponent(NewAI);
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
	
	if(inRace)
	{
		GetComponent.<NetworkView>().RPC("AddLBRacer",RPCMode.All,NetworkRacers[toChange].name,NetworkRacers[toChange].character,NetworkRacers[toChange].points,NetworkRacers[toChange].timer);
		
		var uniquePosition : boolean = false;
			
		while(!uniquePosition)
		{
		
			uniquePosition = true;
		
			for(var i : int = 0; i < Racers.Length; i++)
			{
				if(i != toChange && Racers[i].position == Racers[toChange].position)
				{
					Racers[toChange].position++;
					uniquePosition = false;
					continue;
				}
			}
		}		
	}
	
	if(cutsceneWait && !ending)
	{
			ending = true;
			Debug.Log("Starting coutndowner");
			StartCoroutine("starttoendRace");
	}
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
		
		if(inRace)
		{
			GetComponent.<NetworkView>().RPC("AddLBRacer",RPCMode.All,NetworkRacers[toChange].name,NetworkRacers[toChange].character,NetworkRacers[toChange].points,NetworkRacers[toChange].timer);
			
			var uniquePosition : boolean = false;
			
			while(!uniquePosition)
			{
			
				uniquePosition = true;
			
				for(var i : int = 0; i < Racers.Length; i++)
				{
					if(i != toChange && Racers[i].position == Racers[toChange].position)
					{
						Racers[toChange].position++;
						uniquePosition = false;
						continue;
					}
				}
			}			
		}
		
		if(cutsceneWait && !ending)
		{
			ending = true;
			Debug.Log("Starting coutndowner");
			StartCoroutine("starttoendRace");
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
