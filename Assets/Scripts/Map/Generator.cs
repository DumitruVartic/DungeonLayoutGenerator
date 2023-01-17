using Graph;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Map
{
    public class Generator : MonoBehaviour
    {
        Generator() { }

        static Generator instance;

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

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        enum CellType
        {
            None,
            Room,
            Hallway,
            Stairs,
            StairsAir
        }

        public enum Alghoritm
        {
            Kruskal,
            Prim
        }

        [TextArea(1, 2)] public string seed;
        [SerializeField] Vector3Int size;
        [SerializeField] Vector3Int roomMaxSize;
        [SerializeField] int roomCount;
        [SerializeField] int randomSeed;
        [SerializeField] double probabilityToSelect;
        [SerializeField] int duration;
        [SerializeField] Alghoritm alghoritm;
        [SerializeField] bool toggleCubeRoom;
        [SerializeField] public bool addExtraEdges;
        [SerializeField] bool loadInteriorFirst;
        [SerializeField] bool defaultMaterial;
        [SerializeField] GameObject currentUsedPrefab;
        [SerializeField] GameObject interiorPrefab;
        [SerializeField] GameObject exteriorPrefab;
        [SerializeField] GameObject entrencePrefab;
        [SerializeField] GameObject staircasePrefab;
        [SerializeField] GameObject parapetPrefab;
        [SerializeField] Material roomMaterial;
        [SerializeField] Material hallwayMaterial;
        [SerializeField] Material stairsMaterial;
        [SerializeField] Material staircaseMaterial;

        void Reset()
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

        Random random;
        Grid<CellType> grid;
        Grid<CellType> additionalGrid;
        Graph.Graph graph;
        List<Room> rooms;
        List<GameObject> objects;
        List<GameObject> interiorObjects;
        HashSet<Edge> selectedEdges;
        List<List<Vector3Int>> paths;

        void Start()
        {
            random = new Random(randomSeed);
            grid = new Grid<CellType>(size, Vector3Int.zero);
            additionalGrid = new Grid<CellType>(size, Vector3Int.zero);
            graph = new Graph.Graph();
            rooms = new List<Room>();
            objects = new List<GameObject>();
            interiorObjects = new List<GameObject>();
            paths = new List<List<Vector3Int>>();

            if (seed != string.Empty)
                InputSeed();

            Generate();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.A)) { ViewGridArea(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.E)) { graph.ViewGraph(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.M)) { graph.ViewMST(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.S)) { graph.ViewSelectedEdges(duration); }
        }

        void Generate()
        {
            defaultMaterial = true;
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
            additionalGrid = new Grid<CellType>(size, Vector3Int.zero);
            Generate();
        }

        public void InteriorLoading()
        {
            defaultMaterial = false;
            currentUsedPrefab = interiorPrefab;
            foreach (GameObject item in objects) { Destroy(item); }
            objects.Clear();
            objects = new List<GameObject>();
            ReplacePrefabs();
            CreateStaircases();
            RemovePrefabFaces();
            CreateEntrances();
        }

        public void ExteriorLoading()
        {
            defaultMaterial = true;
            foreach (GameObject item in interiorObjects) { Destroy(item); }
            interiorObjects.Clear();
            interiorObjects = new List<GameObject>();

            foreach (GameObject item in objects) { Destroy(item); }
            objects.Clear();
            objects = new List<GameObject>();

            currentUsedPrefab = exteriorPrefab;
            ReplacePrefabs();
        }

        void ReplacePrefabs()
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        if (grid[x, y, z] == CellType.Room)
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

        void CreateGraph()
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

        void PlaceRooms()
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

        void PlaceCube(Vector3Int location, Vector3Int size, string tag, Material material)
        {
            GameObject go = Instantiate(currentUsedPrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            if (defaultMaterial)
                SetMaterial(go, material);
            go.tag = tag;
            go.name = tag + " " + GameObject.FindGameObjectsWithTag(tag).Length;
            go.transform.parent = transform;
            objects.Add(go);
        }

        void PlaceRoom(Vector3Int location, Vector3Int size)
        {
            PlaceCube(location, size, "Room", roomMaterial);
        }

        void PlaceHallway(Vector3Int location)
        {
            PlaceCube(location, new Vector3Int(1, 1, 1), "Hallway", hallwayMaterial);
        }

        void PlaceStairs(Vector3Int location)
        {
            PlaceCube(location, new Vector3Int(1, 1, 1), "Stairs", stairsMaterial);
        }

        void Pathfind()
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

                                List<Vector3Int> stairs = new List<Vector3Int> // stairs position
                                {
                                    prev + horizontalOffset,
                                    prev + horizontalOffset * 2,
                                    prev + verticalOffset + horizontalOffset,
                                    prev + verticalOffset + horizontalOffset * 2
                                };

                                Vector3Int max;
                                if (current.y > prev.y)
                                    max = current;
                                else
                                    max = prev;

                                additionalGrid[max] = CellType.StairsAir;
                                foreach (var item in stairs)
                                {
                                    if (item.y == max.y)
                                        additionalGrid[item] = CellType.StairsAir;
                                    PlaceStairs(item);
                                    grid[item] = CellType.Stairs;
                                }
                            }

                            Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, duration, false);
                        }
                    }

                    foreach (var pos in path)
                    {
                        if (grid[pos] == CellType.Hallway && IsEmpty(pos))
                        {
                            PlaceHallway(pos);
                        }
                    }
                    paths.Add(path);
                }
            }
        }

        bool IsEmpty(Vector3Int position)
        {
            foreach (var gameObject in objects)
                if (gameObject.transform.position == position)
                    return false;
            return true;
        }

        void ResetData()
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

        void InputSeed()
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

        void SetMaterial(GameObject gameObject, Material material)
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

        void CreateStaircases()
        {
            foreach (var path in paths)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    SetStaircaseByRotation(path[i], path[i - 1]);
                }
            }
        }

        void SetStaircaseByRotation(Vector3Int current, Vector3Int prev)
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

        void CreateEntrances()
        {
            foreach (var path in paths)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    ReplaceEntrence(path[i], path[i - 1]);
                }
            }
        }

        void ReplaceEntrence(Vector3Int current, Vector3Int prev)
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

            if (grid[current] == grid[prev])
                return;

            foreach (var item in interiorObjects)
                if (item.GetComponent<Transform>().position == prev + positionRotation[direction].Item1)
                    return;

            GameObject go = Instantiate(entrencePrefab, prev, Quaternion.identity);
            go.GetComponent<Transform>().localPosition = prev + positionRotation[direction].Item1;
            go.GetComponent<Transform>().localRotation = Quaternion.Euler(positionRotation[direction].Item2);
            go.transform.parent = transform;
            interiorObjects.Add(go);
        }

        void RemovePrefabFaces()
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

        void CheckNeighbours(Vector3Int position, string direction)
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
            else if (grid[position] == CellType.Stairs && grid[position + directionSpecifier[direction]] == CellType.Hallway && direction != "Up" && direction != "Down")
            {
                EliminateQuad(position, direction);
                EliminateQuad(position + directionSpecifier[direction], opositeDirection[direction]);
                CreateParapet(position, direction);
            }
        }

        void EliminateQuad(Vector3Int position, string tag)
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
                            foreach (var item in interiorObjects)
                            {
                                if (child.transform.position == item.transform.position)
                                    Destroy(item);
                            }
                            Destroy(child);
                            return;
                        }
                    }
                }
            }
        }

        void CreateParapet(Vector3Int position, string tag)
        {
            Dictionary<string, (Vector3, Vector3)> positionRotation = new Dictionary<string, (Vector3, Vector3)>()
            {
                {"Right", (new Vector3((float)0.5, 0, 0), new Vector3(0, -90, 0))},
                {"Left", (new Vector3((float)-0.5, 0, 0), new Vector3(0, 90, 0))},
                {"Forward", (new Vector3(0, 0, (float)0.5), new Vector3(0, -180, 0))},
                {"Back", (new Vector3(0, 0, (float)-0.5), new Vector3(0, 0, 0))},
            };

            foreach (GameObject gameObject in objects)
            {
                if (position == Vector3Int.FloorToInt(gameObject.transform.position))
                {
                    if (additionalGrid[position] != CellType.StairsAir)
                        return;

                    Transform transform = gameObject.transform;
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        if (child.CompareTag(tag))
                        {
                            GameObject go = Instantiate(parapetPrefab, child.transform.position, Quaternion.identity);
                            go.GetComponent<Transform>().localRotation = Quaternion.Euler(positionRotation[tag].Item2);
                            go.transform.parent = transform;
                            interiorObjects.Add(go);
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