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

    private int currentNode = 0, nextNode = 1, currentPercent = 0;
    private const float maxXDistance = 2f, minAngle = 3f;

    public enum StartType { WillBoost, WontBoost, WillSpin };
    public StartType myStartType;

    public enum DriveState { Centring, Turning, Nothing};
    public DriveState driveState = DriveState.Centring;

    public bool canDrive = true;

    public Vector3 localPos;
    public float percent;
    private bool reversing = false;

    private static List<List<TrackRoadInfo>> AITrackInfo;
    private List<float> pathLengths;
    private float[] angles;

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
            if (percent > 1)
            {
                currentNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);
                nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);

                currentPercent = 0;
            }
            if (percent < 0)
            {
                currentNode = MathHelper.NumClamp(currentNode - 1, 0, td.positionPoints.Count);
                nextNode = MathHelper.NumClamp(currentNode + 1, 0, td.positionPoints.Count);

                currentPercent = 0;
            }

            Vector3 currentNodePos = td.positionPoints[currentNode].position;
            Vector3 nextNodePos = td.positionPoints[nextNode].position;
            Vector3 finalDir = nextNodePos - currentNodePos;

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(finalDir), Vector3.one).inverse;
            localPos = matrix * (transform.position - currentNodePos);
            percent = localPos.z / finalDir.magnitude;

            Vector3 myForward = transform.forward;
            myForward.y = 0f;
            myForward.Normalize();

            //Get Current Instruction
            int currentInstruction = -1;
            for (int i = 0; i < AITrackInfo[currentNode].Count; i++)
            {
                if (localPos.z > AITrackInfo[currentNode][i].distance)
                    currentInstruction = i;
            }

            bool doNothing = true;

            ks.throttle = 1f;

            //Follow Current Instruction
            if (currentInstruction != -1)
            {
                TrackRoadInfo instruction = AITrackInfo[currentNode][currentInstruction];

                //If Angle is not what we need to be follow instructions
                float angle = MathHelper.Angle(myForward, instruction.finalDir);
                if (Mathf.Abs(angle) > minAngle && Mathf.Sign(angle) == Mathf.Sign(instruction.turnAmount))
                {
                    ks.steer = instruction.turnAmount;
                    driveState = DriveState.Turning;
                    doNothing = false;
                }

            }

            //Do Straightening
            int straightenTurn = 0;
            float finalDirAngle = -MathHelper.Angle(myForward, finalDir);

            //Turn towards Centre if we are far away from it
            if (localPos.x > maxXDistance && finalDirAngle > -minAngle)
                straightenTurn = -1;
            if (localPos.x < -maxXDistance && finalDirAngle < minAngle)
                straightenTurn = 1;

            //If we are close to centre, but off course
            if(Mathf.Abs(localPos.x) < maxXDistance)
            {
                if (finalDirAngle > minAngle)
                    straightenTurn = -1;
                if (finalDirAngle < -minAngle)
                    straightenTurn = 1;
            }

            //If Straighten turn exact opposite of current turn then don't follow through
            //If doNothing or if current turn is 0 or if current turn is same as Straighten Turn then follow through
            //If past left minXDistance and told to turn left, then allowed to straighten
            if (straightenTurn != 0 &&  (doNothing || ks.steer == 0 || (ks.drift && ((localPos.x < -maxXDistance && ks.steer == -1) || (localPos.x > maxXDistance && ks.steer == 1)))))
            {
                ks.steer = straightenTurn;
                driveState = DriveState.Centring;
                doNothing = false;
            }

            //No Instructions to follow, so just drive straight
            if (doNothing)
            {           
                ks.throttle = 1f;
                ks.steer = 0f;
                driveState = DriveState.Nothing;
            }


            //Reverse if Driving into a Wall
            Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.red);
            RaycastHit raycastHit;

            float distance = 1.5f;
            if (reversing)
                distance = 3f;

            if(Physics.Raycast(transform.position, transform.forward,out raycastHit, distance) && raycastHit.transform.GetComponent<Rigidbody>() == null)
            {
                reversing = true;
                ks.throttle = -1;
                ks.steer *= -1f;
            }
            else
            {
                reversing = false;
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

    const float turnAngleRequired = 2f, roadNeededtoStraightenOut = 10f, angleForDrift = 6f;

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
        Vector3[] directions = new Vector3[td.positionPoints.Count];
        pathLengths = new List<float>();

        for (int i = 0; i < td.positionPoints.Count; i++)
        {
            int nextPoint = MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count);
            int nextNextPoint = MathHelper.NumClamp(i + 2, 0, td.positionPoints.Count);

            Vector3 currentDir = td.positionPoints[nextPoint].position - td.positionPoints[i].position;
            Vector3 nextDir = td.positionPoints[nextNextPoint].position - td.positionPoints[nextPoint].position;

            angles[i] = MathHelper.Angle(currentDir, nextDir);
            directions[i] = currentDir;

            pathLengths.Add(currentDir.magnitude);
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
                            newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, false, directions[i]));
                            // Set Steer to old turn direction at 80 %
                            newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), angles[i] > angleForDrift, directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                        }
                        else
                        {
                            // Add turn at 0%
                            newTrackList.Add(new TrackRoadInfo(0, MathHelper.Sign(angles[i]), angles[i] > angleForDrift, directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                        }
                    }
                    else
                    {
                        // Set Steer to zero at 20%
                        newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, false, directions[i]));
                        //Set Steer to new turn direction at 80 %
                        newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), angles[i] > angleForDrift, directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                    }
                }
                else
                {
                    // Set Steer to zero at 20%
                    newTrackList.Add(new TrackRoadInfo(roadNeededtoStraightenOut, 0, false, directions[i]));
                }
            }
            else
            {
                // Look ahead, will I need to turn
                if (Mathf.Abs(angles[i]) > turnAngleRequired)
                {
                    // Set Steer to new turn direction at 80 %
                    newTrackList.Add(new TrackRoadInfo(pathLengths[i] - roadNeededtoStraightenOut, MathHelper.Sign(angles[i]), angles[i] > angleForDrift, directions[MathHelper.NumClamp(i + 1, 0, td.positionPoints.Count)]));
                }
            }

            AITrackInfo.Add(newTrackList);
        }
    }

    private class TrackRoadInfo
    {
        public float distance { get; private set; }
        //0 if no turn is needed
        public float turnAmount { get; private set; }
        //This turn is started before the end of a straight path
        public bool drift { get; private set; }
        //The direction should stop following the instruction in
        public Vector3 finalDir { get; private set; }

        public TrackRoadInfo(float _distance, float _turnAmount, bool _drift, Vector3 _finalDir)
        {
            distance = _distance;
            turnAmount = _turnAmount;
            drift = _drift;
            finalDir = _finalDir;
        }
    }
}