using UnityEngine;
using System.Collections;

[RequireComponent(typeof(kartScript))]
public class kartInput : MonoBehaviour
{

    private kartScript ks;

    public bool camLocked = false;
    public Camera frontCamera, backCamera;

    // Update is called once per frame
    void Update ()
    {
        if (ks == null)
            ks = GetComponent<kartScript>();

        ks.throttle = Input.GetAxis("Throttle");
        ks.steer = Input.GetAxis("Steer");
        ks.drift = (Input.GetAxis("Drift") != 0);

        bool lookBehind = (Input.GetAxis("Rearview") != 0);

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
