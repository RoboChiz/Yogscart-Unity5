
#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;

private var version : String;
var Logo : Texture2D;
var Theme : AudioClip;

enum Menu{StartScreen,MainMenu,LocalMenu,DifficultyMenu,Options,Credits,CharacterSelect};
var State : Menu = Menu.StartScreen;

enum GameChosen{SinglePlayer,Host,Client};
var GameState : GameChosen = GameChosen.SinglePlayer;

var currentSelection : int;

var Flashing : boolean;
var LockedColourAlpha : Color = Color.red;

private var www1 : WWW;
private var Error : boolean = false;

var scrolling : boolean;
private var sideScroll : int;
var scrollTime : float = 5f;

var titlesideScroll : int;
var animated : boolean;
var locked : boolean;

var playerName : String;

//Options
private var ScreenR : int;
private var FullScreen : boolean;
private var Quality : int;

private var creditsHeight : float;

private var ConfirmSound : AudioClip;
private var BackSound : AudioClip;

private var serverScroll : Vector2;

function Awake() {
    MasterServer.RequestHostList("YogscartTournament");
}


function Start(){

gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
im = GameObject.Find("GameData").GetComponent(InputManager);
sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 

version = gd.version;

im.allowedToChange = false;
gd.BlackOut = true;

LockedColourAlpha.a = 0;

GetOptionSettings();

ConfirmSound = Resources.Load("Music & Sounds/SFX/confirm",AudioClip);
BackSound = Resources.Load("Music & Sounds/SFX/back",AudioClip);

yield WaitForSeconds(1);
gd.BlackOut = false;
im.allowedToChange = true;

if(Theme != null)
sm.PlayMusic(Theme);

var url = "https://db.tt/N51AaMhM";
www1 = new WWW (url);
yield www1;

if(!String.IsNullOrEmpty(www1.error))
Error = true;

}


