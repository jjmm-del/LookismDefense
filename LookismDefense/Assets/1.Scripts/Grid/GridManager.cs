using UnityEngine;
using System.Collections.Generic;

public class GridManager : Singleton<GridManager>
{
    [Header("Grid Size")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 3;
    
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private Vector2 originOffset;
    
    [Header("Prefab")]
    [SerializeField] private GridCell gridCellPrefab;

    private GridCell[,] grid;

    public int Width => width;
    public int Height => height;
    
    public int CellCount => width * height;

    protected override void Awake()
    {
        base.Awake();
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new GridCell[width, height];

        Vector3 centerOffset = new Vector3(
            (width - 1) * cellSize * 0.5f,
            0f,
            (height - 1) * cellSize * 0.5f
        );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = 
                    transform.position
                    + new Vector3(x*cellSize, 0f, y*cellSize)
                    -centerOffset
                    + new Vector3(originOffset.x, 0.02f, originOffset.y);

                GridCell cell = Instantiate(
                    gridCellPrefab,
                    position,
                    Quaternion.identity,
                    transform
                    );
                cell.Initialize(new Vector2Int(x,y));
                
                grid[x, y] = cell;
            }
        }

    }

    public GridCell GetCell(int x, int y)
    {
        if (!IsValidCoordinate(x, y))
            return null;
        return grid[x, y];
    }

    public GridCell GetCell(Vector2Int coordinate)
    {
        return GetCell(coordinate.x, coordinate.y);
    }

    public bool IsValidCoordinate(int x, int y)
    {
        return x>= 0 &&
               x<width&&
               y >= 0 &&
               y<height;
    }

    public List<GridCell> GetEmptyCells()
    {
        List<GridCell> emptyCells = new();

        foreach (GridCell cell in grid)
        {
            if(!cell.IsOccupied)
                emptyCells.Add(cell);
        }

        return emptyCells;
    }

    public GridCell GetRandomEmptyCell()
    {
        List<GridCell> emptyCells = GetEmptyCells();

        if (emptyCells.Count == 0)
            return null;
        int index = Random.Range(0, emptyCells.Count);
        
        return emptyCells[index];
    }

    public GridCell GetFirstEmptyCell()
    {
        foreach (GridCell cell in grid)
        {
            if (!cell.IsOccupied)
                return cell;
        }

        return null;
    }

    public int GetOccupiedCount()
    {
        int count = 0;
        foreach (GridCell cell in grid)
        {
            if (cell.IsOccupied)
                count++;
        }

        return count;
    }

    public int GetEmptyCount()
    {
        return CellCount - GetOccupiedCount();
    }




}
