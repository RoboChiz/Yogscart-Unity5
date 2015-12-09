#pragma strict

var skin : GUISkin;

private var im : InputManager;

private var fadeAlpha : float = 0f;
private var fading : boolean;

private var connectRot : float = 0f;
private var popupText : String = "";

var mm : MainMenu;
var dcc : DailyChallengeClient;
private var todaysChallenge : ChallengeMessage;

static var rank : RankingMessage;
public var scoring : boolean;

enum DailyChallengeState {Connecting,Menu,PopUp};
private var state : DailyChallengeState = DailyChallengeState.Connecting;

private var selectedColor : Color = Color.yellow;

function StartMenu(nSelectColor : Color, mainMenu : MainMenu) {
	
	fading = true;
	
	im = GameObject.Find("GameData").GetComponent(InputManager);
	selectedColor = nSelectColor;
	mm = mainMenu;
	
	state = DailyChallengeState.Connecting;
	popupText = "";
	
	var startTime : float = Time.realtimeSinceStartup;	
	var transitionTime : float = 0.5f;
	
	while(Time.realtimeSinceStartup - startTime < transitionTime)
	{
		fadeAlpha = Mathf.Lerp(0,1,(Time.realtimeSinceStartup - startTime) / transitionTime);
		yield;
	}
	
	fadeAlpha = 1;
	fading = false;
	
	//Connect to Server
	SetupClient();
	
}

function StopMenu() {
	fading = true;
	var startTime : float = Time.realtimeSinceStartup;	
	var transitionTime : float = 0.5f;
	
	while(Time.realtimeSinceStartup - startTime < transitionTime)
	{
		fadeAlpha = Mathf.Lerp(1,0,(Time.realtimeSinceStartup - startTime) / transitionTime);
		yield;
	}
	
	fadeAlpha = 0;
	fading = false;
	
	ResetEverything();
	todaysChallenge = null;
	
	this.enabled = false;
}

function OnGUI () 
{
	if(!scoring)
	{
		//Setup Skin
		GUI.skin = skin;
		GUI.color.a = fadeAlpha;
		
		var chunkSize : float;
		
		if(popupText != "")
			state = DailyChallengeState.PopUp;
		
		switch(state)
		{
			case DailyChallengeState.Connecting:
				chunkSize = Mathf.Min(Screen.width, Screen.height) / 20f;
				GUI.skin.label.fontSize = chunkSize;

				GUI.Label(Rect(10,Screen.height/2f - chunkSize, Screen.width - 20,chunkSize*2f),"Connecting to Yogscart Server...");
				
				var connectionCircle : Texture2D = Resources.Load("UI Textures/Main Menu/Connecting",Texture2D);	
				GUIUtility.RotateAroundPivot (connectRot, Vector2(Screen.width/2f,Screen.height/2f + (chunkSize*2f))); 
				GUI.DrawTexture(Rect(Screen.width/2f - chunkSize, Screen.height/2f + chunkSize,2f*chunkSize,2f*chunkSize),connectionCircle);
				
				connectRot += Time.deltaTime * -50f;
			break;
			case DailyChallengeState.PopUp:
				chunkSize = Mathf.Min(Screen.width, Screen.height) / 20f;
				GUI.skin.label.fontSize = chunkSize;
				
				GUI.Label(Rect(10,Screen.height/2f - chunkSize, Screen.width - 20,chunkSize*2f),popupText);
			break;
			case DailyChallengeState.Menu:
				if(todaysChallenge != null)
				{
					chunkSize = Mathf.Min(Screen.width, Screen.height) / 40f;
					GUI.skin.label.fontSize = chunkSize;
					
					GUI.Label(Rect(10,chunkSize * 2f, Screen.width - 20,chunkSize*2f),"Daily Challenge for " + todaysChallenge.dateString);
					GUI.Label(Rect(10,chunkSize * 4f, Screen.width - 20,chunkSize*2f),todaysChallenge.challengeName);
					GUI.Label(Rect(Screen.width*0.25f,chunkSize * 6f, Screen.width/2f,chunkSize*8f),todaysChallenge.challengeDescription);//4 Lines MAX!
					GUI.Label(Rect(10,chunkSize * 14f, Screen.width - 20,chunkSize*2f),"Best Racers:");
					
					var SplitUp : String[] = todaysChallenge.bestPlayers.Split("?"[0]);	
					if(SplitUp != null)
					{
						for(var i : int = 0; i < SplitUp.length - 1; i++)//Last string is blank
						{
							GUI.Label(Rect(10,(chunkSize*16f) + (i*(chunkSize*2f)),Screen.width - 20, chunkSize*2f),SplitUp[i]);
						}
					}
					
					var nChunkSize = Mathf.Min(Screen.width, Screen.height) / 20f;
					GUI.skin.label.fontSize = nChunkSize;
					
					var startChallengeRect : Rect = Rect((Screen.width/2f) - (Screen.width/8f),chunkSize * 26f, Screen.width/4f,nChunkSize*3f);
					
					if(!fading)
					{
						if(im.MouseIntersects(startChallengeRect))
						{
							GUI.skin.label.normal.textColor = selectedColor;
							if(im.GetClick())
								StartChallenge();
			  			}

			  			
			  			if(im.c[0].GetMenuInput("Submit") != 0)
			  			{
			  				GUI.skin.label.normal.textColor = selectedColor;
			  				StartChallenge();
			  			}
		  			}
		  			
					GUI.Label(startChallengeRect,"Start Challenge");
					GUI.skin.label.normal.textColor = Color.white;
					
				}
			break;
		}
	}
}

function SetupClient()
{
    dcc = gameObject.AddComponent(DailyChallengeClient);
    
    dcc.GUIManager = this;
    
    dcc.networkAddress = "127.0.0.1";
    dcc.networkPort = 25000;
    dcc.StartClient(); 
}

function ResetEverything()
{
	if(dcc != null)
	{
		dcc.StopClient();
		Destroy(dcc);
	}	
}

//When the client connects to the server
function OnConnected()
{
	state = DailyChallengeState.Menu;
	
	if(scoring)
		dcc.SendScore(myName,myScore);
}

private var myName : String;
private var myScore : String;

function SetScore(pName : String,pScore : String)
{
	myName = pName;
	myScore = pScore;
}

//When the client disconnects from the server
function OnDisconnected()
{
	ResetEverything();	
}

function OnClientError(error : String)
{
	popupText = "Error: " + error;
	ResetEverything();
}

function RecievedChallenge(newMsg : ChallengeMessage)
{
	todaysChallenge = newMsg;
	
	CurrentGameData.ChallengeScene = newMsg.trackScene;
	
	if(!scoring)
		ResetEverything();
}

function OnRankRecieved(newMsg : RankingMessage)
{
	rank = newMsg;
	ResetEverything();
	Destroy(this);
}

function StartChallenge()
{
	Debug.Log("Start Challenge");
	mm.StartChallengeTimeTrial();
	
}