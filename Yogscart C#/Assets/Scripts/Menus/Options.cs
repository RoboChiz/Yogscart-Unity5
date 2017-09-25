using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Options : MonoBehaviour
{
    private float guiAlpha = 0;
    private const float fadeTime = 0.5f;

    private Texture2D blueTab, greenTab, orangeTab, purpleTab, gameTitle, graphicsTitle, inputTitle, bindingTitle, line, lbTexture, rbTexture, qTexture, eTexture, xTexture, yTexture, aTexture, rsTexture;

    enum OptionsTab { Game, Graphics, Input, Binding};
    private OptionsTab currentTab = OptionsTab.Game;

    private DropDown resDropDown, qualityDropDown, aaDropDown;
    private Toggle fullscreenToggle, streamModeToggle, ambientOcculusionToggle, chromaticAberrationToggle;
    
    private float cancelScale = 1f, applyScale = 1f, plusScale = 2f, minusScale = 2f, editScale;
    private bool editName = false;
    public bool somethingChanged = false;

    public int currentLayoutSelection = 0, selectedInput = -1;
    private Vector2 layoutScrollPosition, configScrollPosition;
    private float[] inputScales, commandSizes;
    private bool lockInputs = false, noInput = false;
    [HideInInspector]
    public string[] availableChanges, availableNames;

    public bool locked = false;

    //Graphics Settings
    private bool fullScreen, lastFullscreen, streamMode;
    private int currentResolution = 0, currentQuality = 0, lastResolution, lastQuality;

    //Keyboard / Controller Controls
    private int currentSelection = 0;

    //Used for Volume Options
    private bool changingSlider = false, currentX = false;
    private float sliderChangeRate = 2f;
    private int lastInput, currentY;

    //Save Graphics Lists for Effecientcy
    private string[] possibleScreens, qualityNames, antiAliasingNames;
    private bool chromaticAberration, ambientOcculusion;
    private EffectsManager.AntiAliasing antiAliasing;

    //Binding Menu
    private List<ControlLayout> allLayouts;

    // Use this for initialization
    void Start()
    {
        //Load the textures
        blueTab = Resources.Load<Texture2D>("UI/Options/BlueTab");
        greenTab = Resources.Load<Texture2D>("UI/Options/GreenTab");
        orangeTab = Resources.Load<Texture2D>("UI/Options/OrangeTab");
        purpleTab = Resources.Load<Texture2D>("UI/Options/PurpleTab");

        gameTitle = Resources.Load<Texture2D>("UI/Options/Game");
        graphicsTitle = Resources.Load<Texture2D>("UI/Options/Graphics");
        inputTitle = Resources.Load<Texture2D>("UI/Options/Input");
        bindingTitle = Resources.Load<Texture2D>("UI/Options/Binding");

        resDropDown = new DropDown();
        qualityDropDown = new DropDown();
        aaDropDown = new DropDown();

        fullscreenToggle = new Toggle();
        streamModeToggle = new Toggle();
        ambientOcculusionToggle = new Toggle();
        chromaticAberrationToggle = new Toggle();

        //Load Textures
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

        possibleScreens = new string[Screen.resolutions.Length];
        for (int i = 0; i < possibleScreens.Length; i++)
            possibleScreens[i] = Screen.resolutions[i].width + " x " + Screen.resolutions[i].height + " - " + Screen.resolutions[i].refreshRate + "hz";

        antiAliasingNames = new string[] {"Off", "FXAA Low", "FXAA Medium", "FXAA High", "TAA" };

        qualityNames = QualitySettings.names;

        //Get Current values
        currentResolution = Screen.resolutions.ToList().IndexOf(Screen.currentResolution);
        fullScreen = Screen.fullScreen;
        currentQuality = QualitySettings.GetQualityLevel();
        lastResolution = currentResolution;
        lastFullscreen = fullScreen;
        lastQuality = currentQuality;
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

        //Get a list of all Layouts
        allLayouts = new List<ControlLayout>();
        allLayouts.Add(KeyboardDevice.myDefault);
        allLayouts.Add(XBox360Device.myDefault);
        allLayouts.AddRange(InputManager.allAvailableConfigs);

        ResetControllerOptions();
    }

    public IEnumerator ActualShowOptions()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine(FadeGui(0f, 1f));
    }

    public void HideOptions()
    {
        StartCoroutine(ActualHideOptions());

        //Clear Values
        selectedInput = -1;
        lockInputs = false;
        InputManager.LoadAllControllerLayouts();
    }

    public IEnumerator ActualHideOptions()
    {
        yield return StartCoroutine(FadeGui(1f, 0f));
        enabled = false;
    }

    private IEnumerator FadeGui(float start, float end)
    {
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < fadeTime)
        {
            guiAlpha = Mathf.Lerp(start, end, (Time.unscaledTime - startTime) / fadeTime);
            yield return null;
        }

        guiAlpha = end;
    }

    void Update()
    {      
        //Handle Input
        if (!lockInputs && InputManager.controllers != null && InputManager.controllers.Count > 0 && guiAlpha == 1)
        {
            int tabChange = 0, vertical = 0;
            float horizontal = 0f;
            bool submitBool = false, cancelBool = false;

            if (!locked)
            {
                vertical = InputManager.controllers[0].GetRawIntInputWithLock("MenuVertical");
                horizontal = 0;

                if ((currentTab == OptionsTab.Graphics && currentSelection == 6) || (currentTab == OptionsTab.Binding && changingSlider))
                {
                    horizontal = InputManager.controllers[0].GetRawIntInputWithLock("MenuHorizontal");
                }
                else
                {
                    horizontal = InputManager.controllers[0].GetRawInput("MenuHorizontal");
                }

                tabChange = InputManager.controllers[0].GetRawIntInputWithLock("TabChange");
                submitBool = (InputManager.controllers[0].GetButtonWithLock("Submit"));
                cancelBool = (InputManager.controllers[0].GetButtonWithLock("Cancel"));
            }

            if (tabChange != 0)
            {
                currentTab += tabChange;
                currentTab = (OptionsTab)MathHelper.NumClamp((int)currentTab, 0, 4);
                ResetControllerOptions();
            }

            if (!Cursor.visible)
            {
                switch (currentTab)
                {
                    case OptionsTab.Game:

                        if (submitBool)
                        {
                            if(currentSelection < 3)
                                changingSlider = !changingSlider;
                            else if(currentSelection == 3 && FindObjectOfType<MainMenu>() != null)
                            {
                                //Find the Name Window and Show it
                                locked = true;
                                FindObjectOfType<ChangeName>().Show();
                            }
                            else if(currentSelection == 4 && FindObjectOfType<MainMenu>() != null)
                            {
                                //Find the PopUp Window and Show it
                                locked = true;
                                FindObjectOfType<MainMenu>().GetComponent<GamePopup>().ShowPopUp();
                            }
                            else if((currentSelection == 3 && FindObjectOfType<MainMenu>() == null) || currentSelection == 5)
                            {
                                streamMode = !streamMode;
                            }
                        }

                        if (!changingSlider)
                        {

                            int maxVal = 4;

                            if (FindObjectOfType<MainMenu>() != null)
                                maxVal = 6;

                            if (vertical != 0)
                                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, maxVal);

                            if (cancelBool)
                            {
                                Quit();
                            }
                        }
                        else
                        {
                            if (horizontal != 0)
                            {
                                float toChange = horizontal * Time.unscaledDeltaTime * sliderChangeRate;

                                if (currentSelection == 0)
                                    SoundManager.masterVolume = Mathf.Clamp(SoundManager.masterVolume + toChange, 0, 1);
                                else if (currentSelection == 1)
                                    SoundManager.musicVolume = Mathf.Clamp(SoundManager.musicVolume + toChange, 0, 1);
                                else if (currentSelection == 2)
                                    SoundManager.sfxVolume = Mathf.Clamp(SoundManager.sfxVolume + toChange, 0, 1);
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
                            else if(currentSelection == 3)
                            {
                                lastInput = (int)antiAliasing;
                                aaDropDown.toggled = !aaDropDown.toggled;
                                changingSlider = !changingSlider;
                            }
                            else if (currentSelection == 4)
                            {
                                ambientOcculusion = !ambientOcculusion;
                            }
                            else if (currentSelection == 5)
                            {
                                chromaticAberration = !chromaticAberration;
                            }
                            else if (currentSelection == 6) //Do Apply and Cancel Stuff
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

                        if (currentSelection == 6 && horizontal != 0f)
                            changingSlider = !changingSlider;

                        if ((currentSelection == 6 || !changingSlider) && vertical != 0)
                        {
                            if (currentSelection == 6)
                                changingSlider = false;

                            currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, 7);
                        }


                        if (!changingSlider)
                        {
                            if (cancelBool)
                                Quit();

                            if (resDropDown.toggled)
                                resDropDown.toggled = false;
                            else if (qualityDropDown.toggled)
                                qualityDropDown.toggled = false;
                        }
                        else
                        {
                            if (cancelBool)
                            {
                                if (currentSelection == 6)
                                {
                                    Quit();
                                }
                                else
                                {
                                    changingSlider = false;

                                    if (resDropDown.toggled)
                                        currentResolution = lastInput;
                                    else if (qualityDropDown.toggled)
                                        currentQuality = lastInput;
                                    else if (aaDropDown.toggled)
                                        antiAliasing = (EffectsManager.AntiAliasing)lastInput;
                                }
                            }

                            if (resDropDown.toggled)
                                resDropDown.SetScroll(currentResolution);
                            else if (qualityDropDown.toggled)
                                qualityDropDown.SetScroll(currentQuality);
                            else if (aaDropDown.toggled)
                                aaDropDown.SetScroll((int)antiAliasing);

                            if (vertical != 0)
                                if (resDropDown.toggled)
                                    currentResolution += vertical;
                                else if (qualityDropDown.toggled)
                                    currentQuality += vertical;
                                else if (aaDropDown.toggled)
                                    antiAliasing += vertical;
                        }
                        break;
                    case OptionsTab.Input:
                        if (submitBool)
                        {
                            if (currentSelection < 2)
                                changingSlider = !changingSlider;
                        }

                        if (!changingSlider)
                        {
                            int maxVal = 2;

                            if (vertical != 0)
                                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, maxVal);

                            if (cancelBool)
                            {
                                Quit();
                            }
                        }
                        else
                        {
                            if (horizontal != 0)
                            {
                                float toChange = horizontal * Time.unscaledDeltaTime * sliderChangeRate * 1.5f;
                                CurrentGameData gd = FindObjectOfType<CurrentGameData>();

                                if (currentSelection == 0)
                                    gd.mouseScale = Mathf.Clamp(gd.mouseScale + toChange, 0.1f, 10f);
                                else if (currentSelection == 1)
                                    gd.controllerScale = Mathf.Clamp(gd.controllerScale + toChange, 0.1f, 10f);
                            }

                            if (cancelBool)
                                changingSlider = false;
                        }
                        break;
                    case OptionsTab.Binding:

                        int configCount = allLayouts.Count;

                        if (cancelBool && (!changingSlider || currentSelection >= configCount) && !editName)
                            Quit();

                        if (!changingSlider)
                        {
                            if (!editName && vertical != 0)
                                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, Mathf.Min(configCount + 1, 12));

                            if (currentSelection < configCount)
                            {
                                currentLayoutSelection = currentSelection;

                                if (currentSelection > 1 && InputManager.controllers[0].inputType == InputType.Xbox360)
                                {
                                    if (InputManager.controllers[0].GetButtonWithLock("Edit") && FindObjectOfType<MainMenu>() != null)
                                        EditPressed();
                                    else if (InputManager.controllers[0].GetButtonWithLock("Minus") && FindObjectOfType<MainMenu>() != null)
                                        MinusPressed();
                                }
                            }

                            if (InputManager.controllers[0].inputType == InputType.Xbox360)
                            {
                                configScrollPosition.y += InputManager.controllers[0].GetRawInput("ViewScroll") * Time.unscaledDeltaTime * 300f;
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
                                layoutScrollPosition.y += Mathf.Sign(diff) * Time.unscaledDeltaTime * 300f;
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

                            string inputToChange = availableChanges[currentY];
                            if (horizontal != 0 && allLayouts[currentLayoutSelection].controls[inputToChange].Count >= 1)
                                currentX = !currentX;

                            if (vertical != 0)
                                currentY = MathHelper.NumClamp(currentY + vertical, 0, availableChanges.Length);

                            //Clear Input
                            if (currentSelection > 1 && InputManager.controllers[0].inputType == InputType.Xbox360)
                            {
                                if (InputManager.controllers[0].GetButtonWithLock("Minus"))
                                    ClearPressed();
                            }

                            //Scroll Scrollview with input
                            float diff = (currentY * 120) - configScrollPosition.y;
                            if (Mathf.Abs(diff) > 1f)
                                configScrollPosition.y += Mathf.Sign(diff) * Time.unscaledDeltaTime * 200f;

                            if (cancelBool)
                                changingSlider = false;
                        }
                        break;
                }
            }
        }
    }

    public void Quit()
    {
        //Save Mouse and Controller Scales
        FindObjectOfType<CurrentGameData>().SaveGame();

        if (somethingChanged)
        {
            //Find the PopUp Window and Show it
            locked = true;
            FindObjectOfType<CancelChanges>().ShowPopUp();
        }
        else
        {
            if (FindObjectOfType<MainMenu>() != null)
                FindObjectOfType<MainMenu>().BackMenu();
            else
                HideOptions();
        }
    }

    //Called when variables used for Controller inputs need to be reset
    private void ResetControllerOptions()
    {
        //Reset Controller Values
        if (currentTab == OptionsTab.Binding)
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
        allLayouts.Add(new ControlLayout(InputManager.controllers[0].inputType, 
            "Layout " + (allLayouts.Count - 1).ToString(),
            new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ControlName>>()));

        SaveChanges();
        selectedInput = -1;
        currentLayoutSelection = allLayouts.Count - 1;
        editName = false;
    }

    private void MinusPressed()
    {
        allLayouts.RemoveAt(currentLayoutSelection);
        SaveChanges();
        currentLayoutSelection -= 1;

        if (!Cursor.visible)
            currentSelection -= 1;

        selectedInput = -1;
        editName = false;
    }

    private void ClearPressed()
    {
        string inputToChange = availableChanges[currentY];
        allLayouts[currentLayoutSelection].ClearInput(inputToChange, currentX ? 1 : 0);

        SaveChanges();
    }

    private void EditPressed()
    {
        if (editName)
            SaveChanges();

        editName = !editName;
        selectedInput = -1;
    }

    void OnGUI()
    {
        GUI.depth = -50;

        if (guiAlpha > 0f && GUIHelper.DrawBack(1f) && !locked)
        {
            Quit();
        }

        if (guiAlpha > 0f)
        {

            GUIHelper.SetGUIAlpha(guiAlpha);
            GUI.matrix = GUIHelper.GetMatrix();
            GUI.skin = Resources.Load<GUISkin>("GUISkins/Options");

            //Draw all the titles
            if (currentTab != OptionsTab.Game)
            {
                Rect gameRect = new Rect(180, 95, 330, 70);
                GUI.DrawTexture(gameRect, gameTitle);

                if (!locked && GUI.Button(gameRect, ""))
                    currentTab = OptionsTab.Game;
            }

            if (currentTab != OptionsTab.Graphics)
            {
                Rect graphicsRect = new Rect(510, 95, 330, 70);
                GUI.DrawTexture(graphicsRect, graphicsTitle);

                if (!locked && GUI.Button(graphicsRect, ""))
                    currentTab = OptionsTab.Graphics;
            }

            if (currentTab != OptionsTab.Input)
            {
                Rect inputRect = new Rect(840, 95, 330, 70);
                GUI.DrawTexture(inputRect, inputTitle);

                if (!locked && GUI.Button(inputRect, ""))
                    currentTab = OptionsTab.Input;
            }

            if (currentTab != OptionsTab.Binding)
            {
                Rect bindingRect = new Rect(1170, 95, 330, 70);
                GUI.DrawTexture(bindingRect, bindingTitle);

                if (!locked && GUI.Button(bindingRect, ""))
                    currentTab = OptionsTab.Binding;
            }

            //Draw Q/E or LB/RB
            bool drawKeyboard = true;
            Rect leftIcon = new Rect(70, 107, 100, 50);
            Rect rightIcon = new Rect(1750, 107, 100, 50);

            if (InputManager.controllers != null && InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Xbox360)
                drawKeyboard = false;

            GUI.DrawTexture(leftIcon, drawKeyboard ? qTexture : lbTexture);
            GUI.DrawTexture(rightIcon, drawKeyboard ? eTexture : rbTexture);

            //Draw the current tab
            Rect tabRect = new Rect(180, 90, 1550, 870);
            Rect tabAreaRect = new Rect(180, 170, 1550, 800);
            //Required GUIStyles
            GUIStyle normalLabel = new GUIStyle(GUI.skin.label);
            GUIStyle selectedLabel = new GUIStyle(GUI.skin.label);

            selectedLabel.normal.textColor = Color.yellow;
            switch (currentTab)
            {
                case OptionsTab.Game:
                    GUI.DrawTexture(tabRect, blueTab);
                    GUIShape.RoundedRectangle(new Rect(210, 200, 1485, 700), 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                    GUIHelper.BeginGroup(tabAreaRect);

                    GUIStyle normalSlider = GUI.skin.horizontalSlider;
                    GUIStyle normalSliderThumb = GUI.skin.horizontalSliderThumb;
                    GUIStyle selectedSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                    selectedSliderThumb.normal.background = selectedSliderThumb.active.background;

                    GUI.Label(new Rect(20, 70, 300, 100), "Master Volume:", (!locked && currentSelection == 0 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);
                    SoundManager.masterVolume = GUI.HorizontalSlider(new Rect(330, 110, 1000, 100), SoundManager.masterVolume, 0f, 1f, normalSlider, (currentSelection == 0 && changingSlider && !Cursor.visible) ? selectedSliderThumb : normalSliderThumb);
                    GUI.Label(new Rect(1300, 70, 250, 100), (int)(SoundManager.masterVolume * 100f) + "%");

                    GUI.Label(new Rect(20, 180, 300, 100), "Music Volume:", (!locked && currentSelection == 1 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);
                    SoundManager.musicVolume = GUI.HorizontalSlider(new Rect(330, 220, 1000, 100), SoundManager.musicVolume, 0f, 1f, normalSlider, (currentSelection == 1 && changingSlider && !Cursor.visible) ? selectedSliderThumb : normalSliderThumb);
                    GUI.Label(new Rect(1300, 180, 250, 100), (int)(SoundManager.musicVolume * 100f) + "%");

                    GUI.Label(new Rect(20, 290, 300, 100), "SFX Volume:", (!locked && currentSelection == 2 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);
                    SoundManager.sfxVolume = GUI.HorizontalSlider(new Rect(330, 330, 1000, 100), SoundManager.sfxVolume, 0f, 1f, normalSlider, (currentSelection == 2 && changingSlider && !Cursor.visible) ? selectedSliderThumb : normalSliderThumb);
                    GUI.Label(new Rect(1300, 290, 250, 100), (int)(SoundManager.sfxVolume * 100f) + "%");

                    GUIStyle centreLabel = GUI.skin.label;
                    centreLabel.alignment = TextAnchor.MiddleCenter;

                    if (currentSelection < 3)
                    {
                        if (InputManager.controllers.Count > 0)
                        {
                            if (InputManager.controllers[0].inputType == InputType.Xbox360)
                            {
                                if (!changingSlider)
                                    GUI.Label(new Rect(20, 400, 1445, 50), "Press A to select an option!", centreLabel);
                                else
                                    GUI.Label(new Rect(20, 400, 1445, 50), "Press B to deselect the option!", centreLabel);
                            }

                            if (InputManager.controllers[0].inputType == InputType.Keyboard && !Cursor.visible)
                            {
                                if (!changingSlider)
                                    GUI.Label(new Rect(20, 400, 1445, 50), "Press Return to select an option!", centreLabel);
                                else
                                    GUI.Label(new Rect(20, 400, 1445, 50), "Press Return to deselect the option!", centreLabel);
                            }
                        }
                        else
                        {
                            changingSlider = false;
                        }
                    }

                    if (FindObjectOfType<MainMenu>() != null)
                    {
                        normalLabel.alignment = TextAnchor.MiddleLeft;
                        selectedLabel.alignment = TextAnchor.MiddleLeft;

                        Rect nameRect = new Rect(50, 450, 600, 100);
                        Rect actualName = new Rect(nameRect.x + tabAreaRect.x, nameRect.y + tabAreaRect.y, nameRect.width, nameRect.height);
                        GUI.Label(nameRect, "Change Player Name", (!locked && currentSelection == 3 && !Cursor.visible) || (!locked && Cursor.visible && actualName.Contains(GUIHelper.GetMousePosition())) ? selectedLabel : normalLabel);

                        if (!locked && Cursor.visible && GUI.Button(nameRect, ""))
                        {
                            //Find the PopUp Window and Show it
                            locked = true;
                            //Do Name Menu
                            FindObjectOfType<ChangeName>().Show();
                        }


                        Rect saveReset = new Rect(50, 525, 300, 100);
                        Rect actualSave = new Rect(saveReset.x + tabAreaRect.x, saveReset.y + tabAreaRect.y, saveReset.width, saveReset.height);

                        GUI.Label(saveReset, "Reset Save Data", (!locked && currentSelection == 4 && !Cursor.visible) || (!locked && Cursor.visible && actualSave.Contains(GUIHelper.GetMousePosition())) ? selectedLabel : normalLabel);

                        if (!locked && Cursor.visible && GUI.Button(saveReset, ""))
                        {
                            //Find the PopUp Window and Show it
                            locked = true;
                            FindObjectOfType<MainMenu>().GetComponent<GamePopup>().ShowPopUp();
                        }                        
                    }

                    bool newStreamVal = streamModeToggle.Draw(new Rect(50, 600, 350, 100), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, streamMode, "Stream Mode:", (!locked && ((currentSelection == 3 && FindObjectOfType<MainMenu>() == null) || currentSelection == 5) && !Cursor.visible) ? selectedLabel : normalLabel);
                    if (!locked)
                        streamMode = newStreamVal;

                    if(streamMode != FindObjectOfType<CurrentGameData>().streamMode)
                    {
                        FindObjectOfType<CurrentGameData>().streamMode = streamMode;
                        FindObjectOfType<CurrentGameData>().SaveGame();

                        if (streamMode)
                        {
                            //Make a Window Appear
                            locked = true;
                            InfoPopUp popUp = gameObject.AddComponent<InfoPopUp>();
                            popUp.Setup("Stream Mode Activated!\nPersonal information (Such as File Locations, IP addresses .etc) will no longer be displayed.");
                        }
                    }

                    break;
                case OptionsTab.Graphics:
                    GUI.DrawTexture(tabRect, greenTab);

                    GUIShape.RoundedRectangle(new Rect(210, 820, 400, 100), 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                    GUIHelper.BeginGroup(tabAreaRect);

                    //Change Resolution
                    GUI.Label(new Rect(20, 70, 300, 100), "Resolution:", (currentSelection == 0 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);

                    int newRes = resDropDown.Draw(new Rect(330, 90, 1000, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, currentResolution, possibleScreens);
                    if (!locked && currentResolution != -1 && newRes != currentResolution)
                    {
                        somethingChanged = true;
                        currentResolution = newRes;
                    }

                    //Fullscreen
                    GUI.Label(new Rect(20, 145, 300, 100), "Fullscreen:", (currentSelection == 1 && !Cursor.visible) ? selectedLabel : normalLabel);
                    bool newFSVal = fullscreenToggle.Draw(new Rect(20, 170, 350, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, fullScreen, "");

                    if (!locked)
                        fullScreen = newFSVal;

                    //Change Quality
                    GUI.Label(new Rect(20, 250, 300, 100), "Graphics Quality:", (currentSelection == 2 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);

                    int newQuality = qualityDropDown.Draw(new Rect(330, 250, 1000, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, currentQuality, qualityNames);
                    if (!locked && currentQuality != -1 && newQuality != currentQuality)
                    {
                        somethingChanged = true;
                        currentQuality = newQuality;
                    }

                    //Anti-Aliasing
                    GUI.Label(new Rect(20, 330, 300, 100), "Anti-Aliasing:", (currentSelection == 3 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);

                    int newAA = aaDropDown.Draw(new Rect(330, 340, 1000, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, (int)antiAliasing, antiAliasingNames);
                    if (!locked && newAA != (int)antiAliasing)
                    {
                        somethingChanged = true;
                        antiAliasing = (EffectsManager.AntiAliasing)newAA;
                    }

                    //Ambient Oculussion
                    GUI.Label(new Rect(20, 420, 300, 100), "Ambient Occulusion:", (currentSelection == 4 && !Cursor.visible) ? selectedLabel : normalLabel);
                    bool newAOVal = ambientOcculusionToggle.Draw(new Rect(20, 450, 350, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, ambientOcculusion, "");

                    if (!locked)
                        ambientOcculusion = newAOVal;

                    //Chromatic Aberration
                    GUI.Label(new Rect(20, 510, 300, 100), "Chromatic Aberration:", (currentSelection == 5 && !Cursor.visible) ? selectedLabel : normalLabel);
                    bool newCAVal = chromaticAberrationToggle.Draw(new Rect(20, 540, 350, 50), new Vector2(tabAreaRect.x, tabAreaRect.y), 50, chromaticAberration, "");

                    if (!locked)
                        chromaticAberration = newCAVal;

                    //Apply and Cancel Buttons
                    Rect cancelRect = GUIHelper.CentreRectLabel(new Rect(50, 650, 150, 100), cancelScale, "Cancel", (cancelScale > 1.1f || (currentSelection == 6 && !changingSlider)) ? Color.yellow : Color.white);
                    cancelScale = GUIHelper.SizeHover(cancelRect, cancelScale, 1f, 1.3f, 3f);

                    if (GUI.Button(cancelRect, ""))
                        ResetEverything();

                    Rect applyRect = GUIHelper.CentreRectLabel(new Rect(270, 650, 150, 100), applyScale, "Apply", (applyScale > 1.1f || (currentSelection == 6 && changingSlider)) ? Color.yellow : Color.white);
                    applyScale = GUIHelper.SizeHover(applyRect, applyScale, 1f, 1.3f, 3f);

                    if (GUI.Button(applyRect, ""))
                    {
                        somethingChanged = false;
                        SaveEverything();
                    }

                    break;
                case OptionsTab.Input:
                    GUI.DrawTexture(tabRect, orangeTab);
                    GUIShape.RoundedRectangle(new Rect(210, 200, 1485, 700), 25, new Color(0, 0, 0, 0.25f * guiAlpha));

                    GUIHelper.BeginGroup(tabAreaRect);

                    normalSlider = GUI.skin.horizontalSlider;
                    normalSliderThumb = GUI.skin.horizontalSliderThumb;
                    selectedSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                    selectedSliderThumb.normal.background = selectedSliderThumb.active.background;

                    normalLabel.fontSize = (int)(normalLabel.fontSize * 0.75f);
                    selectedLabel.fontSize = normalLabel.fontSize;

                    CurrentGameData gd = FindObjectOfType<CurrentGameData>();

                    GUI.Label(new Rect(20, 70, 300, 100), "Mouse Sensitivity\n(For Replay Mode):", (!locked && currentSelection == 0 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);
                    gd.mouseScale = GUI.HorizontalSlider(new Rect(330, 110, 1000, 100), gd.mouseScale, 0.1f, 10f, normalSlider, (currentSelection == 0 && changingSlider && !Cursor.visible) ? selectedSliderThumb : normalSliderThumb);
                    GUI.Label(new Rect(1300, 70, 250, 100), gd.mouseScale.ToString("0.00"));

                    GUI.Label(new Rect(20, 180, 300, 100), "Controller Sensitivity\n(For Replay Mode):", (!locked && currentSelection == 1 && !changingSlider && !Cursor.visible) ? selectedLabel : normalLabel);
                    gd.controllerScale = GUI.HorizontalSlider(new Rect(330, 220, 1000, 100), gd.controllerScale, 0.1f, 10f, normalSlider, (currentSelection == 1 && changingSlider && !Cursor.visible) ? selectedSliderThumb : normalSliderThumb);
                    GUI.Label(new Rect(1300, 180, 250, 100), gd.controllerScale.ToString("0.00"));

                    break;
                case OptionsTab.Binding:
                    GUI.DrawTexture(tabRect, purpleTab);

                    bool showXbox = InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Xbox360;

                    if (showXbox && !changingSlider)
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
                    if (InputManager.allAvailableConfigs.Count < 10)
                    {
                        Rect addLabel = new Rect(50, 700, 30, 50);
                        GUIHelper.CentreRectLabel(addLabel, plusScale, "+", (!Cursor.visible && currentSelection == allLayouts.Count && !changingSlider) ? Color.yellow : Color.white);

                        if (GUI.Button(addLabel, ""))
                        {
                            //Add New Layout
                            PlusPressed();
                        }

                        plusScale = GUIHelper.SizeHover(addLabel, plusScale, 2f, 2.5f, 2f);
                    }

                    //Draw - Gui
                    if (currentLayoutSelection >= 2 && (Cursor.visible || (currentSelection < allLayouts.Count && !changingSlider)) && FindObjectOfType<MainMenu>() != null)
                    {
                        Rect minusRect = new Rect(140, 700, 30, 50);
                        GUIHelper.CentreRectLabel(minusRect, minusScale, "-", Color.white);

                        if (GUI.Button(minusRect, ""))
                        {
                            MinusPressed();
                        }
                        minusScale = GUIHelper.SizeHover(minusRect, minusScale, 2f, 2.5f, 2f);

                        if (showXbox && currentSelection < allLayouts.Count)
                            GUI.DrawTexture(new Rect(130, 650, 50, 50), yTexture);
                    }

                    //Draw Clear Gui
                    if (currentLayoutSelection >= 2 && !Cursor.visible && (currentSelection < allLayouts.Count && changingSlider))
                    {
                        Rect minusRect = new Rect(120, 675, 100, 100);
                        GUI.Label(minusRect, "Clear");

                        if (GUI.Button(minusRect, ""))
                        {
                            ClearPressed();
                        }
                       
                        if (showXbox && currentSelection < allLayouts.Count)
                            GUI.DrawTexture(new Rect(130, 650, 50, 50), yTexture);
                    }

                    //Draw Edit Gui
                    if (currentLayoutSelection >= 2 && (Cursor.visible || (currentSelection < allLayouts.Count && !changingSlider)) && FindObjectOfType<MainMenu>() != null)
                    {
                        Rect editRect = new Rect(210, 675, 100, 100);
                        GUIHelper.CentreRectLabel(editRect, editScale, !editName ? "edit" : "save", Color.white);

                        if (GUI.Button(editRect, ""))
                        {
                            EditPressed();
                        }

                        editScale = GUIHelper.SizeHover(editRect, editScale, 1f, 1.25f, 2f);

                        if (showXbox && currentSelection < allLayouts.Count)
                            GUI.DrawTexture(new Rect(235, 650, 50, 50), xTexture);
                    }

                    if (currentLayoutSelection >= 2 && showXbox && currentSelection < allLayouts.Count)
                    {
                        GUI.Label(new Rect(320, 675, 100, 100), !editName ? "bind" : "type");
                        GUI.DrawTexture(new Rect(345, 650, 50, 50), aTexture);
                    }

                    if (inputScales == null || inputScales.Length != allLayouts.Count)
                    {
                        inputScales = new float[allLayouts.Count];
                        for (int i = 0; i < inputScales.Length; i++)
                            inputScales[i] = 1f;
                    }

                    layoutScrollPosition = GUIHelper.BeginScrollView(scrollviewRect, layoutScrollPosition, new Rect(10, 10, 380, allLayouts.Count * 70));

                    for (int i = 0; i < allLayouts.Count; i++)
                    {
                        //Draw Label
                        Rect labelRect;
                        if (currentLayoutSelection != i || !editName)
                        {
                            labelRect = GUIHelper.CentreRectLabel(new Rect(10, 40 + (70 * i), 380, 50), inputScales[i], allLayouts[i].layoutName, ((Cursor.visible && currentLayoutSelection == i) || (!Cursor.visible && currentSelection == i && !changingSlider)) ? Color.yellow : Color.white);
                        }
                        else
                        {
                            labelRect = new Rect(30, 40 + (70 * i), 300, 50);
                            allLayouts[i].layoutName = GUI.TextField(labelRect, allLayouts[i].layoutName);
                        }

                        Rect labelClickRect = new Rect(labelRect);
                        if (GUI.Button(labelClickRect, ""))
                        {
                            currentLayoutSelection = i;
                            if (editName)
                            {
                                editName = false;
                                InputManager.LoadAllControllerLayouts();
                            }
                        }
                        inputScales[i] = GUIHelper.SizeHover(labelClickRect, inputScales[i], 1f, 1.25f, 2f);

                        //Draw Icon
                        Rect iconRect = GUIHelper.CentreRect(new Rect(330, 40 + (70 * i), 50, 50), inputScales[i]);
                        GUI.DrawTexture(iconRect, Resources.Load<Texture2D>("UI/Controls/" + ((allLayouts[i].inputType == InputType.Keyboard) ? "Keyboard" : "Xbox_1")));

                        if (editName && currentLayoutSelection == i && GUI.Button(iconRect, ""))
                        {
                            ChangeType(i);
                        }

                    }
                    GUIHelper.EndScrollView();

                    //Draw input Configuration
                    ControlLayout current = allLayouts[currentLayoutSelection];

                    GUI.Label(new Rect(500, 20, 333, 50), "Action");
                    GUI.Label(new Rect(833, 20, 333, 50), "Input #1");
                    GUI.Label(new Rect(1166, 20, 333, 50), "Input #2");

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
                            if (current.inputType == InputType.Keyboard)
                                labelText = "Press any Key";
                            else
                                labelText = "Press any Button";

                            commandSizes[(i * 2)] = 0.7f;

                            GUI.DrawTexture(new Rect(333, 35 + (i * 120), 333, 65), Resources.Load<Texture2D>("UI/Options/Button"), ScaleMode.ScaleToFit);
                        }
                        else if (current.controls.ContainsKey(availableChanges[i]) && current.controls[availableChanges[i]].Count > 0)
                        {
                            labelText = current.controls[availableChanges[i]][0].inputName;
                        }

                        GUIHelper.CentreRectLabel(labelRect, commandSizes[(i * 2)], labelText, (!Cursor.visible && changingSlider && !currentX && currentY == i) ? Color.yellow : Color.white);

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
                            if (current.inputType == InputType.Keyboard)
                                labelText = "Press any Key";
                            else
                                labelText = "Press any Button";

                            commandSizes[(i * 2) + 1] = 0.7f;

                            GUI.DrawTexture(new Rect(666, 35 + (i * 120), 333, 65), Resources.Load<Texture2D>("UI/Options/Button"), ScaleMode.ScaleToFit);
                        }
                        else if (current.controls.ContainsKey(availableChanges[i]) && current.controls[availableChanges[i]].Count > 1)
                        {
                            labelText = current.controls[availableChanges[i]][1].inputName;
                        }

                        GUIHelper.CentreRectLabel(labelRect, commandSizes[(i * 2) + 1], labelText, (!Cursor.visible && changingSlider && currentX && currentY == i) ? Color.yellow : Color.white);

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
                    bool usesAxis = false;
                    string xboxInput = InputManager.GetXboxInput(ref usesAxis);

                    if (selectedInput != -1 && noInput && lockInputs && current.inputType == InputType.Keyboard && xboxInput != "")
                    {
                        RemoveInput(selectedInput);
                        selectedInput = -1;
                    }

                    if (selectedInput != -1 && noInput && lockInputs && current.inputType == InputType.Xbox360 && InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Keyboard && Input.GetKeyDown(KeyCode.Escape))
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
                            InputDevice.locked = true;

                            if (current.inputType == InputType.Keyboard)
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
                                        current.ChangeInput(inputToChange, newInput, usesAxis, 0);
                                    else
                                        current.ChangeInput(inputToChange, newInput, usesAxis, 1);

                                    SaveChanges();
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
                                        current.ChangeInput(inputToChange, newInput, usesAxis, 0);
                                    else
                                        current.ChangeInput(inputToChange, newInput, usesAxis, 1);

                                    SaveChanges();
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
                            InputDevice.locked = false;
                        }
                    }
                    break;
            }
            GUIHelper.EndGroup();
        }
    }

    private void SaveChanges()
    {
        List<ControlLayout> newList = new List<ControlLayout>(allLayouts);
        newList.RemoveRange(0, 2);

        InputManager.SetAllControllerLayouts(newList);
        InputManager.SaveAllControllerLayouts();
    }

    private void RemoveInput(int toRemove)
    {
        string inputToChange = availableChanges[selectedInput / 2];
        ControlLayout current = allLayouts[currentLayoutSelection];

        if (selectedInput % 2 == 0)
        {
            current.controls[inputToChange].RemoveAt(0);
        }
        else
        {
            current.controls[inputToChange].RemoveAt(1);
        }

        SaveChanges();
    }

    private void ResetEverything()
    {
        currentResolution = lastResolution;
        fullScreen = lastFullscreen;
        currentQuality = lastQuality;
        streamMode = FindObjectOfType<CurrentGameData>().streamMode;

        EffectsManager effectsMan = FindObjectOfType<EffectsManager>();
        chromaticAberration = effectsMan.GetUseChromaticAberration();
        ambientOcculusion = effectsMan.GetUseAmbientOcculusion();
        antiAliasing = effectsMan.GetAntiAliasing();
    }

    public void SaveEverything()
    {
        Screen.SetResolution(Screen.resolutions[currentResolution].width, Screen.resolutions[currentResolution].height, fullScreen);
        QualitySettings.SetQualityLevel(currentQuality);

        lastResolution = currentResolution;
        lastFullscreen = fullScreen;
        lastQuality = currentQuality;

        //Update Effects Manager
        FindObjectOfType<EffectsManager>().UpdateValues(chromaticAberration, ambientOcculusion, antiAliasing);
    }

    private void ChangeType(int i)
    {
        allLayouts[i].inputType++;
        if ((int)allLayouts[i].inputType >= 2)
            allLayouts[i].inputType = 0;

        allLayouts[i].controls = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ControlName>>();
        SaveChanges();
    }
}
