using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
/*
Gamemode Base Class v1.0
Created by Robert (Robo_Chiz)
FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
If you wanna add something add it as part of your child class.
Thanks
*/

abstract public class GameMode : MonoBehaviour
{
    //Game Managers - Loaded automatically
    protected CurrentGameData gd;
    protected SoundManager sm;
    protected KartMaker km;

    //Player Information
    protected float maxPlayers = 12f;
    protected List<Racer> racers;

    //Match Information
    private float startTimer;
    protected float timer; //Holds the time the match has been going on for...
    public float Timer
    {
        get { return timer; }
        set { }
    }

    /* Important Game Stuff
    AI Enabled - Obvious
    Finished - Used by Menu systems to tell wheter the gamemode has finished
    Client Only - Avoid running host stuff, useful for Network games*/
    protected bool aiEnabled = true, clientOnly = false;
    public bool finished = false;

    //Countdown Stuff
    public int countdownText; //The number on screen
    public bool countdowning = false;

    public float countdownAlpha = 0f, countdownScale = 0f;

    //Load the Game Managers
    void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();
        km = FindObjectOfType<KartMaker>();
    }

    //Called to Start Gamemode by menus. Must be included in your class
    abstract public void StartGameMode();

    /// <summary>
    /// Default way to setup the Racers for the Gamemode
    /// </summary>
    protected void SetupRacers()
    {
        racers = new List<Racer>();
        int controllerCount = InputManager.controllers.Count;

        //Fill racers with AI using default difficulty setup.
        //Change the values in the Start function if you want something else
        if (aiEnabled)
        {

            int diff = CurrentGameData.difficulty;
            int minVal, maxVal;

            switch (diff)
            {
                case 0:
                    maxVal = 2;
                    minVal = 0;
                    break;
                case 1:
                    maxVal = 3;
                    minVal = 1;
                    break;
                case 2:
                    maxVal = 4;
                    minVal = 2;
                    break;
                default:
                    maxVal = 4;
                    minVal = 4;
                    break;
            }

            int counter = minVal;

            List<int> characterShuffle = new List<int>();
            for (int i = 0; i < gd.characters.Length; i++)
            {
                characterShuffle.Add(i);
            }

            characterShuffle = ShuffleArray(characterShuffle);

            //Add Hats if you feel like it XD

            //Add Racers
            for (int i = 0; i < maxPlayers - controllerCount; i++)
            {
                racers.Add(new Racer(-1, counter, characterShuffle[i % characterShuffle.Count], 0, Random.Range(0, gd.karts.Length), Random.Range(0, gd.wheels.Length), i));

                counter++;
                if (counter > maxVal)
                    counter = minVal;
            }
        }

        int startRacer = racers.Count;

        //Add Human Players
        for (int i = controllerCount - 1; i >= 0 ; i--)
        {
            racers.Add(new Racer(i, -1, CurrentGameData.currentChoices[i], startRacer));
            startRacer++;
        }
    }

    //Used as part of ShuffleArray to randomise selected AI characters
    private List<int> ShuffleArray(List<int> arr)
    {
        int i1 = 0, i2 = 0;

        for (int i = 0; i < arr.Count * 2; i++)
        {
            i1 = Random.Range(0, arr.Count);
            i2 = Random.Range(0, arr.Count);

            int holder = arr[i1];
            arr[i1] = arr[i2];
            arr[i2] = holder;
        }

        return arr;
    }

    //Spawns all Karts in the typical race layout
    protected void SpawnAllKarts(Vector3 spawnPosition, Quaternion spawnRotation)
    {

        //Spawn the Karts
        for (int i = 0; i < racers.Count; i++)
        {
            int racePos = racers[i].position;

            Vector3 startPos = spawnPosition + (spawnRotation * Vector3.forward * (3f * 1.5f) * -1.5f);
            Vector3 x2 = spawnRotation * (Vector3.forward * (racePos % 3) * (3 * 1.5f) + (Vector3.forward * .75f * 3));
            Vector3 y2 = spawnRotation * (Vector3.right * (racePos + 1) * 3);
            startPos += x2 + y2;

            racers[i].ingameObj = km.SpawnKart(KartType.Local, startPos, spawnRotation * Quaternion.Euler(0, -90, 0), racers[i].Character, racers[i].Hat, racers[i].Kart, racers[i].Wheel);

            //Set speeds of Kart depending on Difficulty
            SetDifficulty(racers[i].ingameObj.GetComponent<KartMovement>());

            if (racers[i].Human != -1)
            {
                Transform inGameCam = (Transform)Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), startPos, Quaternion.identity);
                inGameCam.name = "InGame Cams";

                KartInput ki = racers[i].ingameObj.GetComponent<KartInput>();
                ki.myController = racers[i].Human;
                ki.camLocked = true;
                ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
                ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

                inGameCam.GetChild(1).tag = "MainCamera";

                inGameCam.GetChild(0).transform.GetComponent<KartCamera>().target = racers[i].ingameObj.GetComponent<KartMovement>().kartBody;
                inGameCam.GetChild(1).transform.GetComponent<KartCamera>().target = racers[i].ingameObj.GetComponent<KartMovement>().kartBody;

                inGameCam.GetChild(0).transform.GetComponent<KartCamera>().rotTarget = racers[i].ingameObj;
                inGameCam.GetChild(1).transform.GetComponent<KartCamera>().rotTarget = racers[i].ingameObj;
                racers[i].cameras = inGameCam;
            }
            else
            {
                Destroy(racers[i].ingameObj.GetComponent<KartInput>());
            }
        }
    }

    public void SetDifficulty(KartMovement kartMovement)
    {
        //Set speeds of Kart depending on Difficulty
        switch (CurrentGameData.difficulty)
        {
            case 0: kartMovement.speedModifier = 0.9f; break;
            case 1: kartMovement.speedModifier = 1f; break;
            default: kartMovement.speedModifier = 1.1f; break;
        }
    }

    //Spawns the given kart in Racers in the Time Trial layout
    protected void SpawnLoneKart(Vector3 spawnPosition, Quaternion spawnRotation, int i)
    {
        //Spawn the Karts
        Vector3 startPos = spawnPosition + (spawnRotation * Vector3.forward * (3 * 1.5f) * -1.5f);
        Vector3 x2 = spawnRotation * (Vector3.forward * 4.5f) + (Vector3.forward * .75f * 3);
        Vector3 y2 = spawnRotation * (Vector3.right * 6);
        startPos += x2 + y2;

        racers[i].ingameObj = km.SpawnKart(KartType.Local, startPos, spawnRotation * Quaternion.Euler(0, -90, 0), racers[i].Character, racers[i].Hat, racers[i].Kart, racers[i].Wheel);

        //Set speeds of Kart depending on Difficulty
        SetDifficulty(racers[i].ingameObj.GetComponent<KartMovement>());

        if (racers[i].Human != -1)
        {
            Transform inGameCam = (Transform)Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), startPos, Quaternion.identity);
            inGameCam.name = "InGame Cams";

            KartInput ki = racers[i].ingameObj.GetComponent<KartInput>();
            ki.myController = racers[i].Human;
            ki.camLocked = true;
            ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
            ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

            inGameCam.GetChild(1).tag = "MainCamera";

            inGameCam.GetChild(0).transform.GetComponent<KartCamera>().target = racers[i].ingameObj.GetComponent<KartMovement>().kartBody;
            inGameCam.GetChild(1).transform.GetComponent<KartCamera>().target = racers[i].ingameObj.GetComponent<KartMovement>().kartBody;

            inGameCam.GetChild(0).transform.GetComponent<KartCamera>().rotTarget = racers[i].ingameObj;
            inGameCam.GetChild(1).transform.GetComponent<KartCamera>().rotTarget = racers[i].ingameObj;

            racers[i].cameras = inGameCam;

            //Setup Camera
            //Kart INFO Stuff
        }
        else
        {
            Destroy(racers[i].ingameObj.GetComponent<KartInput>());
            //Destroy(racers[i].ingameObj.GetComponent<kartInfo>());
            //racers[i].ingameObj.gameObject.AddComponent<AIRacer>();
            //racers[i].ingameObj.GetComponent<AIRacer>().stupidity = racers[i].aiStupidity;
        }
    }

    public void StartCountdown()
    {
        if (!countdowning)
            StartCoroutine("ActualStartCountdown");
    }

    private IEnumerator ActualStartCountdown()
    {
        countdowning = true;

        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/CountDown"));

        for (int i = 3; i >= 0; i--)
        {
            countdownText = i;
            KartMovement.startBoostVal = i;

            countdownScale = 1.1f;
            countdownAlpha = 0f;

            yield return FadeTo(0.8f);

            yield return new WaitForSeconds(0.3f);
        }

        countdownText = -1;
        KartMovement.startBoostVal = -1;

        countdowning = false;
    }

    private IEnumerator FadeTo(float time)
    {
        float startTime = Time.time, startVal = countdownScale;

        while(Time.time - startTime < time)
        {
            countdownScale = Mathf.Lerp(startVal, 0.5f, (Time.time - startTime) / time);

            if(Time.time - startTime < 0.2f)
                countdownAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / 0.2f);
            else if (Time.time - startTime > 0.6f)
                countdownAlpha = Mathf.Lerp(1f, 0f, ((Time.time - startTime) - 0.6f) / 0.2f);

            yield return null;
        }

        countdownScale = 0f;
        countdownAlpha = 0f;
    }

    //Remember to call "base.OnGUI();" to get countdown behaviour
    public virtual void OnGUI()
    {
        //CountDown
        if (countdownAlpha > 0f)
        {
            GUIHelper.SetGUIAlpha(countdownAlpha);

            Matrix4x4 hold = GUI.matrix;
            GUI.matrix = GUIHelper.GetMatrix();

            Texture2D countdownTexture;

            if (countdownText == 0)
                countdownTexture = Resources.Load<Texture2D>("UI/CountDown/GO");
            else if (countdownText > 0)
                countdownTexture = Resources.Load<Texture2D>("UI/CountDown/" + countdownText.ToString());
            else
                countdownTexture = null;

            if (countdownTexture != null)
                GUI.DrawTexture(GUIHelper.CentreRect(new Rect(400, 200, 1120, 680), countdownScale), countdownTexture, ScaleMode.ScaleToFit);

            GUIHelper.ResetColor();
            GUI.matrix = hold;
        }
    }

    public abstract void HostUpdate();

    public abstract void ClientUpdate();

    protected void ForceStop()
    {
        StopAllCoroutines();
    }

    protected void StartTimer()
    {
        startTimer = Time.time;
        StartCoroutine("ActualStartTimer");
    }

    protected void StopTimer()
    {
        StopCoroutine("ActualStartTimer");
    }

    private IEnumerator ActualStartTimer()
    {
        while (true)
        {
            timer = Time.time - startTimer;
            yield return null;
        }
    }

    public virtual void EndGamemode()
    {
        StartCoroutine(QuitGame());
    }

    public virtual void Restart() { }

    protected IEnumerator QuitGame()
    {
        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        AsyncOperation sync = SceneManager.LoadSceneAsync("Main_Menu");

        while(!sync.isDone)
            yield return null;

        FindObjectOfType<MainMenu>().ReturnFromGame();

        Destroy(this);
    }

    //Called when a client disconnects from online gamemode (Ignore if Single Player Only)
    public abstract void OnServerDisconnect(NetworkConnection conn);
    //Called when a client connects to online gamemode (Ignore if Single Player Only)
    public abstract void OnServerConnect(NetworkConnection conn);
    //Called when a new player is requested by a client
    public abstract GameObject OnServerAddPlayer(NetworkRacer nPlayer, GameObject playerPrefab);
    //Called by the server when a Gamemode has finished. Should be used for tidying up
    public abstract void OnEndGamemode();
}

