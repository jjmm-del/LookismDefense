using UnityEngine;
using System.Collections.Generic;
public class GameDatabase : Singleton<GameDatabase>
{
    private readonly Dictionary<string, UnitRecord> units = new();

    public void SetUnits(IReadOnlyList<UnitRecord> records)
    {
        units.Clear();

        foreach (UnitRecord record in records)
        {
            units.Add(record.id, record);
        }
        Debug.Log($"[GameDataBase] 유닛 {units.Count}개 등록 완료");
    }

    public bool TryGetUnit(string unitId, out UnitRecord unit)
    {
        return units.TryGetValue(unitId, out unit);
    }
}
