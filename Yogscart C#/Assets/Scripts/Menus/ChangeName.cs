using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeName : MonoBehaviour
{
    private float guiAlpha;
    public GUISkin skin;

    private string playerName = "";
    private CurrentGameData gd;
    private GUIKeyboard guiKeyboard;

    public bool showing { get { return guiAlpha != 0; } }

    bool submitBool, cancelBool, locked = false;
    float hori, vert;

    public void Start()
    {
        //PlayerPrefs.DeleteAll();

        if(FindObjectOfType<MainMenu>() == null && FindObjectOfType<IntroScript>() == null)
            Show();
    }

    public void Show()
    {
        gd = FindObjectOfType<CurrentGameData>();
        locked = false;

        if (gd != null)
            playerName = gd.playerName;

        if (playerName == "")
            playerName = "Player";

        guiKeyboard = new GUIKeyboard(new Rect(210, 400, 1500, 540));

        if(FindObjectOfType<MainMenu>() == null)
            InputManager.allowedToChange = true;

        InputManager.keyboardAllowed = false;

        StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        StartCoroutine(FadeTo(0f));

        Options options = FindObjectOfType<Options>();
        options.locked = false;

        InputManager.keyboardAllowed = true;
    }

    private IEnumerator FadeTo(float finalVal)
    {
        float startTime = Time.time, startVal = guiAlpha;
        float travelTime = 0.5f;

        while (Time.time - startTime < travelTime)
        {
            guiAlpha = Mathf.Lerp(startVal, finalVal, (Time.time - startTime) / travelTime);
            yield return null;
        }

        guiAlpha = finalVal;
    }

    const string validCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_ ";

    void OnGUI()
    {

        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = skin;
        GUI.depth = -150;

        if (guiAlpha > 0)
        {
            //Background
            Rect backgroundRect = new Rect(200, 100, 1520, 880);
            GUIShape.RoundedRectangle(GUIHelper.CentreRect(backgroundRect,guiAlpha), 20, new Color(52 / 255f, 152 / 255f, 219 / 255f, guiAlpha));

            GUIStyle adjustedLabel = new GUIStyle(skin.label);
            adjustedLabel.fontSize = (int)(adjustedLabel.fontSize * guiAlpha);

            //Instructions
            GUI.Label(GUIHelper.RectScaledbyOtherRect(new Rect(210, 110, 1500, 80), backgroundRect, guiAlpha), "Enter a Nickname:", adjustedLabel);

            adjustedLabel.fontSize /= 2;

            GUI.Label(GUIHelper.RectScaledbyOtherRect(new Rect(210, 150, 1500, 80), backgroundRect, guiAlpha), "(This will be used for Time Trial Ghosts and Online Races)", adjustedLabel);

            adjustedLabel.fontSize *= 4;

            //Text Field
            Rect textField = GUIHelper.RectScaledbyOtherRect(new Rect(250, 210, 1420, 180),backgroundRect, guiAlpha);
            GUIShape.RoundedRectangle(textField, 10, new Color(1f, 1f, 1f, 0.2f));
            GUI.Label(textField, playerName, adjustedLabel);

            //Remove Invalid names
            for(int i = 0; i < playerName.Length; i++)
            {
                if(!validCharacters.Contains(playerName[i].ToString().ToUpper()))
                {
                    playerName = playerName.Remove(i, 1);
                    i--;
                }
            }

            //Draw Keyboard
            bool useController = true;

            if (InputManager.controllers.Count == 0 || InputManager.controllers[0].controlLayout.Type == ControllerType.Keyboard)
                useController = false;

            guiKeyboard.guiAlpha = guiAlpha;
            guiKeyboard.drawRect = GUIHelper.RectScaledbyOtherRect(new Rect(210, 400, 1500, 540), backgroundRect, guiAlpha);
            playerName = guiKeyboard.Draw(playerName, 15, guiAlpha, useController, submitBool, cancelBool, vert, hori);

            if (submitBool || cancelBool || vert != 0 || hori != 0)
            {
                submitBool = false;
                cancelBool = false;
                vert = 0;
                hori = 0;
            }

            if (guiKeyboard.completed && !locked)
                Finish();
        }
    }

    void Update()
    {
        if (guiAlpha == 1f)
        {
            if (InputManager.controllers.Count > 0)
            {
                submitBool = InputManager.controllers[0].GetMenuInput("Submit") != 0;
                cancelBool = InputManager.controllers[0].GetMenuInput("Cancel") != 0;
                vert = InputManager.controllers[0].GetMenuInput("MenuVertical");
                hori = InputManager.controllers[0].GetMenuInput("MenuHorizontal");
            }
        }
    }

    void Finish()
    {
        if (gd != null)
        {
            gd.playerName = playerName;
            gd.SaveGame();
        }

        locked = true;

        Hide();
    }
}
