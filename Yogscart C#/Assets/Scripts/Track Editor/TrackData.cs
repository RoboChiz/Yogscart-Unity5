using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.name = "Track Manager";

            if (introPans == null)
            {
                introPans = new List<CameraPoint>();
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

        }
    }

//#if UNITY_EDITOR
   //     Selection.activeTransform = obj.transform;
//#endif

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