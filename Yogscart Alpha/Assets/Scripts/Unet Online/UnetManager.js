#pragma strict
import UnityEngine.Networking;
import UnityEngine.Networking.NetworkSystem;

//Holds information about the current client
var myClient : NetworkClient;

enum NetworkState {Menu,Connecting,Popup,Lobby,Game};
var networkState : NetworkState = NetworkState.Menu;

var ip : String = "127.0.0.1";
var port : int = 25000;
private var portString : String;

private var popupText : String = "";

function OnGUI()
{

	if(popupText != "")
		networkState = NetworkState.Popup;
		
	GUI.Label(Rect(10,10,Screen.width - 20, 20),"NetworkServer: " + NetworkServer.active);
	
	if(myClient != null)
		GUI.Label(Rect(10,30,Screen.width - 20, 20),"NetworkClient: " + myClient.isConnected);
		
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
			if(GUILayout.Button("Host & Play"))
			{
				SetupServer();
                SetupLocalClient();
			}
			
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
			
				if(GUI.Button(Rect(10,130,100,50),"Close Server"))
				{
					
					if(myClient != null)
					{
						myClient.Disconnect();
					}
					
					NetworkServer.Shutdown();
					NetworkServer.Reset();
					networkState = NetworkState.Menu;
				}
			}
			else
			{
				if(GUI.Button(Rect(10,130,100,50),"Disconnect"))
				{
					myClient.Disconnect();
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

// Create a server and listen on a port
function SetupServer()
{
    NetworkServer.Listen(port);
    networkState = NetworkState.Lobby;
}

// Create a client and connect to the server port
function SetupClient()
{
    myClient = new NetworkClient();
 	RegisterHandlers();
    myClient.Connect(ip, port);
    networkState = NetworkState.Connecting;
}

// Create a local client and connect to the local server
function SetupLocalClient()
{
    myClient = ClientScene.ConnectLocalServer();
    RegisterHandlers();     
}

//Registers event handlers, so that server actions effect client
function RegisterHandlers()
{
    myClient.RegisterHandler(MsgType.Connect, OnConnected);    
    myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);
    myClient.RegisterHandler(MsgType.Error, OnError);
}

//When the client connects to the server
function OnConnected()
{
	networkState = NetworkState.Lobby;
}

//When the client disconnects from the server
function OnDisconnected(disMsg : NetworkMessage)
{
	var disconnectMsg : ErrorMessage = disMsg.ReadMessage.<ErrorMessage>();
	var ne : NetworkError = disconnectMsg.errorCode;
	popupText = ne.ToString();
}

//If there was an error, connecting to the server
function OnError(errMsg : NetworkMessage)
{
	var errorMsg = errMsg.ReadMessage.<ErrorMessage>();
	var ne : NetworkError = errorMsg.errorCode;
	popupText = ne.ToString();
}