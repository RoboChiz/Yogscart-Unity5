using UnityEngine;
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

    public bool unlockedInsane = false;

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
    }

    private void Start()
    {
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

    void LoadGame()
    {
        string gameData = PlayerPrefs.GetString("YogscartData","");
        if(gameData != "")
        {
            //Achievement GET!
            FindObjectOfType<AchievementManager>().UnlockAchievement("WholeNewGame");
        }

        SaveDataManager saveDataManager = FindObjectOfType<SaveDataManager>();
        overallLapisCount = saveDataManager.GetLapisAmount();
        playerName = saveDataManager.GetPlayerName();
        streamMode = saveDataManager.GetStreamMode();
        mouseScale = saveDataManager.GetMouseScale();
        controllerScale = saveDataManager.GetControllerScale();
        unlockedInsane = saveDataManager.GetUnlockedInsane();

        //Unlock Characters
        for (int i = 0; i < characters.Length; i++)
        {
            if(characters[i].unlocked != UnlockedState.FromStart)
            {
                characters[i].unlocked = saveDataManager.GetCharacterUnlocked(i) ? UnlockedState.Unlocked : UnlockedState.Locked;
            }
        }

        //Unlock Hats
        for (int i = 0; i < hats.Length; i++)
        {
            if (hats[i].unlocked != UnlockedState.FromStart)
            {
                hats[i].unlocked = saveDataManager.GetHatUnlocked(i) ? UnlockedState.Unlocked : UnlockedState.Locked;
            }
        }

        //Unlock Karts
        for (int i = 0; i < karts.Length; i++)
        {
            if (karts[i].unlocked != UnlockedState.FromStart)
            {
                karts[i].unlocked = saveDataManager.GetKartUnlocked(i) ? UnlockedState.Unlocked : UnlockedState.Locked;
            }
        }

        //Unlock Wheels
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].unlocked != UnlockedState.FromStart)
            {
                wheels[i].unlocked = saveDataManager.GetWheelUnlocked(i) ? UnlockedState.Unlocked : UnlockedState.Locked;
            }
        }

        //Tournament Ranks
        for (int i = 0; i < tournaments.Length; i++)
            tournaments[i].lastRank = saveDataManager.GetTournamentRanks(i);

        //Track Times
        int trackCount = 0;
        for (int i = 0; i < tournaments.Length; i++)
        {
            for (int j = 0; j < tournaments[i].tracks.Length; j++)
            {
                Track track = tournaments[i].tracks[j];

                track.bestTime = saveDataManager.GetTrackTime(trackCount);
                trackCount++;
            }
        }

        Debug.Log("All data loaded!");
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
            foreach (FileInfo file in fileInfo)
            {
                FileStream fileStream = null;
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    fileStream = file.Open(FileMode.Open);

                    GhostData ghostData = (GhostData)bf.Deserialize(fileStream);
                    bool updateFile = false;

                    if(CompatibleVersion(ghostData.version))
                    {
                        tournaments[ghostData.cup].tracks[ghostData.track].ghosts++;

                        if (ghostData.version != version)
                            updateFile = true;
                    }

                    //Update Save File if Appropriate
                    if (updateFile)
                        UpdateFile(ghostData);
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
        }
        catch { }
    }

    public bool CompatibleVersion(string _version)
    {
        if(_version == TimeTrial.saveVersion.ToString())
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

    public int GetRandomLevelForTrack(int _cup, int _track)
    {
        string[] possibleLevels = tournaments[_cup].tracks[_track].sceneIDs;

        if (possibleLevels.Length == 1)
        {
            return 0;
        }
        else
        {
            //Choose a Level at Random, but allow programmers to change outcome
            int[] levelchance = new int[possibleLevels.Length];
            int totalValue = 0;

            for(int i = 0; i < possibleLevels.Length; i++)
            {
                //Every level has a chance of coming up
                levelchance[i] = 1 + ChangeChances(possibleLevels[i]);

                totalValue += levelchance[i];
            }

            //Get Random Value
            int randomVal = UnityEngine.Random.Range(0, totalValue);
            for (int i = 0; i < possibleLevels.Length; i++)
            {
                randomVal -= levelchance[i];

                if(randomVal <= 0)
                {
                    return i;
                }
            }
        }

        throw new Exception("Shouldn't get here!");
    }

    //Increase a levels chances of coming up based on player input
    public int ChangeChances(string levelName)
    {
        switch(levelName)
        {
            case "SjinsFarm_Spooky": //If Player has chosen Witches Hat then increase odds of this level coming up
                //Find the witches hat
                int witchHatID = FindHatID("Witch Hat");

                foreach (LoadOut loadOut in currentChoices)
                {
                    if(loadOut.hat == witchHatID)
                    {
                        return 5;
                    }
                }
               
                break;
            case "SjinsFarm_Christmas": //If Player has chosen Santa Hat then increase odds of this level coming up
                //Find the witches hat
                int santaHatID = FindHatID("Santa Hat");

                foreach (LoadOut loadOut in currentChoices)
                {
                    if (loadOut.hat == santaHatID)
                    {
                        return 5;
                    }
                }

                break;
        }

        return 0;
    }

    //Return the id of a Hat in the Hats List
    public int FindHatID(string _hatName)
    {
        for (int i = 0; i < hats.Length; i++)
        {
            if (hats[i].name == _hatName)
            {
                return i;
            }
        }

        return -1;
    }

    public bool CanUnlockInsane()
    {
        foreach(Tournament tournament in tournaments)
        {
            bool validTournament = false;

            for(int i = 0; i < tournament.lastRank.Length; i++)
            {
                if(tournament.lastRank[i] >= Rank.Gold)
                {
                    validTournament = true;
                    break;
                }
            }

            if (!validTournament)
                return false;
        }

        Debug.Log("Insane Unlocked!");
        return true;
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
    public string[] sceneIDs;

    [HideInInspector]
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