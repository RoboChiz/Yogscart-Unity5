#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;

function Awake()
{
	gd = transform.GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
}

function Update()
{

var cancelInput : float = im.c[0].GetMenuInput("Cancel");
var cancelBool = (cancelInput != 0);

if(cancelBool)
{
Network.Disconnect();
}


}

@RPC
function QuizNewRacer () 
{

var timeout : int;

while(Application.loadedLevelName != "Lobby")
{
	if(timeout >= 10)
		return;
		
	timeout ++;
	yield WaitForSeconds(0.5f);
}

GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled = true;
GameObject.Find("Menu Holder").GetComponent(CharacterSelect).ResetEverything();

while(true)
{

	if(GameObject.Find("Menu Holder") != null)
	{
		if(!GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled)
			break;
	}
	else
	{
		return;
	}	

	yield;
}

GetComponent.<NetworkView>().RPC("RecievedNewRacer",RPCMode.Server,PlayerPrefs.GetString("playerName","Player"),gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel);//Add support for Character Select

}

@RPC
function LoadNetworkLevel(level : String, levelPrefix : int){

gd.BlackOut = true;

if(transform.GetComponent(Countdown) != null){
Debug.Log("BURN IT!");
Destroy(transform.GetComponent(Countdown));
}

if(transform.GetComponent(VotingScreen) != null){
Debug.Log("BURN IT!");
Destroy(transform.GetComponent(VotingScreen));
}

if(transform.GetComponent(Level_Select) != null){
Debug.Log("BURN IT!");
Destroy(transform.GetComponent(Level_Select));
}

if(transform.GetComponent(Network_Manager).SpectatorCam != null)
Destroy(transform.GetComponent(Network_Manager).SpectatorCam);

yield WaitForSeconds(0.6);

Network.SetSendingEnabled(0, false);    
Network.isMessageQueueRunning = false;
Network.SetLevelPrefix(levelPrefix);
Application.LoadLevel(level);
Network.isMessageQueueRunning = true;
Network.SetSendingEnabled(0, true);

for (var go in FindObjectsOfType(GameObject))
go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver); 

yield;
yield;

if(level == "Lobby"){

	if(Network.isServer)
		transform.GetComponent(Host_Script).serverType = ServerState.Lobby;
		
	transform.GetComponent(Network_Manager).state = ServerState.Lobby;

}

yield WaitForSeconds(0.6);

gd.BlackOut = false;

}

@RPC
function Countdowner(time : int){

	if(transform.GetComponent(Countdown) != null){
		Debug.Log("BURN IT!");
		Destroy(transform.GetComponent(Countdown));
	}

	yield;

	gameObject.AddComponent(Countdown);

	transform.GetComponent(Countdown).cdTime = time;
	transform.GetComponent(Countdown).StartCountDown();

}
