using Graph;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = System.Random;

namespace Map
{
    public class Generator : MonoBehaviour
    {
        private Generator()
        {
        }

        private static Generator instance;

        public static Generator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Generator();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private enum CellType
        {
            None,
            Room,
            Hallway,
            Stairs
        }

        public enum Alghoritm
        {
            Kruskal,
            Prim
        }

        [TextArea(1, 2)] public string seed;
        [SerializeField] private Vector3Int size;
        [SerializeField] private Vector3Int roomMaxSize;
        [SerializeField] private int roomCount;
        [SerializeField] private int randomSeed;
        [SerializeField] private double probabilityToSelect;
        [SerializeField] private int duration;
        [SerializeField] private Alghoritm alghoritm;
        [SerializeField] private bool toggleCubeRoom;
        [SerializeField] public bool addExtraEdges;
        [SerializeField] private bool loadInteriorFirst;
        [SerializeField] private GameObject currentUsedPrefab;
        [SerializeField] private GameObject interiorPrefab;
        [SerializeField] private GameObject exteriorPrefab;
        [SerializeField] private GameObject entrencePrefab;
        [SerializeField] private GameObject staircasePrefab;
        [SerializeField] private Material roomMaterial;
        [SerializeField] private Material hallwayMaterial;
        [SerializeField] private Material stairsMaterial;
        [SerializeField] private Material staircaseMaterial;

        private void Reset()
        {
            size = new Vector3Int(20, 3, 20);
            roomMaxSize = new Vector3Int(6, 1, 6);
            roomCount = 20;
            duration = 10;
            randomSeed = 0;
            alghoritm = Alghoritm.Kruskal;
            toggleCubeRoom = true;
            addExtraEdges = true;
            probabilityToSelect = 0.0125;
            seed = string.Empty;
            loadInteriorFirst = true;
        }

        private Random random;
        private Grid<CellType> grid;
        private Graph.Graph graph;
        private List<Room> rooms;
        private List<GameObject> objects;
        private List<GameObject> interiorObjects;
        private HashSet<Edge> selectedEdges;
        private List<List<Vector3Int>> paths;

