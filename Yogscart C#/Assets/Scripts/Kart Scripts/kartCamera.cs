using UnityEngine;
using System.Collections;

public class kartCamera : MonoBehaviour {

    public Transform target;

    public float distance = 6f, height = 2f, playerHeight = 2f, angle = 0f, sideAmount = 0f;
    const float turnSmooth = 5f, rotSmoothTime = 5f;
    private Quaternion finalRot;
	
	// Update is called once per frame
	void Update ()
    {
        if(target != null)
        {
            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Quaternion test = Quaternion.LookRotation(-target.forward, Vector3.up);
            finalRot = Quaternion.Lerp(finalRot, test * quat, Time.deltaTime * turnSmooth);

            Vector3 pos = target.position + (finalRot * Vector3.forward * distance) + (Vector3.up * height);
            transform.position = pos;

            Vector3 lookDir = target.position - (transform.position - (Vector3.up * playerHeight) + 
                (transform.right * sideAmount));
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), 
                Time.deltaTime * rotSmoothTime);

        }
    }
}
