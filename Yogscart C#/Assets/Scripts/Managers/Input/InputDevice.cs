using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InputType { Keyboard, Xbox360 }

public abstract class InputDevice
{
    //Lock off all inputs
    public static bool locked = false;
    //Lock off this specific input
    public bool localLocked = false;

    //What type of controller is this
    public abstract InputType inputType { get; }

    //What is the ID for this Joystick. -1 = NULL, 0 = Keyboard, 1> = Joystick
    public int joystickID = -1;

    //Provide a Default Layout for the Controller
    public abstract ControlLayout defaultLayout { get; }

    //The layout we are currently using
    public ControlLayout currentLayout { get; private set; }
    public void SetLayout(ControlLayout _currentLayout)
    {
        currentLayout = _currentLayout;
    }

    public string inputLock = "";

    //Used in Menus
    public bool toggle, killing;
    public float boxHeight, alpha, lastXPos;
    public Vector2 layoutSelectionViewRect;
    public int currentSelection = 0;

    //Constructor
    public InputDevice(int _joystickID)
    {
        //Set inital layout to defeault
        currentLayout = defaultLayout;

        joystickID = _joystickID;
        toggle = false;
        boxHeight = 0f;
    }

    enum WantedInput { Either, PlusOnly, MinusOnly, Inverse }

    //Get Inputs
    public float GetInput(string _input) { return ActualGetInput(_input, false, currentLayout); }
    public int GetIntInput(string _input) { return (int)MathHelper.Sign(ActualGetInput(_input, false, currentLayout)); }

    public float GetRawInput(string _input) { return ActualGetInput(_input, true, currentLayout); }
    public int GetRawIntInput(string _input) { return (int)MathHelper.Sign(ActualGetInput(_input, true, currentLayout)); }

    public bool GetButton(string _input) { return ActualGetInput(_input, false, currentLayout) != 0; }

    //Forces lock to be released before input is allowed
    public float GetInputWithLock(string _input)
    {
        if (!toggle && inputLock == "")
        {
            float val = GetInput(_input);
            if (val != 0)
            {
                inputLock = _input;
                return val;
            }
        }

        return 0;
    }

    public int GetIntInputWithLock(string _input)
    {
        if (!toggle && inputLock == "")
        {
            float val = GetIntInput(_input);
            if (val != 0)
            {
                inputLock = _input;
                return (int)Mathf.Sign(val);
            }
        }

        return 0;
    }

    public float GetRawInputWithLock(string _input)
    {
        if (!toggle && inputLock == "")
        {
            float val = GetRawInput(_input);
            if (val != 0)
            {
                inputLock = _input;
                return val;
            }
        }

        return 0;
    }

    public int GetRawIntInputWithLock(string _input)
    {
        if (!toggle && inputLock == "")
        {
            int val = GetRawIntInput(_input);
            if (val != 0)
            {
                inputLock = _input;
                return val;
            }
        }

        return 0;
    }

    public bool GetButtonWithLock(string _input)
    {
        if (!toggle && inputLock == "")
        {
            if (GetButton(_input))
            {
                inputLock = _input;
                return true;
            }
        }

        return false;
    }

    public bool GetButtonWithLockForToggle(string _input)
    {
        if (inputLock == "")
        {
            if (GetButton(_input))
            {
                inputLock = _input;
                return true;
            }
        }

        return false;
    }

    public int GetRawIntInputWithLockForToggle(string _input)
    {
        if (inputLock == "")
        {
            int val = GetRawIntInput(_input);
            if (val != 0)
            {
                inputLock = _input;
                return val;
            }
        }

        return 0;
    }

