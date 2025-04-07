using System;
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
        public BoundsInt Area
        { get; set; }
        
        public RoomType RoomType { get; set; }
        
        public Tilemap Tilemap => tilemap;

        private void Awake()
        {
            Debug.Log("AWAKEN!");
            tilemap ??= transform.Find("Grid/Tilemap").GetComponent<Tilemap>();
            tilemap.CompressBounds();

            Area = tilemap.cellBounds;
        }

        public int GetHeight() => tilemap.cellBounds.size.y;
        public int GetWidth() => tilemap.cellBounds.size.x;
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
