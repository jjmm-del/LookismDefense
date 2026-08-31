using UnityEngine;
using System;
public sealed class UnitFactory
{
    public bool TryCreate(UnitRecord data, GridCell cell, out UnitEntity createdUnit)
    {
        createdUnit = null;
        if (data == null || !data.enabled)
        {
            Debug.LogError("생성할 UnitRecord가 없거나 비활성 상태입니다.");
            return false;
        }

        if (!UnitAssetProvider.TryLoadPrefab(data, out GameObject prefab))
        {
            return false;
        }

        return TryCreateInternal(prefab, cell, entity => entity.Initialize(data), data.DisplayName, out createdUnit);
    }
    private bool TryCreateInternal(GameObject prefab, GridCell cell, Action<UnitEntity> initialize, string displayName,
        out UnitEntity createdUnit)

    {
        createdUnit = null;
        
        if (cell == null || cell.IsOccupied)
        {
            Debug.Log("유닛을 배치할 수 없는 GridCell입니다.");
            return false;
        }

        GameObject unitObject = UnityEngine.Object.Instantiate(prefab, cell.WorldPosition, Quaternion.identity);

        if (!cell.TryPlaceUnit(unitObject))
        {
            UnityEngine.Object.Destroy(unitObject);
            return false;
        }
        
        UnitEntity entity = unitObject.GetComponent<UnitEntity>();
        
        UnitAIController ai = unitObject.GetComponent<UnitAIController>();

        if (entity == null || ai == null)
        {
            Debug.LogError($"{displayName} Prefab에 UnitEntity 또는 UnitController가 없습니다.");
            
            cell.RemoveUnit();
            UnityEngine.Object.Destroy(unitObject);

            return false;
        }

        initialize(entity);
        ai.SetHomeCell(cell);

        createdUnit = entity;
        
        
        return true;
    }
}
