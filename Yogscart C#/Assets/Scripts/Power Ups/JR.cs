using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JR : Egg
{

    private Transform parent, target;
    private bool lockOff = false;
    private int currentPoint;
    private TrackData td;

	// Update is called once per frame
	protected override void Update ()
    {
        //Get Track Data
        td = FindObjectOfType<TrackData>();

        //Update Position to Add Player in Front
        if (actingShield)
        {
            parent = transform.parent;
            currentPoint = parent.GetComponent<PositionFinding>().currentPos;
            bounces = 1;
            offset = 1.5f;
        }
        else
        {
            //Find Target, Only when released
            if (target == null && !lockOff)
            {
                PositionFinding pf = parent.GetComponent<PositionFinding>();

                //If not in first
                if (pf.position > 0)
                {
                    PositionFinding[] pfArray = FindObjectsOfType<PositionFinding>();

                    foreach (PositionFinding possiblePF in pfArray)
                    {
                        if (possiblePF.position == pf.position - 1)
                        {
                            target = possiblePF.transform;
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
                var layerMask = 1 << 10;
                layerMask = ~layerMask;

                if (target.GetComponent<PositionFinding>().currentPos <= currentPoint + 3 &&  Physics.Raycast(transform.position, attackDir, out hit, Mathf.Infinity, layerMask) && hit.transform == target)
                {
                    attackDir.y = 0;
                    direction = attackDir.normalized;
                }
                else
                {
                    //Otherwise travel along track, until we can hit the target
                    attackDir = (td.positionPoints[currentPoint].position - transform.position);
                    attackDir.y = 0;

                    direction = attackDir.normalized;
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
            {
                currentPoint++;
            }

        }

        base.Update();
	}
}
