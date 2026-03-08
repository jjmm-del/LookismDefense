using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;



public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public Action OnResourceChanged;
    public Action OnTeleportRequested; // 텔레포트 버튼 클릭 시 발생할 이벤트
    
    //[Header("GameStart")]
    //[SerializeField] private GameObject difficultyPanel;
    
    [Header("Top Info Panel")]
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    
    [Header("Bottom Unit Info Panel(단일)")]
    [SerializeField] private GameObject unitInfoPanel; // 패널 전체 (켜고 끄기용)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Image portraitImage; // 유닛 초상화 (나중에 추가)
    [SerializeField] private Transform abilityIconContainer;    //아이콘 콘테이너
    [SerializeField] private GameObject abilityIconPrefab;      //아이콘 프리펩
    

    [Header("Bottom Multi Unit Info Panel(다중)")]
    [SerializeField] private GameObject multiUnitInfoPanel; //다중 선택 패널 전체
    [SerializeField] private Transform multiUnitContents; //초상화들이 나열될 부모
    [SerializeField] private GameObject multiUnitPortraitPrefab; //초상화 프리팹
    
    [Header("MainPanel")]
    [SerializeField] private GameObject summonPanel;
    [SerializeField] private GameObject upgradePanel;
    
    [Header("GameOver")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Story")]
    [SerializeField] private Button singleTeleportButton;
    [SerializeField] private Button multiTeleportButton;
    
    [SerializeField] private Transform recipeContents; // ScrollView의 Content
    [SerializeField] private GameObject recipeButtonPrefab; //위에서 만든 버튼 프리팹
    [SerializeField] private List<TierDisplayInfo> tierSettings;
    private Dictionary<UnitTier, TierDisplayInfo> tierMap = new Dictionary<UnitTier, TierDisplayInfo>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        foreach (var setting in tierSettings)
        {
            if (!tierMap.ContainsKey(setting.tier))
            {
                tierMap.Add(setting.tier, setting);
            }
        }
    }

    private void Start()
    {
        //시작할 때 꺼두기
        HideInfoPanel();
        if (summonPanel != null)
        {
            summonPanel.SetActive(false);
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        if (singleTeleportButton != null)
        {
            singleTeleportButton.onClick.AddListener(OnTeleportButtonClicked);
        }

        if (multiTeleportButton != null)
        {
            multiTeleportButton.onClick.AddListener(OnTeleportButtonClicked);
        }
        //이벤트 구독
        OnResourceChanged += RefreshGoldUI;
        //GoldUI 업데이트
        RefreshGoldUI();
    }
    //--- 상단 정보 갱신 ---
    public void UpdateRoundTime(float time)
    {
        // 시간을 00:00 형식으로 표시
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        roundTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateGold(int gold)
    {
        goldText.text = $"Gold: {gold}";
    }

    private void RefreshGoldUI()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        int currentGold = GameManager.Instance.GetCurrency(CurrencyType.Gold);
        UpdateGold(currentGold);
    }
    public void UpdateUnitCount(int current, int max)
    {
        unitCountText.text =$"{current}/{max}";
        
        //위험 수치면 빨간색으로 변경하는 연출 가능
        if (current >= max - 10)
        {
            unitCountText.color = Color.red;
        }
        else
        {
            unitCountText.color = Color.white;
        }
    }

    public void UpdateWaveName(string name)
    {
        waveNameText.text = name;
    }
    
    // --- 하단 유닛 정보 갱신 ---
    public void ShowUnitInfo(UnitData data)
    {
        UpdateRecipeList(data);
        multiUnitInfoPanel.SetActive(false);
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = $"DMG:{data.AttackDamage}";
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
                }
            }
        }
    }

    public void ShowMultiUnitInfo(List<UnitEntity> selectedUnits, Action<UnitEntity> onPortraitClickCallback)
    {
        unitInfoPanel.SetActive(false);
        multiUnitInfoPanel.SetActive(true);
        
        // 기존 초상화 싹 지우기
        foreach (Transform child in multiUnitContents)
        {
            Destroy(child.gameObject);
        }
        
        Dictionary<UnitData, List<UnitEntity>> groupedUnits = new Dictionary<UnitData, List<UnitEntity>>();
        foreach (UnitEntity unit in selectedUnits)
        {
            if (!groupedUnits.ContainsKey(unit.Data))
            {
                groupedUnits[unit.Data] = new List<UnitEntity>();
            }
            groupedUnits[unit.Data].Add(unit);
        }
        // 3. 종류별로 프리팹 찍어내기
        foreach (var kvp in groupedUnits)
        {
            UnitData data = kvp.Key;
            List<UnitEntity> unitList = kvp.Value;
            
            GameObject portraitObj = Instantiate(multiUnitPortraitPrefab, multiUnitContents);
            MultiUnitPortraitUI portraitUI = portraitObj.GetComponent<MultiUnitPortraitUI>();
            if (portraitUI != null)
            {
                // 생성할 때 넘겨받은 콜백을 함께 건내줌
                portraitUI.Setup(data, unitList.Count, unitList[0], onPortraitClickCallback);
            }
            
        }
        
    }

    public void ShowEnemyInfo(EnemyData data, float currentHp)
    {
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = "Enemy";
        attackSpeedText.text = $"HP:{currentHp}/{data.MaxHealth}";
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    
    public void HideInfoPanel()
    {
        unitInfoPanel.SetActive(false);
        multiUnitInfoPanel.SetActive(false);
    }

    public void UpdateRecipeList(UnitData unit)
    {
        // 1. 기존 버튼 싹 지우기
        foreach (Transform child in recipeContents)
        {
            Destroy(child.gameObject);
        }
        // 2. 이 유닛과 관련된 레시피 가져오기
        List<CombinationRecipe> recipes = CombinationManager.Instance.GetRecipesForUnit(unit);
        
        // 3. 버튼 생성하기
        foreach (CombinationRecipe recipe in recipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeContents);
            buttonObj.GetComponent<RecipeButtonUI>().Setup(recipe);

        }
    }

    public void ToggleSummonPanel()
    {
        bool isActive= summonPanel.activeSelf;
        CloseAllPanels(); //다른 패널이 열려있다면 닫고 내 것을 연다
        summonPanel.SetActive(!isActive);
    }

    public void ToggleUpgradePanel()
    {
        bool isActive = upgradePanel.activeSelf;
        CloseAllPanels();
        upgradePanel.SetActive(!isActive);
    }

    public void CloseAllPanels()
    {
        if (summonPanel != null)
        {
            summonPanel.SetActive(false);
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        //unitInfoPanel.SetActive(false);
    }

    
    private void OnDestroy()
    {
        OnResourceChanged -= RefreshGoldUI;
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
}
