using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class SummonService : Singleton<SummonService>
{
    private UnitFactory unitFactory;
    protected override void Awake()
    {
        base.Awake();
        unitFactory = new UnitFactory();
    }
    public IReadOnlyList<UnitRecord> GetUnitsForTier(UnitTier tier)
    {
        
        if (GameDatabase.Instance == null || !GameDatabase.Instance.IsReady)
        {
            return Array.Empty<UnitRecord>();
        }
        return GameDatabase.Instance.GetSummonableUnitsForTier(tier);
        
    }

    public bool IsSelectSummon(CurrencyType type)
    {
        return type == CurrencyType.SelectCommon ||
               type == CurrencyType.SelectUncommon ||
               type == CurrencyType.SelectSpecial ||
               type == CurrencyType.SelectRare;
    }

    public bool TryRandomSummon(CurrencyType costType, int costAmount = 1)
    {
        if (!TryGetRandomTier(costType, out UnitTier tier))
        {
            Debug.LogError($"{costType}은 랜덤 타입 소환이 아닙니다.");
            return false;
        }

        IReadOnlyList<UnitRecord> pool = GetUnitsForTier(tier);

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"{tier}등급 유닛 데이터가 없습니다.");
            return false;
        }

        UnitRecord selected = pool[Random.Range(0, pool.Count)];
        return TrySummonInternal(selected, costType, costAmount);
    }

    public bool TrySelectedSummon(UnitRecord unit, CurrencyType costType, int costAmount = 1)
    {
        if (unit == null)
            return false;

        if (!TryGetSelectTier(costType, out UnitTier expectedTier))
        {
            return false;
        }

        if (unit.tier != expectedTier)
        {
            Debug.LogError($"선택권 등급 불일치: {costType}/{unit.tier}");
            return false;
        }

        return TrySummonInternal(unit, costType, costAmount);

    }

    public IReadOnlyList<UnitRecord> GetSelectableUnits(CurrencyType type)
    {
        if (!TryGetSelectTier(type, out UnitTier tier))
            return Array.Empty<UnitRecord>();

        return GetUnitsForTier(tier);
    }

    private bool TrySummonInternal(UnitRecord unit, CurrencyType costType, int costAmount)
    {
        if (unit == null || GridManager.Instance == null || CurrencyManager.Instance == null)
        {
            return false;
        }
        
        //재화보다 Grid부터 검사
        GridCell emptyCell = GridManager.Instance.GetRandomEmptyCell();

        if (emptyCell == null)
        {
            Debug.Log("빈 GridCell이 없습니다.");
            return false;
        }
        
        // 그 다음 재화 차감
        if (!CurrencyManager.Instance.SpendCurrency(costType, costAmount))
        {
            Debug.Log($"{costType}이 부족합니다.");
            return false;
        }

        if (unitFactory.TryCreate(unit, emptyCell, out UnitEntity createdUnit))
        {
            Debug.Log($"{createdUnit.DisplayName} 소환 완료 [{emptyCell.Coordinate}]");
            return true;
        }
        
        CurrencyManager.Instance.AddCurrency(costType, costAmount);
        
        Debug.LogError("유닛 생성 실패 - 사용한 소환권 환불");
        return false;
    }

    public bool TryDebugSummon(UnitRecord unit)
    {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (unit == null || GridManager.Instance == null)
            return false;

        GridCell emptyCell = GridManager.Instance.GetRandomEmptyCell();

        if (emptyCell == null)
        {
            Debug.LogWarning("[DebugSummon] 빈 Cell이없습니다.");
            return false;
        }

        if (!unitFactory.TryCreate(unit, emptyCell, out UnitEntity createdUnit))
            return false;

        Debug.Log($"[DebugSummon] {createdUnit.DisplayName} 소환 완료");
        return true;

    #else
    Debug.LogWarning("디버그 소환은 Editor 또는 DevelopmentBuild에서만 사용할 수 있습니다.");
    return false;
    #endif
    }

    private bool TryGetRandomTier(CurrencyType type, out UnitTier tier)
    {
        switch (type)
        {
            case CurrencyType.RandomCommon: tier = UnitTier.Common;
                return true;
            case CurrencyType.RandomUncommon: tier = UnitTier.Uncommon;
                return true;
            case CurrencyType.RandomSpecial: tier = UnitTier.Special;
                return true;
            case CurrencyType.RandomRare: tier = UnitTier.Rare;
                return true;
            case CurrencyType.RandomLegendary: tier = UnitTier.Legendary;
                return true;
            default: tier = default;
                return false;
        }
    }
    
    private bool TryGetSelectTier(CurrencyType type, out UnitTier tier)
    {
        switch (type)
        {
            case CurrencyType.SelectCommon: tier = UnitTier.Common;
                return true;
            case CurrencyType.SelectUncommon: tier = UnitTier.Uncommon;
                return true;
            case CurrencyType.SelectSpecial: tier = UnitTier.Special;
                return true;
            case CurrencyType.SelectRare: tier = UnitTier.Rare;
                return true;
            default: tier = default;
                return false;
        }
    }
}