abstract public class TeamGameMode : GameMode
{

    protected List<Team> teams;

    //Used to handle Team Names & Max number of players per team
    public class Team
    {
        /// <summary>
        /// Create a new Team for your GameMode
        /// </summary>
        /// <param name="tn">The name of the new Team.</param>
        /// <param name="mp">The maximum number of players allowed in this Team.</param>
        public Team(string tn, int mp)
        {
            teamName = tn;
            maxPlayers = mp;
        }

        //Team name is set during declaration and cannot be changed directely.
        private string teamName;
        public string TeamName
        {
            get { return teamName; }
            set { }
        }

        //Maximum number of players in team is set during declaration and cannot be changed directely.
        private int maxPlayers;
        public int MaxPlayers
        {
            get { return maxPlayers; }
            set { }
        }

    }

}

public class Racer
{
    //Racer Information //////////////////////////////////
    //What input the racer is -1 = AI, 0 - 3 Human
    protected int human = -1;
    public int Human
    {
        get { return human; }
        set { }
    }

    protected int aiStupidity = -1;
    public int AiStupidity
    {
        get { return aiStupidity; }
        set { }
    }

    //Race Loading Infomation //////////////////////////////////
    protected int character;
    public int Character
    {
        get { return character; }
        set { }
    }

    protected int hat;
    public int Hat
    {
        get { return hat; }
        set { }
    }

