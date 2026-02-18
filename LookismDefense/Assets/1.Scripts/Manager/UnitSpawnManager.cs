using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class UnitSpawnManager : MonoBehaviour
{
    public static UnitSpawnManager Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private Transform spawnAreaCenter; // 유닛이 생성될 구역 중심
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(5, 5); //생성 구역
    
    [Header("Gacha Data")]
    public List<UnitData> CommonUnits; //
    [SerializeField] private List<UnitData> SpecialUnits; //
    [SerializeField] private List<UnitData> RareUnits; //
    [SerializeField] private List<UnitData> LegendaryUnits; //

    [Header("UIReferences")]
    [SerializeField] private UnitSelectorUI selectorUI;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    public void SpawnRandomUnit(UnitTier tier)
    {
        // -- 순수 생성 로직 --
        List<UnitData> targetList = null;
        switch (tier)
        {
            case UnitTier.Common: targetList = CommonUnits; break;
            case UnitTier.Special: targetList = SpecialUnits; break;
            case UnitTier.Rare: targetList = RareUnits; break;
            case UnitTier.Legendary: targetList = LegendaryUnits; break;
        }

        if (targetList != null && targetList.Count > 0)
        {
            // 1. 랜덤 유닛 선택
            int randomIndex = Random.Range(0, targetList.Count);
            UnitData selectedUnit = CommonUnits[randomIndex];
        
            // 2. 랜덤 위치 계산 (겹치지 않게 하려면 나중에 그리드 시스템 적용 필요)
            Vector3 randomPos = GetRandomPosition();
        
            // 3. 생성
            GameObject unitObj = Instantiate(selectedUnit.Prefab, randomPos, Quaternion.identity);
        
            // 4. 데이터 주입
            UnitEntity unitEntity = unitObj.GetComponent<UnitEntity>();
            if(unitEntity != null) unitEntity.Initialize(selectedUnit);
            
            Debug.Log($"{tier}유닛 소환 완료");
        }
        else
        {
            Debug.LogError($"티어 {tier}의 유닛 데이터가 없습니다.");
        }
    }

    //선택권 소환
    public void SpawnSelectedUnit(UnitData unit)
    {
        //유닛 티어에 따라 필요한 재화가 다름
        CurrencyType costType = CurrencyType.RandomCommon;
        int costAmount = 1;
        
        //유닛 티어별 필요 재화 설정
        switch (unit.Tier) //Enum 값에 따라 설정
        {
            case UnitTier.Common: costType = CurrencyType.SelectCommon; break; //
        }
        
        //재화 차감 시도
        if (GameManager.Instance.SpendCurrency(costType, costAmount))
        {
            //SpawnUnitActual(unit);
            //팝업 닫기 등
        }
        else
        {
            Debug.Log($"{costType}이 부족하여 소환할 수 없습니다.");
        }
    }

    //버튼에서 호출하는 통합 소환 함수
    public void TrySummon(CurrencyType type)
    {
        // 1. 재화가 있는지 먼저 확인(GameManager)
        if (GameManager.Instance.GetCurrency(type)<=0)
        {
            Debug.Log("소환권이 부족합니다.");
            return;
        }
        
        // 2. 타입별 행동 분기
        switch (type)
        {
            case CurrencyType.RandomCommon:
                if(GameManager.Instance.SpendCurrency(type,1))
                    SpawnRandomUnit(UnitTier.Common);
                break;
            case CurrencyType.RandomSpecial:
                if(GameManager.Instance.SpendCurrency(type,1))
                    SpawnRandomUnit(UnitTier.Special);
                break;
            case CurrencyType.RandomRare:
                if(GameManager.Instance.SpendCurrency(type,1))
                    SpawnRandomUnit(UnitTier.Rare);
                break;
            case CurrencyType.RandomLegendary:
                if(GameManager.Instance.SpendCurrency(type,1))
                    SpawnRandomUnit(UnitTier.Legendary);
                break;
            case CurrencyType.SelectCommon:
                //선택권은 여기서 재화를 깎지 않고, 선택창을 띄운 뒤 유닛을 고르면 깎음
                OpenCommonSelector();
                break;
            
            //다른 케이스 추가
                
        }
        
    }


    public void OpenCommonSelector()
    {
        //재화 체크를 여기서 먼저 할 수도 있음
        selectorUI.OpenSelector("흔함 선택", CommonUnits);
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float z = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
        return spawnAreaCenter.position + new Vector3(x, 0, z);
    }
    
    //에디터에서 생성 범위 확인용
    private void OnDrawGizmos()
    {
        if (spawnAreaCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnAreaCenter.position, new Vector3(spawnAreaSize.x,1,spawnAreaSize.y));
        }
    }
}
