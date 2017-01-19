using UnityEngine;
using System.Collections;

public class Options : MonoBehaviour
{
    CurrentGameData gd;

    private float guiAlpha = 0;
    private const float fadeTime = 0.5f;

    private Texture2D blueTab, greenTab, orangeTab, gameTitle, graphicsTitle, inputTitle, button, line, lbTexture, rbTexture, qTexture, eTexture, xTexture, yTexture, aTexture, rsTexture;

    enum OptionsTab { Game, Graphics, Input };
    private OptionsTab currentTab = OptionsTab.Game;

    private DropDown resDropDown, qualityDropDown;
    private Toggle fullscreenToggle;
    private int currentResolution = 0, currentQuality = 0;

    private float cancelScale = 1f, applyScale = 1f, plusScale = 2f, minusScale = 2f, editScale;
    private bool fullScreen, somethingChanged = false, editName = false;

    public int currentLayoutSelection = 0, selectedInput = -1;
    private Vector2 layoutScrollPosition, configScrollPosition;
    private float[] inputScales, commandSizes;
    private bool lockInputs = false, noInput = false;
    [SerializeField]
    private string[] availableChanges, availableNames;

    //Keyboard / Controller Controls
    private Vector2 lastMouse;
    private bool mouseLastUsed;
    private int currentSelection = 0;

    //Used for Volume Options
    private bool changingSlider = false, currentX = false;
    private float sliderChangeRate = 50f;
    private int lastInput, currentY;

    // Use this for initialization
    void Start()
    {
        gd = FindObjectOfType<CurrentGameData>();

        //Load the textures
        blueTab = Resources.Load<Texture2D>("UI/Options/BlueTab");
        greenTab = Resources.Load<Texture2D>("UI/Options/GreenTab");
        orangeTab = Resources.Load<Texture2D>("UI/Options/OrangeTab");

        gameTitle = Resources.Load<Texture2D>("UI/Options/Game");
        graphicsTitle = Resources.Load<Texture2D>("UI/Options/Graphics");
        inputTitle = Resources.Load<Texture2D>("UI/Options/Input");

        resDropDown = new DropDown();
        qualityDropDown = new DropDown();

        fullscreenToggle = new Toggle();

        //Load Textures
        button = Resources.Load<Texture2D>("UI/Options/Button");
        line = Resources.Load<Texture2D>("UI/Lobby/Line");

        lbTexture = Resources.Load<Texture2D>("UI/Options/LB");
        rbTexture = Resources.Load<Texture2D>("UI/Options/RB");
        qTexture = Resources.Load<Texture2D>("UI/Options/Q");
        eTexture = Resources.Load<Texture2D>("UI/Options/E");
        xTexture = Resources.Load<Texture2D>("UI/Options/X");
        yTexture = Resources.Load<Texture2D>("UI/Options/Y");
        aTexture = Resources.Load<Texture2D>("UI/Options/A");
        rsTexture = Resources.Load<Texture2D>("UI/Options/RS");

        availableChanges = new string[] { "Throttle", "Brake", "SteerLeft", "SteerRight", "Drift", "Item", "RearView" };
        availableNames = new string[] { "Throttle", "Brake / Reverse", "Steer Left", "Steer Right", "Drift", "Item", "Look Behind" };

        lastMouse = GUIHelper.GetMousePosition();
    }

    public void ShowOptions()
    {
        ResetEverything();
        StartCoroutine(ActualShowOptions());

        //Clear Values
        currentLayoutSelection = 0;
        selectedInput = -1;
        layoutScrollPosition = new Vector2();
        configScrollPosition = new Vector2();
        lockInputs = false;

        ResetControllerOptions();
    }

