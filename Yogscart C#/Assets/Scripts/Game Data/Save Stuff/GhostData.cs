using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostData
{
    public int version, character, hat, kart, wheel, track, cup, variation;
    public float time;
    public string data, playerName;

    [System.NonSerialized]
    public string fileLocation;

    public GhostData(Racer racer, string _data, int _cup, int _track, int _variation, string _playerName)
    {
        character = racer.character;
        hat = racer.hat;
        kart = racer.kart;
        wheel = racer.wheel;

        time = racer.timer;
        data = _data;

        track = _track;
        cup = _cup;
        variation = _variation;

        version = SaveData.saveVersion;
        playerName = _playerName;
    }

    public GhostData()
    {
        data = "";
        playerName = "";
        fileLocation = "";
        version = SaveData.saveVersion;
    }

    public List<List<string>> ToData()
    {
        List<List<string>> replayData = new List<List<string>>();

        string[] frames = data.Split('>');

        foreach (string frame in frames)
        {
            //Create a new list of strings
            List<string> actionsList = new List<string>();
            replayData.Add(actionsList);

            string[] actions = frame.Split(';');
            actionsList.AddRange(actions);
        }

        return replayData;
    }
}