function OnGUI () {

var data : HostData[] = MasterServer.PollHostList();

GUI.skin = Resources.Load("Font/Menu", GUISkin);

var avg = ((Screen.height + Screen.width)/2f)/40f;

GUI.skin.label.fontSize = avg;
//GUI.skin.customStyles[4].fontSize = avg;

//Render Character and Logo
var CharacterRender : Texture2D = Resources.Load("UI Textures/New Main Menu/Side Images/"+ Random.Range(0,1),Texture2D); 
GUI.DrawTexture(Rect(Screen.width/2f - titlesideScroll,10,Screen.width/2f,Screen.height-20),CharacterRender,ScaleMode.ScaleToFit);

var LogoWidth : float = Screen.width/2f;
var Ratio : float = LogoWidth/Logo.width;

var LogoRect = Rect(Screen.width/20f,Screen.width/20f + titlesideScroll,LogoWidth,Logo.height * Ratio);
GUI.DrawTexture(LogoRect,Logo);

if(State == Menu.Credits || State == Menu.CharacterSelect){
if(animated == false)
{
HideTitles(true);
animated = true;
}
}else{
if(animated == true)
{
HideTitles(false);
animated = false;
}
}

var Options : String[];
var stateLocation : String;

if(im.c == null || im.c.Length == 0)
{
var NoControllerRect : Rect = Rect(sideScroll + Screen.width/20f,(Screen.width/20f*(1+5)),Screen.width/2f,Screen.height/4f);
OutLineLabel2(NoControllerRect,"Press start on any controller or keyboard",2);
}

if(im.c != null && im.c.Length > 0){

if(!locked && !scrolling){
var submitInput : float = im.c[0].GetMenuInput("Submit");
var submitBool = (submitInput != 0);

var cancelInput : float = im.c[0].GetMenuInput("Cancel");
var cancelBool = (cancelInput != 0);
}

var hori = im.c[0].GetMenuInput("Horizontal");
var verti = im.c[0].GetMenuInput("Vertical");

if(State != Menu.StartScreen && State != Menu.Credits)
{
var additionText : String;

if(im.c[0].inputName != "Key_")
additionText = "_Controller";

var nextText = Resources.Load("UI Textures/New Main Menu/Next" + additionText,Texture2D);
var backText = Resources.Load("UI Textures/New Main Menu/Back" + additionText,Texture2D);

var ButtonWidth : float = Screen.width/7f;
var ButtonRatio = ButtonWidth/nextText.width;
var ButtonHeight : float = nextText.height*ButtonRatio;

var nextRect : Rect = Rect(Screen.width - sideScroll - ButtonWidth,Screen.height - ButtonHeight,ButtonWidth,ButtonHeight);
var backRect : Rect = Rect(sideScroll,Screen.height - ButtonHeight,ButtonWidth,ButtonHeight);

GUI.DrawTexture(nextRect,nextText);
GUI.DrawTexture(backRect,backText);

}


if(submitBool)
sm.PlaySFX(ConfirmSound);

if(cancelBool)
sm.PlaySFX(BackSound);

switch(State) {
	
    case Menu.StartScreen:
    
    	Options = [];
    
        var PressStart : Texture2D = Resources.Load("UI Textures/New Main Menu/Press Start",Texture2D); 
		var PressStartWidth : float = Screen.width/3f;
		var PressStartRatio : float = PressStartWidth/PressStart.width;
		var PressStartRect = Rect(sideScroll + Screen.width/20f * 2.5f,Screen.height * (4f/6f),PressStartWidth,PressStart.height * PressStartRatio);

		GUI.DrawTexture(PressStartRect,PressStart);

		//Update Version

		var VersionRect : Rect = Rect(sideScroll +  Screen.width/20f * 2.5f,Screen.height * (5f/6f),PressStartWidth,PressStart.height * PressStartRatio);
		if(www1 != null ){
			if(www1.isDone == false)
				OutLineLabel(VersionRect,version + " [Checking]",2);
			else if(Error)
				OutLineLabel(VersionRect,version + " [NO Internet Connection]",2);
			else if(version == www1.text)
				OutLineLabel(VersionRect,www1.text,2);
			else
				OutLineLabel(VersionRect,version + " [UPDATE AVAILABLE]",2);
		}else{
				OutLineLabel(VersionRect,version + " I AM ERROR!",2);
		}
		
		if(submitBool)
			ChangeState(Menu.MainMenu);

	break;
	
	case Menu.MainMenu:
		Options = ["SinglePlayer","Multiplayer","Options","Credits","Quit"];
		stateLocation = "State 1";
		
		if(cancelBool)
			ChangeState(Menu.StartScreen);
			
		if(submitBool)
		{
			switch(currentSelection){
			case 0:
			GameState = GameChosen.SinglePlayer;
			ChangeState(Menu.LocalMenu);		
			break;
			case 1:
			im.RemoveOtherControllers();
			im.allowedToChange = false;
			//Load Lobby	
			break;
			case 2:
			ChangeState(Menu.Options);		
			break;
			case 3:
			creditsHeight = Screen.height;
			ChangeState(Menu.Credits);		
			break;
			case 4:
			Application.Quit();	
			break;
			}
		
		}
		
	break;
	
	case Menu.LocalMenu:
		Options = ["Grand Prix","Custom Race","Time Trial","Back"];
		stateLocation = "State 2";
		
		if(cancelBool)
			ChangeState(Menu.MainMenu);
			
		if(submitBool)
		{/*
			switch(currentSelection){
			case 0:
			transform.GetComponent(Level_Select).GrandPrixOnly = true;
			gd.transform.GetComponent(SinglePlayer_Script).type = RaceStyle.GrandPrix;
			ChangeState(Menu.DifficultyMenu);
			break;
			case 1:
			transform.GetComponent(Level_Select).GrandPrixOnly = false;
			gd.transform.GetComponent(SinglePlayer_Script).type = RaceStyle.CustomRace;
			ChangeState(Menu.DifficultyMenu);
			break;
			case 2:
			transform.GetComponent(Level_Select).GrandPrixOnly = false;
			gd.transform.GetComponent(SinglePlayer_Script).type = RaceStyle.TimeTrial;
			im.RemoveOtherControllers();
			im.allowedToChange = false;
			StartCoroutine("StartSinglePlayer");
			locked = true;
			ChangeState(Menu.CharacterSelect);
			break;
			case 3:
			ChangeState(Menu.MainMenu);	
			break;			
			}*/
		
		}
		
	break;

	case Menu.DifficultyMenu:
		
		if(gd.unlockedInsane)
		Options = ["50cc","100cc","150cc","Insane","Back"];
		else
		Options = ["50cc","100cc","150cc","Back"];
		
		stateLocation = "State 3";
		
		if(cancelBool)
			ChangeState(Menu.LocalMenu);
			
		if(submitBool)
		{
		
			if((gd.unlockedInsane && currentSelection != 4) || (!gd.unlockedInsane && currentSelection != 3))
			{
				//gd.transform.GetComponent(SinglePlayer_Script).Difficulty = currentSelection;
				StartCoroutine("StartSinglePlayer");
				locked = true;
				ChangeState(Menu.CharacterSelect);
			}
			else
			{
				ChangeState(Menu.LocalMenu);
			}
		
		}
		
	break;
	
	case Menu.CharacterSelect:
		Options = [];
		stateLocation = "State 14";
	break;
	
	case Menu.Credits:
		Options = [];
		stateLocation = "State 9";
		
		if(cancelBool)
			ChangeState(Menu.MainMenu);
			
		if(submitBool)
			ChangeState(Menu.MainMenu);
			
		var Credits : String[] = ["Ross - Project Manager / Developer","Robo_Chiz - Lead Programmer / Networking",
		"Mysca - Level Design / UI","Beardbotnik - Character Design / Graphics Designer", "Tom - Animation", "Pico - Music","Hyper - Website Design", "Puda (@ArgonianWTF) - Community Manager","HammerFishys - Community Manager",
		"Yogscart is a non-profit fan game and is in no way affiliated with the Yogscast or the Youth Olympic Games", "We hope you enjoyed the alpha"];
		
		LogoWidth = Screen.width/2f;
		Ratio = LogoWidth/Logo.width;
		LogoRect = Rect(Screen.width/2f - LogoWidth/2f,creditsHeight,LogoWidth,Logo.height * Ratio);

		GUI.DrawTexture(LogoRect,Logo);

		for(var cred : int = 0; cred < Credits.Length; cred++)
			OutLineLabel(Rect(Screen.width/2f - LogoWidth/2f,creditsHeight + (Logo.height * Ratio * (cred+1)) ,LogoWidth,Logo.height * Ratio),Credits[cred],2);

		creditsHeight -= Time.deltaTime*40f;
		
	break;
	
	case Menu.Options:
		Options = ["Resolution","FullScreen","Quality","PlayerName","MasterVolume","MusicVolume","SFXVolume","ResetEverything","SaveChanges","Back"];
		stateLocation = "State 8";
		
		if(cancelBool)
		{
			GetOptionSettings();
			ChangeState(Menu.MainMenu);
		}	
		if(submitBool)
		{
			switch(currentSelection){
			case 1:
			if(FullScreen)
			FullScreen = false;
			else
			FullScreen = true;
			break;
			case 7:
			gd.ResetEverything();
			PlayerPrefs.SetString("playerName","Player");
			GetOptionSettings();
			break;
			case 8:
			Screen.SetResolution(Screen.resolutions[ScreenR].width,Screen.resolutions[ScreenR].height,FullScreen);
			QualitySettings.SetQualityLevel(Quality);
			PlayerPrefs.SetString("playerName",playerName);
			
			PlayerPrefs.SetInt("overallLapisCount",gd.overallLapisCount);
			Debug.Log("Done!");
			break;
			case 9:
			GetOptionSettings();
			ChangeState(Menu.MainMenu);
			break;
		
			}
		
		}
		
	break;

}

	if(verti != 0){
	
		var vinput = -Mathf.Sign(verti);

		currentSelection += vinput;
		
		if(currentSelection < 0)
			currentSelection = Options.Length - 1;
		
		if(currentSelection >= Options.Length)
			currentSelection = 0;

	}

	var YesTexture = Resources.Load("UI/Main Menu/Yes",Texture2D); 
	var NoTexture = Resources.Load("UI/Main Menu/No",Texture2D); 
	
	for(var i : int = 0;i < Options.Length;i++){
		
		var OptionsTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/" + stateLocation + "/" + Options[i],Texture2D); 
		var SelectedOptionsTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/" + stateLocation + "/" + Options[i]+"_Sel",Texture2D); 
		
		var optionsLength : float = Mathf.Clamp(Options.Length + 8,15,Mathf.Infinity);
		
		var optionheight = (Screen.height/optionsLength);
		var ratio = optionheight/OptionsTexture.height;
		
		
		var ServerList : Texture2D = Resources.Load("UI Textures/New Main Menu/Tournament_Lobby/Server_List",Texture2D); 
		var serverWidth = Screen.width/4f;
		var serverRatio = serverWidth/ServerList.width;
		var serverHeight = ServerList.height*serverRatio;
		
		var drawRect = Rect(sideScroll + serverWidth/2f,Screen.height/2f + serverHeight/2f,OptionsTexture.width * ratio,optionheight);

		if(currentSelection == i)
			GUI.DrawTexture(drawRect,SelectedOptionsTexture,ScaleMode.ScaleToFit);
		else
			GUI.DrawTexture(drawRect,OptionsTexture,ScaleMode.ScaleToFit);
		
			
		var testRect : Rect = Rect(sideScroll + Screen.width/20f,(optionheight*(i+5)),OptionsTexture.width * 2f,optionheight);	
						
		if(im.MouseIntersects(testRect))
		currentSelection = i;
		
		var LabelRect : Rect = Rect(sideScroll + Screen.width/20f + OptionsTexture.width * ratio,(optionheight*(i+5)),OptionsTexture.width * ratio,optionheight);
		
		if(State == Menu.Options)
		{
			if(hori != 0){

				var hinput = Mathf.Sign(hori);
				
				if(currentSelection == 0){
					ScreenR += hinput;
			
					if(ScreenR < 0)
						ScreenR = Screen.resolutions.Length - 1;
			
					if(ScreenR >= Screen.resolutions.Length)
						ScreenR = 0;
				}
				
				if(currentSelection == 2){
					Quality += hinput;
			
					if(Quality < 0)
						Quality = QualitySettings.names.Length - 1;
			
					if(Quality >=  QualitySettings.names.Length)
						Quality = 0;
				}
		
		
			}
	

		
		
		
			if(i == 0){
				var resRect : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5)),OptionsTexture.width * ratio,optionheight);
				
				if(im.MouseIntersects(resRect))
					currentSelection = 0;
					
				OutLineLabel2(resRect,Screen.resolutions[ScreenR].width + " x " + Screen.resolutions[ScreenR].height,2,Color.black);
				
				var leftarrowResRect : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio - optionheight,(optionheight*(i+5)),optionheight,optionheight*0.75f);
				var rightarrowResRect : Rect = Rect(sideScroll + Screen.width/10f + (OptionsTexture.width * ratio)*2.5 - optionheight,(optionheight*(i+5)),optionheight,optionheight*0.75f);
				
				if(im.MouseIntersects(leftarrowResRect) && Input.GetMouseButtonDown(0))
				{
					ScreenR -= 1;
					
					if(ScreenR < 0)
					ScreenR = Screen.resolutions.Length - 1;
					
				}
				
				if(im.MouseIntersects(rightarrowResRect) && Input.GetMouseButtonDown(0))
				{
					ScreenR += 1;
					
					if(ScreenR >= Screen.resolutions.Length)
					ScreenR = 0;
					
				}
				
				GUI.DrawTexture(leftarrowResRect,Resources.Load("UI Textures/New Main Menu/Left_Arrow",Texture2D),ScaleMode.ScaleToFit);
				GUI.DrawTexture(rightarrowResRect,Resources.Load("UI Textures/New Main Menu/Right_Arrow",Texture2D),ScaleMode.ScaleToFit);
				
			}
			
			if(i == 1){

				YesTexture = Resources.Load("UI Textures/New Main Menu/State 5/Yes",Texture2D); 
				NoTexture = Resources.Load("UI Textures/New Main Menu/State 5/No",Texture2D); 
				var yesnoRect : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5)),OptionsTexture.width * ratio,optionheight);

				if(im.MouseIntersects(yesnoRect))
					currentSelection = 1;
					

				if(FullScreen == true)
					GUI.DrawTexture(yesnoRect,YesTexture,ScaleMode.ScaleToFit);
				else
					GUI.DrawTexture(yesnoRect,NoTexture,ScaleMode.ScaleToFit);

			}
			
			if(i == 2){
			
				var qualityRect : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5)),OptionsTexture.width * ratio,optionheight);
			
				if(im.MouseIntersects(qualityRect))
					currentSelection = 2;
			
				OutLineLabel2(qualityRect,QualitySettings.names[Quality],2,Color.black);
				
				var leftarrowqualityRect : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio - optionheight,(optionheight*(i+5)),optionheight,optionheight*0.75f);
				var rightarrowqualityRect : Rect = Rect(sideScroll + Screen.width/10f + (OptionsTexture.width * ratio)*2.5 - optionheight,(optionheight*(i+5)),optionheight,optionheight*0.75f);
				
				if(im.MouseIntersects(leftarrowqualityRect) && Input.GetMouseButtonDown(0))
				{
					Quality -= 1;
					
					if(Quality < 0)
					Quality = QualitySettings.names.Length - 1;
					
				}
				
				if(im.MouseIntersects(rightarrowqualityRect) && Input.GetMouseButtonDown(0))
				{
					Quality += 1;
					
					if(Quality >= QualitySettings.names.Length)
					Quality = 0;
					
				}
				
				GUI.DrawTexture(leftarrowqualityRect,Resources.Load("UI Textures/New Main Menu/Left_Arrow",Texture2D),ScaleMode.ScaleToFit);
				GUI.DrawTexture(rightarrowqualityRect,Resources.Load("UI Textures/New Main Menu/Right_Arrow",Texture2D),ScaleMode.ScaleToFit);
				
			}
			
			if(i == 3){
			
				var playerNameRect : Rect = Rect(sideScroll + Screen.width/10f + (OptionsTexture.width * ratio),(optionheight*(i+5)),OptionsTexture.width * ratio,optionheight);
			
				if(im.MouseIntersects(playerNameRect))
					currentSelection = 3;
				
				if(currentSelection == 3)
					playerName = GUI.TextField(playerNameRect,playerName.ToString());
				else
					GUI.Label(playerNameRect,playerName);
			}
			
			var sm : Sound_Manager = GameObject.Find("Sound System").GetComponent(Sound_Manager);
			
			if(i == 4)
			{
			
			var MaxSlider : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5) + optionheight/3f),OptionsTexture.width * ratio,optionheight);
			
			sm.MasterVolume = GUI.HorizontalSlider(MaxSlider,sm.MasterVolume,0,100);

			var MaxLabel : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio - optionheight,(optionheight*(i+5)) + optionheight/4f,optionheight,optionheight*0.75f);
			OutLineLabel2(MaxLabel,sm.MasterVolume.ToString(),2);
			
			}
			
			if(i == 5)
			{
			
			var MusicSlider : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5) + optionheight/3f),OptionsTexture.width * ratio,optionheight);
			
			sm.MusicVolume = GUI.HorizontalSlider(MusicSlider,sm.MusicVolume,0,100);

			var MusicLabel : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio - optionheight,(optionheight*(i+5)) + optionheight/4f,optionheight,optionheight*0.75f);
			OutLineLabel2(MusicLabel,sm.MusicVolume.ToString(),2);
			
			}
			
			if(i == 6)
			{
			
			var SFXSlider : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio,(optionheight*(i+5) + optionheight/3f),OptionsTexture.width * ratio,optionheight);
			
			sm.SFXVolume = GUI.HorizontalSlider(SFXSlider,sm.SFXVolume,0,100);

			var SFXLabel : Rect = Rect(sideScroll + Screen.width/10f + OptionsTexture.width * ratio - optionheight,(optionheight*(i+5)) + optionheight/4f,optionheight,optionheight*0.75f);
			OutLineLabel2(SFXLabel,sm.SFXVolume.ToString(),2);
			
			}

		}
		

	}

}

