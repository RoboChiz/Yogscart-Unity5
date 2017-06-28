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
}
