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
function QuizNewRacer () {

GetComponent.<NetworkView>().RPC("RecievedNewRacer",RPCMode.Server,PlayerPrefs.GetString("playerName","Player"),0,0,0,0);//Add support for Character Select

}

		