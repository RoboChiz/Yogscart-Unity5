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
static var transitioning : boolean;
static var sliding : boolean;
var pictureTransitioning : boolean;
var forcePictureTransitioning : boolean;

private var freezeBack : boolean;

enum NextState{Hidden,Sliding,Fixed};
private var hideNext : NextState = NextState.Fixed; //Literally used to animate the next button, which disappears on the options menu

var transitionTime : float = 0.5f;
var sideAmount : float = 0;

private var sidePicture : Texture2D;
private var sidePictureAmount : float = 0;

//Options
private var currentFS : boolean;
private var currentResolution : int;
private var currentQuality : int;
private var currentVSync : int = 0;

static var xAmount : float;	

private var hideTitle : boolean;
private var titleAlpha : float = 1f;						
																					
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

enum MenuState {Main,SinglePlayer,Difficulty,CharacterSelect,LevelSelect,Multiplayer,Online,Options,Popup,Credits};
var state : MenuState[];

var popupText : String;
private var currentState : MenuState;

function OnGUI ()
{
	
	GUI.skin = skin;
	GUI.skin.label.fontSize = Mathf.Min(Screen.width, Screen.height) / 20f;
	
	var options : String[];
	
	if(im.c == null || im.c.Length == 0)
	{
		GUI.Label(Rect(Screen.width/2f - Screen.width/4f,Screen.height/2f - 200,Screen.width/2f,400),"Press Start / Enter on your controller of choice");
	}
	else
	{
	
		currentState = state[state.Length-1];
	
		if(!hidden)
		{
			//Lock controls while menus are transitioning
			var submitBool : boolean = false;
			var cancelBool : boolean = im.c[0].GetMenuInput("Cancel") != 0 && !transitioning;	
			
			if(currentState != MenuState.Credits)
				submitBool = (im.c[0].GetMenuInput("Submit") != 0 && !transitioning);
			
				
				
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
		{
			var holder = GUI.color.a;
			GUI.color.a = titleAlpha;

			GUI.DrawTexture(Rect(sidePictureAmount,10,Screen.width/2f - 10,Screen.height-20),sidePicture,ScaleMode.ScaleToFit);
			
			GUI.color.a = holder;
		}
		
		var box : Rect = Rect(sideAmount,0,Screen.width/2f,Screen.height);
		var optionHeight = GUI.skin.label.fontSize + 10;
		
		//Setup Options for each Menu
		switch(currentState)
		{
			case MenuState.Main:
			
				options = ["Single Player","Multiplayer","Online",/*"Daily Challenge",*/"Options","Credits","Quit"];
				im.allowedToChange = true;
				hidden = false;
				nm.enabled = false;
				
				if(sidePicture != null)
				{
					switch(options[currentSelection])
					{	
						case "Multiplayer":
						if(sidePicture.name != "Multiplayer")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/Multiplayer",Texture2D));
						break;
						case "Online":
						if(sidePicture.name != "Online")
								ChangePicture(Resources.Load("UI Textures/New Main Menu/Side Images/Online",Texture2D));
						break;
						case "Options":
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
				im.allowedToChange = true;
				options = ["Tournament","VS Race"];
				
			break;
			case MenuState.Online:
				im.allowedToChange = false;
				hidden = true;
				nm.enabled = true;
			break;
			case MenuState.Options:
				im.allowedToChange = false;
				ForceChangePicture(null);
				
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
															
				options = [fsString,resString,qualityString,vSyncString,mvString,muvString,sfxvString,"Save Changes","Reset Everything"];
			
				//SliderRect
				var masterSlider = Rect(box.width * (3f/4f),20 + (box.height/4f) +(4.25f * optionHeight),box.width - 40,optionHeight);
				sm.MasterVolume = GUI.HorizontalSlider(masterSlider,sm.MasterVolume,0,100);
			
				var musicSlider = Rect(box.width * (3f/4f),20 + (box.height/4f) +(5.25f * optionHeight),box.width - 40,optionHeight);
				sm.MusicVolume = GUI.HorizontalSlider(musicSlider,sm.MusicVolume,0,100);
				
				var sfxSlider = Rect(box.width * (3f/4f),20 + (box.height/4f) +(6.25f * optionHeight),box.width - 40,optionHeight);
				sm.SFXVolume = GUI.HorizontalSlider(sfxSlider,sm.SFXVolume,0,100);
				
				//Old Sliders - Delete Later
				var resolutionSlider = Rect(box.width * (7f/8f),20 + (box.height/4f) +(1.25f * optionHeight),(box.width * (7f/8f)) - 40,optionHeight);
				currentResolution = GUI.HorizontalSlider(resolutionSlider,currentResolution,0,Screen.resolutions.Length - 1);
				
				var qualitySlider = Rect(box.width * (3f/4f),20 + (box.height/4f) +(2.25f * optionHeight),box.width - 40,optionHeight);
				currentQuality = GUI.HorizontalSlider(qualitySlider,currentQuality,0,QualitySettings.names.Length-1);
				
				var vSyncSlider = Rect(box.width * (3f/4f),20 + (box.height/4f) +(3.25f * optionHeight),box.width - 40,optionHeight);
				currentVSync = GUI.HorizontalSlider(vSyncSlider,currentVSync,0,2);
			
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
				
				GUI.Label(Rect(box.x + 40,20 + (box.height/4f),box.width - 20,box.height - 20 - (box.height/4f)),popupText);
				
				
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
		
		if(!freezeBack )
			xAmount = sideAmount;
		else
			xAmount = 0;
		
		if(!hidden)
		{
			if(hideTitle)
			{
				titleAlpha = Mathf.Lerp(titleAlpha,0f,Time.deltaTime*5f);
			}
			else
			{
				titleAlpha = Mathf.Lerp(titleAlpha,1f,Time.deltaTime*5f);
			}
			
			//Render the Game Logo
			holder = GUI.color.a;
			GUI.color.a = titleAlpha;
			
			GUI.DrawTexture(Rect(10,10,box.width - 10,box.height/4f),logo,ScaleMode.ScaleToFit);
			
			GUI.color.a = holder;
			
			var nextTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/nextnew",Texture2D);
			var ratio = (box.width/3f)/nextTexture.width;
			var height : float = nextTexture.height * ratio;
			
			if(currentState != MenuState.Main)
			{
				var backTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/backnew",Texture2D);				
				var backRatio : float = (Screen.width/6f)/backTexture.width;
					
				var backRect : Rect = Rect(xAmount,Screen.height - 10 - (backTexture.height*backRatio),Screen.width/6f,backTexture.height*backRatio);	
				GUI.DrawTexture(backRect,backTexture);
				
				if(!transitioning && im.MouseIntersects(backRect) && im.GetClick())
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
			
			var nextRect = Rect(nextxAmount - (box.width/3f),Screen.height - 10 - height,box.width/3f,height);
			
			GUI.DrawTexture(nextRect,nextTexture);
			
			if(!transitioning && im.MouseIntersects(nextRect) && im.GetClick())
			{					
				submitBool = true;
		 	}	
			
		}
		
		//Draw the GUI inside a group which can dragged off screen for transitions
		GUI.BeginGroup(box);
		
			//If the current menu has options.
			if(options != null && options.Length > 0)
			{
				//Single Player is the longest word in the menu and is 13 characters long			
				for(var i : int = 0; i < options.Length; i++)
				{	
					if(currentSelection == i)
						GUI.skin.label.normal.textColor = selectedColor;
					else
						GUI.skin.label.normal.textColor = Color.white;
						
						
					var labelRect = Rect(40,20 + (box.height/4f) +(i * optionHeight),box.width - 20,optionHeight);
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
			}
			
		GUI.EndGroup();
		
		if(submitBool)
				sm.PlaySFX(Resources.Load("Music & Sounds/SFX/confirm",AudioClip));
		
		//DO Menu Inputs
		switch(currentState)
		{
			case MenuState.Main:
				if(submitBool)
				{
					switch(options[currentSelection])
					{
						case "Single Player":
							ChangeMenu(MenuState.SinglePlayer);
							im.RemoveOtherControllers();
						break;
						case "Multiplayer":
							ChangeMenu(MenuState.Multiplayer);						
						break;
						case "Online":
							gd.difficulty = 1;
							ChangeMenu(MenuState.Online);
							nm.LoadServers();
							im.RemoveOtherControllers();
						break;
						case "Options":
							ForceChangePicture(null);
							LoadLibraries();
							ChangeMenu(MenuState.Options);
						break;
						case "Credits":
							//Credits
							ChangeMenu(MenuState.Credits);
							ChangePicture(null);
							transform.GetComponent(Credits).enabled = true;
							transform.GetComponent(Credits).StartCredits();
						break;
						case "Daily Challenge":
							ChangeMenu(MenuState.Credits);
							ChangePicture(null);
							transform.GetComponent(DailyChallengeMenu).enabled = true;
							transform.GetComponent(DailyChallengeMenu).StartMenu(selectedColor, this);
						break;
						case "Quit":
							Application.Quit();
						break;
					}
				}
			break;
			case MenuState.SinglePlayer:
				if(submitBool)
				{
					switch(options[currentSelection])
					{
						case "Tournament":
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = true;
							gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
							ChangeMenu(MenuState.Difficulty);
						break;
						case "VS Race":
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
							ChangeMenu(MenuState.Difficulty);
						break;
						case "Time Trial":
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.TimeTrial;
							gd.difficulty = 1;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						default:
							freezeBack = false;
							BackState();
						break;
					}
				}
			break;
			case MenuState.Multiplayer:		
				if(submitBool)
				{
					switch(options[currentSelection])
					{
						case "Tournament":
							freezeBack = true;	
							gd.GetComponent(Level_Select).GrandPrixOnly = true;
							gd.GetComponent(RaceLeader).type = RaceStyle.GrandPrix;
							ChangeMenu(MenuState.Difficulty);
						break;
						case "VS Race":
							freezeBack = true;
							gd.GetComponent(Level_Select).GrandPrixOnly = false;
							gd.GetComponent(RaceLeader).type = RaceStyle.CustomRace;
							ChangeMenu(MenuState.Difficulty);
						break;
						default:
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
						break;					
						case resString:
							currentResolution ++;
						break;
						case qualityString:
							currentQuality ++;
						break;
						case vSyncString:
							currentVSync ++;
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
						break;
						case resString:
							currentResolution --;

						break;
						case qualityString:
							currentQuality --;
						break;
						case vSyncString:
							currentVSync --;
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
						case "Save Changes":
							var cr : Resolution = Screen.resolutions[currentResolution];
							Screen.SetResolution(cr.width,cr.height,currentFS);
							QualitySettings.SetQualityLevel(currentQuality,true);
							QualitySettings.vSyncCount = currentVSync;
							popupText = "All data has been saved.";
							ChangeMenu(MenuState.Popup);
						break;
						case "Reset Everything":
							gd.ResetEverything();
							popupText = "All data has been reset.";
							ChangeMenu(MenuState.Popup);
						break;					
						case "Back":							
							BackState();
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
							gd.difficulty = 0;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "100cc":
							gd.difficulty = 1;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "150cc":
							gd.difficulty = 2;
							ChangeMenu(MenuState.CharacterSelect);
							StartCoroutine("StartCharacterSelect");
						break;
						case "Insane":
							gd.difficulty = 3;
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
			case MenuState.Credits:
			
			if(sidePicture != null)
				ChangePicture(null);
			
			break;
		}
	}
}

function ChangeMenu(newState : MenuState)
{
	if(!transitioning)
	{
		transitioning = true;
		
		if(newState == MenuState.Credits)
		{
			hideNext = NextState.Sliding;
			hideTitle = true;
		}
		
		yield Slide(0,-(Screen.width/2f));
		
		var holder = state;
		state = new MenuState[holder.Length+1];
		for(var i : int = 0; i < holder.Length; i++)
		{
			state[i] = holder[i];
		}
		state[state.Length-1] = newState;
		
		if(newState == MenuState.Credits)
			hideNext = NextState.Hidden;
		else
			hideTitle = false;
		
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

function ForceChangePicture(texture : Texture2D)
{

	if(!forcePictureTransitioning)
	{
		StopCoroutine("ChangePicture");
		
		forcePictureTransitioning = true;
		pictureTransitioning = true;
		
		yield PictureSlide(sidePictureAmount,Screen.width);
		
		sidePicture = texture;
		
		yield PictureSlide(Screen.width,Screen.width/2f);
		
		forcePictureTransitioning = false;
		pictureTransitioning = false;
	}
}

function BackState()
{
	if(!transitioning)
	{
		transitioning = true;
		
		sm.PlaySFX(Resources.Load("Music & Sounds/SFX/back",AudioClip));
		
		var optionsChange : boolean = (currentState == MenuState.Credits);
		
		if(currentState == MenuState.Credits)
		{
			if(transform.GetComponent(Credits).enabled)
				transform.GetComponent(Credits).StopCredits();
				
			if(transform.GetComponent(DailyChallengeMenu).enabled)
				transform.GetComponent(DailyChallengeMenu).StopMenu();
		}
		
		if(currentState == MenuState.CharacterSelect && state[state.length - 2] == MenuState.Credits)
			hideTitle = true;
		
		yield Slide(0,-(Screen.width/2f));
		
		var holder = state;
		state = new MenuState[holder.Length-1];
		for(var i : int = 0; i < state.Length; i++)
		{
			state[i] = holder[i];
		}
		if(optionsChange)
		{
			hideNext = NextState.Sliding;
			hideTitle = false;
		}
		
		if(state[state.length - 1] == MenuState.Credits)
			hideTitle = true;

		currentSelection = 0;
			
		yield Slide(-(Screen.width/2f),0);
		
		if(optionsChange)
			hideNext = NextState.Fixed;
		
		transitioning = false;
	}
}


function Slide(start : float, end : float)
{
	sliding = true;
	
	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < transitionTime)
	{
		sideAmount = Mathf.Lerp(start,end,(Time.realtimeSinceStartup - startTime) / transitionTime);
		yield;
	}
	sideAmount = end;
	
	sliding = false;
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

function StartChallengeTimeTrial()
{

	transform.GetComponent(DailyChallengeMenu).enabled = false;

	gd.GetComponent(Level_Select).GrandPrixOnly = false;
	gd.GetComponent(RaceLeader).type = RaceStyle.DailyChallenge;
	gd.difficulty = 1;
	ChangeMenu(MenuState.CharacterSelect);
	StartCoroutine("StartChallengeCharacterSelect");
	
}

function StartChallengeCharacterSelect()
{
		
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled = true;
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).ResetEverything();
	
	hidden = true;
	
	while(GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled)
		yield;
		
	gd.GetComponent(RaceLeader).enabled = true;
	gd.GetComponent(RaceBase).enabled = true;
	
	gd.GetComponent(RaceLeader).StartCoroutine("StartSinglePlayer");
		
	Debug.Log("Done");
}

function CancelChallengeCharacterSelect()
{
	Debug.Log("Backed!");
	StopCoroutine("StartChallengeCharacterSelect");
	hidden = false;
	BackState();
	transform.GetComponent(DailyChallengeMenu).enabled = true;
}