OutLineLabel2(Rect(optionheight,(optionheight*(i+5)),avg*9,optionheight),"[Locked]",2,LockedColourAlpha);

}

function FlashRed(){
if(Flashing == false){

Flashing = true;

while(LockedColourAlpha.a < 1){
LockedColourAlpha.a += Time.deltaTime * 2;
yield;
}

while(LockedColourAlpha.a > 0){
LockedColourAlpha.a -= Time.deltaTime * 2;
yield;
}

Flashing = false;

}

}

function Return()
{
locked = false;

if(GameState == GameChosen.SinglePlayer)
StopCoroutine("StartSinglePlayer");

if(GameState != GameChosen.SinglePlayer)
StopCoroutine("StartMultiPlayer");

im.allowedToChange = true;
transform.GetComponent(CharacterSelect).hidden = true;
transform.GetComponent(Level_Select).hidden = true;

if(GameState == GameChosen.SinglePlayer)
ChangeState(Menu.LocalMenu);

}

function StartSinglePlayer(){

gd.CheckforNewStuff();

while(gd.currentChoices.Length == 0){
transform.GetComponent(CharacterSelect).hidden = false;
transform.GetComponent(Level_Select).hidden = true;
yield;
}


sm.StopMusic();

gd.BlackOut = true;	
			
}
	

