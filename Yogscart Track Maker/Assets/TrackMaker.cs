using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class TrackMaker : MonoBehaviour {

    public RoboTerrain terrain;

    // Use this for initialization
    void Start () {

        terrain = new RoboTerrain(10, 10, gameObject);
        terrain.GenerateMesh();
    }
	
	// Update is called once per frame
	void Update () {
	    if(terrain == null)
        {
            terrain = new RoboTerrain(10, 10, gameObject);
        }
	}

    [Serializable]
    public class RoboTerrain
    {
        //Width - The width of the map
        //Height - The length of the map
        public int width, height;

        public Material ground, wall;

        //List containing all of the layers in the Terrain
        public List<Layer> layers;

        //Heightmap that is applied across all layers (As no two layers can be on the same grid space)
        public Texture2D heightMap;

        private GameObject terrainObj;

        public RoboTerrain(int _width, int _height, GameObject _terrainObj)
        {
            width = _width;
            height = _height;

            Layer.wall = Resources.Load<Material>("TrackMaker/Wall");
            Layer.ground = Resources.Load<Material>("TrackMaker/Ground");

            terrainObj = _terrainObj;
            Clear();
        }

        public void Clear()
        {       
            while(terrainObj.transform.childCount > 0)
            {
                DestroyImmediate(terrainObj.transform.GetChild(0).gameObject);
            }

            layers = new List<Layer>();

            Layer baseLayer = new Layer(width, height, 0, terrainObj.transform);
            baseLayer.FillLayer();

            layers.Add(baseLayer);

            GenerateMesh();
        }

        //Accessor to make geting layers easier
        public Layer this[int i] {
            get {
                while(i >= layers.Count)
                {
                    layers.Add(new Layer(width, height, layers.Count, terrainObj.transform));
                }

                return layers[i];
            }           
        }

        //Generates the Entire Terrain
        public void GenerateMesh()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                List<Layer> aboveLayers = new List<Layer>();
                for (int j = i + 1; j < layers.Count; j++)
                    aboveLayers.Add(layers[j]);

                if (aboveLayers.Count > 0)
                    layers[i].UpdateActiveMap(aboveLayers.ToArray());

                layers[i].GenerateMesh();
            }
        }

        //Generates a specific Entire Terrain
        public void GenerateLayer(int i)
        {
            List<Layer> aboveLayers = new List<Layer>();

            for(int j = i + 1; j < layers.Count; j++)
            {
                aboveLayers.Add(layers[j]);
            }

            layers[i].CloneRequestedMap();

            if (aboveLayers.Count > 0)
                layers[i].UpdateActiveMap(aboveLayers.ToArray());

            layers[i].GenerateMesh();
        }

       
    }

    [SerializeField]
    public class Layer
    {
        //The map that is drawn onto
        [SerializeField]
        public byte[,] requestedMap;
        //The map that is used to generate the mesh (Takes into account other layers above it)
        [SerializeField]
        private byte[,] activeMap;
        //Holds the height of the layer
        public float layerHeight;

        [SerializeField]
        private int width, height;
        [SerializeField]
        public static Material ground, wall;

        //Converts and holds the provided map in a way that allows us to generate a mesh quickly
        private SquareGrid squareGrid;

        //The Gameobjects which are being used to render to
        private GameObject layerObj, topMeshObj, wallsMeshObj;
        private Transform terrainObj;

        //Holds the data we need for the Mesh
        private List<Vector3> vertices;
        private List<int> triangles;
        Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
        List<List<int>> outlines = new List<List<int>>();
        HashSet<int> checkedVertices = new HashSet<int>();

        public Layer(int _width, int _height, int _layerHeight, Transform _terrain)
        {
            width = _width;
            height = _height;

            requestedMap = new byte[width, height];
            activeMap = new byte[width, height];

            layerHeight = _layerHeight;

            squareGrid = new SquareGrid(activeMap, layerHeight);

            terrainObj = _terrain;
        }

        //Fills the Layer with the value 1
        public void FillLayer()
        {
            for (int x = 0; x < requestedMap.GetLength(0); x++)
            {
                for (int y = 0; y < requestedMap.GetLength(1); y++)
                {
                    requestedMap[x, y] = 1;
                    activeMap[x, y] = 1;
                }
            }
        }

        //Paint a circle of Brushsize to requested Map
        public void BrushStroke(Vector2 position, int brushSize)
        {
            int xPos = (int)Math.Round(position.x + 0.5f);
            int yPos = (int)Math.Round(position.y + 0.5f);

            //sqrt( a^2 + b^2 )

            float radiusSquared = brushSize * brushSize;

            for (int x = xPos - brushSize; x <= xPos + brushSize; x++)
            {
                for (int y = yPos - brushSize; y <= yPos + brushSize; y++)
                {
                    if(x >= 0 && x < width && y >= 0 && y < height)
                    {
                        if (Math.Abs((new Vector2(x, y) - position).sqrMagnitude) <= radiusSquared)
                        {
                            requestedMap[x, y] = 1;
                        }                            
                    }
                      
                }
            }

        }

        public void CloneRequestedMap()
        {
            activeMap = (byte[,])requestedMap.Clone();
        }

        //Updates the active Map using data from the layers above it
        public void UpdateActiveMap(Layer[] layers)
        {            
            for(int i = 0; i < layers.Length; i++)
            {
                for(int x = 0; x < activeMap.GetLength(0); x++)
                {
                    for (int y = 0; y < activeMap.GetLength(1); y++)
                    {
                        if (layers[i].activeMap[x, y] == 1)
                            activeMap[x, y] = 0;
                    }
                }
            }
        }

        void SmoothMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);

                    if (neighbourWallTiles > 4)
                        activeMap[x, y] = 1;
                    else if (neighbourWallTiles < 4)
                        activeMap[x, y] = 0;

                }
            }
        }

        int GetSurroundingWallCount(int gridX, int gridY)
        {
            int wallCount = 0;
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                    {
                        if (neighbourX != gridX || neighbourY != gridY)
                        {
                            wallCount += activeMap[neighbourX, neighbourY];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
            return wallCount;
        }

        //Generates the Entire Mesh for this Layer
        public void GenerateMesh()
        {
            //Create new Objects for the top and walls if they don't exist
            if (layerObj == null)
            {
                layerObj = new GameObject();
                layerObj.name = "Layer " + layerHeight;
                layerObj.transform.parent = terrainObj;
            }

            if (topMeshObj == null)
            {
                topMeshObj = CreateNewHolder();
                topMeshObj.name = "Top Mesh";
                topMeshObj.transform.parent = layerObj.transform;
                topMeshObj.GetComponent<MeshRenderer>().material = ground;
            }

            if (wallsMeshObj == null)
            {
                wallsMeshObj = CreateNewHolder();
                wallsMeshObj.name = "Wall Mesh";
                wallsMeshObj.transform.parent = layerObj.transform;
                wallsMeshObj.GetComponent<MeshRenderer>().material = wall;
            }             

            //Smooth the edges of the Map
            for(int i = 0; i < 2; i++)
            {
                SmoothMap();
            }

            //Update the squaregrid with the new map
            squareGrid.UpdateMap(activeMap, layerHeight);

            //Clear away the old Data
            vertices = new List<Vector3>();
            triangles = new List<int>();
            outlines.Clear();
            checkedVertices.Clear();
            triangleDictionary.Clear();

            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {
                    TriangulateSquare(squareGrid.squares[x, y]);
                }
            }

            Mesh mesh = new Mesh();
            topMeshObj.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            //Only generate walls if the layer is above zero
            if (layerHeight > 0)
                GenerateWallMesh();

        }

        //Creates a new empty Game Object with a Mesh Filter and Renderer
        private GameObject CreateNewHolder()
        {
            GameObject holder = new GameObject();
            holder.AddComponent<MeshFilter>();
            holder.AddComponent<MeshRenderer>();

            return holder;
        }

        private void GenerateWallMesh()
        {
            CalculateMeshOutlines();

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> walltriangles = new List<int>();

            Mesh wallMesh = new Mesh();

            foreach (List<int> outline in outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(vertices[outline[i]]); //Left Vertex
                    wallVertices.Add(vertices[outline[i + 1]]); //Right Vertex
                    wallVertices.Add(vertices[outline[i]] - (Vector3.up * layerHeight)); //Bottom Left Vertex
                    wallVertices.Add(vertices[outline[i + 1]] - (Vector3.up * layerHeight)); //Bottom Right Vertex

                    walltriangles.Add(startIndex + 0);
                    walltriangles.Add(startIndex + 2);
                    walltriangles.Add(startIndex + 3);

                    walltriangles.Add(startIndex + 3);
                    walltriangles.Add(startIndex + 1);
                    walltriangles.Add(startIndex + 0);
                }
            }

            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = walltriangles.ToArray();
            wallMesh.RecalculateNormals();

            wallsMeshObj.GetComponent<MeshFilter>().mesh = wallMesh;
        }

        void TriangulateSquare(Square square)
        {
            switch (square.configuration)
            {
                case 0:
                    break;

                // 1 points:
                case 1:
                    MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                    break;
                case 2:
                    MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                    break;
                case 4:
                    MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                    break;
                case 8:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                    break;

                // 2 points:
                case 3:
                    MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                case 6:
                    MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                    break;
                case 9:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                    break;
                case 12:
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                    break;
                case 5:
                    MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                    break;
                case 10:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;

                // 3 point:
                case 7:
                    MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                case 11:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                    break;
                case 13:
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                    break;
                case 14:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;

                // 4 point:
                case 15:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);

                    checkedVertices.Add(square.topLeft.vertexIndex);
                    checkedVertices.Add(square.topRight.vertexIndex);
                    checkedVertices.Add(square.bottomRight.vertexIndex);
                    checkedVertices.Add(square.bottomLeft.vertexIndex);
                    break;
            }
        }

        void MeshFromPoints(params Node[] points)
        {

            AssignVertices(points);

            if (points.Length >= 3)
                CreateTriangle(points[0], points[1], points[2]);
            if (points.Length >= 4)
                CreateTriangle(points[0], points[2], points[3]);
            if (points.Length >= 5)
                CreateTriangle(points[0], points[3], points[4]);
            if (points.Length >= 6)
                CreateTriangle(points[0], points[4], points[5]);

        }

        void AssignVertices(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].vertexIndex == -1)
                {
                    points[i].vertexIndex = vertices.Count;
                    vertices.Add(points[i].position);
                }
            }
        }

        void CreateTriangle(Node a, Node b, Node c)
        {
            triangles.Add(a.vertexIndex);
            triangles.Add(b.vertexIndex);
            triangles.Add(c.vertexIndex);

            Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
            AddTriangleToDictionary(a.vertexIndex, triangle);
            AddTriangleToDictionary(b.vertexIndex, triangle);
            AddTriangleToDictionary(c.vertexIndex, triangle);
        }

        void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
        {
            if (triangleDictionary.ContainsKey(vertexIndexKey))
            {
                triangleDictionary[vertexIndexKey].Add(triangle);
            }
            else
            {
                List<Triangle> triangleList = new List<Triangle>();
                triangleList.Add(triangle);
                triangleDictionary.Add(vertexIndexKey, triangleList);
            }
        }

        void CalculateMeshOutlines()
        {
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                if (!checkedVertices.Contains(vertexIndex))
                {
                    int newOulineVertex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOulineVertex != -1)
                    {
                        checkedVertices.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        outlines.Add(newOutline);

                        FollowOutline(newOulineVertex, outlines.Count - 1);
                        outlines[outlines.Count - 1].Add(vertexIndex);
                    }
                }
            }
        }

        void FollowOutline(int vertexIndex, int outlineIndex)
        {
            outlines[outlineIndex].Add(vertexIndex);
            checkedVertices.Add(vertexIndex);
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

            if (nextVertexIndex != -1)
            {
                FollowOutline(nextVertexIndex, outlineIndex);
            }
        }

        int GetConnectedOutlineVertex(int vertexIndex)
        {
            List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

            for (int i = 0; i < trianglesContainingVertex.Count; i++)
            {
                Triangle triangle = trianglesContainingVertex[i];

                for (int j = 0; j < 3; j++)
                {
                    int vertexB = triangle[j];
                    if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                    {
                        if (IsOutlineEdge(vertexIndex, vertexB))
                        {
                            return vertexB;
                        }
                    }
                }
            }

            return -1;
        }

        bool IsOutlineEdge(int vertexA, int vertexB)
        {
            List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
            int sharedTriangleCount = 0;

            for (int i = 0; i < trianglesContainingVertexA.Count; i++)
            {
                if (trianglesContainingVertexA[i].Contains(vertexB))
                {
                    sharedTriangleCount++;
                    if (sharedTriangleCount > 1)
                        break;
                }
            }

            return sharedTriangleCount == 1;
        }
    }



    ///////////////////////////////////////////////////////////////////////// MARCHING CUBE STUFF /////////////////////////////////////////////////////////

    //Stores information about all the Triangles in the Top Mesh. Used for generating walls
    struct Triangle
    {
        public int vertexIndexA, vertexIndexB, vertexIndexC;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return vertexIndexA;
                    case 1:
                        return vertexIndexB;
                    case 2:
                        return vertexIndexC;
                    default:
                        throw new Exception("Length ERROR");
                }
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    //Used to hold information about all of the squares in the Layer
    public class SquareGrid
    {
        public Square[,] squares;
        public SquareGrid(byte[,] map, float layerHeight)
        {
            UpdateMap(map, layerHeight);
        }

        //Used to create all the New Nodes and configutions we need for the new map
        public void UpdateMap(byte[,] map, float layerHeight)
        {
            int nodeCountX = map.GetLength(0), nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX, mapHeight = nodeCountY;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3((-mapWidth / 2) + (x) + .5f, layerHeight, (-mapHeight / 2) + (y) + .5f);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x, y], controlNodes[x + 1, y]);
                }
            }
        }
    }

    //Holds the specific configuration and position needed for the Marching Cubes
    public class Square
    {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomLeft, ControlNode _bottomRight)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active) : base(_pos)
        {
            active = _active;
            above = new Node(position + (Vector3.forward));
            right = new Node(position + (Vector3.right));
        }
    }
}
