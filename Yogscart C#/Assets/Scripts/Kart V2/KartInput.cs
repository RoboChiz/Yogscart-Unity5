using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KartMovement))]
public class KartInput : MonoBehaviour
{
    private KartMovement km;
    public int myController;
    public bool camLocked = false;
    public Camera frontCamera, backCamera;

    public static bool overrideCamera = false;

    void Start()
    {
        km = GetComponent<KartMovement>();

        km.toProcess.Add(frontCamera);
        km.toProcess.Add(backCamera);
    }

    // Update is called once per frame
    void Update ()
    {
        if (InputManager.controllers.Count > myController)
        {
            km.throttle = Mathf.Abs(InputManager.controllers[myController].GetInput("Throttle"));
            km.throttle -= Mathf.Abs(InputManager.controllers[myController].GetInput("Brake"));

            km.steer = Mathf.Abs(InputManager.controllers[myController].GetInput("SteerRight")) - Mathf.Abs(InputManager.controllers[myController].GetInput("SteerLeft"));
            km.drift = (InputManager.controllers[myController].GetInput("Drift") != 0);

            if (frontCamera != null && backCamera != null && !overrideCamera)
            {
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
}
