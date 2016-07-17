using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(TrackMaker))]
public class TrackMakerEditor : Editor
{
    static public bool EditorActive;
    static public int brushSize = 1, currentLayer = 1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!EditorActive && GUILayout.Button("Start editing"))
        {
            EditorActive = true;
            SceneView.RepaintAll();
        }

        if (EditorActive && GUILayout.Button("Stop editing"))
        {
            EditorActive = false;
            SceneView.RepaintAll();
        }

        if(GUILayout.Button("Clear Terrain"))
        {
            var trackMaker = target as TrackMaker;
            var terrain = trackMaker.terrain;

            terrain.Clear();
        }

        if (EditorActive)
        {

            brushSize = EditorGUILayout.IntSlider(brushSize, 1, 100);

            GUILayout.Label("LMB + Drag - Paint geometry");
            GUILayout.Label("Ctrl + LMB + Drag - Erase geometry");
        }
    }

    private void OnSceneGUI()
    {
        var trackMaker = target as TrackMaker;
        var terrain = trackMaker.terrain;

        // Don't do anything if editor isn't active
        if (!EditorActive)
            return;

        // Boilerplate for preventing default events 
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        // Raycast to edit plane
        var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var plane = new Plane(trackMaker.transform.up, trackMaker.transform.position + (trackMaker.transform.up * terrain[1].layerHeight));
        var hitEnter = 0.0f;
        Vector3 hitPoint = Vector3.zero;
        var raycastHitPlane = plane.Raycast(mouseRay, out hitEnter);
        if (raycastHitPlane)
        {
            hitPoint = mouseRay.origin + mouseRay.direction * hitEnter;
        }

        // Draw 3D GUI
        if (raycastHitPlane)
        {
            var c = Color.white;
            c.a = 0.25f;
            Handles.color = c;
            Handles.DrawSolidDisc(hitPoint, Vector3.up, brushSize);
        }

        // Referesh view on mouse move
        if (Event.current.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();
        }

        // Press escape to stop editing 
        if (Event.current.keyCode == KeyCode.Escape)
        {
            EditorActive = false;
            SceneView.RepaintAll();
            Repaint();
            return;
        }

        // Control click to start new terrain 
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0)
        {
            // Raycast to start new geo at hit point
            if (raycastHitPlane)
            {
                //Convert hit point to local position

                Vector3 clickPos = trackMaker.transform.TransformPoint(hitPoint);
                clickPos.x += (trackMaker.terrain.width * trackMaker.transform.localScale.x) / 2f;
                clickPos.z += (trackMaker.terrain.height * trackMaker.transform.localScale.z) / 2f;

                terrain[currentLayer].BrushStroke(new Vector2(clickPos.x, clickPos.z), brushSize);
                terrain.GenerateLayer(currentLayer);

                Selection.activeGameObject = trackMaker.gameObject;          
            }
        }
    }
}