function OutLineLabel(pos : Rect, text : String,Distance : float){
OutLineLabel(pos,text,Distance,Color.black);
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

function OutLineLabel2(pos : Rect, text : String,Distance : float){
OutLineLabel2(pos,text,Distance,Color.black);
}

function OutLineLabel2(pos : Rect, text : String,Distance : float,Colour : Color){
Distance = Mathf.Clamp(Distance,1,Mathf.Infinity);

var style = new GUIStyle(GUI.skin.GetStyle("Special Label"));
style.normal.textColor = Colour;
GUI.Label(Rect(pos.x+Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y+Distance,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x-Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y-Distance,pos.width,pos.height),text,style);
var nstyle = new GUIStyle(GUI.skin.GetStyle("Special Label"));
nstyle.normal.textColor.a = Colour.a;
GUI.Label(pos,text,nstyle);

}


function GetOptionSettings()
{

for(var i : int = 0; i < Screen.resolutions.Length; i++)
{
if(Screen.resolutions[i] == Screen.currentResolution){
ScreenR = i;
break;
}
}

FullScreen = Screen.fullScreen;
Quality = QualitySettings.GetQualityLevel();
playerName = PlayerPrefs.GetString("playerName","Player");

}

function HideTitles(hide : boolean)
{

var startTime = Time.realtimeSinceStartup;

var toScroll : float;
var fromScroll : float;

if(hide)
{
toScroll = -Screen.width/2f - Screen.width/20f;
fromScroll = 0;
}
else
{
toScroll = 0;
fromScroll = -Screen.width/2f - Screen.width/20f;
}

while(Time.realtimeSinceStartup-startTime  < scrollTime){
titlesideScroll = Mathf.Lerp(fromScroll,toScroll,(Time.realtimeSinceStartup-startTime)/scrollTime);
yield;
}

}

function ChangeState(nextStage : Menu)
{

scrolling = true;

var startTime = Time.realtimeSinceStartup;

var toScroll = -Screen.width/2f;

while(Time.realtimeSinceStartup-startTime  < scrollTime){
sideScroll = Mathf.Lerp(0,toScroll,(Time.realtimeSinceStartup-startTime)/scrollTime);
yield;
}

sideScroll = toScroll;

State = nextStage;
currentSelection = 0;

startTime = Time.realtimeSinceStartup;

while(Time.realtimeSinceStartup-startTime  < scrollTime){
sideScroll = Mathf.Lerp(toScroll,0,(Time.realtimeSinceStartup-startTime)/scrollTime);
yield;
}

sideScroll = 0;

scrolling = false;

}