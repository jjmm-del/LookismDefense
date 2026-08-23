using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;



public class UIManager : Singleton<UIManager>
{
    private readonly Stack<UIPopup> popupStack = new();
    public Action OnTeleportRequested; 
    
    [Header("Bottom Unit Info Panel(단일)")]
    [SerializeField] private UnitInfoPanelUI singleUnitInfoPanel; // 패널 전체 (켜고 끄기용)
    
    [Header("Bottom Multi Unit Info Panel(다중)")]
    [SerializeField] private MultiUnitInfoPanelUI multiUnitInfoPanel; //다중 선택 패널 전체

    [Header("Bottom Enemy Info Panel")]
    [SerializeField] private EnemyInfoPanelUI enemyInfoPanel;
    [Header("MainPanel")]
    [SerializeField] private UISummonPopup summonPopup;
    [SerializeField] private GameObject upgradePanel;
    
    [Header("Story")]
    [SerializeField] private Button singleTeleportButton;
    [SerializeField] private Button multiTeleportButton;

    private UIPopup _currentPopup;
    protected override void Awake()
    {
        base.Awake();
    }

    public T OpenPopup<T>(T popup) where T : UIPopup
    {
        popup.Show();
        popupStack.Push(popup);

        return popup;
    }

    public void ClosePopup(UIPopup popup)
    {
        if (popup == null)
            return;
        popup.Hide();

        if (popupStack.Count > 0 &&
            popupStack.Peek() == popup)
        {
            popupStack.Pop();
        }
    }

    public void CloseTopPopup()
    {
        if (popupStack.Count == 0)
            return;
        UIPopup popup = popupStack.Pop();
        popup.Hide();
    }

    private void Start()
    {
        //시작할 때 꺼두기
        HideInfoPanel();

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
    }
  
    // --- 하단 유닛 정보 갱신 ---
    public void ShowUnitInfo(UnitEntity unit)
    {
        multiUnitInfoPanel?.HideInfo();
        enemyInfoPanel?.HideInfo();
        singleUnitInfoPanel?.ShowInfo(unit);
    }

    public void ShowEnemyInfo(EnemyEntity enemy)
    {
        if (enemy == null)
        {
            HideInfoPanel();
            return;
        }
        singleUnitInfoPanel?.HideInfo();
        multiUnitInfoPanel?.HideInfo();
        enemyInfoPanel?.ShowInfo(enemy.Data, enemy.CurrentHealth);
    }
    
    public void ShowMultiUnitInfo(List<UnitEntity> selectedUnits, Action<UnitEntity> onPortraitClickCallback)
    {
        singleUnitInfoPanel?.HideInfo();
        enemyInfoPanel?.HideInfo();
        multiUnitInfoPanel?.ShowInfo(selectedUnits, onPortraitClickCallback);
    }
    
    public void HideInfoPanel()
    {
        singleUnitInfoPanel?.HideInfo();
        multiUnitInfoPanel?.HideInfo();
        enemyInfoPanel?.HideInfo();
    }
    
    public void OnTeleportButtonClicked()
    {
        OnTeleportRequested?.Invoke();
    }

    public void ToggleSummonPanel()
    {
        if (summonPopup == null)
            return;

        if (summonPopup.gameObject.activeSelf)
        {
            ClosePopup(summonPopup);
        }
        else
        {
            OpenPopup(summonPopup);
        }
    }

    public void ToggleUpgradePanel()
    {
        bool isActive = upgradePanel.activeSelf;
        CloseAllPanels();
        upgradePanel.SetActive(!isActive);
    }

    public void CloseAllPanels()
    {
        

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        //unitInfoPanel.SetActive(false);
    }
}
