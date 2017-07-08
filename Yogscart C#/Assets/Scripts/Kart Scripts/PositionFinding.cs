using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionFinding : MonoBehaviour {

    public PointHandler closestPoint;// { get; private set; }
    public float currentPercent, lastPercent;// { get; private set; }
    public int lap = -1;

    const int sections = 5;

    //How far in the race are we
    public int racePosition = -1;

    private TrackData td;
    private static List<float> percentsRequired;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		//Find Track Data
        if(td == null)
        {
            td = FindObjectOfType<TrackData>();
        }
        else
        {
            //Get Percents Required if not looped track
            if(!td.loopedTrack && percentsRequired == null)
            {
                percentsRequired = new List<float>();

                foreach(PointHandler ph in td.validPointHandlers)
                {
                    if (ph.style == PointHandler.Point.Lap)
                        percentsRequired.Add(ph.percent);
                }

                percentsRequired.Sort();
            }

            if (closestPoint == null)
                FindClosestPoint();

            if (closestPoint != null)
            {
                float percentSection = 1f / sections;
                if(currentPercent > lastPercent + percentSection && currentPercent < lastPercent + (percentSection * 2f))
                    lastPercent = lastPercent + percentSection;

                List<Vector2> percentagesAndXPos = new List<Vector2>();

                foreach (PointHandler possibleNextPoint in closestPoint.connections)
                {
                    //Calculate our current percent
                    Vector3 finalDir = possibleNextPoint.lastPos - closestPoint.lastPos;

                    //Find out where we are relative to our current and next node
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(finalDir), Vector3.one).inverse;
                    Vector3 localPos = matrix * (transform.position - closestPoint.lastPos);

                    //How far are we between the closest node and the next
                    float localPercent = localPos.z / finalDir.magnitude;

                    bool loopedTrackTrigger = !td.loopedTrack && ((possibleNextPoint.style == PointHandler.Point.End && localPercent >= 0f) || (closestPoint.style == PointHandler.Point.End && localPercent <= 0f));

                    if (loopedTrackTrigger || (localPercent >= 0 && localPercent <= 1))
                    {
                        //Find out percent difference between the two nodes
                        float percentDifference = possibleNextPoint.percent - closestPoint.percent;

                        if(possibleNextPoint.style == PointHandler.Point.Start && closestPoint.style == PointHandler.Point.End)
                            percentDifference = 1f - closestPoint.percent;

                        float closestPercent = closestPoint.percent;

                        if (possibleNextPoint.style == PointHandler.Point.End && closestPoint.style == PointHandler.Point.Start)
                        {
                            percentDifference = possibleNextPoint.percent - 1f;
                            closestPercent = 1f;
                        }

                        //Find out what our world percentage is and add it to a list of percent
                        percentagesAndXPos.Add(new Vector2(closestPercent + (localPercent * percentDifference), localPos.x));
                    }
                }

                //Check that the kart was relatively close to point
                bool closePoint = false;

                foreach (Vector2 option in percentagesAndXPos)
                    if (Mathf.Abs(option.y) <= 15f)
                    {
                        closePoint = true;
                        break;
                    }

                //If we're a little too far from any point, check we're still close to it
                if(!closePoint)
                {
                    PointHandler hold = closestPoint;
                    FindClosestPoint();

                    //If we're closer to another point, don't use this data
                    if (closestPoint != hold)
                        return;
                }
                else //Check if we're closer to a nearby point
                {
                    float currentDistance = Vector3.Distance(transform.position, closestPoint.lastPos);

                    foreach (PointHandler option in closestPoint.connections)
                    {
                        if(Vector3.Distance(transform.position, option.lastPos) < currentDistance)
                        {
                            closestPoint = option;
                            currentDistance = Vector3.Distance(transform.position, closestPoint.lastPos);
                        }
                    }
                }

                //If we're not near any of it's connetions try finding ourselfs
                if (percentagesAndXPos.Count == 0)
                {
                    FindClosestPoint();
                }
                else
                {
                    float totalPercent = 0;

                    foreach (Vector2 percent in percentagesAndXPos)
                        totalPercent += percent.x;

                    currentPercent = totalPercent / percentagesAndXPos.Count;

                    //Calculate if we've completed a lap
                    if (td.loopedTrack)
                    {
                        if (currentPercent < percentSection && lap == -1)
                            lap = 0;

                        if (currentPercent < percentSection && lastPercent >= percentSection * (sections - 1))
                        {
                            lap++;
                            lastPercent = 0f;
                        }

                        if (lastPercent <= percentSection && currentPercent >= percentSection * (sections - 1))
                            lastPercent = 0f;
                    }
                    else
                    {
                        if (currentPercent > 0f && lap == -1)
                            lap = 0;

                        if (currentPercent >= 1f)
                            lap = percentsRequired.Count + 1;
                        else
                        {
                            for (int i = 0; i < percentsRequired.Count; i++)
                            {
                                if (currentPercent > percentsRequired[i])
                                    lap = i + 1;
                            }
                        }                       
                    }
                }
            }
        }
	}

    public void FindClosestPoint()
    {
        if(td != null && td.pointTree != null)
        {
            //Get a List of nearby points
            List<PointHandler> nearbyPoints = new List<PointHandler>();
            nearbyPoints = td.pointTree.Retrieve(nearbyPoints, transform.position);

            //Last Resort check against every point
            if (nearbyPoints.Count == 0)
                nearbyPoints = FindObjectsOfType<PointHandler>().ToList();

            //Find which point we are closest too
            PointHandler nearestPoint = nearbyPoints[0];
            for(int i = 1; i < nearbyPoints.Count; i++)
            {
                PointHandler checkPoint = nearbyPoints[i];

                if(Vector3.Distance(checkPoint.lastPos, transform.position) < Vector3.Distance(nearestPoint.lastPos, transform.position))
                    nearestPoint = checkPoint;
            }

            closestPoint = nearestPoint;
        }
    }


}

