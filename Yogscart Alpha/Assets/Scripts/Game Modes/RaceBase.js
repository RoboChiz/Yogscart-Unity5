#pragma strict

/*
Race Base
V1.0
Holds functions used by the Client, and Host during Races.
*/

//Used in multiplayer to give server updates
var networkID : int = -1;
var myRacer : Racer;

//Used to load and show the correct GUI
enum GUIState{Blank,CutScene,RaceInfo,Countdown,RaceGUI,ScoreBoard,NextMenu};
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

var finishedCharacters : DisplayName[];

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

var nRacer = new Racer(true,-1,myRacer.character,myRacer.hat,myRacer.kart,myRacer.wheel,0);
myRacer = nRacer;

finishedCharacters = new DisplayName[0];

ChangeState(GUIState.Blank);
networkID = -1;

while(Application.loadedLevelName != "Lobby")
	yield;

this.enabled = false;

}

@RPC
function EndClient()
{
	StopCoroutine("SendUpdates");
	
	Debug.Log("Ahahaha");
	
	if(myRacer != null && !myRacer.finished)
	{
		myRacer.ingameObj.gameObject.AddComponent(Racer_AI);
		Destroy(myRacer.ingameObj.GetComponent(kartInput));
		myRacer.ingameObj.GetComponent(kartInfo).hidden = true;
		
		myRacer.cameras.GetChild(0).GetComponent.<Camera>().enabled = false;
		myRacer.cameras.GetChild(1).GetComponent.<Camera>().enabled = true;

		while(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Distance > -6.5){
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Distance -= Time.fixedDeltaTime * 10;
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Height = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Height,1,Time.fixedDeltaTime);
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight,1,Time.fixedDeltaTime);
		myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount,-1.9,Time.fixedDeltaTime);
		yield;
		}
	}
	
	ChangeState(GUIState.ScoreBoard);
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

function FixedUpdate ()
{
	if(myRacer.ingameObj != null)
	{
		var pf : Position_Finding = myRacer.ingameObj.GetComponent(Position_Finding);
		if(pf.Lap >= td.Laps && !myRacer.finished)
		{
			myRacer.finished = true;
			ChangeState(GUIState.ScoreBoard);
		}
			
	}
}

