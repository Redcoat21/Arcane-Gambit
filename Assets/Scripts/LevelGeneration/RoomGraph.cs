using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGeneration
{
    /// <summary>
    /// Singleton that represents a graph of rooms. Only stores the room's metadata, not the room itself.
    /// </summary>
    /// <summary>
    /// Singleton that represents a graph of rooms. Only stores the room's metadata, not the room itself.
    /// </summary>
    public class RoomGraph
    {
        private static RoomGraph _instance;
        private static readonly object Lock = new object();

        public static RoomGraph Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (Lock)
                {
                    _instance ??= new RoomGraph();
                }

                return _instance;
            }
        }

        private RoomGraph()
        {
            Rooms = new Dictionary<Vector2Int, RoomNode>();
        }

        public Dictionary<Vector2Int, RoomNode> Rooms { get; }

        /// <summary>
        /// Add a connection between two rooms in the graph.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void AddConnection(RoomNode source, RoomNode target)
        {
            if (!Rooms.ContainsKey(source.Position))
            {
                AddNode(source);
            }

            source.AddNeighbor(target);
        }

        /// <summary>
        /// Replace the old node with a new node in the graph.
        /// </summary>
        /// <param name="oldNode">The node to be replaced</param>
        /// <param name="newNode">The node to replace with</param>
        public void UpdateNode(RoomNode oldNode, RoomNode newNode)
        {
            // Update the graph with the new node. Note that at this point our newNode have all the neighbors from the old node.
            // But our neighbors still have the old node as their neighbor.
            newNode.Neighbors = oldNode.Neighbors;
            Rooms.Remove(oldNode.Position);
            Rooms[newNode.Position] = newNode;
            
            // We need to update all the neighbors of the old node to point to the new node.
            foreach (var neighbor in newNode.Neighbors)
            {
                neighbor.Neighbors.Remove(oldNode);
                neighbor.Neighbors.Add(oldNode);
            }
        }

        /// <summary>
        /// Add a new node without a connection to the graph.
        /// <param name="node">The node to be added into the graph.</param>
        /// </summary>
        public void AddNode(RoomNode node)
        {
            Rooms[node.Position] = node;
        }

        /// <summary>
        /// Traverse the graph using the specified traversal strategy.
        /// </summary>
        /// <param name="startPosition">The starting position to traverse the graph from</param>
        /// <param name="strategy">The traversal strategy to use (BFS or DFS)</param>
        /// <param name="nodeAction">Action to perform on each visited node</param>
        public void Traverse(Vector2Int startPosition, ITraversalStrategy strategy, Action<RoomNode> nodeAction)
        {
            if (!Rooms.TryGetValue(startPosition, out var room))
            {
                Debug.LogWarning("Start node does not exist in the graph.");
                return;
            }

            strategy.Traverse(this, room, nodeAction);
        }

        /// <summary>
        /// Reset the singleton instance.
        /// </summary>
        public static void ClearGraph()
        {
            lock (Lock)
            {
                _instance?.Rooms.Clear();
            }
        }
    }

    /// <summary>
    /// Room node in the graph. Contains the room's metadata.
    /// <see cref="RoomGraph"/>
    /// </summary>
    public struct RoomNode : IEquatable<RoomNode>
    {
        public Vector2Int Position { get; }
        public RoomType RoomType { get; set; }
        public HashSet<RoomNode> Neighbors { get; set; }

        /// <summary>
        /// Instantiate a new room node with the specified position and room type.
        /// </summary>
        /// <param name="position">The room position, normalized</param>
        /// <param name="roomType">The type of the room</param>
        public RoomNode(Vector2Int position, RoomType roomType = RoomType.Normal)
        {
            Position = position;
            RoomType = roomType;
            Neighbors = new HashSet<RoomNode>();
        }

        /// <summary>
        /// Instantiate a new room node with the specified room type. Position defaulted to (0, 0)
        /// </summary>
        /// <param name="roomType">The type of the room</param>
        public RoomNode(RoomType roomType) : this(Vector2Int.zero, roomType)
        {
        }

        /// <summary>
        /// Add a neighbor to the room node.
        /// </summary>
        /// <param name="neighbor">The room node to be added as a neighbor</param>
        public void AddNeighbor(RoomNode neighbor)
        {
            Neighbors.Add(neighbor);
        }

        public bool Equals(RoomNode other)
        {
            return Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is RoomNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    /// <summary>
    /// Interface for graph traversal strategies.
    /// </summary>
    public interface ITraversalStrategy
    {
        void Traverse(RoomGraph graph, RoomNode startNode, Action<RoomNode> nodeAction);
    }

    /// <summary>
    /// Breadth-First traversal implementation.
    /// </summary>
    public class BreadthFirstTraversal : ITraversalStrategy
    {
        public void Traverse(RoomGraph graph, RoomNode startNode, Action<RoomNode> nodeAction)
        {
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<RoomNode>();

            queue.Enqueue(startNode);
            visited.Add(startNode.Position);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                nodeAction(currentNode);

                foreach (var neighbor in currentNode.Neighbors)
                {
                    if (visited.Contains(neighbor.Position))
                    {
                        continue;
                    }
                
                    // Get the actual node from the graph
                    var actualNeighbor = graph.Rooms[neighbor.Position];
                    visited.Add(actualNeighbor.Position);
                    queue.Enqueue(actualNeighbor);
                }
            }
        }
    }
    
    /// <summary>
    /// Depth-First traversal implementation.
    /// </summary>
    public class DepthFirstTraversal : ITraversalStrategy
    {
        public void Traverse(RoomGraph graph, RoomNode startNode, Action<RoomNode> nodeAction)
        {
            var visited = new HashSet<Vector2Int>();
            DfsRecursive(graph, startNode, visited, nodeAction);
        }

        private static void DfsRecursive(RoomGraph graph, RoomNode currentNode, HashSet<Vector2Int> visited,
            Action<RoomNode> nodeAction)
        {
            visited.Add(currentNode.Position);
            nodeAction(currentNode);

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (visited.Contains(neighbor.Position))
                {
                    continue;
                }
            
                // Get the actual node from the graph
                RoomNode actualNeighbor = graph.Rooms[neighbor.Position];
                DfsRecursive(graph, actualNeighbor, visited, nodeAction);
            }
        }
    }
}
