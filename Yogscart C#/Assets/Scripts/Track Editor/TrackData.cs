using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using RobsNodes;

//TrackData Script - V4.0 (Feb 2016)
//Created by Robert (Robo_Chiz)
//Do not edit this script, doing so may cause compatibility errors.

[ExecuteInEditMode, System.Serializable, RequireComponent(typeof(InEngineRender))]
public class TrackData : MonoBehaviour
{

    //Track Metadata
    public string trackName = "Untitled Track";
    public AudioClip backgroundMusic;

    public bool loopedTrack = true;

    public int Laps = 3;

    [HideInInspector]
    public Transform spawnPoint;
    public List<CameraPoint> introPans;

    public NodeTree positionPoints;

    [HideInInspector]
    public NodeTree.Node affecter;

    void Awake()
    {
        if (positionPoints != null)
            positionPoints.Computate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.name = "Track Manager";

            if (introPans == null)
            {
                introPans = new List<CameraPoint>();
                positionPoints = new NodeTree();
            }

            //Check for Spawn Point
            if (spawnPoint == null)
            {
                var obj = new GameObject();
                obj.AddComponent<PointHandler>();

                spawnPoint = obj.transform;
                spawnPoint.GetComponent<PointHandler>().style = PointHandler.Point.Spawn;
                spawnPoint.parent = transform;
            }
            spawnPoint.name = "Spawn Point";

            positionPoints.CheckNodeForNull(positionPoints.StartNode);

            //Create a new Start Point if one does not exist
            if (positionPoints.StartNode == null)
            {
                positionPoints.NewNode(CreatePoint(spawnPoint.position));
            }

            //Set up Rules
            positionPoints.StartNode.style = NodeTree.Node.NodeType.Lap;//Make Start Node Lap point

            if (!loopedTrack)
                positionPoints.EndNode.style = NodeTree.Node.NodeType.Lap;
            else
                positionPoints.EndNode.style = NodeTree.Node.NodeType.Normal;

            positionPoints.Computate();

            for(int i = 0; i < positionPoints.MainTrack.nodes.Count; i++)
            {
                positionPoints.MainTrack.nodes[i].representation.name = "Position " + i;
            }

        }
    }

    public void SetAffecter(NodeTree.Node node)
    {
        if (affecter != null && affecter.Representation != null)
            affecter.Representation.GetComponent<NodeHandler>().selected = false;

        affecter = node;

        node.Representation.GetComponent<NodeHandler>().selected = true;
    }

    Transform CreatePoint(Vector3 position)
    {
        GameObject obj = new GameObject();
        obj.transform.position = position;
        obj.transform.parent = transform;
        return obj.transform;
    }

    public void AddPoint(NodeTree.Node node)
    {
        Transform obj = CreatePoint(node.representation.position);
        positionPoints.AddNode(node, obj);
        
        #if UNITY_EDITOR
            Selection.activeTransform = obj.transform;
        #endif
    }

    public void ConnectNodes(NodeTree.Node node)
    {
        positionPoints.ConnectNodes(affecter, node);
        SceneView.RepaintAll();
    }

    public void RemoveConnection(NodeTree.Node node)
    {
        positionPoints.RemoveConnection(affecter, node);
        SceneView.RepaintAll();
    }

    public void InsertNode(NodeTree.Node node)
    {
        Transform obj = CreatePoint(affecter.representation.position);
        positionPoints.InsertNode(affecter, node, obj);
        #if UNITY_EDITOR
            if(obj != null)
                Selection.activeTransform = obj.transform;
        #endif
        SceneView.RepaintAll();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}

[System.Serializable]
public class CameraPoint
{
    public Vector3 startPoint, endPoint, startRotation, endRotation;
    public float travelTime = 3f;
}