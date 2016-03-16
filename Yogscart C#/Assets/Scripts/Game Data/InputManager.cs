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

    private List<float> iconHeights = new List<float>();

    //Loads Saved Input Configurations
    private static void LoadConfig()
    {

        //Load default Input
        allConfigs = new List<InputLayout>();
        allConfigs.Add(new InputLayout("Default,Xbox360,Throttle:A,Throttle:B,Steer:L_XAxis,Drift:TriggersL,Drift:TriggersR,Item:LB,Item:RB,RearView:X,Pause:Start,Submit:Start,Submit:A,Cancel:B,MenuHorizontal:L_XAxis,MenuVertical:L_YAxis,Rotate:R_XAxis"));

        bool saveNeeded = false;

        string allConfigString = PlayerPrefs.GetString("SavedConfigs", "");
        string[] configStrings = allConfigString.Split(";"[0]);

        foreach(string s in configStrings)
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
    private static void SaveConfig()
    {
        List<InputLayout> toSave = new List<InputLayout>();
        string saveString = "";

        for(int i = 1; i < allConfigs.Count; i++)
        {
            saveString += allConfigs[i].ToString() + ";";
        }

        //Remove last ;
        if (allConfigs.Count > 1)
            saveString.Remove(saveString.Length - 1);
        else if(allConfigs.Count == 0)
            Debug.Log("NO CONFIGS AT ALL!!! AHHHH!!! RUN!!!!");

        PlayerPrefs.SetString("SavedConfigs", saveString);
    }

    //Add a new Controller to the Input Manager
    public void AddController(string input)
    {
        //Check that the controller isn't already in
        bool alreadyIn = false;
        for(int i = 0; i < controllers.Count; i++)
        {
            if(controllers[i].controllerName == input)
            {
                alreadyIn = true;
                break;
            }
        }

        //Add the Controller
        if (!alreadyIn)
        {
            controllers.Add(new InputController(input));
            StartCoroutine("ShowInput",controllers.Count - 1);
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
                StartCoroutine("RemoveInput", i);
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
                StartCoroutine("RemoveInput", i);
        }
    }

    void Update()
    {
        if(controllers != null && controllers.Count > 0)
        {
            foreach(InputController c in controllers)
            {
                if (c.buttonLock != "" && c.GetInput(c.buttonLock) == 0)
                    c.buttonLock = "";
            }
        }

        //Look for new Controllers
        if (allowedToChange)
        {
            if (controllers.Count < 4)
            {
               // if (Input.GetAxis("Key_Submit"))
                  //  AddController("Key_");

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
                // if (Input.GetAxis("KeyBack") != 0)
                //  RemoveController("Key_");

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

        for(int i = 0; i < iconHeights.Count; i++)
        {
            if(controllers.Count > i && controllers[i] != null)
                icon = Resources.Load<Texture2D>("UI/Controls/Xbox" + controllers[i].controllerName);
            else
                icon = Resources.Load<Texture2D>("UI/Controls/Gone");

            GUI.Box(new Rect((i * (iconSize + 10)) + 10, iconHeights[i], iconSize, iconSize), icon);
        }
    }

    IEnumerator ShowInput(int toShow)
    {
        float iconSize = Screen.height / 6f;
        float startTime = Time.time;
        float travelTime = 0.5f;

        //Slide UP //////////////////////////////
        while(toShow >= iconHeights.Count)
            iconHeights.Add(Screen.height);

        if (iconHeights[toShow] == Screen.height)
        {
            while (Time.time - startTime < travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(Screen.height, Screen.height - iconSize, (Time.time - startTime) / travelTime);
                yield return null;
            }

            //Pause at Top///////////////////////////////////////
            iconHeights[toShow] = Screen.height - iconSize;
            yield return new WaitForSeconds(travelTime);

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(Screen.height - iconSize, Screen.height, (Time.time - startTime) / travelTime);
                yield return null;
            }
            iconHeights[toShow] = Screen.height;
        }

    }

    IEnumerator RemoveInput(int toShow)
    {
        float iconSize = Screen.height / 6f;
        float startTime = Time.time;
        float travelTime = 0.5f;

        //Slide UP //////////////////////////////
        if (iconHeights[toShow] == Screen.height)
        {

            while (Time.time - startTime < travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(Screen.height, Screen.height - iconSize, (Time.time - startTime) / travelTime);
                yield return null;
            }

            //Pause at Top///////////////////////////////////////
            iconHeights[toShow] = Screen.height - iconSize;
            yield return new WaitForSeconds(travelTime);

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                iconHeights[toShow] = Mathf.Lerp(Screen.height - iconSize, Screen.height, (Time.time - startTime) / travelTime);
                yield return null;
            }
            iconHeights[toShow] = Screen.height;
        }

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
        controlLayout = InputManager.AllConfigs[0];
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
                value = Input.GetAxis(inputAxisOne + controllerName);            
            }
            if ((inputAxisTwo != null) && value == 0)
            {
                value = -Input.GetAxis(inputAxisTwo + controllerName);
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
}

public enum ControllerType { Xbox360,Keyboard};
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
        set { }
    }

    private ControllerType type = ControllerType.Xbox360;
    public ControllerType Type
    {
        get { return type; }
        set { }
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

        List<string> validCommands = new List<string>() {"Throttle","Steer","Drift","Item","RearView","Pause","Submit","Cancel","MenuHorizontal","MenuVertical","Rotate"};
        commandsOne = new Dictionary<string, string>();
        commandsTwo = new Dictionary<string, string>();

        if (splitString.Length >= 3) //Must have 5 Inputs
        {
            name = splitString[0];

            if(splitString[1] == "Xbox360")
            {
                type = ControllerType.Xbox360;
            }
            else
            {
                type = ControllerType.Keyboard;
            }

            for(int i = 2; i < splitString.Length; i++)
            {
                string[] inputSplit = splitString[i].Split(":"[0]);

                if(inputSplit.Length == 2 && validCommands.Contains(inputSplit[0]))
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

    private bool CancelLoad()
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
        Debug.Log("returnString:" + returnString);
        return returnString;

    }
}
