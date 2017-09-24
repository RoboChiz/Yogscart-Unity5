﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class CurrentGameData : MonoBehaviour {

    public string version;
    public int overallLapisCount = 0;
    public string playerName = "";

    //Used for current Game Mode
    static public LoadOut[] currentChoices;
    static public int difficulty = 0;
    /*For Races
    0 = 50CC
    1 = 100CC
    2 = 150CC
    3 = Insane
    */

    public static bool unlockedInsane = false;

    public Character[] characters;
    public Hat[] hats;

    public Kart[] karts;
    public Wheel[] wheels;

    public Tournament[] tournaments;

    public PowerUp[] powerUps;

    public static GameMode currentGamemode;

    //BlackOut Variables
    public static bool blackOut = false;
    public bool isBlackedOut { get { return colourAlpha.a >= 1f; } }

    private Color colourAlpha = Color.white;
    const float animationSpeed = 0.05f;
    private float lastTime = 0f;
    private int currentFrame = 0;
    private Texture2D blackTexture;

    public GameModeInfo[] onlineGameModes;

    //Options
    public bool streamMode;
    public float mouseScale = 1f;
    public float controllerScale = 1f;

    // Use this for initialization
    void Awake ()
    {
        DontDestroyOnLoad(gameObject);

        currentChoices = new LoadOut[4];
        for (int i = 0; i < currentChoices.Length; i++)
            currentChoices[i] = new LoadOut();

        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();

        playerName = "";

        LoadGame();

    }

    void OnGUI()
    {
        GUI.depth = -105;

        if(!blackOut && colourAlpha.a > 0)
        {
            colourAlpha.a -= Time.deltaTime;
        }
        else if(blackOut && colourAlpha.a < 1)
        {
            colourAlpha.a += Time.deltaTime;
        }

        GUI.color = colourAlpha;
        GUI.DrawTexture(new Rect(-5f, -5f, Screen.width + 5f, Screen.height + 5f), blackTexture);      

        //Sort out Animation
        if (Time.time - lastTime >= animationSpeed)
        {
            currentFrame++;
            lastTime = Time.time;
        }

        if (currentFrame > 22)
            currentFrame = 0;

        float aniSize = ((Screen.height + Screen.width) / 2f) / 8f;
        Rect aniRect = new Rect(Screen.width - 10 - aniSize, Screen.height - 10 - aniSize, aniSize, aniSize);
        GUI.DrawTexture(aniRect, Resources.Load<Texture2D>("UI/Loading/" + (currentFrame + 1)));

        GUI.color = Color.white;

    }

    void Update()
    {
        mouseScale = Mathf.Clamp(mouseScale, 0.1f, 10f);
        controllerScale = Mathf.Clamp(controllerScale, 0.1f, 10f);
    }

    public void SaveGame()
    {
        string gameData = "";

        //Data Layout
        //0 - Version Number
        gameData += version + ";";
        //1 - Unlocked Characters
        gameData += ";";
        //2 - Unlocked Hats
        gameData += ";";
        //3 - Unlocked Karts
        gameData += ";";
        //4 - Unlocked Wheels
        gameData += ";";
        //5 - Tournament Ranks
        for (int i = 0; i < tournaments.Length; i++)
        {
            if (tournaments[i].lastRank.Length != 4)
                tournaments[i].lastRank = new Rank[4];

            for (int j = 0; j < tournaments[i].lastRank.Length; j++)
            {
                gameData += (int)tournaments[i].lastRank[j];

                if (i != tournaments.Length - 1 || j != tournaments[i].lastRank.Length - 1)
                {
                    gameData += ",";
                }
            }
        }
        gameData += ";";
        //6 - Track Times
        for (int i = 0; i < tournaments.Length; i++)
        {
            for(int j = 0; j < tournaments[i].tracks.Length; j++)
            {
                gameData += tournaments[i].tracks[j].bestTime;
                if (i != tournaments.Length - 1 || j != tournaments[i].lastRank.Length - 1)
                {
                    gameData += ",";
                }
            }
        }
        gameData += ";";

        //7 - Lapis
        gameData += overallLapisCount;
        gameData += ";";

        //8 - Player Name
        gameData += playerName;
        gameData += ";";

        //9 - Stream Mode
        gameData += streamMode.ToString();
        gameData += ";";

        //10 - Mouse Scale
        gameData += mouseScale;
        gameData += ";";

        //11 - Controller Scale
        gameData += controllerScale;

        PlayerPrefs.SetString("YogscartData", gameData);
    }

    void LoadGame()
    {
        string gameData = PlayerPrefs.GetString("YogscartData","");

        if (gameData == "")
        {
            //No Data Available
            Debug.Log("No Data Available");
            SaveGame();
        }

        try
        {
            string[] splitData = gameData.Split(";"[0]);

            //Data Layout
            //0 - Version Number
            /*switch (splitData[0])
            {
                case "C# Version 0.1":
                    Debug.Log("Version is compatible!");
                    break;
                default:
                    ResetData();
                    return;
            }*/
            //1 - Unlocked Characters
            //2 - Unlocked Hats
            //3 - Unlocked Karts
            //4 - Unlocked Wheels
            //5 - Tournament Ranks
            string[] tournamentRanks = splitData[5].Split(","[0]);
            //Debug.Log("tournamentRanks:" + tournamentRanks.Length + " tournaments.Length:" + tournaments.Length);

            if (tournamentRanks.Length != (tournaments.Length * 4))
            {
                ResetData();
                return;
            }
            else
            {
                Debug.Log("Tournament Ranks is compatible!");
                int rankCount = 0;
                for (int i = 0; i < tournaments.Length; i++)
                {
                    Rank[] ranks = new Rank[4];

                    for (int j = 0; j < ranks.Length; j++)
                    {
                        int outVal = -1;

                        if (!int.TryParse(tournamentRanks[rankCount], out outVal) || ranks[j] < 0 || (int)ranks[j] >= 5)
                        {
                            ResetData();
                            return;
                        }

                        ranks[j] = (Rank)outVal;

                        rankCount++;
                    }

                    tournaments[i].lastRank = ranks;
                }
            }
            //6 - Track Times
            string[] trackTimes = splitData[6].Split(","[0]);
            int timeCount = 0;
            for (int i = 0; i < tournaments.Length; i++)
            {
                for (int j = 0; j < tournaments[i].tracks.Length; j++)
                {
                    float outFloat = -1;
                    if (!float.TryParse(trackTimes[timeCount], out outFloat) || outFloat < 0 || outFloat >= 3600)
                    {
                        ResetData();
                        return;
                    }

                    tournaments[i].tracks[j].bestTime = outFloat;
                    tournaments[i].tracks[j].ghosts = 0;

                    timeCount++;
                }
            }
            Debug.Log("Track Times is compatible!");
            //7 - Lapis
            overallLapisCount = int.Parse(splitData[7]);
            //8 - Player Name
            playerName = splitData[8];
            //9 - Stream Mode
            streamMode = bool.Parse(splitData[9]);
            //10 - Mouse Scale
            mouseScale = float.Parse(splitData[10]);
            //11 - Controller Scale
            controllerScale = float.Parse(splitData[11]);
        }
        catch
        {
            ResetData();
        }

        Debug.Log("All data loaded!");

        CountGhosts();

        Debug.Log("All ghosts loaded!");
    }

    public void ResetData()
    {
        Debug.Log("Data not compatible!");

        //Data Layout
        //0 - Version Number
        //1 - Unlocked Characters
        //2 - Unlocked Hats
        //3 - Unlocked Karts
        //4 - Unlocked Wheels
        //5 - Tournament Ranks
        for (int i = 0; i < tournaments.Length; i++)
        {
            tournaments[i].lastRank = new Rank[4];
        }
        //6 - Track Times
        for (int i = 0; i < tournaments.Length; i++)
        {
            for (int j = 0; j < tournaments[i].tracks.Length; j++)
            {
                tournaments[i].tracks[j].bestTime = 0f;
            }
        }
        //7 - Lapis
        overallLapisCount = 0;

        //8 - PlayerName

        //9 - Stream Mode
        streamMode = false;

        //10 - Mouse Scale
        mouseScale = 1f;

        //11 - Controller Scale
        controllerScale = 1f;

        SaveGame();
    }

    public void CountGhosts()
    {
        for (int i = 0; i < tournaments.Length; i++)
            for (int j = 0; j < tournaments[i].tracks.Length; j++)
                tournaments[i].tracks[j].ghosts = 0;

        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Ghost Data/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Ghost Data/");

            //Check every file in Ghost Data folder
            var info = new DirectoryInfo(Application.persistentDataPath + "/Ghost Data/");
            var fileInfo = info.GetFiles();
            bool updateFile = false;

            foreach (FileInfo file in fileInfo)
            {
                FileStream fileStream = null;
                GhostData gd = null;

                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    fileStream = file.Open(FileMode.Open);

                    gd = (GhostData)bf.Deserialize(fileStream);
                    gd.fileLocation = file.FullName;

                    if (CompatibleVersion(gd.version))
                    {
                        tournaments[gd.cup].tracks[gd.track].ghosts++;

                        if (gd.version != version)
                            updateFile = true;
                    }
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }

                //Update Save File if Appropriate
                if (updateFile)
                    UpdateFile(gd);
            }
        }
        catch { }
    }

    public bool CompatibleVersion(string versionName)
    {
        if (versionName == version)
            return true;

        if (versionName == "C# Version 0.7")
            return true;

        return false;
    }

    public void UpdateFile(GhostData data)
    {
        //Update Ghost Data
        data.version = version;

        //Save File
        FileStream sw = null;
        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Ghost Data/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Ghost Data/");

            BinaryFormatter bf = new BinaryFormatter();
            sw = File.Create(data.fileLocation);
            bf.Serialize(sw, data);
            sw.Flush();

            Debug.Log("Updated " + data.fileLocation + " to " + version);
        }
        finally
        {
            if (sw != null)
                sw.Close();
        }
    }

    public CharacterSoundPack GetCustomSoundPack(int character, int hat)
    {
        CharacterSoundPack playClip = characters[character].defaultSoundPack;

        if (hat != 0)
        {
            foreach (CustomCharacterSoundPack checkSoundPack in characters[character].customSoundPacks)
            {
                if (checkSoundPack.effectedHat == hat)
                {
                    playClip = checkSoundPack;
                    break;
                }
            }
        }

        return playClip;
    }
}


