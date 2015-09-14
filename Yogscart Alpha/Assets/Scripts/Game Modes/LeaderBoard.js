#pragma strict
import System.Collections.Generic; //Cause lazy
import System.Linq; //Cause lazy

private var gd : CurrentGameData;

var hidden : boolean;
var slideTime : float = 0.5f;
var sideAmount : float = 0; //Used to make gui slide off screen

private var movingGUI : boolean = false;

enum LBType{Points,NoPoints,Sorted,TimeTrial,Tournament};
var state : LBType = LBType.Points;

var Racers : List.<DisplayRacer>;

//Textures that can be loaded
private var BoardTexture : Texture2D;

private var pointCount : int;
private var secondCount : float;

function Awake () 
{
	//Load Libaries
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	LoadAssets();
	
	if(hidden)
		sideAmount = Screen.width/2f + 20f;
	else
		sideAmount = 0f; 
		
	/*Debug Tests
	for(var i : int = 0; i < 12; i++)
	{
		var name : int = Random.Range(0,2);
		if(name == 0)
			AddRacer("Test",i%gd.Characters.Length,Random.Range(0,50),0);	
		else
			AddRacer("",i%gd.Characters.Length,Random.Range(0,50),0);	
	}	
	//Debug Tests*/

}

function StartLeaderBoard()
{
	hidden = false;
	state = LBType.Points;
	pointCount = 0;
	secondCount = 0f;
}

function StartTimeTrial()
{
	hidden = false;
	state = LBType.TimeTrial;
}

function StartOnline()
{
	hidden = false;
	state = LBType.NoPoints;
}

function SecondStep()
{
	var holder = SortingScript.CalculatePoints(Racers);
	
	for(var j = 0; j < holder.Count; j++)
	{
		ChangePosition(holder[j],j);
	}
		
	Debug.Log("Sorted!");
	Debug.Log("The best human Racer is " + BestHuman());
	state = LBType.Sorted;
}

function ResetRacers()
{
	Racers = new List.<DisplayRacer>();
	hidden = true;
}

function LoadAssets()
{
	BoardTexture = Resources.Load("UI Textures/GrandPrix Positions/Backing",Texture2D);
}

function OnGUI () 
{
	
	GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);
	GUI.color = Color.white;
	
	var optionSize = Screen.height/16f;
	var BoardRect : Rect = Rect(sideAmount + Screen.width/2f,optionSize,Screen.width/2f  - optionSize,(optionSize)*14f);
	GUI.DrawTexture(BoardRect,BoardTexture);
	
	GUI.BeginGroup(BoardRect);
	switch(state)
	{
		case LBType.TimeTrial:
			if(Racers.Count > 0)
			{
				var bestTime = gd.Tournaments[gd.currentCup].Tracks[gd.currentTrack].BestTrackTime; 		
				var playerTime : float = Racers[0].timer;
				
				if(playerTime <= bestTime)
				{
					GUI.Label(Rect(10,10,BoardRect.width - 20,BoardRect.height),"New Best Time!!!");
				}
				else
				{
					GUI.Label(Rect(10,10,BoardRect.width - 20,BoardRect.height),"You Lost!!!");
				}

				GUI.Label(Rect(10,10 + (optionSize),BoardRect.width  - 20,optionSize),"Best Time");
				GUI.Label(Rect(10,10 + 2*(optionSize),BoardRect.width  - 20,optionSize),TimeManager.TimerToString(bestTime));

				GUI.Label(Rect(10,10 + 3*(optionSize),BoardRect.width  - 20,optionSize),"Your Time");
				GUI.Label(Rect(10,10 + 4*(optionSize),BoardRect.width  - 20,optionSize),TimeManager.TimerToString(playerTime));
			}
		break;
		default:
			for(var i : int = 0; i < Racers.Count; i++)
			{
				//Render the position Number
				var PosTexture : Texture2D = Resources.Load("UI Textures/GrandPrix Positions/" + (i+1).ToString(),Texture2D);						
				var Ratio = (optionSize)/PosTexture.height;
				GUI.DrawTexture(Rect(20,(i+1)*optionSize,PosTexture.width * Ratio,optionSize),PosTexture);
				
				var nRacer = Racers[i];
				
				var CharacterIcon = gd.Characters[nRacer.character].Icon;
				GUI.DrawTexture(Rect(20 + (PosTexture.width * Ratio),(nRacer.position+1)*optionSize,(PosTexture.width * Ratio),optionSize),CharacterIcon,ScaleMode.ScaleToFit);
				
				var nameWidth : float = BoardRect.width - 20 -((PosTexture.width * Ratio * 2f));
				var nameRect : Rect = Rect(10 + (PosTexture.width * Ratio * 2f),(nRacer.position+1)*optionSize,nameWidth,optionSize);
				
				if(nRacer.human && nRacer.name != null && nRacer.name != "")
				{
					GUI.Label(nameRect,nRacer.name);
				}
				else
				{
						var NameTexture : Texture2D;
						if(nRacer.human)
							NameTexture = Resources.Load("UI Textures/GrandPrix Positions/" + gd.Characters[nRacer.character].Name + "_Sel",Texture2D);
						else
							NameTexture = Resources.Load("UI Textures/GrandPrix Positions/" + gd.Characters[nRacer.character].Name,Texture2D);
							
						var Ratio2 = (optionSize)/NameTexture.height;
						
						GUI.DrawTexture(nameRect,NameTexture,ScaleMode.ScaleToFit);
				}
				
				if(state != LBType.NoPoints)
				{
					var points : int = nRacer.points;
					if(state == LBType.Points)
					{		
						points -= (15-i);
						points = Mathf.Clamp(points + pointCount,0f,nRacer.points);
					}
					
					var plusVal : int = ((15 - i) - pointCount);
				
					if(plusVal > 0)
						GUI.Label(Rect(BoardRect.width - (PosTexture.width * Ratio * 2f) - 10,(nRacer.position+1)*optionSize,PosTexture.width * Ratio,optionSize),"+ " + plusVal);
					
					GUI.Label(Rect(BoardRect.width - (PosTexture.width * Ratio) - 20,(nRacer.position+1)*optionSize,PosTexture.width * Ratio,optionSize),points.ToString());
				}
			}
		break;
	}
	GUI.EndGroup();
	
	if(state != LBType.TimeTrial && state != LBType.Tournament && pointCount < 15)
	{
		if(state == LBType.Points)
			secondCount += Time.deltaTime;
		else
			secondCount += Time.deltaTime*10f;
			
		while(secondCount >= 0.5f)
		{
			pointCount += 1;
			secondCount -= 0.5f;
		}
		
	}
}

