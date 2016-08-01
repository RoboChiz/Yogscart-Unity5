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
    public static Rect CentreRectLabel(Rect originalRect, float scale, string text, Color colour)
    {
        GUIStyle nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));

        float newFontsize = nstyle.fontSize * scale;
        nstyle.fontSize = (int)newFontsize;
        nstyle.normal.textColor = colour;
        nstyle.alignment = TextAnchor.MiddleCenter;

        Rect newRect = originalRect;
        if (scale != 1f)
            newRect = CentreRect(originalRect, scale);

        GUI.Label(newRect, text, nstyle);

        return newRect;
    }

    public static Rect LeftRect(Rect originalRect, float scale)
    {
        float halfWidth = originalRect.width / 2f, halfHeight = originalRect.height / 2f;
        float centreY = originalRect.y + halfHeight;

        float newY = Mathf.LerpUnclamped(centreY, originalRect.y, scale);
        float newWidth = Mathf.LerpUnclamped(0, originalRect.width, scale);
        float newHeight = Mathf.LerpUnclamped(0, originalRect.height, scale);

        return new Rect(originalRect.x, newY, newWidth, newHeight);
    }
    public static Rect LeftRectLabel(Rect originalRect, float scale, string text, Color colour)
    {
        GUIStyle nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));

        float newFontsize = nstyle.fontSize * scale;
        nstyle.fontSize = (int)newFontsize;
        nstyle.normal.textColor = colour;

        Rect newRect = originalRect;
        if (scale != 1f)
            newRect = LeftRect(originalRect, scale);

        GUI.Label(newRect, text, nstyle);

        return newRect;
    }


    public static Rect Lerp(Rect start, Rect end, float amount)
    {
        return new Rect(Mathf.Lerp(start.x, end.x, amount), Mathf.Lerp(start.y, end.y, amount), Mathf.Lerp(start.width, end.width, amount), Mathf.Lerp(start.height, end.height, amount));
    }

    public static Vector2 GetMousePosition()
    {
        Vector2 newMousePos = Input.mousePosition;
        newMousePos.y = Screen.height - newMousePos.y;
        return GUIUtility.ScreenToGUIPoint(newMousePos);
    }

    private static float nextScale = 1f, backScale = 1f;

    public static bool DrawNext(float guiAlpha)
    {
        var lastMatrix = GUI.matrix;
        GUI.matrix = GetMatrix();
        SetGUIAlpha(guiAlpha);

        bool returnVal = false;

        Rect nextRect = CentreRect(new Rect(1620, 950, 360, 125), nextScale);

        string toDraw = "UI/New Main Menu/nextKey";
        if (InputManager.controllers != null && InputManager.controllers.Count >= 1)
        {
            if (InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
                toDraw = "UI/New Main Menu/nextXbox";
        }

        GUI.DrawTexture(nextRect, Resources.Load<Texture2D>(toDraw));

        if(nextRect.Contains(GetMousePosition()))
        {
            if (nextScale < 1.25f)
                nextScale += Time.deltaTime * 2f;
            else
                nextScale = 1.25f;
        }
        else
        {
            if (nextScale > 1f)
                nextScale -= Time.deltaTime * 2f;
            else
                nextScale = 1f;
        }

        if (guiAlpha >= 1f && GUI.Button(nextRect, ""))
            returnVal = true;

        ResetColor();
        GUI.matrix = lastMatrix;

        return returnVal;
    }

    public static bool DrawBack(float guiAlpha)
    {
        var lastMatrix = GUI.matrix;
        GUI.matrix = GetMatrix();
        SetGUIAlpha(guiAlpha);

        bool returnVal = false;

        Rect backRect = CentreRect(new Rect(-60, 950, 360, 125), backScale);

        string toDraw = "UI/New Main Menu/backKey";
        if(InputManager.controllers != null && InputManager.controllers.Count >= 1)
        {
            if(InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
                toDraw = "UI/New Main Menu/backXbox";
        }

        GUI.DrawTexture(backRect, Resources.Load<Texture2D>(toDraw));

        if (backRect.Contains(GetMousePosition()))
        {
            if (backScale < 1.25f)
                backScale += Time.deltaTime * 2f;
            else
                backScale = 1.25f;
        }
        else
        {
            if (backScale > 1f)
                backScale -= Time.deltaTime * 2f;
            else
                backScale = 1f;
        }

        GUIStyle newButton = new GUIStyle();
        if (guiAlpha >= 1f && GUI.Button(backRect, "", newButton))
            returnVal = true;

        ResetColor();
        GUI.matrix = lastMatrix;

        return returnVal;
    }

}

[System.Serializable]
public class DropDown
{
    public bool toggled { get; private set; }
    private Vector2 scrollPosition;
    private Rect viewRect;
    private float toggleScale;

    public DropDown()
    {
        scrollPosition = new Vector2();
        viewRect = new Rect();
        toggled = false;
        toggleScale = 1f;
    }

    public int Draw(Rect rect, int toggleSize, int value, string[] options)
    {

        //Check value
        value = MathHelper.NumClamp(value, 0, options.Length);
        int returnValue = value;

        Vector2 newMousePos = GUIHelper.GetMousePosition();
        bool staysOutside = true; //Used to tell if mouse is not inside of dropdown

        //Draw current selection
        Rect boxRect = new Rect(rect.x, rect.y, rect.width - toggleSize, rect.height);
        GUI.Box(boxRect, options[value]);

        if (boxRect.Contains(newMousePos))
            staysOutside = false;

        if (GUI.Button(boxRect, ""))
            toggled = !toggled;

        //Draw Toggle      
        Rect toggleRect = GUIHelper.CentreRect(new Rect(rect.x + (rect.width - toggleSize), rect.y + (rect.height - toggleSize) / 2f, toggleSize, toggleSize), toggleScale);

        if(toggleRect.Contains(newMousePos))
        {
            staysOutside = false;

            if (toggleScale < 1.5f)
                toggleScale += Time.deltaTime * 4f;
        }
        else
        {
            if (toggleScale > 1f)
                toggleScale -= Time.deltaTime * 4f;
        }

        if (GUI.Button(toggleRect, Resources.Load<Texture2D>("UI/New Main Menu/Down_Arrow")))
            toggled = !toggled;

        //Draw Options
        if(toggled)
        {
            Rect scrollRect = new Rect(rect.x, rect.y + rect.height, rect.width - (toggleSize / 3.15f), rect.height * Mathf.Clamp(options.Length,0,5));
            GUI.DrawTexture(scrollRect, Resources.Load<Texture2D>("UI/Options/Green"));

            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, new Rect(rect.x, rect.y + rect.height, rect.width - (toggleSize / 2f) - 20, rect.height * options.Length));
         
            for (int i = 0; i < options.Length; i++)
            {
                Rect optionRect = new Rect(rect.x, rect.y + ((i + 1) * rect.height), rect.width - toggleSize, rect.height);
                if (GUI.Button(optionRect, ""))
                {
                    returnValue = i;
                    toggled = false;
                }

                if (staysOutside && optionRect.Contains(newMousePos))
                    staysOutside = false;                   

                GUI.Box(optionRect, options[i]);
            }

            GUI.EndScrollView();

            //Close if clicked outside
            if (staysOutside && Input.GetMouseButton(0))
                toggled = false;
        }

        return returnValue;
    }

}

