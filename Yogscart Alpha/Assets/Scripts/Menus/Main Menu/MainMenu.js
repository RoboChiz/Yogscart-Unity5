﻿#pragma strict

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
var pictureTransitioning : boolean;

private var freezeBack : boolean;

enum NextState{Hidden,Sliding,Fixed};
private var hideNext : NextState = NextState.Fixed; //Literally used to animate the next button, which disappears on the options menu

var transitionTime : float = 0.5f;
private var sideAmount : float = 0;

private var sidePicture : Texture2D;
private var sidePictureAmount : float = 0;

//Options
private var currentFS : boolean;
private var currentResolution : int;
private var currentQuality : int;
private var currentVSync : int = 0;

 var changesMade : boolean;

function Start ()
{

	LoadLibraries();
	
	if(menuMusic != null)
		sm.PlayMusic(menuMusic);
		
	state = [MenuState.Main];
	
	ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/0",Texture2D));
	sidePictureAmount = Screen.width/2f;
}

function LoadLibraries()
{
	//Load Libaries
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("GameData").transform.GetChild(0).GetComponent(Sound_Manager);
	nm = GameObject.Find("GameData").GetComponent(Network_Manager);
	
	for(var i : int = 0; i < Screen.resolutions.Length; i++)
		if(Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height)
		{
			currentResolution = i;
			currentFS = Screen.fullScreen;
			break;
		}
	
	currentQuality = QualitySettings.GetQualityLevel();
	currentVSync = QualitySettings.vSyncCount;
}

enum MenuState {Main,SinglePlayer,Difficulty,CharacterSelect,LevelSelect,Multiplayer,Online,Options,Popup};
var state : MenuState[];

