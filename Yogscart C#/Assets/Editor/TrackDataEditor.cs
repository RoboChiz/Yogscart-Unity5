using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(TrackData))]
public class NodeTools : EditorWindow
{

    [MenuItem("Track Editor/Create Track Manager")]
    static void CreateTrackManager()
    {
        if (GameObject.Find("Track Manager") == null)
        {
            var obj = new GameObject();
            obj.AddComponent<TrackData>();
        }
    }

    [MenuItem("Track Editor/Show Node Tools")]
    static void ShowNodeTools()
    {
        if (GameObject.Find("Track Manager") != null)
        {
            EditorWindow.GetWindow(typeof(NodeTools));
        }
        else
        {
            Debug.Log("Please create a Track Manager first!");
        }
    }

    private GameObject tm;
    private TrackData td;

    void OnGUI()
    {

        if (GameObject.Find("Track Manager") != null)
        {
            tm = GameObject.Find("Track Manager");
            if (tm.GetComponent<TrackData>() != null)
            {
                td = tm.GetComponent<TrackData>();
                {
                    if (GUILayout.Button("Add Node"))
                    {
                        AddNode();
                    }

                    if (td.affecter != null && td.affecter.Representation != null)
                    {
                        if (GUILayout.Button("Connect " + td.affecter.Representation.name + " to Node"))
                        {
                            ConnectNodes();                           
                        }

                        if (GUILayout.Button("Remove Connection to " + td.affecter.Representation.name))
                        {
                            RemoveConnection();
                        }

                        if (GUILayout.Button("Insert between " + td.affecter.Representation.name + " and Node"))
                        {
                            InsertNode();
                        }

                        GUILayout.Label("Current Affecter:" + td.affecter.Representation.name);

                    }
                    else
                    {
                        GUILayout.Label("Current Affecter: NULL");
                    }
                    
                    if (GUILayout.Button("Set Selection Parent"))
                    {
                        SetParent();
                        SceneView.RepaintAll();
                    }
                }
            }
        }
    }

    void SetParent()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<NodeHandler>() != null)
        {
            td.SetAffecter(Selection.activeTransform.GetComponent<NodeHandler>().myNode);
        }
    }

    void AddNode()
    {
        if(Selection.activeTransform != null && Selection.activeTransform.GetComponent<NodeHandler>() != null)
        {
            td.AddPoint(Selection.activeTransform.GetComponent<NodeHandler>().myNode);
        }
    }

    void ConnectNodes()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<NodeHandler>() != null)
        {
            td.ConnectNodes(Selection.activeTransform.GetComponent<NodeHandler>().myNode);
        }
    }

    void RemoveConnection()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<NodeHandler>() != null)
        {
            td.RemoveConnection(Selection.activeTransform.GetComponent<NodeHandler>().myNode);
        }
    }

    void InsertNode()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<NodeHandler>() != null)
        {
            td.InsertNode(Selection.activeTransform.GetComponent<NodeHandler>().myNode);
        }
    }

}

