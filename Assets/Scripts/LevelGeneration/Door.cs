using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGeneration
{
    public enum DoorDirection
    {
        North,
        South,
        East,
        West
    }

    [ExecuteAlways]
    public class Door : MonoBehaviour
    {
        [SerializeField]
        private Tilemap tilemap;

        [SerializeField]
        private int width = 2;

        [SerializeField]
        private int height = 1;

        [SerializeField]
        private DoorDirection doorDirection;

        public DoorDirection DoorDirection => doorDirection;
        public int Width => width;
        public int Height => height;

        public HashSet<Vector3Int> OccupiedTilesPosition => CalculateOccupiedTiles();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            tilemap ??= GetComponent<Tilemap>();
        }

        HashSet<Vector3Int> CalculateOccupiedTiles()
        {
            var occupiedTiles = new HashSet<Vector3Int>();
            var originalTile = tilemap.WorldToCell(transform.position);
            // Move to the right to adjust, according to the width.
            var nextHorizontalTile = originalTile;
            for (int i = 0; i < width; i++)
            {
                var nextVerticalTile = nextHorizontalTile;
                for (int j = 0; j < height; j++)
                {
                    nextVerticalTile += Vector3Int.down;
                    occupiedTiles.Add(nextVerticalTile);
                }

                occupiedTiles.Add(nextHorizontalTile);
                nextHorizontalTile += Vector3Int.right;
            }

            return occupiedTiles;
        }
    }
}
