using Graph;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Generator : MonoBehaviour
{
    enum CellType
    {
        None,
        Room,
        Hallway//, Stairs
    }

    class Room
    {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size)
        {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    [SerializeField] Vector3Int size = new Vector3Int(20, 3, 20);
    [SerializeField] int roomCount = 20;
    [SerializeField] Vector3Int roomMaxSize = new Vector3Int(6, 1, 6);
    [SerializeField] GameObject cubePrefab;
    [SerializeField] Material roomMaterial;

    Random random;
    Grid<CellType> grid;
    Graph.Graph graph;
    List<Room> rooms;
    List<GameObject> objects;
    List<GameObject> objectsCubes;

    void Start()
    {
        random = new Random(0);
        grid = new Grid<CellType>(size);
        graph = new Graph.Graph();
        rooms = new List<Room>();
        objects = new List<GameObject>();
        objectsCubes = new List<GameObject>();

        PlaceRooms();
        roomCount = rooms.Count;
        CreateGraph();
    }

    void GridAreaView()
    {
        int time = 100;
        Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(size.x, 0, 0), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, size.z), new Color(50, 50, 50), time);

        Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(size.x, size.y, 0), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(0, size.y, size.z), new Color(50, 50, 50), time);

        Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(0, 0, size.z), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, 0, 0), new Color(50, 50, 50), time);

        Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(0, size.y, size.z), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(size.x, size.y, 0), new Color(50, 50, 50), time);

        Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, size.y, 0), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(size.x, 0, 0), new Vector3Int(size.x, size.y, 0), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(0, 0, size.z), new Vector3Int(0, size.y, size.z), new Color(50, 50, 50), time);
        Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, size.y, size.z), new Color(50, 50, 50), time);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Introduse" + objects.Count);
            Debug.Log("Introduse" + rooms.Count);
            Debug.Log("Introduse" + objectsCubes.Count);
        }
        if (Input.GetKeyDown(KeyCode.G)) { GridAreaView(); }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaceRooms();
            Debug.Log("Introduse" + objects.Count);
            Debug.Log("Introduse" + rooms.Count);
            Debug.Log("Introduse" + objectsCubes.Count);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            rooms.Clear();
            rooms = new List<Room>();
            foreach (GameObject item in objects)
            {
                Destroy(item);
            }
            objects.Clear();
            objects = new List<GameObject>();
            foreach (GameObject item in objectsCubes)
            {
                Destroy(item);
            }
            objectsCubes.Clear();
            objectsCubes = new List<GameObject>();
            if (Input.GetKeyDown(KeyCode.R))
            {
                rooms.Clear();
                rooms = new List<Room>();
                foreach (GameObject item in objects)
                {
                    Destroy(item);
                }
                objects.Clear();
                objects = new List<GameObject>();
                foreach (GameObject item in objectsCubes)
                {
                    Destroy(item);
                }
                objectsCubes.Clear();
                objectsCubes = new List<GameObject>();
            }
        }
        if (Input.GetKeyDown(KeyCode.S)) { graph.ViewGraph(); } // de realizat posibilitatea stergere a lor pe c de exemplu
        if (Input.GetKeyDown(KeyCode.A)) { graph.ViewMST(); }
    }

    void CreateGraph()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (Room room in rooms)
        {
            vertices.Add(new Vertex(room.bounds.position + room.bounds.size / 2));
        }

        graph.CreateGraph(vertices);
    }

    void PlaceRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            bool add;
            double currentTime = 1000;
            Vector3Int location;
            Vector3Int roomSize;
            Room newRoom;

            do
            {
                currentTime--;
                if (currentTime == 0)
                {
                    newRoom = null;
                    break;
                }

                add = false;
                location = new Vector3Int(
                    random.Next(0, size.x),
                    random.Next(0, size.y),
                    random.Next(0, size.z)
                );

                roomSize = new Vector3Int(
                    random.Next(1, roomMaxSize.x + 1),
                    random.Next(1, roomMaxSize.y + 1),
                    random.Next(1, roomMaxSize.z + 1)
                );

                newRoom = new Room(location, roomSize);
                Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

                if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                    || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                    || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z)
                {
                    add = true;
                    continue;
                }

                foreach (Room room in rooms)
                {
                    if (Room.Intersect(room, buffer))
                    {
                        add = true;
                        break;
                    }
                }

            } while (add);
            if (newRoom == null)
                continue;

            rooms.Add(newRoom);
            Vector3Int position = newRoom.bounds.position;
            position.x += (newRoom.bounds.size.x / 2);
            position.y += (newRoom.bounds.size.y / 2);
            position.z += (newRoom.bounds.size.z / 2);
            //PlaceRoom(position, newRoom.bounds.size);

            foreach (Vector3Int pos in newRoom.bounds.allPositionsWithin)
            {
                grid[pos] = CellType.Room;
                PlaceCubeByGrid(pos);
            }
        }
    }

    void PlaceCubeByGrid(Vector3Int pos)
    {
        GameObject go = Instantiate(cubePrefab, pos, Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3Int(1, 1, 1);
        go.GetComponent<MeshRenderer>().material = roomMaterial;
        go.name = "CubeRoom " + (objectsCubes.Count + 1);
        objectsCubes.Add(go);
    }

    void PlaceCube(Vector3Int location, Vector3Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
        go.name = "Room " + (objects.Count + 1);
        objects.Add(go);
    }

    void PlaceRoom(Vector3Int location, Vector3Int size)
    {
        PlaceCube(location, size, roomMaterial);
    }
}