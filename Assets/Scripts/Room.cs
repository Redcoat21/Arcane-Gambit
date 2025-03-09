using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [SerializeField]
    private RoomType roomType = RoomType.Normal;

    public RoomType RoomType
    {
        get => roomType;
        set => roomType = value;
    }
    
    public Vector2Int GridPosition;
    public RoomNode Node;
    
    [Header("Room Properties")]
    public int DoorWidth = 2; // How many tiles wide the door should be
    public bool DoorsCreated = false;
    
    // References
    private Tilemap floorTilemap;
    private Tilemap wallTilemap;
    
    // Room bounds and center (calculated from tilemap)
    private BoundsInt roomBounds;
    private Vector3Int roomCenter;

    [SerializeField]
    private GameObject[] doorways;
    
    private void Awake()
    {
        FindTilemaps();
        CalculateRoomBounds();
        
        foreach(var doorway in doorways)
        {
            doorway.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    
    /// <summary>
    /// Form doorway to be always in the center of the respective axis.
    /// <example>Size 10x10 means the doorway will be at (5,10), (5,-10), (10, 5), (-10, 5)</example>
    /// </summary>
    private void FormDoorway()
    {
        // Get the bounds of the tilemap (area where tiles exist)
        var roomBounds = floorTilemap.cellBounds;
        
        // Calculate the center of the room in tilemap coordinates
        var roomCenter = new Vector3Int(
            roomBounds.x + roomBounds.size.x / 2,
            roomBounds.y + roomBounds.size.y / 2,
            0
        );
        
        // Calculate the door width in tilemap units
        int halfWidth = DoorWidth / 2;
        
        // Create doorways at the center of each wall
        // CreateDoorway(new Vector2Int(roomCenter.x, roomBounds.yMax - 1), Vector2Int.up);
        // CreateDoorway(new Vector2Int(roomCenter.x, roomBounds.yMin), Vector2Int.down);
        // CreateDoorway(new Vector2Int(roomBounds.xMax - 1, roomCenter.y), Vector2Int.right);
        // CreateDoorway(new Vector2Int(roomBounds.xMin, roomCenter.y), Vector2Int.left);
    }
    
    public void CalculateRoomBounds()
    {
        if (floorTilemap != null)
        {
            floorTilemap.CompressBounds();
            // Get the bounds of the tilemap (area where tiles exist)
            roomBounds = floorTilemap.cellBounds;
            
            // Calculate the center of the room in tilemap coordinates
            roomCenter = new Vector3Int(
                roomBounds.x + roomBounds.size.x / 2,
                roomBounds.y + roomBounds.size.y / 2,
                0
            );
            
            
            // Debug log to verify
            Debug.Log($"Room {name} bounds: {roomBounds.min} to {roomBounds.max}, center: {roomCenter}");
            Debug.Log($"Size is ${roomBounds.size}");
        }
    }

    public void CreateWalkCorridor(Vector2Int direction)
    {
        
        
    }
    
    public void FindTilemaps()
    {
        // Find the Grid component
        Transform gridTransform = transform.Find("Grid");
        if (gridTransform == null)
        {
            Debug.LogError("Room prefab is missing a Grid component!");
            return;
        }
        
        // Look for Tilemap components
        Tilemap[] tilemaps = gridTransform.GetComponentsInChildren<Tilemap>();
        if (tilemaps.Length == 0)
        {
            Debug.LogError("No Tilemaps found in the room prefab!");
            return;
        }
        
        // If we have multiple tilemaps, try to identify which is which
        if (tilemaps.Length > 1)
        {
            // Look for tilemaps with specific names
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap.name.ToLower().Contains("wall"))
                {
                    wallTilemap = tilemap;
                }
                else if (tilemap.name.ToLower().Contains("floor"))
                {
                    floorTilemap = tilemap;
                }
            }
            
            // If we didn't find tilemaps with specific names, use the first two
            if (floorTilemap == null)
                floorTilemap = tilemaps[0];
                
            if (wallTilemap == null && tilemaps.Length > 1)
                wallTilemap = tilemaps[1];
            else if (wallTilemap == null)
                wallTilemap = tilemaps[0]; // Use the same tilemap for both if only one exists
        }
        else
        {
            // If we only have one tilemap, use it for both floor and walls
            floorTilemap = tilemaps[0];
            wallTilemap = tilemaps[0];
        }
        
        if (floorTilemap == null)
        {
            Debug.LogError("Failed to find a floor tilemap in room: " + name);
        }
    }
    
    // Get the room bounds for external use
    public BoundsInt GetRoomBounds()
    {
        if (roomBounds.size.x == 0 || roomBounds.size.y == 0)
        {
            CalculateRoomBounds();
        }
        return roomBounds;
    }

    // Visualization for debugging in editor
    private void OnDrawGizmosSelected()
    {
        if (floorTilemap != null && Application.isPlaying)
        {
            // Draw room bounds
            Gizmos.color = Color.yellow;
            Vector3 min = floorTilemap.CellToWorld(new Vector3Int(roomBounds.xMin, roomBounds.yMin, 0));
            Vector3 max = floorTilemap.CellToWorld(new Vector3Int(roomBounds.xMax, roomBounds.yMax, 0));
            Gizmos.DrawWireCube((min + max) * 0.5f, max - min);
            
            // Draw room center
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(floorTilemap.CellToWorld(roomCenter) + new Vector3(0.5f, 0.5f, 0), 0.2f);
        }
    }
    
    // Update these methods in the Room class

