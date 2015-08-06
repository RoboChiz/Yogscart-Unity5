#pragma strict

var version : String;

var overallLapisCount : int;
var lastoverallLapisCount : int;

//Tracks 

var currentCup : int = 0;
var currentTrack : int = 0;

var currentChoices : LoadOut[];

@HideInInspector
var currentPosition : int = 0;
@HideInInspector
var Ingame : Transform;

var Tournaments : Tournament[];

@HideInInspector
var unlockedInsane : boolean;

//Characters 
var Characters : Character[];

//PowerUps
var PowerUps : PowerUp[];

//Karts
var Karts : Kart[];

//Wheels
var Wheels : Wheel[];

//Hats
var Hats : Hat[];

var RaceState : int = -1;
//0 - Grand Prix
//1 - Single Race
//2 - Time Trial
//3 - Multiplayer

var Difficulty : int;

var onlineGameModes : GameMode[];

var im : InputManager;

//@HideInInspector
var BlackOut : boolean = true;
private var isPlaying : boolean;
var PlayBackSpeed : float = 0.5f;
private var currentFrame : int = 0;

private var ColourAlpha : Color = Color.white;
	
	function Awake () {
		DontDestroyOnLoad (transform.gameObject);
				
		currentChoices = new LoadOut[4];
		
		for(var i : int = 0; i < currentChoices.Length; i++)
		{
			currentChoices[i] = new LoadOut();
		}
		
		im = transform.GetComponent(InputManager);
		LoadEverything();
		
		
		}
		
		function PlayAnimation()
		{
		
		while(true)
		{
		yield WaitForSeconds(PlayBackSpeed);
		currentFrame += 1;
		
		if(currentFrame > 22)
		currentFrame = 0;
		
		}
		
		}
		
		function Start()
		{
			if(GameObject.Find("OldGameData") != null){
				var oldGD = GameObject.Find("OldGameData").GetComponent(CurrentGameData);
				im.c = oldGD.transform.GetComponent(InputManager).c;
				Destroy(oldGD.gameObject);
			}
		}
	
		private var iconHeights : int[];
		private var inputLock : boolean;
		
		function OnGUI () {
		
		GUI.skin = Resources.Load("GUISkins/Menu", GUISkin);
		
		GUI.depth = -5;
		
		GUI.Label(Rect(10,Screen.height - GUI.skin.label.fontSize - 10,Screen.width,GUI.skin.label.fontSize * 1.25f),"Yogscart Alpha " + version.ToString());
		
		//Black Out
		var texture = new Texture2D(1,1);
		if(BlackOut == false && ColourAlpha.a > 0)
		{
		ColourAlpha.a -= Time.deltaTime;
		}
		if(BlackOut == true && ColourAlpha.a < 1)
		{
		ColourAlpha.a += Time.deltaTime;
		}

		
		texture.SetPixel(0,0,Color.black);
		texture.Apply();
		
		GUI.color = ColourAlpha;
		
			GUI.DrawTexture(Rect(-5,-5,Screen.width +5,Screen.height + 5),texture);
		
			var aniSize : float = ((Screen.height + Screen.width)/2f)/8f;
			var aniRect : Rect = Rect(Screen.width - 10 - aniSize, Screen.height - 10 - aniSize,aniSize,aniSize);
					
			GUI.DrawTexture(aniRect,Resources.Load("UI/Loading/" + (currentFrame+1),Texture2D));
			
		GUI.color = Color.white;
		
		if(BlackOut)
		{
			
			if(!isPlaying)
			{
			StartCoroutine("PlayAnimation");
			isPlaying = true;
			}
		
		
		}else{
		
		if(isPlaying)
			{
			StopCoroutine("PlayAnimation");
			isPlaying = false;
			}
			
		}
		
		}
		
		function LoadEverything(){
		
		var unlockInsane : boolean = true;
		
		for(var n = 0; n < Tournaments.Length; n++){
			Tournaments[n].LastRank = new String[4];
			Tournaments[n].LastRank[0] = PlayerPrefs.GetString(Tournaments[n].Name+"[50cc]","No Rank");
			Tournaments[n].LastRank[1] = PlayerPrefs.GetString(Tournaments[n].Name+"[100cc]","No Rank");
			Tournaments[n].LastRank[2] = PlayerPrefs.GetString(Tournaments[n].Name+"[150cc]","No Rank");
			Tournaments[n].LastRank[3] = PlayerPrefs.GetString(Tournaments[n].Name+"[Insane]","No Rank");
			
			if(Tournaments[n].LastRank[2] == "No Rank")
			unlockInsane = false;
			
			for(var k = 0; k < Tournaments[n].Tracks.Length; k++){
			
				var TimeString = PlayerPrefs.GetString(Tournaments[n].Tracks[k].Name,"0:0:0");
				var words = TimeString.Split(":"[0]);
				Tournaments[n].Tracks[k].BestTrackTime = new Timer(int.Parse(words[0]),int.Parse(words[1]),int.Parse(words[2]));
				
			}
		}
		
		unlockedInsane = unlockInsane;
		
		var foo : int = 0;
		
		for(n = 0; n < Characters.Length; n++){
		if(Characters[n].Unlocked != UnlockedState.FromStart)
		{
		foo = PlayerPrefs.GetInt(Characters[n].Name,0);
		if(foo == 1){
		Characters[n].Unlocked = UnlockedState.Unlocked;
		}else{
		Characters[n].Unlocked = UnlockedState.Locked;
		}
		}
		}
		
		for(n = 1; n < Hats.Length; n++){
		foo = PlayerPrefs.GetInt(Hats[n].Name,0);
		if(foo == 1){
		Hats[n].Unlocked = true;
		}else{
		Hats[n].Unlocked = false;
		}
		}
		
		for(n = 1; n < Karts.Length; n++){
		foo = PlayerPrefs.GetInt(Karts[n].Name,0);
		if(foo == 1){
		Karts[n].Unlocked = true;
		}else{
		Karts[n].Unlocked = false;
		}
		}
		
		for(n = 1; n < Wheels.Length; n++){
		foo = PlayerPrefs.GetInt(Wheels[n].Name,0);
		if(foo == 1){
		Wheels[n].Unlocked = true;
		}else{
		Wheels[n].Unlocked = false;
		}
		}
		
		overallLapisCount = PlayerPrefs.GetInt("overallLapisCount",0);
		lastoverallLapisCount = PlayerPrefs.GetInt("lastoverallLapisCount",0);
				
		
}

