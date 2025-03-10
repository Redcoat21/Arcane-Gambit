using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    //TODO: Do post processing to assign room types to rooms, and make it more balanced (Merchant and treasure should spawn least once).
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
            // Create the starting room first and then add it to the graph.
            var startingNode = new RoomNode(RoomType.Start);
            _roomGraph.AddNode(startingNode);

            // Create a queue to keep track of the rooms that need to be connected.
            var roomQueue = new Queue<RoomNode>();
            roomQueue.Enqueue(startingNode);

            bool merchantSpawned = false;
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

                    var newRoomNode = CreateRoomNode(roomPosition, merchantSpawned);
                    // If the room is a merchant room, set the flag to true to prevent more from spawning
                    if (newRoomNode.RoomType == RoomType.Merchant)
                    {
                        merchantSpawned = true;
                    }
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

            // IF a merchant room never spawned, then pick a random room and turn it into a merchant room.
            if (!merchantSpawned)
            {
                // Exclude the boss and start room for obvious reason
                var randomRoom = _roomGraph.Rooms.ElementAt(Random.Range(1, _roomGraph.Rooms.Count - 1)).Value;
                _roomGraph.UpdateNode(randomRoom, new RoomNode(randomRoom.Position, RoomType.Merchant));
            }
        }

        private void PostProcessing()
        {
            // TODO: Implement post processing to assign room types to rooms.
            
            // First assign the boss room type. Boss room is the last room generated. (Might need to change into the farthest one)
            var lastRoom = _roomGraph.Rooms.Last().Value;
            _roomGraph.UpdateNode(lastRoom, new RoomNode(lastRoom.Position, RoomType.Boss));
            
            throw new NotImplementedException();
        }

        private void AssignRoomTypes(RoomNode roomNode, RoomType? roomType)
        {
            throw new NotImplementedException();
        }
        
        // TODO: Decide whether to keep this function or not. To assign room type at post processing or not.
        /// <summary>
        /// Create a room node with the specified room type.
        /// </summary>
        /// <param name="roomPosition">The position to create the room to. Note that this position is the position in the graph.</param>
        /// <param name="canSpawnMerchant">Track whether merchant room already spawned or not</param>
        /// <returns>The created room node</returns>
        private RoomNode CreateRoomNode(Vector2Int roomPosition, bool canSpawnMerchant)
        {
            // If it's the last room, spawn a boss room. And also if the boss room spawn is checked.
            if (_roomGraph.Rooms.Count == roomCount - 1 && spawnBoss)
            {
                return new RoomNode(roomPosition, RoomType.Boss);
            }

            // Try spawning a merchant room (30% chance) if one hasn't been spawned yet.
            // Why the randomness? So the merchant room isn't always next to the starting room.
            if (canSpawnMerchant && Random.Range(0, 100) < 30)
            {
                return new RoomNode(roomPosition, RoomType.Merchant);
            }

            // Otherwise, spawn a normal room.
            return new RoomNode(roomPosition);
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
