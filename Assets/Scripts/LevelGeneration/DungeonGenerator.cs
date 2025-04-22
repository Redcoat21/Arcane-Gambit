using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    //TODO: Make room more balanced (Merchant and treasure should spawn least once).
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField]
        private bool debugMode;

        [Header("Level Configuration")]
        [SerializeField]
        private bool spawnBoss;

        [Header("Room Configuration")]
        [SerializeField]
        private GameObject[] roomPrefabs;

        [SerializeField]
        private int roomGap = 6;

        [SerializeField]
        [Range(10, 100)]
        private int roomCount = 20;

        [SerializeField]
        [Range(1, 100)]
        [Tooltip(
            "The chance of a room spawning in a given direction. 100 mean that each room always have 4 rooms connected to it. Higher chance tend to create a more diamond like structure.")]
        private float roomSpawnChance = 60;

        [Header("Corridor Configuration")]
        [SerializeField]
        private TileBase corridorFloorTile;

        [Header("Corridor Wall Tiles")]
        [SerializeField]
        private TileBase verticalLeftWallTile; // For left walls in vertical corridors

        [SerializeField]
        private TileBase verticalRightWallTile; // For right walls in vertical corridors

        [SerializeField]
        private TileBase horizontalUpWallTile; // For upper walls in horizontal corridors

        [SerializeField]
        private TileBase horizontalDownWallTile; // For lower walls in horizontal corridors

        private readonly RoomGraph _roomGraph = RoomGraph.Instance;

        [SerializeField]
        private GameObject levelRoot;

        [SerializeField]
        private TileBase backgroundWallTile; // Dark wall tile for the background

        /// <summary>
        /// Entry point for the dungeon generation.
        /// </summary>
        public void CreateLevel()
        {
            // Flush out the old graph and generate a new one. Just to be safe.
            RoomGraph.ClearGraph();
            GenerateGraph();
            PostProcessing();
            PlaceRoomPrefab();
            // Fill background first (so it's behind everything)
            FillBackgroundWalls();
            // ConnectRooms();

            // If debug mode is enabled, visualize the graph.
            if (debugMode)
            {
                GraphVisualizer.DrawGraph(_roomGraph);
            }
        }

        /// <summary>
        /// Create a room graph that contains room's metadata.
        /// </summary>
        private void GenerateGraph()
        {
            // Create a queue to keep track of the rooms that need to be connected.
            var roomQueue = new Queue<RoomNode>();
            roomQueue.Enqueue(new RoomNode(Vector2Int.zero));

            while (_roomGraph.Rooms.Count < roomCount)
            {
                var currentNode = roomQueue.Dequeue();

                var validRoomNodes = new List<RoomNode>();

                // Check each direction to see if a room should be spawned
                foreach (var direction in Direction2D.CardinalDirections)
                {
                    var roomPosition = direction + currentNode.Position;
                    int spawnChance = Random.Range(0, 101);

                    // Check if the room should be spawned and if it doesn't already exist
                    if (spawnChance > roomSpawnChance || _roomGraph.Rooms.ContainsKey(roomPosition))
                    {
                        continue;
                    }

                    var newRoomNode = new RoomNode(roomPosition);
                    validRoomNodes.Add(newRoomNode);

                    // Prevent overflow by stopping when we reach the room limit
                    if (_roomGraph.Rooms.Count + validRoomNodes.Count >= roomCount)
                    {
                        break;
                    }
                }

                // Process valid room nodes
                foreach (var newRoomNode in validRoomNodes)
                {
                    AddConnection(currentNode, newRoomNode);
                    roomQueue.Enqueue(newRoomNode);
                }

                // If the queue is empty, add a new random room to ensure progression
                if (roomQueue.Count == 0)
                {
                    var direction =
                        Direction2D.CardinalDirections.ElementAt(Random.Range(0, Direction2D.CardinalDirections.Count));
                    var roomPosition = direction + currentNode.Position;
                    var newRoomNode = new RoomNode(roomPosition);
                    AddConnection(currentNode, newRoomNode);
                    roomQueue.Enqueue(newRoomNode);
                }
            }
        }

        /// <summary>
        /// Post-processing the graph to assign room types to rooms.
        /// </summary>
        // TODO: Maybe refactor this to make it more readable.
        private void PostProcessing()
        {
            // Should be adjusted as needed.
            const int merchantSpawnLimit = 1;
            const int merchantRoomSpawnChance = 30;

            const int treasureRoomSpawnChance = 20;

            // Mandatory room flag.
            bool merchantRoomSpawned = false;
            bool treasureRoomSpawned = false;

            int merchantNodeCounter = 0;
            // Start from the root node and traverse the graph. If the root node is not set, then defaulted to (0,0)
            // Its kind of redundant because most of the time root is at (0,0)
            var startingPosition = _roomGraph.Root?.Position ?? Vector2Int.zero;

            _roomGraph.Traverse(startingPosition, new BreadthFirstTraversal(), node =>
            {
                // If its last room, then assign it as a boss room.
                if (node == _roomGraph.Rooms.Last().Value && spawnBoss)
                {
                    AssignRoomTypes(node, RoomType.Boss);
                }
                else if (node == _roomGraph.Rooms.First().Value)
                {
                    AssignRoomTypes(node, RoomType.Start);
                }
                else
                {
                    int spawnChance = Random.Range(0, 100 + 1);
                    // Assign a room type based on the room type.
                    // Limit merchant spawns in a level.
                    if (spawnChance < merchantRoomSpawnChance && merchantNodeCounter < merchantSpawnLimit)
                    {
                        AssignRoomTypes(node, RoomType.Merchant);
                        merchantNodeCounter++;
                        merchantRoomSpawned = true;
                    }
                    else if (spawnChance < treasureRoomSpawnChance)
                    {
                        AssignRoomTypes(node, RoomType.Treasure);
                        treasureRoomSpawned = true;
                    }
                }
            });

            // If no treasure room spawned, then spawn it at a random room.
            // The same is also applied to the merchant room.
            RoomNode randomRoom;
            if (!treasureRoomSpawned)
            {
                // Get a random normal type room.
                randomRoom = _roomGraph.Rooms.Where(x => x.Value.RoomType == RoomType.Normal).Select(x => x.Value)
                    .OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                AssignRoomTypes(randomRoom, RoomType.Treasure);
            }

            if (!merchantRoomSpawned)
            {
                // Get a random normal type room.
                randomRoom = _roomGraph.Rooms.Where(x => x.Value.RoomType == RoomType.Normal).Select(x => x.Value)
                    .OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                AssignRoomTypes(randomRoom, RoomType.Merchant);
            }
        }

        /// <summary>
        /// Connect each room with its neighbors by creating corridors.
        /// </summary>
        private void ConnectRooms()
        {
            // Use a HashSet to track which connections we've already processed
            var processedConnections = new HashSet<(Vector2Int, Vector2Int)>();

            // Traverse all rooms in the graph using BFS
            _roomGraph.Traverse(Vector2Int.zero, new BreadthFirstTraversal(), node =>
            {
                // For each room, connect it with all of its neighbors
                foreach (var neighbor in node.Neighbors)
                {
                    // Create a unique key for this connection
                    var connection = node.Position.x < neighbor.Position.x ||
                                     (node.Position.x == neighbor.Position.x && node.Position.y < neighbor.Position.y)
                        ? (node.Position, neighbor.Position)
                        : (neighbor.Position, node.Position);

                    // Skip if we've already processed this connection
                    if (processedConnections.Contains(connection))
                        continue;

                    CreateCorridor(node, neighbor);
                    processedConnections.Add(connection);
                }
            });
        }

        /// <summary>
        /// Create a corridor between two rooms.
        /// </summary>
        /// <param name="nodeA">The first room</param>
        /// <param name="nodeB">The second room</param>
        private void CreateCorridor(RoomNode nodeA, RoomNode nodeB)
        {
            // Find the rooms in the scene by their position
            var roomA = FindRoomByPosition(nodeA.Position);
            var roomB = FindRoomByPosition(nodeB.Position);

            if (roomA == null || roomB == null)
            {
                Debug.LogError($"Could not find rooms at positions {nodeA.Position} and/or {nodeB.Position}");
                return;
            }

            // Calculate direction from A to B
            var direction = nodeB.Position - nodeA.Position;

            // Get room bounds in world space
            var roomABounds = GetRoomWorldBounds(roomA);
            var roomBBounds = GetRoomWorldBounds(roomB);

            // Calculate exit points for both rooms
            var exitA = CalculateExitPoint(roomA, direction);
            var exitB = CalculateExitPoint(roomB, -direction);

            // Create a corridor GameObject
            var corridor = new GameObject($"Corridor_{nodeA.Position}_{nodeB.Position}");
            corridor.transform.SetParent(levelRoot.transform);

            // Add a Grid and Tilemap for the corridor
            var grid = corridor.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            var tilemapObject = new GameObject("Tilemap");
            tilemapObject.transform.SetParent(corridor.transform);

            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = 0;

            // Create a corridor between exits
            CreateCorridorPath(tilemap, exitA, exitB,
                verticalLeftWallTile, verticalRightWallTile,
                horizontalUpWallTile, horizontalDownWallTile,
                corridorFloorTile);

            // Create doorways in both rooms
            CreateDoorway(roomA, direction);
            CreateDoorway(roomB, -direction);
        }

        private Vector3 CalculateExitPoint(Room room, Vector2Int direction)
        {
            var bounds = room.Area;
            Vector3Int cellPosition;

            // Calculate the middle point of the wall based on a direction
            if (direction == Vector2Int.up || direction == Vector2Int.down)
            {
                int middleX = (int)bounds.center.x;
                int y = (direction == Vector2Int.up) ? bounds.max.y - 1 : bounds.min.y;
                cellPosition = new Vector3Int(middleX, y, 0);
            }
            else // left or right
            {
                int middleY = (int)bounds.center.y;
                int x = (direction == Vector2Int.right) ? bounds.max.x - 1 : bounds.min.x;
                cellPosition = new Vector3Int(x, middleY, 0);
            }

            // Convert the cell position to world position
            return room.Tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0); // Center of the tile
        }

        /// <summary>
        /// Get the world bounds of a room.
        /// </summary>
        /// <param name="room">The room to get world bounds from</param>
        /// <returns>The world bounds of the room</returns>
        private Bounds GetRoomWorldBounds(Room room)
        {
            // Get the bounds in cell coordinates
            var cellBounds = room.Area;

            // Convert to world coordinates
            var min = room.Tilemap.CellToWorld(new Vector3Int(cellBounds.min.x, cellBounds.min.y, 0));
            var max = room.Tilemap.CellToWorld(new Vector3Int(cellBounds.max.x, cellBounds.max.y, 0)) +
                      new Vector3(1, 1, 0);

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        /// <summary>
        /// Create a corridor path between two points.
        /// </summary>
        /// <param name="tilemap">The corridor's tilemap</param>
        /// <param name="startPos">Create from where?</param>
        /// <param name="endPos">Endpoint of the corridor</param>
        /// <param name="horizontalWallTile">Wall tile when in a horizontal corridor (along the x-axis)</param>
        /// <param name="verticalWallTile">Wall tile when in a vertical corridor (along the y-axis)</param>
        /// <param name="floorTile">The corridor floor tile</param>
        private void CreateCorridorPath(Tilemap tilemap, Vector3 startPos, Vector3 endPos,
            TileBase verticalLeftWall, TileBase verticalRightWall,
            TileBase horizontalUpWall, TileBase horizontalDownWall,
            TileBase floorTile)
        {
            // Convert world positions to cell positions for the corridor tilemap
            Vector3Int startCell = tilemap.WorldToCell(startPos);
            Vector3Int endCell = tilemap.WorldToCell(endPos);

            // Determine corridor width
            const int corridorWidth = 2;

            // Check if a corridor is horizontal or vertical
            bool isHorizontal = Mathf.Abs(endCell.x - startCell.x) > Mathf.Abs(endCell.y - startCell.y);

            if (isHorizontal)
            {
                // Create horizontal corridor
                int minX = Mathf.Min(startCell.x, endCell.x);
                int maxX = Mathf.Max(startCell.x, endCell.x);
                int y = startCell.y;

                // Create floor tiles first
                for (int x = minX; x <= maxX; x++)
                {
                    for (int i = 0; i < corridorWidth; i++)
                        tilemap.SetTile(new Vector3Int(x, y + i, 0), floorTile);
                }

                // Create walls without extending beyond the corridor ends
                for (int x = minX; x <= maxX; x++)
                {
                    // Bottom wall
                    tilemap.SetTile(new Vector3Int(x, y - 1, 0), horizontalDownWallTile);

                    // Top wall
                    tilemap.SetTile(new Vector3Int(x, y + corridorWidth, 0), horizontalUpWallTile);
                }
            }
            else
            {
                // Create vertical corridor
                int minY = Mathf.Min(startCell.y, endCell.y);
                int maxY = Mathf.Max(startCell.y, endCell.y);
                int x = startCell.x;

                // Create floor tiles first
                for (int y = minY; y <= maxY; y++)
                {
                    for (int i = 0; i < corridorWidth; i++)
                        tilemap.SetTile(new Vector3Int(x + i, y, 0), floorTile);
                }

                // Create walls without extending beyond the corridor ends
                for (int y = minY; y <= maxY; y++)
                {
                    // Left wall
                    tilemap.SetTile(new Vector3Int(x - 1, y, 0), verticalLeftWallTile);

                    // Right wall
                    tilemap.SetTile(new Vector3Int(x + corridorWidth, y, 0), verticalRightWallTile);
                }
            }
        }

        /// <summary>
        /// Create a doorway in the room
        /// </summary>
        /// <param name="room">The room to create a doorway in</param>
        /// <param name="direction">Which direction should the doorway create to?</param>
        private void CreateDoorway(Room room, Vector2Int direction)
        {
            // Get the tilemap of the room
            var tilemap = room.Tilemap;
            var bounds = room.Area;

            // Calculate the doorway position based on the direction and room bounds
            Vector3Int doorPosition;
            int doorWidth = 2; // Width of the door in tiles

            if (direction == Vector2Int.up || direction == Vector2Int.down)
            {
                // For up/down doors, place them in the middle of the top/bottom wall
                int middleX = (int)bounds.center.x;
                int y = (direction == Vector2Int.up) ? bounds.max.y - 1 : bounds.min.y;

                // Clear tiles to create the doorway
                for (int i = -doorWidth / 2; i <= doorWidth / 2; i++)
                {
                    doorPosition = new Vector3Int(middleX + i, y, 0);
                    tilemap.SetTile(doorPosition, null);
                }
            }
            else // direction is left or right
            {
                // For left/right doors, place them in the middle of the left/right wall
                int middleY = (int)bounds.center.y;
                int x = (direction == Vector2Int.right) ? bounds.max.x - 1 : bounds.min.x;

                // Clear tiles to create the doorway
                for (int i = -doorWidth / 2; i <= doorWidth / 2; i++)
                {
                    doorPosition = new Vector3Int(x, middleY + i, 0);
                    tilemap.SetTile(doorPosition, null);
                }
            }
        }

        // TODO: There must be a better way to do this.
        /// <summary>
        /// Find a room in the scene by its position.
        /// </summary>
        /// <param name="position">The searched position</param>
        /// <returns>The founded room</returns>
        private Room FindRoomByPosition(Vector2Int position)
        {
            // Search for a room with the name format "Room_<position>_..."
            string searchPattern = $"Room_{position}_";
            return (from Transform child in levelRoot.transform
                where child.name.StartsWith(searchPattern)
                select child.GetComponent<Room>()).FirstOrDefault();
        }

        /// <summary>
        /// Helper function to update the room type in the graph.
        /// </summary>
        /// <param name="roomNode">The room node to be updated</param>
        /// <param name="roomType">The new room node room type</param>
        private void AssignRoomTypes(RoomNode roomNode, RoomType roomType = RoomType.Normal)
        {
            _roomGraph.UpdateNode(roomNode, new RoomNode(roomNode.Position, roomType));
        }

        /// <summary>
        /// Helper functions to add a connection between two rooms in the graph. It will act bidirectionally.
        /// </summary>
        /// <param name="nodeA">The starting node</param>
        /// <param name="nodeB">The target node</param>
        private void AddConnection(RoomNode nodeA, RoomNode nodeB)
        {
            // Add connection from A to B.
            _roomGraph.AddConnection(nodeA, nodeB);
            // Add connection from B to A.
            _roomGraph.AddConnection(nodeB, nodeA);
        }

        // WARNING: This function is very expensive, luckily it's only called once per level, but still if we have the time, we should optimize this.
        /// <summary>
        /// Place the predefined room prefabs into the world using our graph.
        /// </summary>
        private void PlaceRoomPrefab()
        {
            _roomGraph.Traverse(Vector2Int.zero, new BreadthFirstTraversal(), node =>
            {
                // Instantiate the room at (0, 0, 0) first for now.
                // This is done so the area is calculated, and we can adjust it accordingly later.
                var roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                var room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, levelRoot.transform);
                var roomComponent = room.GetComponent<Room>();
                // The New position will be the position of the node (graph position) multiplied by the room width and height + the room gap.
                var newPosition = new Vector3(node.Position.x * (roomComponent.GetWidth() + roomGap),
                    node.Position.y * (roomComponent.GetHeight() + roomGap), 0);
                room.transform.position = newPosition;

                // Then assign its room type.
                roomComponent.RoomType = node.RoomType;

                // Rename it for debugging purposes.
                // Name follow this format "Room_<Graph_Position>_<RoomType>
                room.name =
                    $"Room_{node.Position}_{node.RoomType}_{roomComponent.GetWidth()}x{roomComponent.GetHeight()}";
            });
        }


        /// <summary>
        /// Calculates the bounds of the entire dungeon based on room positions
        /// </summary>
        private BoundsInt CalculateDungeonBounds()
        {
            BoundsInt bounds = new BoundsInt();
            bool firstRoom = true;

            // Go through all rooms to find the min and max boundaries
            foreach (Transform roomTransform in levelRoot.transform)
            {
                Room room = roomTransform.GetComponent<Room>();
                if (room == null) continue;

                // Get room bounds in world space
                BoundsInt roomBounds = room.Area;

                // Adjust for room's position
                Vector3Int roomPosition = Vector3Int.FloorToInt(room.transform.position);
                roomBounds.position += roomPosition;

                if (firstRoom)
                {
                    bounds = roomBounds;
                    firstRoom = false;
                }
                else
                {
                    // Expand bounds to include this room
                    bounds.min = new Vector3Int(
                        Mathf.Min(bounds.min.x, roomBounds.min.x),
                        Mathf.Min(bounds.min.y, roomBounds.min.y),
                        0);

                    bounds.max = new Vector3Int(
                        Mathf.Max(bounds.max.x, roomBounds.max.x),
                        Mathf.Max(bounds.max.y, roomBounds.max.y),
                        0);
                }
            }

            return bounds;
        }


        /// <summary>
        /// Fills the entire dungeon area with background wall tiles
        /// </summary>
        private void FillBackgroundWalls()
        {
            // Create a GameObject for the background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(levelRoot.transform);

            // Add Grid and Tilemap components
            Grid grid = background.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            GameObject tilemapObject = new GameObject("Tilemap");
            tilemapObject.transform.SetParent(background.transform);

            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = -1; // Set this to be behind other tilemaps

            // Calculate bounds that cover all rooms
            BoundsInt dungeonBounds = CalculateDungeonBounds();

            // Add a buffer around the dungeon bounds
            int buffer = 10;
            dungeonBounds.min -= new Vector3Int(buffer, buffer, 0);
            dungeonBounds.max += new Vector3Int(buffer, buffer, 0);

            // Fill the entire area with background wall tiles
            for (int x = dungeonBounds.min.x; x < dungeonBounds.max.x; x++)
            {
                for (int y = dungeonBounds.min.y; y < dungeonBounds.max.y; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), backgroundWallTile);
                }
            }
        }
    }


}