function CheckforNewStuff()
{

		if(PlayerPrefs.GetFloat("NewCharacter?",0) == 1)
		{			
				while(BlackOut == true)
				yield;
		
				PlayerPrefs.SetFloat("NewCharacter?",0);
				UnlockNewCharacter();

		}

		if(overallLapisCount >= lastoverallLapisCount + 50)
		{
		
		while(BlackOut == true)
		yield;
		
		UnlockNewHat();
		PlayerPrefs.SetInt("lastoverallLapisCount",lastoverallLapisCount+50);
		}	
		
		LoadEverything();
}
		
function UnlockNewCharacter()
{
	//Unlock Character
	var copy = new Array();
	
	for(var n = 0; n < Characters.Length;n++){
	if(Characters[n].Unlocked == UnlockedState.Locked)
	copy.Push(n);	
	}
	
	if(copy.length > 0){
		var unlockedCharacter = Random.Range(0,copy.length);
		PlayerPrefs.SetInt(Characters[copy[unlockedCharacter]].Name,1);
		
	//Popup("You have unlocked a new Character!");
	}
}

function UnlockNewHat()
{
	//Unlock Character
	var copy = new Array();
	
	for(var n = 0; n < Hats.Length;n++){
	if(Hats[n].Unlocked == false)
	copy.Push(n);	
	}
	
	if(copy.length > 0){
		var unlockedHat = Random.Range(0,copy.length);
		PlayerPrefs.SetInt(Hats[copy[unlockedHat]].Name,1);
		
	//Popup("You have unlocked a new Hat!");
	}
}
		
function ResetEverything()
{
		
	for(var n = 0; n < Tournaments.Length; n++){
		
		PlayerPrefs.SetString(Tournaments[n].Name+"[50cc]","No Rank");
		PlayerPrefs.SetString(Tournaments[n].Name+"[100cc]","No Rank");
		PlayerPrefs.SetString(Tournaments[n].Name+"[150cc]","No Rank");
		PlayerPrefs.SetString(Tournaments[n].Name+"[Insane]","No Rank");

	for(var k = 0; k < Tournaments[n].Tracks.Length; k++){
		PlayerPrefs.SetString(Tournaments[n].Tracks[k].Name,"0:0:0");
	}

	}
		
	unlockedInsane = false;
	
	for(n = 1; n < Characters.Length; n++){
	PlayerPrefs.SetInt(Characters[n].Name,0);
	}
	
	for(n = 1; n < Hats.Length; n++){
	PlayerPrefs.SetInt(Hats[n].Name,0);
	}
	
	for(n = 1; n < Karts.Length; n++){
	PlayerPrefs.SetInt(Karts[n].Name,0);
	}
	
	for(n = 1; n < Wheels.Length; n++){
	PlayerPrefs.SetInt(Wheels[n].Name,0);
	}
	
	PlayerPrefs.SetInt("overallLapisCount",0);
	PlayerPrefs.SetInt("lastoverallLapisCount",0);
	
	LoadEverything();
		
}

//Classes

enum UnlockedState{FromStart,Unlocked,Locked};

