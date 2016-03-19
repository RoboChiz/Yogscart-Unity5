using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LBType { Points, NoPoints, Sorted, TimeTrial, Tournament };

public class Leaderboard : MonoBehaviour
{

    CurrentGameData gd;

    public bool hidden = true;
    private bool movingGUI = false;
    const float slideTime = 0.5f;
    private float sideAmount = 0;

    public LBType state = LBType.Points;

    public List<DisplayRacer> racers;

    //Textures that can be loaded
    private Texture2D BoardTexture;

    private float pointCount;

    // Use this for initialization
    void Awake()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        BoardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");

        if (hidden)
            sideAmount = (Screen.width / 2f) + 20f;
        else
            sideAmount = 0f;

        racers = new List<DisplayRacer>();
    }

    public void StartLeaderBoard()
    {
        hidden = false;
        state = LBType.Points;
        pointCount = 0;
    }

    public void StartTimeTrial()
    {
        hidden = false;
        state = LBType.TimeTrial;
    }

    public void StartOnline()
    {
        hidden = false;
        state = LBType.NoPoints;
    }

    public void SecondStep()
    {
        List<DisplayRacer> holder = SortingScript.CalculatePoints(racers);

        for (var j = 0; j < holder.Count; j++)
        {
            StartCoroutine(ChangePosition(holder[j], j));
        }

        Debug.Log("Sorted!");
        Debug.Log("The best human Racer is " + BestHuman());
        state = LBType.Sorted;
    }

    void Update()
    {
        if (!movingGUI)
        {
            if (hidden && sideAmount < (Screen.width / 2f))
                StartCoroutine("HideGUI");
            if (!hidden && sideAmount > 0)
                StartCoroutine("ShowGUI");
        }
    }

    private IEnumerator ShowGUI()
    {
        movingGUI = true;

        float startTime = Time.time;

        while (Time.time - startTime < slideTime)
        {
            sideAmount = Mathf.Lerp((Screen.width / 2f) + 20f, 0f, (Time.time - startTime) / slideTime);
            yield return null;
        }
        sideAmount = 0f;

        movingGUI = false;
    }

    private IEnumerator HideGUI()
    {
        movingGUI = true;

        float startTime = Time.time;

        while (Time.time - startTime < slideTime)
        {
            sideAmount = Mathf.Lerp(0f, (Screen.width / 2f) + 20f, (Time.time - startTime) / slideTime);
            yield return null;
        }
        sideAmount = Screen.width / 2f + 20f;

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

        GUIStyle nStyle =  new GUIStyle(Resources.Load<GUISkin>("GUISkins/Main Menu").label);
        nStyle.fontSize = (int)(Mathf.Min(Screen.width, Screen.height) / 25f);

        GUI.skin.label = nStyle;

        GUIHelper.ResetColor();

        float optionSize = Screen.height / 16f;
        Rect BoardRect = new Rect(sideAmount + Screen.width / 2f, optionSize, Screen.width / 2f - optionSize, (optionSize) * 14f);
        GUI.DrawTexture(BoardRect, BoardTexture);

        GUI.BeginGroup(BoardRect);
        switch (state)
        {
            case LBType.TimeTrial:
                if (racers.Count > 0)
                {
                    float bestTime = gd.tournaments[Race.currentCup].tracks[Race.currentTrack].bestTime;
                    float playerTime = racers[0].timer;

                    if (playerTime <= bestTime)
                        GUI.Label(new Rect(10, 10, BoardRect.width - 20, BoardRect.height), "New Best Time!!!");
                    else
                        GUI.Label(new Rect(10, 10, BoardRect.width - 20, BoardRect.height), "You Lost!!!");

                    GUI.Label(new Rect(10, 10 + (optionSize), BoardRect.width - 20, optionSize), "Best Time");
                    GUI.Label(new Rect(10, 10 + 2 * (optionSize), BoardRect.width - 20, optionSize), TimeManager.ToString(bestTime));

                    GUI.Label(new Rect(10, 10 + 3 * (optionSize), BoardRect.width - 20, optionSize), "Your Time");
                    GUI.Label(new Rect(10, 10 + 4 * (optionSize), BoardRect.width - 20, optionSize), TimeManager.ToString(playerTime));
                }
                break;
            default:
                //Render Bars showing what place you came
                for (int i = 0; i < racers.Count; i++)
                {
                    DisplayRacer nRacer = racers[i];
                    if (nRacer.human != -1)
                    {
                        Texture2D humanTexture;

                        humanTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Winner_P" + (nRacer.human + 1).ToString());
                        Rect humanTextureRect = new Rect(0, ((nRacer.position + 1) * optionSize) - 7, BoardRect.width, optionSize + 14);
                        GUI.DrawTexture(humanTextureRect, humanTexture);
                    }
                }

                for (int i = 0; i < racers.Count; i++)
                {
                    DisplayRacer nRacer = racers[i];

                    //Render the position Number
                    Texture2D PosTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/" + (i + 1).ToString());

                    float Ratio = (optionSize) / PosTexture.height;
                    GUI.DrawTexture(new Rect(20, (i + 1) * optionSize, PosTexture.width * Ratio, optionSize), PosTexture);

                    Texture2D CharacterIcon = gd.characters[nRacer.character].icon;
                    GUI.DrawTexture(new Rect(20 + (PosTexture.width * Ratio), (nRacer.position + 1) * optionSize, (PosTexture.width * Ratio), optionSize), CharacterIcon, ScaleMode.ScaleToFit);

                    float nameWidth = BoardRect.width - 20 - ((PosTexture.width * Ratio * 2f));
                    Rect nameRect = new Rect(10 + (PosTexture.width * Ratio * 2f), (nRacer.position + 1) * optionSize, nameWidth, optionSize);

                    if (nRacer.human != -1 && nRacer.name != null && nRacer.name != "")
                    {
                        GUI.Label(nameRect, nRacer.name);
                    }
                    else
                    {
                        Texture2D NameTexture;
                        if (nRacer.human != -1)
                        {
                            NameTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/" + gd.characters[nRacer.character].name + "_Sel");
                        }
                        else
                            NameTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/" + gd.characters[nRacer.character].name);

                        GUI.DrawTexture(nameRect, NameTexture, ScaleMode.ScaleToFit);
                    }

                    if (state != LBType.NoPoints)
                    {
                        int points = nRacer.points;

                        if (state == LBType.Points)
                        {
                            int startPoint = points;

                            points -= (15 - i);
                            points = (int)Mathf.Clamp(points + (int)pointCount, 0f, startPoint);

                            int plusVal = (15 - i) - (int)pointCount;

                            if (plusVal > 0)
                                GUI.Label(new Rect(BoardRect.width - (PosTexture.width * Ratio * 2f) - 10, (nRacer.position + 1) * optionSize, PosTexture.width * Ratio, optionSize), "+ " + plusVal);
                        }

                        GUI.Label(new Rect(BoardRect.width - (PosTexture.width * Ratio) - 20, (nRacer.position + 1) * optionSize, PosTexture.width * Ratio, optionSize), points.ToString());
                    }
                }

                break;
        }
        GUI.EndGroup();

        if (state != LBType.TimeTrial && state != LBType.Tournament && pointCount < 15)
        {
            pointCount += Time.deltaTime*2;
        }

        pointCount = Mathf.Clamp(pointCount, 0f, 15f);
    }
}

public class DisplayRacer
{
    public string name;
    public int character, points, human;
    public float timer, position;

    const float slideTime = 0.5f;

    //All Human Players should have a name
    public DisplayRacer(int po, string n, int c, int p, float t)
    {
        name = n;
        character = c;
        points = p;
        timer = t;
        human = 0;
        position = po;
    }

    //All Human Players should have without name
    public DisplayRacer(int po, int h, int c, int p, float t)
    {
        human = h;
        character = c;
        points = p;
        timer = t;
        position = po;
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
    }

    public DisplayRacer(Racer racer)
    {
        human = racer.Human;
        character = racer.Character;
        points = racer.points;
        timer = racer.timer;
        position = racer.position;
    }

}