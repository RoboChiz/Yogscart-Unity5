using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    private static float lastMouseTime;
    private static Vector2 lastMousePos;

    public static Vector2 groupOffset;
    private static List<Vector2> offsets;

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
        float halfHeight = originalRect.height / 2f;
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
        //Only perform calculation once per frame
        if (lastMouseTime != Time.unscaledDeltaTime)
        {         
            Vector2 newMousePos = Input.mousePosition;
            newMousePos.y = Screen.height - newMousePos.y;
            lastMousePos = GUIUtility.ScreenToGUIPoint(newMousePos);

            lastMouseTime = Time.unscaledDeltaTime;
        }

        return lastMousePos;
    }

    private static float nextScale = 1f, backScale = 1f;

    public static bool DrawNext(float guiAlpha)
    {
        var lastMatrix = GUI.matrix;
        GUI.matrix = GetMatrix();
        SetGUIAlpha(guiAlpha);

        bool returnVal = false;

        Rect nextRect = CentreRect(new Rect(1682, 950, 288, 100), nextScale);

        string toDraw = "UI/New Main Menu/nextKey";
        if (InputManager.controllers != null && InputManager.controllers.Count >= 1)
        {
            if (InputManager.controllers[0].inputType == InputType.Xbox360)
                toDraw = "UI/New Main Menu/nextXbox";
        }

        GUI.DrawTexture(nextRect, Resources.Load<Texture2D>(toDraw));
        nextScale = SizeHover(nextRect, nextScale, 1f, 1.25f, 2f);

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

        Rect backRect = CentreRect(new Rect(-50, 950, 288, 100), backScale);

        string toDraw = "UI/New Main Menu/backKey";
        if(InputManager.controllers != null && InputManager.controllers.Count >= 1)
        {
            if(InputManager.controllers[0].inputType == InputType.Xbox360)
                toDraw = "UI/New Main Menu/backXbox";
        }

        GUI.DrawTexture(backRect, Resources.Load<Texture2D>(toDraw));
        backScale = SizeHover(backRect, backScale, 1f, 1.25f, 2f);

        GUIStyle newButton = new GUIStyle();
        if (guiAlpha >= 1f && GUI.Button(backRect, "", newButton))
            returnVal = true;

        ResetColor();
        GUI.matrix = lastMatrix;

        return returnVal;
    }

    public static void BeginGroup(Rect position)
    {
        if (offsets == null)
            offsets = new List<Vector2>();

        Vector2 nOffset = new Vector2(position.x, position.y);
        offsets.Add(nOffset);

        groupOffset += nOffset;

        GUI.BeginGroup(position);
    }
    public static void EndGroup()
    {
        groupOffset -= offsets[offsets.Count - 1];
        offsets.RemoveAt(offsets.Count - 1);

        GUI.EndGroup();
    }
    public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect)
    {
        if (offsets == null)
            offsets = new List<Vector2>();

        Vector2 nOffset = new Vector2(position.x - scrollPosition.x, position.y - scrollPosition.y);
        offsets.Add(nOffset);
        groupOffset += nOffset;

        return GUI.BeginScrollView(position,scrollPosition,viewRect);
    }
    public static void EndScrollView()
    {
        groupOffset -= offsets[offsets.Count - 1];
        offsets.RemoveAt(offsets.Count - 1);

        GUI.EndScrollView();
    }

    public static float SizeHover(Rect rect,float value, float min, float max, float timeScale)
    {
        Vector2 mousePos = GetMousePosition();

        rect.x += groupOffset.x;
        rect.y += groupOffset.y;

        if(Cursor.visible && rect.Contains(mousePos))
        {
            if (value < max)
                value += Time.deltaTime * timeScale;
            else
                value = max;
        }
        else
        {
            if (value > min)
                value -= Time.deltaTime * timeScale;
            else
                value = min;
        }

        return value;
    }

    public static bool CheckString(string checkString, int maxLength)
    {
        bool nReturn = true;

        if (maxLength > 0 && checkString.Length > maxLength)
            nReturn = false;

        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_0123456789.# ";

        for (int i = 0; i < checkString.Length; i++)
        {
            if (!letters.Contains(checkString[i].ToString()))
            {
                nReturn = false;
                break;
            }
        }

        return nReturn;
    }

    public static Rect RectScaledbyOtherRect(Rect original, Rect originalParent, float scale)
    {
        float xRelative      = (original.x - originalParent.x) * scale, 
              yRelative      = (original.y - originalParent.y) * scale,
              widthRelative  = original.width                  * scale,
              heightRelative = original.height                 * scale;


        Rect parentNow = CentreRect(originalParent, scale);

        return new Rect(parentNow.x + xRelative, parentNow.y + yRelative, widthRelative, heightRelative);
    }

}

