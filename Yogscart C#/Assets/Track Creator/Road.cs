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

    public Road() { }

    public Road(Node _a, Node _b)
    {
        a = _a;
        b = _b;
    }

    public abstract Mesh GenerateRoad();
}

[System.Serializable]
public class StraightRoad : Road
{
    public StraightRoad(Node _a, Node _b) : base(_a, _b) {}

    public override Mesh GenerateRoad()
    {
        //Create Mesh Lists
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //Create Road Geometry
        float aWidth = a.roadWidth / 2f, bWidth = b.roadWidth / 2f;

        //Get Verts
        verts.Add(a.transform.position + (laneDirection * aWidth));
        verts.Add(a.transform.position - (laneDirection * aWidth));
        verts.Add(b.transform.position + (laneDirection * bWidth));
        verts.Add(b.transform.position - (laneDirection * bWidth));

        for (int i = 0; i < 4; i++)
            normals.Add(Vector3.up);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, length));
        uvs.Add(new Vector2(1, length));

        tris.AddRange(new int[] { 0, 1, 2, 2, 1, 3 });

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        return newMesh;
    }

}

[System.Serializable]
public class BezierRoad : Road
{
    public Transform[] anchorPoints;
    public int segments;

    public BezierRoad(Node _a, Node _b, Transform[] _anchorPoints, int _segments) : base(_a, _b) { anchorPoints = _anchorPoints; segments = _segments; }

    public override Mesh GenerateRoad()
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

        float currentRoadLength = 0f;

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
                lastPointLeft = currentPoint - (rightDir * aWidth);
                lastPointRight = currentPoint + (rightDir * aWidth);
            }

            //Get Verts
            verts.Add(lastPointRight);
            verts.Add(lastPointLeft);           

            //Set the next lastPoints
            lastPointLeft = nextPoint - (rightDir * bWidth);
            lastPointRight = nextPoint + (rightDir * bWidth);

            verts.Add(lastPointRight);
            verts.Add(lastPointLeft);

            for (int j = 0; j < 4; j++)
                normals.Add(Vector3.up);

            float startRoadLength = currentRoadLength;
            currentRoadLength += dir.magnitude;

            uvs.Add(new Vector2(0, startRoadLength));
            uvs.Add(new Vector2(1, startRoadLength));
            uvs.Add(new Vector2(0, currentRoadLength));
            uvs.Add(new Vector2(1, currentRoadLength));

            tris.AddRange(new int[] { vertCount + 0, vertCount + 1, vertCount + 2, vertCount + 2, vertCount + 1, vertCount + 3});
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        return newMesh;
    }
}
