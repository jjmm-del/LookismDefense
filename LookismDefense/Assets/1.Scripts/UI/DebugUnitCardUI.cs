using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugUnitCardUI : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image tierFrame;

    [SerializeField] private TMP_Text unitIdText;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private Button summonButton;

    private UnitRecord unit;
    private Action<UnitRecord> onSummonRequested;

    private void Awake()
    {
        if (summonButton != null)
        {
            summonButton.onClick.AddListener(HandleSummonClicked);
        }
    }

    public void Setup(UnitRecord record, UnitTierDisplaySettings tierDisplaySettings, Action<UnitRecord> summonCallback)
    {
        unit = record;
        onSummonRequested = summonCallback;

        if (record == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (unitIdText != null)
        {
            unitIdText.text = unit.id;
        }

        if (unitNameText != null)
        {
            unitNameText.text = record.DisplayName;
        }

        if (portraitImage != null)
        {
            Sprite portrait = UnitAssetProvider.LoadPortrait(record);

            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        if (tierFrame != null && tierDisplaySettings != null)
        {
            tierFrame.color = tierDisplaySettings.GetColor(record.tier);
        }

        bool canSummon = record.enabled && !string.IsNullOrWhiteSpace(record.prefabKey);

        if (summonButton != null)
        {
            summonButton.interactable = canSummon;
        }

        if (statusText != null)
        {
            statusText.text = GetStatusText(record);
        }
    }

    private static string GetStatusText(UnitRecord record)
    {
        if (!record.enabled)
        {
            return "비활성";
        }

        if (string.IsNullOrWhiteSpace(record.prefabKey))
        {
            return "PrefabKey 없음";
        }

        return "소환 가능";
    }

    private void HandleSummonClicked()
    {
        if (unit == null)
            return;

        onSummonRequested?.Invoke(unit);
    }

    private void OnDestroy()
    {
        if (summonButton != null)
        {
            summonButton.onClick.RemoveListener(HandleSummonClicked);
        }
    }
}
