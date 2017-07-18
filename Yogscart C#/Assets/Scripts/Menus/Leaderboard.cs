using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LBType { AddingPoints, AddedPoints, NoPoints, Sorted, TimeTrial };

public class Leaderboard : MonoBehaviour
{

    CurrentGameData gd;

    public bool hidden = true;
    private bool movingGUI = false;
    const float slideTime = 0.5f;
    private float sideAmount = 0, guiAlpha = 0f;

    public bool showing { get; private set; }

    public LBType state = LBType.AddingPoints;

    public List<DisplayRacer> racers;

    //Textures that can be loaded
    private Texture2D BoardTexture;

    private float pointCount, pcSpeed = 2f;
    private bool startedSecond = false, speeding = false;

    private Race race;

    // Use this for initialization
    void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        BoardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");

        if (hidden)
            sideAmount = 1920;
        else
            sideAmount = 960;

        pcSpeed = 2f;
    }

    public void StartLeaderBoard(Race gamemode)
    {
        race = gamemode;
        StartCoroutine(ActualStartLeaderBoard());   
    }

    public IEnumerator ActualStartLeaderBoard()
    {
        hidden = false;

        while(guiAlpha < 0.9f)
        {
            yield return null;
        }

        state = LBType.AddingPoints;
        pointCount = 0;
    }

    public void StartTimeTrial(TimeTrial gamemode)
    {
        race = gamemode;
        hidden = false;
        state = LBType.TimeTrial;
    }

    public void StartOnline()
    {
        hidden = false;
        state = LBType.NoPoints;
    }

    public void DoInput()
    {
        if(state == LBType.AddingPoints && !startedSecond)
        {
            startedSecond = true;
            StartCoroutine(SpeedUpPoints());
        }
        else if(state == LBType.AddedPoints && !speeding)
        {
            List<DisplayRacer> holder = SortingScript.CalculatePoints(racers);
            for (var j = 0; j < holder.Count; j++)
            {
                StartCoroutine(ChangePosition(holder[j], j));
            }

            Debug.Log("Sorted!");
            //Debug.Log("The best human Racer is " + BestHuman());
            state = LBType.Sorted;
        }
    }

    private IEnumerator SpeedUpPoints()
    {
        speeding = true;

        if (pointCount < 15f)
            pcSpeed = 20f;

        while (pointCount < 15f)
            yield return null;    
    
        speeding = false;

        GoToAddedPoints();
    }

    private void GoToAddedPoints()
    {
        state = LBType.AddedPoints;

        //Check to see if List is sorted
        bool sorted = true;
        for (var j = 0; j < racers.Count; j++)
        {
            if (j < racers.Count - 1)
            {
                if (racers[j].points < racers[j + 1].points)
                {
                    sorted = false;
                    break;
                }
            }
        }

        if (sorted)
            state = LBType.Sorted;
    }

    void Update()
    {
        if (!movingGUI)
        {
            if (hidden && sideAmount < 1920)
                StartCoroutine("HideGUI");
            if (!hidden && sideAmount > 960)
                StartCoroutine("ShowGUI");
        }

        showing = guiAlpha != 0f;
    }

    private IEnumerator ShowGUI()
    {
        movingGUI = true;

        float startTime = Time.time;

        while (Time.time - startTime < slideTime)
        {
            sideAmount = Mathf.Lerp(1920, 960, (Time.time - startTime) / slideTime);
            guiAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / slideTime);
            yield return null;
        }

        sideAmount = 960;
        guiAlpha = 1f;

        movingGUI = false;
    }

    private IEnumerator HideGUI()
    {
        movingGUI = true;

        float startTime = Time.time;

        while (Time.time - startTime < slideTime)
        {
            sideAmount = Mathf.Lerp(960, 1920, (Time.time - startTime) / slideTime);
            guiAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / slideTime);
            yield return null;
        }

        sideAmount = 1920;
        guiAlpha = 0f;

        movingGUI = false;
    }

    private IEnumerator ChangePosition(DisplayRacer toChange, int i)
    {
        if (toChange.position != i)
        {
            float startTime = Time.time;
            float startPosition = toChange.position;

            while (Time.time - startTime < slideTime)
            {
                toChange.position = Mathf.Lerp(startPosition, i, (Time.time - startTime) / slideTime);
                yield return null;
            }

            toChange.position = i;
        }
    }

    private int BestHuman()
    {
        int returnVal = -1;

        if (racers != null)
        {
            for (int i = 0; i < racers.Count; i++)
            {
                if (racers[i].human != -1 && (returnVal == -1 || (racers[i].points > racers[returnVal].points)))
                {
                    returnVal = i;
                }
            }
        }

        return returnVal;
    }

    void OnGUI()
    {
        //Version 2
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Leaderboard");
        GUI.matrix = GUIHelper.GetMatrix();
        GUIHelper.SetGUIAlpha(guiAlpha);

        float optionHeight = 67.5f;
        Rect BoardRect = new Rect(970, optionHeight, 940, 1000);
        GUI.DrawTexture(BoardRect, BoardTexture);

        GUI.BeginGroup(BoardRect);
        switch (state)
        {
            case LBType.TimeTrial:
                if (racers != null && racers.Count > 0)
                {
                    float bestTime = gd.tournaments[race.currentCup].tracks[race.currentTrack].bestTime;
                    float playerTime = racers[0].timer;

                    TimeTrial tt = FindObjectOfType<TimeTrial>();

                    if(tt == null || tt.ghost == null)
                    { 
                        if (playerTime <= bestTime || bestTime == 0)
                            GUI.Label(new Rect(10, 10, BoardRect.width - 20, BoardRect.height), "New Best Time!!!");
                        else
                            GUI.Label(new Rect(10, 10, BoardRect.width - 20, BoardRect.height), "You Lost!!!");

                        GUI.Label(new Rect(10, 10 + (optionHeight), BoardRect.width - 20, optionHeight), "Best Time");
                        GUI.Label(new Rect(10, 10 + 2 * (optionHeight), BoardRect.width - 20, optionHeight), TimeManager.ToString(bestTime));

                        GUI.Label(new Rect(10, 10 + 3 * (optionHeight), BoardRect.width - 20, optionHeight), "Your Time");
                        GUI.Label(new Rect(10, 10 + 4 * (optionHeight), BoardRect.width - 20, optionHeight), TimeManager.ToString(playerTime));
                    }
                    else
                    {
                        string text = "";

                        if(playerTime < bestTime || bestTime == 0)
                        {
                            if (playerTime < tt.ghost.time)
                                text = "You beat the Ghost and set a new Best Time!!!";
                            else
                                text = "You didn't beat the Ghost but set a new Best Time!!!";
                        }
                        else
                        {
                            if (playerTime < tt.ghost.time)
                                text = "You beat the ghost, but didn't set a new Best Time!!!";
                            else
                                text = "You lose!!";
                        }

                        GUI.Label(new Rect(10, 10 + (optionHeight), BoardRect.width - 20, optionHeight), text);

                        GUI.Label(new Rect(10, 10 + (3 * optionHeight), BoardRect.width - 20, optionHeight), "Best Time");
                        GUI.Label(new Rect(10, 10 + (4 * optionHeight), BoardRect.width - 20, optionHeight), TimeManager.ToString(bestTime));

                        GUI.Label(new Rect(10, 10 + (5 * optionHeight), BoardRect.width - 20, optionHeight), "Ghost Time");
                        GUI.Label(new Rect(10, 10 + (6 * optionHeight), BoardRect.width - 20, optionHeight), TimeManager.ToString(tt.ghost.time));

                        GUI.Label(new Rect(10, 10 + (7 * optionHeight), BoardRect.width - 20, optionHeight), "Your Time");
                        GUI.Label(new Rect(10, 10 + (8 * optionHeight), BoardRect.width - 20, optionHeight), TimeManager.ToString(playerTime));
                    }
                }
                break;
            default:
                if (racers != null)
                {
                    for (int i = 0; i < racers.Count; i++)
                    {
                        DisplayRacer nRacer = racers[i];

                        //Render Bars showing what place you came
                        if (nRacer.human >= 0)
                        {
                            Texture2D humanTexture;

                            humanTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Winner_P" + (nRacer.human + 1).ToString());
                            Rect humanTextureRect = new Rect(0, ((nRacer.position + 1) * optionHeight) - 15, BoardRect.width, optionHeight + 30);
                            GUI.DrawTexture(humanTextureRect, humanTexture);
                        }

                        //Render the Position and Character head of the Racer
                        string posString = nRacer.finished ? ("UI/GrandPrix Positions/" + (i + 1).ToString()) : "UI/GrandPrix Positions/DNF";
                        Texture2D PosTexture = Resources.Load<Texture2D>(posString);
                        GUI.DrawTexture(new Rect(40, (i + 1) * optionHeight, optionHeight, optionHeight), PosTexture, ScaleMode.ScaleToFit);

                        Texture2D CharacterIcon = gd.characters[nRacer.character].icon;
                        GUI.DrawTexture(new Rect(40 + optionHeight, (nRacer.position + 1) * optionHeight, optionHeight, optionHeight), CharacterIcon, ScaleMode.ScaleToFit);

                        Rect nameRect = new Rect(50 + (optionHeight * 2), (nRacer.position + 1) * optionHeight, 576, optionHeight);

                        //Draw the name of the Racer if it is available 
                        if (nRacer.human != -1 && nRacer.name != null && nRacer.name != "")
                            GUI.Label(nameRect, nRacer.name);
                        else
                        {
                            Texture2D nameTexture;
                            if (nRacer.human >= 0)
                                nameTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/" + gd.characters[nRacer.character].name + "_Sel");
                            else
                                nameTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/" + gd.characters[nRacer.character].name);

                            GUI.DrawTexture(nameRect, nameTexture, ScaleMode.ScaleToFit);
                        }

                        //Draw the overall points for the Racer
                        if (state != LBType.NoPoints)
                        {
                            int endPoints = nRacer.points;
                            int startPoints = nRacer.finished ? endPoints - 15 + i : endPoints;

                            int renderPoints = (int)Mathf.Clamp(startPoints + pointCount, startPoints, endPoints);
                            int plusVal = nRacer.finished ? (15 - i) - (int)pointCount : 0;

                            GUIHelper.OutLineLabel(new Rect(BoardRect.width - 30 - (optionHeight * 2.5f), (nRacer.position + 1) * optionHeight, optionHeight, optionHeight), renderPoints.ToString(), 2);

                            if (plusVal > 0)
                                GUIHelper.OutLineLabel(new Rect(BoardRect.width - 30 - (optionHeight * 1.5f), (i + 1) * optionHeight, optionHeight * 1.5f, optionHeight), "+ " + plusVal, 2);
                        }
                    }
                }
                break;
        }

        //Render Continue Icon in Corner
        GUIHelper.OutLineLabel(new Rect(BoardRect.width - 175, BoardRect.height - 90, 150, 50), "Next", 2);
        GUI.DrawTexture(new Rect(BoardRect.width - 225, BoardRect.height - 90, 50, 50), Resources.Load<Texture2D>("UI/Options/" + ((InputManager.controllers[0].controlLayout.Type == ControllerType.Keyboard) ? "Return" : "A")), ScaleMode.ScaleToFit);

        GUI.EndGroup();

        if (state != LBType.TimeTrial && pointCount < 15f)
        {
            pointCount += Time.deltaTime * pcSpeed;
        }
        else if(state == LBType.AddingPoints)
        {
            GoToAddedPoints();
            speeding = false;
        }

       // pointCount = Mathf.Clamp(pointCount, 0f, float.);

        //Reset the GUI Alpha after changing it
        GUIHelper.ResetColor();
    }
}

