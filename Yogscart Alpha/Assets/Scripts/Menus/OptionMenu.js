#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;

enum State {game,graphics,audio,controls,extras};
var menuState : State = State.game;

/*
Main State holds graphics and game options
Audio hold sound and subtitles
Controls hold key binding options
*/

private var savedRect : Rect;
private var safeRect : Rect;

var selectedColour : Color = Color.cyan;
var selectedOption : int;

//Store textures once to get menu to run better
private var topBar : Texture2D;
private var topBarHeight : float;

private var leftArrow : Texture2D;
private var rightArrow : Texture2D;

function Start () 
{
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 

	LoadMenu();	
}

function LoadMenu()
{
	
	//Calculate Safe Rect
	var srHeight : float = Screen.height * (3f/4f);
	var srWidth : float = (srHeight/3f)*4f;
	var srGap : float = (Screen.height/8f);
	safeRect = Rect(Screen.width/2f - srWidth/2f,srGap,srWidth,srHeight);

	topBar = Resources.Load("OptionsMenu/Top_Bar",Texture2D);
	topBarHeight = (safeRect.width / topBar.width) * topBar.height;
	
	leftArrow = Resources.Load("UI Textures/New Main Menu/Left_Arrow",Texture2D);
	rightArrow = Resources.Load("UI Textures/New Main Menu/Right_Arrow",Texture2D);
	
	savedRect = Rect(0,0,Screen.width,Screen.height);
}

function Update () 
{
	//Get Inputs
	var menuHori : int = 0;
	var holdVal : float;
	
	if(im.c[0].inputName == "Key_")
	{
		holdVal = im.c[0].GetMenuInput("Rotate");
		if(holdVal != 0)
			menuHori = Mathf.Sign(holdVal);
	}
	else
	{
		holdVal = -im.c[0].GetMenuInput("Use Item");
		if(holdVal != 0)
			menuHori = Mathf.Sign(holdVal);
	}
	
	menuState += menuHori;
	if(menuState >= 5)
		menuState = 0;
	
	if(menuState < 0)
		menuState = 4;
	
	
}

var itemPercent : float = 0.161f;

function OnGUI () 
{
	
	GUI.skin = Resources.Load("OptionsMenu/Options",GUISkin);
	GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 40f;
	
	if(savedRect.width != Screen.width || savedRect.height != Screen.height)
		LoadMenu();
	
	GUI.BeginGroup(safeRect);
	//Draw the Bar at the top of the Screen
	GUI.DrawTexture(Rect(0,0,safeRect.width,topBarHeight),topBar);
	
	//Draw the options of the topBar
	
	var edgeWidth : float = (safeRect.width * 0.099f);
	GUI.DrawTexture(Rect(0,topBarHeight/10f,topBarHeight - 10,topBarHeight - 10),leftArrow);
	GUI.DrawTexture(Rect(safeRect.width - (topBarHeight*0.8),topBarHeight/10f,topBarHeight - 10,topBarHeight - 10),rightArrow);
	
	if(im.c != null && im.c.length > 0)
	{
		if(im.c[0].inputName != "Key_")
		{
			GUI.Label(Rect(0,0,edgeWidth,topBarHeight),"     LB");
			GUI.Label(Rect(safeRect.width - edgeWidth,0,edgeWidth,topBarHeight),"RB     ");
		}
		else
		{
			GUI.Label(Rect(0,0,edgeWidth,topBarHeight),"      Q");
			GUI.Label(Rect(safeRect.width - edgeWidth,0,edgeWidth,topBarHeight),"E      ");
		}
	
	}
	
	var topItems : String[] = ["Game","Graphics","Sound","Controls","Extras"];
	
	for(var i : int = 0; i < topItems.Length; i++)
	{
		if(i == menuState)
			GUI.skin.label.normal.textColor = selectedColour;
		else
			GUI.skin.label.normal.textColor = Color.white;

		GUI.Label(Rect(edgeWidth + ((safeRect.width*itemPercent) * i),0,safeRect.width*itemPercent,topBarHeight),topItems[i]);
	}
	
	GUI.skin.label.normal.textColor = Color.white;
	
	var options : String[];
	 
	switch(menuState)
	{
		case State.game:
		
			options = ["Reset All Game Data"];
			
		break;
		case State.graphics:
		
			options = ["Fullscreen","Resolution","Quality"];
		
		break;
		case State.audio:
		
			options = ["Master Volume","Music Volume","SFX Volume","Subtitles"];
		
		break;
		case State.controls:
		
			options = ["Coming Soon..."];
		
		break;
		case State.extras:
		
			options = ["Cheat Codes"];
		
		break;
	}
	
	var itemHeight : int = Screen.height - topBarHeight - 10;
	
	for(i = 0; i < options.Length; i++)
	{
		if(i == selectedOption)
			GUI.skin.label.normal.textColor = selectedColour;
		else
			GUI.skin.label.normal.textColor = Color.white;

		GUI.Label(Rect(0, topBarHeight + (i *(itemHeight/8f)), safeRect.width,(itemHeight/8f)),options[i]);
	}
	
	GUI.skin.label.normal.textColor = Color.white;

	
	GUI.EndGroup();
}