    private float ActualGetInput(string _input, bool getRaw, ControlLayout controlLayout)
    {
        float returnVal = 0f;

        if (!localLocked && !locked && controlLayout != null)
        {
            if (controlLayout.controls.ContainsKey(_input))
            {
                foreach (ControlName control in controlLayout.controls[_input])
                {
                    string buttonName = "";

                    //Add Joystick name to button if not Keyboard
                    if (inputType != InputType.Keyboard)
                    {
                        if(!control.isAxis)
                            buttonName = "joystick " + joystickID + " ";
                        else
                            buttonName = "Joystick" + joystickID;
                    }

                    //Add Input
                    buttonName += control.inputName;

                    //Check for + / - or * symbol 
                    //Side Note: Using +/- on an axis will only allow values which are positive or negative. 
                    //Using on a button will return either a positive or negative value
                    //Using * will inverse an axis
                    WantedInput wantedInput = WantedInput.Either;

                    if (buttonName[buttonName.Length - 1] == '-')
                        wantedInput = WantedInput.MinusOnly;
                    else if (buttonName[buttonName.Length - 1] == '+')
                        wantedInput = WantedInput.PlusOnly;
                    else if(buttonName[buttonName.Length - 1] == '*')
                        wantedInput = WantedInput.Inverse;

                    //Remove the + or - from the input
                    if (wantedInput != WantedInput.Either)
                        buttonName = buttonName.Remove(buttonName.Length - 1, 1);

                    //Check if input is an axis or a button
                    if (control.isAxis)
                    {
                        //Get the actual input
                        float tempValue = 0f;
                        if (getRaw)
                            tempValue = Input.GetAxisRaw(buttonName);
                        else
                            tempValue = Input.GetAxis(buttonName);

                        if (tempValue != 0f)
                        {
                            //Assign the temp value if it is valid
                            switch (wantedInput)
                            {
                                case WantedInput.Either: returnVal = tempValue; break;
                                case WantedInput.MinusOnly: if (tempValue <= 0f) returnVal = tempValue; break;
                                case WantedInput.PlusOnly: if (tempValue >= 0f) returnVal = tempValue; break;
                                case WantedInput.Inverse: returnVal = -tempValue; break;
                            }
                        }
                    }
                    else
                    {
                        //Get the actual input if button is pressed
                        returnVal = Input.GetKey(buttonName) ? (wantedInput == WantedInput.MinusOnly ? -1 : 1) : 0;
                    }

                    //Break out of inputs if input found
                    if (returnVal != 0f)
                        break;
                }
            }
            else
            {
                if (controlLayout == defaultLayout)
                    Debug.LogError(_input + " is not a registered input!");
                else if(!InputIgnoreList.ignoreList.Contains(_input))
                    return ActualGetInput(_input, getRaw, defaultLayout);
            }
        }

        return returnVal;
    }
}

public class ControlLayout
{
    //Dictionary containing all input names and what button/axis are used for them
    public Dictionary<string, List<ControlName>> controls;
    public InputType inputType { get; set; }
    public string layoutName;

    public ControlLayout(InputType _inputType, string _name, Dictionary<string, List<ControlName>> _controls)
    {
        inputType = _inputType;
        controls = _controls;
        layoutName = _name;
    }

    //Returns current state of Control Layout for Saving
    public override string ToString()
    {
        string toReturn = ((int)inputType).ToString() + "|" + layoutName;

        foreach (KeyValuePair<string, List<ControlName>> pair in controls)
        {
            if (pair.Value.Count > 0)
            {
                toReturn += "|";

                toReturn += pair.Key;

                foreach (ControlName cn in pair.Value)
                {
                    toReturn += ":";
                    toReturn += cn.inputName;
                }
            }
        }

        return toReturn;
    }

    //Parse current string into Control Layout
    public static ControlLayout Parse(string _data)
    {
        //Split the data
        List<string> allPairs = _data.Split('|').ToList();

        //Make the return value
        ControlLayout toReturn = new ControlLayout((InputType)int.Parse(allPairs[0]), allPairs[1], new Dictionary<string, List<ControlName>>());

        //Remove input type and name from array
        allPairs.RemoveRange(0, 2);

        foreach (string pair in allPairs)
        {
            //Split each input string into another list
            string[] array = pair.Split(':');

            //If we already have this input throw an exception
            if (toReturn.controls.ContainsKey(array[0]))
                throw new Exception("Input already exists!");

            //Create a new list of control names
            List<ControlName> inputs = new List<ControlName>();
            for (int i = 1; i < array.Length; i++)
                inputs.Add(new ControlName(array[i], (array[i].Length > 4f && array[i].Substring(0, 4) == "Axis") ? true : false));

            toReturn.controls.Add(array[0], inputs);
        }

        return toReturn;
    }

    public void ChangeInput(string _input, string _button, bool _usesAxis, int position)
    {
        //If the input dosen't exist create it
        if (!controls.ContainsKey(_input))
            controls.Add(_input, new List<ControlName>());

        //Make sure we have an input to replace
        while (controls[_input].Count < position + 1)
            controls[_input].Add(new ControlName("", false));

        controls[_input][position] = new ControlName(_button, _usesAxis);
    }

    public void ClearInput(string _input, int position)
    {
        //If the input dosen't exist create it
        if (controls.ContainsKey(_input) && controls[_input].Count > position)
        {
            controls[_input].RemoveAt(position);
        }
    }
}

public class ControlName
{
    public string inputName { get; private set; }
    public bool isAxis { get; private set; }

    public ControlName(string _inputName, bool _isAxis)
    {
        inputName = _inputName;
        isAxis = _isAxis;
    }
}