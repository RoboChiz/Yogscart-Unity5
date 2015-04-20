#pragma strict

private var currentCup : int;
private var currentTrack : int;
private var gd : CurrentGameData;
private var im : InputManager;

private var stickLockH : boolean;
private var stickLockV : boolean;

var TypeSelecion : boolean;

var hidden : boolean = true;

var GrandPrixOnly : boolean;

private var Alpha : float;
function Awake(){
gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
im = GameObject.Find("GameData").GetComponent(InputManager);
}

function OnGUI () {

if(hidden == false)
Alpha = Mathf.Lerp(Alpha,1,Time.deltaTime*5);
else
Alpha = Mathf.Lerp(Alpha,0,Time.deltaTime*5);

GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);
GUI.color = Color(256,256,256,Alpha);

if(!hidden){
var submitInput = im.c[0].GetInput("Submit");
var submitBool = (submitInput != 0);

var cancelInput = im.c[0].GetInput("Cancel");
var cancelBool = (cancelInput != 0);
}

if(hidden == false){
//Do Input
if(im.c[0].GetInput("Vertical") != 0 && stickLockV == false && !GrandPrixOnly){
stickLockV = true; 
ButtonWaitV();
TypeSelecion = !TypeSelecion;
} 

if(cancelBool){
if(transform.GetComponent(newCharacterSelect) != null){
gd.currentChoices = new LoadOut[0];
transform.GetComponent(newCharacterSelect).ResetEverything();
transform.GetComponent(Main_Menu).StopCoroutine("StartSinglePlayer");
transform.GetComponent(Main_Menu).StartCoroutine("StartSinglePlayer");
}
}

if(TypeSelecion){ //Track Selection

if(im.c[0].GetInput("Horizontal") != 0 && stickLockH == false){
currentTrack += Mathf.Sign(im.c[0].GetInput("Horizontal"));
if(currentTrack < 0)
currentTrack = 3;

if(currentTrack > 3)
currentTrack = 0;

stickLockH = true; 
ButtonWaitH();
} 

if(submitBool){
Finished();
}

}else{

if(im.c[0].GetInput("Horizontal") != 0 && stickLockH == false){
currentCup += Mathf.Sign(im.c[0].GetInput("Horizontal"));
if(currentCup < 0)
currentCup = gd.Tournaments.Length-1;

if(currentCup >= gd.Tournaments.Length)
currentCup = 0;

stickLockH = true; 
ButtonWaitH();
}

if(GrandPrixOnly && submitBool){
currentTrack = 0;
Finished();
}

}
}

//Get Textures
var LevelHolder : Texture2D = Resources.Load("UI Textures/Level Selection/LevelHolder",Texture2D);
var Selected : Texture2D = Resources.Load("UI Textures/Level Selection/Selected",Texture2D);
var Tab : Texture2D = Resources.Load("UI Textures/Level Selection/Tab",Texture2D);
var SelectedTab : Texture2D = Resources.Load("UI Textures/Level Selection/SelectedTab",Texture2D);
//Get Width/Ratio/Height of Level Holder
var Width : float = Screen.width-150;
var Ratio : float = Width/LevelHolder.width;
var Height : int = LevelHolder.height * Ratio;

//Render Tracks
for(var j : int = 0; j < gd.Tournaments[currentCup].Tracks.Length; j++){
var OverallRect : Rect = Rect(75 + ((j+1)*14.5f*Ratio) + (j*238f*Ratio),Screen.height/2f + (16f*Ratio),239f*Ratio,210f*Ratio);

GUI.DrawTexture(OverallRect,gd.Tournaments[currentCup].Tracks[j].Logo);

if(TypeSelecion && currentTrack == j)
GUI.DrawTexture(OverallRect,Selected);

if(!hidden && !GrandPrixOnly && im.MouseIntersects(OverallRect))
{
currentTrack = j;
TypeSelecion = true;
}

}


//Render Tournaments
var TabSize : int = (Width-20-(gd.Tournaments.Length*10))/gd.Tournaments.Length;
TabSize = Mathf.Clamp(TabSize,0,(Width-20-(12*10))/12);

for(var i : int = 0; i < gd.Tournaments.Length; i++){
var TRect : Rect = Rect(85 + (i*10) + (i*TabSize),Screen.height/2f - TabSize,TabSize,TabSize);

if(currentCup == i && !TypeSelecion)
GUI.DrawTexture(TRect,SelectedTab);
else
GUI.DrawTexture(TRect,Tab);

GUI.DrawTexture(TRect,gd.Tournaments[i].Icon);

if(!hidden && im.MouseIntersects(TRect))
{
currentCup = i;
TypeSelecion = false;
}

}


//Render level holder
var LHRect : Rect = Rect(75,Screen.height/2,Width,Height);
GUI.DrawTexture(LHRect,LevelHolder);

if(GrandPrixOnly){

for(var a : int = 0; a < gd.Tournaments.Length; a++){

var rankText : String = gd.Tournaments[a].LastRank[0];
var rankRect : Rect = Rect(75,Screen.height/2 + 10 + Height/1.5f,Width,Height);
OutLineLabel(rankRect,rankText,2,Color.black);

}

}

}


function Finished(){
if(Network.isServer == true || Network.isClient == true){
SendRPC();
}else{
//Single Player Stuff
}
}

function SendRPC(){
Debug.Log("Send me a RPC");
/*
if(Network.isClient == true)
GetComponent.<NetworkView>().RPC ("LevelChoose",RPCMode.Server,currentCup,currentTrack);
else
transform.GetComponent(Host_Script).LevelChoose(currentCup,currentTrack);
*/
//transform.GetComponent(VotingScreen).hidden = false;

Destroy(this);
}


function WithinBounds(Area : Rect){

if(Input.mousePosition.x >= Area.x && Input.mousePosition.x <= Area.x + Area.width 
&&  Screen.height-Input.mousePosition.y >= Area.y &&  Screen.height-Input.mousePosition.y <= Area.y + Area.height)
return true;
else
return false;

}

function ButtonWaitH(){
yield WaitForSeconds(0.2);
stickLockH = false;
}

function ButtonWaitV(){
yield WaitForSeconds(0.2);
stickLockV = false;
}

function OutLineLabel(pos : Rect, text : String,Distance : float,Colour : Color){
Distance = Mathf.Clamp(Distance,1,Mathf.Infinity);

var style = new GUIStyle(GUI.skin.GetStyle("Label"));
style.normal.textColor = Colour;
GUI.Label(Rect(pos.x+Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y+Distance,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x-Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y-Distance,pos.width,pos.height),text,style);
var nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));
nstyle.normal.textColor.a = Colour.a;
GUI.Label(pos,text,nstyle);

}
