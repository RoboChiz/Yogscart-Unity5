using UnityEngine;
using System.Collections;

public class Options : MonoBehaviour
{
    CurrentGameData gd;
    SoundManager sm;

    private float guiAlpha = 0;
    private const float fadeTime = 0.5f;

    private Texture2D blueTab, greenTab, orangeTab, gameTitle, graphicsTitle, inputTitle;

    enum OptionsTab { Game, Graphics, Input };
    private OptionsTab currentTab = OptionsTab.Game;

    private DropDown resDropDown, qualityDropDown;
    private Toggle fullscreenToggle;
    private int currentResolution = 0, currentQuality = 0;

    private float cancelScale = 1f, applyScale = 1f, plusScale = 2f;
    private bool fullScreen, somethingChanged = false;

    private int currentLayoutSelection = 0;
    private Vector2 layoutScrollPosition, configScrollPosition;
    private float[] inputScales;

    // Use this for initialization
    void Start()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

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
    }

    public void ShowOptions()
    {
        ResetEverything();
        StartCoroutine(ActualShowOptions());
    }

    public IEnumerator ActualShowOptions()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeGui(0f, 1f));
    }

    public void HideOptions()
    {
        StartCoroutine(ActualHideOptions());
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
        if (InputManager.controllers != null && InputManager.controllers.Count > 0 && guiAlpha == 1)
        {
            int vertical = InputManager.controllers[0].GetMenuInput("MenuVertical");
            int tabChange = InputManager.controllers[0].GetMenuInput("TabChange");
            bool submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);

            if (tabChange != 0)
            {
                currentTab += tabChange;
                currentTab = (OptionsTab)MathHelper.NumClamp((int)currentTab, 0, 3);
            }
        }
    }

    void OnGUI()
    {
        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Options");

        Vector2 mousePosition = GUIHelper.GetMousePosition();

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

        //Draw the current tab
        Rect tabRect = new Rect(180, 90, 1550, 870);
        Rect tabAreaRect = new Rect(180, 170, 1550, 800);

        switch (currentTab)
        {
            case OptionsTab.Game:
                GUI.DrawTexture(tabRect, blueTab);

                GUI.BeginGroup(tabAreaRect);

                GUI.Label(new Rect(20, 70, 300, 100), "Master Volume:");
                SoundManager.masterVolume = GUI.HorizontalSlider(new Rect(330, 110, 1000, 100), SoundManager.masterVolume, 0f, 100f);
                GUI.Label(new Rect(1300, 70, 250, 100), (int)SoundManager.masterVolume + "%");

                GUI.Label(new Rect(20, 180, 300, 100), "Music Volume:");
                SoundManager.musicVolume = GUI.HorizontalSlider(new Rect(330, 220, 1000, 100), SoundManager.musicVolume, 0f, 100f);
                GUI.Label(new Rect(1300, 180, 250, 100), (int)SoundManager.musicVolume + "%");

                GUI.Label(new Rect(20, 290, 300, 100), "SFX Volume:");
                SoundManager.sfxVolume = GUI.HorizontalSlider(new Rect(330, 330, 1000, 100), SoundManager.sfxVolume, 0f, 100f);
                GUI.Label(new Rect(1300, 290, 250, 100), (int)SoundManager.sfxVolume + "%");

                break;
            case OptionsTab.Graphics:
                GUI.DrawTexture(tabRect, greenTab);

                GUI.BeginGroup(tabAreaRect);

                //Fullscreen
                bool newFullscreen = fullscreenToggle.Draw(new Rect(20, 170, 350, 50), 50, fullScreen, "Fullscreen:");
                if (!resDropDown.toggled)
                    fullScreen = newFullscreen;

                //Change Quality
                GUI.Label(new Rect(20, 250, 300, 100), "Graphics Quality:");

                int newQuality = qualityDropDown.Draw(new Rect(330, 250, 1000, 50), 50, currentQuality, QualitySettings.names);
                if (newQuality != currentQuality)
                    somethingChanged = true;
                currentQuality = newQuality;

                //Change Resolution
                GUI.Label(new Rect(20, 70, 300, 100), "Resolution:");

                string[] possibleScreens = new string[Screen.resolutions.Length];
                for (int i = 0; i < possibleScreens.Length; i++)
                    possibleScreens[i] = Screen.resolutions[i].width + " x " + Screen.resolutions[i].height;

                int newRes = resDropDown.Draw(new Rect(330, 90, 1000, 50), 50, currentResolution, possibleScreens);
                if (newRes != currentResolution)
                    somethingChanged = true;
                currentResolution = newRes;

                //Apply and Cancel Buttons
                Rect cancelRect = GUIHelper.CentreRectLabel(new Rect(20, 650, 200, 100), cancelScale, "Cancel", (cancelScale > 1.1f) ? Color.yellow : Color.white);

                Rect cancelClickRect = new Rect(cancelRect);
                cancelClickRect.x += tabAreaRect.x;
                cancelClickRect.y += tabAreaRect.y;

                if (cancelClickRect.Contains(mousePosition))
                {
                    if (cancelScale < 1.5f)
                        cancelScale += Time.deltaTime * 4f;
                    else
                        cancelScale = 1.5f;
                }
                else
                {
                    if (cancelScale > 1f)
                        cancelScale -= Time.deltaTime * 4f;
                    else
                        cancelScale = 1f;
                }

                if (GUI.Button(cancelRect, ""))
                    ResetEverything();

                Rect applyRect = GUIHelper.CentreRectLabel(new Rect(250, 650, 200, 100), applyScale, "Apply", (applyScale > 1.1f) ? Color.yellow : Color.white);

                Rect applyClickRect = new Rect(applyRect);
                applyClickRect.x += tabAreaRect.x;
                applyClickRect.y += tabAreaRect.y;

                applyScale = GUIHelper.SizeHover(applyClickRect, applyScale, 1f, 1.5f, 4f);

                if (GUI.Button(applyRect, ""))
                {
                    somethingChanged = false;
                    SaveEverything();
                }

                break;
            case OptionsTab.Input:
                GUI.DrawTexture(tabRect, orangeTab);
                GUI.BeginGroup(tabAreaRect);

                if (inputScales == null || inputScales.Length != InputManager.AllConfigs.Count)
                {
                    inputScales = new float[InputManager.AllConfigs.Count];
                    for (int i = 0; i < inputScales.Length; i++)
                        inputScales[i] = 1f;
                }

                GUI.Label(new Rect(0, 20, 450, 50), "Available Layouts");
                GUI.DrawTexture(new Rect(450, 20, 5, tabAreaRect.height - 60), Resources.Load<Texture2D>("UI/Lobby/Line"));

                Rect scrollviewRect = new Rect(30, 70, 400, tabAreaRect.height - 120);
                GUI.DrawTexture(scrollviewRect, Resources.Load<Texture2D>("UI/Options/InputBack"));               

                scrollviewRect.y += 10;
                scrollviewRect.width += 12;
                scrollviewRect.height -= 110;

                //Draw + Gui
                Rect addLabel = new Rect(330, 650, 100, 100);
                GUIHelper.CentreRectLabel(addLabel, plusScale, "+", Color.white);

                addLabel.x += tabAreaRect.x;
                addLabel.y += tabAreaRect.y;
                plusScale = GUIHelper.SizeHover(addLabel, plusScale, 2f, 2.5f, 2f);

                layoutScrollPosition = GUI.BeginScrollView(scrollviewRect, layoutScrollPosition, new Rect(10, 10, 380, InputManager.AllConfigs.Count * 500));

                for (int i = 0; i < InputManager.AllConfigs.Count; i++)
                {
                    //Draw Label
                    Rect labelRect = GUIHelper.CentreRectLabel(new Rect(10, 40 + (70 * i), 380, 50), inputScales[i], InputManager.AllConfigs[i].Name, (currentLayoutSelection == i) ? Color.yellow : Color.white);

                    Rect labelClickRect = new Rect(labelRect);
                    if (GUI.Button(labelClickRect, ""))
                        currentLayoutSelection = i;

                    labelClickRect.x += tabAreaRect.x + scrollviewRect.x;
                    labelClickRect.y += tabAreaRect.y + scrollviewRect.y;
                    inputScales[i] = GUIHelper.SizeHover(labelClickRect, inputScales[i], 1f, 1.25f, 2f);

                    //Draw Icon
                    Rect iconRect = GUIHelper.CentreRect(new Rect(330, 40 + (70 * i), 50, 50), inputScales[i]);
                    GUI.DrawTexture(iconRect, Resources.Load<Texture2D>("UI/Controls/" + ((InputManager.AllConfigs[i].Type == ControllerType.Keyboard) ? "Keyboard" : "Xbox_1")));
                }
                GUI.EndScrollView();

                //Draw input Configuration
                InputLayout current = InputManager.AllConfigs[currentLayoutSelection];

                GUI.Label(new Rect(500, 20, 333, 50), "Action");
                GUI.Label(new Rect(833, 20, 333, 50), (current.Type == ControllerType.Keyboard) ? "Key +" : "Button +");
                GUI.Label(new Rect(1166, 20, 333, 50), (current.Type == ControllerType.Keyboard) ? "Key -" : "Button -");

                Rect configScrollView = new Rect(500, 70, 1000, tabAreaRect.height - 120);
                GUI.DrawTexture(configScrollView, Resources.Load<Texture2D>("UI/Options/InputBack"));

                configScrollView.y += 10;
                configScrollView.width += 10;
                configScrollView.height -= 20;

                configScrollPosition = GUI.BeginScrollView(configScrollView, configScrollPosition, new Rect(0,0,980,3000));

                //"Default,Xbox360,Throttle:A,Throttle:B,Steer:L_XAxis,Drift:TriggersL,Drift:TriggersR,Item:LB,Item:RB,RearView:X,Pause:Start,Submit:Start,Submit:A,Cancel:B,MenuHorizontal:L_XAxis,MenuVertical:L_YAxis,Rotate:R_XAxis,TabChange:RB,TabChange:LB

                string[] availableChanges = new string[] { "Throttle","Brake", "Steer (Right/Left)", "Drift", "Item", "Look Behind" };

                for(int i = 0; i < availableChanges.Length; i++)
                {
                    GUI.Label(new Rect(0, 20 + (i* 120), 333, 100), availableChanges[i]);

                    //Change string to actual input name
                    if (availableChanges[i] == "Look Behind")
                        availableChanges[i] = "RearView";
                    if (availableChanges[i] == "Steer (Right/Left)")
                        availableChanges[i] = "Steer";

                    if(current.commandsOne.ContainsKey(availableChanges[i]))
                        GUI.Label(new Rect(333, 20 + (i * 120), 333, 100), current.commandsOne[availableChanges[i]]);
                    if (current.commandsTwo.ContainsKey(availableChanges[i]))
                        GUI.Label(new Rect(666, 20 + (i * 120), 333, 100), current.commandsTwo[availableChanges[i]]);

                    if(i < availableChanges.Length - 1)
                        GUI.DrawTexture(new Rect(50, 120 + (i * 120), 900, 5), Resources.Load<Texture2D>("UI/Lobby/Line"));
                }

                GUI.EndScrollView();

                break;
        }

        GUI.EndGroup();
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
}
