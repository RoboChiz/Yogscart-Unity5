#pragma strict
import UnityEngine.Networking;
import UnityEngine.Networking.NetworkSystem;
import System;

public class DailyChallenge extends NetworkManager
{
	var GUIManager : MonoBehaviour;	
}

public class DailyChallengeServer extends DailyChallenge 
{	
	// called when a client connects 
	public virtual function OnServerConnect(conn : NetworkConnection)
	{
		//Tell the user the Current Server Date
		var dateMsg = new MasterMsgTypes.DateMessage(System.DateTime.Now.Date.ToString());
		Debug.Log("Created Date Message!");
		conn.Send(MasterMsgTypes.DateMessageID,dateMsg);
		
	};

	// called when a client disconnects
	public virtual function OnServerDisconnect(conn : NetworkConnection)
	{
	    NetworkServer.DestroyPlayersForConnection(conn);
	};
	
	// called when a new player is added for a client
	public virtual function OnServerAddPlayer(conn : NetworkConnection, playerControllerId: short)
	{
	};
}

public class DailyChallengeClient extends DailyChallenge 
{
	// called when connected to a server
	public virtual function OnClientConnect(conn : NetworkConnection)
	{
	    ClientScene.Ready(conn);
	    ClientScene.AddPlayer(0);
	    
	    //Setup Event Handlers
		client.RegisterHandler(MasterMsgTypes.DateMessageID, DateRecieved);
	    
	    GUIManager.StartCoroutine("OnConnected");
	    
	};

	// called when disconnected from a server
	public virtual function OnClientDisconnect(conn : NetworkConnection)
	{
		GUIManager.StartCoroutine("OnDisconnected");
	    StopClient();
	};

	// called when a network error occurs
	public virtual function OnClientError(conn: Networking.NetworkConnection, errorCode: int)
	{
		var errMsg : NetworkError = errorCode;
		GUIManager.StartCoroutine("OnClientError",errMsg.ToString());
	};
	
	function DateRecieved(netMsg : NetworkMessage)
	{
		Debug.Log("Recieved Date Message!");
		var msg : MasterMsgTypes.DateMessage = new MasterMsgTypes.DateMessage();
		netMsg.ReadMessage.<MasterMsgTypes.DateMessage>(msg);
		
		if(msg.dateString == System.DateTime.Now.Date.ToString())
		{
			Debug.Log("Player " + netMsg.conn.connectionId + " has already downloaded today's challenge!");
		}
	}
}