[System.Serializable]
public class DropDown
{
    public bool toggled;
    private Vector2 scrollPosition;
    private float toggleScale, optionHeight;

    public DropDown()
    {
        scrollPosition = new Vector2();
        toggled = false;
        toggleScale = 1f;
    }

    public void SetScroll(int value)
    {
        float diff = (value * optionHeight) - scrollPosition.y;
        if (Mathf.Abs(diff) > 1f)
            scrollPosition.y += diff * Time.deltaTime * 2f;
    }

    public int Draw(Rect rect,Vector2 adjuster, int toggleSize, int value, string[] options)
    {
        optionHeight = rect.height;

        //Check value
        value = MathHelper.NumClamp(value, 0, options.Length);
        int returnValue = value;

        Vector2 newMousePos = GUIHelper.GetMousePosition();
        bool staysOutside = true; //Used to tell if mouse is not inside of dropdown

        //Draw current selection
        Rect boxRect = new Rect(rect.x, rect.y, rect.width - toggleSize, rect.height);
        GUI.Box(boxRect, options[value]);
        if (GUI.Button(boxRect, ""))
            toggled = !toggled;

        boxRect.x += adjuster.x;
        boxRect.y += adjuster.y;

        if (boxRect.Contains(newMousePos))
            staysOutside = false;

        //Draw Toggle      
        Rect toggleRect = GUIHelper.CentreRect(new Rect(rect.x + (rect.width - toggleSize), rect.y + (rect.height - toggleSize) / 2f, toggleSize, toggleSize), toggleScale);
        if (GUI.Button(toggleRect, Resources.Load<Texture2D>("UI/New Main Menu/Down_Arrow")))
            toggled = !toggled;

        toggleRect.x += adjuster.x;
        toggleRect.y += adjuster.y;

        if (toggleRect.Contains(newMousePos))
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

        //Draw Options
        if (toggled)
        {
            Rect scrollRect = new Rect(rect.x, rect.y + rect.height, rect.width - (toggleSize / 3.15f), rect.height * Mathf.Clamp(options.Length, 0, 5));
            GUI.DrawTexture(scrollRect, Resources.Load<Texture2D>("UI/Options/Green"));

            if (scrollRect.Contains(newMousePos - GUIHelper.groupOffset))
            {
                staysOutside = false;
                //Debug.Log("Stays Outside!");
            }              

            scrollPosition = GUIHelper.BeginScrollView(scrollRect, scrollPosition, new Rect(rect.x, rect.y + rect.height, rect.width - (toggleSize / 2f) - 20, rect.height * options.Length));

            for (int i = 0; i < options.Length; i++)
            {
                Rect optionRect = new Rect(rect.x, rect.y + ((i + 1) * rect.height), rect.width - toggleSize, rect.height);
                if (GUI.Button(optionRect, ""))
                {
                    returnValue = i;
                    toggled = false;
                }

                GUI.Box(optionRect, options[i]);
            }

            GUIHelper.EndScrollView();
        }

        //Close if clicked outside
        if (staysOutside && Input.GetMouseButtonDown(0))
            toggled = false;

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

    public bool Draw(Rect rect, Vector2 adjuster, int toggleSize, bool toggled, string label)
    {
        return Draw(rect, adjuster, toggleSize, toggled, label, GUI.skin.label);
    }

    public bool Draw(Rect rect, Vector2 adjuster, int toggleSize, bool toggled, string label, GUIStyle style)
    {
        GUIStyle labelStyle = new GUIStyle(style);
        labelStyle.alignment = TextAnchor.MiddleLeft;

        //Draw the label
        Rect labelRect = new Rect(rect.x, rect.y, rect.width - toggleSize, rect.height);
        GUI.Label(labelRect, label, labelStyle);

        //Draw Toggle
        Rect toggleRect = GUIHelper.CentreRect(new Rect(rect.x + (rect.width - toggleSize), rect.y + (rect.height - toggleSize) / 2f, toggleSize, toggleSize), toggleScale);      

        if (GUI.Button(toggleRect, Resources.Load<Texture2D>("UI/New Main Menu/Toggle")))
        {
            toggled = !toggled;
        }

        //Draw Options
        if (toggled)
        {
            GUIHelper.CentreRectLabel(toggleRect,toggleScale, "X", Color.white);
        }

        toggleRect.x += adjuster.x;
        toggleRect.y += adjuster.y;

        toggleScale = GUIHelper.SizeHover(toggleRect, toggleScale, 1f, 1.5f, 4f);

        return toggled;
    }

}

public class GUIKeyboard
{
    public Rect drawRect;
    private Rect lastRect;
    public float guiAlpha = 1f;

