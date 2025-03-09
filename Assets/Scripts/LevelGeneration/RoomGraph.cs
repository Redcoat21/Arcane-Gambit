using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGeneration
{
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
            Rooms = new Dictionary<RoomNode, HashSet<RoomNode>>();
        }

        public Dictionary<RoomNode, HashSet<RoomNode>> Rooms { get; }

        /// <summary>
        /// Add a connection between two rooms in the graph.
        /// </summary>
        /// <param name="source">The source node to be connected from</param>
        /// <param name="target">The target node to be connected to</param>
        public void AddConnection(RoomNode source, RoomNode target)
        {
            if (!Rooms.ContainsKey(source))
            {
                AddNode(source);
            }

            Rooms[source].Add(target);
        }

        /// <summary>
        /// Add a new node without a connection to the graph.
        /// </summary>
        /// <param name="node">The node to be added</param>
        public void AddNode(RoomNode node)
        {
            Rooms[node] = new HashSet<RoomNode>();
        }

        /// <summary>
        /// Check if the given node exists in the graph.
        /// </summary>
        /// <typeparam name="T">Should be either RoomNode or Vector2Int to represent position.</typeparam>
        /// <param name="identifier">The node to be checked.</param>
        /// <returns>True if the node exists, false otherwise.</returns>
        public bool CheckNode<T>(T identifier) where T : struct
        {
            return identifier switch
            {
                RoomNode roomNode => Rooms.ContainsKey(roomNode),
                Vector2Int position => Rooms.Any(room => room.Key.Position == position),
                _ => throw new ArgumentException(identifier + " is not a valid room node")
            };
        }

        /// <summary>
        /// Traverse the graph using the specified traversal strategy.
        /// </summary>
        /// <param name="startNode">The starting node for traversal</param>
        /// <param name="strategy">The traversal strategy to use (BFS or DFS)</param>
        /// <param name="nodeAction">Action to perform on each visited node</param>
        public void Traverse(RoomNode startNode, ITraversalStrategy strategy, Action<RoomNode> nodeAction)
        {
            if (!Rooms.ContainsKey(startNode))
            {
                Debug.LogWarning("Start node does not exist in the graph.");
                return;
            }

            strategy.Traverse(this, startNode, nodeAction);
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

        /// <summary>
        /// Instantiate a new room node with the specified position and room type.
        /// </summary>
        /// <param name="position">The room position, normalized</param>
        /// <param name="roomType">The type of the room</param>
        public RoomNode(Vector2Int position, RoomType roomType = RoomType.Normal)
        {
            Position = position;
            RoomType = roomType;
        }

        /// <summary>
        /// Instantiate a new room node with the specified room type. Position defaulted to (0, 0)
        /// </summary>
        /// <param name="roomType">The type of the room</param>
        public RoomNode(RoomType roomType) : this(Vector2Int.zero, roomType)
        {
        }

        public bool Equals(RoomNode other)
        {
            return Position.Equals(other.Position) && RoomType == other.RoomType;
        }

        public override bool Equals(object obj)
        {
            return obj is RoomNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, (int)RoomType);
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
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<RoomNode>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                nodeAction(currentNode);

                if (!graph.Rooms.TryGetValue(currentNode, out var neighbors))
                {
                    continue;
                }

                foreach (var neighbor in neighbors.Where(neighbor => !visited.Contains(neighbor)))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
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
            var visited = new HashSet<RoomNode>();
            DfsRecursive(graph, startNode, visited, nodeAction);
        }

        private static void DfsRecursive(RoomGraph graph, RoomNode currentNode, HashSet<RoomNode> visited,
            Action<RoomNode> nodeAction)
        {
            visited.Add(currentNode);
            nodeAction(currentNode);

            if (!graph.Rooms.TryGetValue(currentNode, out var neighbors))
            {
                return;
            }

            foreach (var neighbor in neighbors.Where(neighbor => !visited.Contains(neighbor)))
            {
                DfsRecursive(graph, neighbor, visited, nodeAction);
            }
        }
    }
}
