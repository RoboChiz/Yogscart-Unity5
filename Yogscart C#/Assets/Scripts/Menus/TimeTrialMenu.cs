using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialMenu : MonoBehaviour
{
    private float guiAlpha = 0f;
    public bool showing { get { return guiAlpha != 0; } }
    public bool hiding = false;

    public int selectedOption = 0;
    private float[] optionScales;

    enum TTMMenuState { None, ChoosingDev, ChoosingLocal}
    TTMMenuState menuState = TTMMenuState.None;

    bool choosingGhost = false;

    private readonly string[] options = new string[] { "Race by Yourself", "Race Developer Ghost", "Race Ghost" };
    private SoundManager sm;

	public void Show()
    {
        sm = FindObjectOfType<SoundManager>();
        selectedOption = 0;
        optionScales = new float[options.Length];
        hiding = false;

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

                    optionScales[i] = Mathf.Clamp(optionScales[i] + (Time.deltaTime * 5f), 1f, 1.2f);
                }
                else
                    optionScales[i] = Mathf.Clamp(optionScales[i] - (Time.deltaTime * 5f), 1f, 1.2f);

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
        }

        menuState = (TTMMenuState)selectedOption;
    }

    void Update()
    {
        if(guiAlpha == 1f)
        {
            int vert = InputManager.controllers[0].GetMenuInput("MenuVertical");
            bool submitBool = (InputManager.controllers[0].GetMenuInput("Submit") != 0);
            bool cancelBool = (InputManager.controllers[0].GetMenuInput("Cancel") != 0);

            if (!choosingGhost && vert != 0)
                selectedOption = MathHelper.NumClamp(selectedOption + vert, 0, options.Length);

            if (submitBool)
                DoSubmit();

            if (cancelBool)
                DoCancel();
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
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/confirm"));

        if (!choosingGhost)
        {
            switch(options[selectedOption])
            {
                case "Race by Yourself":
                    Hide();
                    FindObjectOfType<TimeTrial>().FinishTimeTrialMenu();
                    break;
                case "Race Ghost":
                case "Race Developer Ghost":
                    choosingGhost = true;
                    break;
            }
        }
    }
}
