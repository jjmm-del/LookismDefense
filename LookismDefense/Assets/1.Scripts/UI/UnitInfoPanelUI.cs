using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class UnitInfoPanelUI: MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Image portraitImage;
    
    [Header("Abilities")]
    [SerializeField] private Transform abilityIconContainer;    //아이콘 콘테이너
    [SerializeField] private GameObject abilityIconPrefab;      //아이콘 프리펩
    
    [Header("Recipes")]
    [SerializeField] private Transform recipeContents; // ScrollView의 Content
    [SerializeField] private GameObject recipeButtonPrefab; //위에서 만든 버튼 프리팹

    [Header("TierSettings")]
    [SerializeField] private List<TierDisplayInfo> tierSettings = new List<TierDisplayInfo>()
    {
        new TierDisplayInfo { tier = UnitTier.Common, displayName = "흔함", textColor = Color.white },
        new TierDisplayInfo { tier = UnitTier.Uncommon, displayName = "안흔함", textColor = Color.green },
        new TierDisplayInfo { tier = UnitTier.Special, displayName = "특별함", textColor = Color.dodgerBlue }, // 파란색
        new TierDisplayInfo { tier = UnitTier.Rare, displayName = "희귀함", textColor = Color.purple },
        new TierDisplayInfo { tier = UnitTier.Legendary, displayName = "전설적인", textColor = Color.orange }, // 주황색
        new TierDisplayInfo { tier = UnitTier.Hidden, displayName = "히든조합", textColor = Color.crimson}, // 보라색
        new TierDisplayInfo { tier = UnitTier.Changed, displayName = "변화된", textColor = Color.deepPink},
        new TierDisplayInfo { tier = UnitTier.Transcendence, displayName = "초월함", textColor = Color.aquamarine},
        new TierDisplayInfo { tier = UnitTier.Immortal, displayName = "불멸의", textColor = Color.plum}, // 진한 빨간색
        new TierDisplayInfo { tier = UnitTier.Eternal, displayName = "영원함", textColor = Color.lightGoldenRod}, // 핑크색
        new TierDisplayInfo { tier = UnitTier.Limited, displayName = "제한됨", textColor = Color.firebrick }
    };
	
    private Dictionary<UnitTier, TierDisplayInfo> tierMap = new Dictionary<UnitTier, TierDisplayInfo>();
    
    private void Awake()
    {
        foreach (var setting in tierSettings)
        {
            if (!tierMap.ContainsKey(setting.tier))
            {
                tierMap.Add(setting.tier, setting);
            }
        }
    }

    public void ShowInfo(UnitEntity unit)
    {
        gameObject.SetActive(true);
        UnitData data = unit.Data;
        nameText.text = SetUnitName(data); //이름 세팅
        float baseDamage = data.AttackDamage;
        float finalDamage = UpgradeManager.Instance.GetFinalDamage(baseDamage, data.Tier);
        damageText.text = $"DMG:{finalDamage:F0}";

        TooltipTrigger dmgTooltip = damageText.gameObject.GetComponent<TooltipTrigger>();
        if (dmgTooltip == null) dmgTooltip = damageText.gameObject.AddComponent<TooltipTrigger>();
        dmgTooltip.content = $"기본 공격력:{baseDamage}\n업그레이드 추가: +{finalDamage - baseDamage:F0}";

        attackSpeedText.text = $"ASP:{data.AttackSpeed}";
        if (portraitImage != null)
        {
            if (data.PortraitIcon != null)
            {
                portraitImage.sprite = data.PortraitIcon;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {

                // 아직 초상화가 안 들어간 유닛을 위해 임시로 꺼두기 
                portraitImage.gameObject.SetActive(false);
            }
        }

        UpdateAbilities(data);
        UpdateRecipeList(data);
    
        

    }
    public void ShowEnemyInfo(EnemyData data, float currentHp)
    {
        gameObject.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = "Enemy";
        attackSpeedText.text = $"HP:{currentHp}/{data.MaxHealth}";
        foreach (Transform child in abilityIconContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in recipeContents)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateAbilities(UnitData data)
    {
        foreach (Transform child in abilityIconContainer)
        {
            Destroy(child.gameObject);
        }

        if (data.Abilities == null)
        {
            return;
        }
        foreach (AbilityData ability in data.Abilities)
        {
            if (ability.abilityIcon != null)
            {
                GameObject iconObj = Instantiate(abilityIconPrefab, abilityIconContainer);
                iconObj.GetComponent<Image>().sprite = ability.abilityIcon;

                TooltipTrigger tooltip = iconObj.AddComponent<TooltipTrigger>();
                tooltip.content = $"<color=yellow><b>{ability.abilityName}</b></color>\n확률: {ability.chance}%";
            }
        }
    }
    public void UpdateRecipeList(UnitData unit)
    {
        foreach (Transform child in recipeContents)
        {
            Destroy(child.gameObject);
        }
        // 레시피 가져오기
        List<CombinationRecipe> recipes = CombinationManager.Instance.GetRecipesForUnit(unit);
        
        // 버튼 생성하기
        foreach (CombinationRecipe recipe in recipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeContents);
            buttonObj.GetComponent<RecipeButtonUI>().Setup(recipe);
        }
    }
    private string SetUnitName(UnitData unit)
    {
        string tierName = unit.Tier.ToString();
        string colorHex = "FFFFFF"; //기본 색상

        if (tierMap.TryGetValue(unit.Tier, out TierDisplayInfo info))
        {
            tierName = info.displayName;
            colorHex = ColorUtility.ToHtmlStringRGB(info.textColor);
        }

        string titleStr = string.IsNullOrEmpty(unit.Title) ? "" : $"[{unit.Title}]";
        return $"<color=#{colorHex}>{titleStr}{unit.EntityName} - {tierName}</color>";
    }

    public void HideInfo()
    {
        gameObject.SetActive(false);
    }
}
