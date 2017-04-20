using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivingIK : MonoBehaviour
{
    private Animator animator;
    private kartScript ks;
    private InterestManager interestManager;

    public Transform leftHandTarget, rightHandTarget, leftFoorTarget, rightFootTarget;

    public PointOfInterest headLook;
    public float headWait, headWeight;

    private Transform myParent;

    private Quaternion startThrottleRot, endThrottleRot;


    void Start()
    {
        interestManager = FindObjectOfType<InterestManager>();

        myParent = transform;

        while (myParent.parent != null)
            myParent = myParent.parent;

        ks = myParent.GetComponent<kartScript>();

        //Set up Pedal Animation Position
        startThrottleRot = rightFootTarget.localRotation;
        endThrottleRot = rightFootTarget.localRotation * Quaternion.AngleAxis(30f, Vector3.right);
    }

    void Update()
    {

        //Do Pedal Movements
        if (ks != null)
        {
            rightFootTarget.localRotation = Quaternion.Lerp(startThrottleRot, endThrottleRot, Mathf.Abs(ks.throttle));
        }

        //Do Head Movements
        headWait -= Time.deltaTime;

        if(headWait <= 0)
        {
            if(headLook == null)
            {
                List<PointOfInterest> possiblePoints = interestManager.GetPoI(transform.position);

                //Decide which point we should look at
                PointOfInterest bestPoint = null;

                foreach(PointOfInterest point in possiblePoints)
                {
                    TransformPointOfInterest transformPoint = point as TransformPointOfInterest;

                    if (transformPoint == null || transformPoint.target != myParent)
                    {
                        float distToPoint = Vector3.Distance(point.GetLocation(), transform.position);

                        //Any Interest Point has to be within 10 Metres
                        if (distToPoint < 10f)
                        {
                            //Choose first point as Best Point 
                            if (bestPoint == null)
                                bestPoint = point;

                            //Choose point is an attack
                            if (point.interestType == InterestType.Attack)
                            {
                                if (bestPoint.interestType != InterestType.Attack)
                                    bestPoint = point;
                                else
                                {
                                    //If both are attacks, look at the one that closer
                                    float distToBestPoint = Vector3.Distance(bestPoint.GetLocation(), transform.position);
                                    if (distToPoint < distToBestPoint)
                                        bestPoint = point;
                                }
                            }
                            else if (bestPoint.interestType != InterestType.Attack)
                            {
                                //If Neither Point is an attack, choose the closest
                                float distToBestPoint = Vector3.Distance(bestPoint.GetLocation(), transform.position);
                                if (distToPoint < distToBestPoint)
                                    bestPoint = point;
                            }
                        }
                    }
                }

                headLook = bestPoint;
                headWait = Random.Range(3f, 5f);
            }
            else
            {
                headLook = null;
                headWait = Random.Range(1f, 10f);
            }      
        }

        //Look at Object if in front of Vehicle
        bool canLook = true;

        if (headLook == null)
            canLook = false;
        else
        {
            float dot = Vector3.Dot(transform.forward, (headLook.GetLocation() - transform.position));

            if ((headLook.interestType != InterestType.Attack && dot <= 0) || (headLook.interestType == InterestType.Attack && dot <= 0.25f))
                canLook = false;
            else if (Mathf.Abs(headLook.GetLocation().y - transform.position.y) > 5f)
                canLook = false;
        }

        if(canLook)
        {
            if (headWeight < 1f)
                headWeight += Time.deltaTime / 2f;
            else
                headWeight = 1f;
        }
        else
        {
            if (headWeight > 0f)
                headWeight -= Time.deltaTime / 2f;
            else
                headWeight = 0f;
        }
    }

    public void ForceLook(Transform target)
    {
        headLook = new TransformPointOfInterest(target, InterestType.Attack);
        headWait = Random.Range(5f, 7f);
    }

	// Update is called once per frame
	void OnAnimatorIK ()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        else
        {
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);

            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoorTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoorTarget.rotation);

            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            if(headLook != null)
                animator.SetLookAtPosition(headLook.GetLocation());

            animator.SetLookAtWeight(headWeight);
        }		
	}
}
