using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;

public class MultiUnitInfoPanelUI : UIPanel
{
    [Header("UI References")]
    [SerializeField] private Transform multiUnitContents;
    [SerializeField] private GameObject multiUnitPortraitPrefab;

	private IObjectPool<GameObject> portraitPool;

    private List<GameObject> activePortraits = new List<GameObject>();
    
    private void Awake()
    {
        portraitPool = new ObjectPool<GameObject>(
            createFunc: CreatePortrait,     // 1.풀에 여분이 없을 때 새로 찍어내는 방법
            actionOnGet: OnGetPortrait,     // 2.풀에서 꺼내 쓸 때 할 행동(켜기
            actionOnRelease: OnReleasePortrait,      // 3.풀에 다 쓰고 반납할 때 할 행동(끄기)
            actionOnDestroy: OnDestroyPortrait,     // 4.풀이 꽉찼는데 반납할 때(삭제)
            collectionCheck: false, 
            defaultCapacity: 20, //시작하자마자 20개를 미리 만들어 둠
            maxSize: 100 //최대 100개까지만 보관
        );
    }
    // -- 풀링 시스템 핵심 함수 4가지 --
    private GameObject CreatePortrait()
    {
        //풀에 여분이 없을 때만 Instantiate를 합니다.
        GameObject portraitObj = Instantiate(multiUnitPortraitPrefab, multiUnitContents);
        
        if(portraitObj.GetComponent<TooltipTrigger>() == null)
            portraitObj.AddComponent<TooltipTrigger>();

        return portraitObj;
    }

    private void OnGetPortrait(GameObject portrait)
    {
        portrait.SetActive(true); //꺼낼 때 켭니다.
    }

    private void OnReleasePortrait(GameObject portrait)
    {
        portrait.SetActive(false); //반납할 때 끕니다.
    }

    private void OnDestroyPortrait(GameObject portrait)
    {
        Destroy(portrait); //100개가 넘어가면 그땐 진짜 파괴
    }

    // --- 실제 사용 부분 ---
    public void SetData(List<UnitEntity> selectedUnits, Action<UnitEntity> onPortraitClickCallback)
    {
        gameObject.SetActive(true);
        
        // 기존 초상화 싹 파괴하지 않고, 풀에 반납합니다.
        foreach (GameObject portrait in activePortraits)
        {
            portraitPool.Release(portrait);
        }
        activePortraits.Clear();
        
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

            GameObject portraitObj = portraitPool.Get();
            activePortraits.Add(portraitObj);
            
            MultiUnitPortraitUI portraitUI = portraitObj.GetComponent<MultiUnitPortraitUI>();
            if (portraitUI != null)
            {
                // 생성할 때 넘겨받은 콜백을 함께 건내줌
                portraitUI.Setup(data, unitList.Count, unitList[0], onPortraitClickCallback);
            }
            
            TooltipTrigger tooltip = portraitObj.GetComponent<TooltipTrigger>();
            if (tooltip != null)
            {
                string title = string.IsNullOrEmpty(data.Title) ? "" : $"[{data.Title}] ";
                tooltip.content = $"<b>{title}{data.EntityName}</b>\n<size=80%>{data.Tier}</size>";
            }
        }
    }

    public void HideInfo()
    {
        Close();
    }
    
}
