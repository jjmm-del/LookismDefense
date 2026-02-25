using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public Action OnResourceChanged;
    
    [Header("Top Info Panel")]
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    
    [Header("Bottom Unit Info Panel")]
    [SerializeField] private GameObject unitInfoPanel; // 패널 전체 (켜고 끄기용)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Image portraitImage; // 유닛 초상화 (나중에 추가)

    [Header("MainPanel")]
    [SerializeField] private GameObject summonPanel;
    [SerializeField] private GameObject upgradePanel;
    
    [SerializeField] private Transform recipeContents; // ScrollView의 Content
    [SerializeField] private GameObject recipeButtonPrefab; //위에서 만든 버튼 프리팹
    
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        HideInfoPanel();
        //시작할 때 꺼두기
        summonPanel.SetActive(false);
        upgradePanel.SetActive(false);
        OnResourceChanged += RefreshGoldUI;
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
        unitCountText.text = $"Enemy: {current}/{max}";
        
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
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = $"DMG:{data.AttackDamage}";
        attackSpeedText.text = $"ASP:{data.AttackSpeed}";
        //portraitImage.sprite = data.Icon; //아이콘이 있다면
        
    }

    public void ShowEnemyInfo(EnemyData data, float currentHp)
    {
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = "Enemy";
        attackSpeedText.text = $"HP:{currentHp}/{data.MaxHealth}";
    }

    public void HideInfoPanel()
    {
        unitInfoPanel.SetActive(false);
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
}
