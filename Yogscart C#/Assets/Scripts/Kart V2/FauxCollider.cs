using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All the fun of Wheel Colliders without the stress
public class FauxCollider : MonoBehaviour
{
    public bool groundHit;
    public Vector3 surfaceImpactPoint { get; private set; }
    public Vector3 surfaceImpactNormal { get; private set; }
    public string surfaceImpactTag { get; private set; }
    public float compressionRatio = 0f;

    public float suspensionDistance = 0.35f;
    public float springCoefficent = 10f, dampeningCoefficent = 10f;

    private Rigidbody ownerRigidBody;

    private float lastX = 0.25f;

    void Start()
    {
        ownerRigidBody = GetComponentInParent<Rigidbody>();
    }

	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (ownerRigidBody != null)
        {

            bool upright = Vector3.Angle(transform.up, Vector3.up) < 45f;

            //Test if we've hit the ground
            RaycastHit hit = new RaycastHit();
            var layerMask = ~((1 << 8) | (1 << 9) | (1 << 10) | (1 << 11));

            if (upright && Physics.Raycast(transform.position, -transform.up, out hit, suspensionDistance, layerMask) && Vector3.Angle(hit.normal, Vector3.up) < 35f)
            {
                groundHit = true;
                surfaceImpactPoint = hit.point;
                surfaceImpactNormal = hit.normal;
                surfaceImpactTag = hit.transform.tag;

                //Get Compression Ratio
                compressionRatio = (suspensionDistance - (transform.position - hit.point).magnitude) / suspensionDistance;           
            }
            else
            {
                compressionRatio = 0f;
                //Reset Ground Hit
                groundHit = false;
            }

            compressionRatio = Mathf.Clamp(compressionRatio, 0f, 1f);


            //Apply Compression Ratio Force
            if (groundHit)
            {
                //F = - kx - bv
                float k = springCoefficent, x = (transform.position - hit.point).magnitude - suspensionDistance;
                float b = dampeningCoefficent, v = (x- lastX) / Time.fixedDeltaTime;
                float f = -(k * x) -(b * v);

                ownerRigidBody.AddForceAtPosition(ownerRigidBody.transform.up * f, transform.position, ForceMode.Acceleration);

                lastX = x;
            }
        }
	}
}
