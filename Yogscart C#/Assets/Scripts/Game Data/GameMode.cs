using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
Gamemode Base Class v1.0
Created by Robert (Robo_Chiz)
FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
If you wanna add something add it as part of your child class.
Thanks
*/

abstract public class GameMode : MonoBehaviour
{
    protected float maxPlayers = 12f;
    protected List<Racer> racers;
    protected float timer;

    protected bool aiEnabled = true, finished = false, clientOnly = false;

    protected CurrentGameData gd;

    //Called to Start Gamemode, can't be changed
    public void StartGameMode()
    {
        StartCoroutine("ActualStartGameMode");
    }

    private IEnumerator ActualStartGameMode()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();

        //Apply programmers desired changes, probably load the level
        yield return StartCoroutine("MyStart");

        Debug.Log("It seriously worked...");

        //Setup the Racers for the Gamemode
        Setup();

        Debug.Log("Lets load some Karts");
        //Spawn the karts however programmer wants 
        SpawnKarts();
        
        //Do the intro to the Map
        yield return StartCoroutine("DoIntro");
        
        //Do the Countdown
        yield return StartCoroutine("StartCountdown");

        //Wait for the gamemode to be over
        while (!finished)
        {
            ClientUpdate();

            if (!clientOnly)
                HostUpdate();

            yield return null;
        }

        //Show Results

        //Tidy Up

    }

    /// <summary>
    /// Add your own start behaviour here
    /// </summary>
    public abstract IEnumerator MyStart();

    /// <summary>
    /// Setup the Racers for the GamemOde
    /// </summary>
    private void Setup()
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
                    maxVal = 10;
                    minVal = 7;
                    break;
                case 1:
                    maxVal = 8;
                    minVal = 4;
                    break;
                case 2:
                    maxVal = 5;
                    minVal = 0;
                    break;
                default:
                    maxVal = 3;
                    minVal = 0;
                    break;
            }

            int counter = minVal;

            List<int> characterShuffle = new List<int>();
            for(int i = 0; i < gd.characters.Length; i++)
            {
                characterShuffle.Add(i);
            }

            characterShuffle = ShuffleArray(characterShuffle);

            //Add Hats if you feel like it XD

            //Add Racers
            for(int i = 0; i < maxPlayers - controllerCount; i++)
            {
                racers.Add(new Racer(-1, counter, characterShuffle[i % characterShuffle.Count], 0, Random.Range(0, gd.karts.Length), Random.Range(0, gd.wheels.Length), i));

                counter++;
                if (counter > maxVal)
                    counter = minVal;
            }
        }

        //Add Human Players
        for(int i = 0; i < controllerCount; i++)
        {
            racers.Add(new Racer(i, -1, CurrentGameData.currentChoices[i], i));
        }
    }

    private List<int> ShuffleArray(List<int> arr)
    {
        int i1 = 0, i2 = 0;

        for(int i = 0; i < arr.Count * 2; i++)
        {
            i1 = Random.Range(0, arr.Count);
            i2 = Random.Range(0, arr.Count);

            int holder = arr[i1];
            arr[i1] = arr[i2];
            arr[i2] = holder;
        }

        return arr;
    }

    public virtual void SpawnKarts()
    {
    }

    //Do your intro to the map
    public abstract IEnumerator DoIntro();

    private IEnumerator StartCountdown()
    {
        yield return null;
    }

    abstract public void HostUpdate();

    abstract public void ClientUpdate();

    protected void ForceStop()
    {
        StopAllCoroutines();
    }
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
    //The name of the Player
    private string name = "";
    public string Name
    {
        get { return name; }
        set { }
    }

    //What input the racer is -1 = AI, 0 - 3 Human
    private int human = -1;
    public int Human
    {
        get { return human; }
        set { }
    }

    private int aiStupidity = -1;
    public int AiStupidity
    {
        get { return aiStupidity; }
        set { }
    }

    //Race Loading Infomation //////////////////////////////////
    private int character;
    public int Character
    {
        get { return character; }
        set { }
    }

    private int hat;
    public int Hat
    {
        get { return hat; }
        set { }
    }

    private int kart;
    public int Kart
    {
        get { return kart; }
        set { }
    }

    private int wheel;
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

    //After Race Information //////////////////////////////////
    public int points;
    public int teams;

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
}

