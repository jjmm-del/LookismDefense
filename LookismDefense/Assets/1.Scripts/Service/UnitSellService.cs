using UnityEngine;
using System.Collections.Generic;
public class UnitSellService : Singleton<UnitSellService>
{
    [SerializeField]
    private SellRewardSettings sellRewardSettings;

    public List<SellRewardSettings.RewardItem> GetSellRewardInfo(UnitTier tier)
    {
        if (sellRewardSettings == null)
            return null;

        return sellRewardSettings.GetRewards(tier);
    }

    public bool SellUnit(UnitEntity unit)
    {
        if (unit == null)
            return false;

        if (EntityRegistry.Instance == null || !EntityRegistry.Instance.ContainsUnit(unit))
        {
            return false;
        }

        List<SellRewardSettings.RewardItem> rewards = GetSellRewardInfo(unit.Tier);

        if (rewards != null)
        {
            GiveRewards(rewards);
        }
        EntityRegistry.Instance.UnregisterUnit(unit);
        Destroy(unit.gameObject);
        return true;
    }

    private void GiveRewards(List<SellRewardSettings.RewardItem> rewards)
    {
        if (CurrencyManager.Instance == null)
            return;

        foreach (var reward in rewards)
        {
            float roll = Random.Range(0f, 100f);
            if (roll >= reward.chance)
                continue;
            
            CurrencyManager.Instance.AddCurrency(reward.rewardType, reward.amount);
            
            Debug.Log($"판매 보상 획득 : {reward.rewardType} + {reward.amount}");
        }
    }
}