public Vector3Int CreateDoorway(Vector2Int direction)
{
    if (wallTilemap == null)
    {
        Debug.LogError("Cannot create doorway in room " + name + ": Wall tilemap is missing!");
        return Vector3Int.zero;
    }
    
    if (roomBounds.size.x == 0 || roomBounds.size.y == 0)
    {
        CalculateRoomBounds();
    }
    
    // Calculate the door position based on the direction and room bounds
    Vector3Int doorPosition = CalculateDoorPosition(direction);
    
    // Create the doorway by removing wall tiles
    CreateDoorwayAtPosition(doorPosition, direction);
    
    // For debugging
    Debug.Log($"Created doorway in room {name} at {doorPosition} in direction {direction}");
    
    return doorPosition;
}

private Vector3Int CalculateDoorPosition(Vector2Int direction)
{
    // Calculate position at the middle of the appropriate wall
    Vector3Int doorPos = roomCenter;
    
    if (direction == Vector2Int.up) // North door
    {
        doorPos.y = roomBounds.yMax - 1; // Top wall
    }
    else if (direction == Vector2Int.right) // East door
    {
        doorPos.x = roomBounds.xMax - 1; // Right wall
    }
    else if (direction == Vector2Int.down) // South door
    {
        doorPos.y = roomBounds.yMin; // Bottom wall
    }
    else if (direction == Vector2Int.left) // West door
    {
        doorPos.x = roomBounds.xMin; // Left wall
    }
    
    return doorPos;
}

private void CreateDoorwayAtPosition(Vector3Int centerPos, Vector2Int direction)
{
    // Determine door orientation
    bool isHorizontal = (direction.x == 0); // If direction is up or down, door is horizontal
    
    // Calculate door width (odd number works better for centering)
    int halfWidth = DoorWidth / 2;
    
    // Clear wall tiles to create the doorway
    for (int i = -halfWidth; i <= halfWidth; i++)
    {
        Vector3Int doorTile;
        
        if (isHorizontal)
        {
            // For north/south doors, varies the x-coordinate
            doorTile = new Vector3Int(centerPos.x + i, centerPos.y, 0);
        }
        else
        {
            // For east/west doors, varies the y-coordinate
            doorTile = new Vector3Int(centerPos.x, centerPos.y + i, 0);
        }
        
        // Remove the wall tile
        wallTilemap.SetTile(doorTile, null);
    }
}

// Convert a tilemap position to world position
public Vector3 GetWorldPositionFromTile(Vector3Int tilePos)
{
    if (floorTilemap != null)
    {
        // Convert to world space and add half a cell to get the center
        return floorTilemap.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0);
    }
    
    // Fallback if no tilemap
    return transform.position + new Vector3(tilePos.x, tilePos.y, 0);
}

// Get a sample floor/wall tile for corridors to use
public TileBase GetFloorTile()
{
    if (floorTilemap != null)
    {
        // Try to find any non-null tile in the floor tilemap
        foreach (var pos in floorTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = floorTilemap.GetTile(pos);
            if (tile != null)
            {
                return tile;
            }
        }
    }
    return null;
}

public TileBase GetWallTile()
{
    if (wallTilemap != null)
    {
        // Try to find any non-null tile in the wall tilemap
        foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = wallTilemap.GetTile(pos);
            if (tile != null)
            {
                return tile;
            }
        }
    }
    return null;
}
    
}