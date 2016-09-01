﻿using UnityEngine;
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

    private int currentNode = 0, nextNode = 1, nextnextNode = 2, currentPercent = 0;
    private const float maxXDistance = 2f, minAngle = 3f;

    public enum PathCorrecting { Fine, Turning, Straightening };
    public PathCorrecting pc = PathCorrecting.Fine;

    public float angle = 0f, nextAngle = 0f;

    // Use this for initialization
    void Start()
    {
        td = FindObjectOfType<TrackData>();
        pf = FindObjectOfType<PositionFinding>();

        AnalyseTrack();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, td.positionPoints[currentNode].position, Color.green);

        if (percent > 1)
        {
            currentNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            nextnextNode = MathHelper.NumClamp(currentNode + 2, 0, td.positionPoints.Count);

            currentPercent = 0;
        }
        if (percent < 0)
        {
            currentNode = MathHelper.NumClamp(currentNode - 1, 0, td.positionPoints.Count);
            nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            nextnextNode = MathHelper.NumClamp(currentNode + 2, 0, td.positionPoints.Count);

            currentPercent = 0;
        }

        Vector3 startPos = td.positionPoints[currentNode].position, endPos = td.positionPoints[nextNode].position;
        Vector3 vecBetweenPoints = (endPos - startPos);

        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(vecBetweenPoints), Vector3.one).inverse;

        localPos = matrix * (transform.position - startPos);
        percent = localPos.z / vecBetweenPoints.magnitude;

        kartScript ks = GetComponent<kartScript>();

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
            if (percent >= AITrackInfo[currentNode][currentPercent].percent)
            {
                ks.steer = AITrackInfo[currentNode][currentPercent].turnAmount;

                if (ks.steer != 0)
                    pc = PathCorrecting.Fine;

                if ((ks.steer > 0 && localPos.x > maxXDistance * 2f) || (ks.steer < 0 && localPos.x < -maxXDistance * 2f))
                    pc = PathCorrecting.Turning;

                float nExpectedSpeed = Mathf.Lerp(ks.maxSpeed, (ks.driftStarted) ? ks.maxSpeed : 17f, 10 / pathLengths[currentNode]);
                if (ks.expectedSpeed > nExpectedSpeed)
                    ks.throttle = 0;
                else
                    ks.throttle = 1;

                if (AITrackInfo[currentNode][currentPercent].drift)
                {
                    if (MathHelper.Sign(ks.steer) == MathHelper.Sign(angles[currentNode]))
                        ks.drift = true;
                }
                else
                    ks.drift = false;

            }
            else if (Mathf.Abs(angle) < minAngle)
            {
                ks.steer = 0;
                ks.drift = false;
            }
            else
            {
                ks.drift = false;
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
            if (!ks.drift && (Mathf.Abs(localPos.x) > maxXDistance || ks.steer == 0 || pc != PathCorrecting.Fine))
            {
                switch (pc)
                {
                    case PathCorrecting.Fine:
                        //If we're too far from the line, everything is not okay!
                        if (Mathf.Abs(localPos.x) > maxXDistance || Mathf.Abs(angle) > minAngle)
                            pc = PathCorrecting.Turning;
                        break;
                    case PathCorrecting.Turning:

                        float aimPoint = percent + 0.2f;

                        float offset = Mathf.Abs(angles[currentNode]) >= turnAngleRequired ? MathHelper.Sign(angles[currentNode]) : 0f * 4f;

                        Vector3 rightVec = Vector3.zero;
                        if (offset != 0f)
                            rightVec = (Quaternion.AngleAxis(90, Vector3.up) * vecBetweenPoints).normalized;

                        Vector3 attackPoint = startPos + (vecBetweenPoints * aimPoint) + (rightVec * offset);
                        Debug.DrawLine(transform.position, attackPoint, Color.yellow);

                        Vector3 attackFoward = attackPoint - transform.position, myForward = transform.forward;
                        attackFoward.y = 0;
                        myForward.y = 0;

                        if (Mathf.Abs(localPos.x) < maxXDistance)
                            pc = PathCorrecting.Straightening;

                        float nAngle = MathHelper.Angle(myForward, attackFoward);
                        if (nAngle > minAngle)
                            ks.steer = 1;
                        else if (nAngle < -minAngle)
                            ks.steer = -1;
                        else if (Mathf.Abs(angle) < minAngle)
                            pc = PathCorrecting.Straightening;
                        break;
                    case PathCorrecting.Straightening:
                        if (angle > minAngle)
                            ks.steer = -1;
                        else if (angle < -minAngle)
                            ks.steer = 1;
                        else
                        {
                            ks.steer = 0;
                            pc = PathCorrecting.Fine;
                        }

                        break;
                }
            }

        }
        else
        {
            //We're going the wrong way
            ks.throttle = 1;
        }
    }

    const float turnAngleRequired = 10f, roadNeededtoStraightenOut = 15f;

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
            int nextPoint = MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count);
            int nextNextPoint = MathHelper.NumClamp(i + 2, 0, td.positionPoints.Count);

            Vector3 currentDir = td.positionPoints[nextPoint].position - td.positionPoints[i].position;
            Vector3 nextDir = td.positionPoints[nextNextPoint].position - td.positionPoints[nextPoint].position;

            angles[i] = MathHelper.Angle(currentDir, nextDir);
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