        private void Start()
        {
            random = new Random(randomSeed);
            grid = new Grid<CellType>(size, Vector3Int.zero);
            graph = new Graph.Graph();
            rooms = new List<Room>();
            objects = new List<GameObject>();
            interiorObjects = new List<GameObject>();
            paths = new List<List<Vector3Int>>();

            if (seed != string.Empty)
                InputSeed();

            Generate();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.A)) { ViewGridArea(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.E)) { graph.ViewGraph(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.M)) { graph.ViewMST(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.S)) { graph.ViewSelectedEdges(duration); }
        }

        private void Generate()
        {
            PlaceRooms();
            CreateGraph();
            Pathfind();

            if (loadInteriorFirst)
                InteriorLoading();
        }

        public void Generate(string menuData)
        {
            ResetData();
            seed = menuData;
            InputSeed();
            grid = new Grid<CellType>(size, Vector3Int.zero);
            Generate();
        }

        public void InteriorLoading()
        {
            if (!loadInteriorFirst)
            {
                currentUsedPrefab = interiorPrefab;
                foreach (GameObject item in objects) { Destroy(item); }
                objects.Clear();
                objects = new List<GameObject>();
                ReplacePrefabs();
            }
            RemovePrefabFaces();
            CreateEntrances();
            CreateStaircases();
        }

        public void ExteriorLoading()
        {
            foreach (GameObject item in interiorObjects) { Destroy(item); }
            interiorObjects.Clear();
            interiorObjects = new List<GameObject>();

            foreach (GameObject item in objects) { Destroy(item); }
            objects.Clear();
            objects = new List<GameObject>();

            currentUsedPrefab = exteriorPrefab;
            ReplacePrefabs();
        }

        private void ReplacePrefabs()
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        if (grid[x, y , z] == CellType.Room)
                        {
                            PlaceRoom(new Vector3Int(x, y, z), Vector3Int.one);
                        }
                        if (grid[x, y, z] == CellType.Hallway)
                        {
                            PlaceHallway(new Vector3Int(x, y, z));
                        }
                        if (grid[x, y, z] == CellType.Stairs)
                        {
                            PlaceStairs(new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }

        public void TeleportPlayerInside()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = objects[0].transform.position;
        }

        private void CreateGraph()
        {
            List<Vertex> vertices = new List<Vertex>();

            foreach (Room room in rooms)
                vertices.Add(new Vertex(room.bounds.position + room.bounds.size / 2));

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].SetIndex(i);
                rooms[i].SetIndex(i);
            }

            graph.CreateGraph(vertices, probabilityToSelect, randomSeed, addExtraEdges, alghoritm);
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
                    position += (newRoom.bounds.size / 2);
                    PlaceRoom(position, newRoom.bounds.size);
                }

                foreach (Vector3Int pos in newRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                    if (toggleCubeRoom)
                    {
                        PlaceCube(pos, new Vector3Int(1, 1, 1), "CubeRoom", roomMaterial);
                    }
                }
            }
            roomCount = rooms.Count;
        }

        private void PlaceCube(Vector3Int location, Vector3Int size, string tag, Material material)
        {
            GameObject go = Instantiate(currentUsedPrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            SetMaterial(go, material);
            go.tag = tag;
            go.name = tag + " " + (GameObject.FindGameObjectsWithTag(tag).Length);
            go.transform.parent = transform;
            objects.Add(go);
        }

        private void PlaceRoom(Vector3Int location, Vector3Int size)
        {
            PlaceCube(location, size, "Room", roomMaterial);
        }

        private void PlaceHallway(Vector3Int location)
        {
            PlaceCube(location, new Vector3Int(1, 1, 1), "Hallway", hallwayMaterial);
        }

        private void PlaceStairs(Vector3Int location)
        {
            PlaceCube(location, new Vector3Int(1, 1, 1), "Stairs", stairsMaterial);
        }

        private void Pathfind()
        {
            DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);
            
            selectedEdges = graph.GetSelectedEdges(); // ?

            foreach (var edge in selectedEdges)
            {
                var startRoom = rooms.Find(item => item.Index == edge.Source.Index);
                var endRoom = rooms.Find(item => item.Index == edge.Destination.Index);

                var startPosf = startRoom.bounds.center;
                var endPosf = endRoom.bounds.center;
                var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
                var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

                var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) =>
                {
                    var pathCost = new DungeonPathfinder3D.PathCost();

                    var delta = b.Position - a.Position;

                    if (delta.y == 0)
                    {
                        //flat hallway
                        pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                        if (grid[b.Position] == CellType.Stairs)
                        {
                            return pathCost;
                        }
                        else if (grid[b.Position] == CellType.Room)
                        {
                            pathCost.cost += 5;
                        }
                        else if (grid[b.Position] == CellType.None)
                        {
                            pathCost.cost += 1;
                        }

                        pathCost.traversable = true;
                    }
                    else
                    {
                        //staircase
                        if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                            || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

                        pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                        int xDir = Mathf.Clamp(delta.x, -1, 1);
                        int zDir = Mathf.Clamp(delta.z, -1, 1);
                        Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                        Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        if (!grid.InBounds(a.Position + verticalOffset)
                            || !grid.InBounds(a.Position + horizontalOffset)
                            || !grid.InBounds(a.Position + verticalOffset + horizontalOffset))
                        {
                            return pathCost;
                        }

                        if (grid[a.Position + horizontalOffset] != CellType.None
                            || grid[a.Position + horizontalOffset * 2] != CellType.None
                            || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                            || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
                        {
                            return pathCost;
                        }

                        pathCost.traversable = true;
                        pathCost.isStairs = true;
                    }

                    return pathCost;
                });

                if (path != null)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        var current = path[i];

                        if (grid[current] == CellType.None)
                        {
                            grid[current] = CellType.Hallway;
                        }

                        if (i > 0)
                        {
                            var prev = path[i - 1];

                            var delta = current - prev;

                            if (delta.y != 0)
                            {
                                int xDir = Mathf.Clamp(delta.x, -1, 1);
                                int zDir = Mathf.Clamp(delta.z, -1, 1);
                                Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                                Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                                grid[prev + horizontalOffset] = CellType.Stairs;
                                grid[prev + horizontalOffset * 2] = CellType.Stairs;
                                grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                                grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                                PlaceStairs(prev + horizontalOffset);
                                PlaceStairs(prev + horizontalOffset * 2);
                                PlaceStairs(prev + verticalOffset + horizontalOffset);
                                PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                            }

                            Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, duration, false);
                        }
                    }

                    foreach (var pos in path)
                    {
                        if (grid[pos] == CellType.Hallway && isEmpty(pos))
                        {
                            PlaceHallway(pos);
                        }
                    }
                    paths.Add(path);
                }
            }
        }

        private bool isEmpty(Vector3Int position)
        {
            foreach (var gameObject in objects)
                if (gameObject.transform.position == position)
                    return false;
            return true;
        }

        private void ResetData()
        {
            rooms.Clear();
            rooms = new List<Room>();
            foreach (GameObject item in objects) { Destroy(item); }
            objects.Clear();
            objects = new List<GameObject>();

            foreach (Room room in rooms)
                foreach (Vector3Int pos in room.bounds.allPositionsWithin)
                    grid[pos] = CellType.None;
            grid = new Grid<CellType>(size, Vector3Int.zero);

            graph.Clear();
            graph = new Graph.Graph();
            roomCount = 20;

            random = new Random(randomSeed);
            randomSeed = random.Next();

            paths.Clear();
            paths = new List<List<Vector3Int>>();
        }

        private void InputSeed()
        {
            string valid = "SXSYSZRXRYRZRPARS";
            bool found = false;
            string last = string.Empty;
            foreach (string s in SplitAlpha(seed))
            {
                if (found)
                {
                    found = false;
                    switch (last)
                    {
                        case "SX": size.x = Convert.ToInt32(s); break;
                        case "SY": size.y = Convert.ToInt32(s); break;
                        case "SZ": size.z = Convert.ToInt32(s); break;
                        case "RX": roomMaxSize.x = Convert.ToInt32(s); break;
                        case "RY": roomMaxSize.y = Convert.ToInt32(s); break;
                        case "RZ": roomMaxSize.z = Convert.ToInt32(s); break;
                        case "R": roomCount = Convert.ToInt32(s); break;
                        case "A": alghoritm = (Alghoritm)Convert.ToInt32(s); break;
                        case "P": probabilityToSelect = Convert.ToDouble(s); break;
                        case "RS": randomSeed = Convert.ToInt32(s); break;
                        default: Debug.Log("Incorect seed"); break;
                    }
                }

                if (valid.Contains(s.ToUpper()))
                {
                    found = true;
                    last = s.ToUpper();
                }
            }
        }

        private void OutputSeed()
        {
            seed =
                "SX" + size.x + "SY" + size.y + "SZ" + size.z +
                "RX" + roomMaxSize.x + "RY" + roomMaxSize.y + "RZ" + roomMaxSize.z +
                "R" + roomCount + "A" + Convert.ToInt32(alghoritm) + "P" + probabilityToSelect + "RS" + randomSeed;
        }

        private static IEnumerable<string> SplitAlpha(string seed)
        {
            var words = new List<string> { string.Empty };
            for (var i = 0; i < seed.Length; i++)
            {
                words[words.Count - 1] += seed[i];
                if (i + 1 < seed.Length && char.IsLetter(seed[i]) != char.IsLetter(seed[i + 1]))
                {
                    words.Add(string.Empty);
                }
            }
            return words;
        }

        private void SetMaterial(GameObject gameObject, Material material)
        {
            if (!toggleCubeRoom)
            {
                gameObject.GetComponent<MeshRenderer>().material = material;
                return;
            }
            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                child.GetComponent<MeshRenderer>().material = material;
            }
        }

        private void CreateStaircases()
        {
            foreach (var path in paths)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    SetStaircaseByRotation(path[i], path[i - 1]);
                }
            }
        }

        private void SetStaircaseByRotation(Vector3Int current, Vector3Int prev)
        {
            Dictionary<Vector3Int, string> directions = new Dictionary<Vector3Int, string>()
            {
                {Vector3Int.right, "Right"},
                {Vector3Int.left, "Left"},
                {Vector3Int.forward, "Forward"},
                {Vector3Int.back, "Back"}
            };

            Dictionary<string, (Vector3, Vector3)> positionRotation = new Dictionary<string, (Vector3, Vector3)>()
            {
                {"Right", (new Vector3(1, 0, 0), new Vector3(0, 90, 0))},
                {"Left", (new Vector3(-1, 0, 0), new Vector3(0, -90, 0))},
                {"Forward", (new Vector3(0, 0, 1), new Vector3(0, 0, 0))},
                {"Back", (new Vector3(0, 0, -1), new Vector3(0, 180, 0))},
            };

            string direction;
            Vector3Int delta;
            if (current.y >= prev.y)
                delta = current - prev;
            else
                delta = prev - current;

            if (delta.y == 0)
                return;

            Vector3Int staircasePosition;
            if (current.y > prev.y)
                staircasePosition = prev;
            else
                staircasePosition = current;

            int xDir = Mathf.Clamp(delta.x, -1, 1);
            int zDir = Mathf.Clamp(delta.z, -1, 1);
            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

            delta = (prev + horizontalOffset) - prev;
            direction = directions[delta];

            GameObject go = Instantiate(staircasePrefab, staircasePosition + horizontalOffset, Quaternion.identity);
            go.GetComponent<Transform>().localPosition = staircasePosition + horizontalOffset + positionRotation[direction].Item1;
            go.GetComponent<Transform>().localRotation = Quaternion.Euler(positionRotation[direction].Item2);
            go.transform.parent = transform;
            interiorObjects.Add(go);
        }

        private void CreateEntrances()
        {
            foreach (var path in paths)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    ReplaceEntrence(path[i], path[i - 1]);
                }
            }
        }

        private void ReplaceEntrence(Vector3Int current, Vector3Int prev)
        {
            Dictionary<Vector3Int, string> directions = new Dictionary<Vector3Int, string>()
            {
                {Vector3Int.right, "Right"},
                {Vector3Int.left, "Left"},
                {Vector3Int.forward, "Forward"},
                {Vector3Int.back, "Back"}
            };

            Dictionary<string, string> opositeDirection = new Dictionary<string, string>()
            {
                {"Right", "Left"},
                {"Left", "Right"},
                {"Forward", "Back"},
                {"Back", "Forward"}
            };

            Dictionary<string, (Vector3, Vector3)> positionRotation = new Dictionary<string, (Vector3, Vector3)>()
            {
                {"Right", (new Vector3((float)0.5, 0, 0), new Vector3(0, -90, 0))},
                {"Left", (new Vector3((float)-0.5, 0, 0), new Vector3(0, 90, 0))},
                {"Forward", (new Vector3(0, 0, (float)0.5), new Vector3(0, -180, 0))},
                {"Back", (new Vector3(0, 0, (float)-0.5), new Vector3(0, 0, 0))},
            };

            string direction;
            Vector3Int delta = current - prev;
            if (delta.y != 0) // stairs
            {
                int xDir = Mathf.Clamp(delta.x, -1, 1);
                int zDir = Mathf.Clamp(delta.z, -1, 1);
                Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                delta = (prev + horizontalOffset) - prev;
                direction = directions[delta];
                EliminateQuad(prev, direction);
                EliminateQuad(prev + horizontalOffset, opositeDirection[direction]);

                delta = (prev + verticalOffset + horizontalOffset * 2) - current;
                direction = directions[delta];
                EliminateQuad(current, direction);
                EliminateQuad(prev + verticalOffset + horizontalOffset * 2, opositeDirection[direction]);

                return;
            }

            direction = directions[delta];
            EliminateQuad(prev, direction);
            EliminateQuad(current, opositeDirection[direction]);

            if (!loadInteriorFirst)
                return;

            if (grid[current] == grid[prev])
                return;

            GameObject go = Instantiate(entrencePrefab, prev, Quaternion.identity);
            go.GetComponent<Transform>().localPosition = prev + positionRotation[direction].Item1;
            go.GetComponent<Transform>().localRotation = Quaternion.Euler(positionRotation[direction].Item2);
            go.transform.parent = transform;
            interiorObjects.Add(go);
        }

        private void RemovePrefabFaces()
        {
            foreach (GameObject firstGameObject in objects)
            {
                Vector3Int position = Vector3Int.FloorToInt(firstGameObject.transform.position);
                CheckNeighbours(position, "Right");
                CheckNeighbours(position, "Left");
                CheckNeighbours(position, "Forward");
                CheckNeighbours(position, "Back");
                if (grid[position] == CellType.Stairs)
                {
                    CheckNeighbours(position, "Up");
                    CheckNeighbours(position, "Down");
                }
            }
        }

        private void CheckNeighbours(Vector3Int position, string direction)
        {
            Dictionary<string, Vector3Int> directionSpecifier = new Dictionary<string, Vector3Int>()
            {
                {"Up", Vector3Int.up},
                {"Down", Vector3Int.down},
                {"Right", Vector3Int.right},
                {"Left", Vector3Int.left},
                {"Forward", Vector3Int.forward},
                {"Back", Vector3Int.back}
            };

            Dictionary<string, string> opositeDirection = new Dictionary<string, string>()
            {
                {"Up", "Down"},
                {"Down", "Up"},
                {"Right", "Left"},
                {"Left", "Right"},
                {"Forward", "Back"},
                {"Back", "Forward"}
            };

            Vector3Int pos = position + directionSpecifier[direction];
            if (pos.x < 0 || pos.x >= size.x
            || pos.y < 0 || pos.y >= size.y
            || pos.z < 0 || pos.z >= size.z)
            {
                return;
            }

            if (grid[position] == grid[position + directionSpecifier[direction]])
            {
                EliminateQuad(position, direction);
                EliminateQuad(position + directionSpecifier[direction], opositeDirection[direction]);
            }
        }

        private void EliminateQuad(Vector3Int position, string tag)
        {
            foreach (GameObject gameObject in objects)
            {
                if (position == Vector3Int.FloorToInt(gameObject.transform.position))
                {
                    Transform transform = gameObject.transform;
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        if (child.CompareTag(tag))
                        {
                            Destroy(child);
                            return;
                        }
                    }
                }
            }
        }
        
        public void ViewGridArea(int duration)
        {
            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(size.x, 0, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(size.x, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(0, size.y, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(0, 0, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, 0, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(0, size.y, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(size.x, size.y, 0), Color.white, duration);

            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, 0), new Vector3Int(size.x, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, 0, size.z), new Vector3Int(0, size.y, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, size.y, size.z), Color.white, duration);
        }
    }
}