using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGamePopup : MonoBehaviour
{
    public float guiAlpha = 0f;
    public GUISkin skin;

    bool noSelected = true;

	// Update is called once per frame
	void OnGUI ()
    {
		if(guiAlpha > 0f)
        {
            GUI.skin = skin;
            GUIHelper.SetGUIAlpha(guiAlpha);
            GUI.matrix = GUIHelper.GetMatrix();

            GUIShape.RoundedRectangle(GUIHelper.CentreRect(new Rect(760, 440, 400, 200), guiAlpha), 20, new Color(52 /255f, 152 / 255f, 219 / 255f, guiAlpha));
            GUIHelper.CentreRectLabel(new Rect(800, 460, 320, 130), guiAlpha, "Are you sure you want to reset all save data?", Color.white);

            GUIHelper.CentreRectLabel(new Rect(40, 120, 150, 30), guiAlpha, "Yes", (noSelected) ? Color.white : Color.yellow);
            GUIHelper.CentreRectLabel(new Rect(200, 120, 150, 30), guiAlpha, "No", (noSelected) ? Color.yellow : Color.white);

        }
	}

    void Update()
    {
        if(guiAlpha == 1f)
        {
            if (InputManager.controllers.Count > 0)
            {
                bool submit = InputManager.controllers[0].GetRawMenuInput("Submit") != 0;
                bool horizontal = InputManager.controllers[0].GetRawMenuInput("MenuHorizontal") != 0;

                if (submit)
                {
                    if (!noSelected)
                        FindObjectOfType<CurrentGameData>().ResetData();

                    HidePopUp();
                }

                if (horizontal)
                    noSelected = !noSelected;
            }
        }
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