//Other Classes
public enum UnlockedState { FromStart, Unlocked, Locked};

[System.Serializable]
public class Character
{
    public string name;
    public UnlockedState unlocked;
    public Texture2D icon;

    public Transform model;
    public Transform CharacterModel_Standing;

    public CharacterSoundPack defaultSoundPack;
    public List<CustomCharacterSoundPack> customSoundPacks;
}

[System.Serializable]
public class CharacterSoundPack
{
    public string name;
    public AudioClip selectedSound;
    public AudioClip[] hitSounds, tauntSounds;
}

[System.Serializable]
public class CustomCharacterSoundPack : CharacterSoundPack
{
    public int effectedHat;
}

[System.Serializable]
public class Kart
{
    public string name;
    public UnlockedState unlocked;
    public Texture2D icon;
    public Transform model;
}

[System.Serializable]
public class Hat
{
    public string name;
    public UnlockedState unlocked;
    public Texture2D icon;
    public Transform model;
}

[System.Serializable]
public class Wheel
{
    public string name;
    public UnlockedState unlocked;
    public Texture2D icon;
    public Transform model;
}

[System.Serializable]
public class LoadOut
{
    public int character;
    public int hat;
    public int kart;
    public int wheel;

    public LoadOut()
    {
        character = 0;
        hat = 0;
        kart = 0;
        wheel = 0;
    }

    public LoadOut(int ch, int ha, int ka, int wh)
    {
        character = ch;
        hat = ha;
        kart = ka;
        wheel = wh;
    }
}

[System.Serializable]
public class Track
{
    public string name;
    public Texture2D logo; //Maybe Animated???
    public Texture2D logo_GreyOut; //Maybe Animated???
    public Texture2D preview;

    public float bestTime;
    public string sceneID;

    public int ghosts;
}

public enum Rank { NoRank, Bronze, Silver, Gold, Perfect};

[System.Serializable]
public class Tournament
{
    public string name;
    public Texture2D icon;
    public Transform[] trophyModels;
    public Rank[] lastRank = new Rank[4];
    public Track[] tracks;
    public UnlockedState unlocked;
}

public enum ItemType { AffectsPlayer, AffectsOther, Projectile };

[System.Serializable]
public class PowerUp
{
    public string name;
    public Texture2D icon;

    public Transform model;
    public Transform onlineModel;

    public ItemType type;
    public bool multipleUses;
    public bool useableShield;
    public int[] likelihood;

    public enum AIUnderstanding { SpeedBoost, Lapis, Dirt, Egg, JR};
    public AIUnderstanding aiUnderstanding = AIUnderstanding.SpeedBoost;
}