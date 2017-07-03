using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class NodeConnector : MonoBehaviour
{

    public Node a, b;
    public List<Transform> extras;

    public int segments;

    //Cut ends of track off by this much to allow for mesh joining
    public float aOffset = 2f, bOffset = 2f;

    [HideInInspector, SerializeField]
    public bool generatedMesh = false;

    public List<Vertex> aEndVertices, bEndVertices;

    public NodeConnector()
    {
        extras = new List<Transform>();
        segments = 10;
    }

    public void SetConnector(Node _a, Node _b, List<Transform> _extras)
    {
        a = _a;
        b = _b;
        extras = _extras;
        segments = 10;
    }

    public void SetConnector(NodeConnector nc)
    {
        a = nc.a;
        b = nc.b;

        extras = nc.extras;
        segments = nc.segments;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            transform.name = "";

            if (a != null)
                transform.name += a.name;

            if (transform.name != "" && b != null)
                transform.name += " & ";

            if (b != null)
                transform.name += b.name;

            transform.name += " Road";

            transform.position = Vector3.zero;
        }
    }

    public bool SameNodeConnector(NodeConnectorCopy nc)
    {
        if (a != nc.a)
            return false;

        if (b != nc.b)
            return false;

        if (aOffset != nc.lastAOffset || nc.lastBOffset != bOffset)
            return false;

        if (nc.lastExtraPos == null || extras == null || nc.extras.Count != nc.lastExtraPos.Length)
            return false;

        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].transform.position != nc.lastExtraPos[i])
                return false;

        if (segments != nc.segments)
            return false;

        if (a != null && a.transform.position != nc.lastA)
            return false;

        if (b != null && b.transform.position != nc.lastB)
            return false;

        if (a.rotateAmount != nc.lastARotate)
            return false;

        if (b.rotateAmount != nc.lastBRotate)
            return false;

        if (a.roadWidth != nc.lastAWidth)
            return false;

        if (b.roadWidth != nc.lastBWidth)
            return false;

        return true;
    }

    public Node Opposite(Node node)
    {
        if (a == node)
            return b;
        if (b == node)
            return a;

        return null;
    }

    public void GenerateRoad()
    {
        //Create Mesh Lists
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        aEndVertices = new List<Vertex>();
        bEndVertices = new List<Vertex>();

        //Create Bezier Curve
        List<Vector3> anchorPointPositions = new List<Vector3>();
        anchorPointPositions.Add(a.transform.position);

        foreach (Transform extra in extras)
            anchorPointPositions.Add(extra.position);

        anchorPointPositions.Add(b.transform.position);

        Bezier curve = new Bezier(segments, anchorPointPositions.ToArray());

        float currentRoadLength = 0f;

        //Create Last Points and set it to the road at zero
        Vector3 lastPointLeft = Vector3.zero, lastPointRight = Vector3.zero;

        bool firstTime = false;

        for (int i = 0; i < segments; i++)
        {
            //Create Road Geometry
            float aWidth = a.roadWidth / 2f, bWidth = b.roadWidth / 2f;
            int vertCount = verts.Count;

            Vector3 currentPoint = curve.curvePoints[i], nextPoint = curve.curvePoints[i + 1];
            Vector3 dir = nextPoint - currentPoint;
            Vector3 rightDir = Quaternion.AngleAxis(90f, Vector3.up) * Vector3.Scale(dir.normalized, new Vector3(1f, 0f, 1f));

            float curveLength = curve.length;

            if (aOffset < curveLength - bOffset && currentRoadLength + dir.magnitude >= aOffset && currentRoadLength <= curveLength - bOffset)
            {
                //Setup Lengths
                float startRoadLength = currentRoadLength, endRoadLength = currentRoadLength + dir.magnitude;

                if (!firstTime)
                {
                    firstTime = true;

                    Quaternion rotateAngle = Quaternion.AngleAxis(a.rotateAmount, dir);

                    if (currentRoadLength < aOffset)
                    {
                        currentPoint += dir.normalized * (aOffset - currentRoadLength);
                        startRoadLength += (aOffset - currentRoadLength);
                    }

                    lastPointLeft = currentPoint - ((rotateAngle * rightDir) * aWidth);
                    lastPointRight = currentPoint + ((rotateAngle * rightDir) * aWidth);

                    //Get Vertices at start of Road
                    aEndVertices.Add(new Vertex(lastPointLeft, Vector3.up, new Vector2(0, startRoadLength)));
                    aEndVertices.Add(new Vertex(lastPointRight, Vector3.up, new Vector2(1, startRoadLength)));
                }

                //Get Verts
                verts.Add(lastPointRight);
                verts.Add(lastPointLeft);

                if(currentRoadLength + dir.magnitude > curveLength - bOffset)
                {
                    nextPoint = currentPoint + (dir.normalized * ((curveLength - bOffset) - currentRoadLength));
                    endRoadLength = currentRoadLength += ((curveLength - bOffset) - currentRoadLength);
                }

                //Set the next lastPoints
                float percent = (currentRoadLength + dir.magnitude - aOffset) / (curveLength - aOffset - bOffset);

                Quaternion endRotateAngle = Quaternion.AngleAxis(Mathf.Lerp(a.rotateAmount, b.rotateAmount, percent), dir);
                lastPointLeft = nextPoint - ((endRotateAngle * rightDir) * Mathf.Lerp(aWidth, bWidth, percent));
                lastPointRight = nextPoint + ((endRotateAngle * rightDir) * Mathf.Lerp(aWidth, bWidth, percent));

                //Get Vertices at end of Road
                if (currentRoadLength + dir.magnitude > curveLength - bOffset)
                {
                    bEndVertices.Add(new Vertex(lastPointLeft, Vector3.up, new Vector2(0, endRoadLength)));
                    bEndVertices.Add(new Vertex(lastPointRight, Vector3.up, new Vector2(1, endRoadLength)));
                }

                verts.Add(lastPointRight);
                verts.Add(lastPointLeft);

                for (int j = 0; j < 4; j++)
                    normals.Add(Vector3.up);   

                uvs.Add(new Vector2(0, startRoadLength));
                uvs.Add(new Vector2(1, startRoadLength));
                uvs.Add(new Vector2(0, endRoadLength));
                uvs.Add(new Vector2(1, endRoadLength));

                tris.AddRange(new int[] { vertCount + 0, vertCount + 1, vertCount + 2, vertCount + 2, vertCount + 1, vertCount + 3 });
            }

            currentRoadLength += dir.magnitude;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        generatedMesh = true;

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = newMesh;
        GetComponent<MeshCollider>().sharedMesh = newMesh;

        if (verts.Count >= 4)
        {
            for (int i = 0; i < 2; i++)
                aEndVertices.Add(new Vertex(verts[i], normals[i], uvs[i]));

            for (int i = 0; i < 2; i++)
            {
                int grabPoint = verts.Count - 2 + i;
                bEndVertices.Add(new Vertex(verts[grabPoint], normals[grabPoint], uvs[grabPoint]));
            }
        }
    }

    public Vector3 Direction(Node startNode)
    {
        //Create Bezier Curve
        List<Vector3> anchorPointPositions = new List<Vector3>();
        anchorPointPositions.Add(a.transform.position);
        foreach (Transform extra in extras)
            anchorPointPositions.Add(extra.position);
        anchorPointPositions.Add(b.transform.position);

        Bezier curve = new Bezier(segments, anchorPointPositions.ToArray());

        if (startNode == a)
            return curve.curvePoints[1] - curve.curvePoints[0];

        if (startNode == b)
            return curve.curvePoints[curve.curvePoints.Length - 2] - curve.curvePoints[curve.curvePoints.Length - 1];

        Debug.LogError("AHHHH!");
        throw new Exception("Broken!");
    }
}

public class NodeConnectorCopy
{
    public Node a, b;
    public Vector3 lastA, lastB;
    public float lastARotate, lastBRotate, lastAOffset, lastBOffset, lastAWidth, lastBWidth;

    public List<Transform> extras;
    public Vector3[] lastExtraPos;

    public int segments;

    public NodeConnectorCopy(NodeConnector nc)
    {
        a = nc.a;
        b = nc.b;

        extras = nc.extras;
        segments = nc.segments;

        lastAOffset = nc.aOffset;
        lastBOffset = nc.bOffset;

        if (a != null)
        {
            lastA = a.transform.position;
            lastARotate = a.rotateAmount;
            lastAWidth = a.roadWidth;
        }

        if (b != null)
        {
            lastB = b.transform.position;
            lastBRotate = b.rotateAmount;
            lastBWidth = b.roadWidth;
        }

        lastExtraPos = new Vector3[extras.Count];
        for (int i = 0; i < extras.Count; i++)
        {
            if(extras[i] != null)
                lastExtraPos[i] = extras[i].transform.position;
        }
    }
}
