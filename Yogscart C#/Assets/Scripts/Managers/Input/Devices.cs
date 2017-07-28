using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputIgnoreList
{
    public static readonly List<string> ignoreList = new List<string>() { "Throttle", "Brake", "SteerLeft", "SteerRight", "Drift", "Item", "RearView"};
}

//Predefined Controllers
public class KeyboardDevice : InputDevice
{
    //Constructor
    public KeyboardDevice(int _joystickID) : base(0) { }

    //Tell it we're a Keyboard
    public override InputType inputType { get { return InputType.Keyboard; } }

    //Return a static default control layout
    public override ControlLayout defaultLayout { get { return myDefault; } }
    public static ControlLayout myDefault = new ControlLayout(InputType.Keyboard, "Default",
        new Dictionary<string, List<ControlName>>()
        {
            //Menu Controls
            {"Leave", new List<ControlName>() {new ControlName("backspace", false) } },
            {"MenuHorizontal", new List<ControlName>() {new ControlName("a-", false), new ControlName("d+", false) } },
            {"MenuVertical", new List<ControlName>() {new ControlName("w-", false), new ControlName("s+", false) } },
            {"Submit", new List<ControlName>() {new ControlName("return", false) } },
            {"Cancel", new List<ControlName>() {new ControlName("escape", false) } },
            {"TabChange", new List<ControlName>() {new ControlName("q-", false), new ControlName("e+", false) } },
            {"HeightChange", new List<ControlName>() {new ControlName("q+", false), new ControlName("e-", false) } },
            {"ChangeTarget" , new List<ControlName>() {new ControlName("z-", false), new ControlName("x+", false) } },
            {"Toggle", new List<ControlName>() {new ControlName("space", false) } },
            {"Pause", new List<ControlName>() {new ControlName("escape", false) } },
            {"Rotate", new List<ControlName>() {new ControlName("q+", false), new ControlName("e-", false) } },
            {"HideUI", new List<ControlName>() {new ControlName("h", false) } },
            //Driving Controls
            {"Throttle", new List<ControlName>() {new ControlName("w", false) } },
            {"Brake", new List<ControlName>() {new ControlName("s", false) } },
            {"SteerLeft", new List<ControlName>() {new ControlName("a", false) } },
            {"SteerRight", new List<ControlName>() {new ControlName("d", false) } },
            {"Drift", new List<ControlName>() {new ControlName("space", false) } },
            {"Item", new List<ControlName>() {new ControlName("return", false) } },
            {"RearView", new List<ControlName>() {new ControlName("q", false) } },
        }
    );
}
public class XBox360Device : InputDevice
{
    //Constructor
    public XBox360Device(int _joystickID) : base(_joystickID) { }

    //Tell it we're a 360 controller
    public override InputType inputType { get { return InputType.Xbox360; } }

    //Return a static default control layout
    public override ControlLayout defaultLayout { get { return myDefault; } }
    public static ControlLayout myDefault = new ControlLayout(InputType.Xbox360, "Default",
        new Dictionary<string, List<ControlName>>()
        {
            //Menu Controls
            {"Leave", new List<ControlName>() {new ControlName(Back, false) } },
            {"MenuHorizontal", new List<ControlName>() {new ControlName(LeftStickHori,true), new ControlName(DPadHori, true) } },
            {"MenuVertical", new List<ControlName>() {new ControlName(LeftStickVert,true), new ControlName(DPadVert, true) } },
            {"Submit", new List<ControlName>() {new ControlName(Start, false), new ControlName(A, false) } },
            {"Cancel", new List<ControlName>() {new ControlName(B, false) } },
            {"HeightChange", new List<ControlName>() {new ControlName(LT + "-", true), new ControlName(RT + "+", true) } },
            {"RightStickVert", new List<ControlName>() {new ControlName(RightStickVert, true) } },
            {"RightStickHori", new List<ControlName>() {new ControlName(RightStickHori, true) } },
            {"TabChange" , new List<ControlName>() {new ControlName(LB + "-", false), new ControlName(RB + "+", false) } },
            {"SprintToggle", new List<ControlName>() {new ControlName(LS, false) } },
            {"ChangeTarget" , new List<ControlName>() {new ControlName(LB + "-", false), new ControlName(RB + "+", false) } },
            {"Toggle", new List<ControlName>() {new ControlName(X, false) } },
            {"Pause", new List<ControlName>() {new ControlName(Start, false) } },
            {"Rotate", new List<ControlName>() {new ControlName(RightStickHori, true) } },
            {"HideUI", new List<ControlName>() {new ControlName(Y, false) } },
            //Driving Controls
            {"Throttle", new List<ControlName>() {new ControlName(A, false) } },
            {"Brake", new List<ControlName>() {new ControlName(B, false) } },
            {"SteerLeft", new List<ControlName>() {new ControlName(LeftStickHori + "-", true) } },
            {"SteerRight", new List<ControlName>() {new ControlName(LeftStickHori + "+", true) } },
            {"Drift", new List<ControlName>() {new ControlName(LT, true), new ControlName(RT, true) } },
            {"Item", new List<ControlName>() {new ControlName(LB, false), new ControlName(RB, false) } },
            {"RearView", new List<ControlName>() {new ControlName(X, false) } },
            //Xbox Specific
            {"Minus", new List<ControlName>() {new ControlName(Y, false) } },
            {"Edit", new List<ControlName>() {new ControlName(X, false) } },
            {"ViewScroll", new List<ControlName>() {new ControlName(RightStickVert, true) } },
        }
    );

    //Const Mappings to avoid knowing exact button names
    public const string LeftStickHori = "AxisX";
    public const string LeftStickVert = "AxisY";

    public const string LT = "Axis3-";
    public const string RT = "Axis3+";

    public const string RightStickHori = "Axis4";
    public const string RightStickVert = "Axis5";

    public const string DPadHori = "Axis6";
    public const string DPadVert = "Axis7";

    public const string A = "button 0";
    public const string B = "button 1";
    public const string X = "button 2";
    public const string Y = "button 3";

    public const string LB = "button 4";
    public const string RB = "button 5";

    public const string Back = "button 6";
    public const string Start = "button 7";

    public const string LS = "button 8";
    public const string RS = "button 9";
}