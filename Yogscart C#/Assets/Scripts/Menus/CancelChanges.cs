using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelChanges : GamePopup
{

    public override string text { get { return "Would you like to confirm changes?"; } }

    protected override void DoInput()
    {
        if (!noSelected)
        {
            FindObjectOfType<Options>().SaveEverything();
        }

        FindObjectOfType<Options>().somethingChanged = false;
        FindObjectOfType<Options>().Quit();
        HidePopUp();
    }
    
}
