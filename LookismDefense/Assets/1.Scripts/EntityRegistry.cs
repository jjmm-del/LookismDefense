using UnityEngine;
using System;
using System.Collections.Generic;

public class EntityRegistry : Singleton<EntityRegistry>
{
    private readonly List<UnitEntity> playerUnits = new();
    private readonly List<EnemyEntity> activeEnemies = new();
    
    public IReadOnlyList<UnitEntity> PlayerUnits => playerUnits;
    public IReadOnlyList<EnemyEntity> ActiveEnemies => activeEnemies;
    
    public int PlayerUnitCount => playerUnits.Count;
    public int EnemyCount => activeEnemies.Count;

    public event Action<UnitEntity> OnUnitRegistered;
    public event Action<UnitEntity> OnUnitUnregistered;

    public event Action<int> OnEnemyCountChanged;
    
    //unit
    public void RegisterUnit(UnitEntity unit)
    {
        if (unit == null)
            return;

        if (playerUnits.Contains(unit))
            return;

        playerUnits.Add(unit);
        
        OnUnitRegistered?.Invoke(unit);
    }

    public void UnregisterUnit(UnitEntity unit)
    {
        if (unit == null)
            return;
        
        if (!playerUnits.Remove(unit))
            return;
        
        OnUnitUnregistered?.Invoke(unit);
    }

    public bool ContainsUnit(UnitEntity unit)
    {
        return unit != null && playerUnits.Contains(unit);
    }
    
    //Enemy
    public void RegisterEnemy(EnemyEntity enemy)
    {
        if (enemy == null)
            return;

        if (activeEnemies.Contains(enemy))
            return;
        
        activeEnemies.Add(enemy);
        
        OnEnemyCountChanged?.Invoke(activeEnemies.Count);
    }

    public void UnregisterEnemy(EnemyEntity enemy)
    {
        if (enemy == null)
            return;

        if (!activeEnemies.Remove(enemy))
            return;

        OnEnemyCountChanged?.Invoke(activeEnemies.Count);
    }
}
