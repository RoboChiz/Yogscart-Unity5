using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Road
{
    public Node a, b;

    public Vector3 direction { get { return (b.transform.position - a.transform.position); } }
    public Vector3 laneDirection { get { return Quaternion.AngleAxis(90f, Vector3.up) * Vector3.Scale(direction, new Vector3(1f,0f,1f)).normalized; } }

    public float length { get { return direction.magnitude; } }

    public bool addedToMesh = false;

    public Road() { }

    public Road(Node _a, Node _b)
    {
        a = _a;
        b = _b;
    }

    public abstract Vector3 Direction(Node startNode);
    public abstract Mesh GenerateRoad(float startLength, out float endLength);

    public Node Opposite(Node node)
    {
        if (a == node)
            return b;
        if (b == node)
            return a;

        return null;
    }
}

[System.Serializable]
public class StraightRoad : Road
{
    public StraightRoad(Node _a, Node _b) : base(_a, _b) {}

    public override Mesh GenerateRoad(float startLength, out float endLength)
    {
        //Create Mesh Lists
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //Create Road Geometry
        float aWidth = a.roadWidth / 2f, bWidth = b.roadWidth / 2f;

        //Get Verts
        Vector3 aLaneDirection = Quaternion.AngleAxis(a.rotateAmount, direction) * laneDirection;
        Vector3 bLaneDirection = Quaternion.AngleAxis(b.rotateAmount, direction) * laneDirection;

        verts.Add(a.transform.position + (aLaneDirection * aWidth));
        verts.Add(a.transform.position - (aLaneDirection * aWidth));
        verts.Add(b.transform.position + (bLaneDirection * bWidth));
        verts.Add(b.transform.position - (bLaneDirection * bWidth));

        for (int i = 0; i < 4; i++)
            normals.Add(Vector3.up);

        uvs.Add(new Vector2(0, startLength));
        uvs.Add(new Vector2(1, startLength));

        endLength = startLength + length;

        uvs.Add(new Vector2(0, endLength));
        uvs.Add(new Vector2(1, endLength));

        tris.AddRange(new int[] { 0, 1, 2, 2, 1, 3 });

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        return newMesh;
    }

    public override Vector3 Direction(Node startNode)
    {
        return Opposite(startNode).transform.position - startNode.transform.position;
    }

}

[System.Serializable]
public class BezierRoad : Road
{
    public Transform[] anchorPoints;
    public int segments;

    public BezierRoad(Node _a, Node _b, Transform[] _anchorPoints, int _segments) : base(_a, _b) { anchorPoints = _anchorPoints; segments = _segments; }

    public override Mesh GenerateRoad(float startLength, out float endLength)
    {
        //Create Mesh Lists
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //Create Bezier Curve
        List<Vector3> anchorPointPositions = new List<Vector3>();
        anchorPointPositions.Add(a.transform.position);

        foreach (Transform extra in anchorPoints)
            anchorPointPositions.Add(extra.position);

        anchorPointPositions.Add(b.transform.position);

        Bezier curve = new Bezier(segments, anchorPointPositions.ToArray());

        float currentRoadLength = startLength;

        //Create Last Points and set it to the road at zero
        Vector3 lastPointLeft = Vector3.zero, lastPointRight = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            //Create Road Geometry
            float aWidth = a.roadWidth / 2f, bWidth = b.roadWidth / 2f;
            int vertCount = verts.Count;

            Vector3 currentPoint = curve.curvePoints[i], nextPoint = curve.curvePoints[i + 1];
            Vector3 dir = nextPoint - currentPoint;
            Vector3 rightDir = Quaternion.AngleAxis(90f, Vector3.up) * Vector3.Scale(dir.normalized, new Vector3(1f, 0f, 1f));

            if (i == 0)
            {
                Quaternion rotateAngle = Quaternion.AngleAxis(a.rotateAmount, dir);

                lastPointLeft = currentPoint - ((rotateAngle * rightDir) * aWidth);
                lastPointRight = currentPoint + ((rotateAngle * rightDir) * aWidth);
            }

            //Get Verts
            verts.Add(lastPointRight);
            verts.Add(lastPointLeft);

            //Set the next lastPoints
            float percent = (i + 1) / (float)segments;
            Quaternion endRotateAngle = Quaternion.AngleAxis(Mathf.Lerp(a.rotateAmount, b.rotateAmount, percent), dir);
            lastPointLeft = nextPoint - ((endRotateAngle * rightDir) * Mathf.Lerp(aWidth,bWidth, percent));
            lastPointRight = nextPoint + ((endRotateAngle * rightDir) * Mathf.Lerp(aWidth, bWidth, percent));

            verts.Add(lastPointRight);
            verts.Add(lastPointLeft);

            for (int j = 0; j < 4; j++)
                normals.Add(Quaternion.AngleAxis(90f, dir) * rightDir);

            float startRoadLength = currentRoadLength;
            currentRoadLength += dir.magnitude;

            uvs.Add(new Vector2(0, startRoadLength));
            uvs.Add(new Vector2(1, startRoadLength));
            uvs.Add(new Vector2(0, currentRoadLength));
            uvs.Add(new Vector2(1, currentRoadLength));

            tris.AddRange(new int[] { vertCount + 0, vertCount + 1, vertCount + 2, vertCount + 2, vertCount + 1, vertCount + 3});
        }

        endLength = currentRoadLength;

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        addedToMesh = true;

        return newMesh;
    }

    public override Vector3 Direction(Node startNode)
    {
        //Create Bezier Curve
        List<Vector3> anchorPointPositions = new List<Vector3>();
        anchorPointPositions.Add(a.transform.position);

        foreach (Transform extra in anchorPoints)
            anchorPointPositions.Add(extra.position);

        anchorPointPositions.Add(b.transform.position);

        Bezier curve = new Bezier(segments, anchorPointPositions.ToArray());

        if (startNode == a)
            return curve.curvePoints[1] - curve.curvePoints[0];

        if(startNode == b)
            return curve.curvePoints[curve.curvePoints.Length-2] - curve.curvePoints[curve.curvePoints.Length - 1];

        Debug.LogError("AHHHH!");
        throw new Exception("Broken!");
    }
}
