#pragma strict
/*
Race Base
V1.0
Holds functions used by the Client, and Host during Races.
*/

import System.Collections.Generic; //Cause lazy
import System.Linq; //Cause lazy

//Used in multiplayer to give server updates
var networkID : int = -1;
var myRacer : Racer;

//Used to load and show the correct GUI
enum GUIState{Blank,CutScene,RaceInfo,Countdown,RaceGUI,Finish,ScoreBoard,NextMenu,Win};
var currentGUI : GUIState = GUIState.Blank;
private var guiAlpha : float = 255;
private var fading : boolean;
private var scrollTime : float = 0.5f;

//Countdown Variables
private var CountdownText : int;
private var CountdownRect : Rect;
private var CountdownShow : boolean;
private var CountdownAlpha : float;

//Create Libaries
private var gd : CurrentGameData;
private var im : InputManager;
private var td : TrackData;
private var sm : Sound_Manager;
private var rl : RaceLeader;
private var ss : SortingScript;

@HideInInspector
var lb : LeaderBoard;

//Used for Single Player
var currentSelection : int = 0;

var paused : int = -1;
var selectedColor : Color = Color.yellow;

private var sentTransform : boolean = false;

private var secondCount : float;
private var pointCount : float;

var bestPosition : int;

var numberofRaces : int;

function Start()
{
	LoadLibaries();
	
	myRacer = transform.GetComponent(Client_Script).myRacer;
}

@RPC
function LoadLibaries () {

	//Load Libaries
	gd = transform.GetComponent(CurrentGameData);
	im = transform.GetComponent(InputManager);
	
	if(GameObject.Find("Track Manager") != null)
		td = GameObject.Find("Track Manager").GetComponent(TrackData);
		
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 
	rl = transform.GetComponent(RaceLeader);
	ss = transform.GetComponent(SortingScript);
	lb = transform.GetComponent(LeaderBoard);
	
	sentTransform = false;
	secondCount = 0;
	pointCount = 0;
}

@RPC
function ShowLevelSelect()
{

	gameObject.AddComponent(Level_Select);
	gameObject.AddComponent(VotingScreen);
	
	transform.GetComponent(VotingScreen).hidden = true;
	transform.GetComponent(Level_Select).hidden = false;
}

@RPC
function RaceEnded()
{

Network.RemoveRPCs(transform.GetComponent.<NetworkView>().owner);

var kartHolder = myRacer.ingameObj;

ChangeState(GUIState.Blank);

while(Application.loadedLevelName != "Lobby")
	yield;

//Don't know if I need this or not		
//var nRacer = new Racer(true,-1,myRacer.character,myRacer.hat,myRacer.kart,myRacer.wheel,0);
//myRacer = nRacer;	
	
networkID = -1;	

this.enabled = false;

}

@RPC
function EndClient()
{
	//StopCoroutine("SendUpdates");
	
	Debug.Log("Ahahaha");
	
	WrapUp();
	
	if(myRacer != null && myRacer.ingameObj.GetComponent(NewAI) == null)
	{
		WrapUpRacer();
	}
	
}

@RPC
function YourID(id : int)
{
	networkID = id;
}

@RPC
function SetPosition(pos : int)
{
	myRacer.position = pos;
}

@RPC
function SetKartPos(id : NetworkViewID, pos : int)
{
	
	var targetKart : Transform = NetworkView.Find(id).transform;
	
	if(targetKart != null)
	{
	
		targetKart.GetComponent(Position_Finding).position = pos;
	
		if(id.isMine)
			myRacer.position = pos;
	
	}

}

function Update()
{
	if(currentGUI == GUIState.RaceGUI)
	{
		if(paused != -1) {
			if(!Network.isClient && !Network.isServer)
				Time.timeScale = 0;
		} else {
			Time.timeScale = 1;
		}
		
		for(var cm : int = 0; cm < im.c.Length; cm++)
		{
			if(im.c[cm].GetMenuInput("Pause"))
			{	
				
				if(paused == -1){
				
					paused = cm;
					
					var inputs = GameObject.FindObjectsOfType(kartInput);
					
					for(var i : int = 0; i < inputs.Length; i++)
						inputs[i].enabled = false;
					
				} else if(paused == cm)
					UnPause();
				
			}
		}
	}
	
	if(Network.isClient)
		myRacer = transform.GetComponent(Client_Script).myRacer;

	if(myRacer.ingameObj != null)
	{
		var pf : Position_Finding = myRacer.ingameObj.GetComponent(Position_Finding);
		if(pf.Lap >= td.Laps && !myRacer.finished)
		{
			myRacer.finished = true;
			WrapUp();
		}
		
		if(!sentTransform && Network.isClient)
		{
			transform.GetComponent.<NetworkView>().RPC("myIngame",RPCMode.Server,myRacer.ingameObj.GetComponent(NetworkView).viewID,networkID);	
		}
			
	}
	
}

