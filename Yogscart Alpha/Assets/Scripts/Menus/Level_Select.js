#pragma strict

 var currentCup : int;
 var currentTrack : int;
private var gd : CurrentGameData;
private var im : InputManager;
private var mm : MainMenu;

var TypeSelecion : boolean;

var hidden : boolean = true;

var GrandPrixOnly : boolean;

private var Alpha : float;

function Awake(){
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);

	if(GameObject.Find("Menu Holder").GetComponent(MainMenu) != null)
		mm = GameObject.Find("Menu Holder").GetComponent(MainMenu);
		
}

function OnGUI () {

	var canInput : boolean = true;
	
	if(mm != null && mm.transitioning)
		canInput = false;

if(hidden == false)
Alpha = Mathf.Lerp(Alpha,1,Time.deltaTime*5);
else
Alpha = Mathf.Lerp(Alpha,0,Time.deltaTime*5);

GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);
GUI.color = Color(256,256,256,Alpha);

if(GrandPrixOnly)
	TypeSelecion = false;

var submitBool : boolean;
var cancelBool : boolean;
var vert : boolean;
var hori : float;

if(!hidden && canInput){
submitBool = im.c[0].GetMenuInput("Submit") != 0;
cancelBool = im.c[0].GetMenuInput("Cancel") != 0;
vert = im.c[0].GetMenuInput("Vertical") != 0;
hori = im.c[0].GetMenuInput("Horizontal");
}

if(hidden == false){
//Do Input
if(vert && !GrandPrixOnly){
TypeSelecion = !TypeSelecion;
} 

if(cancelBool){
	// Exit Stuff
	if(!Network.isServer && !Network.isClient)
	{
		var mm = GameObject.Find("Menu Holder").GetComponent(MainMenu);
		
		mm.CancelCharacterSelect();
		mm.StartCoroutine("StartCharacterSelect");
		
		hidden = true;
		this.enabled = false;
	}
}

if(TypeSelecion){ //Track Selection

if(hori != 0){
currentTrack += Mathf.Sign(hori);
if(currentTrack < 0)
currentTrack = 3;

if(currentTrack > 3)
currentTrack = 0;
} 

if(submitBool){
Finished();
}

}else{

if(hori){
currentCup += Mathf.Sign(hori);
if(currentCup < 0)
currentCup = gd.Tournaments.Length-1;

if(currentCup >= gd.Tournaments.Length)
currentCup = 0;

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

if(!hidden && canInput)
	var click = im.GetClick();

//Render Tracks
for(var j : int = 0; j < gd.Tournaments[currentCup].Tracks.Length; j++){
var OverallRect : Rect = Rect(75 + ((j+1)*14.5f*Ratio) + (j*238f*Ratio),Screen.height/2f + (16f*Ratio),239f*Ratio,210f*Ratio);

GUI.DrawTexture(OverallRect,gd.Tournaments[currentCup].Tracks[j].Logo);

if(TypeSelecion && currentTrack == j)
GUI.DrawTexture(OverallRect,Selected);

if(!hidden && !GrandPrixOnly && im.MouseIntersects(OverallRect) && canInput)
{
currentTrack = j;
TypeSelecion = true;

if(click)
	Finished();

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

if(!hidden && im.MouseIntersects(TRect) && canInput)
{
currentCup = i;
TypeSelecion = false;

if(GrandPrixOnly && click)
	Finished();
}

}


//Render level holder
var LHRect : Rect = Rect(75,Screen.height/2,Width,Height);
GUI.DrawTexture(LHRect,LevelHolder);

if(GrandPrixOnly){

var rankText : String = gd.Tournaments[currentCup].LastRank[gd.Difficulty];
var rankRect : Rect = Rect(75,Screen.height/2 + 10 + Height/1.5f,Width,Height);
OutLineLabel(rankRect,rankText,2,Color.black);


}

	if(transform.GetComponent(RaceLeader).type == RaceStyle.TimeTrial)
	{
		var timeRect : Rect = Rect(75,Screen.height/2 + 10 + Height/1.5f,Width,Height);
		var timeString : String = gd.Tournaments[currentCup].Tracks[currentTrack].BestTrackTime.ToString();
		OutLineLabel(timeRect,timeString,2,Color.black);
	}

}


function Finished(){
	if(Network.isServer == true || Network.isClient == true){
		SendRPC();
	}else{
		//Single Player Stuff
		gd.currentCup = currentCup;
		
		if(!GrandPrixOnly)
			gd.currentTrack = currentTrack;
		else
		{
			gd.currentTrack = 0;
		}
		
		this.enabled = false;
		
	}
}

function SendRPC(){
Debug.Log("Send me a RPC");

if(Network.isClient == true)
GetComponent.<NetworkView>().RPC ("LevelChoose",RPCMode.Server,currentCup,currentTrack);
else
transform.GetComponent(RaceLeader).LevelChoose(currentCup,currentTrack);

transform.GetComponent(VotingScreen).hidden = false;
hidden = true;

}


function WithinBounds(Area : Rect){

if(Input.mousePosition.x >= Area.x && Input.mousePosition.x <= Area.x + Area.width 
&&  Screen.height-Input.mousePosition.y >= Area.y &&  Screen.height-Input.mousePosition.y <= Area.y + Area.height)
return true;
else
return false;

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
