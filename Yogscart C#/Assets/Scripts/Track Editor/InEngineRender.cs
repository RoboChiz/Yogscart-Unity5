﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode, RequireComponent(typeof(TrackData))]
public class InEngineRender : MonoBehaviour
{
    private TrackData td;

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
        foreach(PointConnector pc in td.connections)
        {
            if(pc.a != null && pc.b != null)
                Debug.DrawLine(pc.a.transform.position, pc.b.transform.position, Color.red);

            if(pc.a.oneWay)
            {
                Vector3 forwardDir = (pc.b.transform.position - pc.a.transform.position).normalized;
                Vector3 rightDir = (Quaternion.AngleAxis(-90f, Vector3.up) * forwardDir).normalized;

                Debug.DrawLine(pc.a.transform.position, pc.a.transform.position - forwardDir + rightDir, Color.green);
                Debug.DrawLine(pc.a.transform.position, pc.a.transform.position - forwardDir - rightDir, Color.green);
            }
        }
        
    }
}
