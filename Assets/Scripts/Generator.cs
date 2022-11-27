using Graph;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
/*
 * de realizat vizualizarea la G cost, H cost, F cost in debug, (daca asa ceva e posibil)
 * cred ca astfel de vizualizare este posibila doar in step by step sau 
 * daca este doar 2 camere
 * de terminat metoda neighbours ce returneaza o structura? sao lista (de verificat ce va fi mai convenabil) cu vecinii sai
 * Refactoring :
 * de scos functiile ce nu trebuie sa se afle aici(si variabile)
 * (gen functiile de debug sa se afle in clasa aparte, si de chemat aici), 
 * variabile ce tin de graf de scos de asemeni, gen (probability to select.alghoritm etc)
 * Este nevoie oare de pus enum'urile sa fie aparte
 * addExtraEdges - se va  folosi obligatoriui
 * instantPathfinding deasemeni
 * toggle cube room slabe sanse, duration e doar pentru debug
 * dar cel mai probabil ca astea 3 ar trebui de scos in clasa debug (impreuna cu metodele pentr debug)
 * de facto, in clasa generator e nevoie doar de 3 variabile, size la grid, maxsize la room, roomCount
 * celelalte ar putea fi scoase pentr debug
 * metodele pentru debug din update din mutat in clasa debug, 
 * practic aceasta oricum se va numi Debug si va fi monobehavior
 * clasa seed
 * seedul ar putea fi facut ca clasa aparte
 * singura problema e cum el va avea acces la variabile pentru a le seta ?
 * 
 */

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

        // https://stackoverflow.com/questions/70568157/cant-use-c-sharp-net-6-priorityqueue-in-unity?fbclid=IwAR3f7Fa2GfsJHlLH8AnWAm7rmSXeslAE8y2GnmK77zN_b47gxxW7DFAIPL8
        // motivul adaugarii PriorityQueue

        public enum Alghoritm
        {
            Kruskal,
            Prim
        }

        [TextArea(1, 2)]
        [SerializeField] private string seed;
        [SerializeField] private Vector3Int size;
        [SerializeField] private Vector3Int roomMaxSize;
        [SerializeField] private int roomCount;
        [SerializeField] private int duration;
        [SerializeField] private int randomSeed;
        [SerializeField] private double probabilityToSelect;
        [SerializeField] private Alghoritm alghoritm;
        [SerializeField] private bool toggleCubeRoom;
        [SerializeField] private bool addExtraEdges;
        [SerializeField] private bool instantPathfindig;
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private Material roomMaterial;
        [SerializeField] private Material hallwayMaterial;
        [SerializeField] private Material stairsMaterial;

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
            instantPathfindig = true;
            probabilityToSelect = 0.0125;
            seed = string.Empty;
           
        }

        private Random random;
        private Grid<CellType> grid;
        private Graph.Graph graph;
        private List<Room> rooms;
        private List<GameObject> objects;
        private HashSet<Edge> selectedEdges;
        private List<Vector3Int> entries; 
        /*
         * entries - detine inceputul si sfirsutul la path
         * dar acolo esste un if, probabil mai bine din el de luat primul si ultimul halleay
         * iar din afara de luat inceputul si sfirsitul
         * care probabil se afla in room sau altceva
         */

        private void Start()
        {
            random = new Random(randomSeed);
            grid = new Grid<CellType>(size, Vector3Int.zero);
            graph = new Graph.Graph();
            rooms = new List<Room>();
            objects = new List<GameObject>();
            entries = new List<Vector3Int>();

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    for (int k = 0; k < size.z; k++)
                    {
                        grid[i, j, k] = CellType.None;
                    }
                }
            }

            if (seed != string.Empty)
                InputSeed();

            Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D)) { ViewData(); }
            if (Input.GetKeyDown(KeyCode.R)) { ResetData(); }
            if (Input.GetKeyDown(KeyCode.N)) { Generate(); }
            if (Input.GetKeyDown(KeyCode.P)) { Pathfind(); }
            if (Input.GetKeyDown(KeyCode.I)) { InputSeed(); }
            if (Input.GetKeyDown(KeyCode.O)) { OutputSeed(); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.A)) { ViewGridArea(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.E)) { graph.ViewGraph(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.M)) { graph.ViewMST(duration); }
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.S)) { graph.ViewSelectedEdges(duration); }
        }

        private void Generate()
        {
            PlaceRooms();
            CreateGraph();
            if (instantPathfindig)
                Pathfind();
            RemovePrefabFaces();
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
                        PlaceCube(pos, new Vector3Int(1, 1, 1), "CubeRoom", roomMaterial);
                    }
                }
            }
            roomCount = rooms.Count;
        }

        private void PlaceCube(Vector3Int location, Vector3Int size, string tag, Material material)
        {
            GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            SetMaterial(go, material);
            go.tag = tag;
            go.name = tag + " " + (GameObject.FindGameObjectsWithTag(tag).Length);
            objects.Add(go);
        }

        private void PlaceRoom(Vector3Int location, Vector3Int size)
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

            selectedEdges = graph.GetSelectedEdges();

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
                        if (grid[pos] == CellType.Hallway)
                        {
                            PlaceHallway(pos);
                        }
                    }

                    entries.Add(path[0]);
                    entries.Add(path[path.Count - 1]);
                }
            }
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
            Transform transform = gameObject.transform.GetChild(0); // exterior
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                child.GetComponent<MeshRenderer>().material = material;
            }

            transform = gameObject.transform.GetChild(1); // interior
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                child.GetComponent<MeshRenderer>().material = material;
            }
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
                CheckNeighbours(position, "Up");
                CheckNeighbours(position, "Down");
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

            if (grid[position] == grid[position + directionSpecifier[direction]])
            {
                EliminateQuad(position, direction, "Interior");
                EliminateQuad(position, direction, "Exterior");
                EliminateQuad(position + directionSpecifier[direction], opositeDirection[direction], "Interior");
                EliminateQuad(position + directionSpecifier[direction], opositeDirection[direction], "Exterior");
            }
        }

        private void EliminateQuad(Vector3Int position, string tag, string type)
        {
            int childIndex = 0;
            if (type == "Interior")
                childIndex = 1;

            foreach (GameObject gameObject in objects)
            {
                if (position == Vector3Int.FloorToInt(gameObject.transform.position))
                {
                    Transform transform = gameObject.transform.GetChild(childIndex);
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

        // Methods for Debuging
        private void ViewData()
        {
            Debug.Log("Rooms - " + rooms.Count);
            Debug.Log("Rooms Objects - " + GameObject.FindGameObjectsWithTag("Room").Length);
            Debug.Log("CubeRoom Objects - " + GameObject.FindGameObjectsWithTag("CubeRoom").Length + 1);
            Debug.Log("Stairs Objects - " + GameObject.FindGameObjectsWithTag("Stairs").Length + 1);
            Debug.Log("Halways Objects - " + GameObject.FindGameObjectsWithTag("Hallway").Length + 1);
            graph.ViewData();
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