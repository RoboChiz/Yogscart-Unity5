using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrackGenerator : MonoBehaviour
{
    private List<Road> allRoads;

    public List<NodeConnector> connections = new List<NodeConnector>();
    private List<NodeConnector> lastConnections = new List<NodeConnector>();

    // Update is called once per frame
    void Update()
    {
        //Check current Nodes against old Node
        bool changesFound = false;

        if (connections.Count != lastConnections.Count)
            changesFound = true;
        else
        {
            for (int i = 0; i < connections.Count; i++)
            {
                //Check Node Connection against other Node Connection
                if (!connections[i].SameNodeConnector(lastConnections[i]))
                {
                    changesFound = true;
                    break;
                }
            }
        }

        if (changesFound)
        {
            //Update last node list
            lastConnections = new List<NodeConnector>();

            foreach (NodeConnector nc in connections)
                lastConnections.Add(new NodeConnector(nc));

            //Remake Mesh
            UpdateMesh();
        }

    }

    private void UpdateMesh()
    {
        float startTime = Time.realtimeSinceStartup;

        //Clean up the Network
        CollectNetwork();

        //Create Mesh Lists
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //Create Road Geometry
        foreach (Road road in allRoads)
        {
            int vertStart = verts.Count;

            Mesh roadMesh = road.GenerateRoad();

            verts.AddRange(roadMesh.vertices);
            normals.AddRange(roadMesh.normals);
            uvs.AddRange(roadMesh.uv);

            List<int> roadTris = roadMesh.triangles.ToList();
            for (int i = 0; i < roadTris.Count; i++)
                roadTris[i] += vertStart;

            tris.AddRange(roadTris);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        GetComponent<MeshFilter>().mesh = newMesh;

        //Debug.Log("Generated track of " + lastNodesPosition.Count + " Nodes in " + (Time.realtimeSinceStartup - startTime).ToString() + " seconds.");
    }

    //Collects all Node components from a scene, and cleans the Road Network
    private void CollectNetwork()
    {
        allRoads = new List<Road>();

        //Make sure each node has an empty list
        foreach (NodeConnector nc in connections.ToArray())
        {
            if (nc.a != null)
                nc.a.connections = new List<NodeConnector>();
            if (nc.b != null)
                nc.b.connections = new List<NodeConnector>();

            nc.segments = Mathf.Clamp(nc.segments, 2, 50);
        }

        //Clean Nodes
        foreach (NodeConnector nc in connections)
        {
            //Add Node Connetions to nodes if acceptable
            if (nc.a != null && nc.b != null && (nc.connectionType == NodeConnector.ConnectionType.Straight || (nc.extras != null && nc.extras.Length > 0)))
            {
                nc.a.connections.Add(nc);
                nc.b.connections.Add(nc);
            }
        }

        Node[] allNodes = FindObjectsOfType<Node>();

        foreach (Node node in allNodes)
        {
            //Rearrange Roads in Clockwise Order
            List<float> angles = new List<float>();
            List<NodeConnector> sortedList = new List<NodeConnector>();

            for (int i = 0; i < node.connections.Count; i++)
            {
                Vector3 direction = node.connections[i].Other(node).transform.position - node.transform.position;
                angles.Add(MathHelper.Angle(Vector3.forward, direction));
            }

            //Sort Roads by Angles
            while (angles.Count > 0)
            {
                int smallestAngle = 0;

                for (int j = 1; j < angles.Count; j++)
                {
                    if (angles[j] < angles[smallestAngle])
                    {
                        smallestAngle = j;
                    }
                }

                sortedList.Add(node.connections[smallestAngle]);

                node.connections.RemoveAt(smallestAngle);
                angles.RemoveAt(smallestAngle);
            }

            node.connections = sortedList;
        }

        //Generate Roads
        foreach (NodeConnector connectedNode in connections)
        {
            //If allRoads does not contain this road
            if (!HasRoad(connectedNode.a, connectedNode.b))
            {
                Road road;
                if (connectedNode.connectionType == NodeConnector.ConnectionType.Straight)
                    road = new StraightRoad(connectedNode.a, connectedNode.b);
                else
                    road = new BezierRoad(connectedNode.a, connectedNode.b, connectedNode.extras, connectedNode.segments);

                allRoads.Add(road);
            }
        }
    }

    private bool HasRoad(Node nodeA, Node nodeB)
    {
        foreach (Road road in allRoads)
        {
            if ((road.a == nodeA && road.b == nodeB) || (road.a == nodeB && road.b == nodeA))
                return true;
        }

        return false;
    }
}
