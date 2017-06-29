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

    private int currentNode = 0, nextNode = 1, nextNextNode = 2;
    private const float maxXDistance = 1f, minAngle = 3f;

    public enum StartType { WillBoost, WontBoost, WillSpin };
    public StartType myStartType;

    public enum DriveState { Centring, Turning, DriftCentring, Fixing, Nothing };
    public DriveState driveState = DriveState.Centring;

    public enum DriftThought { None, Gonna, NotGonna, Drifting };
    public DriftThought driftThought;

    public bool canDrive = true;

    public Vector3 localPos;
    public float percent = -1;
    private bool reversing = false;

    private static List<List<TrackRoadInfo>> AITrackInfo;
    private List<float> pathLengths;
    private float[] angles;

    // Use this for initialization
    IEnumerator Start()
    {
        td = FindObjectOfType<TrackData>();

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

        yield return new WaitForEndOfFrame();

        currentNode = 0;

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

        /*
        if (canDrive)
        {           
            CalculatePercent();

            //If we are ahead of the next node or behind the current node make adjustments
            if (percent > 1)
                currentNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            if (percent < 0)
                currentNode = MathHelper.NumClamp(currentNode - 1, 0, td.positionPoints.Count);

            //Find the next two nodes ahead of us
            nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
            nextNextNode = MathHelper.NumClamp(currentNode + 2, 0, td.positionPoints.Count);

            //Get the location of our nodes
            Vector3 currentNodePos = td.positionPoints[currentNode].position;
            Vector3 nextNodePos = td.positionPoints[nextNode].position;
            Vector3 nextNextNodePos = td.positionPoints[nextNextNode].position;

            Vector3 finalDir = nextNodePos - currentNodePos;

            Vector3 nextDir = nextNextNodePos - nextNodePos; 

            Vector3 myForward = Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)).normalized;

            //Angle between kart direction and final direction
            float finalDirAngle = -MathHelper.Angle(myForward, finalDir);
            float nextFinalDirAngle = -MathHelper.Angle(myForward, nextDir);

            //Get Current Instruction
            int currentInstruction = -1;
            for (int i = 0; i < AITrackInfo[currentNode].Count; i++)
            {
                if (localPos.z > AITrackInfo[currentNode][i].distance || AITrackInfo[currentNode][i].distance <= 0)
                    currentInstruction = i;
            }

            bool doNothing = true;

            ks.throttle = 1f;

            //Follow Current Instruction
            if (currentInstruction != -1f)
            {
                TrackRoadInfo instruction = AITrackInfo[currentNode][currentInstruction];

                //If Angle is not what we need to be follow instructions
                float angle = MathHelper.Angle(myForward, instruction.finalDir);

                if (Mathf.Abs(angle) > minAngle && Mathf.Sign(angle) == Mathf.Sign(instruction.turnAmount))
                {
                    ks.steer = instruction.turnAmount;
                    driveState = DriveState.Turning;
                    doNothing = false;

                    //Slow down if you're not gonna make it
                    if ((!ks.drift || driftThought == DriftThought.None || driftThought == DriftThought.NotGonna) && ((ks.steer < 0 && localPos.x > maxXDistance) || (ks.steer > 0 && localPos.x < -maxXDistance)))
                    {
                        if (intelligence >= AIStupidity.Great)
                        {
                            //Drift round corners
                            ks.drift = true;
                            driftThought = DriftThought.Drifting;
                        }
                        else
                        {
                            //Slow down to do corners
                            ks.throttle = 0.5f;
                        }
                    }
                }

                //Drifting 2.0            
                if (instruction.drift != DriftType.NoDrift && driftThought == DriftThought.Gonna && MathHelper.Sign(ks.steer) == Mathf.Sign(instruction.turnAmount))
                {
                    driftThought = DriftThought.Drifting;
                    ks.drift = true;
                }

                //If we're drifting but in the wrong direction
                if (driftThought == DriftThought.Drifting && MathHelper.Sign(ks.driftSteer) != MathHelper.Sign(instruction.turnAmount))
                    driftThought = DriftThought.None;

                //Do Drift Centring
                if (ks.drift && ks.driftSteer != 0)
                {
                    driveState = DriveState.DriftCentring;
                    doNothing = false;

                    float maxDistance = 5f;

                    //Get Target X Pos we should be drifting at
                    float centringOffset = 0f; ;
                    switch (instruction.drift)
                    {
                        case DriftType.Close: centringOffset += instruction.turnAmount * 2f; break;
                        case DriftType.Wide: centringOffset -= instruction.turnAmount * 2f; break;
                    }

                    //Stick to the centring Offset
                    if (localPos.x < centringOffset) //If we are on the left side of our goal, steer right
                        ks.steer = 1;
                    else if (localPos.x > centringOffset) //If we are on the right side of our goal, steer left
                        ks.steer = -1;

                    //Cancel Drift if we go too far from the goal
                    if (Mathf.Abs(centringOffset - localPos.x) > maxDistance)
                        driftThought = DriftThought.None;

                    //If we're drifting right and oversteer right
                    if (ks.driftSteer > 0 && nextFinalDirAngle > minAngle * 5f)
                        driftThought = DriftThought.None;

                    //If we're drifting left and oversteer left
                    if (ks.driftSteer < 0 && nextFinalDirAngle < -minAngle * 5f)
                        driftThought = DriftThought.None;
                }

            }

            //Should Drift
            if (AITrackInfo[nextNode].Count > 0)
            {
                //Calculate if Drifting will have to Drift
                TrackRoadInfo nextInstruction = AITrackInfo[nextNode][0];

                if (nextInstruction.drift != DriftType.NoDrift && driftThought == DriftThought.None)
                {
                    if (ShouldDoDrift())
                        driftThought = DriftThought.Gonna;
                    else
                        driftThought = DriftThought.NotGonna;
                }

                //Cancel Drift
                //if (nextInstruction.drift == DriftType.NoDrift)
                // driftThought = DriftThought.None;

            }
            else
            {
                // driftThought = DriftThought.None;
            }


            if (!ks.drift)
            {

                /*float centringOffset = 0f;

                //Adjust Centreing for Drift if we're gonna drift
                if (AITrackInfo[nextNode].Count > 0 && driftThought == DriftThought.Gonna)
                {
                    TrackRoadInfo nextInstruction = AITrackInfo[nextNode][0];
                    switch (nextInstruction.drift)
                    {
                        case DriftType.Normal: centringOffset -= nextInstruction.turnAmount * 2f; break;
                        case DriftType.Wide: centringOffset -= nextInstruction.turnAmount * 4f; break;
                    }
                }

                //Do Straightening
                int straightenTurn = 0;

                //Turn towards Next Point if we are far away from
                if (Mathf.Abs(localPos.x) > maxXDistance || (driveState != DriveState.Turning && Mathf.Abs(finalDirAngle) > minAngle))
                {
                    //Centring Stuff
                    Vector3 dir = (nextNodePos - currentNodePos);
                    float percentAmount = percent + (10f / dir.magnitude);

                    //If Percent goes over current node use the next ones
                    percentAmount = Mathf.Clamp(percentAmount, 0f, 1f);

                    Vector3 offset = dir * percentAmount;

                    Vector3 desiredDirection = Vector3.Scale((currentNodePos + offset) - transform.position, new Vector3(1f, 0f, 1f));
                    float nextDirAngle = MathHelper.Angle(myForward, desiredDirection);

                    bool canStraighten = (Mathf.Abs(nextDirAngle) < 45f || reversing || ks.actualSpeed < 15f || (ks.steer == -1 && nextDirAngle > -(minAngle/2f)) || (ks.steer == 1 && nextDirAngle < (minAngle / 2f)));

                    Debug.DrawRay(transform.position, desiredDirection, canStraighten ? Color.yellow : Color.red);

                    if (nextDirAngle > 0 && canStraighten)
                        straightenTurn = 1;
                    else if (nextDirAngle < 0 && canStraighten)
                        straightenTurn = -1;
                    else
                        straightenTurn = 0;

                }

                //If Straighten turn exact opposite of current turn then don't follow through
                //If doNothing or if current turn is 0 or if current turn is same as Straighten Turn then follow through
                //If past left minXDistance and told to turn left, then allowed to straighten
                if (straightenTurn != 0 && !reversing && (doNothing || ks.steer == 0))
                {
                    ks.steer = straightenTurn;
                    driveState = DriveState.Centring;
                    doNothing = false;
                }
            }

            //No Instructions to follow, so just drive straight
            if (doNothing)
            {
                ks.throttle = 1f;
                ks.steer = 0f;
                //driftThought = DriftThought.None;

                driveState = DriveState.Nothing;
            }

            bool canFinishBlue = (ks.blueTime - ks.driftTime < 0.4f), canFinishOrange = (ks.orangeTime - ks.driftTime < 0.3f);
            bool blueFinished = ks.driftTime >= ks.blueTime, orangeFinished = ks.driftTime >= ks.orangeTime;

            if (driftThought == DriftThought.None)
            {
                //Debug.Log("canFinishBlue:" + canFinishBlue + " blueFinished:" + blueFinished);
                if ((!canFinishBlue || blueFinished) && (!canFinishOrange || orangeFinished))
                {
                    // if(ks.driftTime > 0f)
                    //Debug.Log("Cancelled Drift! Drift Time:" + ks.driftTime);

                    ks.drift = false;
                }

            }

            //Reverse if Driving into a Wall
            RaycastHit raycastHit;
            Color hitColor = Color.red;

            float distance = 3f;

            if (reversing)
                distance = 10f;

            Vector3 startPos = transform.position + (Vector3.up * 0.5f);
            int layerMask = ~((1 << 8) | (1 << 9) | (1 << 10));

            if (Physics.Raycast(startPos, transform.forward, out raycastHit, distance, layerMask) ||
                Physics.Raycast(startPos + (transform.right / 2f), transform.forward, out raycastHit, distance, layerMask) ||
                Physics.Raycast(startPos - (transform.right / 2f), transform.forward, out raycastHit, distance, layerMask))
            {
                driveState = DriveState.Fixing;
                reversing = true;

                ks.throttle = -1f;

                Vector3 dir = (nextNodePos - currentNodePos);
                Vector3 offset = dir * (percent + (10f / dir.magnitude));

                float newSteer = MathHelper.Angle(
                    Vector3.Scale(currentNodePos + offset, new Vector3(1f, 0f, 1f)),
                    Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)));
                ks.steer = Mathf.Sign(newSteer);

                Debug.DrawLine(startPos, currentNodePos + offset, Color.blue);
                hitColor = Color.green;

                driftThought = DriftThought.None;
                ks.drift = false;
            }
            else if (distance == 10f && reversing)
            {
                reversing = false;
            }

            Debug.DrawRay(startPos, transform.forward * distance, hitColor);
            Debug.DrawRay(startPos + (transform.right / 2f), transform.forward * distance, hitColor);
            Debug.DrawRay(startPos - (transform.right / 2f), transform.forward * distance, hitColor);
        }
*/
    }

   /* private void CalculatePercent()
    {
        Vector3 currentNodePos = td.positionPoints[currentNode].transform.position;
        Vector3 nextNodePos = td.positionPoints[MathHelper.NumClamp(currentNode + 1,0,td.positionPoints.Count)].transform.position;
        Vector3 finalDir = nextNodePos - currentNodePos;

        //Find out where we are relative to our current and next node
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(finalDir), Vector3.one).inverse;
        localPos = matrix * (transform.position - currentNodePos);
        percent = localPos.z / finalDir.magnitude;
    }

    private bool ShouldDoDrift()
    {

        //Calculate if the kart should do a drift
        switch (intelligence)
        {
            case AIStupidity.Perfect:
                return true;
            case AIStupidity.Great:
                int randomNum = Random.Range(0, 100);
                if (randomNum < 75) //75% chance of drifting
                    return true;
                break;
            case AIStupidity.Good:
                randomNum = Random.Range(0, 100);
                if (randomNum < 50) //50% chance of drifting
                    return true;
                break;
            case AIStupidity.Bad:
                randomNum = Random.Range(0, 100);
                if (randomNum < 25) //25% chance of drifting
                    return true;
                break;
        }

        return false;
    }*/

    const float turnAngleRequired = 5f, roadNeededtoStraightenOut = 6f;
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
        

        //Find out the angles and lengths of all the paths
        angles = new float[td.positionPoints.Count];
        Vector3[] directions = new Vector3[td.positionPoints.Count];
        pathLengths = new List<float>();

        for (int i = 0; i < td.positionPoints.Count; i++)
        {
            int nextPoint = MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count);
            int nextNextPoint = MathHelper.NumClamp(i + 2, 0, td.positionPoints.Count);

            Vector3 currentPos = Vector3.Scale(td.positionPoints[i].position, new Vector3(1f, 0f, 1f));
            Vector3 nextPos = Vector3.Scale(td.positionPoints[nextPoint].position, new Vector3(1f, 0f, 1f));
            Vector3 nextnextPos = Vector3.Scale(td.positionPoints[nextNextPoint].position, new Vector3(1f, 0f, 1f));

            Vector3 currentDir = nextPos - currentPos;
            Vector3 nextDir = nextnextPos - nextPos;

            angles[i] = MathHelper.Angle(currentDir, nextDir);
            directions[i] = currentDir;

            pathLengths.Add(currentDir.magnitude);

            //FindObjectOfType<InEngineRender>().roadColours.Add(Color.red);
        }

        //Find Driftable Areas
        DriftType[] driftable = new DriftType[angles.Length];
        float angleNeeded = 15f;

        for (int driftCount = 0; driftCount < td.positionPoints.Count; driftCount++)
        {
            if (Mathf.Abs(angles[driftCount]) > minAngle && pathLengths[driftCount] < 15f)
            {
                float totalAngle = angles[driftCount];
                int currentNode = driftCount;
                //Find the total angle of the Turn
                while (true)
                {
                    currentNode = currentNode + 1;

                    //Stop looking when turn changes direction or the change in angle is too small or if the Road straightens out..
                    if (currentNode >= angles.Length || MathHelper.Sign(angles[currentNode]) != MathHelper.Sign(totalAngle) || Mathf.Abs(angles[currentNode]) < minAngle || pathLengths[driftCount] >= roadNeededtoStraightenOut * 2f)
                        break;

                    //Otherwise add the sum of the angles
                    totalAngle += angles[currentNode];
                }

                //If the Turn is bigger than the required Angle
                if (Mathf.Abs(totalAngle) > angleNeeded)
                {
                    //Change the driftable for this Node and all Nodes included
                    for (int j = driftCount; j <= currentNode; j++)
                    {
                        // if(j == currentNode)
                        //    driftable[j] = DriftType.Normal;
                        if (Mathf.Abs(angles[j]) > 20f && pathLengths[j] < 8f)
                        {
                            driftable[j] = DriftType.Close;
                          //  FindObjectOfType<InEngineRender>().roadColours[j] = Color.cyan;
                        }
                        else if (Mathf.Abs(angles[j]) < 14f)
                        {
                            driftable[j] = DriftType.Wide;
                           // FindObjectOfType<InEngineRender>().roadColours[j] = Color.green;
                        }
                        else
                        {
                            driftable[j] = DriftType.Normal;
                            //FindObjectOfType<InEngineRender>().roadColours[j] = Color.yellow;
                        }

                    }
                }

                //Skip ahead as we've checked up to this Node
                driftCount = currentNode - 1;
            }
        }

        //Do Behaviour
        for (int i = 0; i < td.positionPoints.Count; i++)
        {
            List<TrackRoadInfo> newTrackList = new List<TrackRoadInfo>();

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
                        if (pathLengths[i] >= roadNeededtoStraightenOut * 2f)
                        {
                            // Set Steer to zero at 20%
                            newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, driftable[i], directions[i]));
                            // Set Steer to old turn direction at 80 %
                            newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), driftable[i], directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                        }
                        else
                        {
                            // Add turn at 0%
                            newTrackList.Add(new TrackRoadInfo(0, MathHelper.Sign(angles[i]), driftable[i], directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                        }
                    }
                    else
                    {
                        // Set Steer to zero at 20%
                        newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, DriftType.NoDrift, directions[i]));
                        //Set Steer to new turn direction at 80 %
                        newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), driftable[i], directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                    }
                }
                else
                {
                    // Set Steer to zero at 20%
                    newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, DriftType.NoDrift, directions[i]));
                }
            }
            else
            {
                // Look ahead, will I need to turn
                if (Mathf.Abs(angles[i]) > turnAngleRequired)
                {
                    // Set Steer to new turn direction at 80 %
                    newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), driftable[i], directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                }
            }

            AITrackInfo.Add(newTrackList);
        }*/
    }

    enum DriftType { NoDrift, Normal, Wide, Close };

    private class TrackRoadInfo
    {
        public float distance { get; private set; }
        //0 if no turn is needed
        public float turnAmount { get; private set; }
        //This turn is started before the end of a straight path
        public DriftType drift { get; private set; }
        //The direction should stop following the instruction in
        public Vector3 finalDir { get; private set; }

        public TrackRoadInfo(float _distance, float _turnAmount, DriftType _drift, Vector3 _finalDir)
        {
            distance = _distance;
            turnAmount = _turnAmount;
            drift = _drift;
            finalDir = _finalDir;
        }
    }
}