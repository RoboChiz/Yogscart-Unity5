using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowTipping : MonoBehaviour
{

    public enum PushState { None, LeftNormal, LeftHalf, RightNormal, RightHalf};

    public PushState pushState;
    private bool waitPush = false, waitLand;

    private float pushSpeed = 2f, pushTime = 0.1f, currentPushTime, desiredY, offset = 0.5f;

    private new Rigidbody rigidbody;
    private Vector3 lastVelocity;

	// Use this for initialization
	void Start ()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        /*
        Vector3 relativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);

        if(currentPushTime > 0)
        {
            float finalPush = 0f;
            switch(pushState)
            {
                case PushState.LeftNormal: finalPush = -pushSpeed; break;
                case PushState.RightNormal: finalPush = pushSpeed; break;
                case PushState.LeftHalf: finalPush = -pushSpeed/2f; break;
                case PushState.RightHalf: finalPush = pushSpeed/2f; break;
            }

            Vector3 upNormal = Vector3.up;

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity) && hit.transform.tag == "Ground" || hit.transform.tag == "OffRoad")
            {
                upNormal = hit.normal;
            }

            Vector3 dir = Vector3.ProjectOnPlane(transform.right, upNormal).normalized;
            rigidbody.AddForce(dir * (finalPush - relativeVelocity.x), ForceMode.VelocityChange);

            currentPushTime -= Time.fixedDeltaTime;
        }
        else
        {

            if(waitPush)
            {
                waitPush = false;
                rigidbody.useGravity = true;

                WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
                foreach (WheelCollider wheel in wheels)
                    wheel.enabled = true;
            }

            if(currentPushTime < 0)
                currentPushTime = 0;

            //Keep Kart travelling forward after Push
            if (GetComponent<KartMovement>().isFalling)
            {
                if(waitLand)
                    rigidbody.velocity = Vector3.Scale(rigidbody.velocity,new Vector3(0f, 1f, 0f)) + Vector3.Scale(lastVelocity, new Vector3(1f, 0, 1f));
            }
            else
            {
                waitLand = false;
            }
        }*/
	}

    public void TipCow()
    {
        currentPushTime = pushTime;

        rigidbody.useGravity = false;

        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheels)
            wheel.enabled = false;

        lastVelocity = rigidbody.velocity;

        waitPush = true;
        waitLand = true;
    }

}
