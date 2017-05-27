using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowTipping : MonoBehaviour
{

    public enum PushState { None, LeftNormal, LeftHalf, RightNormal, RightHalf};

    public PushState pushState;
    private bool waitPush = false, waitLand;

    private float pushSpeed = 10, pushTime = 0.2f, currentPushTime, desiredY, offset = 0.5f;

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

            rigidbody.AddRelativeForce(finalPush - relativeVelocity.x, 0, 0, ForceMode.VelocityChange);

            RaycastHit hit;
            var layerMask = 1 << 8;
            layerMask = ~layerMask;

            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                desiredY = hit.point.y + offset;

                float difference = desiredY - transform.position.y;

                if(difference > 1)
                    transform.position = transform.position + new Vector3(0, difference, 0);
            }

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
            if (GetComponent<kartScript>().isFalling)
            {
                if(waitLand)
                    rigidbody.velocity = Vector3.Scale(rigidbody.velocity,new Vector3(0f, 1f, 0f)) + Vector3.Scale(lastVelocity, new Vector3(1f, 0, 1f));
            }
            else
            {
                waitLand = false;
            }
        }
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