var popupText : String;

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
			var submitBool : boolean = (im.c[0].GetMenuInput("Submit") != 0 && !transitioning);
			var cancelBool : boolean = im.c[0].GetMenuInput("Cancel") != 0 && !transitioning;		
				
			var vertical : float = 0;
			var horizontal : float = 0;
			if(!transitioning)
			{
				vertical = im.c[0].GetMenuInput("Vertical");
				
				if(currentState == MenuState.Options && currentSelection <= 3)
					horizontal = im.c[0].GetMenuInput("Horizontal");
				else
					horizontal = im.c[0].GetInput("Horizontal");
				
				if(vertical != 0)
					vertical = Mathf.Sign(vertical);
					
				if(horizontal != 0)
					horizontal = Mathf.Sign(horizontal);
					
			}
			
			if(cancelBool && currentState != MenuState.Main)
			{
				
				if(currentState != MenuState.Difficulty && currentState != MenuState.CharacterSelect )
					freezeBack = false;
					
				BackState();//Go back to the previous state
				
			}
		}
		
		if(!hidden && sidePicture != null)
			GUI.DrawTexture(Rect(sidePictureAmount,10,Screen.width/2f - 10,Screen.height-20),sidePicture,ScaleMode.ScaleToFit);
		
		
		var box : Rect = Rect(sideAmount,0,Screen.width/2f,Screen.height);
		
		//Setup Options for each Menu
		switch(currentState)
		{
			case MenuState.Main:
			
				options = ["Single Player","Multiplayer","Online","Options","Quit"];
				im.allowedToChange = true;
				hidden = false;
				nm.enabled = false;
				
				if(sidePicture != null)
				{
					switch(currentSelection)
					{	
						case 1:
						if(sidePicture.name != "Multiplayer")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/Multiplayer",Texture2D));
						break;
						case 2:
						if(sidePicture.name != "Online")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/Online",Texture2D));
						break;
						case 3:
						if(sidePicture.name != "Options")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/Options",Texture2D));
						break;
						default:
							if(sidePicture.name != "0")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/0",Texture2D));
						break;
					}
				}else{
					ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/0",Texture2D));
				}
				
			break;
			case MenuState.SinglePlayer:
			
				options = ["Tournament","VS Race","Time Trial"];
				im.allowedToChange = false;
				
			break;
			case MenuState.Multiplayer:	
			
				options = ["Tournament","VS Race"];
				
			break;
			case MenuState.Online:
				hidden = true;
				nm.enabled = true;
			break;
			case MenuState.Options:
			
				if(currentResolution >= Screen.resolutions.Length)
					currentResolution = 0;
					
				if(currentResolution < 0)
					currentResolution = Screen.resolutions.Length - 1;
					
				if(currentQuality >= QualitySettings.names.Length)
					currentQuality = 0;
					
				if(currentQuality < 0)
					currentQuality = QualitySettings.names.Length - 1;	
					
				if(currentVSync < 0)
					currentVSync = 2;
					
				if(currentVSync > 2)
					currentVSync = 0;
					
				var fsString : String = "Full Screen - " + currentFS;
				var resString : String = "Resolution - " + Screen.resolutions[currentResolution].width + " x " + Screen.resolutions[currentResolution].height;
				var qualityString : String = "Quality - " + QualitySettings.names[currentQuality];
				var mvString : String = "Master Volume - " + sm.MasterVolume + "%";
				var muvString : String = "Music Volume - " + sm.MusicVolume + "%";
				var sfxvString : String = "SFX Volume - " + sm.SFXVolume + "%";
				
				var vSyncString : String = "VSync - ";
				
				if(currentVSync == 0)
					vSyncString += "Disabled";
				if(currentVSync == 1)
					vSyncString += "Enabled";
				if(currentVSync == 2)
					vSyncString += "Enabled (Double Buffered)";
															
				options = [fsString,resString,qualityString,vSyncString,mvString,muvString,sfxvString,"Reset Everything"];
			
			break;
			case MenuState.Difficulty:
			
				//options = ["50cc - Only for little Babby!","100cc - You mother trucker!","150cc - Oh what big strong muscles!","Insane - Prepare your butts!","Back"];
				if(gd.unlockedInsane)
					options = ["50cc","100cc","150cc","Insane"];
				else
					options = ["50cc","100cc","150cc"];
				
			break;
			case MenuState.Popup:
			
				options = [];
				
				var fontSize : float = (box.width / 13f);
				var holder : float = GUI.skin.label.fontSize;
				GUI.skin.label.fontSize = fontSize;
				
				GUI.Label(Rect(box.x + 40,20 + (box.height/4f),box.width - 20,box.height - 20 - (box.height/4f)),popupText);
				
				GUI.skin.label.fontSize = holder;
				
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
				
				if(!transitioning && im.MouseIntersects(Rect(xAmount,Screen.height - 10 - height,box.width/3f,height)) && im.GetClick())
				{
				
					if(currentState != MenuState.Difficulty && currentState != MenuState.CharacterSelect )
						freezeBack = false;
					
					BackState();//Go back to the previous state
			 	}	 
				
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
				fontSize = (box.width / 15f);
				holder = GUI.skin.label.fontSize;
				GUI.skin.label.fontSize = fontSize;
				
				for(var i : int = 0; i < options.Length; i++)
				{	
					if(currentSelection == i)
						GUI.skin.label.normal.textColor = selectedColor;
					else
						GUI.skin.label.normal.textColor = Color.white;
						
						
					var labelRect = Rect(40,20 + (box.height/4f) +(i * (10+fontSize)),box.width - 20,fontSize);
					GUI.Label(labelRect,options[i]);

					labelRect.x += box.x;
					labelRect.y += box.y;
					
					if(!transitioning && im.MouseIntersects(labelRect))
					{
							currentSelection = i;
							
							if(im.GetClick())
							{
								submitBool = true;
							}
							
					}
					
				}
				
				GUI.skin.label.normal.textColor = Color.white;
				GUI.skin.label.fontSize = holder;
			}
			
		GUI.EndGroup();
		
		//Show Changes Made
		if(changesMade)
			{
				GUI.skin.label.fontSize = fontSize/2f;
				GUI.skin.label.normal.textColor = Color.red;
				
				GUI.Label(Rect(Screen.width/2f - (box.width/4f) ,20 + (box.height/4f),box.width - 20,fontSize),"Changes Made. Confirm?");
				
				GUI.skin.label.normal.textColor = Color.white;
				GUI.skin.label.fontSize = fontSize;
			}
		
		if(submitBool)
				sm.PlaySFX(Resources.Load("Music & Sounds/SFX/confirm",AudioClip));
		
		//DO Menu Inputs
		switch(currentState)
		{
			case MenuState.Main:
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
							nm.LoadServers();
							im.RemoveOtherControllers();
						break;
						case 3:
							LoadLibraries();
							ChangeMenu(MenuState.Options);
						break;
						case 4:
							Application.Quit();
						break;
					}
				}
			break;
			case MenuState.SinglePlayer:
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
			case MenuState.Options:
				if(horizontal > 0)
				{
					switch(options[currentSelection])
					{
						case fsString:
							currentFS = !currentFS;
							changesMade = true;	
						break;					
						case resString:
							currentResolution ++;
							changesMade = true;
						break;
						case qualityString:
							currentQuality ++;
							changesMade = true;
						break;
						case vSyncString:
							currentVSync ++;
							changesMade = true;
						break;
						case mvString:
							sm.MasterVolume++;
							sm.MasterVolume = Mathf.Clamp(sm.MasterVolume,0,100);
						break;
						case muvString:
							sm.MusicVolume++;
							sm.MusicVolume = Mathf.Clamp(sm.MusicVolume,0,100);
						break;	
						case sfxvString:
							sm.SFXVolume++;
							sm.SFXVolume = Mathf.Clamp(sm.SFXVolume,0,100);
						break;		
					}
				}
				
				if(horizontal < 0)
				{
					switch(options[currentSelection])
					{
						case fsString:
							currentFS = !currentFS;
							changesMade = true;
						break;
						case resString:
							currentResolution --;
							changesMade = true;
						break;
						case qualityString:
							currentQuality --;
							changesMade = true;
						break;
						case vSyncString:
							currentVSync --;
							changesMade = true;
						break;
						case mvString:
							sm.MasterVolume--;
							sm.MasterVolume = Mathf.Clamp(sm.MasterVolume,0,100);
						break;
						case muvString:
							sm.MusicVolume--;
							sm.MusicVolume = Mathf.Clamp(sm.MusicVolume,0,100);
						break;	
						case sfxvString:
							sm.SFXVolume--;
							sm.SFXVolume = Mathf.Clamp(sm.SFXVolume,0,100);
						break;	
					}
				}
				
				if(submitBool)
				{
					switch(options[currentSelection])
					{
						case mvString:
						break;
						case muvString:
						break;
						case sfxvString:
						break;
						case "Reset Everything":
							gd.ResetEverything();
							popupText = "All data has been reset.";
							ChangeMenu(MenuState.Popup);
						break;
						
						case "Back":							
							BackState();
						break;
						default:
							var cr : Resolution = Screen.resolutions[currentResolution];
							Screen.SetResolution(cr.width,cr.height,currentFS);
							QualitySettings.SetQualityLevel(currentQuality,true);
							QualitySettings.vSyncCount = currentVSync;
							changesMade = false;
						break;
					}
				}
			
			break;
			case MenuState.Difficulty:
			
				if(submitBool)
				{
					switch(options[currentSelection])
					{
						case "50cc":
							gd.Difficulty = 0;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "100cc":
							gd.Difficulty = 1;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "150cc":
							gd.Difficulty = 2;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "Insane":
							gd.Difficulty = 3;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "Back":
							BackState();
						break;
					}
				}
			
			break;
			case MenuState.Popup:
			
				if(submitBool)
				{
					BackState();
				}
				
			break;
		}
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

function ChangePicture(texture : Texture2D)
{
	if(!pictureTransitioning)
	{
		pictureTransitioning = true;
		
		yield PictureSlide(Screen.width/2f,Screen.width);
		
		sidePicture = texture;
		
		yield PictureSlide(Screen.width,Screen.width/2f);
		
		pictureTransitioning = false;
	}
}

function BackState()
{
	if(!transitioning)
	{
		transitioning = true;
		
		sm.PlaySFX(Resources.Load("Music & Sounds/SFX/back",AudioClip));
		
		var optionsChange : boolean = (state[state.Length-1] == MenuState.Options);
		changesMade = false;
		
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

function PictureSlide(start : float, end : float)
{
	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < transitionTime)
	{
		sidePictureAmount = Mathf.Lerp(start,end,(Time.realtimeSinceStartup - startTime) / transitionTime);
		yield;
	}
	sidePictureAmount = end;
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