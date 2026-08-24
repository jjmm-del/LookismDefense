using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;



public class UIManager : Singleton<UIManager>
{
    public Action OnTeleportRequested; 
    
    [Header("Panel Root")]
    [SerializeField] private Transform panelRoot;
    
    // [Header("Bottom Unit Info Panel(단일)")]
    // [SerializeField] private UnitInfoPanelUI singleUnitInfoPanel; // 패널 전체 (켜고 끄기용)
    //
    // [Header("Bottom Multi Unit Info Panel(다중)")]
    // [SerializeField] private MultiUnitInfoPanelUI multiUnitInfoPanel; //다중 선택 패널 전체
    //
    // [Header("Bottom Enemy Info Panel")]
    // [SerializeField] private EnemyInfoPanelUI enemyInfoPanel;
    // [Header("MainPanel")]
    // [SerializeField] private UISummonPanel summonPanel;
    [SerializeField] private GameObject upgradePanel;
    
    [Header("Story")]
    [SerializeField] private Button singleTeleportButton;
    [SerializeField] private Button multiTeleportButton;
    
    private readonly Dictionary<Type, UIPanel> panels = new();
    private readonly Stack<UIPopup> popupStack = new();

    private UIPanel currentPanel;
    private UIPopup currentPopup;
    protected override void Awake()
    {
        base.Awake();
        RegisterPanels();
    }

    private void RegisterPanels()
    {
        Transform root = panelRoot != null ? panelRoot : transform;
        
        UIPanel[] foundPanels = root.GetComponentsInChildren<UIPanel>(true);

        foreach (UIPanel panel in foundPanels)
        {
            Type panelType = panel.GetType();

            if (panels.ContainsKey(panelType))
            {
                Debug.LogError($"[UIManager]중복된 패널 타입: {panelType.Name}", panel);
                continue;
            }

            panels.Add(panelType, panel);
            panel.Hide();
        }
        
    }

    public T ShowPanel<T>(Action<T> setup = null) where T : UIPanel
    {
        if (!panels.TryGetValue(typeof(T), out UIPanel panel))
        {
            Debug.LogError($"[UIManager] 등록되지 않은 패널: {typeof(T).Name}");
            return null;
        }

        if (currentPanel != null && currentPanel != panel)
        {
            currentPanel.Hide();
        }

        T typedPanel = panel as T;
        
        setup?.Invoke(typedPanel);

        typedPanel.Show();
        currentPanel = typedPanel;

        return typedPanel;
    }

    public void ClosePanel(UIPanel panel)
    {
        if (panel == null)
            return;

        panel.Hide();

        if (currentPanel == panel)
        {
            currentPanel = null;
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
            return;

        currentPanel.Hide();
        currentPanel = null;
    }

    public bool IsCurrentPanel<T>() where T : UIPanel
    {
        return currentPanel is T;
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
        if (unit == null)
        {
            CloseCurrentPanel();
            return;
        }
        ShowPanel<UnitInfoPanelUI>(panel => panel.SetData(unit));
    }
    
    public void ShowEnemyInfo(EnemyEntity enemy)
    {
        if (enemy == null)
        {
            CloseCurrentPanel();
            return;
        }
        ShowPanel<EnemyInfoPanelUI>(panel => panel.SetData(enemy));
    }
    
    public void ShowMultiUnitInfo(List<UnitEntity> units, Action<UnitEntity> onPortraitClicked)
    {
        if (units == null || units.Count == 0)
        {
            CloseCurrentPanel();
            return;
        }
        ShowPanel<MultiUnitInfoPanelUI>(panel => panel.SetData(units, onPortraitClicked));
    }
    
    public void ShowSummonPanel()
    {
        ShowPanel<UISummonPanel>();
    }
    public void OnTeleportButtonClicked()
    {
        OnTeleportRequested?.Invoke();
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
