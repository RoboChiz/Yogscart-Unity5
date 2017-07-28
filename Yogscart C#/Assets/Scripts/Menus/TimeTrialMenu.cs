using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TimeTrialMenu : MonoBehaviour
{
    private float guiAlpha = 0f;
    public bool showing { get { return guiAlpha != 0; } }
    public bool hiding = false;

    public int selectedOption = 0, selectedTT = 0;
    private float[] optionScales;

    enum TTMMenuState { None, ChoosingDev, ChoosingLocal}
    TTMMenuState menuState = TTMMenuState.None;

    bool choosingGhost = false;

    private readonly string[] options = new string[] { "Race by Yourself", "Race Developer Ghost", "Race Ghost" };
    private SoundManager sm;
    private CurrentGameData gd;
    private TimeTrial timeTrial;

    public List<GhostData> validTimeTrials;
    public List<GhostData> devTimeTrials;

    public List<GhostData> current;

    private float[] timeTrialScales;

    private Vector2 sliderPosition;
    private float deleteButtonScale;

    public void Show()
    {
        sm = FindObjectOfType<SoundManager>();
        gd = FindObjectOfType<CurrentGameData>();
        timeTrial = FindObjectOfType<TimeTrial>();

        selectedOption = 0;
        selectedTT = 0;
        sliderPosition = Vector2.zero;

        optionScales = new float[options.Length];
        hiding = false;

        //Load Save Data
        ReadInGhosts();
        ReadInDevGhosts();

        //Get Ghosts for this track
        foreach (GhostData ghostData in validTimeTrials.ToArray())
        {
            if (ghostData.cup != timeTrial.currentCup || ghostData.track != timeTrial.currentTrack || !gd.CompatibleVersion(ghostData.version))
                validTimeTrials.Remove(ghostData);
        }

        //Get Dev Ghosts
        foreach (GhostData gd in devTimeTrials.ToArray())
        {
            if (gd.cup != timeTrial.currentCup || gd.track != timeTrial.currentTrack)
                devTimeTrials.Remove(gd);
        }

        StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        hiding = true;
        StartCoroutine(FadeTo(0f));
    }

    private IEnumerator FadeTo(float finalVal)
    {
        float startTime = Time.time, startVal = guiAlpha;
        float travelTime = 0.5f;

        while(Time.time - startTime < travelTime)
        {
            guiAlpha = Mathf.Lerp(startVal, finalVal, (Time.time - startTime) / travelTime);
            yield return null;
        }

        guiAlpha = finalVal;
    }

    public void ReadInGhosts()
    {
        //Reset List
        validTimeTrials = new List<GhostData>();

        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Ghost Data/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Ghost Data/");

            //Check every file in Ghost Data folder
            var info = new DirectoryInfo(Application.persistentDataPath + "/Ghost Data/");
            var fileInfo = info.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                FileStream fileStream = null;
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    fileStream = file.Open(FileMode.Open);

                    validTimeTrials.Add((GhostData)bf.Deserialize(fileStream));
                    validTimeTrials[validTimeTrials.Count - 1].fileLocation = file.FullName;
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
        }
        catch { }
    }

    public void ReadInDevGhosts()
    {
        //Reset List
        devTimeTrials = new List<GhostData>();

        TextAsset[] assets = Resources.LoadAll<TextAsset>("Dev Ghosts/");

        try
        {
            //Check every file in Ghost Data folder
            foreach (TextAsset file in assets)
            {
                FileStream fileStream = null;
                try
                {
                    Stream s = new MemoryStream(file.bytes);
                    BinaryFormatter bf = new BinaryFormatter();

                    devTimeTrials.Add((GhostData)bf.Deserialize(s));
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
        }
        catch { }
    }

    void OnGUI()
    {

        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = Resources.Load<GUISkin>("GUISkins/TimeTrialSkin");

        if (guiAlpha > 0)
        {
            for(int i = 0; i < options.Length; i++)
            {
                Color textColor = new Color(1f, 1f, 1f, guiAlpha);

                if(!choosingGhost && selectedOption == i)
                {
                    textColor = Color.yellow;
                    textColor.a = guiAlpha;

                    optionScales[i] = Mathf.Clamp(optionScales[i] + (Time.deltaTime * 3f), 1f, 1.2f);
                }
                else
                    optionScales[i] = Mathf.Clamp(optionScales[i] - (Time.deltaTime * 3f), 1f, 1.2f);

                Rect labelRect = new Rect(100, 100 + (100 * i), 500, 90);
                GUIHelper.LeftRectLabel(labelRect, optionScales[i], options[i], textColor);

                if(!choosingGhost && Cursor.visible)
                {
                    if (labelRect.Contains(GUIHelper.GetMousePosition()))
                        selectedOption = i;

                    if (GUI.Button(labelRect, ""))
                    {
                        selectedOption = i;
                        DoSubmit();
                    }

                }
            }

            switch(menuState)
            {
                case TTMMenuState.ChoosingLocal:

                    if (validTimeTrials.Count > 0)
                    {
                        RenderList(validTimeTrials);

                        //Talk about deletion
                        if(choosingGhost && menuState == TTMMenuState.ChoosingLocal)
                        {
                            GUI.DrawTexture(new Rect(1150, 510, 100, 100), 
                                Resources.Load<Texture2D>("UI/Options/" + ((InputManager.controllers[0].inputType == InputType.Keyboard) ? "Space" : "X")), ScaleMode.ScaleToFit);

                            GUIHelper.LeftRectLabel(new Rect(1300, 510, 500, 100), deleteButtonScale, "Delete Record", deleteButtonScale == 1f ? Color.white : Color.yellow);

                            Rect buttonRect = new Rect(1150, 510, 600, 100);
                            if (GUI.Button(buttonRect, ""))
                            {
                                DeleteCurrentGhost();
                            }

                            if(buttonRect.Contains(GUIHelper.GetMousePosition()))
                                deleteButtonScale = Mathf.Clamp(deleteButtonScale + (Time.deltaTime * 2f), 1f, 1.2f);
                            else
                                deleteButtonScale = Mathf.Clamp(deleteButtonScale - (Time.deltaTime * 2f), 1f, 1.2f);
                        }
                    }
                    else
                    {
                        GUI.Label(new Rect(100, 500, 1720, 480), "There is no Ghost Data for this track.");
                    }
                    break;
                case TTMMenuState.ChoosingDev:
                    if (devTimeTrials.Count > 0)
                    {
                        RenderList(devTimeTrials);
                    }
                    else
                    {
                        GUI.Label(new Rect(100, 500, 1720, 480), "There are no Dev times for this track.");
                    }
                    
                    break;
            }
        }

        menuState = (TTMMenuState)selectedOption;
    }

    void RenderList(List<GhostData> data)
    {
        if (data != null && data.Count > 0)
        {
            if (timeTrialScales == null || timeTrialScales.Length != data.Count)
                timeTrialScales = new float[data.Count];

            Rect sliderArea = new Rect(100, 500, 1000, 420);
            sliderPosition = GUIHelper.BeginScrollView(sliderArea, sliderPosition, new Rect(0, 0, 950, data.Count * 100));

            for (int i = 0; i < data.Count; i++)
            {
                Color textColor = new Color(1f, 1f, 1f, guiAlpha);

                if (choosingGhost && selectedTT == i)
                {
                    textColor = Color.yellow;
                    textColor.a = guiAlpha;

                    timeTrialScales[i] = Mathf.Clamp(timeTrialScales[i] + (Time.deltaTime * 3f), 1f, 1.2f);
                }
                else
                    timeTrialScales[i] = Mathf.Clamp(timeTrialScales[i] - (Time.deltaTime * 3f), 1f, 1.2f);

                Rect labelRect = new Rect(10, 100 * i, 990, 90);
                GUIHelper.LeftRectLabel(labelRect, timeTrialScales[i], data[i].playerName + " - " + TimeManager.ToString(data[i].time), textColor);

                if (choosingGhost && Cursor.visible)
                {
                    Rect actualRect = new Rect(labelRect.x + sliderArea.x, +labelRect.y + sliderArea.y - sliderPosition.y, labelRect.width, labelRect.height);
                    if (actualRect.Contains(GUIHelper.GetMousePosition()))
                        selectedTT = i;

                    if (GUI.Button(labelRect, ""))
                    {
                        selectedTT = i;
                        DoSubmit();
                    }

                }
            }

            GUIHelper.EndScrollView();

            //Draw Info about selected Data
            GUIShape.RoundedRectangle(new Rect(1100, 100, 700, 300), 10, new Color(0.6f, 0.6f, 0.6f, guiAlpha * 0.5f));

            GUI.DrawTexture(new Rect(1110, 110, 140, 140), gd.characters[data[selectedTT].character].icon);
            GUI.DrawTexture(new Rect(1250, 110, 140, 140), gd.hats[data[selectedTT].hat].icon);

            GUI.DrawTexture(new Rect(1110, 250, 140, 140), gd.karts[data[selectedTT].kart].icon);
            GUI.DrawTexture(new Rect(1250, 250, 140, 140), gd.wheels[data[selectedTT].wheel].icon);

            GUI.Label(new Rect(1400, 110, 490, 50), data[selectedTT].playerName);
            GUI.Label(new Rect(1400, 170, 490, 50), TimeManager.ToString(data[selectedTT].time));
        }
    }

    void Update()
    {
        if(guiAlpha == 1f)
        {
            int vert = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");
            bool submitBool = (InputManager.controllers[0].GetButtonWithLock("Submit"));
            bool cancelBool = (InputManager.controllers[0].GetButtonWithLock("Cancel"));
            bool ToggleBool = (InputManager.controllers[0].GetButtonWithLock("Toggle"));

            if (vert != 0)
            {
                if (!choosingGhost)
                {
                    selectedOption = MathHelper.NumClamp(selectedOption + vert, 0, options.Length);
                    sliderPosition = new Vector2();
                    selectedTT = 0;

                    if (selectedOption == 0)
                        current = null;
                    if (selectedOption == 1)
                        current = devTimeTrials;
                    if (selectedOption == 2)
                        current = validTimeTrials;

                }
                else
                {
                    selectedTT = MathHelper.NumClamp(selectedTT + vert, 0, current.Count);
                }
            }

            if (submitBool)
                DoSubmit();

            if (cancelBool)
                DoCancel();

            if(ToggleBool)
            {
                //Delete the current Ghost
                DeleteCurrentGhost();
            }

            if (!choosingGhost && InputManager.controllers[0].inputType == InputType.Xbox360)
            {
                sliderPosition.y += InputManager.controllers[0].GetRawInput("ViewScroll") * Time.deltaTime * 300f;
            }

            if(choosingGhost && !Cursor.visible)
            {
                float diff = (selectedTT * 100f) - sliderPosition.y;
                if (Mathf.Abs(diff) > 1f)
                    sliderPosition.y += diff * Time.deltaTime * 3f;
            }

            if (choosingGhost && menuState == TTMMenuState.ChoosingLocal && validTimeTrials.Count == 0)
                choosingGhost = false;
        }
    }

    public void DoCancel()
    {
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/back"));

        if (choosingGhost)
        {
            choosingGhost = false;
        }
        else
        {
            Hide();
            FindObjectOfType<TimeTrial>().CancelTimeTrialMenu();
        }
    }

    void DoSubmit()
    {
        bool didSomething = false;

        if (!choosingGhost)
        {
            switch(options[selectedOption])
            {
                case "Race by Yourself":
                    Hide();
                    FindObjectOfType<TimeTrial>().FinishTimeTrialMenu();
                    didSomething = true;
                    break;
                case "Race Ghost":
                    if (validTimeTrials.Count > 0)
                    {
                        choosingGhost = true;
                        didSomething = true;
                    }
                    break;
                case "Race Developer Ghost":
                    if (devTimeTrials.Count > 0)
                    {
                        choosingGhost = true;
                        didSomething = true;
                    }
                    break;
            }
        }
        else
        {
            if(menuState == TTMMenuState.ChoosingLocal || menuState == TTMMenuState.ChoosingDev)
            {
                if(menuState == TTMMenuState.ChoosingLocal)
                    timeTrial.ghost = validTimeTrials[selectedTT];
                else
                    timeTrial.ghost = devTimeTrials[selectedTT];

                Hide();
                FindObjectOfType<TimeTrial>().FinishTimeTrialMenu();
            }
        }

        if (didSomething)
            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));
    }

    void DeleteCurrentGhost()
    {
        GhostData kill = validTimeTrials[selectedTT];
        File.Delete(kill.fileLocation);

        validTimeTrials.Remove(kill);
    }
}
