using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapViewer : MonoBehaviour
{

    public float mapAlpha = 1f;

    private TrackData td;

    public List<MapObject> objects = new List<MapObject>();

    public void ShowMapViewer()
    {
        StartCoroutine(FadeMapTo(1f));
    }

    public void HideMapViewer()
    {
        StartCoroutine(FadeMapTo(0f));
    }

    private IEnumerator FadeMapTo(float finalVal)
    {
        float travelTime = 0.5f;
        float startTime = Time.time, startVal = mapAlpha;

        while(Time.time - startTime < travelTime)
        {
            mapAlpha = Mathf.Lerp(startVal, finalVal, (Time.time - startTime) / travelTime);
            yield return null;
        }

        mapAlpha = finalVal;
    }

    // Update is called once per frame
    void OnGUI()
    {
        if(td == null)
            td = FindObjectOfType<TrackData>();

        //Set the Alpha
        GUIHelper.SetGUIAlpha(mapAlpha);

        if (td != null && mapAlpha > 0f)
        {
            //Draw Map
            Rect drawRect;
            float mapSize = Mathf.Min(Screen.width, Screen.height) / 3f;

            if (InputManager.controllers.Count == 1)//Put Map in bottom left corner
                drawRect = new Rect(Screen.width - mapSize - 10, (Screen.height / 2f) - (mapSize / 2f), mapSize, mapSize);
            else //Put Map in centre of the Screen
                drawRect = new Rect((Screen.width / 2f) - (mapSize / 2f), (Screen.height / 2f) - (mapSize / 2f), mapSize, mapSize);

            GUI.DrawTexture(drawRect, td.map);

            //Draw Icons
            float iconSize = mapSize / 8f;

            List<MapObject> toDraw = new List<MapObject>(objects);

            while(toDraw.Count > 0)
            {
                //Get Object with highest Depth
                int highestDepth = 0;
                for (int i = 1; i < toDraw.Count; i++)
                    if (toDraw[i].depth > toDraw[highestDepth].depth)
                        highestDepth = i;

                //Draw the highest object
                MapObject mapObject = toDraw[highestDepth];

                if (mapObject.transform != null && mapObject.icon != null)
                {
                    Vector3 edgeOne = Vector3.Scale(td.mapEdgesInWorldSpace[1], new Vector3(1f, 0f, 1f)) - Vector3.Scale(td.mapEdgesInWorldSpace[0], new Vector3(1f, 0f, 1f));
                    Vector3 edgeTwo = Vector3.Scale(td.mapEdgesInWorldSpace[2], new Vector3(1f, 0f, 1f)) - Vector3.Scale(td.mapEdgesInWorldSpace[0], new Vector3(1f, 0f, 1f));

                    float xSize = edgeOne.magnitude;
                    float zSize = edgeTwo.magnitude;

                    Quaternion rotation = Quaternion.FromToRotation(edgeOne, transform.forward);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

                    Vector3 pos = matrix * mapObject.transform.position;
                    Vector3 startPos = matrix * td.mapEdgesInWorldSpace[0];

                    Vector2 localPos = new Vector2((startPos.x - pos.x) * (mapSize / xSize), (pos.z - startPos.z) * (mapSize / zSize));

                    float halfIconSize = iconSize / 2f;
                    Rect iconRect = new Rect(drawRect.x + localPos.x - halfIconSize, drawRect.y + localPos.y - halfIconSize, iconSize, iconSize);

                    GUI.depth = mapObject.depth;
                    GUI.DrawTexture(iconRect, mapObject.icon);
                }

                toDraw.RemoveAt(highestDepth);
            }
        }
    }
}

[System.Serializable]
public class MapObject
{
    public Transform transform;
    public Texture2D icon;
    public int depth = 0;

    public MapObject()
    {

    }

    public MapObject(Transform _transform, Texture2D _icon, int _depth)
    {
        transform = _transform;
        icon = _icon;
        depth = _depth;
    }
}
