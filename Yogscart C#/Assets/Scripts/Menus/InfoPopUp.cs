using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPopUp : GamePopup
{
    private string info = "";

    public override string text { get { return info; } }
    public override bool showYesNo { get { return false; } }

    public override Rect boxRect { get { return new Rect(560, 340, 800, 400); } }
    public override Rect labelRect { get { return new Rect(570, 350, 780, 330); } }

    public override Rect yesRect { get { return new Rect(570, 670, 380, 60); } }
    public override Rect noRect { get { return new Rect(960, 670, 380, 60); } }

    public void Setup(string _info)
    {
        info = _info;
        skin = Resources.Load<GUISkin>("GUISkins/PopUp");
        ShowPopUp();
    }

    public override void HidePopUp()
    {
        StartCoroutine(ActualHide());
        StartCoroutine(KillSelf());
    }

    private IEnumerator KillSelf()
    {
        yield return new WaitForSeconds(0.6f);
        Destroy(this);
    }

}
