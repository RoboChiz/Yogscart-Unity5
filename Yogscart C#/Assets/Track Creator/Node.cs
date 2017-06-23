using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public float roadWidth = 25f;
    //In Degrees
    public float rotateAmount = 0f;

    [HideInInspector]
    public List<NodeConnector> connections = new List<NodeConnector>();

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(5, 5, 5));
    }
}

[System.Serializable]
public class NodeConnector
{
    public string name;
    public Node a
    {
        get { return nodeA; }
        set { nodeA = value; UpdateName(); }
    }
    public Node b
    {
        get { return nodeB; }
        set { nodeB = value; UpdateName(); }
    }

    public Node nodeA, nodeB;
    private Vector3 lastA, lastB;
    private float lastARotate, lastBRotate;

    public enum ConnectionType { Straight, Curve}
    public ConnectionType connectionType;

    public List<Transform> extras;
    private Vector3[] lastExtraPos;

    [HideInInspector]
    public Road road;

    public int segments;

    public NodeConnector()
    {
        connectionType = ConnectionType.Straight;
        extras = new List<Transform>();
        segments = 10;
    }

    public void UpdateName()
    {
        name = "";

        if (nodeA != null)
            name += a.transform.name;

        if (name != "" && b != null)
            name += " & ";

        if (nodeB != null)
            name += b.transform.name;
    }

    public NodeConnector(Node _a, Node _b, ConnectionType _connectionType, List<Transform> _extras)
    {
        a = _a;
        b = _b;
        connectionType = _connectionType;
        extras = _extras;
        segments = 10;

        lastA = a.transform.position;
        lastB = b.transform.position;
    }

    public NodeConnector(NodeConnector nc)
    {
        a = nc.a;
        b = nc.b;
        connectionType = nc.connectionType;

        extras = nc.extras;
        segments = nc.segments;

        if(a != null)
            lastA = a.transform.position;
        if(b != null)
            lastB = b.transform.position;
    }

    public void UpdateLasts()
    {
        if (a != null)
            lastA = a.transform.position;
        else
            lastA = Vector3.zero;

        if (b != null)
            lastB = b.transform.position;
        else
            lastB = Vector3.zero;

        lastExtraPos = new Vector3[extras.Count];

        for (int i = 0; i < extras.Count; i++)
            if(extras[i] != null)
                lastExtraPos[i] = extras[i].transform.position;

        lastARotate = a.rotateAmount;
        lastBRotate = b.rotateAmount;
    }

    public bool SameNodeConnector(NodeConnector nc)
    {
        if (a != nc.a)
            return false;

        if (b != nc.b)
            return false;

        if (connectionType != nc.connectionType)
            return false;

        if (lastExtraPos == null || extras == null || extras.Count != lastExtraPos.Length)
        {
            UpdateLasts();
            return false;
        }

        for (int i = 0; i < extras.Count; i++)
            if (extras[i] != null && extras[i].transform.position != lastExtraPos[i])
            {
                UpdateLasts();
                return false;
            }

        if (segments != nc.segments)
            return false;

        if (a != null && a.transform.position != lastA)
        {
            UpdateLasts();
            return false;
        }

        if (b != null && b.transform.position != lastB)
        {
            UpdateLasts();
            return false;
        }

        if(a.rotateAmount != lastARotate)
        {
            return false;
        }

        if (b.rotateAmount != lastBRotate)
        {
            return false;
        }

        return true;
    }

    public Node Other(Node node)
    {
        if (a == node)
            return b;
        if (b == node)
            return a;

        return null;
    }
}