function Update()
{
	if(!movingGUI)
	{
		if(hidden && sideAmount < Screen.width/2f)
			HideGUI();
		if(!hidden && sideAmount > 0)
			ShowGUI();
	}
}

function HideGUI()
{
	movingGUI = true;
	
	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < slideTime)
	{
		sideAmount = Mathf.Lerp(0,Screen.width/2f + 20f,(Time.realtimeSinceStartup - startTime) / slideTime);
		yield;
	}
	sideAmount = Screen.width/2f + 20f;
	
	movingGUI = false;
}

function ShowGUI()
{
	movingGUI = true;

	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < slideTime)
	{
		sideAmount = Mathf.Lerp(Screen.width/2f + 20f,0,(Time.realtimeSinceStartup - startTime) / slideTime);
		yield;
	}
	sideAmount = 0f;
	
	movingGUI = false;
}

function ChangePosition(toChange : DisplayRacer, i : int)
{
	if(toChange.position != i)
	{
		var startTime : float = Time.realtimeSinceStartup;
		var startPosition : float = toChange.position;
		
		while(Time.realtimeSinceStartup - startTime < slideTime)
		{
			toChange.position = Mathf.Lerp(startPosition,i,(Time.realtimeSinceStartup - startTime) / slideTime);
			yield;
		}
		
		toChange.position = i;
		
	}
}

@RPC
function AddLBRacer(name : String, character : int, points : int, timer : float)
{
	var nRacer : DisplayRacer;
	
	if(name == null || name == "")
		nRacer = new DisplayRacer(Racers.Count,character,points,timer);
	else
		nRacer = new DisplayRacer(Racers.Count,name,character,points,timer);
		
	Racers.Add(nRacer);
}

function AddLBRacer(human : boolean, character : int, points : int, timer : float)
{
	var nRacer : DisplayRacer;
	
	nRacer = new DisplayRacer(Racers.Count,human,character,points,timer);
		
	Racers.Add(nRacer);
}

function BestHuman()
{

	var returnVal : int = -1;
	
	if(Racers != null)
	{
		for(var i : int = 0; i < Racers.Count; i++)
		{
			if(Racers[i].human && (returnVal == -1 || (Racers[i].points > Racers[returnVal].points)))
			{
				returnVal = i;
			}
		}
	}
	
	return returnVal;

}

class DisplayRacer
{
	var name : String;
	var character : int;
	var points : int;
	var timer : float;
	var human : boolean;	
	
	var position : float;
	private static var slideTime : float = 0.5f;
	
	//All Human Players should have a name
	function DisplayRacer(po : int, n : String, c : int, p : int, t : float)
	{
		name = n;
		character = c;
		points = p;
		timer = t;
		human = true;	
		position = po;
	}
	
	//All Human Players should have a name
	function DisplayRacer(po : int, h : boolean, c : int, p : int, t : float)
	{
		human = h;
		character = c;
		points = p;
		timer = t;
		position = po;
	}
	
	//AI Racers won't have a name
	function DisplayRacer(po : int, c : int, p : int, t : float)
	{
		name = "AI Racer";
		character = c;
		points = p;
		timer = t;
		human = false;	
		position = po;
	}
		
}

