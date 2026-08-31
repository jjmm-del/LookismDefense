using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUnitCatalogPopup : UIPopup
{
    [Header("Filter")]
    [SerializeField] private TMP_Dropdown tierDropdown;
    [SerializeField] private UnitTierDisplaySettings tierDisplaySettings;

    [Header("Unit List")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private DebugUnitCardUI unitCardPrefab;
    [SerializeField] private TMP_Text unitCountText;

    [Header("Result")]
    [SerializeField] private TMP_Text resultText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private readonly List<UnitTier> tierOptions = new();

    private bool optionsBuilt;
    private bool waitingForDatabase;

    private void Awake()
    {
        if (tierDropdown != null)
        {
            tierDropdown.onValueChanged.AddListener(HandleTierChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        BuildTierOptions();
    }

    public override void Show()
    {
        base.Show();

        BuildTierOptions();
        RefreshList();
    }

    public override void Hide()
    {
        UnsubscribeDatabaseReady();
        ClearCards();

        base.Hide();
    }

    private void BuildTierOptions()
    {
        if (optionsBuilt || tierDropdown == null)
            return;

        optionsBuilt = true;

        tierOptions.Clear();
        tierDropdown.ClearOptions();

        List<string> displayOptions = new()
        {
            "전체"
        };

        foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
        {
            tierOptions.Add(tier);

            string displayName = tierDisplaySettings != null
                ? tierDisplaySettings.GetDisplayName(tier)
                : tier.ToString();

            displayOptions.Add(displayName);
        }

        tierDropdown.AddOptions(displayOptions);
        tierDropdown.SetValueWithoutNotify(0);
    }

    private void HandleTierChanged(int _)
    {
        RefreshList();
    }

    private void RefreshList()
    {
        ClearCards();

        GameDatabase database = GameDatabase.Instance;

        if (database == null || !database.IsReady)
        {
            if (unitCountText != null)
            {
                unitCountText.text = "DB 로딩 중...";
            }

            SubscribeDatabaseReady(database);
            return;
        }

        UnsubscribeDatabaseReady();

        List<UnitRecord> units = new();

        // 0번은 전체
        if (tierDropdown == null || tierDropdown.value == 0)
        {
            foreach (UnitTier tier in tierOptions)
            {
                AddUnitsForTier(database, tier, units);
            }
        }
        else
        {
            int tierIndex = tierDropdown.value - 1;

            if (tierIndex >= 0 && tierIndex < tierOptions.Count)
            {
                AddUnitsForTier(
                    database,
                    tierOptions[tierIndex],
                    units);
            }
        }

        units.Sort(CompareUnitId);

        foreach (UnitRecord unit in units)
        {
            CreateUnitCard(unit);
        }

        if (unitCountText != null)
        {
            unitCountText.text = $"등록 유닛: {units.Count}";
        }
    }

    private static void AddUnitsForTier(
        GameDatabase database,
        UnitTier tier,
        List<UnitRecord> destination)
    {
        IReadOnlyList<UnitRecord> tierUnits =
            database.GetUnitsForTier(tier);

        foreach (UnitRecord unit in tierUnits)
        {
            if (unit != null)
            {
                destination.Add(unit);
            }
        }
    }

    private void CreateUnitCard(UnitRecord unit)
    {
        if (unitCardPrefab == null || contentArea == null)
            return;

        DebugUnitCardUI card =
            Instantiate(unitCardPrefab, contentArea);

        card.Setup(
            unit,
            tierDisplaySettings,
            HandleSummonRequested);
    }

    private void HandleSummonRequested(UnitRecord unit)
    {
        if (SummonService.Instance == null)
        {
            SetResult("SummonService가 없습니다.");
            return;
        }

        bool success =
            SummonService.Instance.TryDebugSummon(unit);

        SetResult(
            success
                ? $"{unit.id} 소환 성공"
                : $"{unit.id} 소환 실패");
    }

    private void SetResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    private static int CompareUnitId(
        UnitRecord left,
        UnitRecord right)
    {
        return string.Compare(
            left?.id,
            right?.id,
            StringComparison.OrdinalIgnoreCase);
    }

    private void SubscribeDatabaseReady(GameDatabase database)
    {
        if (database == null || waitingForDatabase)
            return;

        waitingForDatabase = true;
        database.Ready += HandleDatabaseReady;
    }

    private void UnsubscribeDatabaseReady()
    {
        if (!waitingForDatabase)
            return;

        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.Ready -= HandleDatabaseReady;
        }

        waitingForDatabase = false;
    }

    private void HandleDatabaseReady()
    {
        UnsubscribeDatabaseReady();
        RefreshList();
    }

    private void ClearCards()
    {
        if (contentArea == null)
            return;

        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeDatabaseReady();

        if (tierDropdown != null)
        {
            tierDropdown.onValueChanged.RemoveListener(
                HandleTierChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }
}