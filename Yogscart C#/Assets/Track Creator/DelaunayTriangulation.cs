using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Based upon PseudoCode of the Bowyer–Watson algorithm
public class DelaunayTriangulation
{
    public static Mesh GenerateMesh(List<Vertex> points)
    {
        List<Triangle> triangles = BowyerWatson(points);
        List<Vertex> vertices = new List<Vertex>();

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        foreach (Triangle tri in triangles)
        {
            foreach(Vertex vertex in tri.refPoints)
            {
                int vertexIndex = vertices.IndexOf(vertex);

                if (vertexIndex == -1)
                {
                    vertexIndex = vertices.Count;
                    vertices.Add(vertex);
                }

                tris.Add(vertexIndex);
            }
        }

        foreach(Vertex vertex in vertices)
        {
            verts.Add(vertex.position);
            norms.Add(vertex.normal);
            uvs.Add(vertex.uv);
        }

        Mesh returnValue = new Mesh();

        returnValue.vertices = verts.ToArray();
        returnValue.normals = norms.ToArray();
        returnValue.uv = uvs.ToArray();
        returnValue.triangles = tris.ToArray();

        return returnValue;
    }


    public static List<Triangle> BowyerWatson(List<Vertex> points)
    {
        //Create list holding final triangle design
        List<Triangle> triangulation = new List<Triangle>();

        if (points != null && points.Count >= 3)
        {
            //Calculate min and max from the points
            Vector3 min = points[0].position, max = points[0].position;

            foreach (Vertex point in points)
            {
                Vector3 pointPos = point.position;

                if (pointPos.x < min.x)
                    min.x = pointPos.x;

                if (pointPos.y < min.y)
                    min.y = pointPos.y;

                if (pointPos.z < min.z)
                    min.z = pointPos.z;

                if (pointPos.x > max.x)
                    max.x = pointPos.x;

                if (pointPos.y > max.y)
                    max.y = pointPos.y;

                if (pointPos.z > max.z)
                    max.z = pointPos.z;
            }

            float xSize = (max.x - min.x) * 2f, zSize = (max.z - min.z) * 2f;

            //Get Bounds for Super Triangle
            Vertex pointA = new Vertex(new Vector3(min.x - xSize    , 0f, min.z - zSize), Vector3.up, Vector2.zero),
                pointB = new Vertex(new Vector3(min.x + (xSize / 2f), 0f, max.z + zSize), Vector3.up, Vector2.zero),
                pointC = new Vertex(new Vector3(max.x + xSize       , 0f, min.z - zSize), Vector3.up, Vector2.zero);

            Edge[] superTriangleEdges = new Edge[] { new Edge(pointA, pointB), new Edge(pointB, pointC), new Edge(pointC, pointA) };

            //Add to Triangulation
            triangulation.Add(new Triangle(superTriangleEdges));

            foreach (Vertex point in points)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                //Find all Triangles that are no longer valid
                foreach (Triangle triangle in triangulation)
                {
                    //If Point is inside circmumcircle of triangle
                    if (triangle.PointInside(point))
                        badTriangles.Add(triangle);
                }

                //Create a Polygon
                Polygon polygon = new Polygon();

                foreach (Triangle badTriangle in badTriangles)
                {
                    foreach (Edge edge in badTriangle.edges)
                    {
                        //If Edge is not shared by any other triangle in bad triangles
                        bool edgeShared = false;

                        foreach (Triangle otherBadTriangle in badTriangles)
                        {
                            if (otherBadTriangle != badTriangle)
                            {
                                foreach (Edge otherEdge in otherBadTriangle.edges)
                                {
                                    if ((otherEdge.one == edge.one && otherEdge.two == edge.two) || (otherEdge.one == edge.two && otherEdge.two == edge.one))
                                    {
                                        edgeShared = true;
                                        break;
                                    }
                                }
                            }

                            if (edgeShared)
                                break;
                        }

                        //Add edge to polygon
                        if (!edgeShared)
                            polygon.edges.Add(edge);
                    }
                }

                //Remove each bad triangle from triangulation
                foreach (Triangle badTriangle in badTriangles)
                    triangulation.Remove(badTriangle);

                //For each Edge in Polygon
                foreach (Edge edge in polygon.edges)
                {
                    //Form a triangle from edge to point
                    Edge[] nEdgeArray = new Edge[] { edge, new Edge(edge.two, point), new Edge(point, edge.one) };
                    Triangle nTriangle = new Triangle(nEdgeArray);

                    //Add newTriangle to triangulation
                    triangulation.Add(nTriangle);

                }
            }

            //foreach triangle in triangulation
            foreach (Triangle triangle in triangulation.ToArray())
            {
                //If triangle contains a vertex from original super triangle
                foreach (Edge edge in triangle.edges)
                {
                    if (edge.one == pointA || edge.one == pointB || edge.one == pointC ||
                        edge.two == pointA || edge.two == pointB || edge.two == pointC)
                    {
                        //Remove triangle from triangulation
                        triangulation.Remove(triangle);
                        break;
                    }
                }
            }
        }

        return triangulation;
    }

    public class Triangle
    {
        public List<Edge> edges;
        private List<Vector3> points;
        public List<Vertex> refPoints;

        Vector3 circleCentre;
        float circleRadius = 0;

        public Triangle(Edge[] _edges)
        {
            edges = _edges.ToList();
            points = new List<Vector3>();

            refPoints = new List<Vertex>();

            //Get Points
            foreach (Edge edge in _edges)
            {
                if (!refPoints.Contains(edge.one))
                    refPoints.Add(edge.one);

                if (!refPoints.Contains(edge.two))
                    refPoints.Add(edge.two);
            }

            foreach (Vertex refPoint in refPoints)
                points.Add(Vector3.Scale(refPoint.position, new Vector3(1f, 0f, 1f)));

            CalculateCircle();
        }

        public void CalculateCircle()
        {
            Vector3 a = points[0] - points[2];
            Vector3 b = points[1] - points[2];

            Vector3 z = Vector3.Cross(a, b);
            circleCentre = Vector3.Cross(Vector3.Dot(a, a) * b - Vector3.Dot(b, b) * a, z) *
                (0.5f / Vector3.Dot(z, z)) + points[2];

            circleRadius = (points[2] - circleCentre).magnitude;
        }

        public bool PointInside(Vertex point)
        {
            float length = (Vector3.Scale(point.position, new Vector3(1f, 0f, 1f)) - circleCentre).magnitude;
            return length <= circleRadius;
        }
    }

    public class Polygon
    {
        public List<Edge> edges;

        public Polygon()
        {
            edges = new List<Edge>();
        }

    }

    public class Edge
    {
        public Vertex one, two;

        public Edge(Vertex _one, Vertex _two)
        {
            one = _one;
            two = _two;
        }

    }

    /*public class Vertex
    {
        public Vector3 value;

        public Vertex(Vector3 _vector)
        {
            value = _vector;
        }
    }*/


}