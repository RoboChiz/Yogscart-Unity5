#pragma strict

var playing : boolean;
var creditsHeight : float = 0f;
var scrollSpeed : float = 5f;
var creditsAlpha : float = 0f;

var mm : MainMenu;

var creditsMusic : AudioClip;
private var sm : Sound_Manager;
private var im : InputManager;

var skin : GUISkin;

function StartCredits () 
{
	creditsHeight = 0;
	
	sm = GameObject.Find("GameData").transform.GetChild(0).GetComponent(Sound_Manager);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	
	sm.PlayMusic(creditsMusic);
	playing = true;
}


function StopCredits()
{
	playing = false;
	
	yield WaitForSeconds(0.5f);
	
	sm.PlayMusic(mm.menuMusic);
	
	this.enabled = false;

}

function OnGUI ()
{

	GUI.skin = skin;

	var holder = GUI.color.a;
	GUI.color.a = creditsAlpha;

	if(playing)
	{
		creditsHeight += Time.deltaTime * scrollSpeed;
		creditsAlpha = Mathf.Lerp(creditsAlpha,1f,Time.deltaTime * 5f);
	}
	else
	{
		creditsAlpha = Mathf.Lerp(creditsAlpha,0f,Time.deltaTime * 5f);
	}	
	
	var logoHeight = (Screen.width/2f)/mm.logo.width  *  mm.logo.height;
	GUI.DrawTexture(Rect(Screen.width/2f - Screen.width/4f,Screen.height - creditsHeight, Screen.width/2f,logoHeight),mm.logo,ScaleMode.ScaleToFit);

	var credits : String[] =
	["Created By","Team Yogscart",
	"Programing","Robo_Chiz",
	"A bit of Everything","Ross",
	"3D / 2D Art","Beardbotnik",
	"Graphics Dude","Mysca",
	"Other Graphics Dude","LinkTCOne",
	"Trophy Design","Duck",
	"Music By","Pico",
	"Yogscast Outro performed by","Ben Binderow",
	"Additional Music By","Kevin MacLeod (incompetech.com) \n Licensed under Creative Commons: By Attribution 3.0 \n http://creativecommons.org/licenses/by/3.0/",
	"","",
	"","The End",
	"","Yogscart is a non-profit fan game and is in no way \n affiliated with the Yogscast or the youth olympic games. \n Please don't sue us! XXX"];
	
	var startY = Screen.height - creditsHeight + logoHeight;
	
	for(var i = 0; i < credits.Length; i += 2)
	{
		GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 40f;
		GUI.Label(Rect(0,startY + ((logoHeight/3f)*i),Screen.width,logoHeight),credits[i]);
		
		var secHeight : int = startY + ((logoHeight/3f)*(i + 0.4f));
		
		GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 20f;
		GUI.Label(Rect(0,secHeight,Screen.width,logoHeight),credits[i+1]);
		
		if(playing && i == credits.Length-2 && secHeight <= 0)
		{	
			mm.ChangeMenu(MenuState.Main);
			StopCredits();
		}
		
	}

	GUI.color.a = holder;
}