function UnPause()
{
	paused = -1;
	
	var inputs = GameObject.FindObjectsOfType(kartInput);
	
	for(var i : int = 0; i < inputs.Length; i++)
		inputs[i].enabled = true;
		
	currentSelection = 0;
}

function WrapUp()
{
	if(currentGUI <= GUIState.RaceGUI)
	{

		currentGUI = GUIState.Finish;	
		
		if(myRacer != null && myRacer.ingameObj != null)
			myRacer.ingameObj.GetComponent(kartInfo).Finish();
		
		yield WaitForSeconds(1.5f);
		
		ChangeState(GUIState.ScoreBoard);
		
		lb.enabled = true;
		if(rl.type == RaceStyle.TimeTrial)
			lb.StartTimeTrial();
		else if(rl.type == RaceStyle.Online)
			lb.StartOnline();
		else
			lb.StartLeaderBoard();

	}
}

function WrapUpRacer()
{
	myRacer.ingameObj.gameObject.AddComponent(NewAI);
	Destroy(myRacer.ingameObj.GetComponent(kartInput));
	myRacer.ingameObj.GetComponent(kartInfo).hidden = true;
	myRacer.ingameObj.GetComponent(kartItem).locked = true;
	
	myRacer.cameras.GetChild(0).GetComponent.<Camera>().enabled = false;
	myRacer.cameras.GetChild(1).GetComponent.<Camera>().enabled = true;
	
	yield WaitForSeconds(2);

	if(myRacer != null)
	{
		while(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).angle < 180){
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).angle += Time.deltaTime * 30;
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).height = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).height,1,Time.fixedDeltaTime);
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).playerHeight = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).playerHeight,1,Time.fixedDeltaTime);
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount,-1.9,Time.fixedDeltaTime);
		yield;
		}
	}
}

function SendUpdates()
{	

	CalculateSendUpdate();
	
	while(currentGUI != GUIState.Countdown)
	{
		yield;
	}

	while(!myRacer.finished)
	{	
		CalculateSendUpdate();	
		yield WaitForSeconds(0.5f);
	}
	
	if(Network.isClient)
		transform.GetComponent.<NetworkView>().RPC("Finished",RPCMode.Server,networkID);		
	else if(Network.isServer)
		rl.LocalFinish(networkID);
		
	WrapUp();
	WrapUpRacer();
	
}

function CalculateSendUpdate()
{
	if(myRacer.ingameObj != null)
	{
		var pf : Position_Finding = myRacer.ingameObj.GetComponent(Position_Finding);
		
		myRacer.TotalDistance = pf.currentTotal;
		myRacer.NextDistance = pf.currentDistance;
		pf.position = myRacer.position;
			
		if(Network.isClient)
			transform.GetComponent.<NetworkView>().RPC("PositionUpdate",RPCMode.Server,networkID,myRacer.TotalDistance,myRacer.NextDistance);
		else
			rl.LocalPositionUpdate(networkID,myRacer.TotalDistance,myRacer.NextDistance);
	}
}

@RPC
function UnlockKart()
{
	
	if(myRacer.ingameObj)
	{
		myRacer.ingameObj.GetComponent(kartInput).camLocked = false;
		myRacer.ingameObj.GetComponent(kartItem).locked = false;
	}
	
	var racers = GameObject.FindObjectsOfType(kartScript);
	for(var i : int = 0; i < racers.Length; i++)
	{
		racers[i].locked = false;
	}
	
	if(!Network.isServer && !Network.isClient)
	{
		var itemracers = GameObject.FindObjectsOfType(kartItem);
		for(i = 0; i < itemracers.Length; i++)
		{
			itemracers[i].GetComponent(kartItem).locked = false;
		}
		
		var kiracers = GameObject.FindObjectsOfType(kartInput);
		for(i = 0; i < kiracers.Length; i++)
		{
			kiracers[i].GetComponent(kartInput).camLocked = false;
		}
	
	}
	
}

