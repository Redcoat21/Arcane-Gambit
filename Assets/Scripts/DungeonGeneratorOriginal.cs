using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DungeonGeneratorOriginal : MonoBehaviour
{
    [SerializeField]
    private GameObject[] roomPrefabs;

    [SerializeField]
    private int minRooms = 8;

    [SerializeField]
    private int maxRooms = 10;

    [SerializeField]
    [Range(10f, 50f)]
    private float baseRoomSpacing = 20f; // Base distance between room centers

    [SerializeField]
    [Range(1f, 3f)]
    private float spacingMultiplier = 1.2f; // Extra spacing to prevent overlap

    [Header("Room Types")]
    [SerializeField]
    private bool generateBossRoom = true;

    [SerializeField]
    private bool generateStartRoom = true;

    [SerializeField]
    private bool generateTreasureRoom = true;

    [SerializeField]
    private TileBase floorTile;
    [SerializeField]
    private TileBase wallTile;

    private readonly Dictionary<Vector2Int, Room> _placedRooms = new Dictionary<Vector2Int, Room>();
    private readonly Dictionary<Vector2Int, Vector2Int> _roomSizes = new Dictionary<Vector2Int, Vector2Int>();

    // Graph representation
    private readonly List<RoomNode> _roomGraph = new List<RoomNode>();

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        ClearExistingDungeon();
        GenerateRoomGraph();
        CalculateRoomSizes();
        PlaceRoomsInWorld();
        ConnectRooms();
        PopulateRooms();
    }

    private void ClearExistingDungeon()
    {
        // Clear any existing dungeon
        _placedRooms.Clear();
        _roomGraph.Clear();
        _roomSizes.Clear();

        // Destroy existing room GameObjects if there are any
        var existingRooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var room in existingRooms)
        {
            Destroy(room.gameObject);
        }
    }

    private void GenerateRoomGraph()
    {
        // Determine the number of rooms to generate
        int numRooms = Random.Range(minRooms, maxRooms + 1);

        // Start with creating a central room
        RoomNode startNode = new RoomNode { Position = Vector2Int.zero, Type = RoomType.Start };
        _roomGraph.Add(startNode);

        // Generate other rooms branching from start room
        Queue<RoomNode> frontier = new Queue<RoomNode>();
        frontier.Enqueue(startNode);

        while (_roomGraph.Count < numRooms && frontier.Count > 0)
        {
            RoomNode currentNode = frontier.Dequeue();

            // Try to add rooms in cardinal directions
            foreach (var direction in CardinalDirections())
            {
                // Don't always add a room in each direction
                if (!(Random.value < 0.6f) || _roomGraph.Count >= numRooms)
                {
                    continue;
                }

                var newPosition = currentNode.Position + direction;

                // Check if position is already taken
                if (_roomGraph.Any(r => r.Position == newPosition))
                    continue;

                // Create new room
                RoomType roomType = DetermineRoomType(_roomGraph.Count, numRooms);
                RoomNode newNode = new RoomNode
                {
                    Position = newPosition,
                    Type = roomType,
                    Connections = new List<RoomNode>()
                };

                // Connect the rooms
                currentNode.Connections.Add(newNode);
                newNode.Connections.Add(currentNode);

                _roomGraph.Add(newNode);
                frontier.Enqueue(newNode);
            }
        }

        // Ensure we have a boss room and a treasure room if needed
        EnsureSpecialRooms();
    }

    private void CalculateRoomSizes()
    {
        // Pre-calculate sizes of all room prefabs
        foreach (RoomNode node in _roomGraph)
        {
            // Select a random room prefab for this node
            int prefabIndex = Random.Range(0, roomPrefabs.Length);
            GameObject selectedPrefab = roomPrefabs[prefabIndex];

            // Store which prefab we'll use for this room
            node.PrefabIndex = prefabIndex;

            // Temporarily instantiate to get its dimensions (will be destroyed)
            GameObject tempRoom = Instantiate(selectedPrefab, new Vector3(-1000, -1000, -1000), Quaternion.identity);
            tempRoom.AddComponent<Room>();
            Room roomComponent = tempRoom.GetComponent<Room>();

            // Make sure tilemaps are initialized
            roomComponent.FindTilemaps();
            roomComponent.CalculateRoomBounds();

            // Get the bounds and store the size
            BoundsInt bounds = roomComponent.GetRoomBounds();
            Vector2Int size = new Vector2Int(bounds.size.x, bounds.size.y);

            // Store the size for this room position
            _roomSizes[node.Position] = size;
            // else
            // {
            //     // Default size if Room component not found
            //     roomSizes[node.Position] = new Vector2Int(10, 10);
            //     Debug.LogWarning($"Room prefab {selectedPrefab.name} is missing Room component!");
            // }

            // Clean up a temporary object
            Destroy(tempRoom);
        }
    }

    private Vector3 CalculateRoomPosition(RoomNode node)
    {
        // For the first room, place at origin
        if (node.Position == Vector2Int.zero)
        {
            return Vector3.zero;
        }

        // Find the connected room that's already been placed
        RoomNode connectedNode = node.Connections.FirstOrDefault(n => _placedRooms.ContainsKey(n.Position));

        if (connectedNode == null)
        {
            UnityEngine.Debug.LogError($"Cannot place room at {node.Position}: No connected rooms have been placed yet!");
            return Vector3.zero;
        }

        // Get the already placed room
        Room connectedRoom = _placedRooms[connectedNode.Position];
        Vector3 connectedRoomPos = connectedRoom.transform.position;

        // Direction from connected room to this room
        Vector2Int direction = node.Position - connectedNode.Position;

        // Get sizes of both rooms
        Vector2Int connectedRoomSize = _roomSizes[connectedNode.Position];
        Vector2Int currentRoomSize = _roomSizes[node.Position];

        // Calculate spacing based on the sizes of the rooms in the direction we're moving
        float spacingX = baseRoomSpacing;
        float spacingY = baseRoomSpacing;

        if (direction.x != 0)
        {
            // Moving horizontally - use room widths for spacing
            spacingX = (connectedRoomSize.x + currentRoomSize.x) * 0.5f * spacingMultiplier;
        }

        if (direction.y != 0)
        {
            // Moving vertically - use room heights for spacing
            spacingY = (connectedRoomSize.y + currentRoomSize.y) * 0.5f * spacingMultiplier;
        }

        // Calculate position based on direction and spacing
        Vector3 newPosition = new Vector3(
            Mathf.Round(connectedRoomPos.x + direction.x * spacingX),
            Mathf.Round(connectedRoomPos.y + direction.y * spacingY),
            0
        );

        return newPosition;
    }

    private void EnsureSpecialRooms()
    {
        if (generateBossRoom && _roomGraph.All(r => r.Type != RoomType.Boss))
        {
            // Find a leaf node (has only one connection) furthest from start
            RoomNode leafNode = _roomGraph
                .Where(r => r.Connections.Count == 1 && r.Type == RoomType.Normal)
                .OrderByDescending(r => Vector2Int.Distance(r.Position, Vector2Int.zero))
                .FirstOrDefault();

            if (leafNode != null)
                leafNode.Type = RoomType.Boss;
        }

        if (generateTreasureRoom && _roomGraph.All(r => r.Type != RoomType.Treasure))
        {
            // Find a different leaf node for treasure
            RoomNode leafNode = _roomGraph
                .FirstOrDefault(r => r.Connections.Count == 1 && r.Type == RoomType.Normal);

            if (leafNode != null)
                leafNode.Type = RoomType.Treasure;
        }
    }

    private RoomType DetermineRoomType(int currentRoomCount, int totalRooms)
    {
        // The First room is always the start room
        if (currentRoomCount == 0 && generateStartRoom)
            return RoomType.Start;

        // The Last room is boss room
        if (currentRoomCount == totalRooms - 1 && generateBossRoom)
            return RoomType.Boss;

        // Random chance for treasure room
        if (generateTreasureRoom && Random.value < 0.1f && _roomGraph.All(r => r.Type != RoomType.Treasure))
            return RoomType.Treasure;

        // Default is a normal room
        return RoomType.Normal;
    }

    private void PlaceRoomsInWorld()
    {
        // First place the root (start) room
        RoomNode startNode = _roomGraph.FirstOrDefault(n => n.Position == Vector2Int.zero);
        if (startNode != null)
        {
            PlaceRoom(startNode, Vector3.zero);
        }

        // Then place all other rooms relative to their connected rooms
        foreach (RoomNode node in _roomGraph.Where(n => n.Position != Vector2Int.zero))
        {
            Vector3 worldPosition = CalculateRoomPosition(node);
            PlaceRoom(node, worldPosition);
        }
    }

    private void PlaceRoom(RoomNode node, Vector3 worldPosition)
    {
        // Get the prefab index for this room
        int prefabIndex = node.PrefabIndex;

        // Instantiate room prefab
        GameObject roomObject = Instantiate(roomPrefabs[prefabIndex], worldPosition, Quaternion.identity);
        roomObject.transform.parent = transform;

        // Add a Room component if it doesn't exist
        Room roomComponent = roomObject.GetComponent<Room>();
        if (roomComponent == null)
            roomComponent = roomObject.AddComponent<Room>();

        // Configure room
        roomComponent.RoomType = node.Type;
        roomComponent.GridPosition = node.Position;

        // Store reference to placed room
        _placedRooms[node.Position] = roomComponent;

        // Store node reference in room for connection creation later
        roomComponent.Node = node;

        // Name the room based on type and position
        roomObject.name = $"Room_{node.Type}_{node.Position.x}_{node.Position.y}";
    }

    private void ConnectRooms()
    {
        // Track which connections we've already processed
        HashSet<string> processedConnections = new HashSet<string>();

        // Make sure all rooms have calculated their bounds
        foreach (var roomEntry in _placedRooms)
        {
            Room room = roomEntry.Value;
            // Force bounds calculation
            room.FindTilemaps();
            room.CalculateRoomBounds();
        }

        // Process all rooms to create doorways and corridors
        foreach (RoomNode node in _roomGraph)
        {
            if (!_placedRooms.TryGetValue(node.Position, out var currentRoom))
            {
                UnityEngine.Debug.LogError($"Missing room at position {node.Position}");
                continue;
            }

            // Connect to each adjacent room
            foreach (RoomNode connectedNode in node.Connections)
            {
                if (!_placedRooms.TryGetValue(connectedNode.Position, out var targetRoom))
                {
                    UnityEngine.Debug.LogError($"Missing connected room at position {connectedNode.Position}");
                    continue;
                }

                // Create a unique ID for this connection (order-independent)
                string connectionId = GetConnectionId(node.Position, connectedNode.Position);

                // Skip if we've already processed this connection
                if (processedConnections.Contains(connectionId))
                {
                    continue;
                }

                // Get the direction between rooms
                Vector2Int direction = connectedNode.Position - node.Position;

                // Normalize the direction to ensure it's a unit vector in one of the cardinal directions
                if (direction.x != 0) direction.x = direction.x / Mathf.Abs(direction.x);
                if (direction.y != 0) direction.y = direction.y / Mathf.Abs(direction.y);

                // Create a door in the current room
                Vector3Int sourceDoorPos = currentRoom.CreateDoorway(direction);

                // Create a door in the connected room (opposite direction)
                Vector2Int oppositeDirection = new Vector2Int(-direction.x, -direction.y);
                Vector3Int targetDoorPos = targetRoom.CreateDoorway(oppositeDirection);

                // Convert door positions to world space
                Vector3 sourceWorldPos = currentRoom.GetWorldPositionFromTile(sourceDoorPos);
                Vector3 targetWorldPos = targetRoom.GetWorldPositionFromTile(targetDoorPos);

                // Create corridor between the doors
                CreateCorridor(currentRoom, sourceWorldPos, targetRoom, targetWorldPos, direction);

                // Mark this connection as processed
                processedConnections.Add(connectionId);
            }
        }

        UnityEngine.Debug.Log("Room connections complete: " + _roomGraph.Count + " rooms connected with corridors.");
    }

    private string GetConnectionId(Vector2Int pos1, Vector2Int pos2)
    {
        // Create a consistent ID regardless of order
        return pos1.x < pos2.x || (pos1.x == pos2.x && pos1.y < pos2.y)
            ? $"{pos1}_{pos2}"
            : $"{pos2}_{pos1}";
    }

    private void CreateCorridor(Room sourceRoom, Vector3 sourceWorldPos, Room targetRoom, Vector3 targetWorldPos,
        Vector2Int direction)
    {
        // Create corridor GameObject
        GameObject corridorObject = new GameObject($"Corridor_{sourceRoom.GridPosition}_{targetRoom.GridPosition}");
        corridorObject.transform.parent = transform;

        // Add corridor component
        Corridor corridor = corridorObject.AddComponent<Corridor>();

        // Set corridor properties if needed
        // corridor.SetTiles(floorTile, wallTile);

        // Initialize the corridor
        corridor.Initialize(sourceWorldPos, targetWorldPos, direction, wallTile, floorTile);
    }

    private void PopulateRooms()
    {
        foreach (var roomEntry in _placedRooms)
        {
            Room room = roomEntry.Value;

            // Populate room based on its type
            switch (room.RoomType)
            {
                case RoomType.Start:
                    // Empty room or minimal enemies
                    break;

                case RoomType.Boss:
                    // Add a boss enemy
                    break;

                case RoomType.Treasure:
                    // Add treasure chests or rewards
                    break;

                case RoomType.Normal:
                    // Add standard enemies and props
                    break;
            }
        }
    }

    private Vector2Int[] CardinalDirections()
    {
        return new[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
    }
}

public enum RoomType
{
    Normal,
    Start,
    Boss,
    Treasure,
    Shop,
    Secret
}


// Extended RoomNode class with prefab index
public class RoomNode
{
    public Vector2Int Position;
    public RoomType Type = RoomType.Normal;
    public List<RoomNode> Connections = new List<RoomNode>();
    public int PrefabIndex; // Stores which prefab to use for this room
}
