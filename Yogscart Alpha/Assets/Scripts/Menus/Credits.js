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
	
	mm.ChangeMenu(MenuState.Main);
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
	["Created By","Robo_Chiz",
	"Programmed By","Robo_Chiz",
	"Art By","Robo_Chiz",
	"Music By","Robo_Chiz",
	"Tea Provided By","Robo_Chiz",
	"Massages Given By","Robo_Chiz",
	"Coughs coughed By","Robo_Chiz",
	"Sandwiches Eaten By","Robo_Chiz",
	"Josh cursed By","Robo_Chiz",
	"Credit Titles By","Robo_Chiz",
	"All fucking work By","Robo_Chiz",
	"","The End",
	"","Yogscart is a non-profit fan game and is in no way \n affiliated with the Yogscast or the youth olympic games. \n Please don't sue us! XXX"];
	
	var startY = Screen.height - creditsHeight + logoHeight;
	
	for(var i = 0; i < credits.Length; i += 2)
	{
		GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 40f;
		GUI.Label(Rect(0,startY + ((logoHeight/3f)*i),Screen.width,logoHeight),credits[i]);
		
		GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 20f;
		GUI.Label(Rect(0,startY + ((logoHeight/3f)*(i + 0.4f)),Screen.width,logoHeight),credits[i+1]);
		
	}

	GUI.color.a = holder;
}