    private Vector2 currentSelection;
    private bool upperCase, capsLockUpperCase;

    private float buttonSize = 0f;

    readonly static int[] lettersPerRow = new int[] {11, 10, 9, 9, 2 };
    const int maxLetters = 11;

    private float[] xOffsets;
    private float yOffset;
    private float xBeforeSpace = -1;

    const string lettersInOrder = "1234567890<qwertyuiopasdfghjkl^zxcvbnm_ /";
    const float iconSpacing = 10f;

    public bool completed, buttonlock;

    public GUIKeyboard(Rect _drawRect)
    {
        drawRect = _drawRect;
        currentSelection = Vector2.zero;

        ResetValues();
    }

    public void ResetValues()
    {
        buttonSize = Mathf.Min((drawRect.width - (iconSpacing * maxLetters)) / maxLetters, (drawRect.height - (iconSpacing * lettersPerRow.Length)) / lettersPerRow.Length);

        xOffsets = new float[lettersPerRow.Length];

        for (int i = 0; i < lettersPerRow.Length; i++)
        {
            xOffsets[i] = (drawRect.width / 2f) - ((iconSpacing + ((buttonSize + iconSpacing) * lettersPerRow[i])) / 2f);

            if (i == 0)
                xOffsets[0] -= buttonSize/2f;
        }

        yOffset = (drawRect.height - (iconSpacing + ((buttonSize + iconSpacing) * lettersPerRow.Length)))/2f;
    }

