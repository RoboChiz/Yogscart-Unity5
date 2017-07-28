using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public static class UnetMessages
{
    //Default Network Messages
    public const short versionMsg = 1002;
    public const short acceptedMsg = 1003;
    public const short clientErrorMsg = 1004;
    public const short playerUpMsg = 1005;
    public const short playerInfoMsg = 1006;
    public const short displayNameUpdateMsg = 1007;
    public const short rejectPlayerUpMsg = 1008;
    public const short forceCharacterSelectMsg = 1009;
    public const short loadGamemodeMsg = 1010;
    public const short timerMsg = 1011;
    public const short readyMsg = 1012;
    public const short countdownMsg = 1013;
    public const short unlockKartMsg = 1014;
    public const short returnLobbyMsg = 1016;
    public const short loadLevelID = 1017;

    //Power Up Messages
    public const short recieveItemMsg = 1018;
    public const short useItemMsg = 1019;
    public const short useShieldMsg = 1020;
    public const short dropShieldMsg = 1021;

    //Network Race Messages
    public const short showLvlSelectMsg = 1102;
    public const short trackVoteMsg = 1103;
    public const short voteListUpdateMsg = 1104;
    public const short startRollMsg = 1105;
    public const short forceLevelSelectMsg = 1106;
    public const short allVoteListMsg = 1107;
    public const short loadLevelMsg = 1108;
    public const short spawnKartMsg = 1109;
    public const short positionMsg = 1110;
    public const short finishRaceMsg = 1111;
    public const short playerFinishedMsg = 1112;
    public const short allPlayerFinishedMsg = 1113;
    public const short leaderboardPosMsg = 1114;
}

//Sent by Client to Server to check if both are running the same version
public class VersionMessage : MessageBase //1002
{
    public string version;
}

//Sent by Server if Client passes Version Check. Contains game info
public class AcceptedMessage : MessageBase //1003
{
    public UnetHost.GameState currentState;
    public bool playerUp;
}

//Sent by Server if Client has caused an Error
public class ClientErrorMessage : MessageBase //1004
{
    public string message;

    public ClientErrorMessage()
    {
        message = "";
    }

    public ClientErrorMessage(string error)
    {
        message = error;
    }
}

//Sent by Server when a Message needs to be sent with no Info inside of it
public class EmptyMessage : MessageBase //1005, 1008, 1009, 1101, 1102, 1105, 1106, 1109, 1012, 1013, 1014, 1111, 1016
{
}

//Sent by Client, contains all info on the Player
public class PlayerInfoMessage : MessageBase //1006
{
    public string displayName;
    public int character, hat, kart, wheel;
}

//Sent by Server, contains all racing Players
public class DisplayNameUpdateMessage : MessageBase //1007
{
    public string[] players;
}

//Sent by Server to tell client to add the scripts of a Specific Gamemode
public class LoadGamemodeMessage : MessageBase //1010
{
    public int gamemode;
}

//Sent by Server to make a timer appear on Client 
public class TimerMessage : MessageBase //1011
{
    public float time;

    public TimerMessage(float nTime)
    {
        time = nTime;
    }

    public TimerMessage()
    {
        time = -1;
    }
}

//----------------------------------- GAMEMODE 0 - Network Race ----------------------------------------------------//

//Sent by Client to Server & From Server to all Clients
public class TrackVoteMessage : MessageBase //1103, 1104
{
    public int cup;
    public int track;

    public TrackVoteMessage()
    {
        cup = 0;
        track = 0;
    }

    public TrackVoteMessage(int nCup, int nTrack)
    {
        cup = nCup;
        track = nTrack;
    }
}

public class AllVoteMessage : MessageBase //1107
{
    public int[] cups;
    public int[] tracks;
}

//Sent by Server to Client when an int needs sending
public class intMessage : MessageBase //1105, 1109, 1110, 1114
{
    public int value;

    public intMessage()
    {
        value = 0;
    }

    public intMessage(int nValue)
    {
        value = nValue;
    }
}

//Sent by Server to Client when a float needs sending
public class floatMessage : MessageBase //1021
{
    public float value;

    public floatMessage()
    {
        value = 0;
    }

    public floatMessage(float _Value)
    {
        value = _Value;
    }
}

//Sent by Server to Client when an string needs sending
public class stringMessage : MessageBase //1112, 1017
{
    public string value;

    public stringMessage()
    {
        value = "";
    }

    public stringMessage(string v)
    {
        value = v;
    }
}

//----------------------------------- GAME MODE REQUIRED STATIC CLASSES ----------------------------------------------------//

//Used to add the Host script for each Online Gamemode to the GameData Object
public static class OnlineGameModeScripts
{
    public static GameMode AddHostScript(int gamemode)
    {
        GameObject gdObject = GameObject.FindObjectOfType<CurrentGameData>().gameObject;
       // switch (gamemode)
      //  {
            //Online Race
           // case 0:
                //NetworkRaceHost host = gdObject.AddComponent<NetworkRaceHost>();
               // CurrentGameData.currentGamemode = host;
                //return host;
       // }

        return null;
    }

    public static GameMode AddClientScript(int gamemode)
    {
        GameObject gdObject = GameObject.FindObjectOfType<CurrentGameData>().gameObject;
      //  switch (gamemode)
      //  {
            //Online Race
           // case 0:
                //NetworkRaceClient client = gdObject.AddComponent<NetworkRaceClient>();
               // CurrentGameData.currentGamemode = client;
               // return client;
      //  }

        return null;
    }
}



