using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    public static class Direction2D
    {
        public static HashSet<Vector2Int> CardinalDirections = new HashSet<Vector2Int>() { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    }
}
