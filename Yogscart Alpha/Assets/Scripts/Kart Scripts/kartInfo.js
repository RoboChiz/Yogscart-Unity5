#pragma strict

//kartInfo V1
//Created by Robo_Chiz
//Purpose: The purpose of this script is to display the race GUI for the kart involved.

enum ScreenType{Full,TopLeft,TopRight,BottomLeft,BottomRight,Top,Bottom};

var screenPos : ScreenType = ScreenType.Full;

var position : int = -1;

private var lastPosition : int = -1;
private var flashing : boolean;

//@HideInInspector
var lapisCount : int = 0;
@HideInInspector
var lap : int = 0;

var cameras : Camera[];

var itemFlashing : boolean;
var itemFlashSpeed : float = 5f;
private var itemFlashAlpha : float = 0f;
private var itemFlashDirection : boolean;

var turnAroundFlashing : boolean;
private var turnAroundFlashAlpha : float = 0f;
private var turnAroundFlashDirection : boolean;

private var td : TrackData;
private var rl : RaceLeader;
private var sm : Sound_Manager;

var finishShow : boolean;
var finishAlpha : float = 0f;
var finishRect : Rect;
var screenRect : Rect;

function Start () {

	td = GameObject.Find("Track Manager").GetComponent(TrackData);
	rl = GameObject.Find("GameData").GetComponent(RaceLeader);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 
		
}

//Handles Fading in, and flashes on Position Change
@HideInInspector
var hidden : boolean = true;
@HideInInspector
var shrunk : boolean = true;

private var raceGUIAlpha : int = 0;
private var PosGUISize : int = 0;

function OnGUI () {

GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);

//Update Lapis and Lap
lapisCount = transform.GetComponent(kartScript).lapisAmount;
lap = transform.GetComponent(Position_Finding).Lap;
position = transform.GetComponent(Position_Finding).position;

//Affect Alpha and Size based on booleans
if(hidden)
raceGUIAlpha = Mathf.Lerp(raceGUIAlpha,0,Time.deltaTime*2f);
else
raceGUIAlpha = Mathf.Lerp(raceGUIAlpha,255,Time.deltaTime*2f);

if(shrunk)
PosGUISize = Mathf.Lerp(PosGUISize,0,Time.deltaTime*3f);
else
if(screenPos == ScreenType.Full)
PosGUISize = Mathf.Lerp(PosGUISize,Screen.height/6f,Time.deltaTime*3f);
else
PosGUISize = Mathf.Lerp(PosGUISize,Screen.height/8f,Time.deltaTime*3f);

//Play Position Animation
if(lastPosition != position && !flashing){
FlashPos();
flashing = true;
}

//Adjust Cameras to fit in the Required Space
if(cameras != null)
for(var i : int = 0; i < cameras.Length; i++){

if(screenPos == ScreenType.Full)
cameras[i].rect = Rect(0,0,1,1);

if(screenPos == ScreenType.TopLeft)
cameras[i].rect = Rect(0,0.5,0.5,0.5);

if(screenPos == ScreenType.TopRight)
cameras[i].rect = Rect(0.5,0.5,0.5,0.5);

if(screenPos == ScreenType.BottomLeft)
cameras[i].rect = Rect(0,0,0.5,0.5);

if(screenPos == ScreenType.BottomRight)
cameras[i].rect = Rect(0.5,0,0.5,0.5);

if(screenPos == ScreenType.Top)
cameras[i].rect = Rect(0,0.5,1,0.5);

if(screenPos == ScreenType.Bottom)
cameras[i].rect = Rect(0,0,1,0.5);

}


GUI.color = new Color32(255, 255, 255, raceGUIAlpha);

