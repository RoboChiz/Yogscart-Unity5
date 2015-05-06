#pragma strict

private var gd : CurrentGameData;
var Votes : Vector2[];

var hidden : boolean = true;

function Awake(){
gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
Votes = new Vector2[0];
}

@RPC
function VoteUpdate(record : int){

var addTo : int;
var nVote : Vector2;

for(var i : int = 0; i < gd.Tournaments.Length; i++){
for(var j : int = 0; j < gd.Tournaments[i].Tracks.Length; j++){
if(record == addTo){
nVote = Vector2(i,j);
}
addTo += 1;
}
}

var copy = new Array();
copy = Votes;
copy.Push(nVote);

Votes = copy;

}

function OnGUI () {

GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);

if(hidden == false){
for(var i : int = 0; i < Votes.Length; i++){

if(selected == i)
GUI.Box(Rect(Screen.width/2 - 100,20 + (50*i),200,40),gd.Tournaments[Votes[i].x].Tracks[Votes[i].y].Name,GUI.skin.GetStyle("SelectedBox"));
else
GUI.Box(Rect(Screen.width/2 - 100,20 + (50*i),200,40),gd.Tournaments[Votes[i].x].Tracks[Votes[i].y].Name); //Add track name!

}
}
}

private var selected : int = -1;

@RPC
function StartRoll(i : int){

gameObject.AddComponent(AudioSource);
transform.GetComponent(AudioSource).GetComponent.<AudioSource>().clip = Resources.Load("Music & Sounds/Ting",AudioClip);
var t : float;

while (t < 3f){
selected += 1;

if(selected >= Votes.Length)
selected = 0; 

transform.GetComponent(AudioSource).GetComponent.<AudioSource>().Play ();

yield WaitForSeconds(0.2);
t +=0.2f;
}

while(selected != i){
selected += 1;

if(selected >= Votes.Length)
selected = 0; 

yield WaitForSeconds(0.2);
t +=0.2f;

}

Destroy(transform.GetComponent(AudioSource));

}
