using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Node : MonoBehaviour
{
    public float roadWidth = 25f;
    //In Degrees
    public float rotateAmount = 0f;

    public bool flipUV;

    [HideInInspector]
    public List<NodeConnector> connections = new List<NodeConnector>();

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(5, 5, 5));
    }

    public void ResetMesh()
    {
        Mesh falseMesh = new Mesh();

        GetComponent<MeshFilter>().mesh = falseMesh;
        GetComponent<MeshCollider>().sharedMesh = falseMesh;
    }

    public void GenerateMesh()
    {
        List<Vertex> points = new List<Vertex>();

        foreach(NodeConnector nc in connections)
        {
            if(this == nc.a)
            {
                points.Add(nc.aEndVertices[0]);
                points.Add(nc.aEndVertices[1]);
            }
            else if (this == nc.b)
            {
                points.Add(nc.bEndVertices[0]);
                points.Add(nc.bEndVertices[1]);
            }           
        }

        if (points.Count >= 4f)
        {
            Mesh mesh = DelaunayTriangulation.GenerateMesh(points);

            //Remove Node position from vertices
            Vector3[] verticePositions = mesh.vertices;
            for (int i = 0; i < verticePositions.Length; i++)
                verticePositions[i] -= transform.position;

            mesh.vertices = verticePositions;

            if(flipUV && mesh.uv.Length >= 2)
            {
                Vector2[] uv = mesh.uv;

                Vector2 holder = uv[0];
                uv[0] = uv[1];
                uv[1] = holder;

                mesh.uv = uv;
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}