    public string Draw(string originalString, int maxLetters, float scale, bool useController, bool submit, bool cancel, float verticalInput, float horizontalInput)
    {
        bool showSelected = !Cursor.visible && useController;

        //Resize constants if needed
        if(lastRect.x != drawRect.x || lastRect.y != drawRect.y || lastRect.width != drawRect.width || lastRect.height != drawRect.height)
        {
            lastRect = new Rect(drawRect);
            ResetValues();
        }

        //Get Styles
        GUIStyle normalLabel = new GUIStyle(GUI.skin.label), selectedLabel = new GUIStyle(GUI.skin.label);
        selectedLabel.normal.textColor = Color.yellow;

        normalLabel.fontSize = (int)(normalLabel.fontSize * scale * 0.8f);
        selectedLabel.fontSize = normalLabel.fontSize;

        //Draw Letters
        int x = 0, y = 0, currentVal = -1;
        for(int i = 0; i < lettersInOrder.Length; i++)
        {
            Rect buttonRect = new Rect(xOffsets[y] + drawRect.x + iconSpacing + (x * (buttonSize + iconSpacing)), yOffset + drawRect.y + iconSpacing + (y * (buttonSize + iconSpacing)), buttonSize, buttonSize);
            
            if(i == lettersInOrder.Length - 2)
            {
                buttonRect.width = buttonSize * 6f;
                buttonRect.x -= buttonSize * 3f;
            }

            if (i == lettersInOrder.Length - 1)
            {
                buttonRect.x += buttonSize * 2f;
                buttonRect.width = buttonSize * 2f;
            }


            if (i == 10)
            {
                buttonRect.width = buttonSize * 2f;
            }

            Rect actualButton = GUIHelper.RectScaledbyOtherRect(buttonRect, drawRect, 1f);
            GUIShape.RoundedRectangle(actualButton, 10, new Color(1f, 1f, 1f, 0.2f));

            string letterString = lettersInOrder[i].ToString();

            if (letterString == " ")
                letterString = "Space";
            else if (letterString == "<")
                letterString = "Delete";
            else if (letterString == "/")
                letterString = "Enter";
            else if (letterString != "^")
                letterString = upperCase ? letterString.ToUpper() : letterString.ToLower();

            if (!completed)
            {
                if (Cursor.visible && actualButton.Contains(GUIHelper.GetMousePosition()))
                {
                    showSelected = true;
                    currentSelection = new Vector2(x, y);
                }
            }

            GUI.Label(actualButton, letterString, (showSelected && x == currentSelection.x && y == currentSelection.y) ? selectedLabel : normalLabel);

            if (showSelected && x == currentSelection.x && y == currentSelection.y)
                currentVal = i;

            if(!completed)
            {
                if (Cursor.visible && GUI.Button(actualButton, ""))
                {
                    currentSelection = new Vector2(x, y);
                    currentVal = i;
                    DoInput(currentVal, ref originalString);
                }
            }

            //Increment position 
            x++;

            if(x >= lettersPerRow[y])
            {
                x = 0;
                y++;
            }
        }

        //Do Inputs
        if (!completed && guiAlpha == 1f)
        {
            if (useController)
            {
                //Controller Controls
                if (submit && currentVal >= 0)
                {
                    DoInput(currentVal, ref originalString);
                }

                if (cancel && originalString.Length >= 1)
                {
                    originalString = originalString.Remove(originalString.Length - 1);
                }

                if (horizontalInput != 0)
                    currentSelection.x = MathHelper.NumClamp(currentSelection.x + horizontalInput, 0, lettersPerRow[(int)currentSelection.y]);

                if (verticalInput != 0)
                {
                    currentSelection.y = MathHelper.NumClamp(currentSelection.y + verticalInput, 0, lettersPerRow.Length);

                    //If hitting space bar, remember where we were
                    if (currentSelection.y == lettersPerRow.Length - 1)
                    {
                        xBeforeSpace = currentSelection.x;
                    }
                    else if (xBeforeSpace != -1)
                    {
                        currentSelection.x = xBeforeSpace;
                        xBeforeSpace = -1;
                    }

                    //Make sure impossible keys aren't selected
                    while (currentSelection.x >= lettersPerRow[(int)currentSelection.y])
                        currentSelection.x--;
                }
            }
            else
            {
                bool inputDetected = false;

                //Keyboard Controls
                foreach (char character in lettersInOrder)
                {
                    if (character == '^' || character == '<' || character == '/' || character == ' ' || character == '_')
                    {
                        //Duff, Don't do anything
                    }
                    else if (Input.GetKey(character.ToString()))
                    {
                        inputDetected = true;
                        if (!buttonlock)
                        {
                            originalString += upperCase ? character.ToString().ToUpper() : character.ToString().ToLower();
                            buttonlock = true;
                        }
                    }
                }

                if (Input.GetKey(KeyCode.Backspace) && originalString.Length >= 1)
                {
                    inputDetected = true;
                    if (!buttonlock)
                    {
                        originalString = originalString.Remove(originalString.Length - 1);
                        buttonlock = true;
                    }
                }

                if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Escape))
                {
                    inputDetected = true;
                    if (!buttonlock)
                    {
                        if(originalString != "")
                            completed = true;
                        buttonlock = true;
                    }
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    inputDetected = true;
                    if (!buttonlock)
                    {
                        originalString += " ";
                        buttonlock = true;
                    }
                }

                if (Input.GetKey(KeyCode.Minus))
                {
                    inputDetected = true;
                    if (!buttonlock)
                    {
                        originalString += "_";
                        buttonlock = true;
                    }
                }

                if (Input.GetKey(KeyCode.CapsLock))
                {
                    inputDetected = true;
                    if (!buttonlock)
                    {
                        capsLockUpperCase = !capsLockUpperCase;
                        buttonlock = true;
                    }
                }

                if (!inputDetected && buttonlock)
                    buttonlock = false;

                upperCase = capsLockUpperCase;


                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    upperCase = !upperCase;


            }
        }

        if (originalString.Length > maxLetters)
            originalString = originalString.Remove(maxLetters);

        return originalString;
    }

    private void DoInput(int currentVal, ref string originalString)
    {
        if (lettersInOrder[currentVal] == '^')
        {
            upperCase = !upperCase;
        }
        else if (lettersInOrder[currentVal] == '<')
        {
            if (originalString.Length >= 1)
                originalString = originalString.Remove(originalString.Length - 1);
        }
        else if (lettersInOrder[currentVal] == '/' && originalString != "")
        {
            //Complete Keyboard
            completed = true;
        }
        else
        {
            string letterString = lettersInOrder[currentVal].ToString();
            originalString += upperCase ? letterString.ToUpper() : letterString.ToLower();
        }
    }
}