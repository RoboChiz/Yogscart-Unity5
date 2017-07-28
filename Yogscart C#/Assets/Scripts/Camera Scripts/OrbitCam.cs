using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCam : MonoBehaviour
{
    public Vector2 xyValues = Vector2.zero, actualXYValues;
    public float xSpeed = 150f, ySpeed = 150f, lerpSpeed = 3f, zoomSpeed = 1f;
    private float actualRadius;

    public float radius = 6f;
    public Transform target;

    private CurrentGameData gd;

    public float rotateControllerSpeed = 2f, zoomControllerSpeed = 0.2f;

    // Use this for initialization
    void Start ()
    {
        gd = FindObjectOfType<CurrentGameData>();
        xyValues = new Vector2(-90, 5f);
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (target)
        {
            if (Input.GetMouseButton(1))
            {
                float xInput = Input.GetAxis("Mouse X") * gd.mouseScale;
                float yInput = Input.GetAxis("Mouse Y") * gd.mouseScale;

                //Inputs
                xyValues.x += xInput * xSpeed * Time.deltaTime;
                xyValues.y += yInput * ySpeed * Time.deltaTime;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if (InputManager.controllers.Count > 0 && InputManager.controllers[0].inputType == InputType.Xbox360)
            {
                xyValues.y -= InputManager.controllers[0].GetInput("RightStickVert") * rotateControllerSpeed * gd.controllerScale;
                xyValues.x -= InputManager.controllers[0].GetInput("RightStickHori") * rotateControllerSpeed * gd.controllerScale;

                radius += InputManager.controllers[0].GetInput("MenuVertical") * zoomControllerSpeed;
            }

            radius -= Input.mouseScrollDelta.y * zoomSpeed;

            xyValues.y = Mathf.Clamp(xyValues.y, 0f, 90f);
            radius = Mathf.Clamp(radius, 2f, 15f);

            actualXYValues = Vector2.Lerp(actualXYValues, xyValues, Time.deltaTime * lerpSpeed);
            actualRadius = Mathf.Lerp(actualRadius, radius, Time.deltaTime * lerpSpeed);

            Quaternion rotation = Quaternion.Euler(actualXYValues.y, actualXYValues.x, 0f);

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -actualRadius);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position + Vector3.up;

        }
    }
}