function SendUpdates()
{	

	CalculateSendUpdate();
	
	while(currentGUI != GUIState.RaceGUI)
	{
		yield;
	}

	while(!myRacer.finished)
	{	
		CalculateSendUpdate();	
		yield WaitForSeconds(0.5f);
	}
	
	myRacer.ingameObj.gameObject.AddComponent(Racer_AI);
	Destroy(myRacer.ingameObj.GetComponent(kartInput));
	myRacer.ingameObj.GetComponent(kartInfo).hidden = true;
	myRacer.ingameObj.GetComponent(kartItem).locked = true;
	
	myRacer.cameras.GetChild(0).GetComponent.<Camera>().enabled = false;
	myRacer.cameras.GetChild(1).GetComponent.<Camera>().enabled = true;
	
	if(Network.isClient)
		transform.GetComponent.<NetworkView>().RPC("Finished",RPCMode.Server,networkID);
		
	if(Network.isServer)
		transform.GetComponent(RaceLeader).LocalFinish(networkID);

	yield WaitForSeconds(2);

	while(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Distance > -6.5){
	myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Distance -= Time.fixedDeltaTime * 10;
	myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Height = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).Height,1,Time.fixedDeltaTime);
	myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).PlayerHeight,1,Time.fixedDeltaTime);
	myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount = Mathf.Lerp(myRacer.cameras.GetChild(1).GetComponent(Kart_Camera).sideAmount,-1.9,Time.fixedDeltaTime);
	yield;
	}
	
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
			transform.GetComponent(RaceLeader).LocalPositionUpdate(networkID,myRacer.TotalDistance,myRacer.NextDistance);
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
			transform.GetComponent(RaceLeader).LocalFinish(networkID);
		else
			transform.GetComponent(RaceLeader).SetFinished(true);//Single Player Check
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
				if(transform.GetComponent(RaceLeader).type == RaceStyle.TimeTrial)
					raceTexture = Resources.Load("UI Textures/Level Selection/TimeTrial",Texture2D);
				else
					raceTexture = Resources.Load("UI Textures/Level Selection/" + transform.GetComponent(RaceLeader).race,Texture2D);
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
		case GUIState.ScoreBoard:
		
			var BoardTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/Backing",Texture2D);
			var BoardRect : Rect = Rect(Screen.width/2f - Screen.height/16f,Screen.height/16f,Screen.width/2f ,(Screen.height/16f)*14f);

			GUI.DrawTexture(BoardRect,BoardTexture);

			GUI.BeginGroup(BoardRect);

				for(var f : int = 0; f < finishedCharacters.Length; f++){
				
					if(finishedCharacters[f] != -1)
					{
						var PosTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/" + (f+1).ToString(),Texture2D);
						var SelPosTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/" + (f+1).ToString() + "_Sel",Texture2D);
						//var NameTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/" + gd.Characters[finishedCharacters[f].character].Name,Texture2D);
						//var SelNameTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/" + gd.Characters[finishedCharacters[f].character].Name + "_Sel",Texture2D);

						var Ratio = (Screen.height/16f)/PosTexture.height;
						//var Ratio2 = (Screen.height/16f)/NameTexture.height;

						GUI.DrawTexture(Rect(10,(f+1)*Screen.height/16f,PosTexture.width * Ratio,Screen.height/16f),PosTexture);
						//GUI.DrawTexture(Rect(20 + PosTexture.width * Ratio,(f+1)*Screen.height/16f,NameTexture.width * Ratio2,Screen.height/16f),SelNameTexture);
						GUI.Label(Rect(10 + (PosTexture.width * Ratio) + Screen.height/16f,(f+1)*Screen.height/16f,BoardRect.width - (20 + (PosTexture.width * Ratio) + Screen.height/16f),Screen.height/16f),finishedCharacters[f].name);
						
						
						if(finishedCharacters[f].character != -1)
						{
							var CharacterIcon = gd.Characters[finishedCharacters[f].character].Icon;
							GUI.DrawTexture(Rect(10 + (PosTexture.width * Ratio),(f+1)*Screen.height/16f,Screen.height/16f,Screen.height/16f),CharacterIcon);
						}
						/*if(isEmpty(SPRacers[f].timer))
							GUI.Label(Rect(20 + (PosTexture.width * Ratio) + (NameTexture.width * Ratio2 * 1.5f) ,3 + (f+1)*Screen.height/16f,NameTexture.width * Ratio2,Screen.height/16f),"-N/A-");
						else
							GUI.Label(Rect(20 + (PosTexture.width * Ratio) + (NameTexture.width * Ratio2 * 1.5f) ,3 + (f+1)*Screen.height/16f,NameTexture.width * Ratio2,Screen.height/16f),SPRacers[f].timer.ToString());

						GUI.Label(Rect(20 + (PosTexture.width * Ratio) + (NameTexture.width * Ratio2 * 2.5f) ,3 + (f+1)*Screen.height/16f,NameTexture.width * Ratio2,Screen.height/16f),SPRacers[f].points.ToString());

						GUI.Label(Rect(20 + (PosTexture.width * Ratio) + (NameTexture.width * Ratio2 * 2.9f) ,3 + (f+1)*Screen.height/16f,NameTexture.width * Ratio2,Screen.height/16f),"+ " + (15 - f).ToString());*/
					}
				}

			GUI.EndGroup();

		break;
	
	}
}	

@RPC
function ScoreBoardAdd(character : int, name : String, i : int, size : int)
{

	if(finishedCharacters.Length != size)
	{
		finishedCharacters = new DisplayName[size];
		
		for(var j : int = 0; j <  finishedCharacters.Length; j++)
		{
			finishedCharacters[j] = new DisplayName();
		}
		
	}
	
	Debug.Log(name + " has come " + (i) + "th");
		
	finishedCharacters[i].character = character;	
	finishedCharacters[i].name = name;	
	
	if(networkID == -1)
	{
		ChangeState(GUIState.ScoreBoard);
	}

}

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
	
}	
	
@RPC
function SetupGDTracks(cup : int, track : int)
{

	gd.currentCup = cup;
	gd.currentTrack = track;

}	
	
	