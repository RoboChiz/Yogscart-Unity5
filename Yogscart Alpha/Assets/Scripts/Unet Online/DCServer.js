#pragma strict
import UnityEngine.Networking;
import UnityEngine.Networking.NetworkSystem;
import System;
import MasterMsgTypes;

public class DailyChallengeServer extends DailyChallenge 
{	

	private var todayChallenge : ChallengeMessage;
	private var todayScore : PlayerRank[];
	
	//Called when monobehaviour is created
	public function Start()
	{
		NetworkServer.RegisterHandler(ScoreMessageID,OnRecieveScore);
		UpdateChallenge();
	}
	
	// called when a client connects 
	public virtual function OnServerConnect(conn : NetworkConnection)
	{
		//Tell the user the Current Server Date
		var challengeMsg = todayChallenge;
		//Debug.Log("Created Challenge Message!");
		//Debug.Log("Date:" + challengeMsg.dateString);
		//Debug.Log("Name:" + challengeMsg.challengeName);
		//Debug.Log("Players:" + challengeMsg.bestPlayers);
		
		conn.Send(ChallengeMessageID,challengeMsg);
		
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
	
	public function OnRecieveScore(netMsg : NetworkMessage)
	{
		var msg : ScoreMessage = new ScoreMessage();
		msg = netMsg.ReadMessage.<ScoreMessage>();
		CheckScore(msg,netMsg.conn);
	}
	
	//Check that Score is valid for challenge type 
	private function CheckScore(msg : ScoreMessage,conn : NetworkConnection)
	{
	
		var playerRankScore : float;
		try
		{
			playerRankScore = TimeManager.Parse(msg.playerScore);
		
			//Perform Checks on Data for legitimacy
			if(playerRankScore > 60 && playerRankScore < 600)
			{
				//Check that message is valid
				AddScore(msg,playerRankScore,conn);
			}
		
		}catch(err){}
	}
	
	//Add Score to Todays Score, scoreFloat is the score value converted to a number to make sorting easier
	private function AddScore(msg : ScoreMessage, scoreFloat : float,conn : NetworkConnection)
	{
		//First Score of the Day
		if(todayScore == null || todayScore.length == 0)
		{
			todayScore = new PlayerRank[1];
			todayScore[0] = new PlayerRank(msg.playerName,msg.playerScore,scoreFloat);
		}
		else
		{
			//Perform a binary search on existing records
			var addPoint : long = GetPoint(scoreFloat);
			
			var holder = todayScore;
			
			todayScore = new PlayerRank[holder.length + 1];
			
			var counter : long;
			
			for(var i : long = 0; i < todayScore.length; i++)
			{
				if(i != addPoint)
				{
					todayScore[i] = holder[counter];
					counter++;
				}
				else
					todayScore[addPoint] = new PlayerRank(msg.playerName,msg.playerScore,scoreFloat);
			}
		}
		
		//Update Challenge
		UpdateChallenge();
		
		var nRank : RankingMessage = new RankingMessage(addPoint,todayScore.length);
		conn.Send(RankingMessageID,nRank);
		conn.Send(ChallengeMessageID,todayChallenge);
		
	}
	
	private function GetPoint(val : float) : long
	{
		for(var i : int = 0; i < todayScore.length; i++)
		{
			if(val <= todayScore[i].playerRankScore)
				return i;
		}
		
		return todayScore.length;
	}
	
	private function UpdateChallenge()
	{
	
		var todayDate : String = System.DateTime.Now.Date.ToString("dd/MM/yyyy");
		
		if(todayChallenge != null)
		{
			if(todayChallenge.dateString != todayDate)
			{
				todayScore = new PlayerRank[0];
			}
		}
		
		var bestPlayers : PlayerRank[];
		
		if(todayScore != null)
		{
			bestPlayers = new PlayerRank[Mathf.Clamp(10,1,todayScore.length)];
		
			for(var i : int = 0; i < bestPlayers.length; i++)
			{
				bestPlayers[i] = todayScore[i];
				//Debug.Log("Player " + i.ToString() + ":" + bestPlayers[i].ToString());					
			}			
		}
		
		todayChallenge = new ChallengeMessage(todayDate,"Test Challenge #1","Race around Sjin's Farm as fast as you can!",bestPlayers);
		todayChallenge.trackScene = "Downhill Sprint";
	}

}