#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;
private var nm : Network_Manager;

var skin : GUISkin;

var logo : Texture2D;
var menuMusic : AudioClip;

var selectedColor : Color = Color.yellow;

private var currentSelection : int = 0;

var hidden : boolean;
var transitioning : boolean;

private var freezeBack : boolean;

enum NextState{Hidden,Sliding,Fixed};
private var hideNext : NextState = NextState.Fixed; //Literally used to animate the next button, which disappears on the options menu

var transitionTime : float = 0.5f;
private var sideAmount : float = 0;

function Start ()
{

	LoadLibraries();
	
	if(menuMusic != null)
		sm.PlayMusic(menuMusic);
		
	state = [MenuState.Main];
	
}

function LoadLibraries()
{
	//Load Libaries
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("GameData").transform.GetChild(0).GetComponent(Sound_Manager);
	nm = GameObject.Find("GameData").GetComponent(Network_Manager);
}

enum MenuState {Main,SinglePlayer,Difficulty,CharacterSelect,LevelSelect,Multiplayer,Online,Options};
var state : MenuState[];


function OnGUI ()
{
	
	GUI.skin = skin;
	
	var options : String[];
	
	if(im.c == null || im.c.Length == 0)
	{
		GUI.Label(Rect(Screen.width/2f - Screen.width/4f,Screen.height/2f - 200,Screen.width/2f,400),"Press Start / Enter on your controller of choice");
	}
	else
	{
	
		var currentState = state[state.Length-1];
	
		if(!hidden)
		{
			//Lock controls while menus are transitioning
			var submitBool : boolean = im.c[0].GetMenuInput("Submit") != 0 && !transitioning;
			var cancelBool : boolean = im.c[0].GetMenuInput("Cancel") != 0 && !transitioning;
			
			var vertical : float = 0;
			if(!transitioning)
			{
				vertical = im.c[0].GetMenuInput("Vertical");
				
				if(vertical != 0)
					vertical = Mathf.Sign(vertical);
					
			}
			
			if(cancelBool && currentState != MenuState.Main)
			{
				
				if(currentState != MenuState.Difficulty && currentState != MenuState.CharacterSelect )
					freezeBack = false;
					
				BackState();//Go back to the previous state
				
			}
		}
		
		//Setup Options for each Menu
		switch(currentState)
		{
			case MenuState.Main:
			
				options = ["Single Player","Multiplayer","Online","Options","Quit"];
				im.allowedToChange = true;
				hidden = false;
				nm.enabled = false;
				
				if(submitBool)
				{
					switch(currentSelection)
					{
						case 0:
							ChangeMenu(MenuState.SinglePlayer);
							im.RemoveOtherControllers();
						break;
						case 1:
							ChangeMenu(MenuState.Multiplayer);
						break;
						case 2:
							ChangeMenu(MenuState.Online);
							im.RemoveOtherControllers();
						break;
						case 3:
							ChangeMenu(MenuState.Options);
						break;
						case 4:
							Application.Quit();
						break;
					}
				}
				
			break;
			case MenuState.SinglePlayer:
			
				options = ["Tournament","VS Race","Time Trial","Back"];
				im.allowedToChange = false;
				
				if(submitBool)
				{
					switch(currentSelection)
					{
						case 0:
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = true;
							gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
							ChangeMenu(MenuState.Difficulty);
						break;
						case 1:
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
							ChangeMenu(MenuState.Difficulty);
						break;
						case 2:
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.TimeTrial;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case 3:
							freezeBack = false;
							BackState();
						break;
					}
				}
				
			break;
			case MenuState.Multiplayer:	
			
				options = ["Tournament","VS Race","Back"];
				
				if(submitBool)
				{
					switch(currentSelection)
					{
						case 0:
							freezeBack = true;	
							gd.GetComponent(Level_Select).GrandPrixOnly = true;
							gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
							ChangeMenu(MenuState.Difficulty);
						break;
						case 1:
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
							ChangeMenu(MenuState.Difficulty);
						break;
						case 2:
							freezeBack = false;
							BackState();
						break;
					}
				}
				
			break;
			case MenuState.Online:
				hidden = true;
				nm.enabled = true;
			break;
			case MenuState.Options:
			
				options = ["Back"];
				
				if(submitBool)
				{
					switch(currentSelection)
					{
						case 0:							
							BackState();
						break;
					}
				}
			
			break;
			case MenuState.Difficulty:
			
				options = ["50cc - Only for little Babby!","100cc - You mother trucker!","150cc - Oh what big strong muscles!","Insane - Prepare your butts!","Back"];
				
				if(submitBool)
				{
					switch(currentSelection)
					{
						case 0:
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case 1:
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case 2:
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case 3:
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case 4:
							BackState();
						break;
					}
				}
				
			break;
		}
		
		//Keyboard controls
		if(options != null)
		{
			currentSelection -= vertical;
			
			if(currentSelection < 0)
				currentSelection = options.Length -1;
				
			if(currentSelection >= options.Length)
				currentSelection = 0;
		}
		
		var box : Rect = Rect(sideAmount,0,Screen.width/2f,Screen.height);
		
		if(!hidden)
		{
			//Render the Game Logo
			GUI.DrawTexture(Rect(10,10,box.width - 10,box.height/4f),logo,ScaleMode.ScaleToFit);
			
			var nextTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/nextnew",Texture2D);
			var ratio = (box.width/3f)/nextTexture.width;
			var height : float = nextTexture.height * ratio;
			
			if(currentState != MenuState.Main)
			{
				var backTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/backnew",Texture2D);
				var xAmount : float;
				
				if(!freezeBack )
					xAmount = sideAmount;
				else
					xAmount = 0;
					
				GUI.DrawTexture(Rect(xAmount,Screen.height - 10 - height,box.width/3f,height),backTexture);
			}
			
			var nextxAmount : float = Screen.width;
			
			if(hideNext == NextState.Hidden)
				nextxAmount = Screen.width + (Screen.width/2f);
			if(hideNext == NextState.Sliding)
				nextxAmount = Screen.width - sideAmount;
			
			GUI.DrawTexture(Rect(nextxAmount - (box.width/3f),Screen.height - 10 - height,box.width/3f,height),nextTexture);
		}
		
		//Draw the GUI inside a group which can dragged off screen for transitions
		GUI.BeginGroup(box);
		
			//If the current menu has options.
			if(options != null && options.Length > 0)
			{
				//Single Player is the longest word in the menu and is 13 characters long
				var fontSize : float = (box.width / 13f);
				var holder : float = GUI.skin.label.fontSize;
				GUI.skin.label.fontSize = fontSize;
				
				for(var i : int = 0; i < options.Length; i++)
				{	
					if(currentSelection == i)
						GUI.skin.label.normal.textColor = selectedColor;
					else
						GUI.skin.label.normal.textColor = Color.white;
				
					GUI.Label(Rect(40,20 + (box.height/4f) +(i * (10+fontSize)),box.width - 20,fontSize),options[i]);
				}
				
				GUI.skin.label.normal.textColor = Color.white;
				GUI.skin.label.fontSize = holder;
			}
			
		GUI.EndGroup();
	}
}

