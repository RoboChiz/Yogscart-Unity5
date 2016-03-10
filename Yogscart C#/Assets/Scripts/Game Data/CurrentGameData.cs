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

    public static GameMode currentGamemode;

    //BlackOut Variables
    public static bool blackOut = true;
    private Color colourAlpha = Color.white;
    const float animationSpeed = 0.05f;
    private float lastTime = 0f;
    private int currentFrame = 0;
    private Texture2D blackTexture;

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

        //LoadEverything();

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
    public Texture2D logo;
    public Texture2D preview; //Maybe Animated???
    public Texture2D preview_GreyOut; //Maybe Animated???

    public float bestTime;
    public string sceneID;
}

[System.Serializable]
public class Tournament
{
    public string name;
    public Texture2D icon;
    public Transform[] trophyModels;
    public string[] lastRank;
    public Track[] tracks;
    public UnlockedState unlocked;
}