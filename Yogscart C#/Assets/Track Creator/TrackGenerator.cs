using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class TrackGenerator : MonoBehaviour
{
    public Material roadMat;

    public List<NodeConnector> connections = new List<NodeConnector>();
    private List<NodeConnectorCopy> lastConnections = new List<NodeConnectorCopy>();

    // Update is called once per frame
    void Update()
    {
        //Check current Nodes against old Node
        bool changesFound = false;

        connections = FindObjectsOfType<NodeConnector>().ToList();

        if (connections.Count != lastConnections.Count)
            changesFound = true;
        else
        {
            for (int i = 0; i < connections.Count; i++)
            {
                //Check Node Connection against other Node Connection
                if (connections[i].a != null && connections[i].b != null && !connections[i].SameNodeConnector(lastConnections[i]))
                {
                    changesFound = true;
                    break;
                }
            }
        }

        if (changesFound)
        {
            UpdateLC();
        }

    }

    void UpdateLC()
    {
        //Update last node list
        lastConnections = new List<NodeConnectorCopy>();

        Transform trackChild = transform.Find("Roads");

        if(trackChild == null)
        {
            GameObject tcGO = new GameObject("Roads");
            trackChild = tcGO.transform;                 
        }

        trackChild.parent = transform;
        trackChild.localPosition = Vector3.zero;

        foreach (NodeConnector nc in connections)
        {
            lastConnections.Add(new NodeConnectorCopy(nc));

            nc.transform.parent = trackChild;
            nc.generatedMesh = false;
        }

        //Remake Mesh
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        //float startTime = Time.realtimeSinceStartup;

        //Clean up the Network
        List<NodeConnector> validConnections = CollectNetwork();
        List<Node> usedNode = new List<Node>();

        //Create a list of used Nodes
        foreach(NodeConnector nc in validConnections)
        {
            if(!usedNode.Contains(nc.a))
                usedNode.Add(nc.a);
            if (!usedNode.Contains(nc.b))
                usedNode.Add(nc.b);
        }

        //Create Road Geometry
        foreach (NodeConnector nc in validConnections)
        {
            if (!nc.generatedMesh)
            {
                GenerateRoad(nc);
            }
        }

        //Create Node Geometry
        foreach (Node node in usedNode)
        {
            node.ResetMesh();
            node.GenerateMesh();
        }

        //float totalTime = (Time.realtimeSinceStartup - startTime);
        //Debug.Log("Track Generated in " + totalTime + " seconds!");
    }

    private void GenerateRoad(NodeConnector nc)
    {
        nc.GenerateRoad();

        //Test for connected roads, to connect vertices together
        Node a = nc.a;
        Node b = nc.b;

        //If A node if there is only one other connection
        if (a.connections.Count == 2)
        {           
            NodeConnector aCheck = null;
            foreach(NodeConnector checkNC in a.connections)
            {
                if(checkNC != nc)
                {
                    aCheck = checkNC;
                    break;
                }
            }

            //If this connection has a road that hasn't been generated and is in an opposite direction to our road
            if (!aCheck.generatedMesh && Vector3.Dot(nc.Direction(a), aCheck.Direction(a)) < -0.5f)
            {
                GenerateRoad(aCheck);
            }
        }

        //If B node if there is only one other connection
        if (b.connections.Count == 2)
        {
            NodeConnector bCheck = null;
            foreach (NodeConnector checkNC in b.connections)
            {
                if (checkNC != nc)
                {
                    bCheck = checkNC;
                    break;
                }
            }

            //If this connection has a road that hasn't been generated and is in an opposite direction to our road
            if (!bCheck.generatedMesh && Vector3.Dot(nc.Direction(b), bCheck.Direction(b)) < -0.5f)
            {
                GenerateRoad(bCheck);
            }
        }
    }

    public void CleanNetwork()
    {
        //Make sure each node has an empty list
        foreach (NodeConnector nc in connections.ToArray())
        {
            foreach (Transform extra in nc.extras.ToArray())
            {
                if (extra == null)
                    nc.extras.Remove(extra);
            }

        }
    }

    public bool ContainsConnection(Node a, Node b)
    {
        foreach(NodeConnector nc in connections)
        {
            if ((nc.a == a && nc.b == b) || (nc.b == a && nc.a == b))
                return true;
        }

        return false;
    }

    public NodeConnector FindConnection(Node a, Node b)
    {
        foreach (NodeConnector nc in connections)
        {
            if ((nc.a == a && nc.b == b) || (nc.b == a && nc.a == b))
                return nc;
        }

        return null;
    }

    //Collects all Node components from a scene, and cleans the Road Network
    private List<NodeConnector> CollectNetwork()
    {
        //Clean the Network
        CleanNetwork();

        List<NodeConnector> returnValue = new List<NodeConnector>();

        Node[] allNodes = FindObjectsOfType<Node>();
        AnchorPoint[] allAnchors = FindObjectsOfType<AnchorPoint>();

        //Clean all Node Connections
        foreach (Node node in allNodes)
        {
            node.connections = new List<NodeConnector>();
            node.transform.parent = transform;
        }

        foreach (AnchorPoint anchor in allAnchors)
            anchor.transform.parent = transform;

        //Make sure each node has an empty list
        foreach (NodeConnector nc in connections.ToArray())
        {
            nc.segments = Mathf.Clamp(nc.segments, 2, 75);
        }

        //Clean Nodes
        foreach (NodeConnector nc in connections)
        {
            //Clear Extra Away
            foreach (Transform extra in nc.extras.ToArray())
            {
                if (extra == null)
                    nc.extras.Remove(extra);
            }

            //Add Node Connetions to nodes if acceptable
            if (nc.a != null && nc.b != null)
            {
                nc.a.connections.Add(nc);
                nc.b.connections.Add(nc);

                returnValue.Add(nc);
            }
        }

        foreach (Node node in allNodes)
        {
            //Rearrange Roads in Clockwise Order
            List<float> angles = new List<float>();
            List<NodeConnector> sortedList = new List<NodeConnector>();

            for (int i = 0; i < node.connections.Count; i++)
            {
                Vector3 direction = node.connections[i].Opposite(node).transform.position - node.transform.position;
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

        return returnValue;
    }

    public class Vertex
    {
        public Vector3 position, normal;
        public Vector2 uv;

        public Vertex(Vector3 _position, Vector3 _normal, Vector2 _uv)
        {
            position = _position;
            normal = _normal;
            uv = _uv;
        }
    }
}
