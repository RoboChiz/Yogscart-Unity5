using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{

    public static bool canPause = false, onlineGame = false;
    private int paused = -1;
    private bool hide;

    private float guiAlpha;
    public float[] optionsSize;

    private const float fadeTime = 0.25f;
    private int currentSelection = 0;

    public void Awake()
    {
        optionsSize = new float[3] { 1, 1, 1 };
    }

    private void ShowPause()
    {
        currentSelection = 0;

        StartCoroutine(FadeGui(0f, 1f));

        if (!onlineGame) //Only freeze time if game is offline
            Time.timeScale = 0f;
        else
            FindObjectOfType<kartInput>().enabled = false;
    }

    public void HidePause()
    {
        StartCoroutine(FadeGui(guiAlpha, 0f));

        if (Time.timeScale != 1)
            Time.timeScale = 1f;

        if(onlineGame)
            FindObjectOfType<kartInput>().enabled = true;
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
        if (canPause)
        {
            if (InputManager.controllers != null)
            {
                for (int i = 0; i < InputManager.controllers.Count; i++)
                {
                    if (InputManager.controllers[i].GetRawMenuInput("Pause") != 0)
                    {
                        if (paused == -1)//Game is not currently paused so pause now
                        {
                            ShowPause();
                            paused = i;
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

            string[] options = new string[] { "Resume", "Options", "Quit" };

            for(int i = 0; i < options.Length; i++)
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

                GUIHelper.CentreRectLabel(new Rect(670, 400 + (i * 100), 580, 100), optionsSize[i], options[i], (currentSelection == i)?Color.yellow:Color.white);

            }

            if (paused != -1)
            {
                int vertical = InputManager.controllers[paused].GetRawMenuInput("MenuVertical");
                bool submitBool = (InputManager.controllers[paused].GetRawMenuInput("Submit") != 0);
        
                if (vertical != 0)
                {
                    currentSelection += vertical;
                    currentSelection = MathHelper.NumClamp(currentSelection, 0, 3);
                }

                if (submitBool)
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
        }

        if(GetComponent<Options>().enabled == false && paused != -1 && guiAlpha == 0)
        {
            StartCoroutine(FadeGui(0f, 1f));
        }

        GUIHelper.ResetColor();
    }
}
