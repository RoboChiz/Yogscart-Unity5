#pragma strict
import UnityEngine.Networking;
import UnityEngine.Networking.NetworkSystem;
import System;
import MasterMsgTypes;

public class DailyChallengeClient extends DailyChallenge 
{
	// called when connected to a server
	public virtual function OnClientConnect(conn : NetworkConnection)
	{
	    ClientScene.Ready(conn);
	    ClientScene.AddPlayer(0);
	    
	    //Setup Event Handlers
		client.RegisterHandler(ChallengeMessageID, ChallengeRecieved);
	    client.RegisterHandler(RankingMessageID, OnRankRecieved);
	    
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
		var passString : String = errMsg.ToString();
		
		if(errorCode == 6)
		{
			passString = "Unable to connect. Try Again Later";
		}
		
		GUIManager.StartCoroutine("OnClientError",passString);
	};
	
	function ChallengeRecieved(netMsg : NetworkMessage)
	{
		Debug.Log("Recieved Date Message!");
		
		var msg : ChallengeMessage = new ChallengeMessage();
		msg = netMsg.ReadMessage.<ChallengeMessage>();
		
		Debug.Log("Date:" + msg.dateString);
		Debug.Log("Name:" + msg.challengeName);
		Debug.Log("Players:" + msg.bestPlayers);
		
		GUIManager.StartCoroutine("RecievedChallenge",msg);
		
	}
	
	function OnRankRecieved(netMsg : NetworkMessage)
	{
		var msg : RankingMessage = new RankingMessage(-1f,0);
		msg = netMsg.ReadMessage.<RankingMessage>();
		GUIManager.StartCoroutine("OnRankRecieved",msg);
	}
	
	function SendScore(name : String,score : String)
	{
		var msg = new ScoreMessage(name,score);
		client.Send(ScoreMessageID,msg);
		
		Debug.Log("Sent Score");
	}
}