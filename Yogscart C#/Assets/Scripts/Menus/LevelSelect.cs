using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour
{
    CurrentGameData gd;
    public GUISkin skin;

    bool state = false;

    int currentCup = 0, currentTrack = 0;

    //Transtition
    private float menuAlpha = 1f;
    public bool sliding = false, canClick = false, mouseLast = false;
    private Vector2 lastMousePos;
    private float[] cupScales, trackScales;

    private SoundManager sm;

    // Use this for initialization
    void Start()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        trackScales = new float[] { 1f, 1f, 1f, 1f };
        cupScales = new float[gd.tournaments.Length];

        for (int i = 0; i < cupScales.Length; i++)
            cupScales[i] = 1f;

        if (skin == null)
            skin = Resources.Load<GUISkin>("GUISkins/LevelSelect");

    }

    void OnGUI()
    {
        CurrentGameData.blackOut = false;
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = skin;
        GUI.depth = -5;

        canClick = false;

        GUIHelper.SetGUIAlpha(menuAlpha);
        Vector2 mousePos = GUIHelper.GetMousePosition();
        if (mousePos != lastMousePos)
        {
            mouseLast = true;
            lastMousePos = mousePos;
        }       

        float cupNameHeight = GUIHelper.height / 2f;
        float individualHeight = cupNameHeight / Mathf.Clamp(gd.tournaments.Length, 4, 8);
        Texture2D rectangle = Resources.Load<Texture2D>("UI/Level Selection/Rectangle");

        //Draw Tournaments
        for (int i = 0; i < gd.tournaments.Length; i++)
        {
            Rect rectRect = GUIHelper.CentreRect(new Rect(300, 270 + (i * individualHeight) + 10, 400, individualHeight - 20), cupScales[i]);
            GUI.DrawTexture(rectRect, rectangle);

            //Scale the Cup Names
            bool contains = mouseLast && rectRect.Contains(mousePos);
            if (!state && (contains || currentCup == i))
            {
                if (cupScales[i] < 1.1f)
                    cupScales[i] += Time.deltaTime;
                else
                    cupScales[i] = 1.1f;

                if (contains)
                {
                    currentCup = i;
                    canClick = true;
                }
            }
            else
            {
                if (cupScales[i] > 1f)
                    cupScales[i] -= Time.deltaTime;
                else
                    cupScales[i] = 1f;
            }

            GUIHelper.CentreRectLabel(new Rect(300, 270 + (i * individualHeight) + 15, 400, individualHeight - 30), cupScales[i], gd.tournaments[i].name, (currentCup == i)?Color.yellow:Color.white);
        }

        Race gamemode = (Race)CurrentGameData.currentGamemode;

        int tempCurrentCup = Mathf.Clamp(currentCup, 0, 4);

        //Draw Tracks
        for (int i = 0; i < gd.tournaments[tempCurrentCup].tracks.Length; i++)
        {
            Texture2D trackPreview;

            if (gamemode.raceType != RaceType.GrandPrix && state && i != currentTrack)
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].logo_GreyOut;
            }
            else
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].logo;
            }

            Rect previewRect = GUIHelper.CentreRect(new Rect(800 + (500 * (i % 2)), 150 + (390 * (i / 2)), 400, 365), trackScales[i]);

            //Scale the Track Names
            bool contains = mouseLast && previewRect.Contains(mousePos);
            if (state && (contains || currentTrack == i))
            {
                if (trackScales[i] < 1.1f)
                    trackScales[i] += Time.deltaTime;
                else
                    trackScales[i] = 1.1f;

                if (contains)
                {
                    currentTrack = i;
                    canClick = true;
                }
            }
            else
            {
                if (trackScales[i] > 1f)
                    trackScales[i] -= Time.deltaTime;
                else
                    trackScales[i] = 1f;
            }

            GUI.DrawTexture(previewRect, trackPreview, ScaleMode.ScaleToFit);
        }

        Rect timeRect = new Rect(800, 880, 1000, 200);

        if (state && gamemode.raceType == RaceType.TimeTrial && gd.tournaments[tempCurrentCup].tracks.Length > currentTrack && currentTrack != -1)
        {         
            string timeString = TimeManager.ToString(gd.tournaments[tempCurrentCup].tracks[currentTrack].bestTime);
            GUIHelper.OutLineLabel(timeRect, timeString, 3, Color.black);
        }
        else if(gamemode.raceType == RaceType.GrandPrix)
        {
            string rank = gd.tournaments[tempCurrentCup].lastRank[CurrentGameData.difficulty].ToString();
            if (rank == "NoRank")
                rank = "No Rank";

            GUIHelper.OutLineLabel(timeRect, rank, 3, Color.black);
        }

        //Inputs
        int vert = 0, hori = 0;
        bool submit = false, cancel = false;

        if (!sliding && (FindObjectOfType<MainMenu>() == null || !FindObjectOfType<MainMenu>().sliding))
        {
            vert = InputManager.controllers[0].GetMenuInput("MenuVertical");
            hori = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
            submit = (InputManager.controllers[0].GetMenuInput("Submit") != 0);
            cancel = (InputManager.controllers[0].GetMenuInput("Cancel") != 0);

            if(vert != 0 || hori != 0 || submit || cancel)
                mouseLast = false;

            if (canClick && Input.GetMouseButton(0))
                submit = true;

            if (hori != 0)
            {
                if (state)
                {
                    if (currentTrack <= 1)
                        currentTrack = MathHelper.NumClamp(currentTrack + hori, 0, 2);
                    else
                        currentTrack = MathHelper.NumClamp(currentTrack + hori, 2, 4);
                }
            }

            if (vert != 0)
            {
                if (state)
                    currentTrack += (2 * vert);
                else
                {
                    currentCup += vert;
                    currentTrack = 0;
                }
            }

            currentTrack = MathHelper.NumClamp(currentTrack, 0, 4);
            currentCup = MathHelper.NumClamp(currentCup, 0, gd.tournaments.Length);
        }

        if (gamemode.raceType != RaceType.Online)
        {
            Race.currentTrack = currentTrack;
            Race.currentCup = currentCup;
        }

        if (submit)
        {
            if (gamemode.raceType == RaceType.GrandPrix)
            {
                FinishLevelSelect();
            }
            else
            {
                if (!state)
                    state = true;
                else
                    FinishLevelSelect();
            }

            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));
        }

        if (cancel)
        {
            if(FindObjectOfType<MainMenu>() == null || state)
                sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/back"));

            if (!state)
            {
                CancelLevelSelect();
            }
            else
            {
                state = false;
            }
        }

        GUI.color = Color.white;

    }

    public void CancelLevelSelect()
    {
        //Cancel out of Level Select
        StartCoroutine(HideLevelSelect());
        currentCup = -1;
        currentTrack = -1;

        Race gamemode = (Race)CurrentGameData.currentGamemode;

        //If backing out of Level Select make sure Voting Screen is showed
        if (gamemode.raceType == RaceType.Online)
        {
            if (FindObjectOfType<VotingScreen>() == null)
                gd.gameObject.AddComponent<VotingScreen>();

            FindObjectOfType<VotingScreen>().ShowScreen();
        }
        else if(gamemode.raceType == RaceType.VSRace)
        {
            gamemode.CancelLevelSelect();
        }
    }

    public void ShowLevelSelect()
    {
        currentCup = 0;
        currentTrack = 0;

        if (FindObjectOfType<MainMenu>() != null)
            FindObjectOfType<MainMenu>().ChangeMenu(MainMenu.MenuState.LevelSelect);

        StartCoroutine(ActualShowLevelSelect());
    }
    private IEnumerator ActualShowLevelSelect()
    {
        sliding = true;

        float startTime = Time.time;
        float travelTime = 0.25f;
        //Slide Off //////////////////////////////
        menuAlpha = 0f;

        while (Time.time - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 1f;

        sliding = false;
    }

    private void FinishLevelSelect()
    {
        Race gamemode = (Race)CurrentGameData.currentGamemode;
        if (gamemode.raceType == RaceType.Online)
        {
            //Send data to server
            FindObjectOfType<NetworkRaceClient>().SendVote(currentCup, currentTrack);

            if(FindObjectOfType<VotingScreen>() == null)
                gd.gameObject.AddComponent<VotingScreen>();

            FindObjectOfType<VotingScreen>().ShowScreen();
        }
        else if(gamemode.raceType == RaceType.VSRace)
        {
            gamemode.FinishLevelSelect(currentCup, currentTrack);
        }

        StartCoroutine(HideLevelSelect());
    }

    public void ForceFinishLevelSelect()
    {
        StartCoroutine(HideLevelSelect());
    }

    private IEnumerator HideLevelSelect()
    {
        sliding = true;

        float startTime = Time.time;
        float travelTime = 0.25f;
        //Slide Off //////////////////////////////
        menuAlpha = 1f;

        while (Time.time - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 0f;

        sliding = false;
        enabled = false;
    }
}