if(rl.type != RaceStyle.TimeTrial)
{
	//Render Position GUI
	if(position != -1){
	var postexture = Resources.Load("UI Textures/Positions/" + (lastPosition+1).ToString(),Texture2D);	
	var renderArea : Rect;
	
	if(screenPos == ScreenType.Full || screenPos == ScreenType.BottomRight || screenPos == ScreenType.Bottom )
	renderArea = Rect(Screen.width - 10 - PosGUISize,Screen.height - 10 - PosGUISize,PosGUISize,PosGUISize);

	if(screenPos == ScreenType.TopLeft)
	renderArea = Rect(Screen.width/2f - 10 - PosGUISize,Screen.height/2f - 10 - PosGUISize,PosGUISize,PosGUISize);

	if(screenPos == ScreenType.TopRight)
	renderArea = Rect(Screen.width - 10 - PosGUISize,Screen.height/2f - 10 - PosGUISize,PosGUISize,PosGUISize);

	if(screenPos == ScreenType.BottomLeft)
	renderArea = Rect(Screen.width/2f - 10 - PosGUISize,Screen.height - 10 - PosGUISize,PosGUISize,PosGUISize);

	if(screenPos == ScreenType.Top)
	renderArea = Rect(Screen.width - 10 - PosGUISize,Screen.height/2f - 10 - PosGUISize,PosGUISize,PosGUISize);

	if(postexture != null)
	GUI.DrawTexture(renderArea,postexture,ScaleMode.ScaleToFit);
	}
}
else
{
	//Draw Timer
	GUI.Label(Rect(Screen.width - 10 - Screen.width/5f,Screen.height - 20 - GUI.skin.label.fontSize,Screen.width/5f,GUI.skin.label.fontSize + 5),TimeManager.TimerToString(rl.raceTimer));
}

//Render Lap and Lapis
var style = new GUIStyle(GUI.skin.GetStyle("Special Box"));

var BoxWidth : float;
var BoxHeight : float;

if(screenPos == ScreenType.Full){
BoxWidth = Screen.width / 10f;
BoxHeight = Screen.height / 16f;
}else{
BoxWidth = Screen.width / 16f;
BoxHeight = Screen.height / 22f;
}

style.fontSize = (BoxHeight+BoxWidth)/8f;

//Calculate renderarea
if(screenPos == ScreenType.Full)
renderArea = Rect(10,Screen.height - 10 - BoxHeight,BoxWidth,BoxHeight);

if(screenPos == ScreenType.TopLeft || screenPos == ScreenType.Top)
renderArea = Rect(10,Screen.height/2f - 10 - BoxHeight,BoxWidth,BoxHeight);

if(screenPos == ScreenType.TopRight)
renderArea = Rect(10 + Screen.width/2f,Screen.height/2f - 10 - BoxHeight,BoxWidth,BoxHeight);

if(screenPos == ScreenType.BottomLeft || screenPos == ScreenType.Bottom)
renderArea = Rect(10,Screen.height - 10 - BoxHeight,BoxWidth,BoxHeight);

if(screenPos == ScreenType.BottomRight)
renderArea = Rect(10 + Screen.width/2f,Screen.height - 10 - BoxHeight,BoxWidth,BoxHeight);

GUI.Box(renderArea,"Lap : " + Mathf.Clamp(lap+1,1,td.Laps).ToString() + " / " + td.Laps,style);

