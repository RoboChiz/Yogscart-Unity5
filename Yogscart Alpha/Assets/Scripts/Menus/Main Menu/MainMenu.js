#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;

var skin : GUISkin;

var transitioning : boolean;

var transitionTime : float = 0.5f;
private var sideAmount : float = 0;

function Start ()
{
	LoadLibraries();
}

function LoadLibraries()
{
	//Load Libaries
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("GameData").transform.GetChild(0).GetComponent(Sound_Manager);
}

enum MenuState {Main,SinglePlayer,Difficulty,Multiplayer,Options};
var state : MenuState = MenuState.Main;

function OnGUI ()
{
	
	GUI.skin = skin;
	
	var options : String[];
	
	//Setup Options for each Menu
	switch(state)
	{
		case MenuState.Main:
			options = ["Single Player","Multiplayer","Online","Options","Quit"];
		break;
	}
	
	//Draw the GUI inside a group which can dragged off screen for transitions
	var box : Rect = Rect(sideAmount,0,Screen.width/2f,Screen.height);
	GUI.BeginGroup(box);
		
		//If the current menu has options.
		if(options != null && options.Length > 0)
		{
			
			var fontSize : float = (box.height / (options.Length + 1)) - 30;
			var holder : float = GUI.skin.label.fontSize;
			GUI.skin.label.fontSize = fontSize;
			
			for(var i : int = 0; i < options.Length; i++)
			{
				GUI.Label(Rect(20,10 + ((i+2) * fontSize),box.width - 20,fontSize),options[i]);
			}
			
			GUI.skin.label.fontSize = holder;
		}
		
	GUI.EndGroup();

}

function ChangeMenu(newState : MenuState)
{
	if(!transitioning)
	{
		transitioning = true;
		var startTime : float = Time.realtimeSinceStartup;
		
		while(Time.realtimeSinceStartup - startTime < transitionTime)
		{
			sideAmount = Mathf.Lerp(0,-Screen.width/2f,(Time.realtimeSinceStartup - startTime) / transitionTime);
			yield;
		}
		sideAmount = -Screen.width/2f;
		state = newState;
		
		startTime = Time.realtimeSinceStartup;
		
		while(Time.realtimeSinceStartup - startTime < transitionTime)
		{
			sideAmount = Mathf.Lerp(-Screen.width/2f,0,(Time.realtimeSinceStartup - startTime) / transitionTime);
			yield;
		}
		sideAmount = 0f;
		
		transitioning = false;
	}
}