using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePopup : MonoBehaviour
{
    public float guiAlpha { get; private set; }
    public GUISkin skin;

    protected bool noSelected = true;

    public virtual string text { get { return "Are you sure you want to reset all save data?"; } }
    public virtual bool showYesNo { get { return true; } }

    public virtual Rect boxRect { get { return new Rect(760, 440, 400, 200); } }
    public virtual Rect yesRect { get { return new Rect(770, 570, 180, 60); } }
    public virtual Rect noRect { get { return new Rect(960, 570, 180, 60); } }
    public virtual Rect labelRect { get { return new Rect(770, 450, 380, 130); } }

    void Start()
    {
        guiAlpha = 0f;
    }

    // Update is called once per frame
    void OnGUI ()
    {
		if(guiAlpha > 0f)
        {
            GUI.skin = skin;
            GUIHelper.SetGUIAlpha(guiAlpha);
            GUI.matrix = GUIHelper.GetMatrix();
            GUI.depth = -100;

            Rect actualBoxRect = GUIHelper.CentreRect(boxRect, guiAlpha);
            GUIShape.RoundedRectangle(actualBoxRect, 20, new Color(52 /255f, 152 / 255f, 219 / 255f, guiAlpha * 0.8f));

            GUIStyle adjustedLabel = new GUIStyle(skin.label);
            adjustedLabel.fontSize = (int)(adjustedLabel.fontSize * guiAlpha);

            //Instructions
            GUI.Label(GUIHelper.RectScaledbyOtherRect(labelRect, boxRect, guiAlpha), text, adjustedLabel);

            if (showYesNo)
            {          
                adjustedLabel.normal.textColor = noSelected ? Color.white : Color.yellow;
                Rect actualYes = GUIHelper.RectScaledbyOtherRect(yesRect, boxRect, guiAlpha);
                GUI.Label(actualYes, "Yes", adjustedLabel);

                adjustedLabel.normal.textColor = !noSelected ? Color.white : Color.yellow;
                Rect actualNo = GUIHelper.RectScaledbyOtherRect(noRect, boxRect, guiAlpha);
                GUI.Label(actualNo, "No", adjustedLabel);

                if (GUI.Button(actualYes, ""))
                {
                    noSelected = false;
                    DoInput();
                }

                if (GUI.Button(actualNo, ""))
                {
                    noSelected = true;
                    DoInput();
                }

                if (Cursor.visible && actualYes.Contains(GUIHelper.GetMousePosition()))
                    noSelected = false;

                if (Cursor.visible && actualNo.Contains(GUIHelper.GetMousePosition()))
                    noSelected = true;
            }
            else
            {
                if (Cursor.visible && GUI.Button(boxRect, ""))
                    HidePopUp();
            }

        }
	}

    void Update()
    {
        if(guiAlpha == 1f)
        {
            if (InputManager.controllers.Count > 0)
            {
                bool submit = InputManager.controllers[0].GetButtonWithLock("Submit");
                bool horizontal = false;

                if(showYesNo)
                    horizontal = InputManager.controllers[0].GetRawInputWithLock("MenuHorizontal") != 0;

                if (submit)
                {
                    if (showYesNo)
                        DoInput();
                    else
                        HidePopUp();
                }

                if (horizontal)
                    noSelected = !noSelected;
            }
        }
    }

    protected virtual void DoInput()
    {
        SaveDataManager saveDataManager = FindObjectOfType<SaveDataManager>();
        saveDataManager.ResetSave();

        HidePopUp();
    }

    public void ShowPopUp()
    {
        noSelected = true;
        StartCoroutine(ActualShow());
    }

    public virtual void HidePopUp()
    {
        StartCoroutine(ActualHide());
    }

    public IEnumerator ActualShow()
    {
        yield return FadeTo(1.1f, 0.4f);
        yield return FadeTo(1f, 0.1f);
    }

    public IEnumerator ActualHide()
    {
        yield return FadeTo(1.1f, 0.1f);
        yield return FadeTo(0f, 0.4f);

        FindObjectOfType<Options>().locked = false;
    }

    private IEnumerator FadeTo(float finalVal, float time)
    {
        float startTime = Time.realtimeSinceStartup, startVal = guiAlpha;

        while(Time.realtimeSinceStartup - startTime < time)
        {
            guiAlpha = Mathf.Lerp(startVal, finalVal, (Time.realtimeSinceStartup - startTime) / time);
            yield return null;
        }

        guiAlpha = finalVal;
    }
}
