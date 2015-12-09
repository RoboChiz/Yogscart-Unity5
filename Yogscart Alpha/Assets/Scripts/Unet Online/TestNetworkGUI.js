#pragma strict
import UnityEngine.Networking;
import UnityEngine.Networking.NetworkSystem;
import MasterMsgTypes;

enum NetworkState {Menu,Connecting,Popup,Lobby,Game};
var networkState : NetworkState = NetworkState.Menu;

var ip : String = "127.0.0.1";
var port : int = 25000;
private var portString : String;

private var popupText : String = "";
var dcs : DailyChallengeServer;
var dcc : DailyChallengeClient;

var msg : ChallengeMessage;

var pName : String = "Robo_Chiz";
var pScore : String = "9000";

function OnGUI()
{

	if(popupText != "")
		networkState = NetworkState.Popup;
	
	if(dcs != null)	
		GUI.Label(Rect(10,10,Screen.width - 20, 20),"Server: " + dcs.isNetworkActive);
	
	if(dcc != null)
		GUI.Label(Rect(10,30,Screen.width - 20, 20),"Client: " + dcc.isNetworkActive);
		
	switch(networkState)
	{
		case NetworkState.Menu:
			GUILayout.BeginArea(Rect(10,90,Screen.width/2f - 50,Screen.height - 20));
			
			ip = GUILayout.TextField(ip);
			
			portString = port.ToString();
			portString = GUILayout.TextField(portString);
			int.TryParse(portString,port);
			
			if(GUILayout.Button("Join"))
				 SetupClient();
			if(GUILayout.Button("Host"))
				SetupServer();
			
			GUILayout.EndArea();
		break;
		case NetworkState.Connecting:
			GUI.Label(Rect(10,60,Screen.width - 20, 20),"You are connecting...");
		break;
		case NetworkState.Lobby:
			GUI.Label(Rect(10,60,Screen.width - 20, 20),"You are connected to the lobby!");
			
			if(NetworkServer.active)
			{
				
				GUI.Label(Rect(10,100,Screen.width - 20, 20),"Connections:" + NetworkServer.connections.Count);
			
				if(GUI.Button(Rect(10,120,100,50),"Close Server"))
				{
					dcs.StopServer();
					ResetEverything();	

					networkState = NetworkState.Menu;
				}
			}
			else
			{
				if(GUI.Button(Rect(10,90,100,50),"Disconnect"))
				{
					dcc.StopClient();
					OnDisconnected();	
				}
				
				if(msg != null)
				{
					GUI.Label(Rect(10,150,Screen.width - 20, 20),"Daily Challenge for: " + msg.dateString);
					GUI.Label(Rect(10,170,Screen.width - 20, 20),msg.challengeName);
					GUI.Label(Rect(10,190,Screen.width - 20, 20),"Best Players:");
					
					var SplitUp : String[] = msg.bestPlayers.Split(":"[0]);
					
					if(SplitUp != null)
					{
						for(var i : int = 0; i < SplitUp.length - 1; i++)//Last string is blank
						{
							GUI.Label(Rect(10,210 + (i*20),Screen.width - 20, 20),SplitUp[i]);
						}
					}
						
					pName = GUI.TextField(Rect(10,450,Screen.width-20,20),pName);
					pScore = GUI.TextField(Rect(10,470,Screen.width-20,20),pScore);
					
					if(GUI.Button(Rect(10,490,100,50),"Send Score"))
					{
						dcc.SendScore(pName,pScore);					
					}
					
					
				}
				
				
			}
		break;
		case NetworkState.Popup:
			GUI.Label(Rect(10,60,Screen.width - 20, 20),popupText);
			
			if(GUI.Button(Rect(10,90,100,50),"Okay"))
			{
				popupText = "";
				networkState = NetworkState.Menu;
			}
		break;
	}
}

function ResetEverything()
{
	Destroy(dcs);	
	Destroy(dcc);	
}

// Create a server and listen on a port
function SetupServer()
{
	dcs = gameObject.AddComponent(DailyChallengeServer);
	
	dcs.GUIManager = this;
	
	dcs.networkPort = port;
	dcs.StartServer();
	
    networkState = NetworkState.Lobby;
}

// Create a client and connect to the server port
function SetupClient()
{
    dcc = gameObject.AddComponent(DailyChallengeClient);
    
    dcc.GUIManager = this;
    
    dcc.networkAddress = ip;
    dcc.networkPort = port;
    dcc.StartClient(); 
    
    
    networkState = NetworkState.Connecting;
}

/*Not needed for Challenge Server!!!!!
// Create a local client and connect to the local server
function SetupLocalClient()
{
    myClient = ClientScene.ConnectLocalServer();
    RegisterHandlers();     
}*/

//When the client connects to the server
function OnConnected()
{
	networkState = NetworkState.Lobby;
}

//When the client disconnects from the server
function OnDisconnected()
{
	if(popupText == "")
		popupText = "You were disconnected from the Server.";
		
	ResetEverything();	
}

function OnClientError(error : String)
{
	popupText = "Error: " + error;
	ResetEverything();
}

function RecievedChallenge(newMsg : ChallengeMessage)
{
	msg = newMsg;
}

function OnRankRecieved(newMsg : RankingMessage)
{
	popupText = "You are ranked " + (newMsg.yourRank + 1) + " out of " + newMsg.totalPlayers;
	dcc.StopClient();
	ResetEverything();
}