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

    private float cancelScale = 1f, applyScale = 1f;
    private bool fullScreen, somethingChanged = false;

    // Use this for initialization
    void Start()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        sm = GameObject.FindObjectOfType<SoundManager>();

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

                if (applyClickRect.Contains(mousePosition))
                {
                    if (applyScale < 1.5f)
                        applyScale += Time.deltaTime * 4f;
                    else
                        applyScale = 1.5f;
                }
                else
                {
                    if (applyScale > 1f)
                        applyScale -= Time.deltaTime * 4f;
                    else
                        applyScale = 1;
                }

                if (GUI.Button(applyRect, ""))
                {
                    somethingChanged = false;
                    SaveEverything();
                }

                break;
            case OptionsTab.Input:
                GUI.DrawTexture(tabRect, orangeTab);

                GUI.BeginGroup(tabAreaRect);
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
