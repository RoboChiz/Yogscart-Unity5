using UnityEngine;
using System.Collections;

public class Options : MonoBehaviour
{
    CurrentGameData gd;
    SoundManager sm;

    public GUISkin skin;
    private float alpha = 0, xPos;

    // Use this for initialization
    void Start()
    {
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        sm = GameObject.FindObjectOfType<SoundManager>();

        StartCoroutine(ShowMenu());
    }

    // Update is called once per GUI Frame
    void OnGUI()
    {

        GUI.skin = skin;
        GUI.matrix = GUIHelper.GetMatrix();
        GUIHelper.SetGUIAlpha(alpha);

        GUI.BeginGroup(new Rect(0, 0, 1920, 1080));

        GUI.Label(new Rect(xPos, 0, 1920, 1080), "OPTIONS MENU");

        GUI.EndGroup();

    }

    //Show Options Menu
    public IEnumerator ShowMenu()
    {

        float startTime = Time.time, travelTime = 0.5f;

        while (Time.time - startTime < travelTime)
        {
            alpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
            xPos = Mathf.Lerp(-960f, 0f, (Time.time - startTime) / travelTime);
            yield return null;
        }

        alpha = 1f;
        xPos = 0;

    }
	
}