@RPC
function Countdown(){

	Debug.Log("Start the countdown!");

	if(networkID != -1)
	{
		if(Network.isClient || Network.isServer)
			myRacer.ingameObj.GetComponent(kartInfo).hidden = false;
	}
	else
	{
		var kiracers = GameObject.FindObjectsOfType(kartInfo);
		for(var j : int = 0; j < kiracers.Length; j++)
		{
			kiracers[j].GetComponent(kartInfo).hidden = false;
		}
	}


	ChangeState(GUIState.Countdown);
	
	yield WaitForSeconds(0.5f);
	
	sm.PlaySFX(Resources.Load("Music & Sounds/CountDown",AudioClip));
	
	for(var i : int = 3; i >= 0; i--){
		CountdownText = i;
		
		if(networkID != -1 || (!Network.isServer && !Network.isClient))
			setStartBoost(i);
			
		CountdownRect = Rect(Screen.width/2 - (Screen.height/1.5f)/2f,Screen.height/2 - (Screen.height/1.5f)/2f,Screen.height/1.5f,Screen.height/1.5f);
		CountdownShow = true;
		yield WaitForSeconds(0.8);
		CountdownShow = false;
		yield WaitForSeconds(0.3);
	}

	CountdownText = -1;
	
	yield WaitForSeconds(0.5f);
	
	if(networkID != -1)
		setStartBoost(-1);

	ChangeState(GUIState.RaceGUI);

}

@RPC
function PlayCutscene()
{

	LoadLibaries();
	
	StartCoroutine("SendUpdates");

	ChangeState(GUIState.CutScene);
	var CutsceneCam = new GameObject();
	CutsceneCam.AddComponent(Camera);
	CutsceneCam.tag = "MainCamera";
	
	sm.PlayMusic(Resources.Load("Music & Sounds/RaceStart",AudioClip));
	
	CutsceneCam.transform.position = td.IntroPans[0].StartPoint;
	CutsceneCam.transform.rotation = Quaternion.Euler(td.IntroPans[0].StartRotation);
	
	gd.BlackOut = false;
	yield WaitForSeconds(0.5);
	
	for(var i : int = 0; i < td.IntroPans.Length; i++){
		yield Play(CutsceneCam.transform,td.IntroPans[i]);
	}
	 
	ChangeState(GUIState.RaceInfo);
	 
	gd.BlackOut = true;
	yield WaitForSeconds(0.5);
	//Spawn Player Cam
	CutsceneCam.GetComponent.<Camera>().depth = -5;
	
	yield WaitForSeconds(0.5);
	gd.BlackOut = false;
	
	Destroy(CutsceneCam);
	sm.PlayMusic(td.backgroundMusic);
	
	if(networkID != -1 || (!Network.isClient && !Network.isServer))
	{
		if(Network.isClient)
			transform.GetComponent.<NetworkView>().RPC("Finished",RPCMode.Server,networkID);
		else if(Network.isServer)
			rl.LocalFinish(networkID);
		else
			rl.SetFinished(true);//Single Player Check
	}
	else
	{
		transform.GetComponent(Network_Manager).SpectatePlease();
	}

}

function Play (cam : Transform,Clip : CameraPoint) {

	var startTime = Time.realtimeSinceStartup;

	while((Time.realtimeSinceStartup-startTime) < Clip.TravelTime){
		cam.position = Vector3.Lerp(Clip.StartPoint,Clip.EndPoint,(Time.realtimeSinceStartup-startTime)/Clip.TravelTime);
		cam.rotation = Quaternion.Slerp(Quaternion.Euler(Clip.StartRotation),Quaternion.Euler(Clip.EndRotation),(Time.realtimeSinceStartup-startTime)/Clip.TravelTime);
		yield;
	}

}

