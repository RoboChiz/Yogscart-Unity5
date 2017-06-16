using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class OBJExporter
{
    public static string ToOBJ(Mesh mesh)
    {
        string toReturn ="#Robo_Chiz OBJ Exporter V0.1 (c)2017\n\n";
       
        foreach (Vector3 vertex in mesh.vertices)
            toReturn += "v  " + vertex.x.ToString("0.0000") + " " + vertex.y.ToString("0.0000") + " " + vertex.z.ToString("0.0000") + "\n";
        toReturn += "#Vertices: " + mesh.vertices.Length + "\n\n";
    
        foreach (Vector3 normal in mesh.normals)
            toReturn += "vn " + normal.x.ToString("0.0000") + " " + normal.y.ToString("0.0000") + " " + normal.z.ToString("0.0000") + "\n";
        toReturn += "#Normals: " + mesh.normals.Length + "\n\n";
     
        foreach (Vector2 uv in mesh.uv)
            toReturn += "vt " + uv.x.ToString("0.0000") + " " + uv.y.ToString("0.0000") + " 0.0000\n";
        toReturn += "#Texture Co-ordinates: " + mesh.uv.Length + "\n\n";


        toReturn += "g Track\n";

        for(int i = 0; i < mesh.triangles.Length; i+=3)
        {
            int triOne = mesh.triangles[i] + 1;
            int triTwo = mesh.triangles[i + 1] + 1;
            int triThree = mesh.triangles[i + 2] + 1;

            toReturn += "f " + triOne + "/" + triOne + "/" + triOne;
            toReturn += " " + triTwo + "/" + triTwo + "/" + triTwo;
            toReturn += " " + triThree + "/" + triThree + "/" + triThree + " \n";
        }
        toReturn += "#Tris: " + mesh.triangles.Length + "\n";

        return toReturn;
    }

    public static void SaveOBJ(string fileName, string data)
    {
        File.WriteAllText(Application.dataPath + "/" + fileName + ".obj", data);
    }

    public static void SaveMeshAsOBJ(string fileName, Mesh mesh)
    {
        SaveOBJ(fileName,ToOBJ(mesh));
    }
}
