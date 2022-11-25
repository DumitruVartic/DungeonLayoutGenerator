using Graph;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Map
{
    public class Generator : MonoBehaviour
    {
        private enum CellType
        {
            None,
            Room,
            Hallway,
            Stairs
        }

        [SerializeField] private Vector3Int size;
        [SerializeField] private Vector3Int roomMaxSize;
        [SerializeField] private int roomCount;
        [SerializeField] private int duration;
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private Material roomMaterial;
        [SerializeField] private bool toggleCubeRoom;

        private void Reset()
        {
            size = new Vector3Int(20, 3, 20);
            roomMaxSize = new Vector3Int(6, 1, 6);
            roomCount = 20;
            duration = 10;
            toggleCubeRoom = true;
        }

        private Random random;
        private Grid<CellType> grid;
        private Graph.Graph graph;
        private List<Room> rooms;
        private List<GameObject> roomObjects;
        private List<GameObject> cubeRoomObjects;

        private void Start()
        {
            random = new Random(0);
            grid = new Grid<CellType>(size);
            graph = new Graph.Graph();
            rooms = new List<Room>();
            roomObjects = new List<GameObject>();
            cubeRoomObjects = new List<GameObject>();

            Generate();
        }

        private void Update() // To do : Step by Step Creation
        {
            if (Input.GetKeyDown(KeyCode.D)) { ViewData(); }
            if (Input.GetKeyDown(KeyCode.R)) { ResetData(); }
            if (Input.GetKeyDown(KeyCode.N)) { Generate(); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.A)) { grid.ViewArea(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.E)) { graph.ViewGraph(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.M)) { graph.ViewMST(duration); }
        }

        private void Generate()
        {
            PlaceRooms();
            CreateGraph();
        }

        private void CreateGraph()
        {
            List<Vertex> vertices = new List<Vertex>();

            foreach (Room room in rooms)
            {
                vertices.Add(new Vertex(room.bounds.position + room.bounds.size / 2));
            }

            graph.CreateGraph(vertices);
        }

        private void PlaceRooms()
        {
            for (int i = 0; i < roomCount; i++)
            {
                bool add;
                double infinityLoopPrevention = 100;
                Vector3Int location;
                Vector3Int roomSize;
                Room newRoom;

                do
                {
                    infinityLoopPrevention--;
                    if (infinityLoopPrevention == 0)
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
                if (!toggleCubeRoom)
                {
                    Vector3Int position = newRoom.bounds.position;
                    position.x += (newRoom.bounds.size.x / 2);
                    position.y += (newRoom.bounds.size.y / 2);
                    position.z += (newRoom.bounds.size.z / 2);
                    PlaceRoom(position, newRoom.bounds.size);
                }
                foreach (Vector3Int pos in newRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                    if (toggleCubeRoom)
                    {
                        PlaceCubeByGrid(pos);
                    }
                }
            }
            roomCount = rooms.Count;
        }

        private void PlaceCubeByGrid(Vector3Int pos)
        {
            GameObject go = Instantiate(cubePrefab, pos, Quaternion.identity);
            go.GetComponent<Transform>().localScale = new Vector3Int(1, 1, 1);
            go.GetComponent<MeshRenderer>().material = roomMaterial;
            go.name = "CubeRoom " + (cubeRoomObjects.Count + 1);
            cubeRoomObjects.Add(go);
        }

        private void PlaceCube(Vector3Int location, Vector3Int size, Material material)
        {
            GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            go.GetComponent<MeshRenderer>().material = material;
            go.name = "Room " + (roomObjects.Count + 1);
            roomObjects.Add(go);
        }

        private void PlaceRoom(Vector3Int location, Vector3Int size)
        {
            PlaceCube(location, size, roomMaterial);
        }

        // Methods for Debuging
        private void ViewData()
        {
            Debug.Log("Rooms - " + rooms.Count);
            Debug.Log("RoomsObjects - " + roomObjects.Count);
            Debug.Log("CubeRoomObjects - " + cubeRoomObjects.Count);
            graph.ViewData();
        }

        private void ResetData()
        {
            rooms.Clear();
            rooms = new List<Room>();

            foreach (GameObject item in roomObjects) { Destroy(item); }
            roomObjects.Clear();
            roomObjects = new List<GameObject>();

            foreach (GameObject item in cubeRoomObjects) { Destroy(item); }
            cubeRoomObjects.Clear();
            cubeRoomObjects = new List<GameObject>();

            foreach (Room room in rooms)
                foreach (Vector3Int pos in room.bounds.allPositionsWithin)
                    grid[pos] = CellType.None;
            grid = new Grid<CellType>(size);

            graph.Clear();
            graph = new Graph.Graph();
            roomCount = 1;
        }
    }
}