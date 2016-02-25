using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Node & NodeTree Script - V1.0 (Feb 2016)
//Created by Robert (Robo_Chiz)
//Do not edit this script, doing so may cause compatibility errors.
namespace RobsNodes
{
    public class NodeTree
    {
        //Node Class, used instead of Position Points from V1.0 - 3.0
        public class Node
        {

            public Transform Representation
            {
                get { return representation; }
                set { }
            } //Public accessor to Representation, cannot be changed
            internal Transform representation;

            public float roadWidth = 5f; //Width of the road at this Node

            internal List<Node> next; //All Nodes that follow this one
            internal List<Node> previous;//All Nodes that come before this one

            public float Length
            {
                get { return length; }
                set { }
            } //The length of the path up to this node
            internal float length = -1f;

            public float Value
            {
                get { return value; }
                set { }
            }//The amount to add to Position Finding when this Node is reached
            internal float value = -1f;

            public enum NodeType { Normal, Lap, BoostRequired };
            public NodeType style = NodeType.Normal;

            public Node(Transform t)
            {
                representation = t;
                next = new List<Node>();
                previous = new List<Node>();
                value = -1f;
                length = -1f;
            }
        }

        private Node startNode; //The start of the Node Tree
        public Node StartNode
        {
            get { return startNode; }
            set { }
        }

        private Node endNode; //The end of the Node Tree. By default the node with the longest length
        public Node EndNode
        {
            get { return EndNode; }
            set { }
        }

        private float length;//The length of the default path (Longest path through tree)
        public float Length
        {
            get { return length; }
            set { }
        }

        private List<List<Node>> paths;

        private List<Node> mainTrack;
        public List<Node> MainTrack
        {
            get { return mainTrack; }
            set { }
        }//AKA The longest path through the tree


        /// <summary>
        /// Checks to see if a Node exists as a child of another Node
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="findNode"></param>
        /// <returns></returns>
        private bool CheckNode(Node currentNode, Node findNode)
        {
            if (currentNode == findNode)
            {
                return true;//The Node has been found!
            }
            else
            {
                //Otherwise check all the child of all the current Node's children
                foreach (Node child in currentNode.next)
                {
                    if (CheckNode(child, findNode))
                        return true;
                }

                //If no Children or Node not found
                return false;
            }
        }

        /// <summary>
        /// Returns the length between two Nodes
        /// </summary>
        /// <param name="nodeOne"></param>
        /// <param name="nodeTwo"></param>
        /// <returns></returns>
        public float GetLength(Node nodeOne, Node nodeTwo)
        {
            //Find the Node closest to the Start
            if (nodeOne.length <= nodeTwo.length)
            {
                if (CheckNode(nodeOne, nodeTwo)) //Check that both Nodes exist on the same path
                    return nodeTwo.length - nodeOne.length;
            }
            else
            {

                if (CheckNode(nodeTwo, nodeOne)) //Check that both Nodes exist on the same path
                    return nodeOne.length - nodeTwo.length;
            }

            return -1; //Either one or both Nodes don't exist in this tree
        }

        public Node CreateNode(Transform rep)
        {
            Node nNode = new Node(rep);

            rep.gameObject.AddComponent<NodeHandler>();
            rep.GetComponent<NodeHandler>().myNode = nNode;

            return nNode;
        }

        /// <summary>
        /// Creates a new Node in the Start Node if one dosen't exist
        /// </summary>
        public Node NewNode(Transform rep)
        {
            if (startNode == null)
            {
                Node nNode = CreateNode(rep);
                startNode = nNode;
                return nNode;
            }
            else
            {
                throw new System.Exception("This tree already has a Start Node. Try using the AddNode function!");
            }
        }

        /// <summary>
        /// Adds a new Node after the provided Node
        /// </summary>
        /// <param name="parent">The parent to the new Node</param>
        public Node AddNode(Node parent, Transform rep)
        {
            Node nNode = CreateNode(rep);
            ConnectNodes(parent, nNode);
            return nNode;
        }

