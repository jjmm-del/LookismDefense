using UnityEngine;
using System;
using System.Collections.Generic;
public class GameDatabase : Singleton<GameDatabase>
{
    private static readonly IReadOnlyList<UnitRecord> EmptyUnits = Array.Empty<UnitRecord>();
    
    //UnitDB
    private readonly Dictionary<string, UnitRecord> units = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<UnitTier, List<UnitRecord>> unitsByTier = new();
    private readonly Dictionary<UnitTier, List<UnitRecord>> summonableUnitsByTier = new();
    
    //CombinationCB
    private readonly Dictionary<string, CombinationRecord> recipes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<CombinationRecord>> recipesByMainUnitId =
        new(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlyList<CombinationRecord> EmptyRecipes = Array.Empty<CombinationRecord>();
    
    public bool IsReady{get; private set;}
    public bool AreRecipesReady{get; private set;}
    public event Action Ready;
    
    //유닛 세팅
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

    //레시피 세팅
    public void SetRecipes(IReadOnlyList<CombinationRecord> records)
    {
        recipes.Clear();
        recipesByMainUnitId.Clear();

        foreach (CombinationRecord record in records)
        {
            if (record == null || !record.enabled)
                continue;

            recipes.Add(record.id, record);

            string mainUnitId = record.MainIngredientId;

            if (string.IsNullOrWhiteSpace(mainUnitId))
                continue;

            if (!recipesByMainUnitId.TryGetValue(mainUnitId, out List<CombinationRecord> unitRecipes))
            {
                unitRecipes = new List<CombinationRecord>();
                recipesByMainUnitId.Add(mainUnitId, unitRecipes);
            }

            unitRecipes.Add(record);
        }

        AreRecipesReady = true;
        Debug.Log($"[GameDatabase] 조합법 {recipes.Count}개 등록 완료");
    }

    public bool TryGetRecipe(string recipeId, out CombinationRecord recipe)
    {
        return recipes.TryGetValue(recipeId, out recipe);
    }

    public IReadOnlyList<CombinationRecord> GetRecipesForMainUnit(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
        {
            return EmptyRecipes;
        }

        return recipesByMainUnitId.TryGetValue(unitId, out List<CombinationRecord> result) ? result : EmptyRecipes;
    }
    
}
