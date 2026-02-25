using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TierUpgradeData
{
    public string name; //에디터 표시용 이름(예: 흔함 업그레이드)
    public UnitTier targetTier; //적용될 등급
    public int currentLevel = 0; //현재 레벨
    
    [Header("Cost Settings")]
    public int baseCost = 100;
    public int costPerLevel = 10; //레벨업 당 비용 증가분
    
    [Header("Stat Settings")]
    public float damageBonusPerLevel = 0.1f; // 1업당 공격력 10 % 추가
    // 공속도 같이 올리고 싶으면 여기에 추가
    //public float speedBonusPerLevel = 0.0f;
}
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("Tier Upgrades Setup")]
    //인스펙터에서 이 리스트에 흔함, 안흔함, 특별함 등을 추가하고 설정하세요
    [SerializeField] private List<TierUpgradeData> tierUpgrades;
    
    //빠른 검색을 위한 딕셔너리(게임 시작 시 생성)
    private Dictionary<UnitTier, TierUpgradeData> upgradeMap = new Dictionary<UnitTier, TierUpgradeData>();

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this.gameObject);
        
        // 리스트를 딕셔너리로 변환 (성능 최적화)
        foreach (var data in tierUpgrades)
        {
            if (!upgradeMap.ContainsKey(data.targetTier))
            {
                upgradeMap.Add(data.targetTier,data);
            }
        }
    }
    
    // --- 외부 호출용 함수 ---
    
    // 업그레이드 시도
    public void TryUpgradeTier(UnitTier tier)
    {
        if (!upgradeMap.ContainsKey(tier))
        {
            Debug.LogWarning($"[{tier}]등급에 대한 업그레이드 데이터가 없습니다.");
            return;
        }

        TierUpgradeData upgradeData = upgradeMap[tier];
        int cost = upgradeData.baseCost + (upgradeData.currentLevel * upgradeData.costPerLevel);

        if (GameManager.Instance.SpendCurrency(CurrencyType.Gold, cost))
        {
            upgradeData.currentLevel++;
            Debug.Log($"{tier}업그레이드 성공! Lv.{upgradeData.currentLevel}");
            
            //UI갱신 이벤트 호출 권장
        }
    }
    
    // --- 유닛 스탯 계산 함수 ---
    
    //유닛이 공격할  때 이 함수를 호출해서 최종 데미지를 받아감
    public float GetFinalDamage(float baseDamage, UnitTier tier)
    {
        // 1. 해당 티어의 업그레이드 보너스 적용
        float multiplier = 1.0f;

        if (upgradeMap.ContainsKey(tier))
        {
            TierUpgradeData upgradeData = upgradeMap[tier];
            multiplier += (upgradeData.currentLevel * upgradeData.damageBonusPerLevel);
        }
        //2. 확장성  나중에 '전체 유닛 공격력 증가' 같은 특수 업그레이드가 있다면 여기서 추가 연산
        // multiplier += globalDamageBonus;
        
        return baseDamage * multiplier;
    }
    
    //UI 표시용 정보 가져오기
    public TierUpgradeData GetUpgradeData(UnitTier tier)
    {
        if (upgradeMap.ContainsKey(tier)) return upgradeMap[tier];
        return null;
    }

    public List<TierUpgradeData> GetAllUpgradeData()
    {
        return tierUpgrades;
    }
}
