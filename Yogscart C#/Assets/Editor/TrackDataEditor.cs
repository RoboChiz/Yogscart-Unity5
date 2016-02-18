using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(TrackData))]
public class TrackDataEditor : EditorWindow
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

    [MenuItem("Track Editor/Create Position Point at end of queue")]
    static void CreatePositionPoint()
    {
        GameObject tm = GameObject.Find("Track Manager");
        if (tm != null && tm.GetComponent<TrackData>() != null)
        {
            tm.GetComponent<TrackData>().NewPoint();
        }
    }

    [MenuItem("Track Editor/Create Position Point after selection")]
    static void CreatePositionPointAfterSelection()
    {
        GameObject tm = GameObject.Find("Track Manager");
        TrackData td = tm.GetComponent<TrackData>();

        if (Selection.activeTransform != null && tm != null && td != null)
        {
            Transform cs = Selection.activeTransform;
            int index = td.positionPoints.FindIndex(o => o == cs);

            if (index >= 0 && index < td.positionPoints.Count)
                td.AddPoint(index+1);

        }
    }

    [MenuItem("Track Editor/Create Short Cut")]
    static void CreateShortCut()
    {
        GameObject tm = GameObject.Find("Track Manager");
        TrackData td = tm.GetComponent<TrackData>();

        if (Selection.activeTransform != null && tm != null && td != null)
        {
            Transform cs = Selection.activeTransform;
            int index = td.positionPoints.FindIndex(o => o == cs);

            if (index >= 0 && index < td.positionPoints.Count)
                td.CreateShortcut(index);

        }
    }

    [MenuItem("Track Editor/Create Short Cut Point at end of queue")]
    static void CreateShortCutPoint()
    {
        GameObject tm = GameObject.Find("Track Manager");
        TrackData td = tm.GetComponent<TrackData>();

        if (Selection.activeTransform != null && tm != null && td != null)
        {
            Transform cs = Selection.activeTransform;

            foreach (ShortCut sc in td.shortCuts)
            {
                int index = sc.positionPoints.FindIndex(o => o == cs);

                if (index >= 0 && index < sc.positionPoints.Count)
                {
                    td.CreateShortcutPoint(index, sc);
                    break;
                }
            }
        }
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
