using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class UISummonPopup : UIPopup
{
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
    // [Header("Random Summon")] 
    // [SerializeField] private Button randomCommonButton;
    // [SerializeField] private Button randomUncommonButton;
    // [SerializeField] private Button randomSpecialButton;
    // [SerializeField] private Button randomRareButton;
    // [SerializeField] private Button randomLegendaryButton;
    //
    // [Header("Select Summon")]
    // [SerializeField] private Button selectCommonButton;
    // [SerializeField] private Button selectUncommonButton;
    // [SerializeField] private Button selectSpecialButton;
    // [SerializeField] private Button selectRareButton;
    //
    // [Header("Popup")]
    // [SerializeField] private Button closeButton;
    
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
            GetButton((int)pair.Key).onClick.AddListener(()=>UnitSpawnManager.Instance.TrySummon(type));
        }
        GetButton((int)Buttons.CloseButton).onClick.AddListener(Close);
    }

    // private void TrySummon(CurrencyType type)
    // {
    //     if (UnitSpawnManager.Instance == null)
    //     {
    //         Debug.LogError("UnitSpawnManager가 존재하지 않습니다.");
    //         return;
    //     }
    //     UnitSpawnManager.Instance.TrySummon(type);
    // }

    private void OnDestroy()
    {
        foreach (var pair in summonMap)
        {
            GetButton((int)pair.Key).onClick.RemoveAllListeners();
        }
        GetButton((int)Buttons.CloseButton).onClick.RemoveAllListeners();
    }
        
}