public class Character
 {
    var Name : String;
    var model : Transform;
    
    //Delete Later////
    var CharacterModel_Standing : Transform;
    //Delete Later////
    
    var selectedSound : AudioClip;

	var hitSounds : AudioClip[];
	var tauntSounds : AudioClip[];
	var Unlocked : UnlockedState;
	var Icon : Texture2D;
 }

 public class Kart
 {
    var Name : String;
    var Icon : Texture2D;
    var model : Transform;
    var Unlocked : boolean = false;
 }

public class Track
 {	
    var Name : String;
    var Logo : Texture2D;
    var Preview : Texture2D;
    
    @HideInInspector
    var BestTrackTime : Timer;
    
    var SceneID : String;
 }
 
enum ItemType{AffectsPlayer,AffectsOther,Projectile}; 
 
public class PowerUp
 {
    var Name : String;
    var Icon : Texture2D;
    var Model : Transform;
    
    var type : ItemType;
    var MultipleUses : boolean;
    var usableShield : boolean;
    
	var likelihood : int[];

 }

public class Tournament
 {
    var Name : String;
    var Icon : Texture2D;
    var TrophyModels : Transform[];
    var LastRank : String[];
    var Tracks : Track[];
    var Unlocked : boolean = false;
 }
 
public class Hat
 {
    var Name : String;
    var Icon : Texture2D;
    var Model : Transform;
    var Unlocked : boolean = false;

 } 
 
public class Wheel
{  
    var Name : String;
    var Icon : Texture2D;
    var model : Transform;
    var Unlocked : boolean = false;
}

//New Racer class which will be used in both Single Player and Multiplayer
public class Racer
{
//Racer Information
var name : String;
var human : boolean;
var aiStupidity : int = -1;

//Race Loading Infomation
var character : int;
var hat : int;
var kart : int;
var wheel : int;

//During Race Information
var finished : boolean;
var position : int;
var ingameObj : Transform;
var cameras : Transform;
var timer : Timer;
var TotalDistance : int;
var NextDistance : float;

//After Race Information
var points : int;
var team : int;

function Racer(Human : boolean, AiStupidity : int, Character : int, Hat : int, Kart : int, Wheel : int, Position : int)
{
	this.constructBase(Human,AiStupidity,Character,Hat,Kart,Wheel,Position);
}

function Racer()
{
}

function constructBase(Human : boolean, AiStupidity : int, Character : int, Hat : int, Kart : int, Wheel : int, Position : int)
{
human = Human;
aiStupidity = AiStupidity;
character = Character;
hat = Hat;
kart = Kart;
wheel = Wheel;
position = Position;
}

}

//Used to store additional Racer information for Multiplayer only. 'Racer' is called by reference.
public class NetworkedRacer extends Racer
{

 var networkplayer : NetworkPlayer;
 var connected : boolean;
 
 function NetworkedRacer(Character : int, Hat : int, Kart : int, Wheel : int, Position : int, Name : String, np : NetworkPlayer)
 {
  
    super.constructBase(true, -1, Character, Hat, Kart, Wheel, Position);
 
 	networkplayer = np;
 	connected = true;
 	name = Name;
 	
 }
 
}

public class Timer
{
	var minutes : byte;
	var seconds : byte;
	var milliSeconds : int;
	var ticking : boolean;
	
	function Timer()
	{
		minutes = 0;
		seconds = 0;
		milliSeconds = 0;
	}
	
	function Timer(m : byte, s : byte, ms : int)
	{
		minutes = m;
		seconds = s;
		milliSeconds = ms;
	}
	
	function Timer(t : Timer)
	{
		minutes = t.minutes;
		seconds = t.seconds;
		milliSeconds = t.milliSeconds;
	}
	
	function isEmpty() : boolean
	{
		if(minutes == 0 && seconds == 0 && milliSeconds == 0)
			return true;
		else
			return false;
	}
	
	function BiggerThan(t : Timer)
	{
	
		if(isEmpty())
			return true;
			
		if(t.minutes < minutes)
			return true;
		
		if(t.minutes <= minutes && t.seconds < seconds)
			return true;
			
		if(t.minutes <= minutes && t.seconds <= seconds && t.milliSeconds <= milliSeconds)
			return true;
			
		return false;		
			
	}
	
	function ToString() : String
	{
		var returnString : String = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliSeconds.ToString("000");
		return returnString;
	}
	
}

function Exit(){
Time.timeScale = 1f;
BlackOut = true;

Network.SetLevelPrefix(0);

transform.name = "OldGameData";

yield WaitForSeconds(1);
Application.LoadLevel("Main_Menu");

yield;

}
 
function StartTick(t : Timer)
{
	StartCoroutine("tick",t);
}

function StopTick()
{
	StopCoroutine("tick");
}

function tick(t : Timer)
{
	while(true)
	{
		t.milliSeconds += Time.deltaTime*1000f;
		
		if(t.milliSeconds >= 1000)
		{
			t.milliSeconds -= 1000;
			t.seconds++;
		}
		
		if(t.seconds >= 60)
		{
			t.minutes++;
			t.seconds -= 60;
		}
		
		yield;
	}
}
	 
