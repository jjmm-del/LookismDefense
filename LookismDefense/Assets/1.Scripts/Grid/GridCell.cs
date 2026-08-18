using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int Coordinate {get; private set;}
    public GameObject OccupiedUnit { get; private set; }
    
    public bool IsOccupied => OccupiedUnit != null;
    public Vector3 WorldPosition => transform.position;

    public void Initialize(Vector2Int coordinate)
    {
        Coordinate = coordinate;
        //transform.localScale = new Vector3(size, 1f, size);
        name = $"GridCell_{coordinate.x},{coordinate.y}";
    }

    public bool TryPlaceUnit(GameObject unit)
    {
        if (unit == null)
            return false;
        if (IsOccupied)
            return false;
        
        OccupiedUnit = unit;
        unit.transform.position = WorldPosition;

        return true;
    }

    public void RemoveUnit()
    {
        OccupiedUnit = null;
    }

    public GameObject TakeUnit()
    {
        if (!IsOccupied)
            return null;
        
        GameObject unit = OccupiedUnit;
        OccupiedUnit = null;

        return unit;

    }
}