    protected int kart;
    public int Kart
    {
        get { return kart; }
        set { }
    }

    protected int wheel;
    public int Wheel
    {
        get { return wheel; }
        set { }
    }

    //During Race Information //////////////////////////////////
    public bool finished;
    public int position;
    public int overallPosition;
    public Transform ingameObj;
    public Transform cameras;

    public float timer;
    public int lap;
    public float currentPercent;

    //After Race Information //////////////////////////////////
    public int points = 0;
    public int team;

    //Constructor
    public Racer(int hum, int ais, int ch, int h, int k, int w, int p)
    {
        human = hum;
        aiStupidity = ais;
        character = ch;
        hat = h;
        kart = k;
        wheel = w;
        position = p;

    }

    public Racer(int hum, int ais, LoadOut lo, int p)
    {
        human = hum;
        aiStupidity = ais;
        character = lo.character;
        hat = lo.hat;
        kart = lo.kart;
        wheel = lo.wheel;
        position = p;
    }

    public Racer()
    {
        human = 0;
        aiStupidity = -1;
        character = -1;
        hat = -1;
        kart = -1;
        wheel = -1;
        position = -1;
    }
}

public class ReplayRacer : Racer
{
    //Ghost Data
    public List<List<string>> ghostData;

    public ReplayRacer(Racer otherRacer)
    {
        human = otherRacer.Human;
        aiStupidity = otherRacer.AiStupidity;
        character = otherRacer.Character;
        hat = otherRacer.Hat;
        kart = otherRacer.Kart;
        wheel = otherRacer.Wheel;
        position = otherRacer.position;
        overallPosition = otherRacer.overallPosition;
    }

    public string DataToString()
    {
        string finalString = "";

        foreach (List<string> actionList in ghostData)
        {
            if (actionList.Count > 0)
            {
                foreach (string action in actionList)
                {
                    finalString += action + ";";
                }

                //Remove the last ; to stop errors
                finalString = finalString.Remove(finalString.Length - 1);
            }

            //Denote end of frame
            finalString += ">";
        }

        //Remove the last > to stop errors
        finalString = finalString.Remove(finalString.Length - 1);

        return finalString;
    }

}

