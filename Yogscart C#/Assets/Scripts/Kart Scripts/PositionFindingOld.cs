using UnityEngine;
using System.Collections;

public class PositionFindingOld : MonoBehaviour
{
    /*
        private TrackData td;

        public int lap = -1, currentPos, currentShortcut = -1, currentTotal, position = -1;
        public float currentDistance;

        private bool changedPitched;

        void Start()
        {
            GameObject tm = GameObject.Find("Track Manager");
            if (tm != null)
                td = tm.GetComponent<TrackData>();
        }

        // Update is called once per frame
        void Update()
        {
            if (td == null)
            {
                GameObject tm = GameObject.Find("Track Manager");
                if (tm != null)
                    td = tm.GetComponent<TrackData>();
            }
            else
            {
                 float closestDistance = float.MaxValue - 1f;
                 closestDistance = Vector3.Distance(transform.position, td.positionPoints[MathHelper.NumClamp(currentPos, 0, td.positionPoints.Count)].position);

                 CheckForwards(closestDistance);
                 CheckBackwards(closestDistance);

                 if(currentPos == 0 && currentTotal < 0)
                 {
                     currentTotal = 0;
                 }

                 if (lap == -1 && currentPos > 1 && currentTotal > 0)
                 {
                     currentPos = 0;
                     currentTotal = 0;
                 }
                 else
                 {
                     if (td.loopedTrack)
                         currentPos = MathHelper.NumClamp(currentPos, 0, td.positionPoints.Count);
                     else
                         currentPos = Mathf.Clamp(currentPos, 0, td.positionPoints.Count);
                 }

                 currentDistance = Vector3.Distance(transform.position, td.positionPoints[MathHelper.NumClamp(currentPos + 1, 0, td.positionPoints.Count)].position);

                 Transform cPP = td.positionPoints[currentPos]; //Current Position Point
                 if (cPP.GetComponent<PointHandler>().style == PointHandler.Point.Lap)
                 {
                     Vector3 position2 = Vector3.zero, position1 = cPP.position;
                     if (currentPos + 1 < td.positionPoints.Count)
                         position2 = td.positionPoints[currentPos + 1].position;
                     else if (currentPos - 1 >= 0)
                         position2 = td.positionPoints[currentPos - 1].position;

                     float ang = Vector3.Angle(position2 - position1, transform.position - position1);
                     if (ang > 85 && ang < 95)
                     {
                         if (!td.loopedTrack)
                         {
                             if (currentTotal >= CalculateAmount(lap + 1))
                             {
                                 IncreaseLap();
                             }
                         }
                         else
                         {
                             if (currentTotal >= (lap + 1) * td.positionPoints.Count)
                             {
                                 IncreaseLap();
                             }
                         }
                     }

                 }

                 if(!td.loopedTrack)
                 {
                     //Lap Catch, used if for some reason the above code dosen't work. i.e. Lag going across the line
                     if (currentTotal > CalculateAmount(lap + 1) || lap == -1)
                     {
                         IncreaseLap();
                         //Debug.Log("Lap from overlap detection, Lap : " + Lap.ToString());
                     }
                 }
                 else
                 {
                     //Lap Catch, used if for some reason the above code dosen't work. i.e. Lag going across the line
                     if ((currentTotal > (lap + 1) * td.positionPoints.Count) || (currentTotal >= (lap + 1) * td.positionPoints.Count && currentPos > 0))
                     {
                         IncreaseLap();
                         //Debug.Log("Lap from overlap detection, Lap : " + Lap.ToString());
                     }
                 }

                 lap = Mathf.Clamp(lap, -1, td.Laps);
                 Debug.DrawLine(transform.position, td.positionPoints[currentPos].position, Color.red);
             }
            }
        }

        void CheckForwards(float closestDistance)
        {
            for (int i = 1; i < 10; i++)
            {
                float newDistance = Vector3.Distance(transform.position, td.positionPoints[MathHelper.NumClamp(currentPos + i, 0, td.positionPoints.Count)].position);
                if (newDistance < closestDistance)
                {
                    closestDistance = newDistance;
                    currentPos += i;
                    currentTotal += i;
                }
            }
        }
        void CheckBackwards(float closestDistance)
        {
            for (int i = -1; i > -10; i--)
            {
                float newDistance = Vector3.Distance(transform.position, td.positionPoints[MathHelper.NumClamp(currentPos + i, 0, td.positionPoints.Count)].position);

                if (newDistance < closestDistance)
                {
                    closestDistance = newDistance;
                    currentPos += i;
                    currentTotal += i;               
                }
            }
        }

        void IncreaseLap()
        {
            lap += 1;

            //if(transform.GetComponent(KartInfo) != null && Lap < tm.Laps && Lap > 0)
            //transform.GetComponent(KartInfo).NewLap();
        }

        int CalculateAmount(int lapVal)
        {
            if(td.loopedTrack)
            {
                return lapVal * td.positionPoints.Count;
            }
            else
            {
                int returnVal = 0, lapCounted = 0;

                for(int i = 1; i < td.positionPoints.Count; i++)
                {
                    if(td.positionPoints[i].GetComponent<PointHandler>().style == PointHandler.Point.Lap)
                    {
                        lapCounted++;
                    }

                    returnVal++;

                    if (lapCounted == lapVal)
                        break;
                }

                return returnVal;
            }
        }
        */

}