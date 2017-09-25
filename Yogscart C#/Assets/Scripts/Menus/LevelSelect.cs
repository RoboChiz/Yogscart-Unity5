using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour
{
    CurrentGameData gd;
    public GUISkin skin;
    bool state = false;

    private int currentCup = 0, currentTrack = 0;
    public int trackNum = 4;

    //Transtition
    private float menuAlpha = 1f;
    public bool sliding = false, canClick = false;
    private float[] cupScales, trackScales;

    private SoundManager sm;

    // Use this for initialization
    void Start()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        gd.CountGhosts();

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

        float cupNameHeight = GUIHelper.height / 2f;
        float individualHeight = cupNameHeight / Mathf.Clamp(gd.tournaments.Length, 4, 8);
        Texture2D rectangle = Resources.Load<Texture2D>("UI/Level Selection/Rectangle");
        float startY = 200;

        //Draw Tournaments
        for (int i = 0; i < gd.tournaments.Length; i++)
        {
            Rect rectRect = GUIHelper.CentreRect(new Rect(300, startY + (i * individualHeight) + 10, 400, individualHeight - 20), cupScales[i]);
            GUI.DrawTexture(rectRect, rectangle);

            //Scale the Cup Names
            bool contains = Cursor.visible && rectRect.Contains(mousePos);
            if (!state && (contains || currentCup == i))
            {
                if (cupScales[i] < 1.1f)
                    cupScales[i] += Time.unscaledDeltaTime;
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
                    cupScales[i] -= Time.unscaledDeltaTime;
                else
                    cupScales[i] = 1f;
            }

            GUIHelper.CentreRectLabel(new Rect(300, startY + (i * individualHeight) + 15, 400, individualHeight - 30), cupScales[i], gd.tournaments[i].name, (currentCup == i)?Color.yellow:Color.white);
        }

        Race gamemode = (Race)CurrentGameData.currentGamemode;

        int tempCurrentCup = Mathf.Clamp(currentCup, 0, 4);

        //Draw Tracks
        for (int i = 0; i < gd.tournaments[tempCurrentCup].tracks.Length; i++)
        {
            Texture2D trackPreview;

            if ((gamemode is TimeTrial || gamemode is VSRace) && state && i != currentTrack)
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].logo_GreyOut;
            }
            else
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].logo;
            }

            Rect previewRect = GUIHelper.CentreRect(new Rect(800 + (500 * (i % 2)), startY - 120 + (390 * (i / 2)), 400, 365), trackScales[i]);

            //Scale the Track Names
            bool contains = Cursor.visible && previewRect.Contains(mousePos);
            if (state && (contains || currentTrack == i))
            {
                if (trackScales[i] < 1.1f)
                    trackScales[i] += Time.unscaledDeltaTime;
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
                    trackScales[i] -= Time.unscaledDeltaTime;
                else
                    trackScales[i] = 1f;
            }

            GUI.DrawTexture(previewRect, trackPreview, ScaleMode.ScaleToFit);
        }

        Rect timeRect = new Rect(810, startY + 640, 1000, 100);
        Rect ghostRect = new Rect(810, startY + 690, 1000, 100);

        if (state && gamemode is TimeTrial && gd.tournaments[tempCurrentCup].tracks.Length > currentTrack && currentTrack != -1)
        {
            string timeString = "";
            if (gd.tournaments[tempCurrentCup].tracks[currentTrack].bestTime != 0f)
                timeString = TimeManager.ToString(gd.tournaments[tempCurrentCup].tracks[currentTrack].bestTime);
            else
                timeString = "N/A";

            GUIHelper.OutLineLabel(timeRect, "Best Time:  " + timeString, 2, Color.black);

            string ghostString = gd.tournaments[tempCurrentCup].tracks[currentTrack].ghosts.ToString();
            GUIHelper.OutLineLabel(ghostRect, "Local Ghosts:  " + ghostString, 2, Color.black);
        }
        else if(gamemode is TournamentRace && !(gamemode is VSRace))
        {
            string rank = gd.tournaments[tempCurrentCup].lastRank[CurrentGameData.difficulty].ToString();
            if (rank == "NoRank")
                rank = "No Rank";

            GUIHelper.OutLineLabel(timeRect, rank, 2, Color.black);
        }
        else if(gamemode is VSRace)
        {
            if (GetComponent<MainMenu>() != null)
            {
                //Draw Inputs
                Rect leftRect = new Rect(1000, startY + 650, 160, 80);
                Rect rightRect = new Rect(1450, startY + 650, 160, 80);

                GUI.DrawTexture(leftRect, InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Xbox360 ? Resources.Load<Texture2D>("UI/Options/LB") : Resources.Load<Texture2D>("UI/Options/Q"));
                GUI.DrawTexture(rightRect, InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Xbox360 ? Resources.Load<Texture2D>("UI/Options/RB") : Resources.Load<Texture2D>("UI/Options/E"));

                GUIStyle customButton = new GUIStyle();

                if(GUI.Button(leftRect, "", customButton))
                {
                    trackNum = MathHelper.NumClamp(trackNum - 1, 1, 64);
                }

                if (GUI.Button(rightRect, "", customButton))
                {
                    trackNum = MathHelper.NumClamp(trackNum + 1, 1, 64);
                }

                GUIHelper.OutLineLabel(timeRect, "Races: " + trackNum, 2, Color.black);
            }
            else
            {
                VSRace vs = gamemode as VSRace;
                GUIHelper.OutLineLabel(timeRect, "Race " + (gamemode.currentRace + 1).ToString() + " / " + vs.raceCount, 2, Color.black);
            }
        }

        //Inputs
        int vert = 0, hori = 0, tabChange = 0;
        bool submit = false, cancel = false;

        if (!sliding && (FindObjectOfType<MainMenu>() == null || !FindObjectOfType<MainMenu>().sliding))
        {
            vert = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");
            hori = InputManager.controllers[0].GetIntInputWithLock("MenuHorizontal");
            submit = InputManager.controllers[0].GetButtonWithLock("Submit");
            cancel = InputManager.controllers[0].GetButtonWithLock("Cancel");
            tabChange = InputManager.controllers[0].GetIntInputWithLock("TabChange");

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

                    hori = 0;
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

                vert = 0;
            }

            currentTrack = MathHelper.NumClamp(currentTrack, 0, 4);
            currentCup = MathHelper.NumClamp(currentCup, 0, gd.tournaments.Length);

            if(tabChange != 0 && gamemode is VSRace && GetComponent<MainMenu>() != null)
            {
                trackNum = MathHelper.NumClamp(trackNum + tabChange, 1, 64);
                tabChange = 0;
            }
        }

        //if (gamemode is OnlineRace)
       // {
       //     Race.currentTrack = currentTrack;
       //     Race.currentCup = currentCup;
       // }

        if (submit)
        {
            if (gamemode is TournamentRace && !(gamemode is VSRace))
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
            submit = false;
        }

        if (cancel)
        {
            DoCancel();
            cancel = false;
        }

        GUI.color = Color.white;

    }

    public void DoCancel()
    {
        if (FindObjectOfType<MainMenu>() == null || state)
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

    public void CancelLevelSelect()
    {
        //Cancel out of Level Select
        StartCoroutine(HideLevelSelect());
        currentCup = -1;
        currentTrack = -1;

        GameMode gameMode = CurrentGameData.currentGamemode;

        //If backing out of Level Select make sure Voting Screen is showed
        /*if (gamemode.raceType == RaceType.Online)
        {
            if (FindObjectOfType<VotingScreen>() == null)
                gd.gameObject.AddComponent<VotingScreen>();

            FindObjectOfType<VotingScreen>().ShowScreen();
        }*/

        if(FindObjectOfType<Replay>() != null)
        {
            FindObjectOfType<Replay>().GoBack();
        }
        else if (gameMode is VSRace)
        {
            VSRace race = gameMode as VSRace;
            race.CancelLevelSelect();
        }
    }

    public void ShowLevelSelect()
    {
        currentCup = 0;
        currentTrack = 0;
        trackNum = 4;

        if (FindObjectOfType<MainMenu>() != null && FindObjectOfType<MainMenu>().state != MainMenu.MenuState.LevelSelect)
            FindObjectOfType<MainMenu>().ChangeMenu(MainMenu.MenuState.LevelSelect);

        StartCoroutine(ActualShowLevelSelect());
    }
    private IEnumerator ActualShowLevelSelect()
    {
        sliding = true;

        float startTime = Time.realtimeSinceStartup;
        float travelTime = 0.25f;
        //Slide Off //////////////////////////////
        menuAlpha = 0f;

        while (Time.realtimeSinceStartup - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(0f, 1f, (Time.realtimeSinceStartup - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 1f;

        sliding = false;
    }

    private void FinishLevelSelect()
    {
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;

        PauseMenu pm = FindObjectOfType<PauseMenu>();
        if (pm != null)
        {
            pm.pauseHold = -1;
            pm.paused = -1;
            PauseMenu.canPause = false;
        }

        Race gameMode = CurrentGameData.currentGamemode as Race;
        /*if (gamemode.raceType == RaceType.Online)
        {
            //Send data to server
            FindObjectOfType<NetworkRaceClient>().SendVote(currentCup, currentTrack);

            if(FindObjectOfType<VotingScreen>() == null)
                gd.gameObject.AddComponent<VotingScreen>();

            FindObjectOfType<VotingScreen>().ShowScreen();
        }*/

        gameMode.FinishLevelSelect(currentCup, currentTrack);
        StartCoroutine(HideLevelSelect());
    }

    public void ForceFinishLevelSelect()
    {
        StartCoroutine(HideLevelSelect());
    }

    private IEnumerator HideLevelSelect()
    {
        sliding = true;

        float startTime = Time.realtimeSinceStartup;
        float travelTime = 0.25f;
        //Slide Off //////////////////////////////
        menuAlpha = 1f;

        while (Time.realtimeSinceStartup - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(1f, 0f, (Time.realtimeSinceStartup - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 0f;

        sliding = false;
        enabled = false;
    }

    private IEnumerator KillLevelSelect()
    {
        yield return HideLevelSelect();
        Destroy(this);
    }
}