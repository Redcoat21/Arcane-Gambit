using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGeneration
{
    [ExecuteAlways]
    public class Room : MonoBehaviour
    {
        [SerializeField]
        private RoomType roomType;

        [SerializeField]
        private Tilemap tilemap;

        [SerializeField]
        private List<Door> doors;

        public BoundsInt Area { get; private set; }

        public RoomType RoomType { get; set; }

        public Tilemap Tilemap => tilemap;

        private void Awake()
        {
            tilemap ??= transform.Find("Grid/Tilemap").GetComponent<Tilemap>();
            tilemap.CompressBounds();

            Area = tilemap.cellBounds;
            CreateDoorway(DoorDirection.South);
        }

        public int GetHeight() => tilemap.cellBounds.size.y;
        public int GetWidth() => tilemap.cellBounds.size.x;

        public void CreateDoorway(DoorDirection direction)
        {
            var door = doors.Find(d => d.DoorDirection == direction);
            if (door != null)
            {
                Debug.Log(door.OccupiedTilesPosition.Count);
                foreach (var tilePosition in door.OccupiedTilesPosition)
                {
                    tilemap.SetTile(tilePosition, null);
                }
            }
            else
            {
                Debug.LogError($"No door found for direction {direction}");
            }
        }
    }

    public enum RoomType
    {
        Normal,
        Merchant,
        Boss,
        Treasure,
        Minigames,
        Start,
    }
}
