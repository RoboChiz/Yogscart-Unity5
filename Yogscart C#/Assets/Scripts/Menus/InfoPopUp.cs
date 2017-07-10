using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPopUp : GamePopup
{
    private string info = "";

    public override string text { get { return info; } }
    public override bool showYesNo { get { return false; } }
    public override Vector2 size { get { return new Vector2(800,400); } }

    public void Setup(string _info)
    {
        info = _info;
        skin = Resources.Load<GUISkin>("GUISkins/PopUp");
        ShowPopUp();
    }

}
