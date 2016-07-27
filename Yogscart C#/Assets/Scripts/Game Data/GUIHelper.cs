using UnityEngine;
using System.Collections;

//GUI Helper V1.0
//Created by Robert (Robo_Chiz)
//Designed to ensure Gui scales correctly for any and all resolutions
//Just call GUI.matrix = GUIHelper.GetMatrix(); at start of GUI
//Then presume resolution is 1920 by 1080

public class GUIHelper
{
    public const float width = 1920.0f;
    public const float height = 1080.0f;
    public static Rect screenEdges;
    public static Rect guiEdges;
    public static bool widthSmaller;

    public static Matrix4x4 GetMatrix()
    {
        float wScale = Screen.width / width;
        float hScale = Screen.height / height;

        float wStart = (Screen.width - (width * hScale)) / 2;
        float hStart = (Screen.height - (height * wScale)) / 2;

        if (Screen.width > (width * hScale))
        {
            screenEdges = new Rect(-wStart / hScale, 0f, Screen.width / hScale, Screen.height / hScale);

            float newWidth = (Screen.height / 1080f) * Screen.width;
            guiEdges = new Rect(wStart, 0f, newWidth, Screen.height);
            widthSmaller = false;

            return Matrix4x4.TRS(new Vector3(wStart, 0, 0), Quaternion.identity, new Vector3(hScale, hScale, 1));
        }
        else
        {
            screenEdges = new Rect(0f, -hStart / wScale, Screen.width / wScale, Screen.height / wScale);

            float newHeight = (Screen.width / 1920f) * Screen.height;
            guiEdges = new Rect(0f, hStart, Screen.width, newHeight);
            widthSmaller = true;

            return Matrix4x4.TRS(new Vector3(0, hStart, 0), Quaternion.identity, new Vector3(wScale, wScale, 1));
        }
    }

    public static void OutLineLabel(Rect pos, string text, float distance, Color Colour)
    {
        distance = Mathf.Clamp(distance, 1, Mathf.Infinity);

        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
        style.normal.textColor = Colour;

        //Right,Top, Left, Bottom
        GUI.Label(new Rect(pos.x + distance, pos.y, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x, pos.y + distance, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x - distance, pos.y, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x, pos.y - distance, pos.width, pos.height), text, style);

        //Top Right, Top Left, Bottom Right, Bottom Left
        GUI.Label(new Rect(pos.x + distance, pos.y + distance, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x - distance, pos.y + distance, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x + distance, pos.y - distance, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x - distance, pos.y - distance, pos.width, pos.height), text, style);

        GUIStyle nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        GUI.Label(pos, text, nstyle);

    }

    public static void OutLineLabel(Rect pos, string text, float distance)
    {
        OutLineLabel(pos, text, distance, Color.black);
    }

    public static void ResetColor()
    {
        GUI.color = Color.white;
    }

    public static void SetGUIAlpha(float alpha)
    {
        Color nWhite = Color.white;
        nWhite.a = alpha;
        GUI.color = nWhite;
    }

    public static Rect CentreRect(Rect originalRect, float scale)
    {
        float halfWidth = originalRect.width / 2f, halfHeight = originalRect.height / 2f;
        float centreX = originalRect.x + halfWidth, centreY = originalRect.y + halfHeight;

        float newX = Mathf.LerpUnclamped(centreX, originalRect.x, scale);
        float newY = Mathf.LerpUnclamped(centreY, originalRect.y, scale);
        float newWidth = Mathf.LerpUnclamped(0, originalRect.width, scale);
        float newHeight = Mathf.LerpUnclamped(0, originalRect.height, scale);

        return new Rect(newX, newY, newWidth, newHeight);
    }

    public static Rect Lerp(Rect start, Rect end, float amount)
    {
        return new Rect(Mathf.Lerp(start.x, end.x, amount), Mathf.Lerp(start.y, end.y, amount), Mathf.Lerp(start.width, end.width, amount), Mathf.Lerp(start.height, end.height, amount));
    }

}