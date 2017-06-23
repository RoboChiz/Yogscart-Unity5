using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LayoutEditorWindow : EditorWindow
{

    private string fileName = "New Track";

    [MenuItem("Layout Creator/Show Toolbar")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LayoutEditorWindow));
    }

    [MenuItem("Layout Creator/Create Layout Generator")]
    public static void CreateGenerator()
    {
        GameObject newGameObject = new GameObject("Track Modeller");
        newGameObject.AddComponent<TrackGenerator>();
    }

    [MenuItem("Layout Creator/Clean Layout")]
    public static void CleanGenerator()
    {
        TrackGenerator tg = FindObjectOfType<TrackGenerator>();
        tg.CleanNetwork();
    }

    void OnGUI()
    {
        TrackGenerator tg = FindObjectOfType<TrackGenerator>();

        GUILayout.BeginHorizontal();

        if (tg != null)
        {
            //Create New Node Button
            if (GUILayout.Button("New Node"))
            {
                CreateNode();
            }

            //Create a straight road between two selected nodes
            if (GUILayout.Button("Create Straight Road"))
            {
                CreateRoad(NodeConnector.ConnectionType.Straight);
                EditorUtility.SetDirty(tg);
            }

            //Create a curved road between two selected nodes
            if (GUILayout.Button("Create Curved Road"))
            {
                CreateRoad(NodeConnector.ConnectionType.Curve);
            }

            //Delete a road between two nodes
            if (GUILayout.Button("Delete Road"))
            {
                DeleteRoad();
                EditorUtility.SetDirty(tg);
            }

            //Swap Node of a Road
            if (GUILayout.Button("Swap Nodes"))
            {
                SwapRoad();
                EditorUtility.SetDirty(tg);
            }

            GUILayout.BeginVertical();

            fileName = GUILayout.TextField(fileName);

            //Export Mesh as Obj
            if (GUILayout.Button("Export as OBJ"))
            {
                if (fileName != "")
                {
                    OBJExporter.SaveMeshAsOBJ(fileName, tg.GetComponent<MeshFilter>().sharedMesh);
                }
            }

            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("Use 'Layout Creator/Create Layout Generator' to create a Track Generator Object to design level layouts.");
        }

        GUILayout.EndHorizontal();
    }

    public static void CreateNode()
    {
        GameObject newGameObject = new GameObject("Node " + (FindObjectsOfType<Node>().Length + 1));
        newGameObject.AddComponent<Node>();

        if (Selection.activeGameObject != null)
            newGameObject.transform.position = Selection.activeGameObject.transform.position;

        Selection.activeGameObject = newGameObject;

        newGameObject.transform.parent = FindObjectOfType<TrackGenerator>().transform;
    }

    public static void CreateRoad(NodeConnector.ConnectionType connectionType)
    {
        //Get Track Generator
        TrackGenerator tg = FindObjectOfType<TrackGenerator>();
        if (tg != null)
        {
            //Check we have selected 2 node
            if (Selection.gameObjects.Length == 2)
            {
                Node a = Selection.gameObjects[0].GetComponent<Node>(), b = Selection.gameObjects[1].GetComponent<Node>();

                if (a != null && b != null)
                {
                    //Check that a road dosen't already exist
                    if (!tg.HasRoad(a, b))
                    {
                        List<Transform> extra = new List<Transform>();

                        //If a curved road create a anchor node
                        if (connectionType == NodeConnector.ConnectionType.Curve)
                        {
                            GameObject anchorPoint = new GameObject("Node ("
                                + a.name.Substring(5, a.name.Length - 5) + "&"
                                + b.name.Substring(5, b.name.Length - 5) + ") Anchor");

                            anchorPoint.AddComponent<AnchorPoint>();
                            extra.Add(anchorPoint.transform);
                            anchorPoint.transform.position = (a.transform.position + b.transform.position) / 2f;
                        }

                        tg.connections.Add(new NodeConnector(a, b, NodeConnector.ConnectionType.Curve, extra));
                    }
                }
            }
        }
    }

    public static void DeleteRoad()
    {
        //Get Track Generator
        TrackGenerator tg = FindObjectOfType<TrackGenerator>();
        if (tg != null)
        {
            //Check we have selected 2 node
            if (Selection.gameObjects.Length == 2)
            {
                Node a = Selection.gameObjects[0].GetComponent<Node>(), b = Selection.gameObjects[1].GetComponent<Node>();

                if (a != null && b != null)
                {
                    //Check that a road dosen't already exist
                    if (tg.HasRoad(a, b))
                    {
                        foreach (NodeConnector nc in tg.connections.ToArray())
                        {
                            if ((nc.a == a && nc.b == b) || (nc.a == b && nc.b == a))
                            {
                                foreach (Transform extra in nc.extras)
                                    DestroyImmediate(extra.gameObject);

                                tg.connections.Remove(nc);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public static void SwapRoad()
    {
        //Get Track Generator
        TrackGenerator tg = FindObjectOfType<TrackGenerator>();
        if (tg != null)
        {
            //Check we have selected 2 node
            if (Selection.gameObjects.Length == 2)
            {
                Node a = Selection.gameObjects[0].GetComponent<Node>(), b = Selection.gameObjects[1].GetComponent<Node>();

                if (a != null && b != null)
                {
                    //Check that a road dosen't already exist
                    if (tg.HasRoad(a, b))
                    {
                        foreach (NodeConnector nc in tg.connections.ToArray())
                        {
                            if ((nc.a == a && nc.b == b) || (nc.a == b && nc.b == a))
                            {
                                Node newB = nc.a, newA = nc.b;
                                nc.a = newA;
                                nc.b = newB;
                            }
                        }
                    }
                }
            }
        }
    }
}