function ChangeMenu(newState : MenuState)
{
	if(!transitioning)
	{
		transitioning = true;
		
		if(newState == MenuState.Options)
			hideNext = NextState.Sliding;
		
		yield Slide(0,-(Screen.width/2f));
		
		var holder = state;
		state = new MenuState[holder.Length+1];
		for(var i : int = 0; i < holder.Length; i++)
		{
			state[i] = holder[i];
		}
		state[state.Length-1] = newState;
		
		if(newState == MenuState.Options)
			hideNext = NextState.Hidden;
		
		currentSelection = 0;
		
		yield Slide(-(Screen.width/2f),0);
		
		transitioning = false;
	}
}

function BackState()
{
	if(!transitioning)
	{
		transitioning = true;
		
		var optionsChange : boolean = (state[state.Length-1] == MenuState.Options);
		
		yield Slide(0,-(Screen.width/2f));
		
		var holder = state;
		state = new MenuState[holder.Length-1];
		for(var i : int = 0; i < state.Length; i++)
		{
			state[i] = holder[i];
		}
		if(optionsChange)
			hideNext = NextState.Sliding;
		
		currentSelection = 0;
			
		yield Slide(-(Screen.width/2f),0);
		
		if(optionsChange)
			hideNext = NextState.Fixed;
		
		transitioning = false;
	}
}


function Slide(start : float, end : float)
{
	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < transitionTime)
	{
		sideAmount = Mathf.Lerp(start,end,(Time.realtimeSinceStartup - startTime) / transitionTime);
		yield;
	}
	sideAmount = end;
}

function StartCharacterSelect()
{
		
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled = true;
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).ResetEverything();
	
	hidden = true;
	
	while(GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled)
		yield;
		
	ChangeMenu(MenuState.LevelSelect);
	gd.GetComponent(Level_Select).enabled = true;
	gd.GetComponent(Level_Select).hidden = false;
	
	while(gd.GetComponent(Level_Select).enabled)
		yield;
		
	gd.GetComponent(RaceLeader).enabled = true;
	gd.GetComponent(RaceBase).enabled = true;
	
	gd.GetComponent(RaceLeader).StartCoroutine("StartSinglePlayer");
		
	Debug.Log("Done");
}

function CancelCharacterSelect()
{
	
	StopCoroutine("StartCharacterSelect");
	hidden = false;
	BackState();
	
}