        /// <summary>
        /// Inserts a new Node between two existing Nodes
        /// </summary>
        /// <param name="nodeOne">Parent Node</param>
        /// <param name="nodeTwo">Child Node</param>
        public Node InsertNode(Node nodeOne, Node nodeTwo, Transform rep)
        {
            Node nNode = CreateNode(rep);

            nNode.next.Add(nodeTwo);
            nNode.previous.Add(nodeOne);

            nodeOne.next.Remove(nodeTwo);
            nodeOne.next.Add(nNode);

            nodeTwo.previous.Remove(nodeOne);
            nodeTwo.previous.Add(nNode);

            return nNode;
        }

        /// <summary>
        /// Connects two Nodes together
        /// </summary>
        /// <param name="nodeOne">Parent Node</param>
        /// <param name="nodeTwo">Child Node</param>
        public void ConnectNodes(Node nodeOne, Node nodeTwo)
        {
            nodeOne.next.Add(nodeTwo);
            nodeTwo.previous.Add(nodeOne);
        }

        /// <summary>
        /// Removes a Node from the tree
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(Node node)
        {
            if (node.representation != null)
                Destroyer.DestroyGameObject(node.representation.gameObject);

            List<Node> parents = node.previous;
            List<Node> children = node.next;

            //If there is only one Parent and Child connect them together
            if (parents.Count == 1 && children.Count == 1)
            {
                parents[0].next.Add(children[0]);
                children[0].previous.Add(parents[0]);
            }

            foreach (Node parent in parents)
            {
                parent.next.Remove(node);
            }
            foreach (Node child in children)
            {
                child.previous.Remove(node);
            }
        }

        /// <summary>
        /// Calculates length & value for each Node. Also checks that the tree is valid.
        /// </summary>
        public void Computate()
        {
            //Find every path through the Tree
           
            length = 0;
            mainTrack = new List<Node>();
            paths = new List<List<Node>>();
            List<Node> checkPath = new List<Node>();

            FindPaths(checkPath, startNode, true);

            //Set Node values
            for (int i = 0; i < mainTrack.Count; i++)
            {
                mainTrack[i].length = i;
                mainTrack[i].value = 1;
            }

            paths.Remove(mainTrack);

            //Sort paths so that they are in length order
            paths.Sort((a, b) => a.Count - b.Count);

            foreach (List<Node> path in paths)
            {
                float startLength = 0f;

                List<Node> shortcut = new List<Node>();

                if (path[path.Count - 1].length == -1 || path[path.Count - 1].value == -1)
                {
                    throw new System.Exception("There is an unconnected shortcut in this track!");
                }
                    
               for(int i = 0; i < path.Count; i++)
                {
                    if (path[i].length != -1 && path[i].value != -1)
                    {
                        if(shortcut.Count > 0)//We must be at the end of a path
                        {
                            float Increment = (path[i].length - startLength) / (shortcut.Count + 1);
                            Debug.Log("Start:" + startLength + " Current:" + path[i].length + " Increment:" + Increment);

                            for (int j = 0; j < shortcut.Count; j++)
                            {
                                shortcut[j].length = startLength + (Increment * (j + 1));
                                shortcut[j].value = Increment;
                            }

                            shortcut = new List<Node>();
                        }
                        startLength = path[i].length;
                    }
                    else
                    {
                        shortcut.Add(path[i]);
                    }
                    
                }
            }
        }

        private void FindPaths(List<Node> currentPath, Node currentNode, bool canBeMain)
        {
            //Add the Current Node to the Path
            currentPath.Add(currentNode);

            if (currentNode.style == Node.NodeType.BoostRequired)
                canBeMain = false;

            //If at the end of a path
            if (currentNode.next.Count == 0)
            {
                paths.Add(new List<Node>(currentPath));

                //Calculate Main Path .etc
                if(currentPath.Count > length && canBeMain)
                {
                    length = paths[paths.Count - 1].Count;
                    endNode = currentNode;
                    mainTrack = paths[paths.Count-1];
                }

            }
            else
            {
                foreach(Node n in currentNode.next)
                {
                    FindPaths(currentPath, n, canBeMain);
                }
            }

            //Remove the Current Node and Backtrack
            currentPath.Remove(currentNode);
            return;
        }

    }

    public class Destroyer : MonoBehaviour
    {
        public static void DestroyGameObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }
    }
}