using UnityEngine;
using UnityEditor;
using System.Collections;
using RobsNodes;

public class NodeHandler : MonoBehaviour
{

    public NodeTree.Node myNode;
    public bool selected = false;

    // Used to draw Sphere on point
    void OnDrawGizmos()
    {
        if (myNode != null)
        {
            if (selected)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                switch (myNode.style)
                {
                    case NodeTree.Node.NodeType.Normal:
                        Gizmos.color = Color.red;
                        break;
                    case NodeTree.Node.NodeType.Lap:
                        Gizmos.color = Color.blue;
                        break;
                    case NodeTree.Node.NodeType.BoostRequired:
                        Gizmos.color = Color.green;
                        break;
                }
            }
        }

        Gizmos.DrawSphere(transform.position, 0.75f);
    }
}