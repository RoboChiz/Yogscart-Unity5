using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{

    public static bool canPause = false, onlineGame = false;
    public int paused = -1, pauseHold = -1;

    private float guiAlpha;
    public float[] optionsSize;

    private const float fadeTime = 0.25f;
    private int currentSelection = 0;
    private SoundManager sm;

    private List<string> options;

    public void Awake()
    {
        optionsSize = new float[3] { 1, 1, 1 };
    }

    public void Start()
    {
        sm = FindObjectOfType<SoundManager>();
    }

    public void ShowPause() { ShowPause(false); }

    public void ShowPause(bool usePauseHold)
    {
        currentSelection = 0;

        if (usePauseHold && pauseHold != -1)
            paused = pauseHold;

        pauseHold = -1;

        StartCoroutine(FadeGui(0f, 1f));

        if (!onlineGame) //Only freeze time if game is offline
        {
            Time.timeScale = 0f;         
        }

        //Turn off everyone's KartInput
        foreach (KartInput ki in FindObjectsOfType<KartInput>())
            ki.enabled = false;
    }


    public void HidePause() { HidePause(true); }

    public void HidePause(bool fixTime)
    {
        StartCoroutine(FadeGui(guiAlpha, 0f));

        if (fixTime && Time.timeScale != 1)
            Time.timeScale = 1f;

        //Turn on everyone's KartInput
        foreach (KartInput ki in FindObjectsOfType<KartInput>())
            ki.enabled = true;

        foreach (KartItem ki in FindObjectsOfType<KartItem>())
            ki.inputLock = true;
    }

    private IEnumerator FadeGui(float start, float end)
    {
        float startTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - startTime < fadeTime)
        {
            guiAlpha = Mathf.Lerp(start, end, (Time.realtimeSinceStartup - startTime) / fadeTime);
            yield return null;
        }

        guiAlpha = end;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (canPause && !FindObjectOfType<Options>().enabled)
        {
            if (InputManager.controllers != null)
            {
                for (int i = 0; i < InputManager.controllers.Count; i++)
                {
                    if (InputManager.controllers[i].GetButtonWithLock("Pause"))
                    {
                        if (paused == -1)//Game is not currently paused so pause now
                        {                           
                            paused = i;
                            ShowPause();
                        }
                        else if (i == paused)//Otherwise if the person who pressed before has just pressed again, close the pause menu
                        {
                            HidePause();
                            paused = -1;
                        }
                    }
                }
            }
        }
	}

    void OnGUI()
    {

        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Leaderboard");

        if (guiAlpha > 0)
        {
            Texture2D boardTexture = Resources.Load<Texture2D>("UI/GrandPrix Positions/Backing");
            GUI.DrawTexture(new Rect(660, 100, 600, 800), boardTexture);

            options = new List<string>() { "Resume", "Options", "Quit" };

            Race race = FindObjectOfType<Race>();
            if (race != null && race is TimeTrial)
                options.Insert(2, "Restart");                       

            if (optionsSize.Length != options.Count)
            {
                float[] holder = optionsSize;
                optionsSize = new float[options.Count];

                for (int i = 0; i < Mathf.Min(holder.Length, optionsSize.Length); i++)
                    optionsSize[i] = holder[i];
            }

            for (int i = 0; i < options.Count; i++)
            {
                if(currentSelection == i)
                {
                    if (optionsSize[i] < 1.5f)
                        optionsSize[i] += Time.unscaledDeltaTime * 4f;
                    else
                        optionsSize[i] = 1.5f;
                }                               
                else
                {
                    if (optionsSize[i] > 1f)
                        optionsSize[i] -= Time.unscaledDeltaTime * 4f;
                    else
                        optionsSize[i] = 1f;
                }

                float yCentre = 500 - ((options.Count * 100) / 2f);

                Rect optionRect = new Rect(670, yCentre + (i * 100), 580, 100);
                GUIHelper.CentreRectLabel(optionRect, optionsSize[i], options[i], (currentSelection == i)?Color.yellow:Color.white);

                if(Cursor.visible && optionRect.Contains(GUIHelper.GetMousePosition()))
                {
                    currentSelection = i;
                }

                if(Cursor.visible && GUI.Button(optionRect,""))
                {
                    currentSelection = i;
                    DoInput();
                }

            }

            if (paused != -1 && guiAlpha == 1f)
            {
                int vertical = InputManager.controllers[paused].GetRawIntInputWithLock("MenuVertical");
                bool submitBool = (InputManager.controllers[paused].GetButtonWithLock("Submit"));
        
                if (vertical != 0)
                {
                    currentSelection += vertical;
                    currentSelection = MathHelper.NumClamp(currentSelection, 0, options.Count);
                }

                if (submitBool)
                {
                    sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));

                    DoInput();
                }
            }
        }

        if(GetComponent<Options>().enabled == false && paused != -1 && guiAlpha == 0)
        {
            StartCoroutine(FadeGui(0f, 1f));
        }

        GUIHelper.ResetColor();
    }

    private void DoInput()
    {
        switch (options[currentSelection])
        {
            case "Resume":
                HidePause();
                paused = -1;
                break;
            case "Options":
                Debug.Log("Load the options menu");
                StartCoroutine(FadeGui(1f, 0f));

                GetComponent<Options>().enabled = true;
                GetComponent<Options>().ShowOptions();
                break;
            case "Restart":
                HidePause();
                paused = -1;

                FindObjectOfType<GameMode>().Restart();
                break;
            case "Next Race":
                HidePause(false);
                pauseHold = paused;
                paused = -1;

                FindObjectOfType<Race>().NextRace();
                break;
            case "Quit":
                HidePause();

                paused = -1;
                canPause = false;

                //Change Pitch Back
                FindObjectOfType<SoundManager>().SetMusicPitch(1f);
                FindObjectOfType<GameMode>().EndGamemode();
                break;
        }
    }
}
