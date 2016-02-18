using UnityEngine;
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
        List<Transform> pp = td.positionPoints;

        if (pp != null && pp.Count >= 2)
        {

            Transform lastPoint;
            if (td.loopedTrack)
                lastPoint = pp[pp.Count - 1];
            else
                lastPoint = pp[1];

            float adjusterFloat = lastPoint.GetComponent<PointHandler>().roadWidth;

            if (td.loopedTrack)
                adjusterFloat *= -1f;

            Vector3 lastAdjustOne = Vector3.Cross((pp[0].position - lastPoint.position).normalized, transform.up) * -adjusterFloat;
            Vector3 lastAdjustTwo = Vector3.Cross((pp[0].position - lastPoint.position).normalized, transform.up) * adjusterFloat;

            for (int i = 0; i < pp.Count; i++)
            {
                adjusterFloat = pp[i].GetComponent<PointHandler>().roadWidth;

                if (td.loopedTrack || i < pp.Count - 1)
                {
                    int nextPos = MathHelper.NumClamp(i + 1, 0, pp.Count);

                    Debug.DrawLine(pp[i].position, pp[nextPos].position,Color.red);

                    Vector3 adjustOne = Vector3.Cross((pp[nextPos].position - pp[i].position).normalized, transform.up) * adjusterFloat;
                    Vector3 adjustTwo = Vector3.Cross((pp[nextPos].position - pp[i].position).normalized, transform.up) * -adjusterFloat;

                    Debug.DrawLine(pp[i].position + lastAdjustOne, pp[nextPos].position + adjustOne, Color.cyan);
                    Debug.DrawLine(pp[i].position + lastAdjustTwo, pp[nextPos].position + adjustTwo, Color.cyan);

                    lastAdjustOne = adjustOne;
                    lastAdjustTwo = adjustTwo;
                }
            }
        }

        List<ShortCut> sc = td.shortCuts;

        if(sc != null)
        {
            for(int i = 0; i < sc.Count; i++)
            {
                ShortCut si = td.shortCuts[i];

                if(si.positionPoints != null && si.positionPoints.Count > 0)
                {
                    if (pp[si.startPoint] != null)
                        Debug.DrawLine(pp[si.startPoint].position, si.positionPoints[0].position, Color.red);
                    if (pp[si.endPoint] != null)
                        Debug.DrawLine(pp[si.endPoint].position, si.positionPoints[si.positionPoints.Count - 1].position, Color.red);

                    int nextPos = MathHelper.NumClamp(si.startPoint + 1, 0, pp.Count);
                    float adjusterFloat = pp[si.startPoint].GetComponent<PointHandler>().roadWidth;

                    Vector3 lastAdjustOne = Vector3.Cross((pp[nextPos].position - pp[si.startPoint].position).normalized, transform.up) * adjusterFloat;
                    Vector3 lastAdjustTwo = Vector3.Cross((pp[nextPos].position - pp[si.startPoint].position).normalized, transform.up) * -adjusterFloat;

                    for (int j = 0; j < si.positionPoints.Count - 1; j++)
                    {
                        Debug.DrawLine(si.positionPoints[j].position, si.positionPoints[j + 1].position, Color.red);

                        adjusterFloat = si.positionPoints[j + 1].GetComponent<PointHandler>().roadWidth;
                        Vector3 adjustOne = Vector3.Cross((si.positionPoints[j + 1].position - si.positionPoints[j].position).normalized, transform.up) * adjusterFloat;
                        Vector3 adjustTwo = Vector3.Cross((si.positionPoints[j + 1].position - si.positionPoints[j].position).normalized, transform.up) * -adjusterFloat;

                        Debug.DrawLine(si.positionPoints[j].position + lastAdjustOne, si.positionPoints[j + 1].position + adjustOne, Color.cyan);
                        Debug.DrawLine(si.positionPoints[j].position + lastAdjustTwo, si.positionPoints[j + 1].position + adjustTwo, Color.cyan);


                        lastAdjustOne = adjustOne;
                        lastAdjustTwo = adjustTwo;
                    }

                }
            }
        }
    }
}
