using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {

    CurrentGameData gd;
    public GUISkin skin;

    bool state = false;

    int currentCup = 0, currentTrack = 0;

    //Transtition
    private float scale = 1f;
    private float menuAlpha = 1f;
    private bool sliding = false;

    // Use this for initialization
    void Start()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
    }

    void OnGUI()
    {
        CurrentGameData.blackOut = false;
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.matrix *= Matrix4x4.TRS(new Vector3(GUIHelper.width * (1-scale), GUIHelper.height * (1-scale)), Quaternion.identity, new Vector3(scale, scale, scale));
        GUI.skin = skin;

        Color nWhite = Color.white;
        nWhite.a = menuAlpha;
        GUI.color = nWhite;

        float cupNameHeight = GUIHelper.height / 2f;
        float individualHeight = cupNameHeight / Mathf.Clamp(gd.tournaments.Length,4,8);
        Texture2D rectangle = Resources.Load<Texture2D>("UI/Level Selection/Rectangle");

        //Draw Tournaments
        for (int i = 0; i < gd.tournaments.Length; i++)
        {
            GUI.DrawTexture(new Rect(300, 270 + (i * individualHeight) + 10, 400, individualHeight - 20), rectangle);

            if (currentCup == i)
            {
                nWhite = Color.yellow;
                nWhite.a = menuAlpha;
                GUI.color = nWhite;
            }

            GUI.Label(new Rect(300, 270 + (i * individualHeight) + 15, 400, individualHeight - 30), gd.tournaments[i].name);

            nWhite = Color.white;
            nWhite.a = menuAlpha;
            GUI.color = nWhite;

        }

        Race gamemode = (Race)CurrentGameData.currentGamemode;

        int tempCurrentCup = Mathf.Clamp(currentCup, 0, 4);

        //Draw Tracks
        for (int i = 0; i < gd.tournaments[tempCurrentCup].tracks.Length; i++)
        {
            Texture2D trackPreview;

            if(gamemode.raceType != RaceType.GrandPrix && (!state || i != currentTrack))
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].preview_GreyOut;
            }
            else
            {
                trackPreview = gd.tournaments[tempCurrentCup].tracks[i].preview;
            }

            Rect previewRect = new Rect(800 + (500 * (i%2)), 150 + (390 * (i/2)), 400, 365);

            GUI.DrawTexture(previewRect, trackPreview, ScaleMode.ScaleToFit);
        }

        if (gamemode.raceType == RaceType.TimeTrial && gd.tournaments[tempCurrentCup].tracks.Length > currentTrack && currentTrack != -1)
        {
            Rect timeRect = new Rect(800, 880, 1000, 200);
            string timeString = TimeManager.ToString(gd.tournaments[tempCurrentCup].tracks[currentTrack].bestTime);
            GUIHelper.OutLineLabel(timeRect, timeString, 2, Color.black);
        }

        //Inputs
        int vert = 0, hori = 0;
        bool submit = false, cancel = false;

        if (!sliding)
        {
            vert = InputManager.controllers[0].GetMenuInput("MenuVertical");
            hori = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
            submit = (InputManager.controllers[0].GetMenuInput("Submit") != 0);
            cancel = (InputManager.controllers[0].GetMenuInput("Cancel") != 0);

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

        gamemode.currentTrack = currentTrack;
        gamemode.currentCup = currentCup;

        if(submit)
        {          
            if(gamemode.raceType == RaceType.GrandPrix)
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
        }

        if(cancel)
        {
            if(!state)
            {
                //Cancel out of Level Select
                StartCoroutine(HideLevelSelect());
                currentCup = -1;
                currentTrack = -1;
            }
            else
            {
                state = false;
            }
        }

        GUI.color = Color.white;

    }

    public void ShowLevelSelect()
    {
        currentCup = 0;
        currentTrack = 0;

        StartCoroutine(ActualShowLevelSelect());
    }
    private IEnumerator ActualShowLevelSelect()
    {
        sliding = true;

        float startTime = Time.time;
        float travelTime = 0.25f;
        //Slide Off //////////////////////////////
        menuAlpha = 0f;
        scale = 0.9f;

        while (Time.time - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
            scale = Mathf.Lerp(0.9f, 1f, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 1f;
        scale = 1f;

        sliding = false;
    }

    private void FinishLevelSelect()
    {
        StartCoroutine(HideLevelSelect());
    }

    private IEnumerator HideLevelSelect()
    {
        sliding = true;

        float startTime = Time.time;
        float travelTime = 0.25f;
        float endScale = 0.9f;
        //Slide Off //////////////////////////////
        menuAlpha = 1f;
        scale = 1f;

        while (Time.time - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
            scale = Mathf.Lerp(1f, endScale, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 0f;
        scale = endScale;

        sliding = false;

        enabled = false;
    }
}