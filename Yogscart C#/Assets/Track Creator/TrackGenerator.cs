using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TrackGenerator : MonoBehaviour
{
    public List<NodeConnector> connections = new List<NodeConnector>();
    private List<NodeConnector> lastConnections = new List<NodeConnector>();
    private List<Road> allRoads;

    // Update is called once per frame
    void Update()
    {
        //Check current Nodes against old Node
        bool changesFound = false;

        if (allRoads == null)
            changesFound = true;
        else if (connections.Count != lastConnections.Count)
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
        lastConnections = new List<NodeConnector>();

        foreach (NodeConnector nc in connections)
            lastConnections.Add(new NodeConnector(nc));

        //Remake Mesh
        UpdateMesh();
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
            if (!road.addedToMesh)
            {
                GenerateRoad(verts, normals, uvs, tris, road, 0f, null, false);
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = verts.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.uv = uvs.ToArray();

        GetComponent<MeshFilter>().mesh = newMesh;
        GetComponent<MeshCollider>().sharedMesh = newMesh;

        //Debug.Log("Generated track of " + lastNodesPosition.Count + " Nodes in " + (Time.realtimeSinceStartup - startTime).ToString() + " seconds.");
    }

    private void GenerateRoad(List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> tris, Road road, float currentLength, Vertex[] lastTwoVertices, bool atFront)
    {
        int vertStart = verts.Count;

        float newLength = 0f;
        Mesh roadMesh = road.GenerateRoad(currentLength, out newLength);
        currentLength = newLength;

        Vector3[] finalVerts = roadMesh.vertices;
        Vector3[] finalNormals = roadMesh.normals;
        Vector2[] finalUV = roadMesh.uv;

        if (lastTwoVertices != null)
        {
            for (int i = 0; i < 2; i++)
            {
                int grabPosition = atFront ? i : roadMesh.vertices.Length - 2 + i;

                finalVerts[grabPosition] = lastTwoVertices[i].position;
                finalNormals[grabPosition] = lastTwoVertices[i].normal;
                finalUV[grabPosition] = lastTwoVertices[i].uv;
            }       
        }

        verts.AddRange(finalVerts);
        normals.AddRange(finalNormals);
        uvs.AddRange(finalUV);

        List<int> roadTris = roadMesh.triangles.ToList();
        for (int i = 0; i < roadTris.Count; i++)
            roadTris[i] += vertStart;

        tris.AddRange(roadTris);

        //Test for connected roads, to connect vertices together
        Node a = road.a;
        Node b = road.b;

        //If A node if there is only one other connection
        if (road.a.connections.Count == 2)
        {           
            NodeConnector nc = null;
            foreach(NodeConnector checkNC in a.connections)
            {
                if(!((checkNC.a == a && checkNC.b == b) || (checkNC.a == b && checkNC.b == a)))
                {
                    nc = checkNC;
                    break;
                }
            }

            //If this connection has a road that hasn't been generated and is in an opposite direction to our road
            if (nc.road != null && !nc.road.addedToMesh && Vector3.Dot(nc.road.Direction(a), road.Direction(a)) < -0.5f)
            {
                List<Vertex> aListVertices = new List<Vertex>();

                for (int i = 1; i >= 0; i--)
                    aListVertices.Add(new Vertex(roadMesh.vertices[i], roadMesh.normals[i], roadMesh.uv[i]));

                GenerateRoad(verts, normals, uvs, tris, nc.road, currentLength, aListVertices.ToArray(), nc.road.a == a);
            }
        }

        //If B node if there is only one other connection
        if (road.b.connections.Count == 2)
        {         
            NodeConnector nc = null;
            foreach (NodeConnector checkNC in b.connections)
            {
                if (!((checkNC.a == a && checkNC.b == b) || (checkNC.a == b && checkNC.b == a)))
                {
                    nc = checkNC;
                    break;
                }
            }

            //If this connection has a road that hasn't been generated and is in an opposite direction to our road
            if (nc.road != null && !nc.road.addedToMesh && Vector3.Dot(nc.road.Direction(b), road.Direction(b)) < -0.5f)
            {
                List<Vertex> bListVertices = new List<Vertex>();

                for (int i = 1; i >= 0; i--)
                {
                    int grabPoint = roadMesh.vertices.Length - 2 + i;
                    bListVertices.Add(new Vertex(roadMesh.vertices[grabPoint], roadMesh.normals[grabPoint], roadMesh.uv[grabPoint]));
                }

                GenerateRoad(verts, normals, uvs, tris, nc.road, currentLength, bListVertices.ToArray(), nc.road.b == b);
            }
        }
    }

    public void CleanNetwork()
    {
        //Make sure each node has an empty list
        foreach (NodeConnector nc in connections.ToArray())
        {
            if (nc.a == null || nc.b == null)
            {
                foreach (Transform extra in nc.extras.ToArray())
                {
                    if (extra != null)
                        DestroyImmediate(extra.gameObject);

                    nc.extras.Remove(extra);
                }

                connections.Remove(nc);
            }
        }

        CollectNetwork();
    }

    //Collects all Node components from a scene, and cleans the Road Network
    private void CollectNetwork()
    {
        allRoads = new List<Road>();
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
            nc.segments = Mathf.Clamp(nc.segments, 2, 50);
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
            }
        }

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
            if (connectedNode.a != null && connectedNode.b != null && !HasRoad(connectedNode.a, connectedNode.b))
            {
                Road road;
                if (connectedNode.connectionType == NodeConnector.ConnectionType.Straight)
                    road = new StraightRoad(connectedNode.a, connectedNode.b);
                else
                    road = new BezierRoad(connectedNode.a, connectedNode.b, connectedNode.extras.ToArray(), connectedNode.segments);

                allRoads.Add(road);
                connectedNode.road = road;
            }
            else
            {
                connectedNode.road = null;
            }
        }
    }

    public bool HasRoad(Node nodeA, Node nodeB)
    {
        foreach (Road road in allRoads)
        {
            if ((road.a == nodeA && road.b == nodeB) || (road.a == nodeB && road.b == nodeA))
                return true;
        }

        return false;
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
