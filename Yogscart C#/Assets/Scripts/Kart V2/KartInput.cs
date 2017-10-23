using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KartMovement))]
public class KartInput : MonoBehaviour
{
    private KartMovement km;
    private KartItem ki;

    public int myController;
    public bool camLocked = false;
    public Camera frontCamera, backCamera;

    public static bool overrideCamera = false;

    void Start()
    {
        km = GetComponent<KartMovement>();
        ki = GetComponent<KartItem>();

        km.toProcess.Add(frontCamera);
        km.toProcess.Add(backCamera);
    }

    // Update is called once per frame
    void Update ()
    {
        if (InputManager.controllers.Count > myController)
        {
            bool lookBehind = (InputManager.controllers[myController].GetInput("RearView") != 0);

            //Kart Movement
            km.throttle = Mathf.Abs(InputManager.controllers[myController].GetInput("Throttle"));
            km.throttle -= Mathf.Abs(InputManager.controllers[myController].GetInput("Brake"));

            km.steer = Mathf.Abs(InputManager.controllers[myController].GetInput("SteerRight")) - Mathf.Abs(InputManager.controllers[myController].GetInput("SteerLeft"));
            km.drift = (InputManager.controllers[myController].GetInput("Drift") != 0);

            //Kart Item
            ki.input = (InputManager.controllers[myController].GetInput("Item") != 0);

            if(lookBehind)
            {
                ki.direction = -1;
            }
            else
            {
                if (InputManager.controllers[myController].inputType == InputType.Xbox360)
                    ki.direction = InputManager.controllers[myController].GetIntInput("ItemDirection");
                else
                    ki.direction = 1;
            }

            //Cameras
            if (frontCamera != null && backCamera != null && !overrideCamera)
            {
               

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
