using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RobsNodes;

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

        if(td.positionPoints != null && td.positionPoints.StartNode != null)
            DrawNode(td.positionPoints.StartNode);

    }

    void DrawNode(NodeTree.Node currentNode)
    {
        if(currentNode.next.Count > 0)
        {
            for(int i = 0; i < currentNode.next.Count; i++)
            {
                if(currentNode.representation != null)
                    Debug.DrawLine(currentNode.representation.position, currentNode.next[i].representation.position, Color.red);

                DrawNode(currentNode.next[i]);
            }
        }
    }
}
