using UnityEngine;
using UnityEditor;
using System.Collections;
using RobsNodes;

public class NodeHandler : MonoBehaviour
{

    public NodeTree.Node myNode;

    // Used to draw Sphere on point
    void OnDrawGizmos()
    {
        if (myNode != null)
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

        Gizmos.DrawSphere(transform.position, 0.75f);
    }
}