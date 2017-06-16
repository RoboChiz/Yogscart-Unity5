﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public float roadWidth = 5f;

    [HideInInspector]
    public List<NodeConnector> connections = new List<NodeConnector>();

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}

[System.Serializable]
public class NodeConnector
{
    public Node a, b;
    private Vector3 lastA, lastB;

    public enum ConnectionType { Straight, Curve}
    public ConnectionType connectionType;

    public Transform[] extras;
    public Vector3[] lastExtraPos;

    public int segments;

    public NodeConnector()
    {
        connectionType = ConnectionType.Straight;
        extras = new Transform[0];
        segments = 5;
    }

    public NodeConnector(Node _a, Node _b, ConnectionType _connectionType, Transform[] _extras)
    {
        a = _a;
        b = _b;
        connectionType = _connectionType;
        extras = _extras;
        segments = 5;

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
        lastA = a.transform.position;
        lastB = b.transform.position;

        lastExtraPos = new Vector3[extras.Length];

        for (int i = 0; i < extras.Length; i++)
            lastExtraPos[i] = extras[i].transform.position;
    }

    public bool SameNodeConnector(NodeConnector nc)
    {
        if (a != nc.a)
            return false;

        if (b != nc.b)
            return false;

        if (connectionType != nc.connectionType)
            return false;

        if (lastExtraPos == null || extras == null || extras.Length != lastExtraPos.Length)
        {
            UpdateLasts();
            return false;
        }

        for (int i = 0; i < extras.Length; i++)
            if (extras[i].transform.position != lastExtraPos[i])
            {
                UpdateLasts();
                return false;
            }

        if (segments != nc.segments)
            return false;

        if (a.transform.position != lastA)
        {
            UpdateLasts();
            return false;
        }

        if (b.transform.position != lastB)
        {
            UpdateLasts();
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
