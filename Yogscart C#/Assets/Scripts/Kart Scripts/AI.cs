using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI V3 *Sighs*
/// Coded by Robo_Chiz 2017
/// </summary>
[RequireComponent(typeof(KartScript), typeof(PositionFinding))]
public class AI : MonoBehaviour
{
    public bool canDrive = true;

    //Decides how we'll start the Race
    public enum StartType { WillBoost, WontBoost, WillSpin };
    public StartType myStartType;

    public enum AIStupidity { Stupid, Bad, Good, Great, Perfect };
    public AIStupidity intelligence = AIStupidity.Stupid;

    private KartScript ks;
    private PositionFinding pf;
    private TrackData td;

    public PointHandler aimPoint;

    private const float requiredAngle = 7f;
    public bool reversing;

    // Use this for initialization
    void Start ()
    {
        ks = GetComponent<KartScript>();
        pf = GetComponent<PositionFinding>();
        td = FindObjectOfType<TrackData>();

        //Decide my Start Boost
        myStartType = StartType.WontBoost;

        switch(intelligence)
        {
            case AIStupidity.Stupid:
                myStartType = StartType.WillSpin;
                break;

            case AIStupidity.Bad:
                if (Random.Range(0, 10) >= 5) //50% chance of Spinning Out
                    myStartType = StartType.WillSpin;
                break;

            case AIStupidity.Good:
                if(Random.Range(0, 10) >= 4) //40% chance of Boosting at Start
                    myStartType = StartType.WillBoost;
                break;

            case AIStupidity.Great:
                if(Random.Range(0, 10) >= 7)//70% chance of Boosting at Start
                    myStartType = StartType.WillBoost;
                break;

            case AIStupidity.Perfect://Will always of Boost at Start
                myStartType = StartType.WillBoost;
                break;
        }         
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Handles Start Boosting
        if (KartScript.startBoostVal != -1)
        {
            if ((myStartType == StartType.WillBoost && KartScript.startBoostVal <= 2) || (myStartType == StartType.WillSpin && KartScript.startBoostVal <= 3))
                ks.throttle = 1;

            if (KartScript.startBoostVal <= 1)
                canDrive = true;
        }

        if (canDrive && pf.closestPoint != null)
        {
            //Drive towards the nearest point
            if (aimPoint == null || pf.closestPoint == aimPoint || pf.currentPercent >= aimPoint.percent)
                ChooseNewPath();

            //Decide how to steer
            float angle = MathHelper.Angle(MathHelper.ZeroYPos(transform.forward), MathHelper.ZeroYPos(aimPoint.lastPos - transform.position));
            int angleSign = MathHelper.Sign(angle);
            ks.steer = (Mathf.Abs(angle) > requiredAngle) ? angleSign : 0f;

            //Decide how to throttle
            ks.throttle = 1f;

            //Reversing Behaviour
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 1.5f) && (hit.transform.GetComponent<Collider>() == null || !hit.transform.GetComponent<Collider>().isTrigger) 
                && hit.transform.tag != "Ground" && hit.transform.tag != "OffRoad" && hit.transform != transform)
            {
                Debug.Log("Ahh! I hit " + hit.transform.name);
                reversing = true;
            }

            Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.red);

            if(reversing)
            {
                ks.throttle = -1;
                ks.steer *= -1;

                if(Mathf.Abs(angle) < requiredAngle * 2f)
                    reversing = false;
            }
        }

        //Stop Driving if reach finish
        if (!td.loopedTrack && pf.currentPercent >= 1f)
        {
            canDrive = false;
            ks.throttle = 0f;
            ks.steer = 0f;
            ks.drift = false;
        }
    }

    void ChooseNewPath()
    {
        //If Answer is obvious
        if (td.loopedTrack && (pf.closestPoint.style == PointHandler.Point.End || pf.closestPoint.style == PointHandler.Point.Start))
        {
            aimPoint = pf.closestPoint.connections[0];

            for (int i = 1; i < pf.closestPoint.connections.Count; i++)
            {
                if (pf.closestPoint.connections[i].percent < aimPoint.percent)
                    aimPoint = pf.closestPoint.connections[i];
            }          
        }
        else
        {
            int count = 0;
            bool done = false;

            PointHandler target = pf.closestPoint;

            do
            {
                List<PointHandler> validPaths = new List<PointHandler>(target.connections);

                foreach (PointHandler ph in validPaths.ToArray())
                {
                    if (ph.percent <= pf.closestPoint.percent) //If Point goes back remove it
                        validPaths.Remove(ph);
                    else if (ph.style == PointHandler.Point.Shortcut) //If point requires a boost
                        validPaths.Remove(ph);
                }

                //If there's nowhere to go, break
                if (validPaths.Count == 0)
                    return;

                //If Shortcut and have item choose shortcut if smart enough
                aimPoint = validPaths[Random.Range(0, validPaths.Count)];
                count++;

                if (aimPoint.percent < pf.currentPercent)
                    target = aimPoint;
                else
                    done = true;

                if (count > 5)
                    Debug.Log("AHHHH CRASH!");

                if (count > 99)
                    return;

            } while (!done);
        }
    }
}
