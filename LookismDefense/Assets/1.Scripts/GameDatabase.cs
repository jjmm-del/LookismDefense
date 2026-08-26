using UnityEngine;
using System;
using System.Collections.Generic;
public class GameDatabase : Singleton<GameDatabase>
{
    private static readonly IReadOnlyList<UnitRecord> EmptyUnits = Array.Empty<UnitRecord>();
    
    private readonly Dictionary<string, UnitRecord> units = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<UnitTier, List<UnitRecord>> unitsByTier = new();
    private readonly Dictionary<UnitTier, List<UnitRecord>> summonableUnitsByTier = new();
    public bool IsReady{get; private set;}
    public event Action Ready;
    
    public void SetUnits(IReadOnlyList<UnitRecord> records)
    {
        units.Clear();
        unitsByTier.Clear();
        summonableUnitsByTier.Clear();
        
        foreach (UnitRecord record in records)
        {
            if (record == null)
                continue;
            
            units.Add(record.id, record);
            AddToTierIndex(unitsByTier, record);

            if (!string.IsNullOrWhiteSpace(record.prefabKey))
            {
                AddToTierIndex(summonableUnitsByTier, record);
            }
        }

        IsReady = true;
        Debug.Log($"[GameDataBase] 유닛 {units.Count}개 등록 완료");
        Ready?.Invoke();
    }
    
    public bool TryGetUnit(string unitId, out UnitRecord unit)
    {
        return units.TryGetValue(unitId, out unit);
    }

    public IReadOnlyList<UnitRecord> GetUnitsForTier(UnitTier tier)
    {
        return unitsByTier.TryGetValue(tier, out List<UnitRecord> records) ?records : EmptyUnits;
    }

    public IReadOnlyList<UnitRecord> GetSummonableUnitsForTier(UnitTier tier)
    {
        return summonableUnitsByTier.TryGetValue(tier, out List<UnitRecord> records) ?records : EmptyUnits;
    }

    private static void AddToTierIndex(Dictionary<UnitTier, List<UnitRecord>> index, UnitRecord record)
    {
        if (!index.TryGetValue(record.tier, out List<UnitRecord> records))
        {
            records = new List<UnitRecord>();
            index.Add(record.tier, records);
        }

        records.Add(record);
    }
}