    public IEnumerator ActualShowOptions()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeGui(0f, 1f));
    }

    public void HideOptions()
    {
        StartCoroutine(ActualHideOptions());

        //Clear Values
        selectedInput = -1;
        lockInputs = false;
        InputManager.LoadConfig();
    }

    public IEnumerator ActualHideOptions()
    {
        yield return StartCoroutine(FadeGui(1f, 0f));
        enabled = false;
    }

    private IEnumerator FadeGui(float start, float end)
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < fadeTime)
        {
            guiAlpha = Mathf.Lerp(start, end, (Time.realtimeSinceStartup - startTime) / fadeTime);
            yield return null;
        }

        guiAlpha = end;
    }

    void Update()
    {      
        //Handle Input
        if (!lockInputs && InputManager.controllers != null && InputManager.controllers.Count > 0 && guiAlpha == 1)
        {
            int vertical = InputManager.controllers[0].GetRawMenuInput("MenuVertical");
            float horizontal = 0;

            if ((currentTab == OptionsTab.Graphics && currentSelection == 3) || (currentTab == OptionsTab.Input && changingSlider) || (currentTab == OptionsTab.Game))
                horizontal = InputManager.controllers[0].GetRawMenuInput("MenuHorizontal");
            else
                horizontal = InputManager.controllers[0].GetRawInput("MenuHorizontal");

            int tabChange = InputManager.controllers[0].GetRawMenuInput("TabChange");
            bool submitBool = (InputManager.controllers[0].GetRawMenuInput("Submit") != 0);
            bool cancelBool = (InputManager.controllers[0].GetRawMenuInput("Cancel") != 0);

            if (mouseLastUsed && (horizontal != 0 || vertical != 0 || tabChange != 0 || submitBool || cancelBool))
            {
                mouseLastUsed = false;
                ResetControllerOptions();
            }

            if (tabChange != 0)
            {
                currentTab += tabChange;
                currentTab = (OptionsTab)MathHelper.NumClamp((int)currentTab, 0, 3);
                ResetControllerOptions();
            }

            if (!mouseLastUsed)
            {
                switch (currentTab)
                {
                    case OptionsTab.Game:

                        if (submitBool)
                            changingSlider = !changingSlider;

                        if (!changingSlider)
                        {
                            if (vertical != 0)
                                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, 3);
                            if (cancelBool)
                                FindObjectOfType<MainMenu>().BackMenu();
                        }
                        else
                        {
                            if (horizontal != 0)
                            {
                                float toChange = horizontal * Time.deltaTime * sliderChangeRate;
                                if (currentSelection == 0)
                                    SoundManager.masterVolume = Mathf.Clamp(SoundManager.masterVolume + toChange, 0, 100);
                                else if (currentSelection == 1)
                                    SoundManager.musicVolume = Mathf.Clamp(SoundManager.musicVolume + toChange, 0, 100);
                                else if (currentSelection == 2)
                                    SoundManager.sfxVolume = Mathf.Clamp(SoundManager.sfxVolume + toChange, 0, 100);
                            }
                            if (cancelBool)
                                changingSlider = false;
                        }
                        break;
                    case OptionsTab.Graphics:

                        if (submitBool)
                        {
                            if (currentSelection == 0)
                            {
                                lastInput = currentResolution;
                                resDropDown.toggled = !resDropDown.toggled;
                                changingSlider = !changingSlider;
                            }
                            else if (currentSelection == 1)
                                fullScreen = !fullScreen;
                            else if (currentSelection == 2)
                            {
                                lastInput = currentQuality;
                                qualityDropDown.toggled = !qualityDropDown.toggled;
                                changingSlider = !changingSlider;
                            }
                            else if (currentSelection == 3) //Do Apply and Cancel Stuff
                            {
                                if (changingSlider)
                                {
                                    somethingChanged = false;
                                    SaveEverything();
                                }
                                else
                                {
                                    ResetEverything();
                                }
                            }
                        }

                        if (currentSelection == 3 && horizontal != 0f)
                            changingSlider = !changingSlider;

                        if ((currentSelection == 3 || !changingSlider) && vertical != 0)
                        {
                            if (currentSelection == 3)
                                changingSlider = false;

                            currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, 4);
                        }


                        if (!changingSlider)
                        {
                            if (cancelBool)
                                FindObjectOfType<MainMenu>().BackMenu();

                            if (resDropDown.toggled)
                                resDropDown.toggled = false;
                            else if (qualityDropDown.toggled)
                                qualityDropDown.toggled = false;
                        }
                        else
                        {
                            if (cancelBool)
                            {
                                if (currentSelection == 3)
                                    FindObjectOfType<MainMenu>().BackMenu();
                                else
                                {
                                    changingSlider = false;

                                    if (resDropDown.toggled)
                                        currentResolution = lastInput;
                                    else if (qualityDropDown.toggled)
                                        currentQuality = lastInput;
                                }

                            }

                            if (resDropDown.toggled)
                                resDropDown.SetScroll(currentResolution);
                            else if (qualityDropDown.toggled)
                                qualityDropDown.SetScroll(currentQuality);

                            if (vertical != 0)
                                if (resDropDown.toggled)
                                    currentResolution += vertical;
                                else if (qualityDropDown.toggled)
                                    currentQuality += vertical;
                        }
                        break;
                    case OptionsTab.Input:

                        int configCount = InputManager.AllConfigs.Count;

                        if (cancelBool && (!changingSlider || currentSelection >= configCount) && !editName)
                            FindObjectOfType<MainMenu>().BackMenu();

                        if (!changingSlider)
                        {
                            if (!editName && vertical != 0)
                                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, Mathf.Min(configCount + 1, 12));

                            if (currentSelection < configCount)
                            {
                                currentLayoutSelection = currentSelection;

                                if (currentSelection > 1 && InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
                                {
                                    if (InputManager.controllers[0].GetRawMenuInput("Edit") != 0)
                                        EditPressed();
                                    else if (InputManager.controllers[0].GetRawMenuInput("Minus") != 0)
                                        MinusPressed();
                                }
                            }

                            if (InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
                            {
                                configScrollPosition.y += InputManager.controllers[0].GetRawInput("ViewScroll") * Time.deltaTime * 300f;
                            }

                            if (submitBool && currentSelection > 1)
                            {
                                if (currentSelection < configCount)
                                {
                                    if (!editName)
                                    {
                                        changingSlider = !changingSlider;
                                        currentX = false;
                                        currentY = 0;
                                    }
                                    else
                                    {
                                        ChangeType(currentLayoutSelection);
                                    }
                                }
                                else// + Button has been pressed
                                    PlusPressed();
                            }

                            float diff = (currentLayoutSelection * 70f) - layoutScrollPosition.y;
                            if (Mathf.Abs(diff) > 1f)
                                layoutScrollPosition.y += Mathf.Sign(diff) * Time.deltaTime * 300f;
                        }
                        else
                        {
                            if (submitBool)
                            {
                                if (currentSelection < configCount)
                                {
                                    if (!currentX)
                                        selectedInput = (currentY * 2);
                                    else
                                        selectedInput = (currentY * 2) + 1;
                                }
                            }

                            if (horizontal != 0)
                                currentX = !currentX;

                            if (vertical != 0)
                                currentY = MathHelper.NumClamp(currentY + vertical, 0, availableChanges.Length);

                            //Scroll Scrollview with input
                            float diff = (currentY * 120) - configScrollPosition.y;
                            if (Mathf.Abs(diff) > 1f)
                                configScrollPosition.y += Mathf.Sign(diff) * Time.deltaTime * 200f;

                            if (cancelBool)
                                changingSlider = false;
                        }
                        break;
                }
            }
        }
    }

    //Called when variables used for Controller inputs need to be reset
    private void ResetControllerOptions()
    {
        //Reset Controller Values
        if (currentTab == OptionsTab.Input)
            currentSelection = currentLayoutSelection;
        else
            currentSelection = 0;

        changingSlider = false;
        currentX = false;
        currentY = 0;
        selectedInput = -1;
    }

    private void PlusPressed()
    {
        string toAdd = "Keyboard";

        //Add an Xbox Layout if that controller pressed the plus
        if (InputManager.controllers.Count > 0 && InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
            toAdd = "Xbox360";

        InputManager.AllConfigs.Add(new InputLayout("Layout " + (InputManager.AllConfigs.Count - 1).ToString() + "," + toAdd));
        InputManager.SaveConfig();
        selectedInput = -1;
        currentLayoutSelection = InputManager.AllConfigs.Count - 1;
        editName = false;
    }

    private void MinusPressed()
    {
        InputManager.AllConfigs.RemoveAt(currentLayoutSelection);
        InputManager.SaveConfig();
        currentLayoutSelection -= 1;

        if (!mouseLastUsed)
            currentSelection -= 1;

        selectedInput = -1;
        editName = false;
    }

    private void EditPressed()
    {
        if (editName)
            InputManager.SaveConfig();

        editName = !editName;
        selectedInput = -1;
    }

    void OnGUI()
    {
        if (guiAlpha > 0f && GUIHelper.DrawBack(1f))
        {
            FindObjectOfType<MainMenu>().BackMenu();
        }

        Vector2 mousePosition = GUIHelper.GetMousePosition();

        //Do Mouse Input Checks
        if (!mouseLastUsed && (lastMouse != mousePosition || Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            mouseLastUsed = true;
            ResetControllerOptions();
        }

        lastMouse = mousePosition;

        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Options");

        //Draw all the titles
        if (currentTab != OptionsTab.Game)
        {
            Rect gameRect = new Rect(180, 95, 330, 70);
            GUI.DrawTexture(gameRect, gameTitle);

            if (GUI.Button(gameRect, ""))
                currentTab = OptionsTab.Game;
        }

        if (currentTab != OptionsTab.Graphics)
        {
            Rect graphicsRect = new Rect(510, 95, 330, 70);
            GUI.DrawTexture(graphicsRect, graphicsTitle);

            if (GUI.Button(graphicsRect, ""))
                currentTab = OptionsTab.Graphics;
        }

        if (currentTab != OptionsTab.Input)
        {
            Rect inputRect = new Rect(840, 95, 330, 70);
            GUI.DrawTexture(inputRect, inputTitle);

            if (GUI.Button(inputRect, ""))
                currentTab = OptionsTab.Input;
        }

        //Draw Q/E or LB/RB
        bool drawKeyboard = true;
        Rect leftIcon = new Rect(70, 107, 100, 50);
        Rect rightIcon = new Rect(1750, 107, 100, 50);

        if (InputManager.controllers != null && InputManager.controllers.Count > 0 && InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
            drawKeyboard = false;

        GUI.DrawTexture(leftIcon, drawKeyboard ? qTexture : lbTexture);
        GUI.DrawTexture(rightIcon, drawKeyboard ? eTexture : rbTexture);

        //Draw the current tab
        Rect tabRect = new Rect(180, 90, 1550, 870);
        Rect tabAreaRect = new Rect(180, 170, 1550, 800);
        //Required GUIStyles
        GUIStyle normalLabel = GUI.skin.label;
        GUIStyle selectedLabel = new GUIStyle(GUI.skin.label);
        selectedLabel.normal.textColor = Color.yellow;

        switch (currentTab)
        {
            case OptionsTab.Game:
                GUI.DrawTexture(tabRect, blueTab);
                GUIShape.RoundedRectangle(new Rect(210, 200, 1485, 400), 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                GUIHelper.BeginGroup(tabAreaRect);

                GUIStyle normalSlider = GUI.skin.horizontalSlider;
                GUIStyle normalSliderThumb = GUI.skin.horizontalSliderThumb;
                GUIStyle selectedSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                selectedSliderThumb.normal.background = selectedSliderThumb.active.background;

                GUI.Label(new Rect(20, 70, 300, 100), "Master Volume:", (currentSelection == 0 && !changingSlider && !mouseLastUsed) ? selectedLabel : normalLabel);
                SoundManager.masterVolume = GUI.HorizontalSlider(new Rect(330, 110, 1000, 100), SoundManager.masterVolume, 0f, 100f, normalSlider, (currentSelection == 0 && changingSlider && !mouseLastUsed) ? selectedSliderThumb : normalSliderThumb);
                GUI.Label(new Rect(1300, 70, 250, 100), (int)SoundManager.masterVolume + "%");

                GUI.Label(new Rect(20, 180, 300, 100), "Music Volume:", (currentSelection == 1 && !changingSlider && !mouseLastUsed) ? selectedLabel : normalLabel);
                SoundManager.musicVolume = GUI.HorizontalSlider(new Rect(330, 220, 1000, 100), SoundManager.musicVolume, 0f, 100f, normalSlider, (currentSelection == 1 && changingSlider && !mouseLastUsed) ? selectedSliderThumb : normalSliderThumb);
                GUI.Label(new Rect(1300, 180, 250, 100), (int)SoundManager.musicVolume + "%");

                GUI.Label(new Rect(20, 290, 300, 100), "SFX Volume:", (currentSelection == 2 && !changingSlider && !mouseLastUsed) ? selectedLabel : normalLabel);
                SoundManager.sfxVolume = GUI.HorizontalSlider(new Rect(330, 330, 1000, 100), SoundManager.sfxVolume, 0f, 100f, normalSlider, (currentSelection == 2 && changingSlider && !mouseLastUsed) ? selectedSliderThumb : normalSliderThumb);
                GUI.Label(new Rect(1300, 290, 250, 100), (int)SoundManager.sfxVolume + "%");

                break;
            case OptionsTab.Graphics:
                GUI.DrawTexture(tabRect, greenTab);

                GUIShape.RoundedRectangle(new Rect(210, 820, 400, 100), 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                GUIHelper.BeginGroup(tabAreaRect);

                //Fullscreen
                GUI.Label(new Rect(20, 145, 300, 100), "Fullscreen:", (currentSelection == 1 && !mouseLastUsed) ? selectedLabel : normalLabel);
                bool newFullscreen = fullscreenToggle.Draw(new Rect(20, 170, 350, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, fullScreen, "");
                if (!resDropDown.toggled)
                    fullScreen = newFullscreen;

                //Change Quality
                GUI.Label(new Rect(20, 250, 300, 100), "Graphics Quality:", (currentSelection == 2 && !changingSlider && !mouseLastUsed) ? selectedLabel : normalLabel);

                int newQuality = qualityDropDown.Draw(new Rect(330, 250, 1000, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, currentQuality, QualitySettings.names);
                if (newQuality != currentQuality)
                    somethingChanged = true;
                currentQuality = newQuality;

                //Change Resolution
                GUI.Label(new Rect(20, 70, 300, 100), "Resolution:", (currentSelection == 0 && !changingSlider && !mouseLastUsed) ? selectedLabel : normalLabel);

                string[] possibleScreens = new string[Screen.resolutions.Length];
                for (int i = 0; i < possibleScreens.Length; i++)
                    possibleScreens[i] = Screen.resolutions[i].width + " x " + Screen.resolutions[i].height;

                int newRes = resDropDown.Draw(new Rect(330, 90, 1000, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, currentResolution, possibleScreens);
                if (newRes != currentResolution)
                    somethingChanged = true;
                currentResolution = newRes;

                //Apply and Cancel Buttons
                Rect cancelRect = GUIHelper.CentreRectLabel(new Rect(50, 650, 150, 100), cancelScale, "Cancel", (cancelScale > 1.1f || (currentSelection == 3 && !changingSlider)) ? Color.yellow : Color.white);
                cancelScale = GUIHelper.SizeHover(cancelRect, cancelScale, 1f, 1.3f, 3f);

                if (GUI.Button(cancelRect, ""))
                    ResetEverything();

                Rect applyRect = GUIHelper.CentreRectLabel(new Rect(270, 650, 150, 100), applyScale, "Apply", (applyScale > 1.1f || (currentSelection == 3 && changingSlider)) ? Color.yellow : Color.white);
                applyScale = GUIHelper.SizeHover(applyRect, applyScale, 1f, 1.3f, 3f);

                if (GUI.Button(applyRect, ""))
                {
                    somethingChanged = false;
                    SaveEverything();
                }

                break;
            case OptionsTab.Input:
                GUI.DrawTexture(tabRect, orangeTab);

                bool showXbox = !mouseLastUsed && InputManager.controllers.Count > 0 && InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360 && !changingSlider;

                if (showXbox)
                    GUI.DrawTexture(new Rect(1725, 250 + (configScrollPosition.y * 3.4f), 50, 50), rsTexture);

                GUIHelper.BeginGroup(tabAreaRect);

                if (commandSizes == null || commandSizes.Length != availableChanges.Length * 2)
                {
                    commandSizes = new float[availableChanges.Length * 2];
                    for (int i = 0; i < commandSizes.Length; i++)
                        commandSizes[i] = 1f;
                }

                GUI.Label(new Rect(0, 20, 450, 50), "Available Layouts");
                GUI.DrawTexture(new Rect(450, 20, 5, tabAreaRect.height - 60), line);

                Rect scrollviewRect = new Rect(30, 70, 400, tabAreaRect.height - 120);
                GUIShape.RoundedRectangle(scrollviewRect, 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                scrollviewRect.y += 10;
                scrollviewRect.width += 12;
                scrollviewRect.height -= 110;

                //Draw + Gui
                if (InputManager.AllConfigs.Count < 12)
                {
                    Rect addLabel = new Rect(50, 700, 30, 50);
                    GUIHelper.CentreRectLabel(addLabel, plusScale, "+", (!mouseLastUsed && currentSelection == InputManager.AllConfigs.Count && !changingSlider) ? Color.yellow : Color.white);

                    if (GUI.Button(addLabel, ""))
                    {
                        //Add New Layout
                        PlusPressed();
                    }

                    plusScale = GUIHelper.SizeHover(addLabel, plusScale, 2f, 2.5f, 2f);
                }

                //Draw - Gui
                if (currentLayoutSelection >= 2 && (mouseLastUsed || (currentSelection < InputManager.AllConfigs.Count && !changingSlider)))
                {
                    Rect minusRect = new Rect(140, 700, 30, 50);
                    GUIHelper.CentreRectLabel(minusRect, minusScale, "-", Color.white);

                    if (GUI.Button(minusRect, ""))
                    {
                        MinusPressed();
                    }
                    minusScale = GUIHelper.SizeHover(minusRect, minusScale, 2f, 2.5f, 2f);

                    if (showXbox && currentSelection < InputManager.AllConfigs.Count)
                        GUI.DrawTexture(new Rect(130, 650, 50, 50), yTexture);
                }

                //Draw Edit Gui
                if (currentLayoutSelection >= 2 && (mouseLastUsed || (currentSelection < InputManager.AllConfigs.Count && !changingSlider)))
                {
                    Rect editRect = new Rect(210, 675, 100, 100);
                    GUIHelper.CentreRectLabel(editRect, editScale, !editName ? "edit" : "save", Color.white);

                    if (GUI.Button(editRect, ""))
                    {
                        EditPressed();
                    }

                    editScale = GUIHelper.SizeHover(editRect, editScale, 1f, 1.25f, 2f);

                    if (showXbox && currentSelection < InputManager.AllConfigs.Count)
                        GUI.DrawTexture(new Rect(235, 650, 50, 50), xTexture);
                }

                if (currentLayoutSelection >= 2 && showXbox && currentSelection < InputManager.AllConfigs.Count)
                {
                    GUI.Label(new Rect(320, 675, 100, 100), !editName ? "bind" : "type");
                    GUI.DrawTexture(new Rect(345, 650, 50, 50), aTexture);
                }
                    



                if (inputScales == null || inputScales.Length != InputManager.AllConfigs.Count)
                {
                    inputScales = new float[InputManager.AllConfigs.Count];
                    for (int i = 0; i < inputScales.Length; i++)
                        inputScales[i] = 1f;
                }

                layoutScrollPosition = GUIHelper.BeginScrollView(scrollviewRect, layoutScrollPosition, new Rect(10, 10, 380, InputManager.AllConfigs.Count * 70));

                for (int i = 0; i < InputManager.AllConfigs.Count; i++)
                {
                    //Draw Label
                    Rect labelRect;
                    if (currentLayoutSelection != i || !editName)
                    {
                        labelRect = GUIHelper.CentreRectLabel(new Rect(10, 40 + (70 * i), 380, 50), inputScales[i], InputManager.AllConfigs[i].Name, ((mouseLastUsed && currentLayoutSelection == i) || (!mouseLastUsed && currentSelection == i && !changingSlider)) ? Color.yellow : Color.white);
                    }
                    else
                    {
                        labelRect = new Rect(30, 40 + (70 * i), 300, 50);
                        InputManager.AllConfigs[i].Name = GUI.TextField(labelRect, InputManager.AllConfigs[i].Name);
                    }

                    Rect labelClickRect = new Rect(labelRect);
                    if (GUI.Button(labelClickRect, ""))
                    {
                        currentLayoutSelection = i;
                        if (editName)
                        {
                            editName = false;
                            InputManager.LoadConfig();
                        }
                    }
                    inputScales[i] = GUIHelper.SizeHover(labelClickRect, inputScales[i], 1f, 1.25f, 2f);

                    //Draw Icon
                    Rect iconRect = GUIHelper.CentreRect(new Rect(330, 40 + (70 * i), 50, 50), inputScales[i]);
                    GUI.DrawTexture(iconRect, Resources.Load<Texture2D>("UI/Controls/" + ((InputManager.AllConfigs[i].Type == ControllerType.Keyboard) ? "Keyboard" : "Xbox_1")));

                    if (editName && currentLayoutSelection == i && GUI.Button(iconRect, ""))
                    {
                        ChangeType(i);
                    }

                }
                GUIHelper.EndScrollView();

                //Draw input Configuration
                InputLayout current = InputManager.AllConfigs[currentLayoutSelection];

                GUI.Label(new Rect(500, 20, 333, 50), "Action");
                GUI.Label(new Rect(833, 20, 333, 50), (current.Type == ControllerType.Keyboard) ? "Key +" : "Button +");
                GUI.Label(new Rect(1166, 20, 333, 50), (current.Type == ControllerType.Keyboard) ? "Key -" : "Button -");

                Rect configScrollView = new Rect(500, 70, 1000, tabAreaRect.height - 120);
                GUIShape.RoundedRectangle(configScrollView, 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                configScrollView.width += 25;
                configScrollPosition = GUIHelper.BeginScrollView(configScrollView, configScrollPosition, new Rect(0, 0, 980, 20 + (availableChanges.Length * 120)));

                for (int i = 0; i < availableChanges.Length; i++)
                {
                    GUI.Label(new Rect(30, 20 + (i * 120), 303, 100), availableNames[i]);

                    //Change string to actual input name

                    //Draw Plus Command
                    Rect labelRect = new Rect(333, 20 + (i * 120), 333, 100);
                    string labelText = "-";

                    if (selectedInput == (i * 2))
                    {
                        if (current.Type == ControllerType.Keyboard)
                            labelText = "Press any Key";
                        else
                            labelText = "Press any Button";

                        commandSizes[(i * 2)] = 0.7f;

                        GUI.DrawTexture(new Rect(333, 35 + (i * 120), 333, 65), Resources.Load<Texture2D>("UI/Options/Button"), ScaleMode.ScaleToFit);
                    }
                    else if (current.commandsOne.ContainsKey(availableChanges[i]))
                    {
                        labelText = current.commandsOne[availableChanges[i]];
                    }

                    GUIHelper.CentreRectLabel(labelRect, commandSizes[(i * 2)], labelText, (!mouseLastUsed && changingSlider && !currentX && currentY == i) ? Color.yellow : Color.white);

                    if (currentLayoutSelection > 1 && GUI.Button(labelRect, ""))
                    {
                        if (selectedInput != i * 2)
                            selectedInput = (i * 2);
                        else
                        {
                            RemoveInput(selectedInput);
                            selectedInput = -1;
                        }

                    }

                    commandSizes[(i * 2)] = GUIHelper.SizeHover(labelRect, commandSizes[(i * 2)], 1f, 1.3f, 2f);

                    //Draw Minus Command
                    labelRect = new Rect(666, 20 + (i * 120), 333, 100);
                    labelText = "-";

                    if (selectedInput == (i * 2) + 1)
                    {
                        if (current.Type == ControllerType.Keyboard)
                            labelText = "Press any Key";
                        else
                            labelText = "Press any Button";

                        commandSizes[(i * 2) + 1] = 0.7f;

                        GUI.DrawTexture(new Rect(666, 35 + (i * 120), 333, 65), Resources.Load<Texture2D>("UI/Options/Button"), ScaleMode.ScaleToFit);
                    }
                    else if (current.commandsTwo.ContainsKey(availableChanges[i]))
                    {
                        labelText = current.commandsTwo[availableChanges[i]];
                    }

                    GUIHelper.CentreRectLabel(labelRect, commandSizes[(i * 2) + 1], labelText, (!mouseLastUsed && changingSlider && currentX && currentY == i) ? Color.yellow : Color.white);

                    if (currentLayoutSelection > 1 && GUI.Button(labelRect, ""))
                    {
                        if (selectedInput != (i * 2) + 1)
                            selectedInput = (i * 2) + 1;
                        else
                        {
                            RemoveInput(selectedInput);
                            selectedInput = -1;
                        }
                    }

                    commandSizes[(i * 2) + 1] = GUIHelper.SizeHover(labelRect, commandSizes[(i * 2) + 1], 1f, 1.3f, 2f);

                    if (i < availableChanges.Length - 1)
                        GUI.DrawTexture(new Rect(50, 120 + (i * 120), 900, 5), line);
                }

                GUIHelper.EndScrollView();

                //Do Input Changes
                string xboxInput = InputManager.GetXboxInput();

                if (selectedInput != -1 && noInput && lockInputs && current.Type == ControllerType.Keyboard && xboxInput != "")
                {
                    RemoveInput(selectedInput);
                    selectedInput = -1;
                }

                if (selectedInput != -1 && noInput && lockInputs && current.Type == ControllerType.Xbox360 && InputManager.controllers.Count > 0 && InputManager.controllers[0].controlLayout.Type == ControllerType.Keyboard && Input.GetKeyDown(KeyCode.Escape))
                    {
                        RemoveInput(selectedInput);
                        selectedInput = -1;
                    }

                if (selectedInput != -1)
                {
                    if (!noInput)
                    {
                        if (!Input.anyKey && xboxInput == "")
                            noInput = true;
                    }
                    else
                    {
                        lockInputs = true;
                        InputManager.lockEverything = true;

                        if (current.Type == ControllerType.Keyboard)
                        {
                            string newInput = Input.inputString;

                            if (newInput == "")//Check for Arrow keys, ctrls, alts and shifts
                            {
                                if (Input.GetKey(KeyCode.LeftArrow))
                                    newInput = "leftarrow";
                                else if (Input.GetKey(KeyCode.RightArrow))
                                    newInput = "rightarrow";
                                else if (Input.GetKey(KeyCode.UpArrow))
                                    newInput = "uparrow";
                                else if (Input.GetKey(KeyCode.DownArrow))
                                    newInput = "downarrow";
                                else if (Input.GetKey(KeyCode.LeftControl))
                                    newInput = "leftcontrol";
                                else if (Input.GetKey(KeyCode.RightControl))
                                    newInput = "rightcontrol";
                                else if (Input.GetKey(KeyCode.LeftShift))
                                    newInput = "leftshift";
                                else if (Input.GetKey(KeyCode.RightShift))
                                    newInput = "rightshift";
                                else if (Input.GetKey(KeyCode.LeftAlt))
                                    newInput = "leftalt";
                                else if (Input.GetKey(KeyCode.RightAlt))
                                    newInput = "rightalt";
                            }
                            else
                            {
                                if (newInput == "\r")
                                    newInput = "return";
                                else if (newInput == "\b")
                                    newInput = "backspace";
                                else if (newInput == " ")
                                    newInput = "space";
                                else if (newInput.Length > 0)
                                    newInput = newInput[0].ToString();
                            }

                            if (newInput != "")//Input pressed
                            {
                                string inputToChange = availableChanges[selectedInput / 2];
                                //Set command to new input
                                if (selectedInput % 2 == 0)
                                {
                                    //Check that this input isn't used for minus
                                    if (!current.commandsTwo.ContainsKey(inputToChange) || current.commandsTwo[inputToChange] != newInput)
                                        current.commandsOne[inputToChange] = newInput;
                                }
                                else
                                {
                                    //Check that this input isn't used for plus
                                    if (!current.commandsOne.ContainsKey(inputToChange) || current.commandsOne[inputToChange] != newInput)
                                        current.commandsTwo[inputToChange] = newInput;
                                }

                                InputManager.SaveConfig();
                                selectedInput = -1;
                            }
                        }
                        else
                        {
                            string newInput = xboxInput;
                            if (newInput != "")
                            {
                                string inputToChange = availableChanges[selectedInput / 2];
                                //Set command to new input
                                if (selectedInput % 2 == 0)
                                {
                                    //Check that this input isn't used for minus
                                    if (!current.commandsTwo.ContainsKey(inputToChange) || current.commandsTwo[inputToChange] != newInput)
                                        current.commandsOne[inputToChange] = newInput;
                                }
                                else
                                {
                                    //Check that this input isn't used for plus
                                    if (!current.commandsOne.ContainsKey(inputToChange) || current.commandsOne[inputToChange] != newInput)
                                        current.commandsTwo[inputToChange] = newInput;
                                }

                                InputManager.SaveConfig();
                                selectedInput = -1;
                            }
                        }
                    }
                }
                else
                {
                    noInput = false;

                    if (lockInputs && !Input.anyKey && xboxInput == "")
                    {
                        lockInputs = false;
                        InputManager.lockEverything = false;
                    }
                }

                break;
        }

        GUIHelper.EndGroup();
    }

    private void RemoveInput(int toRemove)
    {
        string inputToChange = availableChanges[selectedInput / 2];
        InputLayout current = InputManager.AllConfigs[currentLayoutSelection];

        if (selectedInput % 2 == 0)
        {
            current.commandsOne.Remove(inputToChange);
        }
        else
        {
            current.commandsTwo.Remove(inputToChange);
        }

        InputManager.SaveConfig();
    }

    private void ResetEverything()
    {
        //Get current resolution
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height)
            {
                currentResolution = i;
                break;
            }
        }

        fullScreen = Screen.fullScreen;
        currentQuality = QualitySettings.GetQualityLevel();

    }

    private void SaveEverything()
    {
        Screen.SetResolution(Screen.resolutions[currentResolution].width, Screen.resolutions[currentResolution].height, fullScreen);
        QualitySettings.SetQualityLevel(currentQuality);
    }

    private void ChangeType(int i)
    {
        InputManager.AllConfigs[i].Type++;
        if ((int)InputManager.AllConfigs[i].Type >= 2)
            InputManager.AllConfigs[i].Type = 0;

        InputManager.AllConfigs[i].CancelLoad();
        InputManager.SaveConfig();
    }
}