[System.Serializable]
public class Toggle
{
    private float toggleScale;

    public Toggle()
    {
        toggleScale = 1f;
    }

    public bool Draw(Rect rect, int toggleSize, bool toggled, string label)
    {
        //Draw the label
        Rect labelRect = new Rect(rect.x, rect.y, rect.width - toggleSize, rect.height);
        GUI.Label(labelRect, label);

        //Draw Toggle
        Vector2 newMousePos = GUIHelper.GetMousePosition();
        Rect toggleRect = GUIHelper.CentreRect(new Rect(rect.x + (rect.width - toggleSize), rect.y + (rect.height - toggleSize) / 2f, toggleSize, toggleSize), toggleScale);

        if (toggleRect.Contains(newMousePos))
        {
            if (toggleScale < 1.5f)
                toggleScale += Time.deltaTime * 4f;
        }
        else
        {
            if (toggleScale > 1f)
                toggleScale -= Time.deltaTime * 4f;
        }

        if (GUI.Button(toggleRect, Resources.Load<Texture2D>("UI/New Main Menu/Toggle")))
        {
            toggled = !toggled;
        }

        //Draw Options
        if (toggled)
        {
            GUIHelper.CentreRectLabel(toggleRect,toggleScale, "X", Color.white);
        }

        return toggled;
    }

}