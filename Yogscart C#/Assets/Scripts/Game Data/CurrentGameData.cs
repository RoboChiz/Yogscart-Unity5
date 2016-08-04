using UnityEngine;
using System.Collections;

public class CurrentGameData : MonoBehaviour {

    public string version;
    public int overallLapisCount;

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
    private Color colourAlpha = Color.white;
    const float animationSpeed = 0.05f;
    private float lastTime = 0f;
    private int currentFrame = 0;
    private Texture2D blackTexture;

    public GameModeInfo[] onlineGameModes;

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

        LoadGame();

    }

    void OnGUI()
    {
        GUI.depth = -5;

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
        else
        {
            try
            {
                string[] splitData = gameData.Split(";"[0]);

                //Data Layout
                //0 - Version Number
                switch (splitData[0])
                {
                    case "C# Version 0.1":
                        Debug.Log("Version is compatible!");
                        break;
                    default:
                        ResetData();
                        return;
                }
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

                        timeCount++;
                    }
                }
                Debug.Log("Track Times is compatible!");
            }
            catch
            {
                ResetData();
            }

            Debug.Log("All data loaded!");
        }     
    }

    void ResetData()
    {
        Debug.Log("Data not compatible!");
        PlayerPrefs.SetString("YogscartData", "");
        LoadGame();
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
    //Delete Later////
    public Transform CharacterModel_Standing;
    //Delete Later////

    public AudioClip selectedSound;
    public AudioClip[] hitSounds, tauntSounds;
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
}