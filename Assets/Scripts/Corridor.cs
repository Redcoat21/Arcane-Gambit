using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Corridor : MonoBehaviour
{
    [Header("Corridor Properties")]
    [SerializeField] private int corridorWidth = 2;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector2Int direction;
    
    private Tilemap floorTilemap;
    private Tilemap wallTilemap;
    
    // Keep track of placed floor tiles to avoid overlapping walls
    private HashSet<Vector3Int> floorPositions = new HashSet<Vector3Int>();
    
    // Track existing floor tiles from rooms to avoid overlapping
    private HashSet<Vector3Int> existingFloorTiles = new HashSet<Vector3Int>();
    
    public void Initialize(Vector3 start, Vector3 end, Vector2Int dir, TileBase wallTile, TileBase floorTile)
    {
        startPoint = start;
        endPoint = end;
        direction = dir;

        this.floorTile = floorTile;
        this.wallTile = wallTile;
        
        // Find existing floor tiles in the scene to avoid overlapping
        FindExistingTiles();
        
        CreateTilemaps();
        BuildCorridor();
    }
    
    private void FindExistingTiles()
    {
        // Find all Room objects in the scene
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        
        foreach (Room room in rooms)
        {
            // Get the room bounds
            BoundsInt bounds = room.GetRoomBounds();
            
            // Convert room's tilemap positions to world positions and then to our corridor's tilemap positions
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                // Get the world position of the tile
                Vector3 worldPos = room.GetWorldPositionFromTile(pos);
                
                // Convert to our corridor's local position
                Vector3Int localPos = Vector3Int.FloorToInt(worldPos);
                
                // Add to existing tiles set
                existingFloorTiles.Add(localPos);
            }
        }
        
        // Find all other corridors' floor tiles
        Corridor[] corridors = FindObjectsByType<Corridor>(FindObjectsSortMode.None);
        
        foreach (Corridor corridor in corridors)
        {
            if (corridor != this && corridor.floorTilemap != null)
            {
                foreach (Vector3Int pos in corridor.floorTilemap.cellBounds.allPositionsWithin)
                {
                    if (corridor.floorTilemap.HasTile(pos))
                    {
                        // Convert to world position
                        Vector3 worldPos = corridor.floorTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
                        
                        // Convert to our corridor's local position
                        Vector3Int localPos = Vector3Int.FloorToInt(worldPos);
                        
                        // Add to existing tiles set
                        existingFloorTiles.Add(localPos);
                    }
                }
            }
        }
    }
    
    private void CreateTilemaps()
    {
        // Create Grid for the corridor
        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.parent = transform;
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        
        // Create floor tilemap
        GameObject floorObject = new GameObject("FloorTilemap");
        floorObject.transform.parent = gridObject.transform;
        floorTilemap = floorObject.AddComponent<Tilemap>();
        TilemapRenderer floorRenderer = floorObject.AddComponent<TilemapRenderer>();
        floorRenderer.sortingOrder = 0;
        
        // Create wall tilemap
        GameObject wallObject = new GameObject("WallTilemap");
        wallObject.transform.parent = gridObject.transform;
        wallTilemap = wallObject.AddComponent<Tilemap>();
        TilemapRenderer wallRenderer = wallObject.AddComponent<TilemapRenderer>();
        wallRenderer.sortingOrder = 1; // Make sure walls render above floor
    }
    
    private void BuildCorridor()
    {
        if (floorTile == null || wallTile == null)
        {
            Debug.LogError("Cannot build corridor: Missing tiles");
            return;
        }
        
        // Convert world positions to cell positions
        Vector3Int startCell = Vector3Int.FloorToInt(startPoint);
        Vector3Int endCell = Vector3Int.FloorToInt(endPoint);
        
        // Determine corridor path
        List<Vector3Int> path = GenerateCorridorPath(startCell, endCell);
        
        // Clear floor positions before building
        floorPositions.Clear();
        
        // First pass: build all floor tiles
        foreach (Vector3Int cellPos in path)
        {
            // Create corridor floor
            CreateCorridorFloor(cellPos);
        }
        
        // Second pass: place wall tiles around floor tiles
        // Use a temporary copy of floor positions to avoid modification during iteration
        HashSet<Vector3Int> floorPositionsCopy = new HashSet<Vector3Int>(floorPositions);
        foreach (Vector3Int floorPos in floorPositionsCopy)
        {
            PlaceWallsAroundFloor(floorPos);
        }
        
        // Ensure tilemaps are refreshed
        floorTilemap.RefreshAllTiles();
        wallTilemap.RefreshAllTiles();
    }
    
    private List<Vector3Int> GenerateCorridorPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        
        // Determine if rooms are aligned on X or Y axis
        bool alignedX = start.y == end.y;
        bool alignedY = start.x == end.x;
        
        if (alignedX)
        {
            // Straight horizontal corridor
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            
            for (int x = minX; x <= maxX; x++)
            {
                path.Add(new Vector3Int(x, start.y, 0));
            }
        }
        else if (alignedY)
        {
            // Straight vertical corridor
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            
            for (int y = minY; y <= maxY; y++)
            {
                path.Add(new Vector3Int(start.x, y, 0));
            }
        }
        else
        {
            // L-shaped corridor for rooms not aligned
            
            // Use the direction to determine corridor shape
            if (Mathf.Abs(direction.x) > 0)
            {
                // If moving horizontally, go horizontal first then vertical
                BuildLShapedPath(path, start, end, true);
            }
            else
            {
                // If moving vertically, go vertical first then horizontal
                BuildLShapedPath(path, start, end, false);
            }
        }
        
        return path;
    }
    
    private void BuildLShapedPath(List<Vector3Int> path, Vector3Int start, Vector3Int end, bool horizontalFirst)
    {
        Vector3Int corner;
        
        if (horizontalFirst)
        {
            // Create a corner tile at the point where we turn
            corner = new Vector3Int(end.x, start.y, 0);
            
            // First horizontal segment
            int minX = Mathf.Min(start.x, corner.x);
            int maxX = Mathf.Max(start.x, corner.x);
            
            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int pos = new Vector3Int(x, start.y, 0);
                if (!path.Contains(pos))
                {
                    path.Add(pos);
                }
            }
            
            // Then vertical segment
            int minY = Mathf.Min(corner.y, end.y);
            int maxY = Mathf.Max(corner.y, end.y);
            
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int pos = new Vector3Int(corner.x, y, 0);
                if (!path.Contains(pos))
                {
                    path.Add(pos);
                }
            }
        }
        else
        {
            // Create a corner tile at the point where we turn
            corner = new Vector3Int(start.x, end.y, 0);
            
            // First vertical segment
            int minY = Mathf.Min(start.y, corner.y);
            int maxY = Mathf.Max(start.y, corner.y);
            
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int pos = new Vector3Int(start.x, y, 0);
                if (!path.Contains(pos))
                {
                    path.Add(pos);
                }
            }
            
            // Then horizontal segment
            int minX = Mathf.Min(corner.x, end.x);
            int maxX = Mathf.Max(corner.x, end.x);
            
            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int pos = new Vector3Int(x, corner.y, 0);
                if (!path.Contains(pos))
                {
                    path.Add(pos);
                }
            }
        }
    }
    
    private void CreateCorridorFloor(Vector3Int center)
    {
        // Place floor tiles
        int halfWidth = corridorWidth / 2;
        
        // Adjust halfWidth for even vs odd corridor width
        int negOffset = halfWidth;
        int posOffset = corridorWidth % 2 == 0 ? halfWidth - 1 : halfWidth;
        
        // Check if this is a corner point
        bool isCorner = IsCornerPoint(center);
        
        // If this is a corner point and corridor width > 1, be careful with placement
        int xNegOffset = negOffset;
        int xPosOffset = posOffset;
        int yNegOffset = negOffset;
        int yPosOffset = posOffset;
        
        // For corridor width > 1, reduce the width at corners to prevent overlaps
        if (isCorner && corridorWidth > 1)
        {
            xNegOffset = yNegOffset = 0;
            xPosOffset = yPosOffset = 0;
        }
        
        for (int offsetX = -xNegOffset; offsetX <= xPosOffset; offsetX++)
        {
            for (int offsetY = -yNegOffset; offsetY <= yPosOffset; offsetY++)
            {
                Vector3Int floorPos = new Vector3Int(
                    center.x + offsetX,
                    center.y + offsetY,
                    0
                );
                
                // Place floor tile if not already placed and not overlapping with existing tiles
                if (!floorPositions.Contains(floorPos) && !existingFloorTiles.Contains(floorPos))
                {
                    floorTilemap.SetTile(floorPos, floorTile);
                    floorPositions.Add(floorPos);
                }
            }
        }
    }
    
    private void PlaceWallsAroundFloor(Vector3Int floorPos)
    {
        // Check all 8 neighboring cells
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the center tile (which is a floor)
                if (x == 0 && y == 0) continue;
                
                Vector3Int neighborPos = new Vector3Int(
                    floorPos.x + x,
                    floorPos.y + y,
                    0
                );
                
                // Only place a wall if there isn't already a floor tile there
                // and if it doesn't overlap with existing room tiles
                if (!floorPositions.Contains(neighborPos) && !existingFloorTiles.Contains(neighborPos))
                {
                    // Check if there's already a wall there
                    TileBase existingTile = wallTilemap.GetTile(neighborPos);
                    if (existingTile == null)
                    {
                        wallTilemap.SetTile(neighborPos, wallTile);
                    }
                }
            }
        }
    }
    
    // Helper method to check if this position is at a corridor corner
    private bool IsCornerPoint(Vector3Int position)
    {
        // Convert to world position for more reliable checking
        Vector3 worldPos = new Vector3(position.x, position.y, 0);
        
        // Calculate the distance to start and end
        float distToStart = Vector3.Distance(worldPos, startPoint);
        float distToEnd = Vector3.Distance(worldPos, endPoint);
        
        // Calculate expected total distance if going from start to end through this point
        float directDist = Vector3.Distance(startPoint, endPoint);
        
        // If the point lies on a straight line between start and end, the sum of distances
        // would be roughly equal to the direct distance
        float combinedDist = distToStart + distToEnd;
        
        // If this position is not on a direct line, it's likely a corner
        return Mathf.Abs(combinedDist - directDist) > 0.1f;
    }
    
    // Helper to check if a point is a corner in the corridor path
    private bool IsCornerPoint(List<Vector3Int> path, Vector3Int point)
    {
        if (!path.Contains(point)) return false;
        
        int orthogonalNeighbors = 0;
        
        // Check horizontal neighbors
        if (path.Contains(new Vector3Int(point.x - 1, point.y, 0)))
            orthogonalNeighbors++;
        if (path.Contains(new Vector3Int(point.x + 1, point.y, 0)))
            orthogonalNeighbors++;
            
        // Check vertical neighbors
        if (path.Contains(new Vector3Int(point.x, point.y - 1, 0)))
            orthogonalNeighbors++;
        if (path.Contains(new Vector3Int(point.x, point.y + 1, 0)))
            orthogonalNeighbors++;
            
        // If point has neighbors in more than one direction, it's a corner
        return orthogonalNeighbors > 1;
    }
}