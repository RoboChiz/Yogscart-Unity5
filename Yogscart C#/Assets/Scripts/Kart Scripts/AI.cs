using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI : MonoBehaviour
{    /// <summary>
     /// Stupid - Basic driving, slows down to turn
     /// Bad - Drives, dosen't slow down at turn. Dosen't startline boost or drift
     /// Good - May Boost at Start Line, Drifts some times
     /// Great - Start Line boosts most of the time, drifts, and looks for short cuts
     /// Perfect - Races as perfectly as it can, boosts at start, drifts asap
     /// </summary>
    public enum AIStupidity { Stupid, Bad, Good, Great, Perfect };
    public AIStupidity intelligence = AIStupidity.Stupid;

    private TrackData td;
    private PositionFinding pf;

    public Vector3 localPos;
    public float percent;

    private List<List<TrackRoadInfo>> AITrackInfo;
    private List<float> pathLengths;
    private float[] angles;

    private const float maxXDistance = 2f, minAngle = 3f;

    public enum PathCorrecting { Fine, Turning, Straightening };
    public PathCorrecting pc = PathCorrecting.Fine;

    public enum StartType { WillBoost, WontBoost, WillSpin };
    public StartType myStartType;

    public float angle = 0f, nextAngle = 0f;
    private bool canDrift = true;
    public bool canDrive = true;

    // Use this for initialization
    void Start()
    {
        td = FindObjectOfType<TrackData>();
        pf = FindObjectOfType<PositionFinding>();

        AnalyseTrack();

        //Decide my Start Boost
        myStartType = StartType.WontBoost;
        if (intelligence == AIStupidity.Stupid) //Will always Spin Out
            myStartType = StartType.WillSpin;
        if (intelligence == AIStupidity.Bad && Random.Range(0, 10) >= 5) //50% chance of Spinning Out
            myStartType = StartType.WillSpin;
        if (intelligence == AIStupidity.Good && Random.Range(0, 10) >= 4) //40% chance of Boosting at Start
            myStartType = StartType.WillBoost;
        if (intelligence == AIStupidity.Great && Random.Range(0, 10) >= 7)//70% chance of Boosting at Start
            myStartType = StartType.WillBoost;
        if (intelligence == AIStupidity.Perfect)//Will always of Boost at Start
            myStartType = StartType.WillBoost;
    }

    // Update is called once per frame
    void Update()
    {
        kartScript ks = GetComponent<kartScript>();

        //Handles Start Boosting
        if (kartScript.startBoostVal != -1)
        {
            if ((myStartType == StartType.WillBoost && kartScript.startBoostVal <= 2) || (myStartType == StartType.WillSpin && kartScript.startBoostVal <= 3))
                ks.throttle = 1;

            if (kartScript.startBoostVal <= 1)
                canDrive = true;

        }

        if (canDrive)
        {          
            int currentNode = pf.currentPos;
            int nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            int nextnextNode = MathHelper.NumClamp(currentNode + 2, 0, td.positionPoints.Count);
            int currentPercent = 0;

            Debug.DrawLine(transform.position, td.positionPoints[currentNode].position, Color.green);

            Vector3 startPos = td.positionPoints[currentNode].position, endPos = td.positionPoints[nextNode].position;
            Vector3 vecBetweenPoints = (endPos - startPos);

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(vecBetweenPoints), Vector3.one).inverse;

            localPos = matrix * (transform.position - startPos);
            percent = localPos.z / vecBetweenPoints.magnitude;

            //Do Driving
            angle = -MathHelper.Angle(transform.forward, vecBetweenPoints);
            nextAngle = -MathHelper.Angle(transform.forward, (td.positionPoints[nextnextNode].position - td.positionPoints[nextNode].position));

            if (Mathf.Abs(angle) < 45)
            {
                //We're going the right way
                ks.throttle = 1f;

                //Find the current Percent
                for (int i = 0; i < AITrackInfo[currentNode].Count; i++)
                {
                    if (AITrackInfo[currentNode][i].percent < percent)
                    {
                        currentPercent = i;
                    }
                }

                //On the right path, follow the track analysis
                if (AITrackInfo[currentNode].Count > 0 && percent >= AITrackInfo[currentNode][currentPercent].percent)
                {
                    ks.steer = AITrackInfo[currentNode][currentPercent].turnAmount;

                    if (ks.steer != 0)
                        pc = PathCorrecting.Fine;


                    float nExpectedSpeed = Mathf.Lerp(ks.maxSpeed, (ks.driftStarted) ? ks.maxSpeed : 17f, 10 / pathLengths[currentNode]);
                    if (ks.expectedSpeed > nExpectedSpeed)
                        ks.throttle = 0;
                    else
                        ks.throttle = 1;

                    if (AITrackInfo[currentNode][currentPercent].drift)
                    {
                        if (MathHelper.Sign(ks.steer) == MathHelper.Sign(angles[currentNode]))
                        {
                            if (!canDrift)
                            {
                                canDrift = true;
                                ShouldDoDrift();
                            }
                        }
                    }
                    else
                    {
                        ks.drift = false;
                        canDrift = false;
                    }

                }
                else if (Mathf.Abs(angle) < minAngle)
                {
                    ks.steer = 0;
                    ks.drift = false;
                    canDrift = false;
                }
                else
                {
                    ks.drift = false;
                    canDrift = false;
                }

                if (ks.driftStarted && ks.driftSteer != 0)
                {
                    pc = PathCorrecting.Fine;
                    float tempMinAngle = 30f, tempMaxAngle = 10f, minX = 4f, maxX = 5f;

                    //We're drifting right
                    if (ks.driftSteer > 0)
                    {
                        //Stay on the right side of the path
                        if ((localPos.x < minX && angle < tempMinAngle) || angle < -minAngle || (percent > 0.5f && nextAngle < -minAngle))
                            ks.steer = 1;
                        else if ((localPos.x > maxX && angle > -tempMaxAngle) || angle > minAngle)
                            ks.steer = -1;
                        else
                            ks.steer = 0;
                    }
                    else //We're drifting Left
                    {
                        //Stay on the left side of the path
                        if ((localPos.x > -minX && angle > -tempMinAngle) || angle > minAngle || (percent > 0.5f && nextAngle > minAngle))
                            ks.steer = -1;
                        else if ((localPos.x < -maxX && angle < tempMaxAngle) || angle < -minAngle)
                            ks.steer = 1;
                        else
                            ks.steer = 0;
                    }
                }

                //Straighten Up if you need to
                if(angle > turnAngleRequired)
                {
                    ks.steer = Mathf.Sign(angle);
                }

            }
            else
            {
                //We're going the wrong way
                ks.throttle = 1;
            }
        }
    }

    private void ShouldDoDrift()
    {
        kartScript ks = GetComponent<kartScript>();

        //Calculate if the kart should do a drift
        switch (intelligence)
        {
            case AIStupidity.Perfect:
                ks.drift = true;
                break;
            case AIStupidity.Great:
                int randomNum = Random.Range(0, 100);
                //Debug.Log("Random:" + randomNum);
                if (randomNum < 75) //75% chance of drifting
                    ks.drift = true;
                break;
            case AIStupidity.Good:
                randomNum = Random.Range(0, 100);
                //Debug.Log("Random:" + randomNum);
                if (randomNum < 50) //50% chance of drifting
                    ks.drift = true;
                break;
            case AIStupidity.Bad:
                randomNum = Random.Range(0, 100);
                //Debug.Log("Random:" + randomNum);
                if (randomNum < 25) //25% chance of drifting
                    ks.drift = true;
                break;
            case AIStupidity.Stupid:
                ks.drift = false;
                break;
        }
    }

    const float turnAngleRequired = 5f, roadNeededtoStraightenOut = 15f;

    /// <summary>
    /// Scan the entire track and find places to turn .etc
    /// </summary>
    private void AnalyseTrack()
    {
        //Only perform the scan once, as it is static all AI can access it
        if (AITrackInfo == null || AITrackInfo.Count == 0)
        {
            AITrackInfo = new List<List<TrackRoadInfo>>();
        }

        /*
        NEW AI DESIGN
        ----------------------------------------------
        Fill array of angles for each next path

        Then for each position point
            Check behind me, was I turning last 
            If so
                Look ahead, will I need to turn
                    If So
                        Will I turn in the same direction
                            If So
                                How Long is this path? Do I have time to straighten out?
                                If So
                                    Set Steer to zero at 20%
                                    Set Steer to old turn direction at 80%
                                    DONE!
                                If Not
                                    Add turn at 0%
                                    DONE!
                            If Not
                                Set Steer to zero at 20%
                                Set Steer to new turn direction at 80%
                                DONE!
                    If Not
                        Set Steer to zero at 20%
                        DONE!
            If Not
                Look ahead, will I need to turn
                    If So
                        Set Steer to new turn direction at 80%
                        DONE!
                    If Not
                        DONE!
        */

        //Find out the angles and lengths of all the paths
        angles = new float[td.positionPoints.Count];
        pathLengths = new List<float>();

        for (int i = 0; i < td.positionPoints.Count; i++)
        {
            angles[i] = GetAngle(i, i + 1, i + 2);

            int nextPoint = MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count);
            Vector3 currentDir = td.positionPoints[nextPoint].position - td.positionPoints[i].position;

            pathLengths.Add(currentDir.magnitude);
        }

        //Do Behaviour
        for (int i = 0; i < td.positionPoints.Count; i++)
        {
            List<TrackRoadInfo> newTrackList = new List<TrackRoadInfo>();
            float percentChunk = 15f / pathLengths[i];

            //Check behind me, was I turning last
            int lastPoint = MathHelper.NumClamp(i - 1, 0, td.positionPoints.Count);
            if (Mathf.Abs(angles[lastPoint]) > turnAngleRequired)
            {
                // Look ahead, will I need to turn
                if (Mathf.Abs(angles[i]) > turnAngleRequired)
                {
                    //Will I turn in the same direction
                    if (MathHelper.Sign(angles[lastPoint]) == MathHelper.Sign(angles[i]))
                    {
                        //How Long is this path? Do I have time to straighten out?
                        if (pathLengths[i] >= roadNeededtoStraightenOut)
                        {
                            // Set Steer to zero at 20%
                            newTrackList.Add(new TrackRoadInfo(percentChunk, 0, false));
                            // Set Steer to old turn direction at 80 %
                            newTrackList.Add(new TrackRoadInfo(1 - percentChunk, MathHelper.Sign(angles[i]), true));
                        }
                        else
                        {
                            // Add turn at 0%
                            newTrackList.Add(new TrackRoadInfo(0, MathHelper.Sign(angles[i]), true));
                        }
                    }
                    else
                    {
                        // Set Steer to zero at 20%
                        newTrackList.Add(new TrackRoadInfo(percentChunk, 0, false));
                        //Set Steer to new turn direction at 80 %
                        newTrackList.Add(new TrackRoadInfo(1 - percentChunk, MathHelper.Sign(angles[i]), true));
                    }
                }
                else
                {
                    // Set Steer to zero at 20%
                    newTrackList.Add(new TrackRoadInfo(percentChunk, 0, false));
                }
            }
            else
            {
                // Look ahead, will I need to turn
                if (Mathf.Abs(angles[i]) > turnAngleRequired)
                {
                    // Set Steer to new turn direction at 80 %
                    newTrackList.Add(new TrackRoadInfo(1 - percentChunk, MathHelper.Sign(angles[i]), true));
                }
            }

            AITrackInfo.Add(newTrackList);
        }
    }

    private float GetAngle(int i, int iPlusOne, int iPlusTwo)
    {
        int nextPoint = MathHelper.NumClamp(iPlusOne, 0, td.positionPoints.Count);
        int nextNextPoint = MathHelper.NumClamp(iPlusTwo, 0, td.positionPoints.Count);

        Vector3 currentDir = td.positionPoints[nextPoint].position - td.positionPoints[i].position;
        Vector3 nextDir = td.positionPoints[nextNextPoint].position - td.positionPoints[nextPoint].position;

        return MathHelper.Angle(currentDir, nextDir);
    }

    private class TrackRoadInfo
    {
        public float percent { get; private set; }
        //0 if no turn is needed
        public float turnAmount { get; private set; }
        //This turn is started before the end of a straight path
        public bool drift { get; private set; }

        public TrackRoadInfo(float _percent, float _turnAmount, bool _drift)
        {
            percent = _percent;
            turnAmount = _turnAmount;
            drift = _drift;
        }
    }
}