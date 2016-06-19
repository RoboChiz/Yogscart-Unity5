using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public static class UnetMessages
{
    public const short versionMsg = 1002;
    public const short acceptedMsg = 1003;
    public const short clientErrorMsg = 1004;
    public const short playerUpMsg = 1005;
    public const short playerInfoMsg = 1006;
    public const short displayNameUpdateMsg = 1007;
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

//Sent by Server telling client to pick a character
public class PlayerUpMessage : MessageBase //1005
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
    
