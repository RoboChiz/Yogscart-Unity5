using UnityEngine;
using System.Collections;

[RequireComponent(typeof(kartScript))]
public class kartInput : MonoBehaviour
{

    private kartScript ks;
    public int myController;
    public bool camLocked = false;
    public Camera frontCamera, backCamera;

    // Update is called once per frame
    void Update ()
    {
        if (InputManager.controllers.Count > myController)
        {
            if (ks == null)
                ks = GetComponent<kartScript>();

            ks.throttle = Mathf.Abs(InputManager.controllers[myController].GetInput("Throttle"));
            ks.throttle -= Mathf.Abs(InputManager.controllers[myController].GetInput("Brake"));

            ks.steer = Mathf.Abs(InputManager.controllers[myController].GetInput("SteerRight")) - Mathf.Abs(InputManager.controllers[myController].GetInput("SteerLeft"));
            ks.drift = (InputManager.controllers[myController].GetInput("Drift") != 0);

            bool lookBehind = (InputManager.controllers[myController].GetInput("RearView") != 0);

            if (!camLocked && lookBehind)
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
}
