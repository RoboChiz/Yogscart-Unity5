
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


public class TrackWindow : EditorWindow
{
    private TrackData td;

    private int introPanSize;
    private List<bool> introShowing = new List<bool>();

    [MenuItem("Track Editor/Show Track Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TrackWindow));
    }
	

	// Update is called once per frame
	void OnGUI()
    {
		if(td == null)
        {
            td = FindObjectOfType<TrackData>();

            if (td != null)
                introPanSize = td.introPans.Count;

            GUI.Label(new Rect(0, 0, position.width, 50), "Please create a Track Manager!");
        }
        else
        {
            GUILayout.BeginVertical();

            //Window Name
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Yogscart Track Editor");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Connections: " + td.connections.Count.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            Color startColour = Color.black;

            if (td.trackErrors == null || td.trackErrors.Count == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUI.skin.label.normal.textColor = new Color(0.1f, 0.68f, 0.1f, 1f);
                GUILayout.Label("Track works!");

               GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                foreach(TrackData.TrackErrors te in td.trackErrors)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUI.skin.label.normal.textColor = Color.red;
                    GUILayout.Label(te.ToString());

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }

            GUI.skin.label.normal.textColor = startColour;
            GUILayout.Space(10f);

            GUILayout.Label("Track Name:");
            td.trackName = GUILayout.TextField(td.trackName);

            GUILayout.Label("Looped: " + td.loopedTrack);

            if(!td.loopedTrack)
                GUILayout.Label("Laps: " + td.Laps);
            else
            {
                GUILayout.Space(10f);
                GUILayout.Label("Laps:");
                td.Laps = Mathf.Clamp(EditorGUILayout.IntField(td.Laps), 1, 10);
            }

            GUILayout.Space(10f);

            GUILayout.Label("Background Music:");
            td.backgroundMusic = (AudioClip)EditorGUILayout.ObjectField(td.backgroundMusic, typeof(AudioClip));          

            GUILayout.Space(10f);

            GUILayout.Label("UI Map Texture:");
            td.map = (Texture2D)EditorGUILayout.ObjectField(td.map, typeof(Texture2D));

            GUILayout.Label("UI Map Corners (Where are the corners of the map in World Space):");

            if(td.mapEdgesInWorldSpace == null || td.mapEdgesInWorldSpace.Length != 3)
            {
                td.mapEdgesInWorldSpace = new Vector3[3];
            }

            td.mapEdgesInWorldSpace[0] = EditorGUILayout.Vector3Field("Top Left Corner", td.mapEdgesInWorldSpace[0]);
            td.mapEdgesInWorldSpace[1] = EditorGUILayout.Vector3Field("Bottom Left Corner", td.mapEdgesInWorldSpace[1]);
            td.mapEdgesInWorldSpace[2] = EditorGUILayout.Vector3Field("Top Right Corner", td.mapEdgesInWorldSpace[2]);

            GUILayout.Space(10f);

            GUILayout.Label("Intro Pans:");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size:");
            introPanSize = EditorGUILayout.IntField(introPanSize);
            GUILayout.EndHorizontal();

            //Add Pans until Size is met
            while (introPanSize > td.introPans.Count)
                td.introPans.Add(new CameraPoint());

            //Remove pans until size is met
            while (introPanSize < td.introPans.Count)
                td.introPans.RemoveAt(td.introPans.Count-1);

            //Add bools until Size is met
            while (introPanSize > introShowing.Count)
                introShowing.Add(false);

            //Remove bools until size is met
            while (introPanSize < introShowing.Count)
                introShowing.RemoveAt(introShowing.Count - 1);

            int count = 0;
            foreach (CameraPoint introPoint in td.introPans)
            {
                introShowing[count] = EditorGUILayout.Foldout(introShowing[count], "Intro Pan " + count.ToString());

                if (introShowing[count])
                {
                    GUILayout.Label("Travel Time:");
                    introPoint.travelTime = EditorGUILayout.FloatField(introPoint.travelTime);

                    introPoint.startPoint = EditorGUILayout.Vector3Field("Start Position", introPoint.startPoint);
                    introPoint.startRotation = EditorGUILayout.Vector3Field("Start Rotation", introPoint.startRotation);

                    if (GUILayout.Button("Paste transformation of Selected Object") && Selection.activeGameObject != null)
                    {
                        introPoint.startPoint = Selection.activeGameObject.transform.position;
                        introPoint.startRotation = Selection.activeGameObject.transform.rotation.eulerAngles;
                    }

                    introPoint.endPoint = EditorGUILayout.Vector3Field("End Position", introPoint.endPoint);
                    introPoint.endRotation = EditorGUILayout.Vector3Field("End Rotation", introPoint.endRotation);

                    if (GUILayout.Button("Paste transformation of Selected Object") && Selection.activeGameObject != null)
                    {
                        introPoint.endPoint = Selection.activeGameObject.transform.position;
                        introPoint.endRotation = Selection.activeGameObject.transform.rotation.eulerAngles;
                    }
                }

                count++;
            }

            GUILayout.BeginHorizontal();

            if(GUILayout.Button("Create Point"))
            {
                Transform newPoint = CreatePoint();
                Selection.activeTransform = newPoint.gameObject.transform;
            }

            if (GUILayout.Button("Create Point And Connect"))
            {
                if (Selection.activeGameObject != null)
                {
                    PointHandler ph = Selection.activeGameObject.GetComponent<PointHandler>();

                    if (ph != null && ph.style != PointHandler.Point.Spawn)
                    {
                        Transform newPoint = CreatePoint();
                        newPoint.position = Selection.activeGameObject.transform.position;

                        td.connections.Add(new PointConnector(ph, newPoint.GetComponent<PointHandler>()));
                        Selection.activeTransform = newPoint.gameObject.transform;
                    }
                }
            }

            if (GUILayout.Button("Connect Two Points"))
            {
                if(Selection.gameObjects != null && Selection.gameObjects.Length == 2)
                {
                    PointHandler one = Selection.gameObjects[0].GetComponent<PointHandler>();
                    PointHandler two = Selection.gameObjects[1].GetComponent<PointHandler>();

                    td.connections.Add(new PointConnector(one, two));

                    EditorUtility.SetDirty(td);
                }
            }

            if (GUILayout.Button("Disconnect Two Points"))
            {
                if (Selection.gameObjects != null && Selection.gameObjects.Length == 2)
                {
                    PointHandler one = Selection.gameObjects[0].GetComponent<PointHandler>();
                    PointHandler two = Selection.gameObjects[1].GetComponent<PointHandler>();

                    foreach (PointConnector pc in td.connections.ToArray())
                    {
                        if ((pc.a == one && pc.b == two) || (pc.a == two && pc.b == one))
                        { 
                            td.connections.Remove(pc);
                            break;
                        }
                    }

                    EditorUtility.SetDirty(td);
                }
            }



            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


        }  
    }

    private Transform CreatePoint()
    {
        GameObject obj = new GameObject();
        obj.transform.parent = GameObject.Find("Track Manager").transform;
        obj.AddComponent<PointHandler>();

        return obj.transform;
    }
}
#endif