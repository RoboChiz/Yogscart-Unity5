using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCam : MonoBehaviour
{
    public Vector2 xyValues = Vector2.zero, actualXYValues;
    public float xSpeed = 10f, ySpeed = 15f, lerpSpeed = 3f, zoomSpeed = 1f;
    private Vector2 lastMousePos;
    private float actualRadius;

    public float radius = 6f;
    public Transform target;

    public float rotateControllerSpeed = 2f, zoomControllerSpeed = 0.2f;

    // Use this for initialization
    void Start ()
    {
        lastMousePos = Input.mousePosition;
        xyValues = new Vector2(-90, 5f);
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (target)
        {
            if (Input.GetMouseButton(1))
            {
                Vector2 diff = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - lastMousePos;

                //Inputs
                xyValues.x += diff.x * xSpeed * Time.deltaTime;
                xyValues.y += diff.y * ySpeed * Time.deltaTime;
            }

            if (InputManager.controllers.Count > 0 && InputManager.controllers[0].controlLayout.Type == ControllerType.Xbox360)
            {
                xyValues.y -= InputManager.controllers[0].GetInput("RightStickVert") * rotateControllerSpeed;
                xyValues.x -= InputManager.controllers[0].GetInput("RightStickHori") * rotateControllerSpeed;

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

        lastMousePos = Input.mousePosition;
    }
}
