using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        [Range(10, 100)]
        private int roomCount = 20;
        
        [SerializeField]
        [Range(1, 100)]
        [Tooltip(
            "The chance of a room spawning in a given direction. 100 mean that each room always have 4 rooms connected to it. Higher chance tend to create a more diamond like structure.")]
        private float roomSpawnChance = 60;

        private readonly RoomGraph _roomGraph = RoomGraph.Instance;

        /// <summary>
        /// Entry point for the dungeon generation.
        /// </summary>
        public void CreateLevel()
        {
            // Flush out the old graph and generate a new one. Just to be safe.
            RoomGraph.ClearGraph();
            GenerateGraph();
            PostProcessing();
            
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
        private void PostProcessing()
        {
            // Should be adjusted as needed.
            const int merchantSpawnLimit = 1;
            const int merchantRoomSpawnChance = 30;

            const int treasureRoomSpawnChance = 20;

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
                    }
                    else if (spawnChance < treasureRoomSpawnChance)
                    {
                        AssignRoomTypes(node, RoomType.Treasure);
                    }
                }
            });
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
    }
}
