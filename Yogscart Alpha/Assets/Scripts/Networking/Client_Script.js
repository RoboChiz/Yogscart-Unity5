#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var km : KartMaker;

var otherRacers : GameObject[];
var myRacer : Racer;

function Awake()
{
	gd = transform.GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	km = transform.GetComponent(KartMaker);
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
	
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).online = true;
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).enabled = true;

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
	
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).online = false;

	if(!GameObject.Find("Menu Holder").GetComponent(CharacterSelect).cancelled)
	{
		GetComponent.<NetworkView>().RPC("RecievedNewRacer",RPCMode.Server,PlayerPrefs.GetString("playerName","Player"),gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel);//Add support for Character Select

		myRacer = new Racer(true,-1,gd.currentChoices[0].character,gd.currentChoices[0].hat,gd.currentChoices[0].kart,gd.currentChoices[0].wheel,-1);
		myRacer.name = PlayerPrefs.GetString("playerName","Player");
	}
	GameObject.Find("Menu Holder").GetComponent(CharacterSelect).ResetEverything();
	
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

var objs = GameObject.FindObjectsOfType(kartScript);
for(var i : int = 0; i < objs.Length; i++)
{
	DestroyImmediate(objs[i].gameObject);
}

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
	
	var sm = transform.GetChild(0).GetComponent(Sound_Manager);
	sm.PlayMusic(Resources.Load("Music & Sounds/YC Main Theme",AudioClip));
	
	myRacer.finished = false;

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

@RPC
function SpawnMe(name : String, kart : int, wheel : int, character : int, hat : int,id : NetworkViewID, pos : Vector3, rot : Quaternion)
{
	
	var newKart = km.SpawnKart(KartType.Spectator,Vector3(0,1000,0),Quaternion.identity,kart,wheel,character,hat);
	
	newKart.FindChild("Canvas").GetChild(0).GetComponent(UI.Text).text = name;
	
	newKart.gameObject.AddComponent(NetworkView);
	newKart.GetComponent.<NetworkView>().viewID = id;
	newKart.GetComponent.<NetworkView>().stateSynchronization = NetworkStateSynchronization.Off;
	newKart.tag = "Spectated";
	DontDestroyOnLoad(newKart);
	
	newKart.position = pos;
	newKart.rotation = rot;
	
	var copy = new Array();
	
	if(otherRacers != null)
		copy = otherRacers;
	
	copy.Add(newKart.gameObject);
	
	otherRacers = copy;
	
}

@RPC
function SpawnMyKart(typeInt : int, pos : Vector3, rot : Quaternion)
{

	var type : KartType = typeInt;

	myRacer.ingameObj = km.SpawnKart(type,pos,rot,myRacer.kart,myRacer.wheel,myRacer.character,myRacer.hat);
	
	//Add Camera
	var IngameCam = Instantiate(Resources.Load("Prefabs/Cameras",Transform),pos,Quaternion.identity);
	IngameCam.name = "InGame Cams";
	
	myRacer.ingameObj.GetComponent(kartInput).InputNum = 0;
	myRacer.ingameObj.GetComponent(kartInput).camLocked = true;
	myRacer.ingameObj.GetComponent(kartInput).frontCamera = IngameCam.GetChild(1).GetComponent.<Camera>();
	myRacer.ingameObj.GetComponent(kartInput).backCamera = IngameCam.GetChild(0).GetComponent.<Camera>();
	
	IngameCam.GetChild(1).tag = "MainCamera";

	IngameCam.GetChild(0).transform.GetComponent(Kart_Camera).target = myRacer.ingameObj;
	IngameCam.GetChild(1).transform.GetComponent(Kart_Camera).target = myRacer.ingameObj;
	myRacer.cameras = IngameCam;
	
	var id = Network.AllocateViewID();
	
	myRacer.ingameObj.gameObject.AddComponent(NetworkView);
	myRacer.ingameObj.GetComponent.<NetworkView>().viewID = id;
	myRacer.ingameObj.GetComponent.<NetworkView>().stateSynchronization = NetworkStateSynchronization.Off;
		//SetUpCameras
	var copy = new Array();
	copy.Push(IngameCam.GetChild(0).GetComponent.<Camera>());
	copy.Push(IngameCam.GetChild(1).GetComponent.<Camera>());

	myRacer.ingameObj.GetComponent(kartInfo).cameras = copy;
	
	myRacer.ingameObj.GetComponent(kartUpdate).sending = true;
	
	transform.GetComponent.<NetworkView>().RPC("SpawnMe",RPCMode.OthersBuffered,myRacer.name,myRacer.kart,myRacer.wheel,myRacer.character,myRacer.hat,id,pos,rot);	
	
}

