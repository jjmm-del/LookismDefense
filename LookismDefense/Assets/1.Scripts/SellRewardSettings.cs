using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[CreateAssetMenu(menuName="LookismDefense/SellRewardSettings")]
public class SellRewardSettings : ScriptableObject
{
    [System.Serializable]
    public struct RewardItem
    {
        public CurrencyType rewardType;
        public int amount;
        [Range(0, 100)] public float chance;
    }
    [System.Serializable]
    public struct TierRewardInfo
    {
        public UnitTier tier;                //판매할 유닛의 등급
        public List<RewardItem> rewards;     //판매 보상 목록
    }
    [Header("등급별 판매 보상 설정")]
    public List<TierRewardInfo> tierRewards = new List<TierRewardInfo>();
    
    //외부에서 등급을 넣으면 그 보상의 '목록(List)' 전체를 던져주는 함수
    public List<RewardItem> GetRewards(UnitTier tier)
    {
        foreach (var info in tierRewards)
        {
            if (info.tier == tier)
            {
                return info.rewards;
            }
        }
        return null;
    }
}
