using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivingIK : MonoBehaviour {

    public Transform leftHandTarget, rightHandTarget, leftFoorTarget, rightFootTarget;

    private Animator animator;
	
    void Start()
    {     
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
        }		
	}
}
