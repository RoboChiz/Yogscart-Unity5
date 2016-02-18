using UnityEngine;
using System.Collections;
using TeamUtility.IO;

[RequireComponent(typeof(kartScript))]
public class kartInput : MonoBehaviour
{

    private kartScript ks;
    public PlayerID playerID = PlayerID.One;

    public bool camLocked = false;
    public Camera frontCamera, backCamera;

    // Update is called once per frame
    void Update ()
    {
        if (ks == null)
            ks = GetComponent<kartScript>();

        ks.throttle = InputManager.GetAxis("Throttle", playerID);
        ks.steer = InputManager.GetAxis("Steer", playerID);
        ks.drift = (InputManager.GetAxis("Drift", playerID) != 0);

        bool lookBehind = (InputManager.GetAxis("Rearview", playerID) != 0);

        if(!camLocked && lookBehind)
        {
            backCamera.enabled = true;
            frontCamera.enabled = false;
        }
        else
        {
            backCamera.enabled = false;
            frontCamera.enabled = true;
        }
    }
}
