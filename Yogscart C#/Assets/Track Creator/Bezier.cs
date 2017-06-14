using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Bezier
{
    //How many lines the curves is broken up into
    public int segments
    {
        get { return segmentValue; }
        set { if (value > 0) { segmentValue = value; CalculatePoints(); } }
    }
    private int segmentValue = 4;

    //The anchor points of the curve
    public Vector3[] anchorPoints
    {
        get { return anchorPointsValue; }
        set { anchorPointsValue = value; CalculatePoints(); }
    }
    private Vector3[] anchorPointsValue;

    //The points along the curve at each segment
    public Vector3[] curvePoints { get; private set; }

    public Bezier(int _segments, Vector3[] _anchorPoints)
    {
        segmentValue = _segments;
        anchorPointsValue = _anchorPoints;

        CalculatePoints();
    }

    private void CalculatePoints()
    {
        List<Vector3> finalPoints = new List<Vector3>();

        //Push first anchor point into curve to stop drawing errors
        finalPoints.Add(anchorPoints[0]);

        for (var i = 1; i < segments; i++)
        {
            float sVal = (1f / segments) * i;
            finalPoints.Add(CalculatePoint(anchorPoints.ToList(), sVal)); //Calculate final point at sVal
        }

        finalPoints.Add(anchorPoints.Last());

        curvePoints = finalPoints.ToArray();
    }

    public Vector3 CalculatePoint(List<Vector3> pointList, float value)
    {
        List<Vector3> returnList = new List<Vector3>();

        for (int i = 0; i < pointList.Count; i++)
        {
            if (i + 1 < pointList.Count)
            {
                returnList.Add(Vector3.Lerp(pointList[i], pointList[i + 1], value));
            }
        }

        if (returnList.Count == 1)
        {
            return returnList[0];
        }
        else
        {
            return CalculatePoint(returnList, value);
        }
    }
}
