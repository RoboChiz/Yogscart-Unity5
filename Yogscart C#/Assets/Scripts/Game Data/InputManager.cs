using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Input Manager V2
//Created by Robo_Chiz
//Currently only supports Xboxs Controllers...

public class InputManager : MonoBehaviour
{

    static bool ConfigLoaded = false;

    //Checks that Configs have been loaded before being used
    static public List<InputLayout> AllConfigs
    {
        get
        {
            if (!ConfigLoaded)
            {
                LoadConfig();
                ConfigLoaded = true;
            }
            return allConfigs;
        }
        set { }
    }
    static private List<InputLayout> allConfigs;

    static public bool allowedToChange = true;

    static public List<InputController> controllers = new List<InputController>();
    static private bool mouseLock = false;

    public List<float> iconHeights = new List<float>();

    //Loads Saved Input Configurations
    public static void LoadConfig()
    {

        //Load default Input
        allConfigs = new List<InputLayout>();
        allConfigs.Add(new InputLayout("Default,Keyboard,Throttle:w,Brake:s,Steer:d,Steer:a,Drift:space,Item:e,RearView:q,Pause:escape,Submit:return,Cancel:escape,MenuHorizontal:d,MenuHorizontal:a,MenuVertical:s,MenuVertical:w,Rotate:e,Rotate:q,TabChange:e,TabChange:q"));
        allConfigs.Add(new InputLayout("Default,Xbox360,Throttle:A,Brake:B,Steer:L_XAxis+,Steer:L_XAxis-,Drift:TriggerL,Drift:TriggerR,Item:LB,Item:RB,RearView:X,Pause:Start,Submit:Start,Submit:A,Cancel:B,MenuHorizontal:L_XAxis,MenuVertical:L_YAxis,Rotate:R_XAxis,TabChange:RB,TabChange:LB"));        

        bool saveNeeded = false;

        string allConfigString = PlayerPrefs.GetString("SavedConfigs", "");
        string[] configStrings = allConfigString.Split(";"[0]);

        foreach (string s in configStrings)
        {
            InputLayout n = new InputLayout();
            if (n.LoadInput(s))
                allConfigs.Add(n);
            else
                saveNeeded = true;
        }

        if (saveNeeded)
            SaveConfig();

    }

    //Saves Current Input Configurations
    public static void SaveConfig()
    {
        string saveString = "";

        for (int i = 2; i < allConfigs.Count; i++)
        {
            saveString += allConfigs[i].ToString() + ";";
        }

        //Remove last ;
        if (allConfigs.Count > 2)
            saveString.Remove(saveString.Length - 2);
        else if (allConfigs.Count == 0)
            Debug.Log("NO CONFIGS AT ALL!!! AHHHH!!! RUN!!!!");

        PlayerPrefs.SetString("SavedConfigs", saveString);
    }

