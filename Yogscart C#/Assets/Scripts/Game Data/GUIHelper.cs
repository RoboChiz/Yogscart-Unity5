﻿using UnityEngine;
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

    public static Matrix4x4 GetMatrix()
    {
        float wScale = Screen.width / width;
        float hScale = Screen.height / height;

        float wStart = (Screen.width - (width * hScale)) / 2;
        float hStart = (Screen.height - (height * wScale)) / 2;

        if (Screen.width > (width * hScale))
        {
            return Matrix4x4.TRS(new Vector3(wStart, 0, 0), Quaternion.identity, new Vector3(hScale, hScale, 1));
        }
        else
        {
            return Matrix4x4.TRS(new Vector3(0, hStart, 0), Quaternion.identity, new Vector3(wScale, wScale, 1));
        }
    }

    public static void OutLineLabel(Rect pos, string text, float distance, Color Colour)
    {
        distance = Mathf.Clamp(distance, 1, Mathf.Infinity);

        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
        style.normal.textColor = Colour;

        GUI.Label(new Rect(pos.x + distance, pos.y, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x, pos.y + distance, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x - distance, pos.y, pos.width, pos.height), text, style);
        GUI.Label(new Rect(pos.x, pos.y - distance, pos.width, pos.height), text, style);

        GUIStyle nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        GUI.Label(pos, text, nstyle);

    }


}