var LapisTexture : Texture2D = Resources.Load("UI Textures/Power Ups/Lapis",Texture2D);
GUI.Box(Rect(10 + renderArea.x + BoxWidth,renderArea.y,BoxWidth*0.75f,BoxHeight),"    " + lapisCount,style);
GUI.DrawTexture(Rect(10 + renderArea.x + BoxWidth,renderArea.y,(BoxHeight/LapisTexture.height)*LapisTexture.width,BoxHeight),LapisTexture,ScaleMode.ScaleToFit);
	
	GUI.color = new Color32(255, 255, 255, itemFlashAlpha);
	
	var itemIncoming : Texture = Resources.Load("UI Textures/Race/incoming",Texture2D);
	var iconSize : float = screenRect.width/6f;
	GUI.DrawTexture(Rect(screenRect.x + screenRect.width/2f - (iconSize/2f),screenRect.y + screenRect.height - 10 - iconSize,iconSize,iconSize),itemIncoming);
	
	GUI.color = new Color32(255, 255, 255, turnAroundFlashAlpha);
	itemIncoming = Resources.Load("UI Textures/Race/turn around",Texture2D);
	GUI.DrawTexture(Rect(screenRect.x + screenRect.width/2f - (iconSize/2f),screenRect.y + 10,iconSize,iconSize),itemIncoming);

	//Finish
	switch(screenPos)
	{
		case ScreenType.Full:
			screenRect = Rect(0,0,Screen.width,Screen.height);
		break;
		case ScreenType.TopLeft:
			screenRect = Rect(0,0,Screen.width/2f,Screen.height/2f);
		break;
		case ScreenType.TopRight:
			screenRect = Rect(Screen.width/2f,0,Screen.width/2f,Screen.height/2f);
		break;
		case ScreenType.BottomLeft:
			screenRect = Rect(0,Screen.height/2f,Screen.width/2f,Screen.height/2f);
		break;
		case ScreenType.BottomRight:
			screenRect = Rect(Screen.width/2f,Screen.height/2f,Screen.width/2f,Screen.height/2f);
		break;
		case ScreenType.Top:
			screenRect = Rect(0,0,Screen.width,Screen.height/2f);
		break;
		case ScreenType.Bottom:
			screenRect = Rect(0,Screen.height/2f,Screen.width,Screen.height/2f);
		break;
	}
	
	
	GUI.color.a = finishAlpha;		
	var finishTexture = Resources.Load("UI Textures/CountDown/Finish",Texture2D);

	if(finishTexture != null)
		GUI.DrawTexture(finishRect,finishTexture,ScaleMode.ScaleToFit);

	finishRect.y = Mathf.Lerp(finishRect.y,screenRect.y + (screenRect.height*0.25),Time.deltaTime);
	finishRect.height = Mathf.Lerp(finishRect.height,screenRect.height/2f,Time.deltaTime);

	if(finishShow)
		finishAlpha = Mathf.Lerp(finishAlpha,256,Time.deltaTime*10f);
	else
		finishAlpha = Mathf.Lerp(finishAlpha,0,Time.deltaTime*10f);
			
}

//

function Finish()
{
	finishRect = Rect(screenRect.x,screenRect.y + (screenRect.height/2f) - ((screenRect.height/3f)/2f),screenRect.width,screenRect.height/3f);
	finishShow = true;
	
	yield WaitForSeconds(1f);
	
	finishShow = false;
}

function Update()
{
	//Item Incoming
	if(itemFlashing)
	{
		if(itemFlashDirection)
			itemFlashAlpha = Mathf.Lerp(itemFlashAlpha,255,Time.deltaTime*itemFlashSpeed);
		else
			itemFlashAlpha = Mathf.Lerp(itemFlashAlpha,0,Time.deltaTime*itemFlashSpeed);
		
		if(itemFlashAlpha > 250)
			itemFlashDirection = false;
			
		if(itemFlashAlpha < 5)
			itemFlashDirection = true;
		
	}
	else
	{
		if(itemFlashAlpha > 2f)
			itemFlashAlpha = Mathf.Lerp(itemFlashAlpha,0,Time.deltaTime*itemFlashSpeed);
	}
	
	//Turn Around
	if(turnAroundFlashing)
	{
		if(turnAroundFlashDirection)
			turnAroundFlashAlpha = Mathf.Lerp(turnAroundFlashAlpha,255,Time.deltaTime*itemFlashSpeed);
		else
			turnAroundFlashAlpha = Mathf.Lerp(turnAroundFlashAlpha,0,Time.deltaTime*itemFlashSpeed);
		
		if(turnAroundFlashAlpha > 250)
			turnAroundFlashDirection = false;
			
		if(turnAroundFlashAlpha < 5)
			turnAroundFlashDirection = true;
		
	}
	else
	{
		if(turnAroundFlashAlpha > 2f)
			turnAroundFlashAlpha = Mathf.Lerp(turnAroundFlashAlpha,0,Time.deltaTime*itemFlashSpeed);
	}
}

function NewLap()
{
	sm.PlaySFX(Resources.Load("Music & Sounds/SFX/newlap",AudioClip));
}

function FlashPos(){
shrunk = true;
yield WaitForSeconds(0.5);

lastPosition = position;
shrunk = false;
yield WaitForSeconds(0.5);

flashing = false;
}