function ChangeState(nState : GUIState)
{

	if(currentGUI != nState)
	{

		fading = true;

		var startTime = Time.realtimeSinceStartup;

		while(Time.realtimeSinceStartup-startTime  < scrollTime){
		guiAlpha = Mathf.Lerp(1,0,(Time.realtimeSinceStartup-startTime)/scrollTime);
		yield;
		}

		guiAlpha = 0;
		startTime = Time.realtimeSinceStartup;
		currentGUI = nState;

		while(Time.realtimeSinceStartup-startTime  < scrollTime){
		guiAlpha = Mathf.Lerp(0,1,(Time.realtimeSinceStartup-startTime)/scrollTime);
		yield;
		}

		guiAlpha = 1;

		fading = false;

	}
}


function OnGUI ()
{
	GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);
	GUI.color.a = guiAlpha;
	
	switch(currentGUI)
	{
	
		case GUIState.CutScene:
		
			var idealWidth : float = Screen.width/3f;
			var previewTexture : Texture2D = gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].Preview;
			var previewRatio : float = idealWidth/previewTexture.width;
			var previewRect : Rect = Rect(Screen.width - idealWidth - 20,Screen.height - (previewTexture.height*previewRatio*2f),idealWidth,previewTexture.height*previewRatio);

			GUI.DrawTexture(previewRect,previewTexture);
			
		break;
		case GUIState.RaceInfo:
			
			var raceTexture : Texture2D;
			
			if(Network.isClient || Network.isServer)
			{
				raceTexture = Resources.Load("UI Textures/Level Selection/Online",Texture2D);	
			}
			else
			{
				if(rl.type == RaceStyle.TimeTrial)
					raceTexture = Resources.Load("UI Textures/Level Selection/TimeTrial",Texture2D);
				else
					raceTexture = Resources.Load("UI Textures/Level Selection/" + rl.race,Texture2D);
			}

			GUI.DrawTexture(Rect(10,10,Screen.width-20,Screen.height),raceTexture,ScaleMode.ScaleToFit);

		
		break;
		case GUIState.Countdown:
		
			var texture : Texture2D;
			GUI.color.a = CountdownAlpha;

			if(CountdownText == 0)
				texture = Resources.Load("UI Textures/CountDown/GO",Texture2D);
			else if(CountdownText != -1)
				texture = Resources.Load("UI Textures/CountDown/" + CountdownText.ToString(),Texture2D);

			if(texture != null)
				GUI.DrawTexture(CountdownRect,texture,ScaleMode.ScaleToFit);

			CountdownRect.x = Mathf.Lerp(CountdownRect.x,Screen.width/2 - Screen.height/6f,Time.deltaTime);
			CountdownRect.y = Mathf.Lerp(CountdownRect.y,Screen.height/2 - Screen.height/6f,Time.deltaTime);
			CountdownRect.width = Mathf.Lerp(CountdownRect.width,Screen.height/3f,Time.deltaTime);
			CountdownRect.height = Mathf.Lerp(CountdownRect.height,Screen.height/3f,Time.deltaTime);

			if(CountdownShow)
				CountdownAlpha = Mathf.Lerp(CountdownAlpha,256,Time.deltaTime*10f);
			else
				CountdownAlpha = Mathf.Lerp(CountdownAlpha,0,Time.deltaTime*10f);

		break;
		case GUIState.RaceGUI:			
		
					if(paused != - 1)
					{
						var pauseTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/Backing",Texture2D);
						
						var pauseWidth : float = Screen.width/2f;
						var pauseHeight : float = Screen.height/1.5f;
						var box = Rect(Screen.width/2f - pauseWidth/2f,Screen.height/2f - pauseHeight/2f,pauseWidth,pauseHeight);
						GUI.DrawTexture(box,pauseTexture);
					
						var Options : String[];
						Options = ["Resume","Quit"];

						var vertPause : int = im.c[paused].GetMenuInput("Vertical");
						var submitPause: boolean = (im.c[paused].GetMenuInput("Submit") != 0);
						
						if(vertPause != 0)
							currentSelection -= Mathf.Sign(vertPause);
							
						if(currentSelection < 0)
							currentSelection = Options.Length - 1;
							
						if(currentSelection >= Options.Length)
							currentSelection = 0;

						GUI.BeginGroup(box);
		
						//If the current menu has options.
						if(Options != null && Options.Length > 0)
						{
							//Single Player is the longest word in the menu and is 13 characters long
							var fontSize = (box.width / 15f);
							var holder = GUI.skin.label.fontSize;
							GUI.skin.label.fontSize = fontSize;
							
							for(var i : int = 0; i < Options.Length; i++)
							{	
								if(currentSelection == i)
									GUI.skin.label.normal.textColor = selectedColor;
								else
									GUI.skin.label.normal.textColor = Color.white;
									
									
								var labelRect = Rect(10,10 + (box.height/4f) +(i * (10+fontSize)),box.width - 20,fontSize);
								GUI.Label(labelRect,Options[i]);

								labelRect.x += box.x;
								labelRect.y += box.y;
								
								if(im.MouseIntersects(labelRect))
								{
										currentSelection = i;
										
										if(im.GetClick())
										{
											submitPause = true;
										}
										
								}
								
							}
							
							GUI.skin.label.normal.textColor = Color.white;
							GUI.skin.label.fontSize = holder;
						}
						
					GUI.EndGroup();		
						
					if(submitPause)
					{						
						switch(Options[currentSelection])
						{
							case "Resume":
								UnPause();
							break;
							case "Quit":
								UnPause();
								gd.Exit();
							break;
						}
					}
								
					}
					
		break;
		case GUIState.Finish:

		break;
		case GUIState.ScoreBoard:
		
				
				/*
					var BestTimer = gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].BestTrackTime; 		
					var OverallTimer = bestTimer;
					
					if(BestTimer.BiggerThan(OverallTimer)){
					GUI.Label(Rect(10,10,BoardRect.width,BoardRect.height),"New Best Time!!!");
					}
					else
					{
					GUI.Label(Rect(10,10,BoardRect.width,BoardRect.height),"You Lost!!!");
					}

					GUI.Label(Rect(10,10 + (optionSize),BoardRect.width,optionSize),"Best Time");
					GUI.Label(Rect(10,10 + 2*(optionSize),BoardRect.width,optionSize),BestTimer.ToString());

					GUI.Label(Rect(10,10 + 3*(optionSize),BoardRect.width,optionSize),"Your Time");
					GUI.Label(Rect(10,10 + 4*(optionSize),BoardRect.width,optionSize),OverallTimer.ToString());

				}*/
				
			if(!Network.isServer && !Network.isClient)
			{
				if(im.c[0].GetMenuInput("Submit") || im.GetClick())
				{
					if(lb.state != LBType.Points || numberofRaces == 1)
					{
						ChangeState(GUIState.NextMenu);
						lb.hidden = true;
					}
					else
					{
						lb.SecondStep();	
					}			
				}
			}

		break;
		case GUIState.NextMenu:
			
			if(lb.Racers.Count > 0)
			{
				lb.ResetRacers();
				lb.enabled = false;
			}
			
			var BoardTexture = Resources.Load("UI Textures/GrandPrix Positions/Backing",Texture2D);
			var BoardRect = Rect(Screen.width/2f - Screen.height/16f,Screen.height/16f,Screen.width/2f ,(Screen.height/16f)*14f);

			GUI.DrawTexture(BoardRect,BoardTexture);

			
			if(rl.type == RaceStyle.GrandPrix || rl.type == RaceStyle.CustomRace){
				if(numberofRaces + 1 <= 4)
					Options = ["Next Race","Quit"];
				else
					Options = ["Finish"];
			}
			else
				Options = ["Restart","Quit"];

			var IdealHeight : float = Screen.height/8f;
			var ratio = IdealHeight/100f;

			var vert : int = im.c[0].GetMenuInput("Vertical");
			
			if(vert != 0)
				currentSelection -= Mathf.Sign(vert);
				
			if(currentSelection < 0)
				currentSelection = Options.Length - 1;
				
			if(currentSelection >= Options.Length)
				currentSelection = 0;
				
			var mouseSelecting : boolean = false;

			for(var k : int = 0; k < Options.Length; k++){

				var optionTexture : Texture2D = Resources.Load("UI Textures/Next Menu/" + Options[k],Texture2D);
				var optionTextureSel : Texture2D = Resources.Load("UI Textures/Next Menu/" + Options[k] + "_Sel",Texture2D);
				var optionRect : Rect = Rect(BoardRect.x + BoardRect.width/2f - ((300f*ratio)/2f),BoardRect.y + (IdealHeight*(k+1)),(300f*ratio),IdealHeight);

				if(currentSelection == k)
					GUI.DrawTexture(optionRect,optionTextureSel,ScaleMode.ScaleToFit);
				else
					GUI.DrawTexture(optionRect,optionTexture,ScaleMode.ScaleToFit);

				if(im.MouseIntersects(optionRect))
				{
					currentSelection = k;
					mouseSelecting = true;
				}
			}
			
			var submitBool : boolean = (im.c[0].GetMenuInput("Submit") != 0);

			if(submitBool || (mouseSelecting && im.GetClick()))
			{
			
				ChangeState(GUIState.Blank);
				
				switch(Options[currentSelection])
				{
					case "Quit":
						gd.Exit();
					break;
					case "Next Race":
						gd.currentTrack ++;
						rl.race ++;
						rl.spStartRace();
					break;
					case "Restart":
						rl.spStartRace();
					break;
					case "Replay":

					break;
					case "Finish":
						//DetermineWinner();
						ChangeState(GUIState.Win);
					break;
				}
			}
			
		break;
		case GUIState.Win:
		
			BoardTexture = Resources.Load("UI Textures/GrandPrix Positions/Backing",Texture2D);
			BoardRect = Rect(Screen.width/2f - Screen.height/16f,Screen.height/16f,Screen.width/2f ,(Screen.height/16f)*14f);

			GUI.DrawTexture(BoardRect,BoardTexture);
			
			GUI.BeginGroup(BoardRect);
			
			var lineSize = GUI.skin.label.fontSize + 5;
			
			GUI.Label(Rect(10,lineSize,BoardRect.width,lineSize),"Congratulations!");
			
			var positionString : String = "You came ";
			positionString += (bestPosition+1).ToString();
			
			if(bestPosition == 0)
				positionString += "st!";
			else if(bestPosition == 1)
				positionString += "nd!";
			else if(bestPosition == 2)
				positionString += "rd!";
			else
				positionString += "th!";
			
			GUI.Label(Rect(10,lineSize * 2,BoardRect.width,lineSize),positionString);
			GUI.Label(Rect(10,lineSize * 3,BoardRect.width,lineSize),"This menu is under Construction!");
			
			GUI.EndGroup();
			
			submitBool = (im.c[0].GetMenuInput("Submit") != 0);
			
			if(submitBool)
			{
				gd.Exit();
			}
		
		break;	
	}
}	

