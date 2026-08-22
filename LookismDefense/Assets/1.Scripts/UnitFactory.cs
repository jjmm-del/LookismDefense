using UnityEngine;

public sealed class UnitFactory
{
    public bool TryCreate(UnitData data, GridCell cell, out UnitEntity createdUnit)
    {
        createdUnit = null;

        if (data == null || data.Prefab == null)
        {
            Debug.LogError("생성할 UnitData 또는 Prefab이 없습니다.");
            return false;
        }

        if (cell == null || cell.IsOccupied)
        {
            Debug.Log("유닛을 배치할 수 없는 GridCell입니다.");
            return false;
        }

        GameObject unitObject = Object.Instantiate(data.Prefab, cell.WorldPosition, Quaternion.identity);

        if (!cell.TryPlaceUnit(unitObject))
        {
            Object.Destroy(unitObject);
            return false;
        }
        
        UnitEntity entity = unitObject.GetComponent<UnitEntity>();
        
        UnitAIController ai = unitObject.GetComponent<UnitAIController>();

        if (entity == null || ai == null)
        {
            Debug.LogError($"{data.name} Prefab에 UnitEntity 또는 UnitController가 없습니다.");
            
            cell.RemoveUnit();
            Object.Destroy(unitObject);

            return false;
        }

        entity.Initialize(data);
        ai.SetHomeCell(cell);

        createdUnit = entity;
        
        
        return true;
    }
}
