#pragma strict

public class DailyChallenge extends NetworkManager
{
	var GUIManager : MonoBehaviour;	
}

static public class MasterMsgTypes
{
	
	//Server to client IDs
	static public function get ChallengeMessageID() : short {return challengeMessageID;};  
    static public function set ChallengeMessageID(value : short) {}; //Creates a constant short
    static private var challengeMessageID : short = 150;
    
    static public function get RankingMessageID() : short {return rankingMessageID;};  
    static public function set RankingMessageID(value : short) {}; //Creates a constant short
    static private var rankingMessageID : short = 151;
    
    static public function get TimeTrialMessageID() : short {return timeTrialMessageID;};  
    static public function set TimeTrialMessageID(value : short) {}; //Creates a constant short
    static private var timeTrialMessageID : short = 152;
    
    //Client to Server IDs
    static public function get ScoreMessageID() : short {return scoreMessageID;};  
    static public function set ScoreMessageID(value : short) {}; //Creates a constant short
    static private var scoreMessageID : short = 160;
	
	//Message used to tell client what todays challenge is
	public class ChallengeMessage extends MessageBase 
	{
		
		public var dateString : String;
		public var challengeName : String;
		public var challengeDescription : String;
		public var trackScene : String; //Scene that challenge takes place on
		
		public var bestPlayers : String;
		
		public function ChallengeMessage(pDate : String, cName : String,cDescription : String, bPlayers : PlayerRank[])
		{
			dateString = pDate;
			challengeName = cName;
			challengeDescription = cDescription;
			
			bestPlayers = "";
			
			if(bPlayers != null)
			{
				for(var i : int = 0; i < bPlayers.length; i++)
				{
					bestPlayers += bPlayers[i].ToString() + "?";
				}
			}
		}
		
		public function ChallengeMessage(){}; //Blank Constructor for read Message
		
		/*function GetDateString(){return dateString;};
		function GetChallengeName(){return challengeName;};
		function GetBestTen(){return bestTen;};*/

	}
	
	//Message to tell server what the player scored in the challenge
	public class ScoreMessage extends MessageBase 
	{
		public var playerName : String;
		public var playerScore : String;
		
		public function ScoreMessage(name : String,score : String){playerName = name;playerScore = score;};
		public function ScoreMessage(){};
		
	}
	
	//Message to tell Client what rank they are on for today
	public class RankingMessage extends MessageBase 
	{
		public var yourRank : float;
		public var totalPlayers : float;
		
		public function RankingMessage(rank : float, total : float){yourRank = rank;totalPlayers = total;};
		public function RankingMessage(){};
		
	}
	
	public class PlayerRank 
	{
	
		private var playerName : String;
		private var playerScore : String;
		public var playerRankScore : float;
		
		function PlayerRank(name : String, score : String, rank : float)
		{
			playerName = name;
			playerScore = score;
			playerRankScore = rank;
		}
		
		public function GetName(){return playerName;};
		public function GetScore(){return playerScore;};
		
		public function ToString()
		{
			var returnString : String;
			returnString = playerName + ";" + playerScore;
			return returnString;
		}
		
		public function ReadString(str : String)
		{
			var splitUp : String[] = str.Split(";"[0]);
			
			playerName = splitUp[0];
			playerScore = splitUp[1];
		}
		
	}
}