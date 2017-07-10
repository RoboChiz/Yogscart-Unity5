using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePopup : MonoBehaviour
{
    public float guiAlpha = 0f;
    public GUISkin skin;

    protected bool noSelected = true;

    public virtual string text { get { return "Are you sure you want to reset all save data?"; } }
    public virtual bool showYesNo { get { return true; } }
    public virtual Vector2 size { get { return new Vector2(400, 200); } }

    // Update is called once per frame
    void OnGUI ()
    {
		if(guiAlpha > 0f)
        {
            GUI.skin = skin;
            GUIHelper.SetGUIAlpha(guiAlpha);
            GUI.matrix = GUIHelper.GetMatrix();
            GUI.depth = -100;

            Rect boxRect = GUIHelper.CentreRect(new Rect(960 - (size.x/2f), 540 - (size.y/2f), size.x, size.y), guiAlpha);
            GUIShape.RoundedRectangle(boxRect, 20, new Color(52 /255f, 152 / 255f, 219 / 255f, guiAlpha));
            GUIHelper.CentreRectLabel(new Rect(boxRect.x + 10, boxRect.y + 10, boxRect.width - 20, boxRect.height - 70), guiAlpha, text, Color.white);

            if (showYesNo)
            {
                Rect yesRect = new Rect(boxRect.x + 10, boxRect.y + boxRect.height - 70, (size.x/2f) - 20, 60), noRect = new Rect(boxRect.x + (size.x / 2f) + 20, boxRect.y + boxRect.height - 70, (size.x / 2f) - 20, 60);

                GUIHelper.CentreRectLabel(yesRect, guiAlpha, "Yes", (noSelected) ? Color.white : Color.yellow);
                GUIHelper.CentreRectLabel(noRect, guiAlpha, "No", (noSelected) ? Color.yellow : Color.white);

                if (GUI.Button(yesRect, ""))
                {
                    noSelected = false;
                    DoInput();
                }

                if (GUI.Button(noRect, ""))
                {
                    noSelected = true;
                    DoInput();
                }

                if (Cursor.visible && yesRect.Contains(GUIHelper.GetMousePosition()))
                    noSelected = false;

                if (Cursor.visible && noRect.Contains(GUIHelper.GetMousePosition()))
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
                bool submit = InputManager.controllers[0].GetRawMenuInput("Submit") != 0;
                bool horizontal = false;

                if(showYesNo)
                    horizontal = InputManager.controllers[0].GetRawMenuInput("MenuHorizontal") != 0;

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
        if (!noSelected)
            FindObjectOfType<CurrentGameData>().ResetData();

        HidePopUp();
    }

    public void ShowPopUp()
    {
        noSelected = true;
        StartCoroutine(ActualShow());
    }

    public void HidePopUp()
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
