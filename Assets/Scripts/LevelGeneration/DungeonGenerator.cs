using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    //TODO: Do post processing to assign room types to rooms, and make it more balanced (Merchant and treasure should spawn atleast once).
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField]
        private bool debugMode;
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

        private RoomGraph _roomGraph = RoomGraph.Instance;

        public void CreateLevel()
        {
            // Flush out the old graph and generate a new one. Just to be safe.
            RoomGraph.ClearGraph();
            GenerateGraph();
            if (debugMode)
            {
                _roomGraph.Traverse(_roomGraph.Rooms.First().Key, new BreadthFirstTraversal(), node => Debug.Log(node.RoomType));
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
                    if (spawnChance > roomSpawnChance || _roomGraph.CheckNode(roomPosition))
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

            Debug.Log(merchantSpawned);
            // IF a merchant room never spawned, then pick a random room and turn it into a merchant room.
            // TODO: room is not modified.
            if (!merchantSpawned)
            {
                // Exclude the boss and start room for obvious reason
                var randomRoom = _roomGraph.Rooms.ElementAt(Random.Range(1, _roomGraph.Rooms.Count - 1)).Key;
                randomRoom.RoomType = RoomType.Merchant;
                Debug.Log($"{randomRoom.Position} + {randomRoom.RoomType}");
                Debug.Log(_roomGraph.Rooms.First((pair => pair.Key.Position == randomRoom.Position)));
            }
        }
        
        private RoomNode CreateRoomNode(Vector2Int roomPosition, bool canSpawnMerchant)
        {
            // If it's the last room, spawn a boss room.
            if (_roomGraph.Rooms.Count == roomCount - 1)
            {
                return new RoomNode(roomPosition, RoomType.Boss);
            }

            // Try spawning a merchant room (30% chance) if one hasn't been spawned yet.
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
