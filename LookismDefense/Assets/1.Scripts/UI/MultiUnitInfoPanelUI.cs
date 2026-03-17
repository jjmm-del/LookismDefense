using UnityEngine;
using System.Collections.Generic;
using System;

public class MultiUnitInfoPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform multiUnitContents;
    [SerializeField] private GameObject multiUnitPortraitPrefab;

    public void ShowInfo(List<UnitEntity> selectedUnits, Action<UnitEntity> onPortraitClickCallback)
    {
        gameObject.SetActive(true);
        
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
            TooltipTrigger tooltip = portraitObj.GetComponent<TooltipTrigger>();
            if(tooltip == null)  portraitObj.AddComponent<TooltipTrigger>();
            
            string title = string.IsNullOrEmpty(data.Title) ? "" : $"[{data.Title}] ";
            tooltip.content = $"<b>{title}{data.EntityName}</b>\n<size=80%>{data.Tier}</size>";
        }
    }

    public void HideInfo()
    {
        gameObject.SetActive(false);
    }
}