public class DisplayRacer
{
    public string name;
    public int character, points, human;
    public float timer, position;//Position is position on Screen not race Position
    public bool finished = false;

    const float slideTime = 0.5f;

    //All Human Players should have a name
    public DisplayRacer(int po, string n, int c, int p, float t, bool f)
    {
        name = n;
        character = c;
        points = p;
        timer = t;
        human = 0;
        position = po;
        finished = f;
    }

    //All Human Players should have without name
    public DisplayRacer(int po, int h, int c, int p, float t)
    {
        human = h;
        character = c;
        points = p;
        timer = t;
        position = po;
        finished = true;
    }

    //AI Racers won't have a name
    public DisplayRacer(int po, int c, int p, float t)
    {
        name = "AI Racer";
        character = c;
        points = p;
        timer = t;
        human = -1;
        position = po;
        finished = true;
    }

    public DisplayRacer(Racer racer)
    {
        human = racer.Human;
        character = racer.Character;
        points = racer.points;
        timer = racer.timer;
        position = racer.position;
        finished = true;
    }

    public override string ToString()
    {
        return position + ";" + name + ";" + character + ";" + points + ";" + timer + ";" + human + ";" + finished;
    }

    public DisplayRacer(string readFrom)
    {
        string[] splitString = readFrom.Split(";"[0]);

        position = float.Parse(splitString[0]);
        name = splitString[1];
        character = int.Parse(splitString[2]);
        points = int.Parse(splitString[3]);
        timer = float.Parse(splitString[4]);
        human = int.Parse(splitString[5]);
        finished = bool.Parse(splitString[6]);
    }
}