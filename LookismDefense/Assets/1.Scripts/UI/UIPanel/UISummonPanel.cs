using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class UISummonPanel : UIPanel
{

    [SerializeField] private UnitSelectorPopup selectorPopup;
    private enum Buttons
    {
        RandomCommon,
        RandomUncommon,
        RandomSpecial,
        RandomRare,
        RandomLegendary,
        
        SelectCommon,
        SelectUncommon,
        SelectSpecial,
        SelectRare,
        
        CloseButton
    }
    private readonly Dictionary<Buttons, CurrencyType> summonMap = new()
    {
        { Buttons.RandomCommon, CurrencyType.RandomCommon },
        { Buttons.RandomUncommon, CurrencyType.RandomUncommon },
        { Buttons.RandomSpecial, CurrencyType.RandomSpecial },
        { Buttons.RandomRare, CurrencyType.RandomRare },
        { Buttons.RandomLegendary, CurrencyType.RandomLegendary },

        { Buttons.SelectCommon, CurrencyType.SelectCommon },
        { Buttons.SelectUncommon, CurrencyType.SelectUncommon },
        { Buttons.SelectSpecial, CurrencyType.SelectSpecial },
        { Buttons.SelectRare, CurrencyType.SelectRare },
    };
    
    private void Awake()
    {
        Bind<Button>(typeof(Buttons));
        BindEvents();
    }

    private void BindEvents()
    {
        foreach (var pair in summonMap)
        {
            CurrencyType type = pair.Value;
            GetButton((int)pair.Key).onClick.AddListener(()=>HandleSummon(type));
        }
        GetButton((int)Buttons.CloseButton).onClick.AddListener(Close);
    }

    private void HandleSummon(CurrencyType type)
    {
        if (SummonService.Instance == null)
            return;

        if (!SummonService.Instance.IsSelectSummon(type))
        {
            SummonService.Instance.TryRandomSummon(type);
            return;
        }

        OpenSelector(type);


    }

    private void OpenSelector(CurrencyType type)
    {
        IReadOnlyList<UnitData> units = SummonService.Instance.GetSelectableUnits(type);

        selectorPopup.SetData(GetSelectorTitle(type), units, unit => HandleSelectedSummon(unit, type));
        UIManager.Instance.OpenPopup(selectorPopup);
    }

    private void HandleSelectedSummon(UnitData unit, CurrencyType type)
    {
        SummonService.Instance.TrySelectedSummon(unit, type);
    }
    
    //helper
    private string GetSelectorTitle(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.SelectCommon: return "흔함 선택";
            case CurrencyType.SelectUncommon: return "안흔함 선택";
            case CurrencyType.SelectSpecial: return "특별함 선택";
            case CurrencyType.SelectRare: return "희귀함 선택";
            default: return "유닛 선택";
        }
    }
    private void OnDestroy()
    {
        foreach (var pair in summonMap)
        {
            GetButton((int)pair.Key).onClick.RemoveAllListeners();
        }
        GetButton((int)Buttons.CloseButton).onClick.RemoveAllListeners();
    }
        
}
