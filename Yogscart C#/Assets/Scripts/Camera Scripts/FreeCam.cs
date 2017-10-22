using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public float moveSpeed = 14f, rotateControllerSpeed = 150f, rotateMouseSpeed = 150f, lerpSmooth = 5f;
    public Vector3 targetEuler, actualEuler;

    private bool sprintToggle = false;

    private CurrentGameData gd;

	// Use this for initialization
	void Start ()
    {
        gd = FindObjectOfType<CurrentGameData>();
        SetStartRotation();
    }

    public void SetStartRotation()
    {
        targetEuler = transform.rotation.eulerAngles;
        actualEuler = transform.rotation.eulerAngles;
    }
	
	// Update is called once per frame
	void Update ()
    {
        float rotateHori = 0f, rotateVert = 0f;

        if (InputManager.controllers.Count > 0)
        {
            float actualMoveSpeed = moveSpeed;

            //Move Cam faster if shift is held down
            if (InputManager.controllers[0].inputType == InputType.Keyboard)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    sprintToggle = true;
                else
                    sprintToggle = false;
            }

            if(sprintToggle)
                actualMoveSpeed *= 4f;

            //Move Camera
            float vert = InputManager.controllers[0].GetInput("MenuVertical");
            float hori = InputManager.controllers[0].GetInput("MenuHorizontal");
            float height = 0f;

            height = -InputManager.controllers[0].GetInput("HeightChange");

            transform.position -= transform.forward * vert * actualMoveSpeed * Time.deltaTime;
            transform.position += transform.right * hori * actualMoveSpeed * Time.deltaTime;
            transform.position += transform.up * height * actualMoveSpeed * Time.deltaTime;

            //Rotate Camera
            if (InputManager.controllers[0].inputType == InputType.Xbox360)
            {
                rotateHori = InputManager.controllers[0].GetInput("RightStickVert") * rotateControllerSpeed * gd.controllerScale;
                rotateVert = -InputManager.controllers[0].GetInput("RightStickHori") * rotateControllerSpeed * gd.controllerScale;

                //Toggle Sprint On/Off
                if (InputManager.controllers[0].GetButtonWithLock("SprintToggle"))
                    sprintToggle = !sprintToggle;
            }
        }

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float xInput = -Input.GetAxis("Mouse X");
            float yInput = -Input.GetAxis("Mouse Y");

            rotateHori = yInput * rotateMouseSpeed * gd.mouseScale;
            rotateVert = xInput * rotateMouseSpeed * gd.mouseScale;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        targetEuler.x += rotateHori * Time.deltaTime;
        targetEuler.y -= rotateVert * Time.deltaTime;

        actualEuler = Vector3.Lerp(actualEuler, targetEuler, Time.deltaTime * lerpSmooth);
        transform.rotation = Quaternion.Euler(actualEuler);
    }
}
