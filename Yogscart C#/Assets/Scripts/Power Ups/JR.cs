using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JR : Egg
{

    private Transform parent, target;
    private bool lockOff = false;
    private int currentPoint;
    private TrackData td;

    protected void Start()
    {
        parent = transform.parent;
        currentPoint = parent.GetComponent<PositionFinding>().currentPos;
        offset = 1.5f;
        bounces = 0;
    }

	// Update is called once per frame
	protected override void Update ()
    {
        //Get Track Data
        td = FindObjectOfType<TrackData>();

        //Update Position to Add Player in Front
        if (!actingShield)
        {
            //Find Target, Only when released
            if (target == null && !lockOff)
            {
                PositionFinding pf = parent.GetComponent<PositionFinding>();
                Vector3 roadDir = td.positionPoints[MathHelper.NumClamp(currentPoint + 1, 0, td.positionPoints.Count)].position - td.positionPoints[currentPoint].position;

                //If not in first
                if (pf.position > 0)
                {
                    PositionFinding[] pfArray = FindObjectsOfType<PositionFinding>();

                    foreach (PositionFinding possiblePF in pfArray)
                    {
                        if (possiblePF.position == pf.position - 1)
                        {
                            //Check facing the right direction
                            if (Vector3.Dot(parent.forward, roadDir) > 0)
                            {
                                target = possiblePF.transform;

                                //Inform Kart Info or Attack
                                kartInfo kartInfo = target.GetComponent<kartInfo>();
                                if(kartInfo != null)
                                {
                                    kartInfo.NewAttack(Resources.Load<Texture2D>("UI/Power Ups/Clucky_1JR"), gameObject);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            lockOff = true;

            if (target != null)
            {
                //Test if we can hit the Target
                Vector3 attackDir = target.position - transform.position;
                RaycastHit hit;

                if (target.GetComponent<PositionFinding>().currentPos <= currentPoint + 3 && Physics.Raycast(transform.position, attackDir, out hit, Mathf.Infinity) && hit.transform == target)
                {
                    direction = attackDir.normalized;
                    overrideYPos = true;
                }
                else
                {
                    //Otherwise travel along track, until we can hit the target
                    attackDir = (td.positionPoints[currentPoint].position - transform.position);
                    attackDir.y = 0;

                    direction = attackDir.normalized;
                    overrideYPos = false;
                }
            }

            Vector3 targetPoint = td.positionPoints[currentPoint].position;
            targetPoint.y = 0;
            Vector3 lastPoint = td.positionPoints[MathHelper.NumClamp(currentPoint - 1, 0, td.positionPoints.Count)].position;
            lastPoint.y = 0;
            Vector3 myPos = transform.position;
            myPos.y = 0;

            //If distance to last point is greater than entire road distance
            if (Vector3.Distance(lastPoint, myPos) >= Vector3.Distance(targetPoint, lastPoint))
                currentPoint = MathHelper.NumClamp(currentPoint + 1, 0, td.positionPoints.Count);

        }

        base.Update();
	}
}
