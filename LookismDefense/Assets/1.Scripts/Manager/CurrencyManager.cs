using UnityEngine;
using System;
using System.Collections.Generic;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private readonly Dictionary<CurrencyType, int> currencies = new();
    public event Action<CurrencyType, int> OnCurrencyChanged;

    protected override void Awake()
    {
        base.Awake();

        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            currencies[type] = 0;
        }
    }
    
    //재화량 가져오기
    public int GetCurrency(CurrencyType type)
    {
        return currencies.TryGetValue(type, out int value) ? value : 0;
    }
    
    //재화 획득
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0)
            return;
        
        currencies[type] += amount;
        
        OnCurrencyChanged?.Invoke(type, currencies[type]);
    }
    
    //재화 사용
    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0)
            return false;
        
        if(GetCurrency(type) < amount) 
            return false;
        
        currencies[type] -= amount;

        OnCurrencyChanged?.Invoke(type, currencies[type]);
        Debug.Log($"재화 사용: {type} -{amount}");
        return true;
    }

    public bool HasCurrency(CurrencyType type, int amount)
    {
        return GetCurrency(type) >= amount;
    }
}
