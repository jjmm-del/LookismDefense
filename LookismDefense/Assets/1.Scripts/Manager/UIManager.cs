using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;



public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public Action OnResourceChanged;
    public Action OnTeleportRequested; 
    
    [Header("Top Info Panel")]
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    
    [Header("Bottom Unit Info Panel(단일)")]
    [SerializeField] private UnitInfoPanelUI singleUnitInfoPanel; // 패널 전체 (켜고 끄기용)
    
    [Header("Bottom Multi Unit Info Panel(다중)")]
    [SerializeField] private MultiUnitInfoPanelUI multiUnitInfoPanel; //다중 선택 패널 전체
    
    [Header("MainPanel")]
    [SerializeField] private GameObject summonPanel;
    [SerializeField] private GameObject upgradePanel;
    
    [Header("GameOver")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Story")]
    [SerializeField] private Button singleTeleportButton;
    [SerializeField] private Button multiTeleportButton;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        
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
    public void ShowUnitInfo(UnitEntity unit)
    {
        multiUnitInfoPanel.HideInfo();
        singleUnitInfoPanel.ShowInfo(unit);
    }

    public void ShowEnemyInfo(EnemyData data, float currentHp)
    {
        multiUnitInfoPanel.HideInfo();
        singleUnitInfoPanel.ShowEnemyInfo(data, currentHp);
    }

    public void ShowMultiUnitInfo(List<UnitEntity> selectedUnits, Action<UnitEntity> onPortraitClickCallback)
    {
        singleUnitInfoPanel.HideInfo();
        multiUnitInfoPanel.ShowInfo(selectedUnits, onPortraitClickCallback);
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
        singleUnitInfoPanel.HideInfo();
        multiUnitInfoPanel.HideInfo();
    }
    
    public void OnTeleportButtonClicked()
    {
        OnTeleportRequested?.Invoke();
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
