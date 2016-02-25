using UnityEngine;
using UnityEditor;
using System.Collections;
using RobsNodes;

[CustomEditor(typeof(NodeHandler))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodeHandler myTarget = (NodeHandler)target;
        NodeTree.Node node = myTarget.myNode;

        node.style = (NodeTree.Node.NodeType)EditorGUILayout.EnumPopup("Type:", node.style);
        node.roadWidth = EditorGUILayout.FloatField("Road Width:", node.roadWidth);

        EditorGUILayout.LabelField("Value:" + node.Value);
        EditorGUILayout.LabelField("Length:" + node.Length);

    }
}