    //Add a new Controller to the Input Manager
    public void AddController(string input)
    {
        //Check that the controller isn't already in
        bool alreadyIn = false;
        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i].controllerName == input)
            {
                alreadyIn = true;
                break;
            }
        }

        //Add the Controller
        if (!alreadyIn)
        {
            controllers.Add(new InputController(input));
            StartCoroutine(ShowInput(controllers.Count - 1));
            Debug.Log("Added " + input + " controllers:" + controllers.Count);
        }
    }

    //Removes a Controller from the Input Manager
    public void RemoveController(string input)
    {
        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i].controllerName == input)
            {
                controllers.RemoveAt(i);
                StartCoroutine(ShowInput(i));
                break;
            }
        }
    }

    //Removes all but one Controller from the Input Manager
    public void RemoveOtherControllers()
    {
        for (int i = 1; i < controllers.Count; i++)
        {
            controllers.RemoveAt(i);
            StartCoroutine(ShowInput(i));
        }
    }

    void Update()
    {
        if (controllers == null)
            controllers = new List<InputController>();

        if (controllers.Count > 0)
        {
            foreach (InputController c in controllers)
            {
                if (c.buttonLock != "" && c.GetRawInput(c.buttonLock) == 0)
                    c.buttonLock = "";
            }
        }

        //Reset Mouse Lock
        if (Input.GetMouseButtonUp(0))
            mouseLock = false;

        //Look for new Controllers
        if (allowedToChange)
        {
            if (controllers.Count < 4)
            {
                if (Input.GetKey("return") || (Input.GetMouseButton(0) && controllers.Count == 0))
                    AddController("Key_");

                if (Input.GetAxis("Start_1") != 0)
                    AddController("_1");

                if (Input.GetAxis("Start_2") != 0)
                    AddController("_2");

                if (Input.GetAxis("Start_3") != 0)
                    AddController("_3");

                if (Input.GetAxis("Start_4") != 0)
                    AddController("_4");
            }

            if (controllers.Count >= 1)
            {
                if (Input.GetKey("backspace"))
                    RemoveController("Key_");

                if (Input.GetAxis("Back_1") != 0)
                    RemoveController("_1");

                if (Input.GetAxis("Back_2") != 0)
                    RemoveController("_2");

                if (Input.GetAxis("Back_3") != 0)
                    RemoveController("_3");

                if (Input.GetAxis("Back_4") != 0)
                    RemoveController("_4");
            }
        }
    }

    void OnGUI()
    {
        float iconSize = Screen.height / 6f;
        Texture2D icon;

        while (controllers.Count > iconHeights.Count)
            iconHeights.Add(Screen.height);

        for (int i = 0; i < iconHeights.Count; i++)
        {
            if (controllers.Count > i && controllers[i] != null)
            {
                if(controllers[i].controlLayout.Type == ControllerType.Xbox360)
                    icon = Resources.Load<Texture2D>("UI/Controls/Xbox" + controllers[i].controllerName);
                else
                    icon = Resources.Load<Texture2D>("UI/Controls/Keyboard");
            }
            else
                icon = Resources.Load<Texture2D>("UI/Controls/Gone");

            GUI.Box(new Rect((i * (iconSize + 10)) + 10, Screen.height - iconHeights[i], iconSize, iconSize), icon);
        }
    }

    IEnumerator ShowInput(int toShow)
    {        
        float iconSize = Screen.height / 6f;
        float startTime = Time.time;
        float travelTime = 0.5f;

        //Slide UP //////////////////////////////
        while (toShow >= iconHeights.Count)
            iconHeights.Add(0f);

        //Don't slide if already moving
        if (iconHeights[toShow] == 0f)
        {
            while (Time.time - startTime <= travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(0f, iconSize, (Time.time - startTime) / travelTime);
                yield return null;
            }

            iconHeights[toShow] = iconSize;
            yield return new WaitForSeconds(travelTime);

            startTime = Time.time;
            while (Time.time - startTime <= travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(iconSize, 0f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            iconHeights[toShow] = 0f;
        }
    }

    public static bool MouseIntersects(Rect area)
    {
        if (Input.mousePosition.x >= area.x && Input.mousePosition.x <= area.x + area.width
    && Screen.height - Input.mousePosition.y >= area.y && Screen.height - Input.mousePosition.y <= area.y + area.height)
            return true;
        else
            return false;
    }

    public static bool GetClick()
    {
        if (!mouseLock && Input.GetMouseButtonDown(0))
        {
            mouseLock = true;
            return true;
        }

        return false;
    }

}

public class InputController
{
    public string controllerName;
    public InputLayout controlLayout;

    public string buttonLock;

    public InputController(string inputName)
    {
        controllerName = inputName;

        if(inputName == "Key_")
            controlLayout = InputManager.AllConfigs[0];
        else
            controlLayout = InputManager.AllConfigs[1];

        buttonLock = "Submit";
    }

    public float GetInput(string axis)
    {
        float value = 0f;

        string inputAxisOne = "", inputAxisTwo = "";
        bool HasInputOne = controlLayout.commandsOne.TryGetValue(axis, out inputAxisOne);
        bool HasInputTwo = controlLayout.commandsTwo.TryGetValue(axis, out inputAxisTwo);

        if (!HasInputOne && !HasInputTwo)
        {
            Debug.LogError(axis + " is not an Axis!");
        }
        else
        {
            if (inputAxisOne != null)
            {
                if (controlLayout.Type == ControllerType.Xbox360)
                {
                    int requiredSignPlus = 0;
                    //Check for + or - Sign
                    if (inputAxisOne[inputAxisOne.Length - 1] == '+')
                        requiredSignPlus = 1;
                    if (inputAxisOne[inputAxisOne.Length - 1] == '-')
                        requiredSignPlus = -1;

                    if (requiredSignPlus != 0)
                        inputAxisOne = inputAxisOne.Remove(inputAxisOne.Length - 1);

                    value = Input.GetAxis(inputAxisOne + controllerName);

                    //Reset value if it is not the correct sign
                    if (requiredSignPlus > 0 && value < 0)
                        value = 0;
                    if (requiredSignPlus < 0 && value > 0)
                        value = 0;
                }
                else
                {
                    if (Input.GetKey(inputAxisOne))
                        value = 1;
                    else
                        value = 0;
                }
                    
            }
            if ((inputAxisTwo != null) && value == 0)
            {
                if (controlLayout.Type == ControllerType.Xbox360)
                {
                    int requiredSignMinus = 0;
                    //Check for + or - Sign
                    if (inputAxisTwo[inputAxisTwo.Length - 1] == '+')
                        requiredSignMinus = 1;
                    if (inputAxisTwo[inputAxisTwo.Length - 1] == '-')
                        requiredSignMinus = -1;

                    if (requiredSignMinus != 0)
                        inputAxisTwo = inputAxisTwo.Remove(inputAxisTwo.Length - 1);

                    value = -Mathf.Abs(Input.GetAxis(inputAxisTwo + controllerName));

                    //Reset value if it is not the correct sign
                    if (requiredSignMinus > 0 && value < 0)
                        value = 0;
                    if (requiredSignMinus < 0 && value > 0)
                        value = 0;
                }
                else
                {
                    if (Input.GetKey(inputAxisTwo))
                        value -= 1;
                }
            }

        }

        return value;
    }

    public float GetRawInput(string axis)
    {
        float value = 0f;

        string inputAxisOne = "", inputAxisTwo = "";
        bool HasInputOne = controlLayout.commandsOne.TryGetValue(axis, out inputAxisOne);
        bool HasInputTwo = controlLayout.commandsTwo.TryGetValue(axis, out inputAxisTwo);

        if (!HasInputOne && !HasInputTwo)
        {
            Debug.LogError(axis + " is not an Axis!");
        }
        else
        {
            if (inputAxisOne != null)
            {
                if (controlLayout.Type == ControllerType.Xbox360)
                {
                    int requiredSign = 0;
                    //Check for + or - Sign
                    if (inputAxisOne[inputAxisOne.Length - 1] == '+')
                        requiredSign = 1;
                    if (inputAxisOne[inputAxisOne.Length - 1] == '-')
                        requiredSign = -1;

                    if (requiredSign != 0)
                        inputAxisOne = inputAxisOne.Remove(inputAxisOne.Length - 1);

                    value = Input.GetAxisRaw(inputAxisOne + controllerName);

                    //Reset value if it is not the correct sign
                    if (requiredSign > 0 && value <= 0)
                        value = 0;
                    if (requiredSign < 0 && value >= 0)
                        value = 0;
                }
                else
                {
                    if (Input.GetKey(inputAxisOne))
                        value = 1;
                    else
                        value = 0;
                }

            }
            if ((inputAxisTwo != null) && value == 0)
            {
                if (controlLayout.Type == ControllerType.Xbox360)
                {
                    int requiredSign = 0;
                    //Check for + or - Sign
                    if (inputAxisTwo[inputAxisTwo.Length - 1] == '+')
                        requiredSign = 1;
                    if (inputAxisTwo[inputAxisTwo.Length - 1] == '-')
                        requiredSign = -1;

                    if (requiredSign != 0)
                        inputAxisTwo = inputAxisTwo.Remove(inputAxisTwo.Length - 1);

                    value = -Mathf.Abs(Input.GetAxisRaw(inputAxisTwo + controllerName));

                    //Reset value if it is not the correct sign
                    if (requiredSign > 0 && value <= 0)
                        value = 0;
                    if (requiredSign < 0 && value >= 0)
                        value = 0;
                }
                else
                {
                    if (Input.GetKey(inputAxisTwo))
                        value -= 1;
                }
            }

        }
        return value;
    }

    public int GetMenuInput(string axis)
    {
        float returnValue = GetInput(axis);

        if (buttonLock == "" && returnValue != 0)
        {
            buttonLock = axis;
            return (int)Mathf.Sign(returnValue);
        }
        else
        {
            return 0;
        }

    }

    public int GetRawMenuInput(string axis)
    {
        float returnValue = GetRawInput(axis);

        if (buttonLock == "" && returnValue != 0)
        {
            buttonLock = axis;
            return (int)Mathf.Sign(returnValue);
        }
        else
        {
            return 0;
        }

    }
}

public enum ControllerType { Xbox360, Keyboard };
//Stores the User's Input Config
public class InputLayout
{

    public InputLayout()
    {
        commandsOne = new Dictionary<string, string>();
        commandsTwo = new Dictionary<string, string>();
    }

    public InputLayout(string newInput)
    {
        LoadInput(newInput);
    }

    private string name = "Input Layout";
    public string Name
    {
        get { return name; }
        set
        {
            if (GUIHelper.CheckString(value, 9))
                name = value;
        }
    }

    private ControllerType type = ControllerType.Xbox360;
    public ControllerType Type
    {
        get { return type; }
        set { type = value; }
    }

    public Dictionary<string, string> commandsOne;
    public Dictionary<string, string> commandsTwo;

    /*Options that are not changeable: Everything else
    Steering
    Menu Navigation
    Rotation in Character Select
    Pausing
    */

    //Loads the InputLayout from a string where each input is seperated by a ','
    public bool LoadInput(string contents)
    {
        string[] splitString = contents.Split(","[0]);

        List<string> validCommands = new List<string>() { "Throttle", "Brake", "Steer", "Drift", "Item", "RearView", "Pause", "Submit", "Cancel", "MenuHorizontal", "MenuVertical", "Rotate", "TabChange" };
        commandsOne = new Dictionary<string, string>();
        commandsTwo = new Dictionary<string, string>();

        if (splitString.Length >= 2) //Must have 2
        {
            name = splitString[0];

            if (splitString[1] == "Xbox360")
            {
                type = ControllerType.Xbox360;
            }
            else
            {
                type = ControllerType.Keyboard;
            }

            for (int i = 2; i < splitString.Length; i++)
            {
                string[] inputSplit = splitString[i].Split(":"[0]);

                if (inputSplit.Length == 2 && validCommands.Contains(inputSplit[0]))
                {
                    if (!commandsOne.ContainsKey(inputSplit[0]))
                    {
                        commandsOne.Add(inputSplit[0], inputSplit[1]);
                    }
                    else if (!commandsTwo.ContainsKey(inputSplit[0]))
                    {
                        commandsTwo.Add(inputSplit[0], inputSplit[1]);
                    }
                    else
                        return CancelLoad();
                }
                else
                    return CancelLoad();
            }

            return true;
        }

        //If all else fails
        return CancelLoad();
    }

    public bool CancelLoad()
    {
        commandsOne = new Dictionary<string, string>();
        commandsTwo = new Dictionary<string, string>();
        return false;
    }

    //Used for Saving Layout
    public override string ToString()
    {
        string returnString = name + "," + type;

        foreach (KeyValuePair<string, string> kv in commandsOne)
            returnString += "," + kv.Key + ":" + kv.Value;
        foreach (KeyValuePair<string, string> kv in commandsTwo)
            returnString += "," + kv.Key + ":" + kv.Value;
        //Debug.Log("returnString:" + returnString);
        return returnString;

    }
}
