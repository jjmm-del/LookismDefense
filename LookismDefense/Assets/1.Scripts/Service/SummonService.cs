using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class SummonService : Singleton<SummonService>
{
    [Header("Gacha Data")]
    [SerializeField] private List<UnitData> commonUnits;
    [SerializeField] private List<UnitData> uncommonUnits;
    [SerializeField] private List<UnitData> specialUnits;
    [SerializeField] private List<UnitData> rareUnits;
    [SerializeField] private List<UnitData> legendaryUnits;

    private UnitFactory unitFactory;
    protected override void Awake()
    {
        base.Awake();
        unitFactory = new UnitFactory();
    }
    public IReadOnlyList<UnitData> GetUnitsForTier(UnitTier tier)
    {
        // -- 순수 생성 로직 --
        switch (tier)
        {
            case UnitTier.Common: return commonUnits;
            case UnitTier.Uncommon: return uncommonUnits;
            case UnitTier.Special: return specialUnits;
            case UnitTier.Rare: return rareUnits;
            case UnitTier.Legendary: return legendaryUnits;
            default: return Array.Empty<UnitData>();
        }
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

        IReadOnlyList<UnitData> pool = GetUnitsForTier(tier);

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"{tier}등급 유닛 데이터가 없습니다.");
            return false;
        }

        return TrySummonInternal(pool[Random.Range(0, pool.Count)], costType, costAmount);
    }

    public bool TrySelectedSummon(UnitData unit, CurrencyType costType, int costAmount = 1)
    {
        if (unit == null)
            return false;

        if (!TryGetSelectTier(costType, out UnitTier expectedTier))
        {
            return false;
        }

        if (unit.Tier != expectedTier)
        {
            Debug.LogError($"선택권 등급 불일치: {costType}/{unit.Tier}");
            return false;
        }

        return TrySummonInternal(unit, costType, costAmount);

    }

    public IReadOnlyList<UnitData> GetSelectableUnits(CurrencyType type)
    {
        if (!TryGetSelectTier(type, out UnitTier tier))
            return Array.Empty<UnitData>();

        return GetUnitsForTier(tier);
    }

    private bool TrySummonInternal(UnitData unit, CurrencyType costType, int costAmount)
    {
        if (GridManager.Instance == null || CurrencyManager.Instance == null)
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
            Debug.Log($"{createdUnit.Data.EntityName} 소환 완료 [{emptyCell.Coordinate}]");
            return true;
        }
        
        CurrencyManager.Instance.AddCurrency(costType, costAmount);
        
        Debug.LogError("유닛 생성 실패 - 사용한 소환권 환불");
        return false;
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
