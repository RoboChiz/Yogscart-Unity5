using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vertex
{
    public Vector3 position, normal;
    public Vector2 uv;

    public Vertex(Vector3 _position, Vector3 _normal, Vector2 _uv)
    {
        position = _position;
        normal = _normal;
        uv = _uv;
    }

    public Vertex(Vertex copy)
    {
        position = copy.position;
        normal = copy.normal;
        uv = copy.uv;
    }
}
