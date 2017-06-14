﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode, RequireComponent(typeof(TrackData))]
public class InEngineRender : MonoBehaviour
{

    private TrackData td;
    public List<Color> roadColours;

    // Update is called once per frame
    void Update()
    {
        if (td == null)
            td = transform.GetComponent<TrackData>();

        if (td.spawnPoint != null)
        {
            Quaternion rot = td.spawnPoint.rotation;
            Vector3 centre = td.spawnPoint.position;

            Vector3 pos, pos1, pos2, pos3;
            pos = centre + (rot * Vector3.forward * -6.75f);
            pos1 = centre + (rot * Vector3.forward * 6.75f);
            pos2 = pos1 + (rot * Vector3.right * 39f);
            pos3 = pos + (rot * Vector3.right * 39f);

            Debug.DrawLine(pos, pos1, Color.blue);
            Debug.DrawLine(pos1, pos2, Color.blue);
            Debug.DrawLine(pos2, pos3, Color.blue);
            Debug.DrawLine(pos3, pos, Color.blue);
        }

        //Render Track Lines
        List<Transform> pp = td.positionPoints;

        bool valid = roadColours.Count == td.positionPoints.Count;

        if (pp != null && pp.Count >= 2)
        {
            for (int i = 0; i < pp.Count; i++)
            {
                if (td.loopedTrack || i < pp.Count - 1)
                {
                    Debug.DrawLine(pp[i].position, pp[MathHelper.NumClamp(i + 1, 0, pp.Count)].position,(!valid) ? Color.red : roadColours[i]);

                    //Draw Road Widths
                }
            }
        }
    }
}
