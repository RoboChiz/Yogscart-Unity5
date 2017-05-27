using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

//TrackData Script - V3.0
//Created by Robert (Robo_Chiz)
//Do not edit this script, doing so may cause compatibility errors.

[ExecuteInEditMode, System.Serializable, RequireComponent(typeof(InEngineRender))]
public class TrackData : MonoBehaviour
{

    //Track Metadata
    public string trackName = "Untitled Track";
    public AudioClip backgroundMusic;
    public float lastLapPitch = 1.1f; //The pitch of the Audio Source on the last lap of the race

    public bool loopedTrack = true;

    public int Laps = 3;

    public Texture2D map;
    public Vector3[] mapEdges;

    public Vector2 mapOffset; //The position in the scene where the map begins from
    public Vector2 mapScale; //How the map scales. 1 = 1 pixel per metre
    public float mapRotate;

    [HideInInspector]
    public Transform spawnPoint;
    //DEBUG[HideInInspector]
    public List<Transform> positionPoints;

    public List<ShortCut> shortCuts;
    public List<CameraPoint> introPans;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.name = "Track Manager";

            if (positionPoints == null)
            {
                positionPoints = new List<Transform>();
                shortCuts = new List<ShortCut>();
                introPans = new List<CameraPoint>();
            }

            if (positionPoints.Count > 2)
            {
                if (loopedTrack)
                {
                    //If lap point is lap point in looped track
                    if (positionPoints[positionPoints.Count - 1].GetComponent<PointHandler>().style == PointHandler.Point.Lap)
                    {
                        loopedTrack = false;
                    }
                }
                else
                {
                    //If lap point is lap point in looped track
                    if (positionPoints[positionPoints.Count - 1].GetComponent<PointHandler>().style == PointHandler.Point.Position)
                    {
                        loopedTrack = true;
                    }
                }
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

            //Check that Position Points are in the correct format
            int lapCount = 0;
            for (int i = 0; i < positionPoints.Count; i++)
            {
                if (positionPoints[i] == null) //If spot is blank, delete it from the list
                    positionPoints.RemoveAt(i);

                if (i == 0)
                {
                    positionPoints[0].GetComponent<PointHandler>().style = PointHandler.Point.Lap;
                }

                PointHandler.Point point = positionPoints[i].GetComponent<PointHandler>().style;
                switch (point)
                {
                    case PointHandler.Point.Position:
                        positionPoints[i].name = "Position Point " + i;
                        break;
                    case PointHandler.Point.Shortcut:
                        positionPoints[i].name = "Shortcut Point " + i;
                        break;
                    case PointHandler.Point.Lap:
                        positionPoints[i].name = "Lap Point " + i;
                        lapCount++;
                        break;
                }
            }

            if(!loopedTrack)
                Laps = lapCount;

            for (int i = 0; i < shortCuts.Count; i++)
            {
                for (int j = 0; j < shortCuts[i].positionPoints.Count; j++)
                {
                    if (shortCuts[i].positionPoints[j] == null)
                        shortCuts[i].positionPoints.RemoveAt(j);
                    else
                    {
                        shortCuts[i].positionPoints[j].name = "ShortCut Point " + j;
                        shortCuts[i].positionPoints[j].parent = positionPoints[shortCuts[i].startPoint];
                    }
                }

                if (shortCuts[i].positionPoints.Count == 0)
                    shortCuts.RemoveAt(i);
            }

            if (positionPoints.Count == 0)
                NewPoint();
        }
    }

    Transform CreatePoint()
    {
        GameObject obj = new GameObject();
        obj.transform.parent = GameObject.Find("Track Manager").transform;
        obj.AddComponent<PointHandler>();

#if UNITY_EDITOR
        Selection.activeTransform = obj.transform;
#endif

        return obj.transform;

    }

    public void NewPoint()
    {
        Transform obj = CreatePoint();
        Vector3 pos = spawnPoint.position;
        if (positionPoints.Count > 0)
            pos = positionPoints[positionPoints.Count - 1].position;
        obj.position = pos;

        positionPoints.Add(obj.transform);

    }

    public void AddPoint(int value)
    {
        if (value >= positionPoints.Count)
            NewPoint();
        else
        {
            Transform obj = CreatePoint();
            obj.position = positionPoints[value-1].position;
            positionPoints.Insert(value, obj);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}

[System.Serializable]
public class ShortCut
{
    public int startPoint, endPoint;
    public List<Transform> positionPoints;

    public enum ShortCutType { BoostRequired, NeedSmarts, SplitPath };
    ShortCutType sct = ShortCutType.NeedSmarts;

    public ShortCut(int sp)
    {
        startPoint = sp;
    }

}

[System.Serializable]
public class CameraPoint
{
    public Vector3 startPoint, endPoint, startRotation, endRotation;
    public float travelTime = 3f;
}