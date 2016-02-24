using UnityEngine;
using System.Collections;
using RobsNodes;

public class TestScript : MonoBehaviour {

    NodeTree test;

	// Use this for initialization
	void Start ()
    {
        test = new NodeTree();

        test.NewNode(NewPoint(0, 0));

        NodeTree.Node tempNode = test.StartNode;
        tempNode = test.AddNode(tempNode, NewPoint(2, 2));
        tempNode = test.AddNode(tempNode, NewPoint(4, 2));

        NodeTree.Node currentNode = test.AddNode(test.StartNode, NewPoint(2, 0));   
        currentNode = test.AddNode(currentNode, NewPoint(4, 0));
        currentNode = test.AddNode(currentNode, NewPoint(6, 0));
        test.ConnectNodes(tempNode, currentNode);

        tempNode = currentNode;
        tempNode = test.AddNode(tempNode, NewPoint(10, 2));

        currentNode = test.AddNode(currentNode, NewPoint(8, 0));
        currentNode = test.AddNode(currentNode, NewPoint(10, 0));
        currentNode = test.AddNode(currentNode, NewPoint(12, 0));
        currentNode = test.AddNode(currentNode, NewPoint(14, 0));
        test.ConnectNodes(tempNode, currentNode);

        currentNode = test.AddNode(currentNode, NewPoint(16, 0));

        test.Computate();


    }
	
    Transform NewPoint(float xMod, float zMod)
    {
        GameObject obj = new GameObject();
        obj.transform.position = new Vector3(xMod, 0, zMod);
        return obj.transform;
    }
	
}
