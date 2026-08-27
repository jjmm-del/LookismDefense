using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class UnitInfoPanelUI: UIPanel
{
    [Header("Unit Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Image portraitImage;
    
    [Header("Tooltip")]
    [SerializeField] private TooltipTrigger damageTooltip;
    
    [Header("Abilities")]
    [SerializeField] private Transform abilityIconContainer;    //아이콘 콘테이너
    [SerializeField] private GameObject abilityIconPrefab;      //아이콘 프리펩
    
    [Header("Recipes")]
    [SerializeField] private Transform recipeContents; // ScrollView의 Content
    [SerializeField] private GameObject recipeButtonPrefab; //위에서 만든 버튼 프리팹

    [Header("Sell")]
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellPriceText;
    
    [Header("Display Settings")]
    [SerializeField] private UnitTierDisplaySettings tierDisplaySettings;
    
    private UnitEntity currentTargetUnit;
    

    private void Awake()
    {
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }
        
    }
    public void SetData(UnitEntity unit)
    {
        bool hasAnyData = unit != null && (unit.RuntimeData != null || unit.Data != null);
        
        if (!hasAnyData)
        {
            Close();
            return;
        }
        
        currentTargetUnit = unit; // 클릭한 유닛 기억해두기

        UpdateBasicInfo(unit);
        UpdatePortrait(unit);
        if (unit.Data != null)
        {
            UpdateAbilities(unit.Data);
            
        }
        else
        {
            ClearChildren(abilityIconContainer);
        }
        UpdateRecipeList(unit);
        UpdateSellUI(unit.Tier);
    }

    private void UpdateBasicInfo(UnitEntity unit)
    {
        if (nameText != null)
        {
            nameText.text = GetUnitDisplayName(unit);
        }

        float baseDamage = unit.AttackDamage;
        float finalDamage = baseDamage;

        if (UpgradeManager.Instance != null)
        {
            finalDamage = UpgradeManager.Instance.GetFinalDamage(baseDamage, unit.Tier);
        }

        if (damageText != null)
        {
            damageText.text = $"DMG: {finalDamage:F0}";
        }

        if (damageTooltip != null)
        {
            float bonusDamage = finalDamage - baseDamage;
            damageTooltip.content = $"기본공격력 : {baseDamage:F0}\n" +
                                    $"업그레이드 추가 : {bonusDamage:F0}";
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"ASP:{unit.AttackSpeed}";
        }
        
    }

    private void UpdatePortrait(UnitEntity unit)
    {
        if (portraitImage == null)
            return;

        Sprite portrait = null;
        if (unit.RuntimeData != null)
        {
            portrait = UnitAssetProvider.LoadPortrait(unit.RuntimeData);
        }

        if (portrait == null && unit.Data != null)
        {
            portrait = unit.Data.PortraitIcon;
        }
        
        bool hasPortrait = portrait != null;
        portraitImage.gameObject.SetActive(hasPortrait);
        portraitImage.sprite = portrait;


    }
    
    private void UpdateAbilities(UnitData data)
    {
        ClearChildren(abilityIconContainer);
        
        if (data.Abilities == null)
            return;
        
        foreach (AbilityData ability in data.Abilities)
        {
            if (ability == null || ability.abilityIcon == null)
            {
                continue;
            }
            
            GameObject iconObject = Instantiate(abilityIconPrefab, abilityIconContainer);
            Image iconImage = iconObject.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.sprite = ability.abilityIcon;
            }

            TooltipTrigger tooltip = iconObject.GetComponent<TooltipTrigger>();
            if (tooltip != null)
            {
                tooltip.content = $"<color=yellow><b>" +
                                  $"{ability.abilityName}" +
                                  $"</b></color>\n" +
                                  $"확률: {ability.chance}%";
            }
            
        }
    }
    public void UpdateRecipeList(UnitEntity unit)
    {
        ClearChildren(recipeContents);

        if (unit == null || CombinationManager.Instance == null)
        {
            return;
        }
        
        IReadOnlyList<CombinationRecord> recipes = CombinationManager.Instance.GetRecipesForUnit(unit);
        
        // 버튼 생성하기
        foreach (CombinationRecord recipe in recipes)
        {
            GameObject buttonObject = Instantiate(recipeButtonPrefab, recipeContents);
            
            RecipeButtonUI buttonUI = buttonObject.GetComponent<RecipeButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(recipe);
            }
        }
    }
    
    private void UpdateSellUI(UnitTier tier)
    {
        if (sellButton == null)
            return;

        if (UnitSellService.Instance == null)
        {
            sellButton.gameObject.SetActive(false);
            return;
        }

        List<SellRewardSettings.RewardItem> rewards = UnitSellService.Instance.GetSellRewardInfo(tier);
        
        bool canSell = rewards != null && rewards.Count > 0;
        
        sellButton.gameObject.SetActive(canSell);
        if (!canSell)
        {
            if (sellPriceText != null)
            {
                sellPriceText.text = string.Empty;
            }

            return;
        }
        UpdateSellText(rewards);
    }

    private void UpdateSellText(List<SellRewardSettings.RewardItem> rewards)
    {
        if (sellPriceText == null || rewards == null || rewards.Count == 0)
        {
            return;
        }

        string firstCurrencyName = GetCurrencyName(rewards[0].rewardType);
        if (rewards.Count == 1)
        {
            sellPriceText.text = $"판매 ({rewards[0].chance}% {firstCurrencyName})";
            return;
        }

        sellPriceText.text = $"판매 ({firstCurrencyName}외 {rewards.Count - 1}종)";

    }

    private string GetUnitDisplayName(UnitEntity unit)
    {
        UnitTier tier = unit.Tier;
        string tierName = unit.Tier.ToString();
        Color tierColor = Color.white;

        if (tierDisplaySettings != null)
        {
            tierName = tierDisplaySettings.GetDisplayName(tier);
            tierColor = tierDisplaySettings.GetColor(tier);
        }

        string colorHex = ColorUtility.ToHtmlStringRGB(tierColor);
         
        return $"<color=#{colorHex}>" +
               $"{unit.DisplayName} - {tierName}" +
               $"</color>";
    }

    private void OnSellButtonClicked()
    {
        if (currentTargetUnit == null)
            return;

        bool sold = UnitSellService.Instance?.SellUnit(currentTargetUnit) ?? false;

        if (sold)
        {
            Close();
        }
    }
    public override void Hide()
    {
        currentTargetUnit = null;
        ClearChildren(abilityIconContainer);
        ClearChildren(recipeContents);
        base.Hide();
    }
    
    private void ClearChildren(Transform container)
    {
        if (container == null)
            return;
        
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
    private string GetCurrencyName(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Gold: return "골드";
            case CurrencyType.RandomCommon: return "랜덤흔함";
            case CurrencyType.RandomUncommon: return "랜덤안흔함";
            case CurrencyType.RandomSpecial: return "랜덤특별함";
            case CurrencyType.RandomRare: return "랜덤희귀함";
            case CurrencyType.RandomLegendary: return "랜덤전설";
            case CurrencyType.SelectCommon: return "흔함선택";
            case CurrencyType.SelectUncommon: return "안흔함선택";
            case CurrencyType.SelectSpecial: return "특별함선택";
            case CurrencyType.SelectRare: return "희귀함선택";
            default: return type.ToString();
        }
    }

    private void OnDestroy()
    {
        if (sellButton != null)
        {
            sellButton.onClick.RemoveListener(OnSellButtonClicked);
        }
    }
}
