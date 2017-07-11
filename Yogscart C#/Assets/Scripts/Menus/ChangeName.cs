using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeName : MonoBehaviour
{
    private float guiAlpha;
    public GUISkin skin;

    private string playerName = "";
    private CurrentGameData gd;

    public bool showing { get { return guiAlpha != 0; } }

    public void Show()
    {
        gd = FindObjectOfType<CurrentGameData>();
        InputManager.allowedToChange = true;

        if (gd != null)
            playerName = gd.playerName;

        if (playerName == "")
            playerName = "Player";

        StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        StartCoroutine(FadeTo(0f));

        Options options = FindObjectOfType<Options>();
        options.locked = false;
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
            GUI.Label(GUIHelper.RectScaledbyOtherRect(new Rect(210, 110, 1500, 80), backgroundRect, guiAlpha), "Enter your Name:", adjustedLabel);

            GUIStyle adjustedTextField = new GUIStyle(skin.textField);
            adjustedTextField.fontSize = (int)(adjustedTextField.fontSize * guiAlpha);

            //Text Field
            playerName = GUI.TextField(GUIHelper.RectScaledbyOtherRect(new Rect(250, 210, 1420, 180), backgroundRect, guiAlpha), playerName, 15, adjustedTextField);

            //Remove Invalid names
            for(int i = 0; i < playerName.Length; i++)
            {
                if(!validCharacters.Contains(playerName[i].ToString().ToUpper()))
                {
                    playerName = playerName.Remove(i, 1);
                    i--;
                }
            }
        }
    }

    void Update()
    {
        if (guiAlpha == 1f)
        {
            if (InputManager.controllers.Count > 0)
            {
                if (InputManager.controllers[0].GetMenuInput("Submit") != 0 || InputManager.controllers[0].GetMenuInput("Cancel") != 0)
                {
                    if (playerName != "")
                    {
                        gd.playerName = playerName;
                        gd.SaveGame();

                        Hide();
                    }
                }
            }
        }
    }
}
