using UnityEngine;
using System.Collections;

public class RacerAI : MonoBehaviour
{

    //Straight Port of JS Version
    private kartScript ks;
    private TrackData td;
    private PositionFinding pf;

    public int stupidity = -1; //Bigger the number, stupider the AI.

    private float angleRequired = 3f;
    private float turnSpeed = 15f;
    private float turnAngle = 30f;

    private float reverseDistance = 10f;

    private Transform turnPoint;
    private int turnPointInt;

    private bool reversing = false, startDrive = true;

    public float adjusterFloat = -999f;

    // Use this for initialization
    void Awake()
    {
        ks = GetComponent<kartScript>();
        td = GameObject.FindObjectOfType<TrackData>();
        pf = GetComponent<PositionFinding>();

        if (ks.locked)
            startDrive = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Wait till positions are set to create Adjuster
        if (adjusterFloat <= -999 && pf.position != -1)
        {
            adjusterFloat = 5 - ((pf.position % 3) * 5);
        }

        int currentPos = pf.currentPos;
        Vector3 transformPos = transform.position;
        transformPos.y = 0;

        int pointTotal = td.positionPoints.Count;

        Vector3 nextPoint = td.positionPoints[MathHelper.NumClamp(currentPos + 1, 0, pointTotal)].position;
        nextPoint.y = 0;

        Vector3 nextNextPoint = td.positionPoints[MathHelper.NumClamp(currentPos + 2, 0, pointTotal)].position;
        nextNextPoint.y = 0;

        Vector3 currentPoint = td.positionPoints[currentPos].position;
        currentPoint.y = 0;

        Vector3 desiredDirection = nextPoint - currentPoint;
        desiredDirection.y = 0;

        Vector3 fireDirection = transform.right;
        fireDirection.y = 0;

        float angle = Vector3.Angle(fireDirection, desiredDirection);
        int turnRequired = CheckAngle(angle);

        if (startDrive)
            ks.throttle = 1f;
        else
            ks.throttle = 0f;

        if (turnPointInt == currentPos)
        {
            turnPoint = null;
        }

        if (turnPoint == null)
        {

            turnPoint = td.positionPoints[MathHelper.NumClamp(currentPos + 1, 0, pointTotal)];
            turnPointInt = MathHelper.NumClamp(currentPos + 1, 0, pointTotal);

            if (adjusterFloat > -999)
            {
                adjusterFloat += Random.Range(-1f, 1f);
                float limit = turnPoint.GetComponent<PointHandler>().roadWidth;
                adjusterFloat = Mathf.Clamp(adjusterFloat, -limit, limit);
            }

        }
        else
        {
            var tpDistance = Vector3.Distance(transformPos, turnPoint.position);

            //If turn within turn range start to turn
            if (tpDistance < 7.5f && !reversing)
            {
                SlowDownCar(Vector3.Angle(transform.forward, desiredDirection));
                Debug.DrawRay(transform.position, desiredDirection, Color.green);
            }
            else
            {
                //If not within range of next point, aim kart towards the point	
                var Adjuster = Vector3.Cross((turnPoint.position - currentPoint).normalized, transform.up) * adjusterFloat;
                Debug.DrawLine(transform.position, turnPoint.position + Adjuster, Color.green);

                Vector3 NeededDirection = (turnPoint.position + Adjuster) - transformPos;
                NeededDirection.y = 0;

                float nAngle = Vector3.Angle(fireDirection, NeededDirection);
                int nTurnRequired = CheckAngle(nAngle);

                turnRequired = nTurnRequired;
                SlowDownCar(Vector3.Angle(transform.forward, NeededDirection));
            }
        }

        //Reverse if Kart hits somethings
        if (ks.ExpectedSpeed > ks.maxSpeed/2f && Mathf.Abs(ks.ActualSpeed) < 1) //Presume something is blocking the kart.
            reversing = true;

        if (reversing)
        {
            turnRequired = (int)Mathf.Sign(-turnRequired);
            ks.throttle = -1;

            var checkPos = transform.position + Vector3.up;

            if (!Physics.Raycast(checkPos, transform.forward, reverseDistance) && !Physics.Raycast(checkPos + transform.right, transform.forward, reverseDistance) && !Physics.Raycast(checkPos - transform.right, transform.forward, reverseDistance))
                reversing = false;
        }

        //Handles Start Boosting
        if (kartScript.startBoostVal != -1)
        {
            if ((stupidity < 4 && kartScript.startBoostVal <= 2) || (stupidity > 8 && kartScript.startBoostVal <= 3))
                ks.throttle = 1;

            if (kartScript.startBoostVal <= 1)
                startDrive = true;

        }

        ks.steer = turnRequired;

    }

    void SlowDownCar(float angle)
    {
        if (angle > turnAngle)
        {
            //Slow down to make turn
            if (stupidity < 5f)
            {
                if (ks.ActualSpeed >= turnSpeed)
                    ks.throttle = 0;
            }
            else
            {
                ks.drift = true;
            }
        }
        else
        {
            ks.drift = false;
        }
    }

    int CheckAngle(float angle)
    {
        if (angle > 90 + angleRequired)
            return -1;
        else if (angle < 90 - angleRequired)
            return 1;
        else
            return 0;
    }

}
