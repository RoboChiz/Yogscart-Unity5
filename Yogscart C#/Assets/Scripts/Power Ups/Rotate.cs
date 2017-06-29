using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    public float rotateSpeed = 0.5f, floatSpeed = 0.001f;
    private float origin;
    private bool up = false;

    public Vector3 axis = Vector3.up;

	// Use this for initialization
	void Start ()
    {
        origin = transform.position.y;
	}
	
	// Update is called once per frame
	void FixedUpdate()
    {
        transform.Rotate(axis, rotateSpeed);

        if (up)
            transform.position += new Vector3(0,floatSpeed,0);
        else
            transform.position -= new Vector3(0, floatSpeed, 0);

        if (transform.position.y > origin + 0.1)
            up = false;

        if (transform.position.y < origin - 0.1)
            up = true;
    }
}
