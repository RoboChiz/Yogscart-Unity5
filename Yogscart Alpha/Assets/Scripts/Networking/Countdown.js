#pragma strict

var cdTime : int;

function StartCountDown () {

while(cdTime > 0){
yield WaitForSeconds(1);
cdTime -= 1;
}

Endit();

}

function Endit(){
Destroy(this);
}

private var rotation : float;

function OnGUI () {

GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);

var TimerIcon : Texture2D = Resources.Load("UI Textures/Main Menu/Timer", Texture2D);

if(cdTime > 0){

GUIUtility.RotateAroundPivot(rotation, Vector2(60,35)); 

GUI.DrawTexture(Rect(20,-5,80,80),TimerIcon);

GUIUtility.RotateAroundPivot(-rotation, Vector2(60,35)); 

rotation += Time.deltaTime * 50;

OutLineLabel(Rect(20,20,75,75),cdTime.ToString(),1);

}


}

function OutLineLabel(pos : Rect, text : String,Distance : float){
Distance = Mathf.Clamp(Distance,1,Mathf.Infinity);

var style = new GUIStyle(GUI.skin.GetStyle("label"));
style.normal.textColor = Color.black;
GUI.Label(Rect(pos.x+Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y+Distance,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x-Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y-Distance,pos.width,pos.height),text,style);

GUI.Label(pos,text);

}