/*function DetermineWinner()
{

	 bestRacer = -1;

	for(var i : int = 0; i < rl.Racers.Length;i++)
	{
		if(rl.Racers[i].human)
		{
			if(bestRacer == -1 || rl.Racers[i].points > rl.Racers[bestRacer].points)
				bestRacer = i;
		}
	}
	
	var bestPlayer = rl.Racers[bestRacer];
	
	if(bestPlayer == 60){	
		if(gd.Difficulty == 0)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[50cc]","Perfect");
		if(gd.Difficulty == 1)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[100cc]","Perfect");
		if(gd.Difficulty == 2)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[150cc]","Perfect");
		if(gd.Difficulty == 3)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[Insane]","Perfect");
	}else if(gd.Tournaments[gd.currentCup].LastRank[gd.Difficulty] != "Perfect"){	
		if(gd.Difficulty == 0)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[50cc]","Gold");
		if(gd.Difficulty == 1)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[100cc]","Gold");
		if(gd.Difficulty == 2)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[150cc]","Gold");
		if(gd.Difficulty == 3)
			PlayerPrefs.SetString(gd.Tournaments[gd.currentCup].Name+"[Insane]","Gold");
	}	
	
	PlayerPrefs.SetFloat("NewCharacter?",1);

	ChangeState(GUIState.Win);
	
}*/

function setStartBoost(val : int){
	
	var racers = GameObject.FindObjectsOfType(kartScript);
	for(var i : int = 0; i < racers.Length; i++)
	{
		racers[i].startBoostVal = val;
	}
	
}
	
function StartGame()
{
	//Don't do anything XD
	LoadLibaries();
	
	myRacer.character = gd.currentChoices[0].character;
	myRacer.hat = gd.currentChoices[0].hat;
	myRacer.kart = gd.currentChoices[0].kart;
	myRacer.wheel = gd.currentChoices[0].wheel;
	
	Random.seed = Time.realtimeSinceStartup;
	
}	
	
@RPC
function SetupGDTracks(cup : int, track : int)
{

	gd.currentCup = cup;
	gd.currentTrack = track;

}	
	